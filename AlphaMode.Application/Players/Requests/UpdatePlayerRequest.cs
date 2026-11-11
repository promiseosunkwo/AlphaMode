namespace AlphaMode.Application.Players.Requests
{
    public class UpdatePlayerRequest
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string GamerTag { get; set; } = string.Empty;
    }
}
