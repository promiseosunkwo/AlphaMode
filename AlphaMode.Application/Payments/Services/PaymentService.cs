using AlphaMode.Application.Common.Interfaces;
using AlphaMode.Application.Common.Models;
using AlphaMode.Application.Payments.DTOs;
using AlphaMode.Application.Payments.Interfaces;
using AlphaMode.Application.Payments.Requests;
using AlphaMode.Domain.Entities;
using AlphaMode.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Application.Payments.Services;

public class PaymentService : IPaymentService
{
    private const int DefaultPageSize = 25;
    private const int MaximumPageSize = 100;

    private readonly IApplicationDbContext _context;

    public PaymentService(IApplicationDbContext context)
    {
        _context = context;
    }

    // =====================================================
    // GET PAGED
    // =====================================================

    public async Task<Result<PagedResult<PaymentDto>>> GetPagedAsync(
        int page = 1,
        int pageSize = DefaultPageSize,
        string? search = null,
        PaymentStatus? status = null,
        PaymentType? type = null,
        PaymentMethod? method = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);

        pageSize = pageSize <= 0
            ? DefaultPageSize
            : Math.Min(pageSize, MaximumPageSize);

        var query = _context.Payments
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(x =>
                x.Status == status.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(x =>
                x.Type == type.Value);
        }

        if (method.HasValue)
        {
            query = query.Where(x =>
                x.Method == method.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(x =>
                x.TransactionReference.Contains(term) ||
                (x.ProviderReference != null &&
                 x.ProviderReference.Contains(term)) ||
                x.Player.GamerTag.Contains(term) ||
                x.Player.User.FirstName.Contains(term) ||
                x.Player.User.LastName.Contains(term) ||
                (x.Player.User.Email != null &&
                 x.Player.User.Email.Contains(term)) ||
                (x.Tournament != null &&
                 x.Tournament.Name.Contains(term)));
        }

        var totalCount =
            await query.CountAsync(cancellationToken);

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling(
                totalCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var items = await query
            .OrderByDescending(x => x.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PaymentDto
            {
                Id = x.Id,

                PlayerId = x.PlayerId,
                GamerTag = x.Player.GamerTag,

                PlayerName =
                    (x.Player.User.FirstName + " " +
                     x.Player.User.LastName).Trim(),

                Email =
                    x.Player.User.Email ?? string.Empty,

                AvatarUrl =
                    x.Player.User.AvatarUrl,

                TournamentId =
                    x.TournamentId,

                TournamentName =
                    x.Tournament != null
                        ? x.Tournament.Name
                        : null,

                GameName =
                    x.Tournament != null
                        ? x.Tournament.Game.Name
                        : null,

                TournamentRegistrationId =
                    x.TournamentRegistrationId,

                Amount = x.Amount,
                Currency = x.Currency,

                Status = x.Status,
                Type = x.Type,
                Method = x.Method,

                TransactionReference =
                    x.TransactionReference,

                ProviderReference =
                    x.ProviderReference,

                PaymentProvider =
                    x.PaymentProvider,

                CreatedAt =
                    x.CreatedDate,

                PaidAt =
                    x.PaidAt
            })
            .ToListAsync(cancellationToken);

        return Result<PagedResult<PaymentDto>>
            .Success(
                PagedResult<PaymentDto>.Create(
                    items,
                    page,
                    pageSize,
                    totalCount));
    }

    // =====================================================
    // DETAILS
    // =====================================================

    public async Task<Result<PaymentDetailsDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new PaymentDetailsDto
            {
                Id = x.Id,

                PlayerId = x.PlayerId,

                GamerTag =
                    x.Player.GamerTag,

                FirstName =
                    x.Player.User.FirstName,

                LastName =
                    x.Player.User.LastName,

                Email =
                    x.Player.User.Email ?? string.Empty,

                AvatarUrl =
                    x.Player.User.AvatarUrl,

                TournamentId =
                    x.TournamentId,

                TournamentName =
                    x.Tournament != null
                        ? x.Tournament.Name
                        : null,

                GameName =
                    x.Tournament != null
                        ? x.Tournament.Game.Name
                        : null,

                TournamentRegistrationId =
                    x.TournamentRegistrationId,

                Amount =
                    x.Amount,

                Currency =
                    x.Currency,

                Status =
                    x.Status,

                Type =
                    x.Type,

                Method =
                    x.Method,

                TransactionReference =
                    x.TransactionReference,

                ProviderReference =
                    x.ProviderReference,

                PaymentProvider =
                    x.PaymentProvider,

                CreatedAt =
                    x.CreatedDate,

                PaidAt =
                    x.PaidAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (payment is null)
        {
            return Result<PaymentDetailsDto>
                .Failure("Payment was not found.");
        }

        return Result<PaymentDetailsDto>
            .Success(payment);
    }

    // =====================================================
    // SUMMARY
    // =====================================================

    public async Task<Result<PaymentSummaryDto>> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var completedRevenue =
            await _context.Payments
                .AsNoTracking()
                .Where(x =>
                    x.Type == PaymentType.TournamentEntry &&
                    x.Status == PaymentStatus.Completed)
                .SumAsync(
                    x => (decimal?)x.Amount,
                    cancellationToken)
                ?? 0;

        var pendingAmount =
            await _context.Payments
                .AsNoTracking()
                .Where(x =>
                    x.Type == PaymentType.TournamentEntry &&
                    x.Status == PaymentStatus.Pending)
                .SumAsync(
                    x => (decimal?)x.Amount,
                    cancellationToken)
                ?? 0;

        var completedCount =
            await _context.Payments
                .AsNoTracking()
                .CountAsync(
                    x => x.Status == PaymentStatus.Completed,
                    cancellationToken);

        var pendingCount =
            await _context.Payments
                .AsNoTracking()
                .CountAsync(
                    x => x.Status == PaymentStatus.Pending,
                    cancellationToken);

        var failedCount =
            await _context.Payments
                .AsNoTracking()
                .CountAsync(
                    x => x.Status == PaymentStatus.Failed,
                    cancellationToken);

        var refundedCount =
            await _context.Payments
                .AsNoTracking()
                .CountAsync(
                    x => x.Status == PaymentStatus.Refunded,
                    cancellationToken);

        var summary = new PaymentSummaryDto
        {
            CompletedRevenue = completedRevenue,
            PendingAmount = pendingAmount,
            CompletedCount = completedCount,
            PendingCount = pendingCount,
            FailedCount = failedCount,
            RefundedCount = refundedCount
        };

        return Result<PaymentSummaryDto>
            .Success(summary);
    }

