using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.TournamentFixtures.DTOs;
using AlphaMode.Application.TournamentFixtures.Interfaces;
using AlphaMode.Application.TournamentStandings.Interfaces;
using AlphaMode.Domain.Entities;
using AlphaMode.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Application.TournamentFixtures.Services;

public class TournamentFixtureService : ITournamentFixtureService
{
    private readonly IApplicationDbContext _context;
    private readonly ITournamentStandingsQueryService _leaguestandingUpdater;

    public TournamentFixtureService(IApplicationDbContext context, ITournamentStandingsQueryService leaguestandingUpdater)
    {
        _context = context;
        _leaguestandingUpdater = leaguestandingUpdater;
    }


    // =====================================================
    // GET EXISTING FIXTURES
    // =====================================================

    public async Task<Result<TournamentFixtureDto>> GetAsync(Guid tournamentId, CancellationToken cancellationToken = default)
    {
        if (tournamentId == Guid.Empty)
        {
            return Result<TournamentFixtureDto>.Failure("Tournament is required.");
        }

        var tournament = await _context.Tournaments
                .AsNoTracking()
                .Where(x => x.Id == tournamentId)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    GameName = x.Game.Name,
                    x.Format,
                    x.StartDate,
                    x.Legs
                })
                .FirstOrDefaultAsync(cancellationToken);

        if (tournament is null)
        {
            return Result<TournamentFixtureDto>.Failure("Tournament was not found.");
        }

        var eligiblePlayers = await _context.TournamentRegistrations.AsNoTracking().CountAsync(
                    x => x.TournamentId == tournamentId && x.Status == TournamentRegistrationStatus.CheckedIn, cancellationToken);

        var matches = await _context.Matches
                .AsNoTracking()
                .Where(x => x.TournamentId == tournamentId)
                .OrderBy(x => x.RoundNumber)
                .ThenBy(x => x.MatchWeek)
                .ThenBy(x => x.CreatedDate)
                .Select(x => new FixtureMatchDto
                {
                    MatchId = x.Id,

                    PlayerOneId = x.PlayerOneId,

                    PlayerOneName = x.PlayerOne != null ? x.PlayerOne.GamerTag : "TBD",

                    PlayerTwoId = x.PlayerTwoId,

                    PlayerTwoName = x.PlayerTwo != null ? x.PlayerTwo.GamerTag : "TBD",

                    RoundNumber = x.RoundNumber,

                    MatchWeek = x.MatchWeek,

                    Status = x.Status,

                    ScheduledAt = x.ScheduledAt,

                    WinnerId = x.WinnerId,

                    PlayerOneScore = x.Score != null ? x.Score.PlayerOneScore : null,

                    PlayerTwoScore = x.Score != null ? x.Score.PlayerTwoScore : null,

                    WentToExtraTime = x.Score != null && x.Score.WentToExtraTime,

                    PlayerOnePenaltyScore = x.Score != null ? x.Score.PlayerOnePenaltyScore : null,

                    PlayerTwoPenaltyScore = x.Score != null ? x.Score.PlayerTwoPenaltyScore : null,
                }).ToListAsync(cancellationToken);

                    var rounds = matches.GroupBy(x => x.RoundNumber).OrderBy(x => x.Key).Select(group =>
                    new FixtureRoundDto
                    {
                        RoundNumber = group.Key,

                        Name = GetRoundName(tournament.Format, group.Key, matches.Max(x => x.RoundNumber)),

                        Matches = group.ToList()
                    }).ToList();

        return Result<TournamentFixtureDto>.Success(new TournamentFixtureDto
                {
                    TournamentId = tournament.Id,

                    TournamentName = tournament.Name,

                    GameName = tournament.GameName,

                    Format = tournament.Format,

                    StartDate = tournament.StartDate,

                    Legs = tournament.Legs,

                    EligiblePlayers = eligiblePlayers,

                    FixturesGenerated = matches.Count > 0,

                    Rounds = rounds
                });
    }


    // =====================================================
    // PREVIEW
    // =====================================================

    public async Task<Result<TournamentFixtureDto>> PreviewAsync(Guid tournamentId, CancellationToken cancellationToken = default)
    {
        if (tournamentId == Guid.Empty)
        {
            return Result<TournamentFixtureDto>.Failure("Tournament is required.");
        }

        var tournament = await _context.Tournaments
                .AsNoTracking()
                .Where(x => x.Id == tournamentId)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    GameName = x.Game.Name,
                    x.Format,
                    x.StartDate,
                    x.Legs
                })
                .FirstOrDefaultAsync(cancellationToken);

        if (tournament is null)
        {
            return Result<TournamentFixtureDto>.Failure("Tournament was not found.");
        }

        var fixturesExist = await _context.Matches.AsNoTracking().AnyAsync(x => x.TournamentId == tournamentId, cancellationToken);

        if (fixturesExist)
        {
            return Result<TournamentFixtureDto>.Failure("Fixtures have already been generated for this tournament.");
        }

        var players = await GetEligiblePlayersAsync(tournamentId, cancellationToken);

        if (players.Count < 2)
        {
            return Result<TournamentFixtureDto>
                .Failure("At least two checked-in players are required to generate fixtures.");
        }

        List<FixtureRoundDto> rounds;

        switch (tournament.Format)
        {
            case TournamentFormat.League:

                rounds = BuildLeaguePreview(players, Math.Max(1, tournament.Legs));

                break;


            case TournamentFormat.SingleElimination:

                rounds = BuildSingleEliminationPreview(players);

                break;


            case TournamentFormat.DoubleElimination:

                return Result<TournamentFixtureDto>
                    .Failure("Double-elimination fixture generation is not available yet.");


            default:

                return Result<TournamentFixtureDto>
                    .Failure("This tournament format is not supported.");
        }

        return Result<TournamentFixtureDto>
            .Success(
                new TournamentFixtureDto
                {
                    TournamentId =
                        tournament.Id,

                    TournamentName =
                        tournament.Name,

                    GameName =
                        tournament.GameName,

                    Format =
                        tournament.Format,

                    StartDate =
                        tournament.StartDate,

                    Legs =
                        tournament.Legs,

                    EligiblePlayers =
                        players.Count,

                    FixturesGenerated =
                        false,

                    Rounds =
                        rounds
                });
    }


    // =====================================================
    // GENERATE
    // =====================================================

    public async Task<Result<FixtureGenerationResultDto>>GenerateAsync(Guid tournamentId, CancellationToken cancellationToken = default)
    {
        if (tournamentId == Guid.Empty)
        {
            return Result<FixtureGenerationResultDto>.Failure("Tournament is required.");
        }

        var tournament = await _context.Tournaments.FirstOrDefaultAsync(x => x.Id == tournamentId, cancellationToken);

        if (tournament is null)
        {
            return Result<FixtureGenerationResultDto>.Failure("Tournament was not found.");
        }

        // under consideration, this validation was introduced due to the inception of tournament readiness service.
        //if (tournament.Status != TournamentStatus.Upcoming)
        //{
        //    return Result<FixtureGenerationResultDto>.Failure(
        //        "Fixtures can only be generated when the tournament is Upcoming.");
        //}

        //if (tournament.RegistrationDeadline.HasValue &&
        //    DateTime.UtcNow < tournament.RegistrationDeadline.Value)
        //{
        //    return Result<FixtureGenerationResultDto>.Failure(
        //        "Registration must close before fixtures can be generated.");
        //}

        //if (DateTime.UtcNow >= tournament.StartDate)
        //{
        //    return Result<FixtureGenerationResultDto>.Failure(
        //        "Fixtures cannot be generated after the scheduled tournament start.");
        //}

        var fixturesExist = await _context.Matches.AnyAsync( x => x.TournamentId == tournamentId, cancellationToken);

        if (fixturesExist)
        {
            return Result<FixtureGenerationResultDto>.Failure("Fixtures have already been generated for this tournament.");
        }

        var players = await GetEligiblePlayersAsync(tournamentId, cancellationToken);

        if (players.Count < 2)
        {
            return Result<FixtureGenerationResultDto>.Failure("At least two checked-in players are required.");
        }

        List<Match> matches;

        switch (tournament.Format)
        {
            case TournamentFormat.League:

                matches = BuildLeagueMatches(tournament.Id, tournament.GameId, players, Math.Max(1, tournament.Legs));

                break;


            case TournamentFormat.SingleElimination:

                matches = BuildSingleEliminationMatches(tournament.Id, tournament.GameId, players);

                break;


            case TournamentFormat.DoubleElimination:

                return Result<FixtureGenerationResultDto>.Failure("Double-elimination fixture generation is not available yet.");


            default:

                return Result<FixtureGenerationResultDto>.Failure("This tournament format is not supported.");
        }

        if (matches.Count == 0)
        {
            return Result<FixtureGenerationResultDto>.Failure("No fixtures could be generated.");
        }

       _context.Matches.AddRange(matches);

        // Initialise standings for every eligible League player.
        if (tournament.Format == TournamentFormat.League)
        {
            var standingResult = await _leaguestandingUpdater.InitialiseAsync(tournament.Id,players
                .Select(x => x.PlayerId).ToList(), cancellationToken);

            if (!standingResult.Succeeded)
            {
                return Result<FixtureGenerationResultDto>.Failure(standingResult.Error ?? "Tournament standings could not be initialised.");
            }
        }

        // Persist fixtures and standings together.
        await _context.SaveChangesAsync(cancellationToken);

        return Result<FixtureGenerationResultDto>.Success(new FixtureGenerationResultDto
                {
                    MatchesCreated = matches.Count,

                    RoundsCreated = matches
                            .Select(x => x.RoundNumber)
                            .Distinct()
                            .Count(),

                    EligiblePlayers = players.Count
                },
                $"{matches.Count} fixtures generated successfully.");
    }


    // =====================================================
    // ELIGIBLE PLAYERS
    // =====================================================

    private async Task<List<EligiblePlayer>>GetEligiblePlayersAsync(Guid tournamentId, CancellationToken cancellationToken)
    {
        return await _context.TournamentRegistrations.AsNoTracking().Where(x =>x.TournamentId == tournamentId && x.Status ==
                    TournamentRegistrationStatus.CheckedIn &&
                x.Player.IsActive)
            .OrderByDescending(x =>
                x.Player.RankingPoints)
            .ThenBy(x =>
                x.Player.GamerTag)
            .Select(x =>
                new EligiblePlayer(
                    x.PlayerId,
                    x.Player.GamerTag,
                    x.Player.RankingPoints))
            .ToListAsync(cancellationToken);
    }


    // =====================================================
    // LEAGUE
    // =====================================================

    private static List<FixtureRoundDto>
        BuildLeaguePreview(IReadOnlyList<EligiblePlayer> players, int legs)
    {
        var pairings = BuildLeaguePairings(players, legs);

        return pairings
            .GroupBy(x => x.Round)
            .OrderBy(x => x.Key)
            .Select(group =>
                new FixtureRoundDto
                {
                    RoundNumber = group.Key,

                    Name =$"Matchweek {group.Key}",

                    Matches = group.Select(x => new FixtureMatchDto
                                {
                                    PlayerOneId =
                                        x.PlayerOne.PlayerId,

                                    PlayerOneName =
                                        x.PlayerOne.GamerTag,

                                    PlayerTwoId =
                                        x.PlayerTwo.PlayerId,

                                    PlayerTwoName =
                                        x.PlayerTwo.GamerTag,

                                    RoundNumber =
                                        x.Round,

                                    MatchWeek =
                                        x.Round,

                                    Status =
                                        MatchStatus.Pending
                                })
                            .ToList()
                })
            .ToList();
    }


    private static List<Match>
        BuildLeagueMatches(
            Guid tournamentId,
            Guid gameId,
            IReadOnlyList<EligiblePlayer> players,
            int legs)
    {
        var pairings =
            BuildLeaguePairings(
                players,
                legs);

        return pairings
            .Select(x =>
                new Match
                {
                    TournamentId =
                        tournamentId,

                    // Tournament matches now store GameId directly so they share the same query model as friendlies.
                    GameId = gameId,

                    Type = AlphaMode.Domain.Enums.MatchType.Tournament,

                    IsRanked = true,

                    PlayerOneId =
                        x.PlayerOne.PlayerId,

                    PlayerTwoId =
                        x.PlayerTwo.PlayerId,

                    RoundNumber =
                        x.Round,

                    MatchWeek =
                        x.Round,

                    Status =
                        MatchStatus.Pending,

                    ScheduledAt =
                        null,

                    WinnerId =
                        null
                })
            .ToList();
    }


    private static List<LeaguePairing>BuildLeaguePairings(IReadOnlyList<EligiblePlayer> sourcePlayers, int legs)
    {
        var players = sourcePlayers.Select(x =>
                    new LeagueSlot(
                        x.PlayerId,
                        x.GamerTag,
                        x.RankingPoints,
                        false))
                .ToList();

        // Circle method requires an even number.
        if (players.Count % 2 != 0)
        {
            players.Add(
                new LeagueSlot(
                    null,
                    "BYE",
                    0,
                    true));
        }

        var numberOfPlayers =
            players.Count;

        var roundsPerLeg =
            numberOfPlayers - 1;

        var matchesPerRound =
            numberOfPlayers / 2;

        var rotation =
            new List<LeagueSlot>(players);

        var firstLeg =
            new List<LeaguePairing>();


        for (var round = 1;
             round <= roundsPerLeg;
             round++)
        {
            for (var matchIndex = 0;
                 matchIndex < matchesPerRound;
                 matchIndex++)
            {
                var home =
                    rotation[matchIndex];

                var away =
                    rotation[
                        numberOfPlayers -
                        1 -
                        matchIndex];

                if (home.IsBye ||
                    away.IsBye)
                {
                    continue;
                }

                // Swap periodically so one player isn't
                // always displayed as PlayerOne.
                if (round % 2 == 0)
                {
                    (home, away) =
                        (away, home);
                }

                firstLeg.Add(
                    new LeaguePairing(
                        round,
                        ToEligiblePlayer(home),
                        ToEligiblePlayer(away)));
            }

            RotateLeaguePlayers(rotation);
        }


        var result = new List<LeaguePairing>();

        for (var leg = 0;
             leg < legs;
             leg++)
        {
            var roundOffset = leg * roundsPerLeg;

            foreach (var fixture in firstLeg)
            {
                var reverse = leg % 2 == 1;

                result.Add(new LeaguePairing(fixture.Round + roundOffset,

                        reverse
                            ? fixture.PlayerTwo
                            : fixture.PlayerOne,

                        reverse
                            ? fixture.PlayerOne
                            : fixture.PlayerTwo));
            }
        }

        return result;
    }


    private static void RotateLeaguePlayers(
        List<LeagueSlot> players)
    {
        if (players.Count <= 2)
            return;

        var last =
            players[^1];

        players.RemoveAt(
            players.Count - 1);

        players.Insert(
            1,
            last);
    }


    private static EligiblePlayer ToEligiblePlayer(
        LeagueSlot slot)
    {
        return new EligiblePlayer(
            slot.PlayerId!.Value,
            slot.GamerTag,
            slot.RankingPoints);
    }


    // =====================================================
    // SINGLE ELIMINATION
    // =====================================================

    private static List<FixtureRoundDto>BuildSingleEliminationPreview(IReadOnlyList<EligiblePlayer> players)
    {
        var bracketSize = GetNextPowerOfTwo(players.Count);

        var firstRoundMatchCount = bracketSize / 2;

        var seededSlots =
            BuildSeededBracket(players);

        var rounds =
            new List<FixtureRoundDto>();


        // First round contains the actual seeded players.
        var firstRoundMatches = new List<FixtureMatchDto>();

        for (var i = 0; i < firstRoundMatchCount; i++)
        {
            var playerOne = seededSlots[i * 2];

            var playerTwo = seededSlots[(i * 2) + 1];

            firstRoundMatches.Add(new FixtureMatchDto
                {
                    PlayerOneId = playerOne?.PlayerId,

                    PlayerOneName = playerOne?.GamerTag ?? "BYE",

                    PlayerTwoId = playerTwo?.PlayerId,

                    PlayerTwoName = playerTwo?.GamerTag ?? "BYE",

                    RoundNumber = 1,

                    Status = MatchStatus.Pending
                });
        }

        rounds.Add(
            new FixtureRoundDto
            {
                RoundNumber = 1,

                Name =
                    GetEliminationRoundName(
                        firstRoundMatchCount),

                Matches =
                    firstRoundMatches
            });


        // Later rounds are placeholders in the preview.
        var matchesInRound =
            firstRoundMatchCount / 2;

        var roundNumber =
            2;

        while (matchesInRound >= 1)
        {
            var previewMatches =
                new List<FixtureMatchDto>();

            for (var i = 0;
                 i < matchesInRound;
                 i++)
            {
                previewMatches.Add(
                    new FixtureMatchDto
                    {
                        PlayerOneName = "TBD",
                        PlayerTwoName = "TBD",
                        RoundNumber = roundNumber,
                        Status = MatchStatus.Pending
                    });
            }

            rounds.Add(
                new FixtureRoundDto
                {
                    RoundNumber =
                        roundNumber,

                    Name =
                        GetEliminationRoundName(
                            matchesInRound),

                    Matches =
                        previewMatches
                });

            matchesInRound /= 2;

            roundNumber++;
        }

        return rounds;
    }


    private static List<Match>
        BuildSingleEliminationMatches(
            Guid tournamentId,
            Guid gameId,
            IReadOnlyList<EligiblePlayer> players)
    {
        var seededSlots =
            BuildSeededBracket(players);

        var matches =
            new List<Match>();

        var firstRoundMatches =
            seededSlots.Count / 2;

        for (var i = 0;
             i < firstRoundMatches;
             i++)
        {
            var playerOne =
                seededSlots[i * 2];

            var playerTwo =
                seededSlots[(i * 2) + 1];

            // We save first-round matches only.
            // Later rounds depend on winners and should
            // be created/progressed by the match engine.
            var match =
                new Match
                {
                    TournamentId =
                        tournamentId,

                    // Tournament matches remain ranked and retain their tournament while also carrying GameId.
                    GameId = gameId,

                    Type = AlphaMode.Domain.Enums.MatchType.Tournament,

                    IsRanked = true,

                    PlayerOneId =
                        playerOne?.PlayerId,

                    PlayerTwoId =
                        playerTwo?.PlayerId,

                    RoundNumber =
                        1,

                    MatchWeek =
                        null,

                    Status =
                        MatchStatus.Pending,

                    ScheduledAt =
                        null
                };

            // A bye can be resolved immediately.
            if (playerOne is not null &&
                playerTwo is null)
            {
                match.WinnerId =
                    playerOne.PlayerId;

                match.Status =
                    MatchStatus.Completed;
            }
            else if (playerOne is null &&
                     playerTwo is not null)
            {
                match.WinnerId =
                    playerTwo.PlayerId;

                match.Status =
                    MatchStatus.Completed;
            }

            matches.Add(match);
        }

        return matches;
    }


    // =====================================================
    // SEEDING
    // =====================================================

    private static List<EligiblePlayer?>
        BuildSeededBracket(
            IReadOnlyList<EligiblePlayer> players)
    {
        var orderedPlayers = players.OrderByDescending(x => x.RankingPoints).ThenBy(x => x.GamerTag).ToList();

        var bracketSize = GetNextPowerOfTwo(orderedPlayers.Count);

        var slots = Enumerable.Repeat<EligiblePlayer?>(null, bracketSize).ToList();

        /*
         * MVP deterministic seeding:
         *
         * strongest vs weakest,
         * second strongest vs second weakest, etc.
         *
         * Missing opponents become byes.
         */

        var left = 0;

        var right = bracketSize - 1;

        var playerIndex = 0;

        while (playerIndex < orderedPlayers.Count)
        {
            if (playerIndex < orderedPlayers.Count)
            {
                slots[left] = orderedPlayers[playerIndex];

                playerIndex++;
            }

            if (playerIndex < orderedPlayers.Count)
            {
                slots[right] = orderedPlayers[playerIndex];

                playerIndex++;
            }

            left++;
            right--;
        }

        return ConvertToMatchPairSlots(orderedPlayers, bracketSize);
    }


    private static List<EligiblePlayer?> ConvertToMatchPairSlots( IReadOnlyList<EligiblePlayer> players, int bracketSize)
    {
        var result = new List<EligiblePlayer?>();

        var playerCount = players.Count;

        var byeCount = bracketSize - playerCount;

        var ordered = players.OrderByDescending(x => x.RankingPoints).ThenBy(x => x.GamerTag).ToList();

        /*
         * Give the highest seeds the available byes.
         *
         * Example: 6 players in an 8-player bracket
         *
         * Seed 1 vs BYE
         * Seed 4 vs Seed 5
         * Seed 3 vs Seed 6
         * Seed 2 vs BYE
         */

        var seeds = GenerateSeedOrder(bracketSize);

        var seedPlayers = new Dictionary<int, EligiblePlayer?>();

        for (var seed = 1;
             seed <= bracketSize;
             seed++)
        {
            seedPlayers[seed] =
                seed <= playerCount
                    ? ordered[seed - 1]
                    : null;
        }

        foreach (var seed in seeds)
        {
            result.Add(
                seedPlayers[seed]);
        }

        return result;
    }


    private static List<int>
        GenerateSeedOrder(
            int bracketSize)
    {
        if (bracketSize == 2)
        {
            return [1, 2];
        }

        var seeds =
            new List<int>
            {
                1,
                2
            };

        for (var size = 4;
             size <= bracketSize;
             size *= 2)
        {
            var next =
                new List<int>();

            foreach (var seed in seeds)
            {
                next.Add(seed);

                next.Add(
                    size + 1 - seed);
            }

            seeds =
                next;
        }

        return seeds;
    }


    private static int GetNextPowerOfTwo(
        int value)
    {
        var power =
            1;

        while (power < value)
        {
            power *= 2;
        }

        return power;
    }


    // =====================================================
    // ROUND NAMES
    // =====================================================

    private static string GetRoundName(
        TournamentFormat format,
        int roundNumber,
        int totalRounds)
    {
        if (format == TournamentFormat.League)
        {
            return $"Matchweek {roundNumber}";
        }

        var remaining =
            totalRounds - roundNumber;

        return remaining switch
        {
            0 => "Final",
            1 => "Semi-finals",
            2 => "Quarter-finals",
            _ => $"Round {roundNumber}"
        };
    }


    private static string GetEliminationRoundName(
        int matchesInRound)
    {
        return matchesInRound switch
        {
            1 => "Final",
            2 => "Semi-finals",
            4 => "Quarter-finals",
            8 => "Round of 16",
            16 => "Round of 32",
            _ => $"Round of {matchesInRound * 2}"
        };
    }


    // =====================================================
    // INTERNAL TYPES
    // =====================================================

    private sealed record EligiblePlayer(
        Guid PlayerId,
        string GamerTag,
        int RankingPoints);


    private sealed record LeagueSlot(
        Guid? PlayerId,
        string GamerTag,
        int RankingPoints,
        bool IsBye);


    private sealed record LeaguePairing(
        int Round,
        EligiblePlayer PlayerOne,
        EligiblePlayer PlayerTwo);
}

