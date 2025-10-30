using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BookSwapHub.Application.Enums;

namespace BookSwapHub.Infrastructure.Entities;

public class Book
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Author { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public BookCondition Condition { get; set; } = BookCondition.Good;

    [MaxLength(100)]
    public string? Category { get; set; }

    [MaxLength(256)]
    public string? ImagePath { get; set; }

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    [ForeignKey(nameof(OwnerId))]
    public ApplicationUser? Owner { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
