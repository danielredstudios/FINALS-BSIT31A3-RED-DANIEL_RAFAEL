using BookSwapHub.Application.Enums;

namespace BookSwapHub.Application.Models;

public record BookDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string OwnerId { get; init; } = string.Empty;
    public string OwnerName { get; init; } = string.Empty;
    public BookCondition Condition { get; init; }
    public string? Category { get; init; }
    public string? ImagePath { get; init; }
    public DateTime CreatedAt { get; init; }
}
