using BookSwapHub.Application.Interfaces;
using BookSwapHub.Application.Models;
using BookSwapHub.Infrastructure.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookSwapHub.Presentation.Controllers;

public class BooksController : Controller
{
    private readonly IBookService _books;
    private readonly UserManager<ApplicationUser> _userManager;

    public BooksController(IBookService books, UserManager<ApplicationUser> userManager)
    {
        _books = books;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        var list = await _books.GetAllAsync(q);
        return View(list);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Mine()
    {
        var uid = _userManager.GetUserId(User)!;
        var list = await _books.GetAllAsync(ownerId: uid);
        return View("Index", list);
    }

    [Authorize]
    [HttpGet]
    public IActionResult Create() => View(new CreateBookDto());

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBookDto dto)
    {
        if (!ModelState.IsValid) return View(dto);
        var uid = _userManager.GetUserId(User)!;
        await _books.CreateAsync(dto, uid);
        return RedirectToAction(nameof(Mine));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var uid = _userManager.GetUserId(User)!;
        await _books.DeleteAsync(id, uid);
        return RedirectToAction(nameof(Mine));
    }
}
