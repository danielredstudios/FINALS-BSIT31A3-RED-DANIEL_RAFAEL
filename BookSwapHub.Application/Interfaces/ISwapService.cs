using BookSwapHub.Application.Enums;
using BookSwapHub.Application.Models;

namespace BookSwapHub.Application.Interfaces;

public interface ISwapService
{
    Task<SwapRequestDto> CreateRequestAsync(CreateSwapRequestDto dto, string fromUserId, CancellationToken ct = default);
    Task<bool> RespondAsync(int requestId, string responderUserId, SwapStatus status, CancellationToken ct = default);
    Task<IEnumerable<SwapRequestDto>> GetIncomingAsync(string userId, CancellationToken ct = default);
    Task<IEnumerable<SwapRequestDto>> GetOutgoingAsync(string userId, CancellationToken ct = default);
}
