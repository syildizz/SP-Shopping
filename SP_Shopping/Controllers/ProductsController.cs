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
    private readonly ILogger<ProductsController> _logger;
    private readonly IMapper _mapper;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Category> _categoryRepository;

    public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger, IMapper mapper, IRepository<Product> productRepository, IRepository<Category> categoryRepository)
    {
        _context = context;
        _logger = logger;
        _mapper = mapper;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
    }

    // GET: Products
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("GET: Entering Products/Index.");
        var pdtoList = _mapper.Map<IEnumerable<Product>, IEnumerable<ProductDetailsDto>>
            (await _productRepository.GetAllAsync());
        _logger.LogDebug("Fetching all product information.");
        return View(pdtoList);
    }

    // GET: Products/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        _logger.LogInformation("GET: Entering Products/Details.");
        if (id == null)
        {
            _logger.LogError("The specified id \"{Id}\" for Product/Details does not exist.", id);
            return NotFound();
        }

        //var product = await _context.Products
        //    //.Include(p => p.Category)
        //    .FirstOrDefaultAsync(m => m.Id == id);
        var product = await _productRepository
            .GetSingleAsync(q => q.Include(p => p.Category).Where(m => m.Id == id));
        _logger.LogDebug("Fetching \"{Id}\" product information.", id);
        if (product == null)
        {
            _logger.LogError("Failed to fetch product for id \"{Id}\".", id);
            return NotFound();
        }

        ProductDetailsDto pdto = _mapper.Map<Product, ProductDetailsDto>(product);

        return View(pdto);
    }

    // GET: Products/Create
    public async Task<IActionResult> Create()
    {
        _logger.LogInformation($"GET: Entering Products/Details.");
        var pdto = _mapper.Map<Product, ProductCreateDto>(new Product());
        _logger.LogDebug($"Fetching all categories.");
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
        _logger.LogInformation($"POST: Entering Products/Create.");
        if (ModelState.IsValid)
        {
            try
            {
                _logger.LogDebug($"Creating product.");
                Product product = _mapper.Map<ProductCreateDto, Product>(pdto);
                product.InsertionDate = DateTime.Now;
                //await _context.AddAsync(product);
                //await _context.SaveChangesAsync();
                await _productRepository.CreateAsync(product);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                _logger.LogError($"Couldn't create product with name of \"{pdto.Name}\".");
                return BadRequest();
            }
        }
        else
        {
            _logger.LogError("ModelState is not valid.");
        }
        return View(pdto);
    }

    // GET: Products/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        _logger.LogInformation($"GET: Entering Products/Edit.");
        if (id == null)
        {
            _logger.LogError("Failed to fetch product for id \"{Id}\".", id);
            return NotFound();
        }

        //var product = await _context.Products.FindAsync(id);
        _logger.LogDebug("Fetching product for id \"{Id}\".", id);
        var product = await _productRepository.GetByKeyAsync((int)id);
        if (product == null)
        {
            _logger.LogError("Could not fetch product for id \"{Id}\".", id);
            return NotFound();
        }

        var pdto = _mapper.Map<Product, ProductCreateDto>(product);
        _logger.LogDebug($"Fetching all categories.");
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
        _logger.LogInformation($"GET: Entering Products/Edit.");
        if (!ProductExists(id))
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var product = _mapper.Map<ProductCreateDto, Product>(pdto);
                product.Id = id;
                
                _logger.LogDebug("Updating product.");
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
            catch (DbUpdateConcurrencyException dbuce)
            {
                _logger.LogError(dbuce, "The product with the id of \"{Id}\" could not be updated.", id);
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        _logger.LogDebug($"Fetching all categories.");
        var categorySelectList = await GetCategoriesSelectListAsync();
        ViewBag.CategorySelectList = categorySelectList;
        return View(pdto);
    }

    // GET: Products/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        _logger.LogInformation($"GET: Entering Products/Delete.");
        if (id == null)
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return NotFound();
        }

        //var product = await _context.Products
        //    .Include(p => p.Category)
        //    .FirstOrDefaultAsync(m => m.Id == id);
        _logger.LogDebug("Fetching product for id \"{Id}\".", id);
        var product = await _productRepository.GetSingleAsync(q => q.Where(p => p.Id == id));
        if (product == null)
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
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
        _logger.LogInformation($"POST: Entering Products/Delete.");
        //var product = await _context.Products.FindAsync(id);
        _logger.LogDebug("Fetching product for id \"{Id}\".", id);
        var product = await _productRepository.GetByKeyAsync(id);
        if (product == null)
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
        }
        else
        {
            //_context.Products.Remove(product);
            _logger.LogDebug("Deleting product with id for \"{Id}\" from database", id);
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
