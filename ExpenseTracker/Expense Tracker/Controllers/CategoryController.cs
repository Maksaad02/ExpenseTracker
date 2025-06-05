using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Expense_Tracker.Models;
using Microsoft.AspNetCore.Authorization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace Expense_Tracker.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<CategoryController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var categories = await _context.Categories
                .Where(c => c.UserId == user.Id)
                .Select(c => new Category
                {
                    CategoryId = c.CategoryId,
                    Title = c.Title,
                    Icon = c.Icon,
                    Type = c.Type
                })
                .ToListAsync();

            return View(categories);
        }

        // GET: Category/AddOrEdit
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (id == 0)
            {
                return View(new Category());
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == user.Id);

            if (category == null)
            {
                return NotFound();
            }

            var model = new Category
            {
                CategoryId = category.CategoryId,
                Title = category.Title,
                Icon = category.Icon,
                Type = category.Type
            };

            return View(category);
        }

        // POST: Category/AddOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit([Bind("CategoryId,Title,Icon,Type,UserId")] Category category)

        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("User is not logged in or session has expired.");
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                if (category.CategoryId == 0)
                {
                    // category = new Category
                    //{
                    //    CategoryId = category.CategoryId,
                    //    Title = category.Title,
                    //    Icon = category.Icon,
                    //    Type = category.Type,
                    //    UserId = user.Id
                    //};
                    category.UserId = user.Id; // Set UserId for new category

                    _context.Add(category);
                    _logger.LogInformation("New category created by user {UserId}", user.Id);
                }
                else
                {
                    var existingCategory = await _context.Categories
                        .FirstOrDefaultAsync(c => c.CategoryId == category.CategoryId && c.UserId == user.Id);

                    if (existingCategory == null)
                    {
                        return NotFound();
                    }

                    existingCategory.Title = category.Title;
                    existingCategory.Icon = category.Icon;
                    existingCategory.Type = category.Type;
                    existingCategory.UserId = user.Id; // Ensure UserId is set for updates

                    _context.Update(existingCategory);
                    _logger.LogInformation("Category {CategoryId} updated by user {UserId}", category.CategoryId, user.Id);
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }

                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.UserId == user.Id);

            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Category {CategoryId} deleted by user {UserId}", id, user.Id);

            return RedirectToAction(nameof(Index));
        }
    }
}
