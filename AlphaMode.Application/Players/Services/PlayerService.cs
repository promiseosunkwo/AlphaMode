using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.Players.DTOs;
using AlphaMode.Application.Players.Interfaces;
using AlphaMode.Application.Players.Requests;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Application.Players.Services;

public class PlayerService : IPlayerService
{
    private const int DefaultPageSize = 25;

    private const int MaximumPageSize = 100;

    private readonly IApplicationDbContext _context;


    public PlayerService(
        IApplicationDbContext context)
    {
        _context = context;
    }


    // =====================================================
    // GET PAGED PLAYERS
    // =====================================================

    public async Task<Result<PagedResult<PlayerDto>>> GetPagedAsync(int page = 1, int pageSize = DefaultPageSize, string? search = null, bool? isActive = null, CancellationToken cancellationToken = default)
    {
        // ---------------------------------------------
        // Normalize pagination
        // ---------------------------------------------

        page = Math.Max(page, 1);

        pageSize = pageSize <= 0
            ? DefaultPageSize
            : Math.Min(
                pageSize,
                MaximumPageSize);

        // ---------------------------------------------
        // Base query
        // ---------------------------------------------

        var query = _context.Players.AsNoTracking().AsQueryable();

        // ---------------------------------------------
        // Status filter
        // ---------------------------------------------

        if (isActive.HasValue)
        {
            query = query.Where(
                player =>
                    player.IsActive ==
                    isActive.Value);
        }


        // ---------------------------------------------
        // Search
        // ---------------------------------------------

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm =
                search.Trim();


            query = query.Where(
                player =>
                    player.GamerTag.Contains(searchTerm)
                    ||
                    player.User.FirstName.Contains(searchTerm)
                    ||
                    player.User.LastName.Contains(searchTerm)
                    ||
                    (player.User.Email != null
                     &&
                     player.User.Email.Contains(searchTerm)));
        }


        // ---------------------------------------------
        // Total count
        // ---------------------------------------------

        var totalCount =
            await query.CountAsync(
                cancellationToken);


        // ---------------------------------------------
        // Protect against requesting a page that no
        // longer exists after filtering/deletion/etc.
        // ---------------------------------------------

        var totalPages =
            totalCount == 0
                ? 0
                : (int)Math.Ceiling(
                    totalCount / (double)pageSize);


        if (totalPages > 0 &&
            page > totalPages)
        {
            page = totalPages;
        }


        // ---------------------------------------------
        // Retrieve requested page
        // ---------------------------------------------

        var players = await query
                .OrderByDescending(player =>
                        player.RankingPoints)
                .ThenBy(
                    player =>
                        player.GamerTag)
                .Skip(
                    (page - 1) * pageSize)
                .Take(
                    pageSize)
                .Select(
                    player =>
                        new PlayerDto
                        {
                            Id =
                                player.Id,

                            UserId =
                                player.UserId,

                            FirstName =
                                player.User.FirstName,

                            LastName =
                                player.User.LastName,

                            Email =
                                player.User.Email
                                ?? string.Empty,

                            AvatarUrl =
                                player.User.AvatarUrl,

                            GamerTag =
                                player.GamerTag,

                            RankingPoints =
                                player.RankingPoints,

                            Wins =
                                player.Wins,

                            Losses =
                                player.Losses,

                            Draws =
                                player.Draws,

                            IsActive =
                                player.IsActive,

                            TournamentCount =
                                player.Registrations.Count(),

                            JoinedDate =
                                player.CreatedDate
                        })
                .ToListAsync(
                    cancellationToken);


        // ---------------------------------------------
        // Build paged result
        // ---------------------------------------------

        var pagedResult =
            PagedResult<PlayerDto>.Create(
                players,
                page,
                pageSize,
                totalCount);


