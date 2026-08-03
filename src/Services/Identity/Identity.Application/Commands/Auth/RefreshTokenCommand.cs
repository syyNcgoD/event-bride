using Identity.Application.Common.Interfaces;
using Identity.Application.Common.Models;
using Identity.Application.DTOs;
using MediatR;

namespace Identity.Application.Commands.Auth;

public record RefreshTokenCommand(
    string RefreshToken,
    string IpAddress) : IRequest<ApiResponse<AuthResponse>>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthResponse>>
{
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await _tokenService.RefreshAccessTokenAsync(
            request.RefreshToken, request.IpAddress, cancellationToken);

        if (result is null)
        {
            return ApiResponse<AuthResponse>.Fail("Refresh Token نامعتبر یا منقضی شده است");
        }

        return ApiResponse<AuthResponse>.Ok(result, "توکن با موفقیت بازتولید شد");
    }
}
