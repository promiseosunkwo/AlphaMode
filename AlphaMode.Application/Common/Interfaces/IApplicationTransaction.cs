using AlphaMode.Application.Common.Models;

namespace AlphaMode.Application.Common.Interfaces;

public interface IApplicationTransaction
{
    Task<Result> ExecuteAsync(Func<CancellationToken, Task<Result>> operation, CancellationToken cancellationToken = default);
}