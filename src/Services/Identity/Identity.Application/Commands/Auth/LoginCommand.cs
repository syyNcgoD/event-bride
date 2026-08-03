using Identity.Application.Common.Interfaces;
using Identity.Application.Common.Models;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.Commands.Auth;

public record LoginCommand(
    string UserNameOrEmail,
    string Password,
    string IpAddress) : IRequest<ApiResponse<AuthResponse>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<AuthResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.UserNameOrEmail)
                   ?? await _userManager.FindByNameAsync(request.UserNameOrEmail);

        if (user is null || !user.IsActive)
        {
            return ApiResponse<AuthResponse>.Fail("نام کاربری یا رمز عبور اشتباه است");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                return ApiResponse<AuthResponse>.Fail("حساب شما موقتاً قفل شده است");
            }
            return ApiResponse<AuthResponse>.Fail("نام کاربری یا رمز عبور اشتباه است");
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

        return ApiResponse<AuthResponse>.Ok(response, "ورود با موفقیت انجام شد");
    }
}
