using System.Text.RegularExpressions;
using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.Games.DTOs;
using AlphaMode.Application.Games.Interfaces;
using AlphaMode.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Application.Games.Services;

public class GameService : IGameService
{
    private readonly IApplicationDbContext _context;

    public GameService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<GameDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var games = await _context.Games
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .Select(g => new GameDto
            {
                Id = g.Id,
                Name = g.Name,
                Slug = g.Slug,
                Version = g.Version,
                Platform = g.Platform,
                ImageUrl = g.ImageUrl,
                IsActive = g.IsActive,
                TournamentCount = g.Tournaments.Count,
                CreatedDate = g.CreatedDate
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GameDto>>
            .Success(games);
    }

    public async Task<Result<IReadOnlyList<GameDto>>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var games = await _context.Games
            .AsNoTracking()
            .Where(g => g.IsActive)
            .OrderBy(g => g.Name)
            .Select(g => new GameDto
            {
                Id = g.Id,
                Name = g.Name,
                Slug = g.Slug,
                Version = g.Version,
                Platform = g.Platform,
                ImageUrl = g.ImageUrl,
                IsActive = g.IsActive,
                TournamentCount = g.Tournaments.Count,
                CreatedDate = g.CreatedDate
            })
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GameDto>>
            .Success(games);
    }

    public async Task<Result<GameDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var game = await _context.Games
            .AsNoTracking()
            .Where(g => g.Id == id)
            .Select(g => new GameDto
            {
                Id = g.Id,
                Name = g.Name,
                Slug = g.Slug,
                Version = g.Version,
                Platform = g.Platform,
                ImageUrl = g.ImageUrl,
                IsActive = g.IsActive,
                TournamentCount = g.Tournaments.Count,
                CreatedDate = g.CreatedDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (game is null)
        {
            return Result<GameDto>.Failure(
                "The game could not be found.");
        }

        return Result<GameDto>.Success(game);
    }

    public async Task<Result<GameDto>> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return Result<GameDto>.Failure(
                "The game could not be found.");
        }

        var normalizedSlug = slug
            .Trim()
            .ToLowerInvariant();

        var game = await _context.Games
            .AsNoTracking()
            .Where(g => g.Slug == normalizedSlug)
            .Select(g => new GameDto
            {
                Id = g.Id,
                Name = g.Name,
                Slug = g.Slug,
                Version = g.Version,
                Platform = g.Platform,
                ImageUrl = g.ImageUrl,
                IsActive = g.IsActive,
                TournamentCount = g.Tournaments.Count,
                CreatedDate = g.CreatedDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (game is null)
        {
            return Result<GameDto>.Failure(
                "The game could not be found.");
        }

        return Result<GameDto>.Success(game);
    }

    public async Task<Result<Guid>> CreateAsync(
        CreateGameRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateGame(
            request.Name,
            request.Version,
            request.Platform);

        if (!validation.Succeeded)
        {
            return Result<Guid>.Failure(
                validation.Error!);
        }

        var normalizedName = request.Name.Trim();
        var normalizedVersion = request.Version.Trim();
        var normalizedPlatform = request.Platform.Trim();

        var duplicateExists = await _context.Games
            .AnyAsync(
                g =>
                    g.Name == normalizedName &&
                    g.Version == normalizedVersion &&
                    g.Platform == normalizedPlatform,
                cancellationToken);

        if (duplicateExists)
        {
            return Result<Guid>.Failure(
                "This game and platform combination already exists.");
        }

        var slug = await GenerateUniqueSlugAsync(
            normalizedName,
            normalizedVersion,
            normalizedPlatform,
            null,
            cancellationToken);

        var game = new Game
        {
            Name = normalizedName,
            Version = normalizedVersion,
            Platform = normalizedPlatform,
            ImageUrl = NormalizeOptionalString(request.ImageUrl),
            Slug = slug,
            IsActive = true
        };

        _context.Games.Add(game);

        await _context.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(
            game.Id,
            "Game created successfully.");
    }

    public async Task<Result> UpdateAsync(
        UpdateGameRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateGame(
            request.Name,
            request.Version,
            request.Platform);

        if (!validation.Succeeded)
        {
            return Result.Failure(
                validation.Error!);
        }

        var game = await _context.Games
            .FirstOrDefaultAsync(
                g => g.Id == request.Id,
                cancellationToken);

        if (game is null)
        {
            return Result.Failure(
                "The game could not be found.");
        }

        var normalizedName = request.Name.Trim();
        var normalizedVersion = request.Version.Trim();
        var normalizedPlatform = request.Platform.Trim();

        var duplicateExists = await _context.Games
            .AnyAsync(
                g =>
                    g.Id != request.Id &&
                    g.Name == normalizedName &&
                    g.Version == normalizedVersion &&
                    g.Platform == normalizedPlatform,
                cancellationToken);

        if (duplicateExists)
        {
            return Result.Failure(
                "Another game with the same name, version and platform already exists.");
        }

        var identityChanged =
            !string.Equals(
                game.Name,
                normalizedName,
                StringComparison.Ordinal) ||
            !string.Equals(
                game.Version,
                normalizedVersion,
                StringComparison.Ordinal) ||
            !string.Equals(
                game.Platform,
                normalizedPlatform,
                StringComparison.Ordinal);

        game.Name = normalizedName;
        game.Version = normalizedVersion;
        game.Platform = normalizedPlatform;
        game.ImageUrl =
            NormalizeOptionalString(request.ImageUrl);

        if (identityChanged)
        {
            game.Slug = await GenerateUniqueSlugAsync(
                normalizedName,
                normalizedVersion,
                normalizedPlatform,
                game.Id,
                cancellationToken);
        }

        await _context.SaveChangesAsync(
            cancellationToken);

        return Result.Success(
            "Game updated successfully.");
    }

    public async Task<Result> ActivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var game = await _context.Games
            .FirstOrDefaultAsync(
                g => g.Id == id,
                cancellationToken);

        if (game is null)
        {
            return Result.Failure(
                "The game could not be found.");
        }

        if (game.IsActive)
        {
            return Result.Failure(
                "This game is already active.");
        }

        game.IsActive = true;

        await _context.SaveChangesAsync(
            cancellationToken);

        return Result.Success(
            $"{game.Name} is now active.");
    }

