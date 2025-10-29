using BookSwapHub.Application.Enums;
using BookSwapHub.Application.Interfaces;
using BookSwapHub.Application.Models;
using BookSwapHub.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookSwapHub.Presentation.Controllers;

[Authorize]
public class SwapsController : Controller
{
    private readonly ISwapService _swaps;
    private readonly IBookService _books;
    private readonly UserManager<ApplicationUser> _userManager;

    public SwapsController(ISwapService swaps, IBookService books, UserManager<ApplicationUser> userManager)
    {
        _swaps = swaps;
        _books = books;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Incoming()
    {
        var uid = _userManager.GetUserId(User)!;
        var list = await _swaps.GetIncomingAsync(uid);
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Outgoing()
    {
        var uid = _userManager.GetUserId(User)!;
        var list = await _swaps.GetOutgoingAsync(uid);
        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int toBookId, string toUserId)
    {
        var uid = _userManager.GetUserId(User)!;
        var myBooks = await _books.GetAllAsync(ownerId: uid);
        ViewBag.ToBookId = toBookId;
        ViewBag.ToUserId = toUserId;
        return View(myBooks);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int toBookId, int fromBookId, string toUserId)
    {
        var uid = _userManager.GetUserId(User)!;
        var dto = new CreateSwapRequestDto { ToUserId = toUserId, FromBookId = fromBookId, ToBookId = toBookId };
        await _swaps.CreateRequestAsync(dto, uid);
        return RedirectToAction(nameof(Outgoing));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Respond(int id, string decision)
    {
        var uid = _userManager.GetUserId(User)!;
        var status = decision.Equals("accept", StringComparison.OrdinalIgnoreCase) ? SwapStatus.Accepted : SwapStatus.Rejected;
        await _swaps.RespondAsync(id, uid, status);
        return RedirectToAction(nameof(Incoming));
    }
}
