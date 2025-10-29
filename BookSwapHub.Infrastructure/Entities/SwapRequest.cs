using System.ComponentModel.DataAnnotations;
using BookSwapHub.Application.Enums;

namespace BookSwapHub.Infrastructure.Entities;

public class SwapRequest
{
    public int Id { get; set; }

    [Required]
    public string FromUserId { get; set; } = string.Empty;
    public ApplicationUser? FromUser { get; set; }

    [Required]
    public string ToUserId { get; set; } = string.Empty;
    public ApplicationUser? ToUser { get; set; }

    [Required]
    public int FromBookId { get; set; }
    public Book? FromBook { get; set; }

    [Required]
    public int ToBookId { get; set; }
    public Book? ToBook { get; set; }

    public SwapStatus Status { get; set; } = SwapStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
