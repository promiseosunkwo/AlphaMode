namespace AlphaMode.Services;

public class ToastService
{
    public event Action<ToastMessage>? OnShow;

    public void Success(
        string message,
        string? title = "Success",
        int duration = 4000)
    {
        Show(message, ToastType.Success, title, duration);
    }

    public void Error(
        string message,
        string? title = "Something went wrong",
        int duration = 5000)
    {
        Show(message, ToastType.Error, title, duration);
    }

    public void Warning(
        string message,
        string? title = "Warning",
        int duration = 4500)
    {
        Show(message, ToastType.Warning, title, duration);
    }

    public void Info(
        string message,
        string? title = "Info",
        int duration = 4000)
    {
        Show(message, ToastType.Info, title, duration);
    }

    private void Show(
        string message,
        ToastType type,
        string? title,
        int duration)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        OnShow?.Invoke(new ToastMessage(
            Guid.NewGuid(),
            message.Trim(),
            type,
            title,
            duration));
    }
}