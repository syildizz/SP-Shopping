using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SP_Shopping.Areas.Admin.Controllers;
using SP_Shopping.Areas.Admin.Dtos.Product;
using SP_Shopping.Models;
using SP_Shopping.Service;
using SP_Shopping.Test.TestingUtilities;
using SP_Shopping.Utilities.Filter;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.MessageHandler;
using System.Reflection;
using System.Security.Claims;

namespace SP_Shopping.Test.Admin.Controllers;

[TestClass]
public class AdminProductsControllerTests
{

    private readonly ILogger<ProductsController> _logger;
    private readonly IMapper _mapper;
    private readonly IShoppingServices _shoppingServices;
    private readonly IImageHandlerDefaulting<ProductImageKey> _productImageHandler;
    private readonly IMessageHandler _messageHandler;
    private readonly ProductsController _adminProductsController;

    public AdminProductsControllerTests()
    {
        _logger = new NullLogger<ProductsController>();
        _mapper = A.Fake<IMapper>();
        _shoppingServices = A.Fake<IShoppingServices>();
        _productImageHandler = A.Fake<IImageHandlerDefaulting<ProductImageKey>>();
        _messageHandler = new MessageHandler();

        // SUT

        _adminProductsController = new ProductsController
        (
            logger: _logger,
            mapper: _mapper,
            shoppingServices: _shoppingServices,
            productImageHandler: _productImageHandler,
            messageHandler: _messageHandler
        );

        var fakeUser = new ClaimsPrincipal
        ([
            new ClaimsIdentity
            ([
                new Claim(ClaimTypes.NameIdentifier, "8008-12213-absd-32h9-blablabla"),
                new Claim(ClaimTypes.Name, "Faker"),
                new Claim(ClaimTypes.Email, "faker@faker.fk"),
                new Claim(ClaimTypes.Role, "Admin")
                // other required and custom claims
           ],"TestAuthentication")
        ]);

        _adminProductsController.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
            {
                // Set fake user
                User = fakeUser
            }
        };

