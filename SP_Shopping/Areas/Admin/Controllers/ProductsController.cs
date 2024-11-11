using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Areas.Admin.Dtos.Product;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.MessageHandler;
using System.Security.Claims;

namespace SP_Shopping.Areas.Admin.Controllers;


[Authorize(Roles = "Admin")]
[Area("Admin")]
public class ProductsController : Controller
{
    private readonly ILogger<ProductsController> _logger;
    private readonly IMapper _mapper;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepositoryCaching<Category> _categoryRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IImageHandlerDefaulting<ProductImageKey> _productImageHandler;
    private readonly IMessageHandler _messageHandler;

    public ProductsController
    (
        ILogger<ProductsController> logger,
        IMapper mapper,
        IRepository<Product> productRepository,
        IRepositoryCaching<Category> categoryRepository,
        IRepository<ApplicationUser> userRepository,
        IImageHandlerDefaulting<ProductImageKey> productImageHandler,
        IMessageHandler messageHandler
    )
    {
        _logger = logger;
        _mapper = mapper;
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _userRepository = userRepository;
        _productImageHandler = productImageHandler;
        _messageHandler = messageHandler;
    }

    // GET: Products
    public async Task<IActionResult> Index(string? query, string? type, [FromQuery] bool? sort)
    {
        _logger.LogInformation("GET: Entering Admin/Products.");

        Func<IQueryable<AdminProductDetailsDto>, IQueryable<AdminProductDetailsDto>> queryFilter = q => q;
        Func<IQueryable<AdminProductDetailsDto>, IQueryable<AdminProductDetailsDto>> sortFilter = q => q
            .OrderByDescending(p => p.InsertionDate)
            .ThenByDescending(p => p.ModificationDate);

        try
        {
            if (!string.IsNullOrWhiteSpace(type))
            {
                if (!string.IsNullOrWhiteSpace(query))
                {
                    queryFilter = type switch
                    {
                        nameof(AdminProductDetailsDto.Id) => q => q.Where(p => p.Name.Contains(query)),
                        nameof(AdminProductDetailsDto.Name) => q => q.Where(p => p.Name.Contains(query)),
                        nameof(AdminProductDetailsDto.Price) => decimal.TryParse(query, out var queryNumber) ? q => q.Where(p => p.Price == queryNumber) : q => q,
                        nameof(AdminProductDetailsDto.CategoryName) => q => q.Where(p => p.CategoryName != null && p.CategoryName.Contains(query)),
                        nameof(AdminProductDetailsDto.Description) => q => q.Where(p => p.Description != null && p.Description.Contains(query)),
                        nameof(AdminProductDetailsDto.SubmitterId) => q => q.Where(p => p.SubmitterName.Contains(query)),
                        nameof(AdminProductDetailsDto.SubmitterName) => q => q.Where(p => p.SubmitterName.Contains(query)),
                        nameof(AdminProductDetailsDto.InsertionDate) => q => q.Where(p => p.InsertionDate.ToString().Contains(query)),
                        nameof(AdminProductDetailsDto.ModificationDate) => q => q.Where(p => p.ModificationDate != null && p.ModificationDate.ToString().Contains(query)),
                        _ => throw new NotImplementedException($"{type} is invalid")
                    };
                }

                sort ??= false;
                sortFilter = type switch
                {
                    nameof(AdminProductDetailsDto.Id) => (bool)sort ? q => q.OrderBy(p => p.Id) : q => q.OrderByDescending(p => p.Id),
                    nameof(AdminProductDetailsDto.Name) => (bool)sort ? q => q.OrderBy(p => p.Name) : q => q.OrderByDescending(p => p.Name),
                    nameof(AdminProductDetailsDto.Price) => (bool)sort ? q => q.OrderBy(p => p.Price) : q => q.OrderByDescending(p => p.Price),
                    nameof(AdminProductDetailsDto.CategoryName) => (bool)sort ? q => q.OrderBy(p => p.CategoryName) : q => q.OrderByDescending(p => p.CategoryName),
                    nameof(AdminProductDetailsDto.Description) => (bool)sort ? q => q.OrderBy(p => p.Description) : q => q.OrderByDescending(p => p.Description),
                    nameof(AdminProductDetailsDto.SubmitterId) => (bool)sort ? q => q.OrderBy(p => p.SubmitterId) : q => q.OrderByDescending(p => p.SubmitterId),
                    nameof(AdminProductDetailsDto.SubmitterName) => (bool)sort ? q => q.OrderBy(p => p.SubmitterName) : q => q.OrderByDescending(p => p.SubmitterName),
                    nameof(AdminProductDetailsDto.InsertionDate) => (bool)sort ? q => q.OrderBy(p => p.InsertionDate) : q => q.OrderByDescending(p => p.InsertionDate),
                    nameof(AdminProductDetailsDto.ModificationDate) => (bool)sort ? q => q.OrderBy(p => p.ModificationDate) : q => q.OrderByDescending(p => p.ModificationDate),
                    _ => throw new NotImplementedException($"{type} is invalid")
                };

            }
        }
        catch (NotImplementedException ex)
        {
            return BadRequest(ex.Message);
        }

        _logger.LogDebug("Fetching product information matching search term.");
        var pdtoList = await _productRepository.GetAllAsync(q =>
            _mapper.ProjectTo<AdminProductDetailsDto>(q)
                .Take(20)
                ._(queryFilter)
                ._(sortFilter)
        );


        return View(pdtoList);
        
    }

