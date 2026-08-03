namespace Identity.Application.DTOs;

public record UserResponse(
    string Id,
    string UserName,
    string Email,
    string FirstName,
    string LastName,
    bool IsActive,
    DateTime CreatedAt);