        // Set fake TempData
        _adminProductsController.TempData = new TempDataDictionary(_adminProductsController.HttpContext, A.Fake<ITempDataProvider>());
    }

    [DataRow("Details", (Type[])[typeof(int?)])]
    [DataRow("Create", (Type[])[])]
    [DataRow("Create", (Type[])[typeof(AdminProductCreateDto)])]
    [DataRow("Edit", (Type[])[typeof(int?)])]
    [DataRow("Edit", (Type[])[typeof(int?), typeof(AdminProductCreateDto)])]
    [DataRow("Delete", (Type[])[typeof(int?)])]
    [DataRow("DeleteConfirmed", (Type[])[typeof(int?)])]
    [DataRow("ResetImage", (Type[])[typeof(int?)])]
    [DataTestMethod]
    public void AdminProductController_All_Succeeds_WhenAuthorizedAsAdmin(string methodName, Type[] types)
    {
        // Arrange
        var controller = typeof(ProductsController);
        var action = controller.GetMethod(methodName, types);
        // Act
        var hasAuthorization = AttributeHandler.HasAuthorizationAttributes(controller, action, checkRole: "Admin");
        // Assert
        Assert.IsTrue(hasAuthorization, $"Action {methodName} should be authorized as admin but it isn't");
    }

    [DataRow("Details", (Type[])[typeof(int?)])]
    [DataRow("Edit", (Type[])[typeof(int?)])]
    [DataRow("Edit", (Type[])[typeof(int?), typeof(AdminProductCreateDto)])]
    [DataRow("Delete", (Type[])[typeof(int?)])]
    [DataRow("DeleteConfirmed", (Type[])[typeof(int?)])]
    [DataRow("ResetImage", (Type[])[typeof(int?)])]
    [DataTestMethod]
    public void ProductsController_Some_Fails_WhenIdIsNull_WithBadRequest(string methodName, Type[] types)
    {
        // Arrange
        var controller = typeof(ProductsController);
        var action = controller.GetMethod(methodName, types);
        const string checkedArgument = "id";
        // Act
        var attributes = action?.GetCustomAttribute<IfArgNullBadRequestFilter>(false);
        // Assert
        Assert.IsTrue(attributes is not null and IfArgNullBadRequestFilter, $"Action {methodName} does not have {nameof(IfArgNullBadRequestFilter)} attribute even though it should");
        Assert.IsTrue(attributes.argument is checkedArgument, $"Action {methodName} has {nameof(IfArgNullBadRequestFilter)} attribute but it checks for {checkedArgument} instead of id");
    }


    #region Details

    [TestMethod]
    public async Task AdminProductsController_Details_Succeeds_WithViewResult()
    {
        // Arrange 
            // Id is not null
        const int id = 0;
            // Id exists, read succeeds
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductDetailsDto>>>._))
            .Returns(A.Fake<AdminProductDetailsDto>());
        // Act
        IActionResult result = await _adminProductsController.Details(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<AdminProductDetailsDto>(viewResult.Model);
            // Must have accessed database
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductDetailsDto>>>._))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task AdminProductsController_Details_Fails_WhenProductDoesntExist_WithNotFound()
    {
        // Arrange 
            // Id is not null
        const int id = 0;
            // Id does NOT exist, read NOT succeeds.
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductDetailsDto>>>._))
            .Returns((AdminProductDetailsDto?)null);
        // Act
        IActionResult result = await _adminProductsController.Details(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
            // Must have called database
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductDetailsDto>>>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion Details

    #region Create

    [TestMethod]
    public async Task AdminProductsController_CreateGet_Succeeds_WithViewResult()
    {
        // Arrange
            // SelectList exists
        List<SelectListItem> selectListItems = (List<SelectListItem>)A.CollectionOfFake<SelectListItem>(4);
        A.CallTo(() => _shoppingServices.Category.GetAllAsync(A<string>._, A<Func<IQueryable<Category>, IQueryable<SelectListItem>>>._))
            .Returns(selectListItems);
        // Act
        IActionResult result = await _adminProductsController.Create();
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<AdminProductCreateDto>(viewResult.Model);
            // ViewData is correct
        Assert.IsInstanceOfType<IEnumerable<SelectListItem>>(viewResult.ViewData["categorySelectList"]);
        var viewResultViewDataSelectList = (IEnumerable<SelectListItem>)viewResult.ViewData["categorySelectList"]!;
        Assert.IsTrue(viewResultViewDataSelectList.Count() == selectListItems.Count);
    }

    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("\t")]
    [DataTestMethod]
    public async Task AdminProductsController_CreatePost_Succeeds_WhenSubmitterIdNone_WithRedirect(string? submitterId)
    {
        // Arrange
            // Modelstate is correct
            // SubmitterId is null or empty
        var pdto = A.Fake<AdminProductCreateDto>();
        pdto.SubmitterId = submitterId;
            // Create succeeds
        A.CallTo(() => _shoppingServices.Product.TryCreateAsync(A<Product>._, An<IFormFile?>._))
            .Returns((true, null));
        // Act
        IActionResult result = await _adminProductsController.Create(pdto);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Details");
        Assert.IsNotNull((int?)redirectResult.RouteValues?["id"]);
        // Create attempted
        A.CallTo(() => _shoppingServices.Product.TryCreateAsync(A<Product>._, An<IFormFile?>._))
            .MustHaveHappened();
    }

    [TestMethod]
    public async Task AdminProductsController_CreatePost_Succeeds_WhenSubmitterIdExist_WithRedirect()
    {
        // Arrange
            // Modelstate is correct
            // SubmitterId does exist
        var pdto = A.Fake<AdminProductCreateDto>();
        pdto.SubmitterId = "id";
        A.CallTo(() => _shoppingServices.User.ExistsAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(true);
            // Create succeeds
        A.CallTo(() => _shoppingServices.Product.TryCreateAsync(A<Product>._, An<IFormFile?>._))
            .Returns((true, null));
        // Act
        IActionResult result = await _adminProductsController.Create(pdto);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Details");
        Assert.IsNotNull((int?)redirectResult.RouteValues?["id"]);
            // Create atempted
        A.CallTo(() => _shoppingServices.Product.TryCreateAsync(A<Product>._, An<IFormFile?>._))
            .MustHaveHappened();
    }

    [TestMethod]
    public async Task AdminProductsController_CreatePost_Fails_WhenModelStateIsNotValid_WithViewResult_WithWarningMessage()
    {
        // Arrange
            // Modelstate is NOT correct
        _adminProductsController.ModelState.AddModelError("CategoryId", "The CategoryId for this product is invalid");
            // SelectList exists
        List<SelectListItem> selectListItems = (List<SelectListItem>)A.CollectionOfFake<SelectListItem>(4);
        A.CallTo(() => _shoppingServices.Category.GetAllAsync(A<string>._, A<Func<IQueryable<Category>, IQueryable<SelectListItem>>>._))
            .Returns(selectListItems);
        // Act
        IActionResult result = await _adminProductsController.Create(A.Fake<AdminProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<AdminProductCreateDto>(viewResult.Model);
            // Message exists and is correct
        Assert.IsTrue(_messageHandler.Peek(_adminProductsController.TempData)?.Any(m => m.Type is Message.MessageType.Warning));
            // ViewData is correct
        Assert.IsInstanceOfType<IEnumerable<SelectListItem>>(viewResult.ViewData["categorySelectList"]);
        var viewResultViewDataSelectList = (IEnumerable<SelectListItem>)viewResult.ViewData["categorySelectList"]!;
        Assert.IsTrue(viewResultViewDataSelectList.Count() == selectListItems.Count);
    }

    [TestMethod]
    public async Task AdminProductsController_CreatePost_Fails_WhenSubmitterIdNotExist_WithRedirect()
    {
        // Arrange
            // Modelstate is correct
            // SubmitterId does NOT exist
        var pdto = A.Fake<AdminProductCreateDto>();
        pdto.SubmitterId = "id";
            A.CallTo(() => _shoppingServices.User.ExistsAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(false);
            // SelectList exists
        List<SelectListItem> selectListItems = (List<SelectListItem>)A.CollectionOfFake<SelectListItem>(4);
        A.CallTo(() => _shoppingServices.Category.GetAllAsync(A<string>._, A<Func<IQueryable<Category>, IQueryable<SelectListItem>>>._))
            .Returns(selectListItems);
        // Act
        IActionResult result = await _adminProductsController.Create(pdto);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsTrue(viewResult.Model is AdminProductCreateDto);
            // Message exists and is correct
        Assert.IsTrue(_messageHandler.Peek(_adminProductsController.TempData)?.Any(m => m.Type is Message.MessageType.Warning));
            // ViewData is correct
        Assert.IsInstanceOfType<IEnumerable<SelectListItem>>(viewResult.ViewData["categorySelectList"]);
        var viewResultViewDataSelectList = (IEnumerable<SelectListItem>)viewResult.ViewData["categorySelectList"]!;
        Assert.IsTrue(viewResultViewDataSelectList.Count() == selectListItems.Count);
    }

    [TestMethod]
    public async Task AdminProductsController_CreatePost_Fails_WhenProductCannotBeCreated_WithViewResult_WithWarningMessage()
    {
        // Arrange
            // Modelstate is correct
            // SubmitterId does exist
        var pdto = A.Fake<AdminProductCreateDto>();
        pdto.SubmitterId = "id";
            A.CallTo(() => _shoppingServices.User.ExistsAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(true);
            // Create NOT succeeds
        A.CallTo(() => _shoppingServices.Product.TryCreateAsync(A<Product>._, An<IFormFile?>._))
            .Returns((false, [ new Message { Type = Message.MessageType.Error, Content = "blabla" }]));
        // Act
        IActionResult result = await _adminProductsController.Create(pdto);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Create");
            // Must have accessed database
        A.CallTo(() => _shoppingServices.Product.TryCreateAsync(A<Product>._, An<IFormFile?>._))
            .MustHaveHappened();
            // Message error
        Assert.IsTrue(_messageHandler.Peek(_adminProductsController.TempData)?.Any(m => m.Type is Message.MessageType.Error), "Expected error mesage was not returned");
    }

    #endregion Create

    #region Edit

    [TestMethod]
    public async Task AdminProductsController_EditGet_Succeeds_WithViewResult()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // SelectList exists
        List<SelectListItem> selectListItems = Enumerable.Range(0, 4).Select(s => new SelectListItem()).ToList();
        A.CallTo(() => _shoppingServices.Category.GetAllAsync(A<string>._, A<Func<IQueryable<Category>, IQueryable<SelectListItem>>>._))
            .Returns(selectListItems);
            // Id exists, Product is found
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductCreateDto>>>._))
            .Returns(A.Fake<AdminProductCreateDto>());
        // Act
        IActionResult result = await _adminProductsController.Edit(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<AdminProductCreateDto>(viewResult.Model);

            // Must have called the database
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductCreateDto>>>._))
            .MustHaveHappenedOnceOrMore();

            // ViewData is valid
        Assert.IsInstanceOfType<IEnumerable<SelectListItem>>(viewResult.ViewData["categorySelectList"]);
        var viewResultViewDataSelectList = (IEnumerable<SelectListItem>)viewResult.ViewData["categorySelectList"]!;
        Assert.IsTrue(viewResultViewDataSelectList.Count() == selectListItems.Count);
    }

    [TestMethod]
    public async Task AdminProductsController_EditGet_Fails_WhenProductDoesntExist_WithNotFound()
    {
        // Arrange 
            // Id is not null
        const int id = 0;
            // Id does NOT exist, product is NOT found
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductCreateDto>>>._))
            .Returns((AdminProductCreateDto?)null);
        // Act
        IActionResult result = await _adminProductsController.Edit(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
            // Must have accessed the database
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductCreateDto>>>._))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task AdminProductsController_EditPost_Succeeds_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 3_242_598;
            // Id exists
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(true);
            // Modelstate is valid
            // Update succeeds
        A.CallTo(() => _shoppingServices.Product.TryUpdateAsync(A<Product>._, An<IFormFile?>._))
            .Returns((true, null));
        // Act
        IActionResult result = await _adminProductsController.Edit(id, A.Fake<AdminProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Edit");
        Assert.IsTrue((int?)redirectResult.RouteValues?["id"] is not null and id);
            // Update attempted
        A.CallTo(() => _shoppingServices.Product.TryUpdateAsync(A<Product>._, An<IFormFile?>._))
            .MustHaveHappened();
    }

    [TestMethod]
    public async Task AdminProductsController_EditPost_Fails_WhenProductNotExist_WithNotFound()
    {
        // Arrange
            // Id is not null
        const int id = 3_242_598;
            // Id does NOT exist
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _adminProductsController.Edit(id, A.Fake<AdminProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
    }

    [TestMethod]
    public async Task AdminProductsController_EditPost_Fails_WhenModelStateNotValid_WithRedirect_WithWarningMessage()
    {
        // Arrange
            // Id is not null
        const int id = 3_242_598;
            // Id exists
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(true);
            // Modelstate is NOT valid
        _adminProductsController.ModelState.AddModelError("CategoryId", "The CategoryId for this product is invalid");
        // Act
        IActionResult result = await _adminProductsController.Edit(id, A.Fake<AdminProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Edit");
        Assert.IsTrue((int?)redirectResult.RouteValues?["id"] is not null and id);
            // Check message exists
        Assert.IsTrue(_messageHandler.Peek(_adminProductsController.TempData)?.Any(m => m.Type is Message.MessageType.Warning));
    }

    [TestMethod]
    public async Task AdminProductsController_EditPost_Fails_WhenUpdateFails_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 3_242_598;
            // Id exists
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(true);
            // Modelstate is valid
            // Update does NOT succeed
        A.CallTo(() => _shoppingServices.Product.TryUpdateAsync(A<Product>._, An<IFormFile?>._))
            .Returns((false, [ new Message { Type = Message.MessageType.Error, Content = "blabla" }]));
        // Act
        IActionResult result = await _adminProductsController.Edit(id, A.Fake<AdminProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Edit");
        Assert.IsTrue((int?)redirectResult.RouteValues?["id"] is not null and id);
            // Check message exists
        Assert.IsTrue(_messageHandler.Peek(_adminProductsController.TempData)?.Any(m => m.Type is Message.MessageType.Error));
            // Must have accesses database
        A.CallTo(() => _shoppingServices.Product.TryUpdateAsync(A<Product>._, An<IFormFile?>._))
            .MustHaveHappened();
            // Message error
        Assert.IsTrue(_messageHandler.Peek(_adminProductsController.TempData)?.Any(m => m.Type is Message.MessageType.Error), "Expected error mesage was not returned");
    }

    #endregion Edit

    #region Delete

    [TestMethod]
    public async Task AdminProductsController_DeleteGet_Succeeds_WithViewResult()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is the same as User id
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductDetailsDto>>>._))
            .Returns(A.Fake<AdminProductDetailsDto>());
        // Act
        IActionResult result = await _adminProductsController.Delete(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<AdminProductDetailsDto>(viewResult.Model);
            // Must have called the database
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductDetailsDto>>>._))
            .MustHaveHappenedOnceOrMore();
    }

    [TestMethod]
    public async Task AdminProductsController_DeleteGet_Fails_WhenProductNotFound_WithNotFound()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id does NOT exists, Product is NOT found
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductDetailsDto>>>._))
            .Returns((AdminProductDetailsDto?)null);
        // Act
        IActionResult result = await _adminProductsController.Delete(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
        // Must have called the database
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductDetailsDto>>>._))
            .MustHaveHappened();
    }

    [TestMethod]
    public async Task AdminProductsController_DeletePost_Succeeds_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(A.Fake<Product>());
            // Delete succeeds
        A.CallTo(() => _shoppingServices.Product.TryDeleteAsync(A<Product>._))
            .Returns((true, null));
        // Act
        IActionResult result = await _adminProductsController.DeleteConfirmed(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName is "Index");
        // Delete attempted
        A.CallTo(() => _shoppingServices.Product.TryDeleteAsync(A<Product>._))
            .MustHaveHappened();
    }

    [TestMethod]
    public async Task AdminProductsController_DeletePost_Fails_WhenIdNotExist_WithNotFound()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id does NOT exists, Product is NOT found
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns((Product?)null);
        // Act
        IActionResult result = await _adminProductsController.DeleteConfirmed(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
    }

    [TestMethod]
    public async Task AdminProductsController_DeletePost_Fails_WhenDeleteNotSucceeds_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
        var fakeProduct = A.Fake<Product>();
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(fakeProduct);
            // Delete does NOT succeed
        A.CallTo(() => _shoppingServices.Product.TryDeleteAsync(A<Product>._))
            .Returns((false, [ new Message { Type = Message.MessageType.Error, Content = "blabla" }]));
        // Act
        IActionResult result = await _adminProductsController.DeleteConfirmed(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName is "Delete");
        Assert.IsTrue(redirectResult.RouteValues?["id"] as int? == id);
            // Delete attempted
        A.CallTo(() => _shoppingServices.Product.TryDeleteAsync(A<Product>._))
            .MustHaveHappened();
            // Message error
        Assert.IsTrue(_messageHandler.Peek(_adminProductsController.TempData)?.Any(m => m.Type is Message.MessageType.Error), "Expected error mesage was not returned");
    }

    #endregion Delete

    #region Image

    [TestMethod]
    public async Task AdminProductsController_ResetImage_Succeeds_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(true);
            // Delete image successful
        A.CallTo(() => _productImageHandler.DeleteImage(A<ProductImageKey>._))
            .DoesNothing();
        // Act
        IActionResult result = await _adminProductsController.ResetImage(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName is "Edit");
        Assert.IsTrue(redirectResult.RouteValues?["id"] is id);
            // Must have attempted image delete
        A.CallTo(() => _productImageHandler.DeleteImage(A<ProductImageKey>._))
            .MustHaveHappenedOnceOrMore();
    }

    [TestMethod]
    public async Task AdminProductsController_ResetImage_Fails_WhenIdNotExist_WithNotFound()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id does NOT exists, Product is NOT found
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _adminProductsController.ResetImage(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
            // Mustn't have attempted image delete
        A.CallTo(() => _productImageHandler.DeleteImage(A<ProductImageKey>._))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task AdminProductsController_ResetImage_Fails_WhenDeleteImageFails_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is the same as User id
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(true);
            // Delete image NOT successful
        A.CallTo(() => _productImageHandler.DeleteImage(A<ProductImageKey>._))
            .Throws<Exception>(e => new Exception("Test exception", e));
        // Act
        IActionResult result = await _adminProductsController.ResetImage(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName is "Edit");
        Assert.IsTrue(redirectResult.RouteValues?["id"] is id);
            // Must have attempted image delete
        A.CallTo(() => _productImageHandler.DeleteImage(A<ProductImageKey>._))
            .MustHaveHappenedOnceOrMore();
            // Message not success
        Assert.IsTrue(_messageHandler.Peek(_adminProductsController.TempData)?.Any(m => m.Type is Message.MessageType.Error));
    }


    #endregion Image

}
