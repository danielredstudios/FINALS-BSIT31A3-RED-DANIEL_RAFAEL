namespace BookSwapHub.Application.Models;

public record BookDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string OwnerId { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