    // GET: Products/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        _logger.LogInformation("GET: Entering Admin/Products/Details.");
        if (id == null)
        {
            _logger.LogError("The specified id \"{Id}\" for Product/Details does not exist.", id);
            return BadRequest("Required parameter id not specified");
        }

        AdminProductDetailsDto? pdto = await _productRepository.GetSingleAsync(q => 
            _mapper.ProjectTo<AdminProductDetailsDto>(q
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
    public async Task<IActionResult> Create()
    {
        _logger.LogInformation($"GET: Entering Admin/Products/Details.");
        var pdto = _mapper.Map<Product, AdminProductCreateDto>(new Product());
        _logger.LogDebug($"Fetching all categories and users.");
        IEnumerable<SelectListItem> categorySelectList = await GetCategoriesSelectListAsync();
        IEnumerable<SelectListItem> userSelectList = await GetUsersSelectListAsync();
        ViewBag.categorySelectList = categorySelectList;
        ViewBag.userSelectList = userSelectList;
        return View(pdto);
    }

    // POST: Products/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminProductCreateDto pdto)
    {
        _logger.LogInformation($"POST: Entering Admin/Products/Create.");
        if (ModelState.IsValid)
        {
            try
            {
                // Validate image
                if (pdto.ProductImage is not null) {
                    if (!pdto.ProductImage.ContentType.StartsWith("image"))
                    {
                        _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "The file must be an image" });
                        return RedirectToAction(nameof(Create));
                    }
                    if (pdto.ProductImage.Length > 1_500_000)
                    {
                        _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = $"The file is too large. Must be below {1_500_000M / 1_000_000}MB in size." });
                        return RedirectToAction(nameof(Create));
                    }
                    using var stream = pdto.ProductImage.OpenReadStream();
                    if (!await _productImageHandler.ValidateImageAsync(stream))
                    {
                        _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "The Image format is invalid." });
                        return RedirectToAction(nameof(Create));
                    }
                }

                _logger.LogDebug($"Creating product.");
                Product product = _mapper.Map<AdminProductCreateDto, Product>(pdto);
                product.InsertionDate = DateTime.Now;

                if (product.SubmitterId is null)
                {
                    var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!await _userRepository.ExistsAsync(q => q.Where(u => u.Id == UserId)))
                    {
                        return BadRequest("UserId is invalid. Contact developer");
                    }
                    // Set submitter to null and manually set Submitter foreign key
                    // to avoid generating new ApplicationUser with auto-generated id.
                    product.Submitter = null;
                    product.SubmitterId = UserId!;
                }
                else
                {
                    if (!await _userRepository.ExistsAsync(q => q.Where(u => u.Id == product.SubmitterId)))
                    {
                        return BadRequest("UserId is invalid. Contact developer");
                    }
                }

                await _productRepository.CreateAsync(product);
                await _productRepository.SaveChangesAsync();

                if (pdto.ProductImage is not null)
                {
                    using var ImageStream = pdto.ProductImage.OpenReadStream();
                    if (!await _productImageHandler.SetImageAsync(new(product.Id), ImageStream))
                    {
                        _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "Couldn't set the image. The Image format is invalid." });
                        return RedirectToAction(nameof(Create));
                    }
                }

                return RedirectToAction(nameof(Details), new { id = product.Id });
            }
            catch (DbUpdateException)
            {
                _logger.LogError("Couldn't create product with name of \"{Product}\".", pdto.Name);
                _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Couldn't create product" });
                return RedirectToAction(nameof(Create));
            }
        }
        _logger.LogError("ModelState is not valid.");
        _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "Form is invalid. Please try again" });
        // Create view instead of redirecting to GET to save current form field states.
        _logger.LogDebug($"Fetching all categories and users.");
        IEnumerable<SelectListItem> categorySelectList = await GetCategoriesSelectListAsync();
        IEnumerable<SelectListItem> userSelectList = await GetUsersSelectListAsync();
        ViewBag.categorySelectList = categorySelectList;
        ViewBag.userSelectList = userSelectList;
        return View(pdto);
    }

    // GET: Products/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        _logger.LogInformation($"GET: Entering Admin/Products/Edit.");
        if (id == null)
        {
            _logger.LogError("Failed to fetch product for id \"{Id}\".", id);
            return BadRequest();
        }

        //var product = await _context.Products.FindAsync(id);
        _logger.LogDebug("Fetching product for id \"{Id}\".", id);
        var pdto = await _productRepository.GetSingleAsync(q =>
            _mapper.ProjectTo<AdminProductCreateDto>(q
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

        _logger.LogDebug($"Fetching all categories and users.");
        IEnumerable<SelectListItem> categorySelectList = await GetCategoriesSelectListAsync();
        IEnumerable<SelectListItem> userSelectList = await GetUsersSelectListAsync();
        ViewBag.categorySelectList = categorySelectList;
        ViewBag.userSelectList = userSelectList;

        return View(pdto);
    }

    // POST: Products/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminProductCreateDto pdto)
    {
        _logger.LogInformation($"POST: Entering Admin/Products/Edit.");
        if (!_productRepository.Exists(q => q.Where(p => p.Id == id)))
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var product = _mapper.Map<AdminProductCreateDto, Product>(pdto);

                _logger.LogDebug("Updating product.");
                if (product.SubmitterId is not null)
                {
                    await _productRepository.UpdateCertainFieldsAsync(q => q
                        .Where(p => p.Id == id),
                        setPropertyCalls: s => s
                            .SetProperty(p => p.Name, product.Name)
                            .SetProperty(p => p.Price, product.Price)
                            .SetProperty(p => p.CategoryId, product.CategoryId)
                            .SetProperty(p => p.Description, product.Description)
                            .SetProperty(p => p.SubmitterId, product.SubmitterId)
                            .SetProperty(p => p.ModificationDate, DateTime.Now)
                    );
                }
                else
                {
                    await _productRepository.UpdateCertainFieldsAsync(q => q
                        .Where(p => p.Id == id),
                        setPropertyCalls: s => s
                            .SetProperty(p => p.Name, product.Name)
                            .SetProperty(p => p.Price, product.Price)
                            .SetProperty(p => p.CategoryId, product.CategoryId)
                            .SetProperty(p => p.Description, product.Description)
                            .SetProperty(p => p.ModificationDate, DateTime.Now)
                    );
                }

                // Adding image
                if (pdto.ProductImage is not null) {
                    if (!pdto.ProductImage.ContentType.StartsWith("image"))
                    {
                        _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "The file must be an image" });
                        return RedirectToAction(nameof(Edit));
                    }
                    if (pdto.ProductImage.Length > 1_500_000)
                    {
                        _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = $"The file is too large. Must be below {1_500_000M / 1_000_000}MB in size." });
                        return RedirectToAction(nameof(Edit));
                    }
                    using var ImageStream = pdto.ProductImage.OpenReadStream();
                    if (!await _productImageHandler.SetImageAsync(new(id), ImageStream))
                    {
                        _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "The Image format is invalid." });
                        return RedirectToAction(nameof(Edit));
                    }
                }

            }
            catch (DbUpdateConcurrencyException dbuce)
            {
                _logger.LogError(dbuce, "The product with the id of \"{Id}\" could not be updated.", id);
                _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Couldn't update product" });
                return RedirectToAction(nameof(Details), new { id });
            }
        }
        
        return RedirectToAction(nameof(Edit), new { id });
    }

    // GET: Products/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        _logger.LogInformation($"GET: Entering Admin/Products/Delete.");
        if (id == null)
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return BadRequest();
        }

        _logger.LogDebug("Fetching product for id \"{Id}\".", id);
        var pdto = await _productRepository.GetSingleAsync(q =>
            _mapper.ProjectTo<AdminProductDetailsDto>(q
                .Where(p => p.Id == id)
            )
        );

        if (pdto == null)
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return NotFound();
        }

        return View(pdto);
    }

    // POST: Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        _logger.LogInformation($"POST: Entering Admin/Products/Delete.");
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

        //_context.Products.Remove(product);
        _logger.LogDebug("Deleting product with id for \"{Id}\" from database", id);
        _productRepository.Delete(product);
        if (await _productRepository.SaveChangesAsync() > 0)
        {
            _productImageHandler.DeleteImage(new(id));
        }

        return RedirectToAction("Index");
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    public async Task<IActionResult> ResetImage(int id)
    {

        _logger.LogInformation($"POST: Entering Admin/Products/ResetImage.");
        _logger.LogDebug("Fetching product for id \"{Id}\".", id);
        var productInfo = await _productRepository.GetSingleAsync(q => q
            .Where(p => p.Id == id)
            .Select(p => new 
            {
                p.SubmitterId
            })
        );

        if (productInfo is null)
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return NotFound($"The product with the passed id of \"{id}\" does not exist.");
        }

        _logger.LogDebug("Deleting image for product with id \"{Id}\"", id);
        _productImageHandler.DeleteImage(new(id));

        return Redirect(Url.Action(nameof(Edit), new { id }) ?? @"/");
        
    }

    private async Task<IEnumerable<SelectListItem>> GetCategoriesSelectListAsync()
    {
        return await _categoryRepository.GetAllAsync(nameof(GetCategoriesSelectListAsync), q => q
            .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() })
        );
    }

    private async Task<IEnumerable<SelectListItem>> GetUsersSelectListAsync()
    {
        return await _userRepository.GetAllAsync(q => q
            .Select(u => new SelectListItem { Text = u.UserName, Value = u.Id })
        );
    }
}
