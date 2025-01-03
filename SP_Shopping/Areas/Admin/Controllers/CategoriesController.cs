using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SP_Shopping.Data;
using SP_Shopping.Models;
using SP_Shopping.Service;
using SP_Shopping.ServiceDtos.Category;
using SP_Shopping.ServiceDtos.Product;
using SP_Shopping.Utilities;
using SP_Shopping.Utilities.MessageHandler;

namespace SP_Shopping.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CategoriesController
(
    ApplicationDbContext context,
    ILogger<CategoriesController> logger,
    IShoppingServices shoppingServices,
    IMessageHandler messageHandler
) : Controller
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<CategoriesController> _logger = logger;
    private readonly IShoppingServices _shoppingServices = shoppingServices;
    private readonly IMessageHandler _messageHandler = messageHandler;

    // GET: Categories
    public async Task<IActionResult> Index(string? query, string? type, [FromQuery] bool? sort)
    {
        _logger.LogInformation("GET: Entering Admin/Products/Search.");

        string filterQuery = "";
        string orderQuery = "Name";

        object? filterValue = null;

        try
        {
            if (!string.IsNullOrWhiteSpace(type))
            {
                if (!string.IsNullOrWhiteSpace(query))
                {
                    (filterQuery, filterValue) = type switch
                    {
                        nameof(CategoryGetDto.Id) => 
                            int.TryParse(query, out var queryNumber) 
                                ? ($"{nameof(CategoryGetDto.Id)} == @0", queryNumber as object)
                                : ("false", null),
                        nameof(CategoryGetDto.Name) => 
                            ($"{nameof(CategoryGetDto.Name)}.Contains(@0)", query),
                        _ => throw new NotImplementedException($"{type} is invalid")
                    };
                }

                bool _sort = sort ?? false;
                orderQuery = type switch
                {
                    nameof(CategoryGetDto.Id) => $"{nameof(CategoryGetDto.Id)}{(_sort ? " desc" : "")}",
                    nameof(CategoryGetDto.Name) => $"{nameof(CategoryGetDto.Name)}{(_sort ? " desc" : "")}",
                    _ => throw new NotImplementedException($"{type} is invalid")
                };
            }
        }
        catch (NotImplementedException ex)
        {
            return BadRequest(ex.Message);
        }

        _logger.LogDebug("Fetching product information matching search term.");
        var categoryList = await _shoppingServices.Category.GetAllAsync(filterQuery, orderQuery, filterValue, 20);

        return View(categoryList);
        
    }

    // GET: Categories/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _shoppingServices.Category.GetByIdAsync((int)id);
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
    public async Task<IActionResult> Create([Bind("Name")] CategoryGetDto category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }

        if (!(await _shoppingServices.Category.TryCreateAsync(new CategoryCreateDto { Name = category.Name })).TryOut(out var id, out var errMsgs))
        {
            _messageHandler.Add(TempData, errMsgs!);
            return View(category);
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Categories/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _shoppingServices.Category.GetByIdAsync((int)id);
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

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        if (!(await _shoppingServices.Category.TryUpdateAsync(id, new CategoryEditDto { Name = category.Name })).TryOut(out var errMsgs))
        {
            _messageHandler.Add(TempData, errMsgs!);
            return View(category);
        }

        return RedirectToAction(nameof(Index));
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
        var category = await _shoppingServices.Category.GetByIdAsync((int)id);
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
        if (!await _shoppingServices.Category.ExistsAsync(id))
        {
            return NotFound();
        }

        if (!(await _shoppingServices.Category.TryDeleteAsync(id)).TryOut(out var errMsgs))
        {
            _messageHandler.Add(TempData, errMsgs!);
            return RedirectToAction(nameof(Delete), new { id });
        }

        return RedirectToAction(nameof(Index));
    }

}
