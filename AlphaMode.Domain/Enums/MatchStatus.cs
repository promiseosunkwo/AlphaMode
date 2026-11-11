public enum MatchStatus
{
    Pending = 0,
    Scheduled = 1,
    Live = 2,
    Completed = 3,

    // Friendly fixtures need a non-destructive terminal state so cancelled history remains visible.
    Cancelled = 4
}

