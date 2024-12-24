using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SP_Shopping.Dtos.Product;
using SP_Shopping.Models;
using SP_Shopping.Service;
using SP_Shopping.Utilities;
using SP_Shopping.Utilities.Filters;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.MessageHandler;
using SP_Shopping.Utilities.ModelStateHandler;
using System.Security.Claims;

namespace SP_Shopping.Controllers;

public class ProductsController(
    ILogger<ProductsController> logger,
    IMapper mapper,
    IShoppingServices shoppingServices,
    IImageHandlerDefaulting<ProductImageKey> productImageHandler,
    IMessageHandler messageHandler
    ) : Controller
{
    private readonly ILogger<ProductsController> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IShoppingServices _shoppingServices = shoppingServices;
    private readonly IImageHandlerDefaulting<ProductImageKey> _productImageHandler = productImageHandler;
    private readonly IMessageHandler _messageHandler = messageHandler;
    private readonly int paginationCount = 5;

    public async Task<IActionResult> Search(string? query)
    {
        _logger.LogInformation("GET: Entering Products/Search.");

        IEnumerable<ProductDetailsDto>? pdtoList = null;
        if (!string.IsNullOrWhiteSpace(query))
        {
            _logger.LogDebug("Fetching product information matching search term.");
            pdtoList = await _shoppingServices.Product.GetAllAsync(q =>
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
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> Details(int? id)
    {

        if (User.IsInRole("Admin")) return RedirectToAction("Details", "Products", new { area = "Admin" });

        _logger.LogInformation("GET: Entering Products/Details.");

        _logger.LogDebug("Fetching \"{Id}\" product information.", id);
        ProductDetailsDto? pdto = await _shoppingServices.Product.GetSingleAsync(q => 
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
        if (User.IsInRole("Admin")) return RedirectToAction("Create", "Products", new { area = "Admin" });

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

        if (!ModelState.IsValid)
        {
            _logger.LogError("ModelState is not valid.");
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "Form is invalid. Please try again" });
            // Create view instead of redirecting to GET to save current form field states.
            _logger.LogDebug($"Fetching all categories.");
            IEnumerable<SelectListItem> categorySelectList = await GetCategoriesSelectListAsync();
            ViewBag.categorySelectList = categorySelectList;
            return View(pdto);
        }

        _logger.LogDebug($"Creating product.");
        Product product = _mapper.Map<ProductCreateDto, Product>(pdto);

        product.SubmitterId = User.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? throw new Exception("ClaimTypes.NameIdentifier doesn't exist somehow. Should be unreachable code");

        if (!(await _shoppingServices.Product.TryCreateAsync(product, pdto.ProductImage)).TryOut(out var errMsgs))
        {
            _logger.LogError("Couldn't create product with name of \"{Product}\".", pdto.Name);
            _messageHandler.Add(TempData, errMsgs!);
            return RedirectToAction(nameof(Create));
        }

        return RedirectToAction(nameof(Details), new { id = product.Id });
    }

    // GET: Products/Edit/5
    [Authorize]
    [IfArgNullBadRequestFilter(nameof(id))]
    [ImportModelState]
    public async Task<IActionResult> Edit(int? id)
    {

        if (User.IsInRole("Admin")) return RedirectToAction("Create", "Products", new { area = "Admin" });

        _logger.LogInformation($"GET: Entering Products/Edit.");

        //var product = await _context.Products.FindAsync(id);
        _logger.LogDebug("Fetching product for id \"{Id}\".", id);
        var pdto = await _shoppingServices.Product.GetSingleAsync(q =>
            _mapper.ProjectTo<ProductCreateDto>(q
                .Where(p => p.Id == id)
            )
        );

        if (pdto == null)
        {
            _logger.LogError("Could not fetch product for id \"{Id}\".", id);
            return NotFound($"Product with id {id} does not exist.");
        }

        string? productSubmitterId = await _shoppingServices.Product.GetSingleAsync(q => q
            .Where(p => p.Id == id)
            .Select(p => p.SubmitterId)
        );

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (productSubmitterId != userId)
        {
            _logger.LogDebug("User with id \"{userId}\" attempted to edit product "
                + "belonging to user with id \"{ProductOwnerId}\"", productSubmitterId, id);
            return Unauthorized("Cannot edit product that is not yours");
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
    [IfArgNullBadRequestFilter(nameof(id))]
    [ExportModelState]
    public async Task<IActionResult> Edit(int? id, ProductCreateDto pdto)
    {
        _logger.LogInformation($"POST: Entering Products/Edit.");

        if (!await _shoppingServices.Product.ExistsAsync(q => q.Where(p => p.Id == id)))
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "Form is invalid" });
            return RedirectToAction(nameof(Edit), new { id });
        }

        var product = _mapper.Map<ProductCreateDto, Product>(pdto);
        product.Id = (int)id!;

        // Get user argument from session and edit if the user owns the product.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Get the existing submitter id for the product from the database.
        string? productExistingSubmitterId = await _shoppingServices.Product.GetSingleAsync(q => q
            .Where(p => p.Id == id)
            .Select(p => p.SubmitterId)
        );
        if (productExistingSubmitterId != userId)
        {
            _logger.LogDebug("User with id \"{userId}\" attempted to edit product "
                + "belonging to user with id \"{ProductOwnerId}\"", userId, productExistingSubmitterId);
            return Unauthorized("Cannot edit product that is not yours");
        }
        
        _logger.LogDebug("Updating product.");
        if(!(await _shoppingServices.Product.TryUpdateAsync(product, pdto.ProductImage)).TryOut(out var errMsgs))
        {
            _logger.LogError("The product with the id of \"{Id}\" could not be updated.", id);
            _messageHandler.Add(TempData, errMsgs!);
            return RedirectToAction(nameof(Edit), new { id });
        }
        return RedirectToAction(nameof(Details), new { id });

    }

    // GET: Products/Delete/5
    [Authorize]
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> Delete(int? id)
    {

        if (User.IsInRole("Admin")) return RedirectToAction("Create", "Products", new { area = "Admin" });

        _logger.LogInformation($"GET: Entering Products/Delete.");

        _logger.LogDebug("Fetching product for id \"{Id}\".", id);
        var pdto = await _shoppingServices.Product.GetSingleAsync(q =>
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
            return Unauthorized("Cannot delete product that is not yours");
        }

        return View(pdto);
    }

    // POST: Products/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize]
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        _logger.LogInformation($"POST: Entering Products/Delete.");

        _logger.LogDebug("Fetching product for id \"{Id}\".", id);
        Product? product = await _shoppingServices.Product.GetSingleAsync(q => q
            .Where(q => q.Id == id)
            .Select(p => new Product() { Id = p.Id, SubmitterId = p.SubmitterId })
        );
        if (product == null)
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return NotFound($"The product with the passed id of \"{id}\" does not exist.");
        }

        // Get user argument from session and delete if the user owns the product.
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Get the existing submitter id for the product from the database.
        if (product.SubmitterId != userId)
        {
            _logger.LogDebug("User with id \"{userId}\" attempted to delete product "
                + "belonging to user with id \"{ProductOwnerId}\"", userId, product.SubmitterId);
            return Unauthorized("Cannot delete product that is not yours");
        }

        //_context.Products.Remove(product);
        _logger.LogDebug("Deleting product with id for \"{Id}\" from database", id);
        if(!(await _shoppingServices.Product.TryDeleteAsync(product)).TryOut(out var errMsgs))
        {
            _messageHandler.Add(TempData, errMsgs!);
            return RedirectToAction(nameof(Delete), new { id });
        }

        return RedirectToAction("Index", "User", new { Id = userId });
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost]
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> ResetImage(int? id)
    {
        _logger.LogInformation($"POST: Entering Products/ResetImage.");

        _logger.LogDebug("Fetching product for id \"{Id}\".", id);
        var product = await _shoppingServices.Product.GetSingleAsync(q => q
            .Where(p => p.Id == id)
            .Select(p => new Product { SubmitterId = p.SubmitterId })
        );

        if (product?.SubmitterId is null)
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return NotFound($"The product with the passed id of \"{id}\" does not exist.");
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (product.SubmitterId != userId)
        {
            _logger.LogDebug("User with id \"{userId}\" attempted to reset the image of product"
                + "belonging to user with id \"{ProductOwnerId}\"", userId, product.SubmitterId);
            return Unauthorized("Cannot delete the image of a product that is not yours");
        }

        _logger.LogDebug("Deleting image for product with id \"{Id}\"", id);
        try
        {
            _productImageHandler.DeleteImage(new((int)id!));
        }
        catch (Exception ex)
        {
#if DEBUG
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Failed to reset image" + " " + ex.Message });
#else
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Failed to reset image" });
#endif
            return RedirectToAction(nameof(Edit), new { id });
        }

        return RedirectToAction(nameof(Edit), new { id });
        
    }

    #region API

    [HttpGet("api/[controller]/[action]/{id?}")]
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> ProductCard(int? id)
    {
        var pdto = await _shoppingServices.Product.GetSingleAsync(q => 
            _mapper.ProjectTo<ProductDetailsDto>(q
                .Where(p => p.Id == id)
            )
        );

        if (pdto is null)
        {
            return NotFound("Not Found");
        }

        return PartialView("_ProductCardPartial", pdto);
    }


    #endregion API

    private async Task<IEnumerable<SelectListItem>> GetCategoriesSelectListAsync()
    {
        return await _shoppingServices.Category.GetAllAsync(nameof(GetCategoriesSelectListAsync), q => q
            .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() })
        );
    }


}
