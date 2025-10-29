using BookSwapHub.Application.Enums;

namespace BookSwapHub.Application.Models;

public record SwapRequestDto
{
    public int Id { get; init; }
    public string FromUserId { get; init; } = string.Empty;
    public string ToUserId { get; init; } = string.Empty;
    public int FromBookId { get; init; }
    public int ToBookId { get; init; }
    public SwapStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
}
