public record MovieStats
{
    public int MovieId { get; init; }
    public string Title { get; init; }
    public int AverageWatchDurationS { get; init; }
    public int Watches { get; init; }
    public int ReleaseYear { get; init; }
}