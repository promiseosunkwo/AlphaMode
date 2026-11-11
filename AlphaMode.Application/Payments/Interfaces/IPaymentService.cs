using AlphaMode.Application.Common.Models;
using AlphaMode.Application.Payments.DTOs;
using AlphaMode.Application.Payments.Requests;
using AlphaMode.Domain.Enums;

namespace AlphaMode.Application.Payments.Interfaces;

public interface IPaymentService
{
    Task<Result<PagedResult<PaymentDto>>> GetPagedAsync(
        int page = 1,
        int pageSize = 25,
        string? search = null,
        PaymentStatus? status = null,
        PaymentType? type = null,
        PaymentMethod? method = null,
        CancellationToken cancellationToken = default);

    Task<Result<PaymentDetailsDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PaymentSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default);

    Task<Result<Guid>> CreateAsync(CreatePaymentRequest request, CancellationToken cancellationToken = default);

    Task<Result> CompleteAsync(CompletePaymentRequest request, CancellationToken cancellationToken = default);

    Task<Result> MarkFailedAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result> CancelAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<PaymentCheckoutDto>> GetOrCreateTournamentPaymentAsync(
        Guid registrationId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<Result<PlayerPaymentDto>> GetPlayerPaymentAsync(Guid paymentId, string userId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<PlayerPaymentDto>>> GetPlayerPaymentsAsync(string userId, CancellationToken cancellationToken = default);
}
