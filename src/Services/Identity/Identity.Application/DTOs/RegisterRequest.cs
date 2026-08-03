namespace Identity.Application.DTOs;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string Email,
    string UserName,
    string Password);
