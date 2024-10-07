using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Data;
using SP_Shopping.Dtos;
using SP_Shopping.Models;
using SP_Shopping.Repository;

namespace SP_Shopping.Controllers;

public class ProductsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Category> _categoryRepository;

    public ProductsController(ApplicationDbContext context, IMapper mapper, IRepository<Product> productRepository, IRepository<Category> categoryRepository)
    {
        _context = context;
        _mapper = mapper;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    // GET: Products
    public async Task<IActionResult> Index()
    {
        var pdtoList = _mapper.Map<IEnumerable<Product>, IEnumerable<ProductDetailsDto>>
            (await _productRepository.GetAllAsync());
        return View(pdtoList);
    }

    // GET: Products/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        //var product = await _context.Products
        //    //.Include(p => p.Category)
        //    .FirstOrDefaultAsync(m => m.Id == id);
        var product = await _productRepository
            .GetSingleAsync(q => q.Where(m => m.Id == id));
        if (product == null)
        {
            return NotFound();
        }

        ProductDetailsDto pdto = _mapper.Map<Product, ProductDetailsDto>(product);

        return View(pdto);
    }

    // GET: Products/Create
    public async Task<IActionResult> Create()
    {
        var pdto = _mapper.Map<Product, ProductCreateDto>(new Product());
        IEnumerable<SelectListItem> categorySelectList = await GetCategoriesSelectListAsync();
        ViewBag.categorySelectList = categorySelectList;
        return View(pdto);
    }

    // POST: Products/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateDto pdto)
    {
        if (ModelState.IsValid)
        {
            try
            {
                Product product = _mapper.Map<ProductCreateDto, Product>(pdto);
                product.InsertionDate = DateTime.Now;
                //await _context.AddAsync(product);
                //await _context.SaveChangesAsync();
                await _productRepository.CreateAsync(product);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                return BadRequest();
            }
        }
        return View(pdto);
    }

    // GET: Products/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        //var product = await _context.Products.FindAsync(id);
        var product = await _productRepository.GetByKeyAsync((int)id);
        if (product == null)
        {
            return NotFound();
        }

        var pdto = _mapper.Map<Product, ProductCreateDto>(product);
        var categorySelectList = await GetCategoriesSelectListAsync();
        ViewBag.categorySelectList = categorySelectList;

        return View(pdto);
    }

    // POST: Products/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductCreateDto pdto)
    {
        if (!ProductExists(id))
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var product = _mapper.Map<ProductCreateDto, Product>(pdto);
                product.Id = id;
                await _productRepository.UpdateCertainFieldsAsync(product,
                    q => q.Where(p => p.Id == id),
                    setPropertyCalls: s => s
                        .SetProperty(b => b.Name, product.Name)
                        .SetProperty(b => b.Price, product.Price)
                        .SetProperty(b => b.CategoryId, product.CategoryId)
                        .SetProperty(b => b.ModificationDate, DateTime.Now)
                );
                //_context.Update(product);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
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
        var categorySelectList = await GetCategoriesSelectListAsync();
        ViewBag.CategorySelectList = categorySelectList;
        return View(pdto);
    }

    // GET: Products/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        //var product = await _context.Products
        //    .Include(p => p.Category)
        //    .FirstOrDefaultAsync(m => m.Id == id);
        var product = await _productRepository.GetSingleAsync(q => q.Where(p => p.Id == id));
        if (product == null)
        {
            return NotFound();
        }

        var pdto = _mapper.Map<Product, ProductDetailsDto>(product);
        return View(pdto);
    }

    // POST: Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        //var product = await _context.Products.FindAsync(id);
        var product = await _productRepository.GetByKeyAsync(id);
        if (product != null)
        {
            //_context.Products.Remove(product);
            await _productRepository.DeleteAsync(product);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ProductExists(int id)
    {
        return _productRepository.GetByKeyAsync(id) is not null;
    }

    private async Task<IEnumerable<SelectListItem>> GetCategoriesSelectListAsync()
    {
        return (await _categoryRepository.GetAllAsync())
            .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
    }
}
