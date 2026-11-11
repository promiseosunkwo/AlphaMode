using AlphaMode.Application.Common.Models;
using AlphaMode.Application.Games.DTOs;

namespace AlphaMode.Application.Games.Interfaces;

public interface IGameService
{
    Task<Result<IReadOnlyList<GameDto>>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<GameDto>>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<Result<GameDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result<GameDto>> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<Result<Guid>> CreateAsync(
        CreateGameRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateAsync(
        UpdateGameRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> ActivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}