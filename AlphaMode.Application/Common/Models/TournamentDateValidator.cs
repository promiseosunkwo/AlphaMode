using AlphaMode.Application.Common.Models;

public static class TournamentDateValidator
{
    public static Result Validate(
        DateTime? registrationOpenDate,
        DateTime? registrationDeadline,
        DateTime startDate,
        DateTime endDate)
    {
        if (startDate >= endDate)
        {
            return Result.Failure(
                "Tournament end date must be after the start date.");
        }

        if (registrationOpenDate.HasValue &&
            registrationDeadline.HasValue &&
            registrationOpenDate.Value >= registrationDeadline.Value)
        {
            return Result.Failure(
                "Registration opening must be before the registration deadline.");
        }

        if (registrationDeadline.HasValue &&
            registrationDeadline.Value > startDate)
        {
            return Result.Failure(
                "Registration deadline cannot be after the tournament start.");
        }

        if (registrationOpenDate.HasValue &&
            registrationOpenDate.Value >= startDate)
        {
            return Result.Failure(
                "Registration must open before the tournament starts.");
        }

        return Result.Success();
    }
}