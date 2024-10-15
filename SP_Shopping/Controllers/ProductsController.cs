using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SP_Shopping.Data;
using SP_Shopping.Dtos;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using System.Security.Claims;

namespace SP_Shopping.Controllers;

public class ProductsController : Controller
{
    private readonly ILogger<ProductsController> _logger;
    private readonly IMapper _mapper;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly int paginationCount = 5;

    public ProductsController
    (
        ILogger<ProductsController> logger,
        IMapper mapper,
        IRepository<Product> productRepository,
        IRepository<Category> categoryRepository,
        IRepository<ApplicationUser> userRepository,
        IMemoryCache memoryCache
    )
    {
        _logger = logger;
        _mapper = mapper;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        _memoryCache = memoryCache;
    }

    // GET: Products
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("GET: Entering Products/Index.");
        _logger.LogDebug("Fetching all product information.");
        var products = await _productRepository.GetAllAsync(q => q
            .Include(p => p.Submitter)
            .Select(p => new Product()
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                InsertionDate = p.InsertionDate,
                ModificationDate = p.ModificationDate,
                Category = new Category()
                {
                    Name = p.Category.Name
                },
                Description = p.Description,
                Submitter = new ApplicationUser()
                {
                    UserName = p.Submitter == null ? null : p.Submitter.UserName
                }
            })
        );
        var pdtoList = _mapper.Map<IEnumerable<Product>, IEnumerable<ProductDetailsDto>>(products);
        return View(pdtoList);
    }

    public async Task<IActionResult> Search(string? query)
    {
        _logger.LogInformation("GET: Entering Products/Search.");

        IEnumerable<ProductDetailsDto>? pdtoList = null;
        if (!string.IsNullOrWhiteSpace(query))
        {
            _logger.LogDebug("Fetching product information matching search term.");
            IEnumerable<Product> products = await _productRepository.GetAllAsync(q => q
                .Where(p => p.Name.Contains(query))
                .OrderByDescending(p => p.InsertionDate)
                .ThenByDescending(p => p.ModificationDate)
                .Take(paginationCount)
                .Select(p => new Product()
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    InsertionDate = p.InsertionDate,
                    ModificationDate = p.ModificationDate,
                    Category = new Category()
                    {
                        Name = p.Category.Name
                    },
                    Submitter = new ApplicationUser()
                    {
                        UserName = p.Submitter.UserName
                    }
                })
            );
            pdtoList = _mapper.Map<IEnumerable<Product>, IEnumerable<ProductDetailsDto>>(products);
        }
        return View(pdtoList);
        
    }

    // GET: Products/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        _logger.LogInformation("GET: Entering Products/Details.");
        if (id == null)
        {
            _logger.LogError("The specified id \"{Id}\" for Product/Details does not exist.", id);
            return BadRequest("Required parameter id not specified");
        }

        //var product = await _context.Products
        //    //.Include(p => p.Category)
        //    .FirstOrDefaultAsync(m => m.Id == id);
        var product = await _productRepository
            .GetSingleAsync(q => q
            .Include(p => p.Category)
            .Include(p => p.Submitter)
            .Where(p => p.Id == id)
            .Select(p => new Product()
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Category = new Category()
                {
                    Name = p.Category.Name
                },
                Description = p.Description,
                SubmitterId = p.SubmitterId,
                Submitter = new ApplicationUser()
                {
                    UserName = p.Submitter.UserName
                },
                InsertionDate = p.InsertionDate,
                ModificationDate = p.ModificationDate,
            })
        );
        _logger.LogDebug("Fetching \"{Id}\" product information.", id);
        if (product == null)
        {
            _logger.LogError("Failed to fetch product for id \"{Id}\".", id);
            return NotFound("Product with id does not exist");
        }

        ProductDetailsDto pdto = _mapper.Map<Product, ProductDetailsDto>(product);

        return View(pdto);
    }

    // GET: Products/Create
    [Authorize]
    public async Task<IActionResult> Create()
    {
        _logger.LogInformation($"GET: Entering Products/Details.");
        var pdto = _mapper.Map<Product, ProductCreateDto>(new Product());
        _logger.LogDebug($"Fetching all categories.");
        IEnumerable<SelectListItem> categorySelectList = await GetCategoriesSelectListAsync();
        ViewBag.categorySelectList = categorySelectList;
        ViewBag.isAdmin = false;
        return View(pdto);
    }

    // POST: Products/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
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


                bool userIsAdmin = false;

                string submitterId;
                // if user is admin
                if (userIsAdmin)
                {
                    var productSubmitter = await _userRepository.GetSingleAsync(q => q
                            .Where(u => u.UserName == product.Submitter.UserName)
                            .Select(u => new ApplicationUser()
                            {
                                Id = u.Id
                            })
                    );
                    if (productSubmitter is null)
                    {
                        return BadRequest("User with name does not exist.");
                    }
                    submitterId = productSubmitter.Id;
                }
                else
                {
                    var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!await _userRepository.ExistsAsync(q => q.Where(u => u.Id == UserId)))
                    {
                        return BadRequest("UserId is invalid. Contact developer");
                    }
                    submitterId = UserId!;
                }
                // Set submitter to null and manually set Submitter foreign key
                // to avoid generating new ApplicationUser with auto-generated id.
                product.Submitter = null;
                product.SubmitterId = submitterId;

                //await _context.AddAsync(product);
                //await _context.SaveChangesAsync();
                await _productRepository.CreateAsync(product);
                await _productRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                _logger.LogError($"Couldn't create product with name of \"{pdto.Name}\".");
                return BadRequest();
            }
        }
        _logger.LogError("ModelState is not valid.");
        ViewBag.categorySelectList = await GetCategoriesSelectListAsync();
        ViewBag.isAdmin = false;
        return View(pdto);
    }

    // GET: Products/Edit/5
    [Authorize]
    public async Task<IActionResult> Edit(int? id)
    {
        _logger.LogInformation($"GET: Entering Products/Edit.");
        if (id == null)
        {
            _logger.LogError("Failed to fetch product for id \"{Id}\".", id);
            return BadRequest();
        }

        //var product = await _context.Products.FindAsync(id);
        _logger.LogDebug("Fetching product for id \"{Id}\".", id);
        var product = await _productRepository.GetSingleAsync(q => q
            .Where(p => p.Id == id)
            .Include(p => p.Submitter)
            .Select(p => new Product()
            {
                Name = p.Name,
                Price = p.Price,
                Category = new Category() { Name = p.Category.Name },
                Description = p.Description,
                SubmitterId = p.SubmitterId,
                Submitter = new ApplicationUser()
                {
                    UserName = p.Submitter.UserName
                }
            })
        );
        if (product == null)
        {
            _logger.LogError("Could not fetch product for id \"{Id}\".", id);
            return NotFound();
        }
        if (product.SubmitterId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            _logger.LogDebug("User with id \"{userId}\" attempted to edit product "
                + "belonging to user with id \"{ProductOwnerId}\"", product.SubmitterId, id);
            return Unauthorized("Cannot edit a product that is not yours.");
        }

        var pdto = _mapper.Map<Product, ProductCreateDto>(product);
        _logger.LogDebug($"Fetching all categories.");
        var categorySelectList = await GetCategoriesSelectListAsync();
        ViewBag.categorySelectList = categorySelectList;
        ViewBag.isAdmin = false;

        return View(pdto);
    }

    // POST: Products/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(int id, ProductCreateDto pdto)
    {
        _logger.LogInformation($"POST: Entering Products/Edit.");
        if (!_productRepository.Exists(q => q.Where(p => p.Id == id)))
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var product = _mapper.Map<ProductCreateDto, Product>(pdto);

                string submitterId;

                bool userIsAdmin = false;
                if (userIsAdmin)
                {
                    _logger.LogDebug("User is admin");
                    // Get user argument from the form and use the user there.
                    var productSubmitter = await _userRepository.GetSingleAsync(q => q.Where(u => u.UserName == product.Submitter.UserName));
                    if (productSubmitter is null)
                    {
                        _logger.LogError("The user supplied in the form with username \"{Username}\" does not exist", product.Submitter.UserName);
                        return BadRequest("User with name does not exist.");
                    }
                    submitterId = productSubmitter.Id;
                }
                else
                {
                    // Get user argument from session and edit if the user owns the product.
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!await _userRepository.ExistsAsync(q => q.Where(u => u.Id == userId)))
                    {
                        _logger.LogError("The user with id \"{Id}\" does not exist in the "
                            + "database even through the view is authorized", userId);
                        return BadRequest("UserId is invalid. Contact developer");
                    }

                    // Get the existing submitter id for the product from the database.
                    var productExistingSubmitterId = (await _productRepository.GetSingleAsync(q => q
                        .Where(p => p.Id == id)
                        .Select(p => new Product()
                        {
                            SubmitterId = p.SubmitterId
                        })
                    ))!.SubmitterId;
                    if (productExistingSubmitterId != userId)
                    {
                        _logger.LogDebug("User with id \"{userId}\" attempted to edit product "
                            + "belonging to user with id \"{ProductOwnerId}\"", userId, productExistingSubmitterId);
                        return Unauthorized("Cannot edit a product that is not yours.");
                    }
                    submitterId = userId!;
                }
                
                _logger.LogDebug("Updating product.");
                _productRepository.UpdateCertainFields(
                    q => q
                    .Where(p => p.Id == id),
                    setPropertyCalls: s => s
                        .SetProperty(p => p.Name, product.Name)
                        .SetProperty(p => p.Price, product.Price)
                        .SetProperty(p => p.CategoryId, product.CategoryId)
                        .SetProperty(p => p.Description, product.Description)
                        .SetProperty(p => p.SubmitterId, submitterId)
                        .SetProperty(p => p.ModificationDate, DateTime.Now)
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
        ViewBag.isAdmin = false;
        
        return View(pdto);
    }

    // GET: Products/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        _logger.LogInformation($"GET: Entering Products/Delete.");
        if (id == null)
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return BadRequest();
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
            await _productRepository.DeleteCertainEntriesAsync(q => q.Where(p => p.Id == id));
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<IEnumerable<SelectListItem>> GetCategoriesSelectListAsync()
    {
        //if (_memoryCache.TryGetValue($"{nameof(Category)}List", out List<SelectListItem> categoryList)) return categoryList;
        _logger.LogDebug("Attempting from cache getting categoryList");
        List<SelectListItem>? categoryList = await _memoryCache.GetOrCreateAsync($"{nameof(Category)}List", async entry =>
        {
            _logger.LogDebug("Cache miss for categoryList");
            entry.SetAbsoluteExpiration(TimeSpan.FromHours(6));
            entry.SetSlidingExpiration(TimeSpan.FromMinutes(1));
            return (await _categoryRepository
                .GetAllAsync())
                .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() })
                .ToList();
        });

        //categoryList = (await _categoryRepository.GetAllAsync())
        //    .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() }).ToList();

        //_memoryCache.Set("CategoryList", categoryList, TimeSpan.FromSeconds(20));

        return categoryList;
    }
}
