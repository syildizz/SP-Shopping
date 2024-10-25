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
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace SP_Shopping.Controllers;

public class ProductsController : Controller
{
    private readonly ILogger<ProductsController> _logger;
    private readonly IMapper _mapper;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepositoryCaching<Category> _categoryRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IImageHandlerDefaulting<ProductImageKey> _productImageHandler;
    private readonly int paginationCount = 5;

    public ProductsController
    (
        ILogger<ProductsController> logger,
        IMapper mapper,
        IRepository<Product> productRepository,
        IRepositoryCaching<Category> categoryRepository,
        IRepository<ApplicationUser> userRepository,
        IImageHandlerDefaulting<ProductImageKey> productImageHandler
    )
    {
        _logger = logger;
        _mapper = mapper;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        _productImageHandler = productImageHandler;
    }

    // GET: Products
    public async Task<IActionResult> Index()
    {
        _logger.LogInformation("GET: Entering Products/Index.");
        _logger.LogDebug("Fetching all product information.");
        IEnumerable<ProductDetailsDto> pdtoList = await _productRepository.GetAllAsync(q => _mapper.ProjectTo<ProductDetailsDto>(q));
        return View(pdtoList);
    }

    public async Task<IActionResult> Search(string? query)
    {
        _logger.LogInformation("GET: Entering Products/Search.");

        IEnumerable<ProductDetailsDto>? pdtoList = null;
        if (!string.IsNullOrWhiteSpace(query))
        {
            _logger.LogDebug("Fetching product information matching search term.");
            pdtoList = await _productRepository.GetAllAsync(q =>
                _mapper.ProjectTo<ProductDetailsDto>(q
                    .Where(p => p.Name.Contains(query))
                    .OrderByDescending(p => p.InsertionDate)
                    .ThenByDescending(p => p.ModificationDate)
                    .Take(paginationCount)
                )
            );
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

        //var product = await _productRepository
        //    .GetSingleAsync(q => q
        //    .Include(p => p.Category
        //    .Include(p => p.Submitter)
        //    .Where(p => p.Id == id)
        //    .Select(p => new Product()
        //    {
        //        Id = p.Id,
        //        Name = p.Name,
        //        Price = p.Price,
        //        Category = new Category()
        //        {
        //            Name = p.Category.Name
        //        },
        //        Description = p.Description,
        //        SubmitterId = p.SubmitterId,
        //        Submitter = new ApplicationUser()
        //        {
        //            UserName = p.Submitter.UserName
        //        },
        //        InsertionDate = p.InsertionDate,
        //        ModificationDate = p.ModificationDate,
        //    })
        //);
        //_logger.LogDebug("Fetching \"{Id}\" product information.", id);
        //if (product == null)
        //{
        //    _logger.LogError("Failed to fetch product for id \"{Id}\".", id);
        //    return NotFound("Product with id does not exist");
        //}

        //ProductDetailsDto pdto = _mapper.Map<Product, ProductDetailsDto>(product);

        _logger.LogDebug("Fetching \"{Id}\" product information.", id);
        //ProductDetailsDto? pdto = await _mapper.ProjectTo<ProductDetailsDto>(
        //    _productRepository.Get()
        //        .Where(p => p.Id == id)
        //).SingleOrDefaultAsync();
        ProductDetailsDto? pdto = await _productRepository.GetSingleAsync(q => 
            _mapper.ProjectTo<ProductDetailsDto>(q
                .Where(p => p.Id == id)
            )
        );

        if (pdto == null)
        {
            _logger.LogError("Failed to fetch product for id \"{Id}\".", id);
            return NotFound("Product with id does not exist");
        }

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

                // Validate image
                if (pdto.ProductImage is not null) {
                    if (!pdto.ProductImage.ContentType.StartsWith("image"))
                    {
                        return BadRequest("The file must be an image.");
                    }
                    if (pdto.ProductImage.Length > 1_500_000)
                    {
                        return BadRequest($"The file is too large. Must be below {1_500_000M / 1_000_000}MB in size.");
                    }
                    using var stream = pdto.ProductImage.OpenReadStream();
                    if (!await _productImageHandler.ValidateImageAsync(stream))
                    {
                        return BadRequest("The Image format is invalid.");
                    }
                }

                _logger.LogDebug($"Creating product.");
                Product product = _mapper.Map<ProductCreateDto, Product>(pdto);
                product.InsertionDate = DateTime.Now;

                string submitterId;
                var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!await _userRepository.ExistsAsync(q => q.Where(u => u.Id == UserId)))
                {
                    return BadRequest("UserId is invalid. Contact developer");
                }
                submitterId = UserId!;

                // Set submitter to null and manually set Submitter foreign key
                // to avoid generating new ApplicationUser with auto-generated id.
                product.Submitter = null;
                product.SubmitterId = submitterId;

                //await _context.AddAsync(product);
                //await _context.SaveChangesAsync();
                
                await _productRepository.CreateAsync(product);
                await _productRepository.SaveChangesAsync();

                if (pdto.ProductImage is not null)
                {
                    using var ImageStream = pdto.ProductImage.OpenReadStream();
                    if (!await _productImageHandler.SetImageAsync(new(product.Id), ImageStream))
                    {
                        _productRepository.Delete(product);
                        await _productRepository.SaveChangesAsync();
                        return BadRequest("The Image format is invalid.");
                    }
                }

                return RedirectToAction(nameof(Details), new { id = product.Id });
            }
            catch (DbUpdateException)
            {
                _logger.LogError("Couldn't create product with name of \"{Product}\".", pdto.Name);
                return BadRequest();
            }
        }
        _logger.LogError("ModelState is not valid.");
        ViewBag.categorySelectList = await GetCategoriesSelectListAsync();
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
        var pdto = await _productRepository.GetSingleAsync(q =>
            _mapper.ProjectTo<ProductCreateDto>(q
                .Where(p => p.Id == id)
            )
        );

        if (pdto == null)
        {
            _logger.LogError("Could not fetch product for id \"{Id}\".", id);
            return NotFound($"Product with id {id} does not exist.");
        }

        string? productSubmitterId = await _productRepository.GetSingleAsync(q => q
            .Where(p => p.Id == id)
            .Select(p => p.SubmitterId)
        );

        if (productSubmitterId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            _logger.LogDebug("User with id \"{userId}\" attempted to edit product "
                + "belonging to user with id \"{ProductOwnerId}\"", productSubmitterId, id);
            return Unauthorized("Cannot edit a product that is not yours.");
        }

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

                // Get user argument from session and edit if the user owns the product.
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Get the existing submitter id for the product from the database.
                string? productExistingSubmitterId = await _productRepository.GetSingleAsync(q => q
                    .Where(p => p.Id == id)
                    .Select(p => p.SubmitterId)
                );
                if (productExistingSubmitterId != userId)
                {
                    _logger.LogDebug("User with id \"{userId}\" attempted to edit product "
                        + "belonging to user with id \"{ProductOwnerId}\"", userId, productExistingSubmitterId);
                    return Unauthorized("Cannot edit a product that is not yours.");
                }
                submitterId = userId!;
                
                _logger.LogDebug("Updating product.");
                await _productRepository.UpdateCertainFieldsAsync(q => q
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

                // Adding image
                if (pdto.ProductImage is not null) {
                    if (!pdto.ProductImage.ContentType.StartsWith("image"))
                    {
                        return BadRequest("The file must be an image.");
                    }
                    if (pdto.ProductImage.Length > 1_500_000)
                    {
                        return BadRequest($"The file is too large. Must be below {1_500_000M / 1_000_000}MB in size.");
                    }
                    using var ImageStream = pdto.ProductImage.OpenReadStream();
                    if (!await _productImageHandler.SetImageAsync(new(id), ImageStream))
                    {
                        return BadRequest("Image is not of valid format");
                    }
                }

            }
            catch (DbUpdateConcurrencyException dbuce)
            {
                _logger.LogError(dbuce, "The product with the id of \"{Id}\" could not be updated.", id);
                throw;
            }
            return RedirectToAction(nameof(Details), new { id });
        }
        _logger.LogDebug($"Fetching all categories.");
        var categorySelectList = await GetCategoriesSelectListAsync();
        ViewBag.CategorySelectList = categorySelectList;
        
        return View(pdto);
    }

    // GET: Products/Delete/5
    [Authorize]
    public async Task<IActionResult> Delete(int? id)
    {
        _logger.LogInformation($"GET: Entering Products/Delete.");
        if (id == null)
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return BadRequest();
        }

        _logger.LogDebug("Fetching product for id \"{Id}\".", id);
        var pdto = await _productRepository.GetSingleAsync(q =>
            _mapper.ProjectTo<ProductDetailsDto>(q
                .Where(p => p.Id == id)
            )
        );

        if (pdto == null)
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return NotFound();
        }

        if (pdto.SubmitterId != User.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            _logger.LogDebug("User with id \"{userId}\" attempted to delete product "
                + "belonging to user with id \"{ProductOwnerId}\"", pdto.SubmitterId, id);
            return Unauthorized("Cannot delete a product that is not yours.");
        }

        return View(pdto);
    }

    // POST: Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        _logger.LogInformation($"POST: Entering Products/Delete.");
        //var product = await _context.Products.FindAsync(id);
        _logger.LogDebug("Fetching product for id \"{Id}\".", id);
        Product? product = await _productRepository.GetSingleAsync(q => q
            .Where(q => q.Id == id)
            .Select(p => new Product() { Id = p.Id, SubmitterId = p.SubmitterId })
        );
        if (product == null)
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return NotFound($"The product with the passed id of \"{id}\" does not exist.");
        }

        // Get user argument from session and edit if the user owns the product.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Get the existing submitter id for the product from the database.
        if (product.SubmitterId != userId)
        {
            _logger.LogDebug("User with id \"{userId}\" attempted to delete product "
                + "belonging to user with id \"{ProductOwnerId}\"", userId, product.SubmitterId);
            return Unauthorized("Cannot delete a product that is not yours.");
        }

        //_context.Products.Remove(product);
        _logger.LogDebug("Deleting product with id for \"{Id}\" from database", id);
        _productRepository.Delete(product);
        if (await _productRepository.SaveChangesAsync() > 0)
        {
            _productImageHandler.DeleteImage(new(id));
        }

        return RedirectToAction("Index", "User", new { Id = userId });
    }

    private async Task<IEnumerable<SelectListItem>> GetCategoriesSelectListAsync()
    {
        return await _categoryRepository.GetAllAsync(nameof(GetCategoriesSelectListAsync), q => q
            .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() })
        );
    }
}
