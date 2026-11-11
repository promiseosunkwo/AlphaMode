using System.Text.RegularExpressions;
using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.Tournaments.DTOs;
using AlphaMode.Application.Tournaments.Interfaces;
using AlphaMode.Domain.Entities;
using AlphaMode.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Application.Tournaments.Services;

public class TournamentService : ITournamentService
{
    private readonly IApplicationDbContext _context;

    public TournamentService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<TournamentDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tournaments = await _context.Tournaments
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedDate)
            .Select(t => new TournamentDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                GameId = t.GameId,
                GameName = t.Game.Name,
                Format = t.Format,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                Legs = t.Legs,
                PointsForWin = t.PointsForWin,
                PointsForDraw = t.PointsForDraw,
                PointsForLoss = t.PointsForLoss,
                RegistrationOpenDate = t.RegistrationOpenDate,
                RegistrationDeadline = t.RegistrationDeadline,
                EntryFee = t.EntryFee,
                MaxPlayers = t.MaxPlayers,
                RegisteredPlayers = t.Registrations.Count(r =>
                    r.Status != TournamentRegistrationStatus.Cancelled &&
                    r.Status != TournamentRegistrationStatus.Withdrawn &&
                    r.Status != TournamentRegistrationStatus.Disqualified),
                Slug = t.Slug,
                ImageUrl = t.ImageUrl,
                Status = t.Status
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<TournamentDto>>
            .Success(tournaments);
    }

    public async Task<Result<TournamentDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tournament = await _context.Tournaments
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TournamentDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                GameId = t.GameId,
                GameName = t.Game.Name,
                Format = t.Format,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                Legs = t.Legs,
                PointsForWin = t.PointsForWin,
                PointsForDraw = t.PointsForDraw,
                PointsForLoss = t.PointsForLoss,
                RegistrationOpenDate = t.RegistrationOpenDate,
                RegistrationDeadline = t.RegistrationDeadline,
                EntryFee = t.EntryFee,
                MaxPlayers = t.MaxPlayers,
                RegisteredPlayers = t.Registrations.Count(r =>
                    r.Status != TournamentRegistrationStatus.Cancelled &&
                    r.Status != TournamentRegistrationStatus.Withdrawn &&
                    r.Status != TournamentRegistrationStatus.Disqualified),
                Slug = t.Slug,
                ImageUrl = t.ImageUrl,
                Status = t.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (tournament is null)
        {
            return Result<TournamentDto>.Failure(
                "The tournament could not be found.");
        }

        return Result<TournamentDto>.Success(tournament);
    }

    public async Task<Result<TournamentDto>> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return Result<TournamentDto>.Failure(
                "The tournament could not be found.");
        }

        var tournament = await _context.Tournaments
            .AsNoTracking()
            .Where(t => t.Slug == slug)
            .Select(t => new TournamentDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                GameId = t.GameId,
                GameName = t.Game.Name,
                Format = t.Format,
                StartDate = t.StartDate,
                EndDate = t.EndDate,
                Legs = t.Legs,
                PointsForWin = t.PointsForWin,
                PointsForDraw = t.PointsForDraw,
                PointsForLoss = t.PointsForLoss,
                RegistrationOpenDate = t.RegistrationOpenDate,
                RegistrationDeadline = t.RegistrationDeadline,
                EntryFee = t.EntryFee,
                MaxPlayers = t.MaxPlayers,
                RegisteredPlayers = t.Registrations.Count(r =>
                    r.Status != TournamentRegistrationStatus.Cancelled &&
                    r.Status != TournamentRegistrationStatus.Withdrawn &&
                    r.Status != TournamentRegistrationStatus.Disqualified),
                Slug = t.Slug,
                ImageUrl = t.ImageUrl,
                Status = t.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (tournament is null)
        {
            return Result<TournamentDto>.Failure(
                "The tournament could not be found.");
        }

        return Result<TournamentDto>.Success(tournament);
    }

    public async Task<Result<Guid>> CreateAsync(CreateTournamentRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<Guid>.Failure("Tournament name is required.");
        }

        var gameExists = await _context.Games.AnyAsync(
                game => game.Id == request.GameId && game.IsActive,
                cancellationToken);

        if (!gameExists)
        {
            return Result<Guid>.Failure("The selected game does not exist or is currently inactive.");
        }

        var dateValidation = TournamentDateValidator.Validate(
            request.RegistrationOpenDate,
            request.RegistrationDeadline,
            request.StartDate,
            request.EndDate);

        if (!dateValidation.Succeeded)
        {
            return Result<Guid>.Failure(dateValidation.Error!);
        }

        if (request.EntryFee < 0)
        {
            return Result<Guid>.Failure(
                "The tournament entry fee cannot be negative.");
        }

        if (request.MaxPlayers < 2)
        {
            return Result<Guid>.Failure(
                "A tournament must allow at least two players.");
        }

        var slug = await GenerateUniqueSlugAsync(
            request.Name,
            cancellationToken);

        var tournament = new Tournament
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            GameId = request.GameId,
            Format = request.Format,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            RegistrationOpenDate = request.RegistrationOpenDate,
            RegistrationDeadline = request.RegistrationDeadline,
            EntryFee = request.EntryFee,
            MaxPlayers = request.MaxPlayers,
            ImageUrl = request.ImageUrl,
            Legs = request.Legs,
            PointsForWin = request.PointsForWin,
            PointsForDraw = request.PointsForDraw,
            PointsForLoss = request.PointsForLoss,
            Slug = slug,
            Status = TournamentStatus.Draft
        };

        _context.Tournaments.Add(tournament);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(
            tournament.Id,
            "Tournament created successfully.");
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var slug = name
            .Trim()
            .ToLowerInvariant();

        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        if (string.IsNullOrWhiteSpace(slug))
            slug = "tournament";

        var originalSlug = slug;
        var counter = 2;

        while (await _context.Tournaments
                   .AnyAsync(
                       t => t.Slug == slug,
                       cancellationToken))
        {
            slug = $"{originalSlug}-{counter}";
            counter++;
        }

        return slug;
    }


    public async Task<Result> UpdateAsync(UpdateTournamentRequest request, CancellationToken cancellationToken = default)
    {
        var tournament = await _context.Tournaments.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);


        if (tournament is null)
        {
            return Result.Failure(
                "The tournament could not be found.");
        }


        var gameExists =
            await _context.Games
                .AnyAsync(
                    x =>
                        x.Id == request.GameId
                        && x.IsActive,
                    cancellationToken);


        if (!gameExists)
        {
            return Result.Failure(
                "The selected game is not available.");
        }


        var dateValidation = TournamentDateValidator.Validate(
             request.RegistrationOpenDate,
             request.RegistrationDeadline,
             request.TournamentStartDate,
             request.TournamentEndDate);

        if (!dateValidation.Succeeded)
        {
            return Result.Failure(dateValidation.Error!);
        }

        tournament.Name = request.Name.Trim();

        tournament.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        tournament.GameId = request.GameId;

        tournament.Format = request.Format;

        tournament.StartDate = request.TournamentStartDate;

        tournament.EndDate = request.TournamentEndDate;

        tournament.RegistrationOpenDate = request.RegistrationOpenDate;

        tournament.RegistrationDeadline = request.RegistrationDeadline;

        tournament.MaxPlayers = request.MaxPlayers;

        tournament.EntryFee = request.EntryFee;

        tournament.Legs = request.Legs;

        tournament.PointsForWin = request.PointsForWin;

        tournament.PointsForDraw = request.PointsForDraw;

        tournament.PointsForLoss = request.PointsForLoss;

        tournament.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();

        //tournament.Status = request.Status;


        await _context.SaveChangesAsync(
            cancellationToken);


        return Result.Success(
            "Tournament updated successfully.");
    }



    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tournament = await _context.Tournaments
            .FirstOrDefaultAsync(
                t => t.Id == id,
                cancellationToken);

        if (tournament is null)
        {
            return Result.Failure(
                "Tournament not found.");
        }

        var hasRegistrations = await _context.TournamentRegistrations
            .AnyAsync(
                r => r.TournamentId == id,
                cancellationToken);

        if (hasRegistrations)
        {
            return Result.Failure(
                "This tournament cannot be deleted because players have already registered. Cancel the tournament instead.");
        }

        var hasMatches = await _context.Matches
            .AnyAsync(
                m => m.TournamentId == id,
                cancellationToken);

        if (hasMatches)
        {
            return Result.Failure(
                "This tournament cannot be deleted because matches have already been created.");
        }

        _context.Tournaments.Remove(tournament);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(
            "Tournament deleted successfully.");
    }
}