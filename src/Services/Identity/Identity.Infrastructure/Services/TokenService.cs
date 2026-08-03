using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Identity.Application.Common.Interfaces;
using Identity.Application.Common.Models;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Identity.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly UserManager<User> _userManager;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public TokenService(
        IOptions<JwtSettings> jwtSettings,
        UserManager<User> userManager,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _jwtSettings = jwtSettings.Value;
        _userManager = userManager;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<(string AccessToken, DateTime ExpiresAt, string RefreshToken, DateTime RefreshExpiresAt)>
        GenerateTokensAsync(User user, string ipAddress, CancellationToken cancellationToken = default)
    {
        var accessToken = await GenerateAccessTokenAsync(user);
        var refreshToken = await GenerateRefreshTokenAsync(user, ipAddress, accessToken.Jti, cancellationToken);

        return (accessToken.Token, accessToken.ExpiresAt, refreshToken.Token, refreshToken.ExpiresAt);
    }

    public async Task<AuthResponse?> RefreshAccessTokenAsync(
        string refreshToken, string ipAddress, CancellationToken cancellationToken = default)
    {
        var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
        if (storedToken is null || !storedToken.IsActive)
        {
            return null;
        }

        // چرخش توکن: توکن قبلی استفاده شده می‌شود
        storedToken.IsUsed = true;
        await _refreshTokenRepository.UpdateAsync(storedToken);

        var user = await _userManager.FindByIdAsync(storedToken.UserId);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        var tokens = await GenerateTokensAsync(user, ipAddress, cancellationToken);

        return new AuthResponse(
            user.Id,
            user.UserName!,
            user.Email!,
            tokens.AccessToken,
            tokens.ExpiresAt,
            tokens.RefreshToken,
            tokens.RefreshExpiresAt);
    }

    public async Task RevokeAllRefreshTokensAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _refreshTokenRepository.RevokeAllForUserAsync(userId, cancellationToken);
    }

    private async Task<(string Token, string Jti, DateTime ExpiresAt)> GenerateAccessTokenAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Name, user.FullName),
            new(ClaimTypes.NameIdentifier, user.Id)
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        var jti = token.Id;
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString, jti, expiresAt);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(
        User user, string ipAddress, string jwtId, CancellationToken cancellationToken)
    {
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = GenerateSecureToken(),
            JwtId = jwtId,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedByIp = ipAddress
        };

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        return refreshToken;
    }

    private static string GenerateSecureToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}
