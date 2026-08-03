namespace Identity.Application.DTOs;

public record LoginRequest(string UserNameOrEmail, string Password);
