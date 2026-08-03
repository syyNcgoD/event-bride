using Identity.Domain.Entities;

namespace Identity.Domain.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task UpdateAsync(RefreshToken refreshToken);
    Task RevokeAllForUserAsync(string userId, CancellationToken cancellationToken = default);
}
