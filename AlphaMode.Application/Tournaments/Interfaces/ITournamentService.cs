using AlphaMode.Application.Common.Models;
using AlphaMode.Application.Tournaments.DTOs;

namespace AlphaMode.Application.Tournaments.Interfaces;

public interface ITournamentService
{
    Task<Result<IReadOnlyList<TournamentDto>>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Result<TournamentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<TournamentDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<Result<Guid>> CreateAsync(CreateTournamentRequest request, CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(UpdateTournamentRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}