    // =====================================================
    // CREATE
    // =====================================================

    public async Task<Result<Guid>> CreateAsync(
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.PlayerId == Guid.Empty)
        {
            return Result<Guid>
                .Failure("A player is required.");
        }

        if (request.Amount <= 0)
        {
            return Result<Guid>
                .Failure("Payment amount must be greater than zero.");
        }

        var player = await _context.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == request.PlayerId,
                cancellationToken);

        if (player is null)
        {
            return Result<Guid>
                .Failure("Player was not found.");
        }

        if (request.Type == PaymentType.TournamentEntry)
        {
            if (!player.IsActive)
            {
                return Result<Guid>.Failure(
                    "Suspended players cannot make tournament entry payments.");
            }

            if (!request.TournamentId.HasValue ||
                !request.TournamentRegistrationId.HasValue)
            {
                return Result<Guid>.Failure(
                    "Tournament entry payments must be linked to a tournament registration.");
            }

            var registration =
                await _context.TournamentRegistrations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x =>
                            x.Id ==
                                request.TournamentRegistrationId.Value &&
                            x.PlayerId ==
                                request.PlayerId &&
                            x.TournamentId ==
                                request.TournamentId.Value,
                        cancellationToken);

            if (registration is null)
            {
                return Result<Guid>.Failure(
                    "The tournament registration does not match this player and tournament.");
            }

            if (registration.Status !=
                TournamentRegistrationStatus.PendingPayment)
            {
                return Result<Guid>.Failure(
                    "This registration is not awaiting payment.");
            }

            var tournament =
                await _context.Tournaments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.Id == request.TournamentId.Value,
                        cancellationToken);

            if (tournament is null)
            {
                return Result<Guid>
                    .Failure("Tournament was not found.");
            }

            if (tournament.EntryFee <= 0)
            {
                return Result<Guid>.Failure(
                    "This tournament does not require an entry payment.");
            }

            if (request.Amount != tournament.EntryFee)
            {
                return Result<Guid>.Failure(
                    $"The required tournament entry fee is {tournament.EntryFee:N2}.");
            }
        }

        var currency =
            NormalizeCurrency(request.Currency);

        if (currency.Length != 3)
        {
            return Result<Guid>.Failure(
                "Currency must be a valid 3-letter code.");
        }

        var transactionReference =
            await GenerateUniqueReferenceAsync(
                cancellationToken);

        var payment = new Payment
        {
            PlayerId = request.PlayerId,

            TournamentId =
                request.TournamentId,

            TournamentRegistrationId =
                request.TournamentRegistrationId,

            Amount =
                request.Amount,

            Currency =
                currency,

            Status =
                PaymentStatus.Pending,

            Type =
                request.Type,

            Method =
                request.Method,

            TransactionReference =
                transactionReference,

            PaymentProvider =
                NormalizeOptional(
                    request.PaymentProvider)
        };

        _context.Payments.Add(payment);

        await _context.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(
            payment.Id,
            "Payment created successfully.");
    }

    // =====================================================
    // COMPLETE
    // =====================================================

    public async Task<Result> CompleteAsync(
        CompletePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        var payment = await _context.Payments
            .Include(x => x.Player)
            .Include(x => x.TournamentRegistration)
            .FirstOrDefaultAsync(
                x => x.Id == request.PaymentId,
                cancellationToken);

        if (payment is null)
        {
            return Result.Failure(
                "Payment was not found.");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return Result.Failure(
                "Only pending payments can be completed.");
        }

        if (payment.Type == PaymentType.TournamentEntry)
        {
            if (!payment.Player.IsActive)
            {
                return Result.Failure(
                    "This player is suspended. The entry payment cannot be completed.");
            }

            if (payment.TournamentRegistration is null)
            {
                return Result.Failure(
                    "This tournament entry payment is not linked to a registration.");
            }

            if (payment.TournamentRegistration.Status !=
                TournamentRegistrationStatus.PendingPayment)
            {
                return Result.Failure(
                    "The linked tournament registration is no longer awaiting payment.");
            }

            payment.TournamentRegistration.Status =
                TournamentRegistrationStatus.Registered;
        }

        payment.Status =
            PaymentStatus.Completed;

        payment.PaidAt =
            DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(
                request.ProviderReference))
        {
            payment.ProviderReference =
                request.ProviderReference.Trim();
        }

        if (!string.IsNullOrWhiteSpace(
                request.PaymentProvider))
        {
            payment.PaymentProvider =
                request.PaymentProvider.Trim();
        }

        await _context.SaveChangesAsync(
            cancellationToken);

        return Result.Success(
            "Payment completed successfully.");
    }

    // =====================================================
    // FAILED
    // =====================================================

    public async Task<Result> MarkFailedAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var payment =
            await _context.Payments
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);

        if (payment is null)
        {
            return Result.Failure(
                "Payment was not found.");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return Result.Failure(
                "Only pending payments can be marked as failed.");
        }

        payment.Status =
            PaymentStatus.Failed;

        await _context.SaveChangesAsync(
            cancellationToken);

        return Result.Success(
            "Payment marked as failed.");
    }

    // =====================================================
    // CANCEL
    // =====================================================

    public async Task<Result> CancelAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var payment =
            await _context.Payments
                .FirstOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);

        if (payment is null)
        {
            return Result.Failure(
                "Payment was not found.");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return Result.Failure(
                "Only pending payments can be cancelled.");
        }

        payment.Status =
            PaymentStatus.Cancelled;

        await _context.SaveChangesAsync(
            cancellationToken);

        return Result.Success(
            "Payment cancelled.");
    }

    public async Task<Result<PaymentCheckoutDto>>
    GetOrCreateTournamentPaymentAsync(
        Guid registrationId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (registrationId == Guid.Empty)
        {
            return Result<PaymentCheckoutDto>
                .Failure("Tournament registration is required.");
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<PaymentCheckoutDto>
                .Failure("User account could not be identified.");
        }

        var registration =
            await _context.TournamentRegistrations
                .Include(x => x.Player)
                .Include(x => x.Tournament)
                .FirstOrDefaultAsync(
                    x => x.Id == registrationId,
                    cancellationToken);

        if (registration is null)
        {
            return Result<PaymentCheckoutDto>
                .Failure("Tournament registration was not found.");
        }

        // SECURITY:
        // The logged-in Identity user must own the Player profile.
        if (registration.Player.UserId != userId)
        {
            return Result<PaymentCheckoutDto>
                .Failure("You cannot access this registration.");
        }

        if (!registration.Player.IsActive)
        {
            return Result<PaymentCheckoutDto>
                .Failure("Your player account is currently inactive.");
        }

        if (registration.Status ==
            TournamentRegistrationStatus.Registered)
        {
            return Result<PaymentCheckoutDto>
                .Failure("This tournament registration has already been paid.");
        }

        if (registration.Status ==
            TournamentRegistrationStatus.CheckedIn)
        {
            return Result<PaymentCheckoutDto>
                .Failure("This tournament registration has already been completed.");
        }

        if (registration.Status !=
            TournamentRegistrationStatus.PendingPayment)
        {
            return Result<PaymentCheckoutDto>
                .Failure("This registration is not awaiting payment.");
        }

        if (registration.Tournament.EntryFee <= 0)
        {
            return Result<PaymentCheckoutDto>
                .Failure("This tournament does not require payment.");
        }

        // -------------------------------------------------
        // EXISTING PENDING PAYMENT
        // -------------------------------------------------

        var existingPayment =
            await _context.Payments
                .AsNoTracking()
                .Where(x =>
                    x.TournamentRegistrationId == registration.Id &&
                    x.PlayerId == registration.PlayerId &&
                    x.Type == PaymentType.TournamentEntry &&
                    x.Status == PaymentStatus.Pending)
                .OrderByDescending(x => x.CreatedDate)
                .FirstOrDefaultAsync(cancellationToken);

        if (existingPayment is not null)
        {
            return Result<PaymentCheckoutDto>.Success(
                new PaymentCheckoutDto
                {
                    PaymentId = existingPayment.Id,

                    TournamentRegistrationId =
                        registration.Id,

                    TransactionReference =
                        existingPayment.TransactionReference,

                    Amount =
                        existingPayment.Amount,

                    Currency =
                        existingPayment.Currency,

                    TournamentName =
                        registration.Tournament.Name,

                    AlreadyExists = true
                });
        }

        // -------------------------------------------------
        // CREATE PAYMENT ATTEMPT
        // -------------------------------------------------

        var payment = new Payment
        {
            PlayerId =
                registration.PlayerId,

            TournamentId =
                registration.TournamentId,

            TournamentRegistrationId =
                registration.Id,

            Amount =
                registration.Tournament.EntryFee,

            Currency =
                "NGN",

            Status =
                PaymentStatus.Pending,

            Type =
                PaymentType.TournamentEntry,

            Method =
                PaymentMethod.NotSpecified,

            TransactionReference = await GenerateTransactionReferenceAsync(cancellationToken)
        };

        _context.Payments.Add(payment);

        await _context.SaveChangesAsync(
            cancellationToken);

        return Result<PaymentCheckoutDto>.Success(
            new PaymentCheckoutDto
            {
                PaymentId = payment.Id,

                TournamentRegistrationId =
                    registration.Id,

                TransactionReference =
                    payment.TransactionReference,

                Amount =
                    payment.Amount,

                Currency =
                    payment.Currency,

                TournamentName =
                    registration.Tournament.Name,

                AlreadyExists = false
            },
            "Payment checkout created.");
    }


        public async Task<Result<PlayerPaymentDto>>GetPlayerPaymentAsync(Guid paymentId, string userId, CancellationToken cancellationToken = default)
        {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<PlayerPaymentDto>
                .Failure("User account could not be identified.");
        }

        var payment = await _context.Payments.AsNoTracking().Where(x => x.Id == paymentId && x.Player.UserId == userId)
                .Select(x => new PlayerPaymentDto
                {
                    Id = x.Id,

                    PlayerId = x.PlayerId,

                    TournamentId = x.TournamentId,

                    TournamentRegistrationId =
                        x.TournamentRegistrationId,

                    TournamentName =
                        x.Tournament != null
                            ? x.Tournament.Name
                            : string.Empty,

                    GameName =
                        x.Tournament != null
                            ? x.Tournament.Game.Name
                            : string.Empty,

                    TournamentImageUrl =
                        x.Tournament != null
                            ? x.Tournament.ImageUrl
                            : null,

                    Amount = x.Amount,

                    Currency = x.Currency,

                    Status = x.Status,

                    Type = x.Type,

                    Method = x.Method,

                    TransactionReference =
                        x.TransactionReference,

                    ProviderReference =
                        x.ProviderReference,

                    PaymentProvider =
                        x.PaymentProvider,

                    CreatedDate =
                        x.CreatedDate,

                    PaidAt =
                        x.PaidAt,

                    RegistrationStatus =
                        x.TournamentRegistration != null
                            ? x.TournamentRegistration.Status
                            : null
                })
                .FirstOrDefaultAsync(
                    cancellationToken);

        if (payment is null)
        {
            return Result<PlayerPaymentDto>
                .Failure("Payment was not found.");
        }

        return Result<PlayerPaymentDto>
            .Success(payment);
    }

    public async Task<Result<IReadOnlyList<PlayerPaymentDto>>>GetPlayerPaymentsAsync(string userId,  CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<IReadOnlyList<PlayerPaymentDto>>
                .Failure("User account could not be identified.");
        }

        var payments =
            await _context.Payments
                .AsNoTracking()
                .Where(x =>
                    x.Player.UserId == userId)
                .OrderByDescending(x =>
                    x.CreatedDate)
                .Select(x => new PlayerPaymentDto
                {
                    Id = x.Id,

                    PlayerId = x.PlayerId,

                    TournamentId = x.TournamentId,

                    TournamentRegistrationId =
                        x.TournamentRegistrationId,

                    TournamentName =
                        x.Tournament != null
                            ? x.Tournament.Name
                            : string.Empty,

                    GameName =
                        x.Tournament != null
                            ? x.Tournament.Game.Name
                            : string.Empty,

                    TournamentImageUrl =
                        x.Tournament != null
                            ? x.Tournament.ImageUrl
                            : null,

                    Amount = x.Amount,

                    Currency = x.Currency,

                    Status = x.Status,

                    Type = x.Type,

                    Method = x.Method,

                    TransactionReference =
                        x.TransactionReference,

                    ProviderReference =
                        x.ProviderReference,

                    PaymentProvider =
                        x.PaymentProvider,

                    CreatedDate =
                        x.CreatedDate,

                    PaidAt =
                        x.PaidAt,

                    RegistrationStatus =
                        x.TournamentRegistration != null
                            ? x.TournamentRegistration.Status
                            : null
                })
                .ToListAsync(
                    cancellationToken);

        return Result<IReadOnlyList<PlayerPaymentDto>>
            .Success(payments);
    }

    // =====================================================
    // HELPERS
    // =====================================================

    private async Task<string> GenerateUniqueReferenceAsync(
        CancellationToken cancellationToken)
    {
        while (true)
        {
            var reference =
                $"AM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"
                    .ToUpperInvariant();

            reference =
                reference[..Math.Min(reference.Length, 35)];

            var exists =
                await _context.Payments
                    .AnyAsync(
                        x => x.TransactionReference == reference,
                        cancellationToken);

            if (!exists)
                return reference;
        }
    }

    private static string NormalizeCurrency(
        string? currency)
    {
        return string.IsNullOrWhiteSpace(currency)
            ? "NGN"
            : currency.Trim().ToUpperInvariant();
    }

    private static string? NormalizeOptional(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private async Task<string> GenerateTransactionReferenceAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var reference = $"AM-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N[..8]}".ToUpperInvariant();

            var exists = await _context.Payments.AnyAsync(x => x.TransactionReference == reference, cancellationToken);

            if (!exists) return reference;
        }
    }
}