    public async Task<Result> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var game = await _context.Games
            .FirstOrDefaultAsync(
                g => g.Id == id,
                cancellationToken);

        if (game is null)
        {
            return Result.Failure(
                "The game could not be found.");
        }

        if (!game.IsActive)
        {
            return Result.Failure(
                "This game is already inactive.");
        }

        game.IsActive = false;

        await _context.SaveChangesAsync(
            cancellationToken);

        return Result.Success(
            $"{game.Name} has been deactivated.");
    }

    public async Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var game = await _context.Games
            .Include(g => g.Tournaments)
            .FirstOrDefaultAsync(
                g => g.Id == id,
                cancellationToken);

        if (game is null)
        {
            return Result.Failure(
                "The game could not be found.");
        }

        if (game.Tournaments.Any())
        {
            return Result.Failure(
                "This game cannot be deleted because tournaments are linked to it. Deactivate it instead.");
        }

        _context.Games.Remove(game);

        await _context.SaveChangesAsync(
            cancellationToken);

        return Result.Success(
            "Game deleted successfully.");
    }

    private static Result ValidateGame(
        string name,
        string version,
        string platform)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(
                "Game name is required.");
        }

        if (name.Trim().Length > 150)
        {
            return Result.Failure(
                "Game name cannot exceed 150 characters.");
        }

        if (string.IsNullOrWhiteSpace(version))
        {
            return Result.Failure(
                "Game version is required.");
        }

        if (version.Trim().Length > 50)
        {
            return Result.Failure(
                "Game version cannot exceed 50 characters.");
        }

        if (string.IsNullOrWhiteSpace(platform))
        {
            return Result.Failure(
                "Platform is required.");
        }

        if (platform.Trim().Length > 100)
        {
            return Result.Failure(
                "Platform cannot exceed 100 characters.");
        }

        return Result.Success();
    }

    private async Task<string> GenerateUniqueSlugAsync(
        string name,
        string version,
        string platform,
        Guid? currentGameId,
        CancellationToken cancellationToken)
    {
        var source =
            $"{name} {version} {platform}";

        var slug = source
            .Trim()
            .ToLowerInvariant();

        slug = Regex.Replace(
            slug,
            @"[^a-z0-9\s-]",
            "");

        slug = Regex.Replace(
            slug,
            @"\s+",
            "-");

        slug = Regex.Replace(
            slug,
            @"-+",
            "-");

        slug = slug.Trim('-');

        if (string.IsNullOrWhiteSpace(slug))
        {
            slug = "game";
        }

        var baseSlug = slug;
        var suffix = 2;

        while (await _context.Games.AnyAsync(
                   g =>
                       g.Slug == slug &&
                       (!currentGameId.HasValue ||
                        g.Id != currentGameId.Value),
                   cancellationToken))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }

    private static string? NormalizeOptionalString(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
