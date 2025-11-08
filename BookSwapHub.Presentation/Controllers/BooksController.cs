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
    private readonly IWebHostEnvironment _env;

    public BooksController(IBookService books, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _books = books;
        _userManager = userManager;
        _env = env;
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
        
        // Validate file size (10 MB max)
        const long maxFileSize = 10 * 1024 * 1024;
        if (image.Length > maxFileSize)
        {
            Console.WriteLine($"File too large: {image.Length} bytes. Max allowed: {maxFileSize} bytes");
            return null;
        }
        
        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        var extension = Path.GetExtension(image.FileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
        {
            Console.WriteLine($"Invalid file type: {extension}. Allowed types: {string.Join(", ", allowedExtensions)}");
            return null;
        }
        
        try
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploads);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploads, fileName);
            
            await using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await image.CopyToAsync(stream);
                await stream.FlushAsync();
            }
            
            return $"/uploads/{fileName}";
        }
        catch (Exception ex)
        {
            // Log the error but don't crash the application
            Console.WriteLine($"Error saving image: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB limit
    [DisableRequestSizeLimit] // Allow large file uploads
    public async Task<IActionResult> Create(CreateBookDto dto, IFormFile? image)
    {
        if (!ModelState.IsValid) return View(dto);
        
        // Validate image if provided
        if (image != null && image.Length > 0)
        {
            const long maxFileSize = 10 * 1024 * 1024;
            if (image.Length > maxFileSize)
            {
                ModelState.AddModelError("image", $"File size must be less than 10 MB. Your file is {image.Length / 1024 / 1024:F2} MB.");
                return View(dto);
            }
            
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var extension = Path.GetExtension(image.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("image", "Only image files (.jpg, .jpeg, .png, .gif, .bmp) are allowed.");
                return View(dto);
            }
        }
        
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
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB limit
    [DisableRequestSizeLimit] // Allow large file uploads
    public async Task<IActionResult> Edit(int id, CreateBookDto dto, IFormFile? image)
    {
        if (!ModelState.IsValid) return View(dto);
        
        // Validate image if provided
        if (image != null && image.Length > 0)
        {
            const long maxFileSize = 10 * 1024 * 1024;
            if (image.Length > maxFileSize)
            {
                ModelState.AddModelError("image", $"File size must be less than 10 MB. Your file is {image.Length / 1024 / 1024:F2} MB.");
                return View(dto);
            }
            
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var extension = Path.GetExtension(image.FileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("image", "Only image files (.jpg, .jpeg, .png, .gif, .bmp) are allowed.");
                return View(dto);
            }
        }
        
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
