using AlphaMode.Application.Common.Models;
using AlphaMode.Application.FriendlyMatches.DTOs;
using AlphaMode.Application.FriendlyMatches.Requests;

namespace AlphaMode.Application.FriendlyMatches.Interfaces;

public interface IFriendlyMatchService
{
    Task<Result<IReadOnlyList<FriendlyMatchDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<FriendlyMatchDto>>> GetForUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateAsync(CreateFriendlyMatchRequest request, CancellationToken cancellationToken = default);
    Task<Result> RecordResultAsync(RecordFriendlyResultRequest request, CancellationToken cancellationToken = default);
    Task<Result> CorrectResultAsync(RecordFriendlyResultRequest request, CancellationToken cancellationToken = default);
    Task<Result> CancelAsync(Guid matchId, CancellationToken cancellationToken = default);
}

