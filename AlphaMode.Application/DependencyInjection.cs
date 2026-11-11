using AlphaMode.Application.Games.Interfaces;
using AlphaMode.Application.Games.Services;
using AlphaMode.Application.Payments.Interfaces;
using AlphaMode.Application.Payments.Services;
using AlphaMode.Application.Players.Interfaces;
using AlphaMode.Application.Players.Services;
using AlphaMode.Application.PlayerTournamentRegistrations.Interfaces;
using AlphaMode.Application.PlayerTournamentRegistrations.Services;
using AlphaMode.Application.PrizePools.Interfaces;
using AlphaMode.Application.PrizePools.Services;
using AlphaMode.Application.TournamentFixtures.Interfaces;
using AlphaMode.Application.TournamentFixtures.Services;
using AlphaMode.Application.TournamentMatches.Interfaces;
using AlphaMode.Application.TournamentMatches.Services;
using AlphaMode.Application.TournamentRegistrations.Interfaces;
using AlphaMode.Application.TournamentRegistrations.Services;
using AlphaMode.Application.Tournaments.Interfaces;
using AlphaMode.Application.Tournaments.Services;
using AlphaMode.Application.TournamentStandings.Interfaces;
using AlphaMode.Application.TournamentStandings.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AlphaMode.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ITournamentService, TournamentService>();

        services.AddScoped<ITournamentPlayerRegistrationService, TournamentPlayerRegistrationService>();

        services.AddScoped<IGameService, GameService>();

        services.AddScoped<IPrizePoolService, PrizePoolService>();

        services.AddScoped<IPlayerService, PlayerService>();

        services.AddScoped<ITournamentRegistrationService, TournamentRegistrationsService>();

        services.AddScoped<IPaymentService, PaymentService>();

        services.AddScoped<ITournamentFixtureService, TournamentFixtureService>();

        services.AddScoped<ITournamentMatchService, TournamentMatchService>();

        services.AddScoped<ITournamentMatchService, TournamentMatchService>();

        services.AddScoped<IMatchResultProcessor, MatchResultProcessor>();

        services.AddScoped<ITournamentStandingService, AlphaModeStandingService>();

        services.AddScoped<ITournamentStandingsQueryService, TournamentStandingsQueryService>();

        services.AddScoped<ITournamentLifecycleService, TournamentLifecycleService>();

        services.AddScoped<ITournamentReadinessService, TournamentReadinessService>();

        return services;
    }
}
