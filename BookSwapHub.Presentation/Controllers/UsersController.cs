using BookSwapHub.Application.Interfaces;
using BookSwapHub.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BookSwapHub.Presentation.Controllers;

public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBookService _books;

    public UsersController(UserManager<ApplicationUser> userManager, IBookService books)
    {
        _userManager = userManager;
        _books = books;
    }

    [HttpGet]
    public async Task<IActionResult> Profile(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();
        var theirBooks = await _books.GetAllAsync(ownerId: id);
        ViewBag.User = user;
        return View(theirBooks);
    }
}
