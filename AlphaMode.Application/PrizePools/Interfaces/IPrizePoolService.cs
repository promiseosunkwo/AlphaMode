using AlphaMode.Application.Common.Models;
using AlphaMode.Application.PrizePools.DTOs;
using AlphaMode.Application.PrizePools.Requests;

namespace AlphaMode.Application.PrizePools.Interfaces;

public interface IPrizePoolService
{
    Task<Result<IReadOnlyList<PrizePoolDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<PrizePoolDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PrizePoolDto>> GetByTournamentIdAsync(Guid tournamentId, CancellationToken cancellationToken = default);

    Task<Result<Guid>> CreateAsync(CreatePrizePoolRequest request, CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(UpdatePrizePoolRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}