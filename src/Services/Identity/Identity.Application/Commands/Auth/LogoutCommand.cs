using Identity.Application.Common.Interfaces;
using Identity.Application.Common.Models;
using MediatR;

namespace Identity.Application.Commands.Auth;

public record LogoutCommand(string UserId) : IRequest<ApiResponse<bool>>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApiResponse<bool>>
{
    private readonly ITokenService _tokenService;

    public LogoutCommandHandler(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public async Task<ApiResponse<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await _tokenService.RevokeAllRefreshTokensAsync(request.UserId, cancellationToken);
        return ApiResponse<bool>.Ok(true, "خروج با موفقیت انجام شد");
    }
}
