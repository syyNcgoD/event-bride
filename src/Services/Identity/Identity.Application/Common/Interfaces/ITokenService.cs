using Identity.Application.DTOs;
using Identity.Domain.Entities;

namespace Identity.Application.Common.Interfaces;

public interface ITokenService
{
    Task<(string AccessToken, DateTime ExpiresAt, string RefreshToken, DateTime RefreshExpiresAt)>
        GenerateTokensAsync(User user, string ipAddress, CancellationToken cancellationToken = default);

    Task<AuthResponse?> RefreshAccessTokenAsync(string refreshToken, string ipAddress, CancellationToken cancellationToken = default);

    Task RevokeAllRefreshTokensAsync(string userId, CancellationToken cancellationToken = default);
}
