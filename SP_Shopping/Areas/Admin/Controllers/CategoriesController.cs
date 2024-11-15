using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SP_Shopping.Data;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Service;
using SP_Shopping.Utilities;
using SP_Shopping.Utilities.MessageHandler;

namespace SP_Shopping.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IRepositoryCaching<Category> _categoryRepository;
    private readonly CategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;
    private readonly IMessageHandler _messageHandler;

    public CategoriesController(ApplicationDbContext context, IRepositoryCaching<Category> categoryRepository, ILogger<CategoriesController> logger, IMessageHandler messageHandler, CategoryService categoryService)
    {
        _context = context;
        _categoryRepository = categoryRepository;
        _categoryService = categoryService;
        _logger = logger;
        _messageHandler = messageHandler;
    }

    // GET: Categories
    public async Task<IActionResult> Index(string? query, string? type, [FromQuery] bool? sort)
    {
        _logger.LogInformation("GET: Entering Admin/Products/Search.");

        Func<IQueryable<Category>, IQueryable<Category>> queryFilter = q => q;
        Func<IQueryable<Category>, IQueryable<Category>> sortFilter = q => q
            .OrderByDescending(c => c.Name);

        try
        {
            if (!string.IsNullOrWhiteSpace(type))
            {
                if (!string.IsNullOrWhiteSpace(query))
                {
                    queryFilter = type switch
                    {
                        nameof(Category.Id) => int.TryParse(query, out var queryNumber) ? q => q.Where(c => c.Id == queryNumber) : q => q,
                        nameof(Category.Name) => q => q.Where(c => c.Name.Contains(query)),
                        _ => throw new NotImplementedException($"{type} is invalid")
                    };
                }

                sort ??= false;
                sortFilter = type switch
                {
                    nameof(Category.Id) => (bool)sort ? q => q.OrderBy(c => c.Id) : q => q.OrderByDescending(c => c.Id),
                    nameof(Category.Name) => (bool)sort ? q => q.OrderBy(c => c.Name) : q => q.OrderByDescending(c => c.Name),
                    _ => throw new NotImplementedException($"{type} is invalid")
                };
            }
        }
        catch (NotImplementedException ex)
        {
            return BadRequest(ex.Message);
        }

        _logger.LogDebug("Fetching product information matching search term.");
        var categoryList = await _categoryRepository.GetAllAsync(HttpContext.Request.Path, q => q
            ._(queryFilter)
            ._(sortFilter)
            .Take(20)
        );

        return View(categoryList);
        
    }

    // GET: Categories/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _categoryRepository.GetByKeyAsync(HttpContext.Request.Path, (int)id);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // GET: Categories/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Categories/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name")] Category category)
    {
        if (ModelState.IsValid)
        {
            if (!(await _categoryService.TryCreateAsync(category)).TryOut(out var errMsgs))
            {
                _messageHandler.Add(TempData, errMsgs!);
                return View(category);
            }
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    // GET: Categories/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _categoryRepository.GetByKeyAsync(HttpContext.Request.Path, (int)id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    // POST: Categories/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Category category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            if (!(await _categoryService.TryUpdateAsync(category)).TryOut(out var errMsgs))
            {
                _messageHandler.Add(TempData, errMsgs!);
                return View(category);
            }
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }

    // GET: Categories/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        //var category = await _context.Categories
        //    .FirstOrDefaultAsync(m => m.Id == id);
        var category = await _categoryRepository.GetByKeyAsync(HttpContext.Request.Path, (int)id);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        //var category = await _context.Categories.FindAsync(id);
        var category = await _categoryRepository.GetByKeyAsync("All", id);
        if (category != null)
        {
            if (!(await _categoryService.TryDeleteAsync(category)).TryOut(out var errMsgs))
            {
                _messageHandler.Add(TempData, errMsgs!);
                return View(category);
            }
        }

        //await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

}
