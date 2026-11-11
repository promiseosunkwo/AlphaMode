using AlphaMode.Application.Common.Models;
using AlphaMode.Application.TournamentFixtures.DTOs;

namespace AlphaMode.Application.TournamentFixtures.Interfaces;

public interface ITournamentFixtureService
{
    Task<Result<TournamentFixtureDto>> GetAsync(Guid tournamentId, CancellationToken cancellationToken = default);

    Task<Result<TournamentFixtureDto>> PreviewAsync(Guid tournamentId,  CancellationToken cancellationToken = default);

    Task<Result<FixtureGenerationResultDto>> GenerateAsync(Guid tournamentId,  CancellationToken cancellationToken = default);
}
