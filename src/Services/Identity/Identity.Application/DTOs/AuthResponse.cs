namespace Identity.Application.DTOs;

public record AuthResponse(
    string UserId,
    string UserName,
    string Email,
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);
