using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.PrizePools.DTOs;
using AlphaMode.Application.PrizePools.Interfaces;
using AlphaMode.Application.PrizePools.Requests;
using AlphaMode.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Application.PrizePools.Services;

public class PrizePoolService : IPrizePoolService
{
    private readonly IApplicationDbContext _context;

    public PrizePoolService(
        IApplicationDbContext context)
    {
        _context = context;
    }


    // =====================================================
    // GET ALL
    // =====================================================

    public async Task<Result<IReadOnlyList<PrizePoolDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var prizePools = await _context.PrizePools
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new PrizePoolDto
            {
                Id = x.Id,

                TournamentId = x.TournamentId,

                TournamentName = x.Tournament.Name,

                TotalPrize = x.TotalPrize,

                Currency = x.Currency,

                DistributedAmount =
                    x.Distributions.Sum(d => d.Amount),

                RemainingAmount =
                    x.TotalPrize -
                    x.Distributions.Sum(d => d.Amount),

                Distributions = x.Distributions
                    .OrderBy(d => d.Position)
                    .Select(d => new PrizeDistributionDto
                    {
                        Id = d.Id,
                        Position = d.Position,
                        Amount = d.Amount
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);


        return Result<IReadOnlyList<PrizePoolDto>>
            .Success(prizePools);
    }


    // =====================================================
    // GET BY ID
    // =====================================================

    public async Task<Result<PrizePoolDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var prizePool = await _context.PrizePools
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new PrizePoolDto
            {
                Id = x.Id,

                TournamentId = x.TournamentId,

                TournamentName = x.Tournament.Name,

                TotalPrize = x.TotalPrize,

                Currency = x.Currency,

                DistributedAmount =
                    x.Distributions.Sum(d => d.Amount),

                RemainingAmount =
                    x.TotalPrize -
                    x.Distributions.Sum(d => d.Amount),

                Distributions = x.Distributions
                    .OrderBy(d => d.Position)
                    .Select(d => new PrizeDistributionDto
                    {
                        Id = d.Id,
                        Position = d.Position,
                        Amount = d.Amount
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);


        if (prizePool is null)
        {
            return Result<PrizePoolDto>
                .Failure("Prize pool not found.");
        }


        return Result<PrizePoolDto>
            .Success(prizePool);
    }


    // =====================================================
    // GET BY TOURNAMENT ID
    // =====================================================

    public async Task<Result<PrizePoolDto>> GetByTournamentIdAsync(
        Guid tournamentId,
        CancellationToken cancellationToken = default)
    {
        var prizePool = await _context.PrizePools
            .AsNoTracking()
            .Where(x => x.TournamentId == tournamentId)
            .Select(x => new PrizePoolDto
            {
                Id = x.Id,

                TournamentId = x.TournamentId,

                TournamentName = x.Tournament.Name,

                TotalPrize = x.TotalPrize,

                Currency = x.Currency,

                DistributedAmount =
                    x.Distributions.Sum(d => d.Amount),

                RemainingAmount =
                    x.TotalPrize -
                    x.Distributions.Sum(d => d.Amount),

                Distributions = x.Distributions
                    .OrderBy(d => d.Position)
                    .Select(d => new PrizeDistributionDto
                    {
                        Id = d.Id,
                        Position = d.Position,
                        Amount = d.Amount
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);


        if (prizePool is null)
        {
            return Result<PrizePoolDto>
                .Failure(
                    "No prize pool has been configured for this tournament.");
        }


        return Result<PrizePoolDto>
            .Success(prizePool);
    }


    // =====================================================
    // CREATE
    // =====================================================

    public async Task<Result<Guid>> CreateAsync(
        CreatePrizePoolRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidatePrizePool(
            request.TotalPrize,
            request.Currency,
            request.Distributions);

        if (!validation.Succeeded)
        {
            return Result<Guid>.Failure(
                validation.Error
                ?? "Invalid prize pool.");
        }


        var tournamentExists =
            await _context.Tournaments
                .AnyAsync(
                    x => x.Id == request.TournamentId,
                    cancellationToken);


        if (!tournamentExists)
        {
            return Result<Guid>.Failure(
                "The selected tournament does not exist.");
        }


        var prizePoolExists =
            await _context.PrizePools
                .AnyAsync(
                    x => x.TournamentId == request.TournamentId,
                    cancellationToken);


        if (prizePoolExists)
        {
            return Result<Guid>.Failure(
                "A prize pool has already been configured for this tournament.");
        }


        var prizePool = new PrizePool
        {
            TournamentId = request.TournamentId,

            TotalPrize = request.TotalPrize,

            Currency = NormalizeCurrency(
                request.Currency)
        };


        foreach (var requestDistribution
                 in request.Distributions)
        {
            var distribution =
                new PrizeDistribution
                {
                    Position =
                        requestDistribution.Position,

                    Amount =
                        requestDistribution.Amount
                };


            prizePool.Distributions.Add(
                distribution);
        }


        _context.PrizePools.Add(
            prizePool);


        await _context.SaveChangesAsync(
            cancellationToken);


        return Result<Guid>.Success(
            prizePool.Id,
            "Prize pool created successfully.");
    }


    // =====================================================
    // UPDATE
    // =====================================================

    public async Task<Result> UpdateAsync(
        UpdatePrizePoolRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidatePrizePool(
            request.TotalPrize,
            request.Currency,
            request.Distributions);

        if (!validation.Succeeded)
        {
            return validation;
        }


        var prizePool =
            await _context.PrizePools
                .Include(x => x.Distributions)
                .FirstOrDefaultAsync(
                    x => x.Id == request.Id,
                    cancellationToken);


        if (prizePool is null)
        {
            return Result.Failure(
                "Prize pool not found.");
        }


        // -------------------------------------------------
        // Validate submitted distribution IDs
        // -------------------------------------------------

        var submittedExistingIds =
            request.Distributions
                .Where(x => x.Id.HasValue)
                .Select(x => x.Id!.Value)
                .ToList();


        var duplicateIds =
            submittedExistingIds
                .GroupBy(x => x)
                .Any(group => group.Count() > 1);


        if (duplicateIds)
        {
            return Result.Failure(
                "A prize distribution was submitted more than once.");
        }


        var existingIds =
            prizePool.Distributions
                .Select(x => x.Id)
                .ToHashSet();


        var containsUnknownDistribution =
            submittedExistingIds
                .Any(id => !existingIds.Contains(id));


        if (containsUnknownDistribution)
        {
            return Result.Failure(
                "One or more prize distributions do not belong to this prize pool.");
        }


        // -------------------------------------------------
        // Update Prize Pool
        // -------------------------------------------------

        prizePool.TotalPrize =
            request.TotalPrize;


        prizePool.Currency =
            NormalizeCurrency(
                request.Currency);


        // -------------------------------------------------
        // Determine removed distributions
        // -------------------------------------------------

        var submittedIdSet =
            submittedExistingIds.ToHashSet();


        var distributionsToRemove =
            prizePool.Distributions
                .Where(existing =>
                    !submittedIdSet.Contains(existing.Id))
                .ToList();


        if (distributionsToRemove.Count > 0)
        {
            _context.PrizeDistributions
                .RemoveRange(
                    distributionsToRemove);
        }


        // -------------------------------------------------
        // Update existing / add new distributions
        // -------------------------------------------------

        foreach (var requestDistribution
                 in request.Distributions)
        {
            // Existing distribution
            if (requestDistribution.Id.HasValue)
            {
                var existingDistribution =
                    prizePool.Distributions
                        .First(x =>
                            x.Id ==
                            requestDistribution.Id.Value);


                existingDistribution.Position =
                    requestDistribution.Position;


                existingDistribution.Amount =
                    requestDistribution.Amount;


                continue;
            }


            // New distribution
            var newDistribution =
                new PrizeDistribution
                {
                    PrizePoolId =
                        prizePool.Id,

                    Position =
                        requestDistribution.Position,

                    Amount =
                        requestDistribution.Amount
                };


            prizePool.Distributions.Add(
                newDistribution);
        }


        await _context.SaveChangesAsync(
            cancellationToken);


        return Result.Success(
            "Prize pool updated successfully.");
    }


    // =====================================================
    // DELETE
    // =====================================================

    public async Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var prizePool =
            await _context.PrizePools
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);


        if (prizePool is null)
        {
            return Result.Failure(
                "Prize pool not found.");
        }


        _context.PrizePools.Remove(
            prizePool);


        await _context.SaveChangesAsync(
            cancellationToken);


        return Result.Success(
            "Prize pool deleted successfully.");
    }


    // =====================================================
    // VALIDATION
    // =====================================================

    private static Result ValidatePrizePool(
        decimal totalPrize,
        string currency,
        IReadOnlyList<PrizeDistributionRequest> distributions)
    {
        if (totalPrize <= 0)
        {
            return Result.Failure(
                "The total prize must be greater than zero.");
        }


        if (string.IsNullOrWhiteSpace(currency))
        {
            return Result.Failure(
                "Currency is required.");
        }


        var normalizedCurrency =
            currency.Trim();


        if (normalizedCurrency.Length != 3)
        {
            return Result.Failure(
                "Currency must use a three-letter code such as NGN, GBP or USD.");
        }


        if (distributions is null ||
            distributions.Count == 0)
        {
            return Result.Failure(
                "At least one prize distribution is required.");
        }


        if (distributions.Any(
                x => x.Position <= 0))
        {
            return Result.Failure(
                "Prize positions must be greater than zero.");
        }


        if (distributions.Any(
                x => x.Amount <= 0))
        {
            return Result.Failure(
                "Prize distribution amounts must be greater than zero.");
        }


        var hasDuplicatePositions =
            distributions
                .GroupBy(x => x.Position)
                .Any(group =>
                    group.Count() > 1);


        if (hasDuplicatePositions)
        {
            return Result.Failure(
                "Each prize position can only appear once.");
        }


        var distributedAmount =
            distributions.Sum(
                x => x.Amount);


        if (distributedAmount > totalPrize)
        {
            return Result.Failure(
                "Prize distributions cannot exceed the total prize pool.");
        }


        return Result.Success();
    }


    // =====================================================
    // HELPERS
    // =====================================================

    private static string NormalizeCurrency(
        string currency)
    {
        return currency
            .Trim()
            .ToUpperInvariant();
    }
}