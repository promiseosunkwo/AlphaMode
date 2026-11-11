using AlphaMode.Application.Common.Models;
using AlphaMode.Application.Players.DTOs;
using AlphaMode.Application.Players.Requests;

namespace AlphaMode.Application.Players.Interfaces;

public interface IPlayerService
{
    Task<Result<PagedResult<PlayerDto>>> GetPagedAsync(
        int page = 1,
        int pageSize = 25,
        string? search = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);


    Task<Result<PlayerDetailsDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(UpdatePlayerRequest request, CancellationToken cancellationToken = default);

    Task<Result> SuspendAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
}