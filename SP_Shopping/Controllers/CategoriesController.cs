using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SP_Shopping.Data;
using SP_Shopping.Models;
using SP_Shopping.Repository;

namespace SP_Shopping.Controllers;

public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IRepositoryCaching<Category> _categoryRepository;

    public CategoriesController(ApplicationDbContext context, IRepositoryCaching<Category> categoryRepository)
    {
        _context = context;
        _categoryRepository = categoryRepository;
    }

    // GET: Categories
    public async Task<IActionResult> Index()
    {
        return View(await _categoryRepository.GetAllAsync());
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
            //_context.Add(category);
            //await _context.SaveChangesAsync();
            await _categoryRepository.CreateAsync(category);
            await _categoryRepository.SaveChangesAsync();
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

        //var category = await _context.Categories.FindAsync(id);
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
            try
            {
                //_context.Update(category);
                _categoryRepository.Update(category);
                await _categoryRepository.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (await _categoryRepository.ExistsAsync(HttpContext.Request.Path, q => q.Where(c => c.Id == category.Id)))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
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
            //_context.Categories.Remove(category);
            _categoryRepository.Delete(category);
            await _categoryRepository.SaveChangesAsync();
        }

        //await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

}
