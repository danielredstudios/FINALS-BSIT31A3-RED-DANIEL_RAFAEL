using BookSwapHub.Application.Models;

namespace BookSwapHub.Application.Interfaces;

public interface IBookService
{
    Task<BookDto> CreateAsync(CreateBookDto dto, string ownerId, CancellationToken ct = default);
    Task<IEnumerable<BookDto>> GetAllAsync(string? search = null, string? ownerId = null, CancellationToken ct = default);
    Task<BookDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, CreateBookDto dto, string requesterId, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, string requesterId, CancellationToken ct = default);
}