        return Result<PagedResult<PlayerDto>>
            .Success(
                pagedResult);
    }


    // =====================================================
    // GET PLAYER DETAILS
    // =====================================================

    public async Task<Result<PlayerDetailsDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var player =
            await _context.Players
                .AsNoTracking()
                .Where(
                    player =>
                        player.Id == id)
                .Select(
                    player =>
                        new PlayerDetailsDto
                        {
                            Id =
                                player.Id,

                            UserId =
                                player.UserId,


                            // -------------------------
                            // Account
                            // -------------------------

                            FirstName =
                                player.User.FirstName,

                            LastName =
                                player.User.LastName,

                            Email =
                                player.User.Email
                                ?? string.Empty,

                            PhoneNumber =
                                player.User.PhoneNumber,

                            AvatarUrl =
                                player.User.AvatarUrl,

                            EmailConfirmed =
                                player.User.EmailConfirmed,


                            // -------------------------
                            // Player
                            // -------------------------

                            GamerTag = player.GamerTag,

                            RankingPoints = player.RankingPoints,

                            Wins = player.Wins,

                            Losses = player.Losses,

                            Draws = player.Draws,

                            IsActive = player.IsActive,


                            // -------------------------
                            // Activity
                            // -------------------------

                            TournamentCount = player.Registrations.Count(),


                            // -------------------------
                            // Audit
                            // -------------------------

                            JoinedDate = player.CreatedDate,

                            LastModifiedDate = player.ModifiedDate
                        }).FirstOrDefaultAsync(cancellationToken);


        if (player is null)
        {
            return Result<PlayerDetailsDto>.Failure("Player not found.");
        }

        return Result<PlayerDetailsDto>.Success(player);
    }


    // =====================================================
    // UPDATE PLAYER
    // =====================================================

    public async Task<Result> UpdateAsync(
        UpdatePlayerRequest request,
        CancellationToken cancellationToken = default)
    {
        // ---------------------------------------------
        // Validate
        // ---------------------------------------------

        var validation =
            ValidateUpdateRequest(
                request);


        if (!validation.Succeeded)
        {
            return validation;
        }


        // ---------------------------------------------
        // Load Player + Identity user
        // ---------------------------------------------

        var player =
            await _context.Players
                .Include(
                    player =>
                        player.User)
                .FirstOrDefaultAsync(
                    player =>
                        player.Id == request.Id,
                    cancellationToken);


        if (player is null)
        {
            return Result.Failure(
                "Player not found.");
        }


        var firstName = request.FirstName.Trim();


        var lastName = request.LastName.Trim();


        var gamerTag = request.GamerTag.Trim();


        // ---------------------------------------------
        // GamerTag uniqueness
        // ---------------------------------------------

        var gamerTagExists = await _context.Players
                .AsNoTracking()
                .AnyAsync(
                    otherPlayer =>
                        otherPlayer.Id != player.Id
                        &&
                        otherPlayer.GamerTag == gamerTag,
                    cancellationToken);


        if (gamerTagExists)
        {
            return Result.Failure("That gamer tag is already being used by another player.");
        }

        // ---------------------------------------------
        // Update Identity information
        // ---------------------------------------------

        player.User.FirstName = firstName;

        player.User.LastName = lastName;


        /*
         * ApplicationUser currently still contains
         * GamerTag, so keep it synchronized until
         * GamerTag is eventually moved completely
         * into the Player domain.
         */

        player.User.GamerTag = gamerTag;


        // ---------------------------------------------
        // Update Player information
        // ---------------------------------------------

        player.GamerTag =
            gamerTag;


        // ---------------------------------------------
        // Persist
        // ---------------------------------------------

        await _context.SaveChangesAsync(
            cancellationToken);


        return Result.Success(
            "Player updated successfully.");
    }


    // =====================================================
    // SUSPEND PLAYER
    // =====================================================

    public async Task<Result> SuspendAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var player = await _context.Players.FirstOrDefaultAsync(player => player.Id == id, cancellationToken);

        if (player is null)
        {
            return Result.Failure("Player not found.");
        }

        if (!player.IsActive)
        {
            return Result.Success("Player is already suspended.");
        }

        player.IsActive = false;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("Player suspended successfully.");
    }


    // =====================================================
    // ACTIVATE PLAYER
    // =====================================================

    public async Task<Result> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var player = await _context.Players.FirstOrDefaultAsync(player => player.Id == id, cancellationToken);

        if (player is null)
        {
            return Result.Failure("Player not found.");
        }

        if (player.IsActive)
        {
            return Result.Success("Player is already active.");
        }

        player.IsActive = true;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("Player activated successfully.");
    }


    // =====================================================
    // UPDATE VALIDATION
    // =====================================================

    private static Result ValidateUpdateRequest(UpdatePlayerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            return Result.Failure("First name is required.");
        }

        if (request.FirstName.Trim().Length > 100)
        {
            return Result.Failure("First name cannot exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            return Result.Failure("Last name is required.");
        }

        if (request.LastName.Trim().Length > 100)
        {
            return Result.Failure("Last name cannot exceed 100 characters.");
        }


        if (string.IsNullOrWhiteSpace(request.GamerTag))
        {
            return Result.Failure(
                "Gamer tag is required.");
        }


        var gamerTag =
            request.GamerTag.Trim();


        if (gamerTag.Length < 3 ||
            gamerTag.Length > 50)
        {
            return Result.Failure(
                "Gamer tag must be between 3 and 50 characters.");
        }


        if (!IsValidGamerTag(
                gamerTag))
        {
            return Result.Failure(
                "Gamer tag can only contain letters, numbers, underscores and hyphens.");
        }


        return Result.Success();
    }


    // =====================================================
    // GAMER TAG VALIDATION
    // =====================================================

    private static bool IsValidGamerTag(
        string gamerTag)
    {
        foreach (var character in gamerTag)
        {
            if (!char.IsLetterOrDigit(character)
                &&
                character != '_'
                &&
                character != '-')
            {
                return false;
            }
        }


        return true;
    }
}