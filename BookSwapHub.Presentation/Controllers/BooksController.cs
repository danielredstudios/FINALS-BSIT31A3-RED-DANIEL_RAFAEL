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

    private async Task<string?> SaveImageAsync(IFormFile? image)
    {
        if (image is null || image.Length == 0) return null;
        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "BookSwapHub.Presentation", "wwwroot", "uploads");
        Directory.CreateDirectory(uploads);
        var fileName = $"{Guid.NewGuid():N}{Path.GetExtension(image.FileName)}";
        var fullPath = Path.Combine(uploads, fileName);
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await image.CopyToAsync(stream);
        return $"/uploads/{fileName}";
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBookDto dto, IFormFile? image)
    {
        if (!ModelState.IsValid) return View(dto);
        var uid = _userManager.GetUserId(User)!;
        dto = dto with { ImagePath = await SaveImageAsync(image) };
        await _books.CreateAsync(dto, uid);
        TempData["Success"] = "Success! Your book has been posted.";
        return RedirectToAction(nameof(Mine));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var b = await _books.GetByIdAsync(id);
        if (b is null) return NotFound();
        return View(b);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var b = await _books.GetByIdAsync(id);
        if (b is null) return NotFound();
        var uid = _userManager.GetUserId(User)!;
        if (b.OwnerId != uid) return Forbid();
        var dto = new CreateBookDto { Title = b.Title, Author = b.Author, Description = b.Description, Condition = b.Condition, Category = b.Category, ImagePath = b.ImagePath };
        return View(dto);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateBookDto dto, IFormFile? image)
    {
        if (!ModelState.IsValid) return View(dto);
        var uid = _userManager.GetUserId(User)!;
        var existing = await _books.GetByIdAsync(id);
        if (existing is null || existing.OwnerId != uid) return Forbid();
        var path = await SaveImageAsync(image);
        if (!string.IsNullOrWhiteSpace(path)) dto = dto with { ImagePath = path };
        var ok = await _books.UpdateAsync(id, dto, uid);
        TempData[ok ? "Success" : "Error"] = ok ? "Book updated." : "Unable to update book.";
        return RedirectToAction(nameof(Mine));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var uid = _userManager.GetUserId(User)!;
        var existing = await _books.GetByIdAsync(id);
        if (existing is null)
        {
            TempData["Error"] = "Book not found.";
            return RedirectToAction(nameof(Mine));
        }
        if (existing.OwnerId != uid)
        {
            TempData["Error"] = "You can only delete your own books.";
            return RedirectToAction(nameof(Mine));
        }
        var ok = await _books.DeleteAsync(id, uid);
        TempData[ok ? "Success" : "Error"] = ok ? "Your book was deleted." : "Cannot delete a book with swap history.";
        return RedirectToAction(nameof(Mine));
    }
}
