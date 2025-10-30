using BookSwapHub.Application.Enums;
using BookSwapHub.Application.Interfaces;
using BookSwapHub.Application.Models;
using BookSwapHub.Infrastructure.Data;
using BookSwapHub.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookSwapHub.Infrastructure.Services;

public class SwapService : ISwapService
{
    private readonly AppDbContext _db;

    public SwapService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SwapRequestDto> CreateRequestAsync(CreateSwapRequestDto dto, string fromUserId, CancellationToken ct = default)
    {
        // Validate ownership of from book
        var fromBook = await _db.Books.FirstOrDefaultAsync(b => b.Id == dto.FromBookId, ct)
            ?? throw new InvalidOperationException("Offered (from) book not found");
        if (fromBook.OwnerId != fromUserId)
            throw new InvalidOperationException("You can only offer books you own");

        var toBook = await _db.Books.FirstOrDefaultAsync(b => b.Id == dto.ToBookId, ct)
            ?? throw new InvalidOperationException("Requested (to) book not found");
        if (toBook.OwnerId != dto.ToUserId)
            throw new InvalidOperationException("Requested book is not owned by the specified user");

        if (dto.ToUserId == fromUserId)
            throw new InvalidOperationException("You cannot request a swap with yourself");

        var req = new SwapRequest
        {
            FromUserId = fromUserId,
            ToUserId = dto.ToUserId,
            FromBookId = dto.FromBookId,
            ToBookId = dto.ToBookId,
            Status = SwapStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        _db.SwapRequests.Add(req);
        await _db.SaveChangesAsync(ct);
        return ToDto(req);
    }

    public async Task<bool> RespondAsync(int requestId, string responderUserId, SwapStatus status, CancellationToken ct = default)
    {
        var req = await _db.SwapRequests.FirstOrDefaultAsync(r => r.Id == requestId, ct);
        if (req is null || req.ToUserId != responderUserId || req.Status != SwapStatus.Pending)
            return false;

        if (status == SwapStatus.Accepted)
        {
            // Perform ownership swap
            var fromBook = await _db.Books.FirstAsync(b => b.Id == req.FromBookId, ct);
            var toBook = await _db.Books.FirstAsync(b => b.Id == req.ToBookId, ct);
            var tempOwner = fromBook.OwnerId;
            fromBook.OwnerId = toBook.OwnerId;
            toBook.OwnerId = tempOwner;

            // Invalidate other pending requests involving either book
            var relatedPending = await _db.SwapRequests
                .Where(r => r.Status == SwapStatus.Pending && r.Id != req.Id &&
                            (r.FromBookId == req.FromBookId || r.ToBookId == req.FromBookId ||
                             r.FromBookId == req.ToBookId   || r.ToBookId == req.ToBookId))
                .ToListAsync(ct);
            foreach (var other in relatedPending)
            {
                other.Status = SwapStatus.Cancelled; // cancelled: book no longer available
            }
        }

        req.Status = status;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IEnumerable<SwapRequestDto>> GetIncomingAsync(string userId, CancellationToken ct = default)
    {
        var list = await _db.SwapRequests.AsNoTracking()
            .Include(r => r.FromBook)
            .Include(r => r.ToBook)
            .Where(r => r.ToUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
        return list.Select(ToDto);
    }

    public async Task<IEnumerable<SwapRequestDto>> GetOutgoingAsync(string userId, CancellationToken ct = default)
    {
        var list = await _db.SwapRequests.AsNoTracking()
            .Include(r => r.FromBook)
            .Include(r => r.ToBook)
            .Where(r => r.FromUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
        return list.Select(ToDto);
    }

    public async Task<bool> CancelAsync(int requestId, string requesterUserId, CancellationToken ct = default)
    {
        var req = await _db.SwapRequests.FirstOrDefaultAsync(r => r.Id == requestId, ct);
        if (req is null || req.FromUserId != requesterUserId || req.Status != SwapStatus.Pending)
            return false;
        req.Status = SwapStatus.Cancelled;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static SwapRequestDto ToDto(SwapRequest r) => new()
    {
        Id = r.Id,
        FromUserId = r.FromUserId,
        ToUserId = r.ToUserId,
        FromBookId = r.FromBookId,
        FromBookTitle = r.FromBook?.Title,
        FromBookAuthor = r.FromBook?.Author,
        ToBookId = r.ToBookId,
        ToBookTitle = r.ToBook?.Title,
        ToBookAuthor = r.ToBook?.Author,
        Status = r.Status,
        CreatedAt = r.CreatedAt
    };
}
