using Identity.Application.Common.Interfaces;
using Identity.Application.Common.Models;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.Commands.Auth;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string UserName,
    string Password,
    string IpAddress) : IRequest<ApiResponse<AuthResponse>>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<AuthResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;

    public RegisterCommandHandler(UserManager<User> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return ApiResponse<AuthResponse>.Fail("کاربر با این ایمیل از قبل وجود دارد");
        }

        existingUser = await _userManager.FindByNameAsync(request.UserName);
        if (existingUser is not null)
        {
            return ApiResponse<AuthResponse>.Fail("نام کاربری از قبل وجود دارد");
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.UserName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<AuthResponse>.Fail("ثبت‌نام ناموفق بود", errors);
        }

        var tokens = await _tokenService.GenerateTokensAsync(user, request.IpAddress, cancellationToken);

        var response = new AuthResponse(
            user.Id,
            user.UserName!,
            user.Email!,
            tokens.AccessToken,
            tokens.ExpiresAt,
            tokens.RefreshToken,
            tokens.RefreshExpiresAt);

        return ApiResponse<AuthResponse>.Ok(response, "ثبت‌نام با موفقیت انجام شد");
    }
}
