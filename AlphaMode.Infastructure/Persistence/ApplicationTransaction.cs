using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using System.Data;

namespace AlphaMode.Infrastructure.Persistence;

public sealed class ApplicationTransaction : IApplicationTransaction
{
    private readonly ApplicationDbContext _context;

    public ApplicationTransaction(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> ExecuteAsync(
    Func<CancellationToken, Task<Result>> operation,
    CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                // Elo reads and updates two rating rows together. Serializable isolation prevents
                // simultaneous results in the same game from calculating against stale ratings.
                IsolationLevel.Serializable,
                cancellationToken);

        try
        {
            var result =
                await operation(cancellationToken);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync(
                    cancellationToken);

                _context.ChangeTracker.Clear();

                return result;
            }

            await transaction.CommitAsync(
                cancellationToken);

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            _context.ChangeTracker.Clear();

            throw;
        }
    }
}

