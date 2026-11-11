using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.PlayerTournamentRegistrations.DTOs;
using AlphaMode.Application.PlayerTournamentRegistrations.Interfaces;
using AlphaMode.Domain.Entities;
using AlphaMode.Domain.Enums;
using Microsoft.EntityFrameworkCore;
//using AlphaMode.Domain.Entities;

using RegistrationEntity = AlphaMode.Domain.Entities.TournamentRegistration; //Name collition action

namespace AlphaMode.Application.PlayerTournamentRegistrations.Services;

public class TournamentPlayerRegistrationService : ITournamentPlayerRegistrationService
{
    private readonly IApplicationDbContext _context;

    private readonly ITournamentRegistrationLock _registrationLock;

    public TournamentPlayerRegistrationService(IApplicationDbContext context, ITournamentRegistrationLock registrationLock)
    {
        _context = context;
        _registrationLock = registrationLock;
    }



    public async Task<Result<TournamentPlayerRegistrationDTO>> RegisterAsync(RegisterPlayerForTournamentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.TournamentId == Guid.Empty)
        {
            return Result<TournamentPlayerRegistrationDTO>.Failure(
                "Tournament is required.");
        }

        if (request.PlayerId == Guid.Empty)
        {
            return Result<TournamentPlayerRegistrationDTO>.Failure(
                "Player is required.");
        }

