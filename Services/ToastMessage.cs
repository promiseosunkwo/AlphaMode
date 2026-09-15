namespace AlphaMode.Services;

public enum ToastType
{
    Success = 1,
    Error = 2,
    Warning = 3,
    Info = 4
}

public sealed record ToastMessage(
    Guid Id,
    string Message,
    ToastType Type,
    string? Title = null,
    int Duration = 4000);