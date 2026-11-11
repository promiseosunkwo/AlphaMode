using AlphaMode.Application.Common.Models;

namespace AlphaMode.Application.Payments.Gateway;

public interface IPaymentGateway
{
    Task<Result<PaymentInitializationResult>>InitializeAsync(PaymentInitializationRequest request, CancellationToken cancellationToken = default);

    Task<Result<PaymentVerificationResult>>VerifyAsync(string transactionReference, CancellationToken cancellationToken = default);
}