        return await _registrationLock.ExecuteAsync(request.TournamentId, ct => RegisterInternalAsync(request, ct), cancellationToken);
    }




    //bears the main registration logic but due to the implementation of registration lock to prevent race condition, it was converted to a utility method
    private async Task<Result<TournamentPlayerRegistrationDTO>> RegisterInternalAsync(RegisterPlayerForTournamentRequest request, CancellationToken cancellationToken = default)
    {

        var tournament = await _context.Tournaments.FirstOrDefaultAsync(t => t.Id == request.TournamentId, cancellationToken);

        if (tournament is null)
        {
            return Result<TournamentPlayerRegistrationDTO>.Failure("The tournament could not be found.");
        }

        // -------------------------------------------------------
        // Lifecycle
        // -------------------------------------------------------

        if (tournament.Status != TournamentStatus.RegistrationOpen)
        {
            return Result<TournamentPlayerRegistrationDTO>.Failure(
                "Registration is not currently open for this tournament.");
        }

        var now = DateTime.UtcNow;

        // -------------------------------------------------------
        // Registration window
        // -------------------------------------------------------

        if (tournament.RegistrationOpenDate.HasValue &&
            now < tournament.RegistrationOpenDate.Value)
        {
            return Result<TournamentPlayerRegistrationDTO>.Failure(
                "Registration for this tournament has not opened yet.");
        }

        if (tournament.RegistrationDeadline.HasValue &&
            now > tournament.RegistrationDeadline.Value)
        {
            return Result<TournamentPlayerRegistrationDTO>.Failure(
                "Registration for this tournament has closed.");
        }

        if (now >= tournament.StartDate)
        {
            return Result<TournamentPlayerRegistrationDTO>.Failure(
                "This tournament has already started.");
        }

        // -------------------------------------------------------
        // Player eligibility
        // -------------------------------------------------------

        var player = await _context.Players
            .FirstOrDefaultAsync(
                p => p.Id == request.PlayerId,
                cancellationToken);

        if (player is null)
        {
            return Result<TournamentPlayerRegistrationDTO>.Failure(
                "Your player profile could not be found.");
        }

        if (!player.IsActive)
        {
            return Result<TournamentPlayerRegistrationDTO>.Failure(
                "Your player profile is not currently eligible to register for tournaments.");
        }

        // -------------------------------------------------------
        // Existing registration
        // -------------------------------------------------------

        var existingRegistration = await _context.TournamentRegistrations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.TournamentId == tournament.Id &&
                     r.PlayerId == player.Id,
                cancellationToken);

        if (existingRegistration is not null)
        {
            return Result<TournamentPlayerRegistrationDTO>.Failure(
                GetExistingRegistrationMessage(existingRegistration.Status));
        }

        // -------------------------------------------------------
        // Capacity
        // -------------------------------------------------------

        var activeRegistrationCount = await _context.TournamentRegistrations.AsNoTracking().CountAsync(
                        r => r.TournamentId == tournament.Id &&
                        r.Status != TournamentRegistrationStatus.Cancelled &&
                        r.Status != TournamentRegistrationStatus.Withdrawn &&
                        r.Status != TournamentRegistrationStatus.Disqualified,
                    cancellationToken);

        if (activeRegistrationCount >= tournament.MaxPlayers)
        {
            return Result<TournamentPlayerRegistrationDTO>.Failure(
                "This tournament is full. No additional places are available.");
        }

        // -------------------------------------------------------
        // Payment
        // -------------------------------------------------------
        // Payment processing has deliberately not been activated
        // yet. Do not create an unusable PendingPayment record.

        if (tournament.EntryFee > 0)
        {
            return Result<TournamentPlayerRegistrationDTO>.Failure(
                "Registration for paid tournaments is not available yet.");
        }

        // -------------------------------------------------------
        // Create registration
        // -------------------------------------------------------

        var registration = new RegistrationEntity
        {
            TournamentId = tournament.Id,
            PlayerId = player.Id,
            Status = TournamentRegistrationStatus.Registered
        };

        _context.TournamentRegistrations.Add(registration);

        await _context.SaveChangesAsync(cancellationToken);


        //try
        //{
        //    await _context.SaveChangesAsync(cancellationToken);
        //}
        //catch (DbUpdateException)
        //{
        //    //_context.TournamentRegistrations.Remove(registration);

        //    return Result<TournamentPlayerRegistrationDTO>.Failure(
        //        "The registration could not be completed. Please try again.");
        //}

        var dto = MapRegistration(
            registration,
            tournament.Name,
            player.GamerTag);

        return Result<TournamentPlayerRegistrationDTO>.Success(
            dto,
            "You have successfully registered for the tournament.");
    }





    public async Task<Result<TournamentPlayerRegistrationDTO>> GetByIdAsync(
        Guid registrationId,
        CancellationToken cancellationToken = default)
    {
        var registration = await _context.TournamentRegistrations
            .AsNoTracking()
            .Where(r => r.Id == registrationId)
            .Select(r => new TournamentPlayerRegistrationDTO
            {
                Id = r.Id,
                TournamentId = r.TournamentId,
                TournamentName = r.Tournament.Name,
                PlayerId = r.PlayerId,
                GamerTag = r.Player.GamerTag,
                Status = r.Status,
                RegisteredAt = r.CreatedDate,
                CheckedInAt = r.CheckedInAt,
                DisqualifiedAt = r.DisqualifiedAt,
                DisqualificationReason =
                    r.DisqualificationReason
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (registration is null)
        {
            return Result<TournamentPlayerRegistrationDTO>.Failure(
                "The tournament registration could not be found.");
        }

        return Result<TournamentPlayerRegistrationDTO>
            .Success(registration);
    }

    public async Task<Result<IReadOnlyList<TournamentPlayerRegistrationDTO>>>GetTournamentRegistrationsAsync(
            Guid tournamentId,
            CancellationToken cancellationToken = default)
    {
        var tournamentExists = await _context.Tournaments.AnyAsync(t => t.Id == tournamentId, cancellationToken);

        if (!tournamentExists)
        {
            return Result<IReadOnlyList<TournamentPlayerRegistrationDTO>>
                .Failure(
                    "The tournament could not be found.");
        }

        var registrations = await _context.TournamentRegistrations
                .AsNoTracking()
                .Where(r => r.TournamentId == tournamentId)
                .OrderByDescending(r => r.CreatedDate)
                .Select(r => new TournamentPlayerRegistrationDTO
                {
                    Id = r.Id,
                    TournamentId = r.TournamentId,
                    TournamentName = r.Tournament.Name,
                    PlayerId = r.PlayerId,
                    GamerTag = r.Player.GamerTag,
                    Status = r.Status,
                    RegisteredAt = r.CreatedDate,
                    CheckedInAt = r.CheckedInAt,
                    DisqualifiedAt = r.DisqualifiedAt,
                    DisqualificationReason =
                        r.DisqualificationReason
                })
                .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<TournamentPlayerRegistrationDTO>>
            .Success(registrations);
    }

    public async Task<Result> CheckInAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        var registration = await _context.TournamentRegistrations.Include(r => r.Tournament)
            .FirstOrDefaultAsync(r => r.Id == registrationId, cancellationToken);

        if (registration is null)
        {
            return Result.Failure(
                "The tournament registration could not be found.");
        }

        if (registration.Status == TournamentRegistrationStatus.PendingPayment)
        {
            return Result.Failure(
                "Payment must be completed before you can check in.");
        }

        if (registration.Status == TournamentRegistrationStatus.CheckedIn)
        {
            return Result.Failure(
                "You have already checked in for this tournament.");
        }

        if (registration.Status != TournamentRegistrationStatus.Registered)
        {
            return Result.Failure(
                "This registration is not eligible for check-in.");
        }

        if (registration.Tournament.Status == TournamentStatus.Draft)
        {
            return Result.Failure(
                "This tournament has not been published.");
        }

        if (registration.Tournament.Status == TournamentStatus.RegistrationOpen)
        {
            return Result.Failure(
                "Check-in is not available while registration is still open.");
        }

        if (registration.Tournament.Status == TournamentStatus.Live)
        {
            return Result.Failure(
                "Check-in is closed because the tournament has already started.");
        }

        if (registration.Tournament.Status == TournamentStatus.Completed)
        {
            return Result.Failure(
                "This tournament has already been completed.");
        }

        if (registration.Tournament.Status == TournamentStatus.Cancelled)
        {
            return Result.Failure("This tournament has been cancelled.");
        }

        /*
         * At this point the expected lifecycle state is Upcoming:
         *
         * RegistrationOpen
         *      ↓
         * Upcoming (registration closed)
         *      ↓
         * CHECK-IN
         *      ↓
         * Live
         */

        var now = DateTime.UtcNow;

        if (registration.Tournament.Status != TournamentStatus.Upcoming)
        {
            return Result.Failure("Check-in is not currently available for this tournament.");
        }

        if (registration.Tournament.RegistrationDeadline.HasValue && now < registration.Tournament.RegistrationDeadline.Value)
        {
            return Result.Failure("Check-in will become available after registration closes.");
        }


        var checkInOpens = registration.Tournament.StartDate.AddMinutes(-30);

        if (now < checkInOpens)
        {
            return Result.Failure($"Check-in opens {checkInOpens.ToLocalTime():dd MMM 'at' HH:mm}.");
        }

        if (now >= registration.Tournament.StartDate)
        {
            return Result.Failure("Check-in is closed because the tournament start time has passed.");
        }

        registration.Status = TournamentRegistrationStatus.CheckedIn;

        registration.CheckedInAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(
            "Check-in completed successfully. You're ready to compete.");
    }




    public async Task<Result> WithdrawAsync(Guid registrationId, CancellationToken cancellationToken = default)
    {
        var registration = await _context.TournamentRegistrations
            .Include(r => r.Tournament)
            .FirstOrDefaultAsync(r => r.Id == registrationId, cancellationToken);

        if (registration is null)
        {
            return Result.Failure("The tournament registration could not be found.");
        }

        if (registration.Status is not (TournamentRegistrationStatus.Registered or TournamentRegistrationStatus.CheckedIn))
        {
            return Result.Failure("This registration is not eligible for withdrawal.");
        }

        var tournament = registration.Tournament;
        var now = DateTime.UtcNow;

        if (tournament.Status is
            TournamentStatus.Live or
            TournamentStatus.Completed or
            TournamentStatus.Cancelled)
        {
            return Result.Failure("Withdrawal is no longer available for this tournament.");
        }

        if (now >= tournament.StartDate)
        {
            return Result.Failure("Withdrawal is closed because the tournament start time has passed.");
        }

        var fixturesExist = await _context.Matches
            .AsNoTracking()
            .AnyAsync(
                m => m.TournamentId == tournament.Id,
                cancellationToken);

        if (fixturesExist)
        {
            return Result.Failure("Withdrawal is closed because tournament fixtures have already been generated. Contact the organiser.");
        }

        registration.Status = TournamentRegistrationStatus.Withdrawn;
        registration.CheckedInAt = null;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("You have successfully withdrawn from the tournament.");
    }




    public async Task<Result> PlayerWithdrawAsync(Guid registrationId, string userId, CancellationToken cancellationToken = default)
    {
        if (registrationId == Guid.Empty ||
            string.IsNullOrWhiteSpace(userId))
        {
            return Result.Failure("Invalid withdrawal request.");
        }

        var ownsRegistration = await _context.TournamentRegistrations
            .AsNoTracking()
            .AnyAsync(
                r => r.Id == registrationId &&
                     r.Player.UserId == userId,
                cancellationToken);

        if (!ownsRegistration)
        {
            return Result.Failure(
                "The tournament registration could not be found.");
        }

        return await WithdrawAsync(registrationId, cancellationToken);
    }



    public async Task<Result> DisqualifyAsync(Guid registrationId, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(
                "A reason must be provided before disqualifying a player.");
        }

        var registration = await _context.TournamentRegistrations
            .Include(r => r.Tournament)
            .FirstOrDefaultAsync(
                r => r.Id == registrationId,
                cancellationToken);

        if (registration is null)
        {
            return Result.Failure(
                "The tournament registration could not be found.");
        }

        if (registration.Status == TournamentRegistrationStatus.Disqualified)
        {
            return Result.Failure(
                "This player has already been disqualified.");
        }

        if (registration.Status == TournamentRegistrationStatus.Cancelled ||
            registration.Status == TournamentRegistrationStatus.Withdrawn)
        {
            return Result.Failure(
                "This registration is no longer active.");
        }

        if (registration.Tournament.Status == TournamentStatus.Completed)
        {
            return Result.Failure(
                "Players cannot be disqualified after the tournament has been completed.");
        }

        if (registration.Tournament.Status == TournamentStatus.Cancelled)
        {
            return Result.Failure(
                "Players cannot be disqualified from a cancelled tournament.");
        }

        registration.Status =
            TournamentRegistrationStatus.Disqualified;

        registration.DisqualifiedAt =
            DateTime.UtcNow;

        registration.DisqualificationReason =
            reason.Trim();

        registration.CheckedInAt = null;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(
            "The player has been disqualified from the tournament.");
    }



    public async Task<Result<TournamentRegistrationAvailabilityDto>>GetAvailabilityAsync(Guid tournamentId, Guid playerId, CancellationToken cancellationToken = default)
    {
        var tournament = await _context.Tournaments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                t => t.Id == tournamentId,
                cancellationToken);

        if (tournament is null)
        {
            return Result<TournamentRegistrationAvailabilityDto>.Failure("The tournament could not be found.");
        }


        var player = await _context.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Id == playerId,
                cancellationToken);

        if (player is null)
        {
            return Result<TournamentRegistrationAvailabilityDto>.Failure(
                "Your player profile could not be found.");
        }


        var registration = await _context.TournamentRegistrations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                r => r.TournamentId == tournamentId &&
                     r.PlayerId == playerId,
                cancellationToken);


        var registeredPlayers =
            await _context.TournamentRegistrations
                .AsNoTracking()
                .CountAsync(
                    r =>
                        r.TournamentId == tournamentId &&
                        r.Status != TournamentRegistrationStatus.Cancelled &&
                        r.Status != TournamentRegistrationStatus.Withdrawn &&
                        r.Status != TournamentRegistrationStatus.Disqualified,
                    cancellationToken);


        var fixturesExist = await _context.Matches.AsNoTracking().AnyAsync(m => m.TournamentId == tournamentId, cancellationToken);


        var availability = new TournamentRegistrationAvailabilityDto
        {
            TournamentId = tournament.Id,
            PlayerId = player.Id,

            RegistrationId = registration?.Id,

            TournamentStatus = tournament.Status,

            RegistrationStatus = registration?.Status,

            RegisteredPlayers = registeredPlayers,

            MaxPlayers = tournament.MaxPlayers,

            IsFull =
                registeredPlayers >= tournament.MaxPlayers
        };


        ApplyRegistrationAvailability(
            availability,
            tournament,
            player,
            registration,
            fixturesExist);


        return Result<TournamentRegistrationAvailabilityDto>.Success(
            availability);
    }


    public async Task<Result<TournamentRegistrationAvailabilityDto>>GetAvailabilityByRegistrationAsync(Guid registrationId, string userId, CancellationToken cancellationToken = default)
    {
        if (registrationId == Guid.Empty || string.IsNullOrWhiteSpace(userId))
        {
            return Result<TournamentRegistrationAvailabilityDto>.Failure(
                "Invalid registration request.");
        }

        var registration = await _context.TournamentRegistrations
            .AsNoTracking()
            .Where(r =>
                r.Id == registrationId &&
                r.Player.UserId == userId)
            .Select(r => new
            {
                r.TournamentId,
                r.PlayerId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (registration is null)
        {
            return Result<TournamentRegistrationAvailabilityDto>.Failure(
                "The tournament registration could not be found.");
        }

        return await GetAvailabilityAsync(
            registration.TournamentId,
            registration.PlayerId,
            cancellationToken);
    }



    public async Task<Result> PlayerCheckInAsync(Guid registrationId, string userId, CancellationToken cancellationToken = default)
    {
        if (registrationId == Guid.Empty ||
            string.IsNullOrWhiteSpace(userId))
        {
            return Result.Failure("Invalid check-in request.");
        }

        var ownsRegistration = await _context.TournamentRegistrations
            .AsNoTracking()
            .AnyAsync(
                r => r.Id == registrationId &&
                     r.Player.UserId == userId,
                cancellationToken);

        if (!ownsRegistration)
        {
            return Result.Failure(
                "The tournament registration could not be found.");
        }

        return await CheckInAsync(registrationId, cancellationToken);
    }



    //private async Task<RegistrationEntity?> GetRegistrationEntityAsync(Guid registrationId, CancellationToken cancellationToken)
    //{
    //    return await _context.TournamentRegistrations.FirstOrDefaultAsync(r => r.Id == registrationId, cancellationToken);
    //}



    private static TournamentPlayerRegistrationDTO MapRegistration(RegistrationEntity registration, string tournamentName, string gamerTag)
    {
        return new TournamentPlayerRegistrationDTO
        {
            Id = registration.Id,
            TournamentId = registration.TournamentId,
            TournamentName = tournamentName,
            PlayerId = registration.PlayerId,
            GamerTag = gamerTag,
            Status = registration.Status,
            RegisteredAt = registration.CreatedDate,
            CheckedInAt = registration.CheckedInAt,
            DisqualifiedAt = registration.DisqualifiedAt,
            DisqualificationReason =
                registration.DisqualificationReason
        };
    }


    private static string GetExistingRegistrationMessage(TournamentRegistrationStatus status)
    {
        return status switch
        {
            TournamentRegistrationStatus.PendingPayment =>
                "You already have a registration awaiting payment.",

            TournamentRegistrationStatus.Registered =>
                "You are already registered for this tournament.",

            TournamentRegistrationStatus.CheckedIn =>
                "You are already registered and checked in for this tournament.",

            TournamentRegistrationStatus.Withdrawn =>
                "You previously withdrew from this tournament.",

            TournamentRegistrationStatus.Disqualified =>
                "You were disqualified from this tournament and cannot register again.",

            TournamentRegistrationStatus.Cancelled =>
                "Your previous registration for this tournament was cancelled.",

            _ =>
                "A registration already exists for this tournament."
        };
    }


    private static void ApplyRegistrationAvailability(TournamentRegistrationAvailabilityDto availability, Tournament tournament, Player player, RegistrationEntity? registration,
        bool fixturesExist)
    {
        availability.CanRegister = false;
        availability.CanCheckIn = false;
        availability.CanWithdraw = false;

        var now = DateTime.UtcNow;


        // -------------------------------------------------------
        // Existing registration
        // -------------------------------------------------------

        if (registration is not null)
        {
            switch (registration.Status)
            {
                case TournamentRegistrationStatus.PendingPayment:

                    availability.Message =
                        "Your registration is awaiting payment.";

                    return;


                case TournamentRegistrationStatus.Registered:

                    ApplyRegisteredPlayerAvailability(
                        availability,
                        tournament,
                        now,
                        fixturesExist);

                    return;


                case TournamentRegistrationStatus.CheckedIn:

                    availability.CanWithdraw = CanWithdrawFromTournament(tournament, now, fixturesExist);

                    availability.Message = "You are checked in and ready to compete.";

                    return;


                case TournamentRegistrationStatus.Withdrawn:

                    availability.Message = "You previously withdrew from this tournament.";

                    return;


                case TournamentRegistrationStatus.Disqualified:

                    availability.Message = "You have been disqualified from this tournament.";

                    return;


                case TournamentRegistrationStatus.Cancelled:

                    availability.Message = "Your registration for this tournament was cancelled.";

                    return;
            }
        }


        // -------------------------------------------------------
        // Player eligibility
        // -------------------------------------------------------

        if (!player.IsActive)
        {
            availability.Message = "Your player profile is not currently eligible to register.";

            return;
        }


        // -------------------------------------------------------
        // Tournament lifecycle
        // -------------------------------------------------------

        switch (tournament.Status)
        {
            case TournamentStatus.Draft:

                availability.Message =
                    "This tournament has not been published yet.";

                return;


            case TournamentStatus.Upcoming:

                availability.Message =
                    GetUpcomingRegistrationMessage(tournament, now);

                return;


            case TournamentStatus.Live:

                availability.Message =
                    "This tournament is currently in progress.";

                return;


            case TournamentStatus.Completed:

                availability.Message =
                    "This tournament has been completed.";

                return;


            case TournamentStatus.Cancelled:

                availability.Message =
                    "This tournament has been cancelled.";

                return;


            case TournamentStatus.RegistrationOpen:
                break;


            default:

                availability.Message =
                    "Registration is not currently available.";

                return;
        }


        // -------------------------------------------------------
        // Registration dates
        // -------------------------------------------------------

        if (tournament.RegistrationOpenDate.HasValue &&
            now < tournament.RegistrationOpenDate.Value)
        {
            availability.Message =
                "Registration has not opened yet.";

            return;
        }


        if (tournament.RegistrationDeadline.HasValue &&
            now > tournament.RegistrationDeadline.Value)
        {
            availability.Message =
                "Registration has closed.";

            return;
        }


        if (now >= tournament.StartDate)
        {
            availability.Message =
                "This tournament has already started.";

            return;
        }


        // -------------------------------------------------------
        // Capacity
        // -------------------------------------------------------

        if (availability.IsFull)
        {
            availability.Message = "This tournament is full.";
            return;
        }


        // -------------------------------------------------------
        // Player payments
        // -------------------------------------------------------

        if (tournament.EntryFee > 0)
        {
            availability.Message = "Registration for paid tournaments is not available yet.";
            return;
        }


        // -------------------------------------------------------
        // Registration available
        // -------------------------------------------------------

        availability.CanRegister = true;

        availability.Message = "Registration is open.";
    }


    private static void ApplyRegisteredPlayerAvailability(TournamentRegistrationAvailabilityDto availability, Tournament tournament, DateTime now, bool fixturesExist)
    {
        if (tournament.Status == TournamentStatus.Cancelled)
        {
            availability.Message = "This tournament has been cancelled.";
            return;
        }


        if (tournament.Status == TournamentStatus.Completed)
        {
            availability.Message = "This tournament has been completed.";

            return;
        }


        if (tournament.Status == TournamentStatus.Live)
        {
            availability.Message = "The tournament is currently in progress.";

            return;
        }


        if (now >= tournament.StartDate)
        {
            availability.Message = "The tournament start time has passed.";

            return;
        }


        availability.CanWithdraw = CanWithdrawFromTournament(tournament, now, fixturesExist);


        // Registration must have closed before check-in becomes available.
        var checkInOpens = tournament.StartDate.AddMinutes(-30);

        if (tournament.Status == TournamentStatus.Upcoming && now >= checkInOpens && now < tournament.StartDate && (!tournament.RegistrationDeadline.HasValue ||
             now >= tournament.RegistrationDeadline.Value))
        {
            availability.CanCheckIn = true;

            availability.Message = "Registration confirmed. Check-in is now available.";

            return;
        }


        availability.Message = "You are registered for this tournament.";
    }

    private static bool CanWithdrawFromTournament(Tournament tournament, DateTime now, bool fixturesExist)
    {
        if (tournament.Status is TournamentStatus.Live or TournamentStatus.Completed or TournamentStatus.Cancelled)
        {
            return false;
        }

        return now < tournament.StartDate && !fixturesExist;
    }


    private static string GetUpcomingRegistrationMessage(Tournament tournament, DateTime now)
    {
        if (tournament.RegistrationOpenDate.HasValue &&
            now < tournament.RegistrationOpenDate.Value)
        {
            return "Registration has not opened yet.";
        }

        if (tournament.RegistrationDeadline.HasValue &&
            now >= tournament.RegistrationDeadline.Value)
        {
            return "Registration has closed.";
        }

        return "Registration is not currently open.";
    }
}