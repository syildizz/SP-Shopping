using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SP_Shopping.Areas.Admin.Dtos.Product;
using SP_Shopping.Models;
using SP_Shopping.Service;
using SP_Shopping.ServiceDtos.Product;
using SP_Shopping.Utilities;
using SP_Shopping.Utilities.Filters;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.MessageHandler;
using SP_Shopping.Utilities.ModelStateHandler;
using System.Globalization;
using System.Security.Claims;

namespace SP_Shopping.Areas.Admin.Controllers;

[Authorize(Roles = "Admin")]
[Area("Admin")]
public class ProductsController
(
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

    // GET: Products
    public async Task<IActionResult> Index(string? query, string? type, [FromQuery] bool? sort)
    {
        _logger.LogInformation("GET: Entering Admin/Products.");

        string filterQuery = "";
        string orderQuery = "InsertionDate,ModificationDate";

        object? filterValue = null;

        try
        {
            if (!string.IsNullOrWhiteSpace(type))
            {
                if (!string.IsNullOrWhiteSpace(query))
                {
                    (filterQuery, filterValue) = type switch
                    {
                        nameof(AdminProductDetailsDto.Id) =>
                            int.TryParse(query, out var queryNumber)
                                ? ($"{nameof(ProductGetDto.Id)} == @0", queryNumber)
                                : ("false", null),
                        nameof(AdminProductDetailsDto.Name) =>
                            ($"{nameof(ProductGetDto.Name)}.Contains(@0)", query),
                        nameof(AdminProductDetailsDto.Price) =>
                            decimal.TryParse(query, out var queryNumber)
                                ? ($"@0 - 3 <= {nameof(ProductGetDto.Price)} && {nameof(ProductGetDto.Price)} <= @0 + 3", queryNumber)
                                : ("false", null),
                        nameof(AdminProductDetailsDto.CategoryName) =>
                            ($"{nameof(ProductGetDto.Category)}.{nameof(Category.Name)}.Contains(@0)", query as object),
                        nameof(AdminProductDetailsDto.Description) =>
                            ($"{nameof(ProductGetDto.Description)}.Contains(@0)", query),
                        nameof(AdminProductDetailsDto.SubmitterId) =>
                            ($"{nameof(ProductGetDto.SubmitterId)}.Contains(@0)", query),
                        nameof(AdminProductDetailsDto.SubmitterName) =>
                            ($"{nameof(ProductGetDto.Submitter)}.{nameof(ApplicationUser.UserName)}.Contains(@0)", query),
                        nameof(AdminProductDetailsDto.InsertionDate) =>
                            ($"{nameof(ProductGetDto.InsertionDate)}.ToString().Contains(@0)", query),
                        nameof(AdminProductDetailsDto.ModificationDate) =>
                            ($"{nameof(ProductGetDto.ModificationDate)}.ToString().Contains(@0)", query),
                        _ => throw new NotImplementedException($"{type} is invalid")
                    };
                }

                bool _sort = sort ?? false;
                orderQuery = type switch
                {
                    nameof(AdminProductDetailsDto.Id) => $"{nameof(ProductGetDto.Id)}{(_sort ? " desc" : "")}",
                    nameof(AdminProductDetailsDto.Name) => $"{nameof(ProductGetDto.Name)}{(_sort ? " desc" : "")}",
                    nameof(AdminProductDetailsDto.Price) => $"{nameof(ProductGetDto.Price)}{(_sort ? " desc" : "")}",
                    nameof(AdminProductDetailsDto.CategoryName) => $"{nameof(ProductGetDto.Category)}.{nameof(Category.Name)}{(_sort ? " desc" : "")}",
                    nameof(AdminProductDetailsDto.Description) => $"{nameof(ProductGetDto.Description)}{(_sort ? " desc" : "")}",
                    nameof(AdminProductDetailsDto.SubmitterId) => $"{nameof(ProductGetDto.SubmitterId)}{(_sort ? " desc" : "")}",
                    
                    nameof(AdminProductDetailsDto.SubmitterName) => $"{nameof(ProductGetDto.Submitter)}.{nameof(ApplicationUser.UserName)}{(_sort ? " desc" : "")}",
                    nameof(AdminProductDetailsDto.InsertionDate) => $"{nameof(ProductGetDto.InsertionDate)}{(_sort ? " desc" : "")}",
                    nameof(AdminProductDetailsDto.ModificationDate) => $"{nameof(ProductGetDto.ModificationDate)}{(_sort ? " desc" : "")}",
                    _ => throw new NotImplementedException($"{type} is invalid")
                };

            }
        }
        catch (NotImplementedException ex)
        {
            return BadRequest(ex.Message);
        }

        _logger.LogDebug("Fetching product information matching search term.");
        var pdtoList = await _shoppingServices.Product.GetAllAsync<AdminProductDetailsDto>(filterQuery, orderQuery, filterValue, 20);
            //q =>
            //_mapper.ProjectTo<AdminProductDetailsDto>(q
            //    ._(queryFilter)
            //    ._(sortFilter)
            //    .Take(20)
            //)
        //);

        return View(pdtoList);
        
    }

    // GET: Products/Details/5
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> Details(int? id)
    {
        _logger.LogInformation("GET: Entering Admin/Products/Details.");

        AdminProductDetailsDto? pdto = await _shoppingServices.Product.GetByIdAsync<AdminProductDetailsDto>((int)id!);
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
        var pdto = new AdminProductCreateDto();
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
        if (!ModelState.IsValid)
        {
            _logger.LogError("ModelState is not valid.");
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "Unable to create product" });

            _logger.LogDebug($"Fetching all categories.");
            IEnumerable<SelectListItem> categorySelectList = await GetCategoriesSelectListAsync();
            ViewBag.categorySelectList = categorySelectList;

            return View(pdto);
        }

        _logger.LogDebug($"Creating product.");
        if (string.IsNullOrWhiteSpace(pdto.SubmitterId))
        {
            var UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            pdto.SubmitterId = UserId;
        }
        else
        {
            if (!await _shoppingServices.User.ExistsAsync(q => q.Where(u => u.Id == pdto.SubmitterId)))
            {
                _logger.LogDebug("{SubmitterId} is not a valid user id.", pdto.SubmitterId);
                _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = $"{pdto.SubmitterId} is not a valid user id" });
                _logger.LogDebug($"Fetching all categories.");
                ViewBag.categorySelectList = await GetCategoriesSelectListAsync();
                return View(pdto);
            }
        }

        ProductCreateDto pcdto = _mapper.Map<ProductCreateDto>(pdto);
        if (!(await _shoppingServices.Product.TryCreateAsync(pcdto)).TryOut(out int? id, out var errMsgs))
        {
            _logger.LogError("Couldn't create product with name of \"{Product}\".", pcdto.Name);
            _messageHandler.Add(TempData, errMsgs!);
            return RedirectToAction(nameof(Create));
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Products/Edit/5
    [ImportModelState]
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> Edit(int? id)
    {
        _logger.LogInformation($"GET: Entering Admin/Products/Edit.");

        //var product = await _context.Products.FindAsync(id);
        _logger.LogDebug("Fetching product for id \"{Id}\".", id);
        var pdto = await _shoppingServices.Product.GetByIdAsync<AdminProductCreateDto>((int)id!);
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
        if (!await _shoppingServices.Product.ExistsAsync((int)id!))
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Warning, Content = "Unable to update product" });
            return RedirectToAction(nameof(Edit), new { id });
        }

        ProductEditDto pedto = _mapper.Map<ProductEditDto>(pdto);

        _logger.LogDebug("Updating product.");

        if (!(await _shoppingServices.Product.TryUpdateAsync((int)id!, pedto)).TryOut(out var errMsgs))
        {
            _logger.LogError("The product with the id of \"{Id}\" could not be updated.", id);
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Couldn't update product" });
            _messageHandler.Add(TempData, errMsgs!);
            return RedirectToAction(nameof(Edit), new { id });
        }

        return RedirectToAction(nameof(Edit), new { id });

    }

    // GET: Products/Delete/5
    [IfArgNullBadRequestFilter(nameof(id))]
    public async Task<IActionResult> Delete(int? id)
    {
        _logger.LogInformation($"GET: Entering Admin/Products/Delete.");

        _logger.LogDebug("Fetching product for id \"{Id}\".", id);
        var pdto = await _shoppingServices.Product.GetByIdAsync<AdminProductDetailsDto>((int)id!);
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
        var productExists = await _shoppingServices.Product.ExistsAsync((int)id!);
        if (!productExists)
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return NotFound($"The product with the passed id of \"{id}\" does not exist.");
        }

        //_context.Products.Remove(product);
        _logger.LogDebug("Deleting product with id for \"{Id}\" from database", id);
        if (!(await _shoppingServices.Product.TryDeleteAsync((int)id!)).TryOut(out var errMsgs))
        {
            _logger.LogError("The product with the id of \"{Id}\" could not be deleted.", id);
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Couldn't delete product" });
            _messageHandler.Add(TempData, errMsgs!);
            return RedirectToAction(nameof(Delete), new { id });
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
        var productExists = await _shoppingServices.Product.ExistsAsync((int)id!);
        if (!productExists)
        {
            _logger.LogError("The product with the passed id of \"{Id}\" does not exist.", id);
            return NotFound($"The product with the passed id of \"{id}\" does not exist.");
        }

        _logger.LogDebug("Deleting image for product with id \"{Id}\"", id);
        try
        {
            _productImageHandler.DeleteImage(new((int)id!));
        }
        catch (Exception ex)
        {
#if DEBUG
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Failed to reset image" + " " + ex.StackTrace });
#else
            _messageHandler.Add(TempData, new Message { Type = Message.MessageType.Error, Content = "Failed to reset image" });
#endif
            return RedirectToAction(nameof(Edit), new { id });
        }

        return RedirectToAction(nameof(Edit), new { id });
        
    }

    private async Task<IEnumerable<SelectListItem>> GetCategoriesSelectListAsync()
    {
        return await _shoppingServices.Category.GetAllAsync(nameof(GetCategoriesSelectListAsync), q => q
            .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() })
        );
    }

    private async Task<IEnumerable<SelectListItem>> GetUsersSelectListAsync()
    {
        return await _shoppingServices.User.GetAllAsync(q => q
            .Select(u => new SelectListItem { Text = u.UserName, Value = u.Id })
        );
    }
}
