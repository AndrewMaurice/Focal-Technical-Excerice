public record Metadata
{
    public int Id { get; init; }
    public int MovieId { get; init; }
    public string Title { get; init; }
    public string Language { get; init; }
    public string Duration { get; init; }
    public int ReleaseYear { get; init; }
}