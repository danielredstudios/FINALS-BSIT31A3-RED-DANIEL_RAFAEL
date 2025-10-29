using BookSwapHub.Application.Interfaces;
using BookSwapHub.Application.Models;
using BookSwapHub.Infrastructure.Data;
using BookSwapHub.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookSwapHub.Infrastructure.Services;

public class BookService : IBookService
{
    private readonly AppDbContext _db;

    public BookService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<BookDto> CreateAsync(CreateBookDto dto, string ownerId, CancellationToken ct = default)
    {
        var entity = new Book
        {
            Title = dto.Title.Trim(),
            Author = dto.Author.Trim(),
            Description = dto.Description?.Trim(),
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
        };
        _db.Books.Add(entity);
        await _db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<IEnumerable<BookDto>> GetAllAsync(string? search = null, string? ownerId = null, CancellationToken ct = default)
    {
        IQueryable<Book> q = _db.Books.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(b => EF.Functions.Like(b.Title, $"%{s}%") || EF.Functions.Like(b.Author, $"%{s}%"));
        }
        if (!string.IsNullOrWhiteSpace(ownerId))
        {
            q = q.Where(b => b.OwnerId == ownerId);
        }
        var list = await q.OrderByDescending(b => b.CreatedAt).ToListAsync(ct);
        return list.Select(ToDto);
    }

    public async Task<BookDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await _db.Books.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, ct);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<bool> UpdateAsync(int id, CreateBookDto dto, string requesterId, CancellationToken ct = default)
    {
        var entity = await _db.Books.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (entity is null || entity.OwnerId != requesterId) return false;
        entity.Title = dto.Title.Trim();
        entity.Author = dto.Author.Trim();
        entity.Description = dto.Description?.Trim();
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, string requesterId, CancellationToken ct = default)
    {
        var entity = await _db.Books.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (entity is null || entity.OwnerId != requesterId) return false;
        _db.Books.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static BookDto ToDto(Book b) => new()
    {
        Id = b.Id,
        Title = b.Title,
        Author = b.Author,
        Description = b.Description,
        OwnerId = b.OwnerId,
        CreatedAt = b.CreatedAt
    };
}
