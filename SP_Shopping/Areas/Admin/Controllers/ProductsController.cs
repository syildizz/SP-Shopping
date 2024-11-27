using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuGet.Protocol;
using SP_Shopping.Areas.Admin.Dtos.Product;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Service;
using SP_Shopping.Utilities;
using SP_Shopping.Utilities.Filter;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.MessageHandler;
using SP_Shopping.Utilities.ModelStateHandler;
using System.Security.Claims;
using System.Text.Json;

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
    private readonly ProductService _productService;

    public ProductsController
    (
        ILogger<ProductsController> logger,
        IMapper mapper,
        IRepository<Product> productRepository,
        IRepositoryCaching<Category> categoryRepository,
        IRepository<ApplicationUser> userRepository,
        IImageHandlerDefaulting<ProductImageKey> productImageHandler,
        IMessageHandler messageHandler,
        ProductService productService
    )
    {
        _logger = logger;
        _mapper = mapper;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _productImageHandler = productImageHandler;
        _messageHandler = messageHandler;
        _categoryRepository = categoryRepository;
        _productService = productService;
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
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> Details(int? id)
    {
        _logger.LogInformation("GET: Entering Admin/Products/Details.");

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
        ViewBag.categorySelectList = categorySelectList;
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
            _logger.LogDebug($"Creating product.");

            if (string.IsNullOrWhiteSpace(pdto.SubmitterId))
            {
                var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                pdto.SubmitterId = UserId;
            }
            else
            {
                if (!await _userRepository.ExistsAsync(q => q.Where(u => u.Id == pdto.SubmitterId)))
                {
                    _logger.LogDebug("{SubmitterId} is not a valid user id.", pdto.SubmitterId);
                    _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = $"{pdto.SubmitterId} is not a valid user id" });
                    _logger.LogDebug($"Fetching all categories.");
                    ViewBag.categorySelectList = await GetCategoriesSelectListAsync();
                    return View(pdto);
                }
            }

            Product product = _mapper.Map<AdminProductCreateDto, Product>(pdto);
            if (!(await _productService.TryCreateAsync(product, pdto.ProductImage)).TryOut(out var errMsgs))
            {
                _logger.LogError("Couldn't create product with name of \"{Product}\".", pdto.Name);
                _messageHandler.Add(TempData, errMsgs!);
                return RedirectToAction(nameof(Create));
            }

            return RedirectToAction(nameof(Details), new { id = product.Id });

        }
        _logger.LogError("ModelState is not valid.");
        _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "Unable to create product" });
        _logger.LogDebug($"Fetching all categories.");
        IEnumerable<SelectListItem> categorySelectList = await GetCategoriesSelectListAsync();
        ViewBag.categorySelectList = categorySelectList;
        return View(pdto);
    }

    // GET: Products/Edit/5
    [ImportModelState]
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> Edit(int? id)
    {
        _logger.LogInformation($"GET: Entering Admin/Products/Edit.");

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

        _logger.LogDebug($"Fetching all categories and users.");
        IEnumerable<SelectListItem> categorySelectList = await GetCategoriesSelectListAsync();
        ViewBag.categorySelectList = categorySelectList;


        return View(pdto);
    }

    // POST: Products/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [IfArgNullBadRequestFilter(nameof(id))]
    [ExportModelState]
    public async Task<IActionResult> Edit(int? id, AdminProductCreateDto pdto)
    {
        _logger.LogInformation($"POST: Entering Admin/Products/Edit.");
        if (!_productRepository.Exists(q => q.Where(p => p.Id == id)))
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var product = _mapper.Map<AdminProductCreateDto, Product>(pdto);
            product.Id = (int)id!;

            _logger.LogDebug("Updating product.");

            if (!(await _productService.TryUpdateAsync(product, pdto.ProductImage)).TryOut(out var errMsgs))
            {
                _logger.LogError("The product with the id of \"{Id}\" could not be updated.", id);
                _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Couldn't update product" });
                _messageHandler.Add(TempData, errMsgs!);
                return RedirectToAction(nameof(Edit), new { id });
            }
            return RedirectToAction(nameof(Edit), new { id });
        }

        _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "Unable to update product" });
        return RedirectToAction(nameof(Edit), new { id });
        
    }

    // GET: Products/Delete/5
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> Delete(int? id)
    {
        _logger.LogInformation($"GET: Entering Admin/Products/Delete.");

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
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> DeleteConfirmed(int? id)
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
        if (!(await _productService.TryDeleteAsync(product)).TryOut(out var errMsgs))
        {
            _logger.LogError("The product with the id of \"{Id}\" could not be deleted.", id);
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Couldn't delete product" });
            _messageHandler.Add(TempData, errMsgs!);
            return RedirectToAction(nameof(Edit), new { id });
        }

        return RedirectToAction("Index");
    }

    [ValidateAntiForgeryToken]
    [HttpPost]
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> ResetImage(int? id)
    {

        _logger.LogInformation($"POST: Entering Admin/Products/ResetImage.");
        _logger.LogDebug("Fetching product for id \"{Id}\".", id);
        var productExists = await _productRepository.ExistsAsync(q => q
            .Where(p => p.Id == id)
        );

        if (!productExists)
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return NotFound($"The product with the passed id of \"{id}\" does not exist.");
        }

        _logger.LogDebug("Deleting image for product with id \"{Id}\"", id);
        _productImageHandler.DeleteImage(new((int)id!));

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
