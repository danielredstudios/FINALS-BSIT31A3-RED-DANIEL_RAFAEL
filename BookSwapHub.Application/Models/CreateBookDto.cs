using BookSwapHub.Application.Enums;

namespace BookSwapHub.Application.Models;

public record CreateBookDto
{
    public string Title { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string? Description { get; init; }
    public BookCondition Condition { get; init; } = BookCondition.Good;
    public string? Category { get; init; }
    public string? ImagePath { get; init; }
}
