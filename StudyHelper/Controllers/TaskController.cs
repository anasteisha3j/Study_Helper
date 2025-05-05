using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyApp.Data;
using StudyApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StudyApp.Controllers
{
    [Authorize]
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private const int PageSize = 5;

        public TaskController(ApplicationDbContext context,UserManager<User> userManager)
        {
            _context = context;
            _userManager=userManager;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            _context.ChangeTracker.Clear();
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                Console.WriteLine("User is NULL in Index action.");
                return RedirectToAction("Login", "Account");
            }

            var totalTasks = await _context.Tasks
                .Where(t => t.UserId == user.Id)
                .CountAsync();

            var totalPages = (int)Math.Ceiling(totalTasks / (double)PageSize);
            page = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

            var tasks = await _context.Tasks
                .Where(t => t.UserId == user.Id)
                .OrderBy(t => t.Deadline)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = PageSize;

            return View(tasks);
        }

    // GET: /Task/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new TaskModel 
        {
            Deadline = DateTime.Now.AddDays(1) 
        });
    }

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(TaskModel model)
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
        return RedirectToAction("Login", "Account");
    }

    model.UserId = user.Id;
    model.Author = user.UserName;
    model.IsCompleted = false;

    ModelState.Remove("UserId");
    ModelState.Remove("Author");

    if (ModelState.IsValid)
    {
        _context.Tasks.Add(model);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    return View(model);
}



// GET: /Task/Edit/5
[HttpGet]
public async Task<IActionResult> Edit(int id)
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
        return RedirectToAction("Login", "Account");
    }

    var task = await _context.Tasks
        .AsNoTracking()
        .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);
        
    if (task == null)
    {
        return NotFound();
    }

    return View(task);
}

// POST: /Task/Edit/5
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, TaskModel taskModel)
{
    if (id != taskModel.Id)
    {
        return NotFound();
    }

    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
        return RedirectToAction("Login", "Account");
    }

    var existingTask = await _context.Tasks
        .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);
        
    if (existingTask == null)
    {
        return NotFound();
    }

    if (ModelState.IsValid)
    {
        try
        {
            existingTask.Title = taskModel.Title;
            existingTask.Description = taskModel.Description;
            existingTask.Deadline = taskModel.Deadline;
            existingTask.IsCompleted = taskModel.IsCompleted;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TaskExists(taskModel.Id))
            {
                return NotFound();
            }
            throw;
        }
    }

    return View(taskModel);
}

private bool TaskExists(int id)
{
    return _context.Tasks.Any(e => e.Id == id);
}

        // POST: /Task/Delete
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Delete(int id)
{
    var user = await _userManager.GetUserAsync(User);
    var task = await _context.Tasks
        .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);
        
    if (task != null)
    {
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }
    return RedirectToAction(nameof(Index));
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> MarkCompleted(int id)
{
    var user = await _userManager.GetUserAsync(User);
    var task = await _context.Tasks
        .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);
        
    if (task != null)
    {
        task.IsCompleted = true;
        await _context.SaveChangesAsync();
    }
    return RedirectToAction(nameof(Index));
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ToggleComplete(int id)
{
    var user = await _userManager.GetUserAsync(User);
    var task = await _context.Tasks
        .FirstOrDefaultAsync(t => t.Id == id && t.UserId == user.Id);
        
    if (task != null)
    {
        task.IsCompleted = !task.IsCompleted;
        await _context.SaveChangesAsync();
    }
    return RedirectToAction(nameof(Index));
}
    }
}