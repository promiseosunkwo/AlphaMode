using AlphaMode.Application.Common.Models;
using AlphaMode.Application.PlayerTournamentRegistrations.DTOs;
using AlphaMode.Application.PlayerTournamentRegistrations.Interfaces;
using AlphaMode.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AlphaMode.Infastructure.Persistence;

public sealed class SqlServerTournamentRegistrationLock : ITournamentRegistrationLock
{
    private readonly ApplicationDbContext _context;

    public SqlServerTournamentRegistrationLock(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<TournamentPlayerRegistrationDTO>> ExecuteAsync(Guid tournamentId, Func<CancellationToken, Task<Result<TournamentPlayerRegistrationDTO>>> operation,
    CancellationToken cancellationToken = default)
    {
        var resource = $"AlphaMode:TournamentRegistration:{tournamentId:N}";

        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        try
        {
            // Acquire an exclusive transaction-scoped SQL Server lock.
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"""
            DECLARE @lockResult int;

            EXEC @lockResult = sys.sp_getapplock
                @Resource = {resource},
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = 10000;

            IF @lockResult < 0
                THROW 51000, 'Tournament registration is busy. Please try again.', 1;
            """,
                cancellationToken);

            // Registration checks and insertion happen while
            // the tournament-specific lock is held.
            var result = await operation(cancellationToken);

            if (!result.Succeeded)
            {
                await transaction.RollbackAsync(cancellationToken);
                return result;
            }

            await transaction.CommitAsync(cancellationToken);

            return result;
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        catch (SqlException ex) when (ex.Number == 51000)
        {
            await transaction.RollbackAsync(CancellationToken.None);

            return Result<TournamentPlayerRegistrationDTO>.Failure("Tournament registration is busy. Please try again.");
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(CancellationToken.None);

            return Result<TournamentPlayerRegistrationDTO>.Failure("The registration could not be completed. Please try again.");
        }
    }
}
