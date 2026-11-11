using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.PlayerTournamentRegistrations.DTOs;
using AlphaMode.Application.PlayerTournamentRegistrations.Interfaces;
using AlphaMode.Application.TournamentRegistration.DTOs;
using AlphaMode.Application.TournamentRegistration.Requests;
using AlphaMode.Application.TournamentRegistrationAdminDisplay.DTOs;
using AlphaMode.Application.TournamentRegistrations.DTOs;
using AlphaMode.Application.TournamentRegistrations.Interfaces;
using AlphaMode.Application.TournamentRegistrations.Requests;
using AlphaMode.Components.Pages.UserPages;
using AlphaMode.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using RegistrationEntity = AlphaMode.Domain.Entities.TournamentRegistration; 

namespace AlphaMode.Application.TournamentRegistrations.Services;

public class TournamentRegistrationsService : ITournamentRegistrationService
{
    private const int DefaultPageSize = 25;
    private const int MaximumPageSize = 100;

    private const int CheckInWindowMinutes = 30;

    private readonly IApplicationDbContext _context;
    private readonly ITournamentPlayerRegistrationService _playerRegistrationService;

    public TournamentRegistrationsService(IApplicationDbContext context, ITournamentPlayerRegistrationService playerRegistrationService)
    {
        _context = context;
        _playerRegistrationService = playerRegistrationService;
    }


    // =====================================================
    // PAGED REGISTRATIONS
    // =====================================================

    public async Task<Result<PagedResult<TournamentRegistrationDto>>>GetPagedAsync(
            int page = 1,
            int pageSize = DefaultPageSize,
            string? search = null,
            Guid? tournamentId = null,
            TournamentRegistrationStatus? status = null,
            CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);

        pageSize = pageSize <= 0
            ? DefaultPageSize
            : Math.Min(pageSize, MaximumPageSize);


        var query = _context.TournamentRegistrations
            .AsNoTracking()
            .AsQueryable();


        if (tournamentId.HasValue)
        {
            query = query.Where(x =>
                x.TournamentId == tournamentId.Value);
        }


