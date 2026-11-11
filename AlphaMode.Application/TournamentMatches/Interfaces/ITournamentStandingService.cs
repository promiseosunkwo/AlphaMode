using AlphaMode.Application.Common.Models;
using AlphaMode.Domain.Entities;

namespace AlphaMode.Application.TournamentMatches.Interfaces;

//This interface relates to the standings for each particular tournament. THe Match Service updates the Alpha Mode's ranking table
public interface ITournamentStandingService
{
    Task<Result> ApplyResultAsync(Match match, int playerOneScore, int playerTwoScore, CancellationToken cancellationToken = default);

    Task<Result> ApplyForfeitAsync(Match match, CancellationToken cancellationToken = default);

}
