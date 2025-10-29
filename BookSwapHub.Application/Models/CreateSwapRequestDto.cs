namespace BookSwapHub.Application.Models;

public record CreateSwapRequestDto
{
    public string ToUserId { get; init; } = string.Empty;
    public int FromBookId { get; init; }
    public int ToBookId { get; init; }
}
