namespace BookSwapHub.Application.Models;

public record CreateBookDto
{
    public string Title { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string? Description { get; init; }
}