        if (status.HasValue)
        {
            query = query.Where(x =>
                x.Status == status.Value);
        }


        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(x =>
                x.Player.GamerTag.Contains(term) ||
                x.Player.User.FirstName.Contains(term) ||
                x.Player.User.LastName.Contains(term) ||
                (x.Player.User.Email != null &&
                 x.Player.User.Email.Contains(term)) ||
                x.Tournament.Name.Contains(term) ||
                x.Tournament.Game.Name.Contains(term));
        }


        var totalCount = await query.CountAsync(
            cancellationToken);


        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(
                totalCount / (double)pageSize);


        if (totalPages > 0 &&
            page > totalPages)
        {
            page = totalPages;
        }


        var items = await query
            .OrderByDescending(x => x.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x =>
                new TournamentRegistrationDto
                {
                    Id = x.Id,

                    TournamentId = x.TournamentId,

                    TournamentName = x.Tournament.Name,

                    TournamentSlug = x.Tournament.Slug,

                    GameName = x.Tournament.Game.Name,

                    PlayerId = x.PlayerId,

                    GamerTag = x.Player.GamerTag,

                    PlayerName = (x.Player.User.FirstName + " " + x.Player.User.LastName).Trim(),

                    Email = x.Player.User.Email ?? string.Empty,

                    AvatarUrl = x.Player.User.AvatarUrl,

                    RankingPoints = x.Player.RankingPoints,

                    PlayerIsActive = x.Player.IsActive,

                    Status = x.Status,

                    RegisteredAt = x.CreatedDate,

                    CheckedInAt = x.CheckedInAt,

                    DisqualifiedAt = x.DisqualifiedAt,

                    DisqualificationReason = x.DisqualificationReason,

                    EntryFee = x.Tournament.EntryFee,

                    TournamentStartDate = x.Tournament.StartDate
                })
            .ToListAsync(cancellationToken);


        return Result<PagedResult<TournamentRegistrationDto>>
            .Success(
                PagedResult<TournamentRegistrationDto>
                    .Create(
                        items,
                        page,
                        pageSize,
                        totalCount));
    }


    // =====================================================
    // DETAILS
    // =====================================================

    public async Task<Result<TournamentRegistrationDetailsDto>>
        GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
    {
        var registration =
            await _context.TournamentRegistrations
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x =>
                    new TournamentRegistrationDetailsDto
                    {
                        Id = x.Id,

                        TournamentId =
                            x.TournamentId,

                        TournamentName =
                            x.Tournament.Name,

                        TournamentSlug =
                            x.Tournament.Slug,

                        GameName =
                            x.Tournament.Game.Name,

                        TournamentStartDate =
                            x.Tournament.StartDate,

                        TournamentEndDate =
                            x.Tournament.EndDate,

                        EntryFee =
                            x.Tournament.EntryFee,

                        PlayerId =
                            x.PlayerId,

                        GamerTag =
                            x.Player.GamerTag,

                        FirstName =
                            x.Player.User.FirstName,

                        LastName =
                            x.Player.User.LastName,

                        Email =
                            x.Player.User.Email
                            ?? string.Empty,

                        AvatarUrl =
                            x.Player.User.AvatarUrl,

                        RankingPoints =
                            x.Player.RankingPoints,

                        PlayerIsActive =
                            x.Player.IsActive,

                        Status =
                            x.Status,

                        RegisteredAt =
                            x.CreatedDate,

                        CheckedInAt =
                            x.CheckedInAt,

                        DisqualifiedAt =
                            x.DisqualifiedAt,

                        DisqualificationReason =
                            x.DisqualificationReason
                    })
                .FirstOrDefaultAsync(
                    cancellationToken);


        if (registration is null)
        {
            return Result<TournamentRegistrationDetailsDto>
                .Failure(
                    "Tournament registration was not found.");
        }


        return Result<TournamentRegistrationDetailsDto>
            .Success(registration);
    }


    // =====================================================
    // TOURNAMENT FILTER OPTIONS
    // =====================================================

    public async Task<Result<IReadOnlyList<TournamentRegistrationTournamentDto>>>
        GetTournamentOptionsAsync(
            CancellationToken cancellationToken = default)
    {
        var tournaments =
            await _context.Tournaments
                .AsNoTracking()
                .OrderByDescending(x => x.StartDate)
                .Select(x =>
                    new TournamentRegistrationTournamentDto
                    {
                        Id = x.Id,

                        Name = x.Name,

                        GameName =
                            x.Game.Name
                    })
                .ToListAsync(
                    cancellationToken);


        return Result<
            IReadOnlyList<TournamentRegistrationTournamentDto>>
            .Success(tournaments);
    }


    // =====================================================
    // MARK REGISTERED
    // =====================================================

    public async Task<Result> MarkRegisteredAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var registration = await FindRegistrationAsync(id, cancellationToken);


        if (registration is null)
        {
            return Result.Failure(
                "Tournament registration was not found.");
        }


        if (!registration.Player.IsActive)
        {
            return Result.Failure(
                "This player is currently suspended and cannot be registered.");
        }


        if (registration.Status != TournamentRegistrationStatus.PendingPayment)
        {
            return Result.Failure("Only registrations awaiting payment can be marked as registered.");
        }


        registration.Status = TournamentRegistrationStatus.Registered;


        await _context.SaveChangesAsync(cancellationToken);


        return Result.Success("Player registration confirmed.");
    }


    // =====================================================
    // CHECK IN
    // =====================================================

    public async Task<Result> CheckInAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var registration =
            await FindRegistrationAsync(
                id,
                cancellationToken);


        if (registration is null)
        {
            return Result.Failure(
                "Tournament registration was not found.");
        }


        if (!registration.Player.IsActive)
        {
            return Result.Failure(
                "Suspended players cannot check in.");
        }


        if (registration.Status !=
            TournamentRegistrationStatus.Registered)
        {
            return Result.Failure(
                "Only registered players can check in.");
        }


        registration.Status =
            TournamentRegistrationStatus.CheckedIn;

        registration.CheckedInAt =
            DateTime.UtcNow;


        await _context.SaveChangesAsync(
            cancellationToken);


        return Result.Success(
            "Player checked in successfully.");
    }


    // =====================================================
    // WITHDRAW
    // =====================================================

    public async Task<Result> WithdrawAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var registration =
            await _context.TournamentRegistrations
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);


        if (registration is null)
        {
            return Result.Failure(
                "Tournament registration was not found.");
        }


        if (IsTerminal(registration.Status))
        {
            return Result.Failure(
                "This registration can no longer be withdrawn.");
        }


        registration.Status =
            TournamentRegistrationStatus.Withdrawn;


        await _context.SaveChangesAsync(
            cancellationToken);


        return Result.Success(
            "Player withdrawn from the tournament.");
    }


    // =====================================================
    // CANCEL
    // =====================================================

    public async Task<Result> CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var registration =
            await _context.TournamentRegistrations
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);


        if (registration is null)
        {
            return Result.Failure("Tournament registration was not found.");
        }


        if (IsTerminal(registration.Status))
        {
            return Result.Failure("This registration is already closed.");
        }


        registration.Status = TournamentRegistrationStatus.Cancelled;


        await _context.SaveChangesAsync(cancellationToken);


        return Result.Success("Tournament registration cancelled.");
    }


    // =====================================================
    // DISQUALIFY
    // =====================================================

    public async Task<Result> DisqualifyAsync(
        DisqualifyRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return Result.Failure(
                "A disqualification reason is required.");
        }


        var reason = request.Reason.Trim();


        if (reason.Length < 5)
        {
            return Result.Failure(
                "Please provide a meaningful disqualification reason.");
        }


        if (reason.Length > 500)
        {
            return Result.Failure(
                "The disqualification reason cannot exceed 500 characters.");
        }


        var registration =
            await _context.TournamentRegistrations
                .FirstOrDefaultAsync(
                    x => x.Id == request.RegistrationId,
                    cancellationToken);


        if (registration is null)
        {
            return Result.Failure(
                "Tournament registration was not found.");
        }


        if (IsTerminal(registration.Status))
        {
            return Result.Failure(
                "This registration is already closed.");
        }


        registration.Status = TournamentRegistrationStatus.Disqualified;

        registration.DisqualifiedAt = DateTime.UtcNow;

        registration.DisqualificationReason = reason;


        await _context.SaveChangesAsync(cancellationToken);


        return Result.Success("Player disqualified from the tournament.");
    }


    public async Task<Result<IReadOnlyList<UserTournamentDto>>>GetAvailableForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<IReadOnlyList<UserTournamentDto>>
                .Failure("User account could not be identified.");
        }

        var player = await _context.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);

        if (player is null)
        {
            return Result<IReadOnlyList<UserTournamentDto>>
                .Failure("Player profile was not found.");
        }

        var now = DateTime.UtcNow;

        var tournaments = await _context.Tournaments.AsNoTracking()

            // Do not show finished/cancelled/archived tournaments
            // in the available competition browser.
            .Where(x =>
                x.EndDate > now &&
                x.Status != TournamentStatus.Completed &&
                x.Status != TournamentStatus.Cancelled)

            .OrderBy(x => x.StartDate)

            .Select(x => new UserTournamentDto
            {
                Id = x.Id,

                Name = x.Name,

                Slug = x.Slug,

                Description = x.Description,

                GameId = x.GameId,

                Game = x.Game.Name,

                ImageUrl = x.ImageUrl,

                Format = x.Format,

                Status = x.Status,

                StartsAt = x.StartDate,

                EndsAt = x.EndDate,

                RegistrationOpenDate =
                    x.RegistrationOpenDate,

                RegistrationDeadline =
                    x.RegistrationDeadline,

                EntryFee =
                    x.EntryFee,

                Capacity =
                    x.MaxPlayers,

                RegisteredPlayers =
                    x.Registrations.Count(r =>
                        r.Status ==
                            TournamentRegistrationStatus.Registered ||
                        r.Status ==
                            TournamentRegistrationStatus.CheckedIn),

                RegistrationId =
                    x.Registrations
                        .Where(r =>
                            r.PlayerId == player.Id)
                        .Select(r => (Guid?)r.Id)
                        .FirstOrDefault(),

                RegistrationStatus =
                    x.Registrations
                        .Where(r =>
                            r.PlayerId == player.Id)
                        .Select(r =>
                            (TournamentRegistrationStatus?)r.Status)
                        .FirstOrDefault(),

                PendingPaymentId =
                    x.Registrations
                        .Where(r =>
                            r.PlayerId == player.Id)
                        .SelectMany(r => r.Payments)
                        .Where(p =>
                            p.Status == PaymentStatus.Pending &&
                            p.Type == PaymentType.TournamentEntry)
                        .Select(p => (Guid?)p.Id)
                        .FirstOrDefault(),

                PrizePool =
                    _context.PrizePools
                        .Where(p =>
                            p.TournamentId == x.Id)
                        .Select(p =>
                            (decimal?)p.TotalPrize)
                        .FirstOrDefault() ?? 0,

                Currency =
                    _context.PrizePools
                        .Where(p =>
                            p.TournamentId == x.Id)
                        .Select(p => p.Currency)
                        .FirstOrDefault() ?? "NGN"
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<UserTournamentDto>>
            .Success(tournaments);
    }

    public async Task<Result<TournamentRegistrationResultDto>>RegisterForTournamentAsync(RegisterForTournamentRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return Result<TournamentRegistrationResultDto>.Failure(
                "User account could not be identified.");
        }

        if (request.TournamentId == Guid.Empty)
        {
            return Result<TournamentRegistrationResultDto>.Failure(
                "Tournament is required.");
        }

        var player = await _context.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.UserId == request.UserId,
                cancellationToken);

        if (player is null)
        {
            return Result<TournamentRegistrationResultDto>.Failure(
                "Player profile was not found.");
        }

        if (!player.IsActive)
        {
            return Result<TournamentRegistrationResultDto>.Failure(
                "Your player account is currently unable to enter tournaments.");
        }

        var result = await _playerRegistrationService.RegisterAsync(
            new RegisterPlayerForTournamentRequest
            {
                TournamentId = request.TournamentId,
                PlayerId = player.Id
            },
            cancellationToken);

        if (!result.Succeeded || result.Value is null)
        {
            return Result<TournamentRegistrationResultDto>.Failure(
                result.Error ?? "Unable to register for this tournament.");
        }

            return Result<TournamentRegistrationResultDto>.Success(
             new TournamentRegistrationResultDto
             {
                 RegistrationId = result.Value.Id,
                 TournamentId = result.Value.TournamentId,
                 Status = result.Value.Status,
                 PaymentId = null,
                 PaymentRequired = false,
                 AmountDue = 0,
                 Currency = "NGN"
             },
             result.Message ?? "You have successfully registered for the tournament.");
    }


    public async Task<Result<IReadOnlyList<MyTournamentDto>>>GetMyTournamentsAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<IReadOnlyList<MyTournamentDto>>
                .Failure("User account could not be identified.");
        }

        var playerExists = await _context.Players
                .AsNoTracking()
                .AnyAsync(
                    x => x.UserId == userId,
                    cancellationToken);

        if (!playerExists)
        {
            return Result<IReadOnlyList<MyTournamentDto>>
                .Failure("Player profile was not found.");
        }

        var registrations = await _context.TournamentRegistrations
                .AsNoTracking()
                .Where(x =>
                    x.Player.UserId == userId)
                .OrderBy(x => x.Tournament.StartDate)
                .Select(x => new MyTournamentDto
                {
                    RegistrationId = x.Id,

                    TournamentId = x.TournamentId,

                    TournamentName = x.Tournament.Name,

                    TournamentSlug = x.Tournament.Slug,

                    Description = x.Tournament.Description,

                    GameId = x.Tournament.GameId,

                    GameName = x.Tournament.Game.Name,

                    ImageUrl = x.Tournament.ImageUrl,

                    Format = x.Tournament.Format,

                    TournamentStatus = x.Tournament.Status,

                    RegistrationStatus = x.Status,

                    StartDate = x.Tournament.StartDate,

                    EndDate = x.Tournament.EndDate,

                    RegisteredAt = x.CreatedDate,

                    CheckedInAt = x.CheckedInAt,

                    EntryFee = x.Tournament.EntryFee,

                    MaxPlayers = x.Tournament.MaxPlayers,

                    RegisteredPlayers = x.Tournament.Registrations.Count(r => r.Status == TournamentRegistrationStatus.Registered || r.Status == TournamentRegistrationStatus.CheckedIn),

                    PrizePool = _context.PrizePools
                            .Where(p =>
                                p.TournamentId ==
                                x.TournamentId)
                            .Select(p =>
                                (decimal?)p.TotalPrize)
                            .FirstOrDefault() ?? 0,

                    Currency = _context.PrizePools
                            .Where(p =>
                                p.TournamentId ==
                                x.TournamentId)
                            .Select(p =>
                                p.Currency)
                            .FirstOrDefault() ?? "NGN",

                    PendingPaymentId = x.Payments
                            .Where(p =>
                                p.Status ==
                                    PaymentStatus.Pending &&
                                p.Type ==
                                    PaymentType.TournamentEntry)
                            .OrderByDescending(p =>
                                p.CreatedDate)
                            .Select(p =>
                                (Guid?)p.Id)
                            .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<MyTournamentDto>>
            .Success(registrations);
    }


    public async Task<Result<MyTournamentDto>>GetMyTournamentAsync(
        Guid tournamentId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (tournamentId == Guid.Empty)
        {
            return Result<MyTournamentDto>
                .Failure("Tournament is required.");
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<MyTournamentDto>
                .Failure("User account could not be identified.");
        }

        var tournament =
            await _context.TournamentRegistrations
                .AsNoTracking()
                .Where(x =>
                    x.TournamentId == tournamentId &&
                    x.Player.UserId == userId)
                .Select(x => new MyTournamentDto
                {
                    RegistrationId = x.Id,

                    TournamentId = x.TournamentId,

                    TournamentName = x.Tournament.Name,

                    TournamentSlug = x.Tournament.Slug,

                    Description = x.Tournament.Description,

                    GameId = x.Tournament.GameId,

                    GameName = x.Tournament.Game.Name,

                    ImageUrl = x.Tournament.ImageUrl,

                    Format = x.Tournament.Format,

                    TournamentStatus = x.Tournament.Status,

                    RegistrationStatus = x.Status,

                    StartDate = x.Tournament.StartDate,

                    EndDate = x.Tournament.EndDate,

                    RegisteredAt = x.CreatedDate,

                    CheckedInAt = x.CheckedInAt,

                    EntryFee = x.Tournament.EntryFee,

                    MaxPlayers = x.Tournament.MaxPlayers,

                    RegisteredPlayers = x.Tournament.Registrations.Count(r => r.Status == TournamentRegistrationStatus.Registered || r.Status == TournamentRegistrationStatus.CheckedIn),

                    PrizePool = _context.PrizePools.Where(p => p.TournamentId == x.TournamentId).Select(p => (decimal?)p.TotalPrize).FirstOrDefault() ?? 0,

                    Currency = _context.PrizePools.Where(p => p.TournamentId == x.TournamentId).Select(p => p.Currency).FirstOrDefault() ?? "NGN",

                    PendingPaymentId = x.Payments.Where(p => p.Status == PaymentStatus.Pending && p.Type == PaymentType.TournamentEntry)
                            .OrderByDescending(p => p.CreatedDate)
                            .Select(p => (Guid?)p.Id)
                            .FirstOrDefault()
                })
                .FirstOrDefaultAsync(
                    cancellationToken);

        if (tournament is null)
        {
            return Result<MyTournamentDto>.Failure("Tournament registration was not found.");
        }

        return Result<MyTournamentDto>.Success(tournament);
    }


    public async Task<Result> PlayerCheckInAsync(Guid registrationId, string userId, CancellationToken cancellationToken = default)
    {
        if (registrationId == Guid.Empty)
        {
            return Result.Failure("Tournament registration is required.");
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Failure("User account could not be identified.");
        }

        var registration = await _context.TournamentRegistrations
                .Include(x => x.Player)
                .Include(x => x.Tournament)
                .FirstOrDefaultAsync(
                    x => x.Id == registrationId,
                    cancellationToken);

        if (registration is null)
        {
            return Result.Failure("Tournament registration was not found.");
        }

        // The authenticated user must own this registration.
        if (registration.Player.UserId != userId)
        {
            return Result.Failure("You cannot check in this registration.");
        }

        if (!registration.Player.IsActive)
        {
            return Result.Failure("Your player account is currently inactive.");
        }

        if (registration.Status == TournamentRegistrationStatus.CheckedIn)
        {
            return Result.Failure("You have already checked in.");
        }

        if (registration.Status == TournamentRegistrationStatus.PendingPayment)
        {
            return Result.Failure("Your tournament entry payment must be completed before check-in.");
        }

        if (registration.Status != TournamentRegistrationStatus.Registered)
        {
            return Result.Failure("This registration is not eligible for check-in.");
        }

        var now = DateTime.UtcNow;

        var checkInOpens = registration.Tournament.StartDate.AddMinutes(-CheckInWindowMinutes);

        if (now < checkInOpens)
        {
            return Result.Failure($"Check-in opens {CheckInWindowMinutes} minutes before the tournament.");
        }

        if (now >= registration.Tournament.StartDate)
        {
            return Result.Failure(
                "Check-in has closed because the tournament has started.");
        }

        registration.Status =
            TournamentRegistrationStatus.CheckedIn;

        registration.CheckedInAt =
            now;

        await _context.SaveChangesAsync(
            cancellationToken);

        return Result.Success(
            "You have successfully checked in.");
    }


    public async Task<Result<MyTournamentDetailsDto>>GetPlayerTournamentDetailsAsync(Guid tournamentId, string userId, CancellationToken cancellationToken = default)
    {
        if (tournamentId == Guid.Empty)
        {
            return Result<MyTournamentDetailsDto>
                .Failure("Tournament is required.");
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<MyTournamentDetailsDto>
                .Failure("User account could not be identified.");
        }

        // First establish that the authenticated player
        // actually has a registration for this tournament.
        var registration =
            await _context.TournamentRegistrations
                .AsNoTracking()
                .Where(x =>
                    x.TournamentId == tournamentId &&
                    x.Player.UserId == userId)
                .Select(x => new
                {
                    x.Id,
                    x.Status,
                    x.CheckedInAt
                })
                .FirstOrDefaultAsync(cancellationToken);

        if (registration is null)
        {
            return Result<MyTournamentDetailsDto>
                .Failure(
                    "You are not registered for this tournament.");
        }

        // Keep this sequential because the scoped DbContext
        // must not run concurrent EF operations.
        var tournament =
            await _context.Tournaments
                .AsNoTracking()
                .Where(x => x.Id == tournamentId)
                .Select(x => new MyTournamentDetailsDto
                {
                    RegistrationId = registration.Id,

                    TournamentId = x.Id,

                    TournamentName = x.Name,

                    TournamentSlug = x.Slug,

                    Description = x.Description,

                    GameName = x.Game.Name,

                    ImageUrl = x.ImageUrl,

                    Format = x.Format,

                    TournamentStatus = x.Status,

                    RegistrationStatus = registration.Status,

                    StartDate = x.StartDate,

                    EndDate = x.EndDate,

                    RegistrationDeadline = x.RegistrationDeadline,

                    CheckedInAt = registration.CheckedInAt,

                    EntryFee = x.EntryFee,

                    MaxPlayers = x.MaxPlayers,

                    RegisteredPlayers = x.Registrations.Count(r =>
                            r.Status == TournamentRegistrationStatus.Registered ||
                            r.Status == TournamentRegistrationStatus.CheckedIn),

                    CheckedInPlayers = x.Registrations.Count(r => r.Status == TournamentRegistrationStatus.CheckedIn),

                    Legs = x.Legs,

                    PointsForWin = x.PointsForWin,

                    PointsForDraw = x.PointsForDraw,

                    PointsForLoss = x.PointsForLoss,

                    PrizePool = _context.PrizePools
                            .Where(p =>
                                p.TournamentId == x.Id)
                            .Select(p =>
                                (decimal?)p.TotalPrize)
                            .FirstOrDefault() ?? 0,

                    Currency = _context.PrizePools
                            .Where(p =>
                                p.TournamentId == x.Id)
                            .Select(p =>
                                p.Currency)
                            .FirstOrDefault() ?? "NGN"
                })
                .FirstOrDefaultAsync(cancellationToken);

        if (tournament is null)
        {
            return Result<MyTournamentDetailsDto>.Failure("Tournament was not found.");
        }

        var participants = await _context.TournamentRegistrations.AsNoTracking()
                .Where(x =>
                    x.TournamentId == tournamentId &&
                    (
                        x.Status == TournamentRegistrationStatus.Registered ||
                        x.Status == TournamentRegistrationStatus.CheckedIn
                    ))
                .OrderByDescending(x => x.Player.RankingPoints)
                .ThenBy(x => x.Player.GamerTag)
                .Select(x => new TournamentParticipantDto
                {
                    PlayerId = x.PlayerId,

                    GamerTag = x.Player.GamerTag,

                    AvatarUrl = x.Player.User.AvatarUrl,

                    RankingPoints = x.Player.RankingPoints,

                    Status = x.Status,

                    CheckedInAt = x.CheckedInAt
                })
                .ToListAsync(cancellationToken);

        tournament.Participants = participants;

        return Result<MyTournamentDetailsDto>.Success(tournament);
    }



    public async Task<Result<AdminTournamentRegisteredParticipantsDto>>GetAdminTournamentParticipantsAsync(
        Guid tournamentId,
        CancellationToken cancellationToken = default)
    {
        if (tournamentId == Guid.Empty)
        {
            return Result<AdminTournamentRegisteredParticipantsDto>.Failure("Tournament is required.");
        }

        var tournament = await _context.Tournaments.AsNoTracking().Where(x => x.Id == tournamentId)
                .Select(x => new AdminTournamentReadinessDto
                {
                    TournamentId = x.Id,

                    TournamentName = x.Name,

                    GameName = x.Game.Name,

                    Format = x.Format,

                    TournamentStatus = x.Status,

                    StartDate = x.StartDate,

                    EndDate = x.EndDate,

                    MaxPlayers = x.MaxPlayers,

                    TotalRegistrations = x.Registrations.Count(),

                    PendingPayment = x.Registrations.Count(r => r.Status == TournamentRegistrationStatus.PendingPayment),

                    Registered = x.Registrations.Count(r => r.Status == TournamentRegistrationStatus.Registered),

                    CheckedIn = x.Registrations.Count(r => r.Status == TournamentRegistrationStatus.CheckedIn),

                    Withdrawn = x.Registrations.Count(r => r.Status == TournamentRegistrationStatus.Withdrawn),

                    Disqualified = x.Registrations.Count(r => r.Status == TournamentRegistrationStatus.Disqualified),

                    Cancelled = x.Registrations.Count(r => r.Status == TournamentRegistrationStatus.Cancelled)
                })
                .FirstOrDefaultAsync(cancellationToken);

        if (tournament is null)
        {
            return Result<AdminTournamentRegisteredParticipantsDto>.Failure("Tournament was not found.");
        }

        // Keep this query sequential.
        // Do not Task.WhenAll() with the tournament query
        // because this service uses a scoped DbContext.
        var participants =
            await _context.TournamentRegistrations.AsNoTracking().Where(x => x.TournamentId == tournamentId)
                .OrderBy(x => x.Status)
                .ThenByDescending(x =>
                    x.Player.RankingPoints)
                .ThenBy(x =>
                    x.Player.GamerTag)
                .Select(x =>
                    new AdminTournamentRegisteredParticipantDto
                    {
                        RegistrationId = x.Id,

                        PlayerId = x.PlayerId,

                        GamerTag = x.Player.GamerTag,

                        Email = x.Player.User.Email ?? string.Empty,

                        FullName = ( x.Player.User.FirstName + " " + x.Player.User.LastName).Trim(),

                        AvatarUrl = x.Player.User.AvatarUrl,

                        RankingPoints = x.Player.RankingPoints,

                        IsPlayerActive = x.Player.IsActive,

                        Status = x.Status,

                        RegisteredAt = x.CreatedDate,

                        CheckedInAt = x.CheckedInAt,

                        DisqualifiedAt = x.DisqualifiedAt,

                        DisqualificationReason = x.DisqualificationReason
                    })
                .ToListAsync(cancellationToken);

        return Result<AdminTournamentRegisteredParticipantsDto>.Success(new AdminTournamentRegisteredParticipantsDto
                {
                    Tournament = tournament,
                    Participants = participants
                });
    }

    // =====================================================
    // HELPERS
    // =====================================================

    private async Task<RegistrationEntity?>FindRegistrationAsync( Guid id, CancellationToken cancellationToken)
    {
        return await _context.TournamentRegistrations
            .Include(x => x.Player)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }


    private static bool IsTerminal(TournamentRegistrationStatus status)
    {
        return status is
            TournamentRegistrationStatus.Withdrawn or
            TournamentRegistrationStatus.Disqualified or
            TournamentRegistrationStatus.Cancelled;
    }

    private async Task<string> GeneratePaymentReferenceAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var reference = $"AM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}".ToUpperInvariant();

            reference = reference[..Math.Min(reference.Length,35)];

            var exists = await _context.Payments.AnyAsync(x => x.TransactionReference == reference, cancellationToken);

            if (!exists)
            return reference;
        }
    }
}