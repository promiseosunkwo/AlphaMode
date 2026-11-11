using AlphaMode.Application.Common.Models;
using AlphaMode.Application.Dashboards.DTOs;

namespace AlphaMode.Application.Dashboards.Interfaces;

public interface IDashboardQueryService
{
    Task<Result<UserDashboardDto>> GetUserDashboardAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<UserMatchDto>>> GetUserMatchesAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<UserFixturesDto>> GetUserFixturesAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<AdminDashboardDto>> GetAdminDashboardAsync(CancellationToken cancellationToken = default);
}

