using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SP_Shopping.Areas.Admin.Controllers;
using SP_Shopping.Areas.Admin.Dtos.Product;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Service;
using SP_Shopping.Test.TestingUtilities;
using SP_Shopping.Utilities.Filter;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.MessageHandler;
using System.Security.Claims;

namespace SP_Shopping.Test.Admin.Controllers;

[TestClass]
public class AdminProductsControllerTests
{

    private readonly IRepository<Product> _productRepository;
    private readonly ProductService _productService;
    private readonly IRepositoryCaching<Category> _categoryRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductsController> _logger;
    private readonly IImageHandlerDefaulting<ProductImageKey> _productImageHandler;
    private readonly IMessageHandler _messageHandler;
    private readonly ProductsController _adminProductsController;

    public AdminProductsControllerTests()
    {
        _productRepository = A.Fake<IRepository<Product>>();
        _categoryRepository = A.Fake<IRepositoryCaching<Category>>();
        _userRepository = A.Fake<IRepository<ApplicationUser>>();
        _mapper = A.Fake<IMapper>();
        _logger = new NullLogger<ProductsController>();
        _productImageHandler = A.Fake<IImageHandlerDefaulting<ProductImageKey>>();
        _productService = new ProductService(_productRepository, _productImageHandler);
        _messageHandler = new MessageHandler();

        // SUT

        _adminProductsController = new ProductsController
        (
            logger: _logger,
            mapper: _mapper,
            productRepository: _productRepository,
            categoryRepository: _categoryRepository,
            userRepository: _userRepository,
            productImageHandler: _productImageHandler,
            messageHandler: _messageHandler,
            productService: _productService
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

    #region Details

    [TestMethod]
    public async Task AdminProductsController_Details_Succeeds_WithViewResult()
    {
        // Arrange 
            // Id is not null
        const int id = 0;
            // Id exists, read succeeds
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductDetailsDto>>>._))
            .Returns(A.Fake<AdminProductDetailsDto>());
        // Act
        IActionResult result = await _adminProductsController.Details(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<AdminProductDetailsDto>(viewResult.Model);
            // Must have accessed database
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductDetailsDto>>>._))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public void AdminProductsController_Details_Succeeds_WhenAuthorizedAsAdmin()
    {
        // Arrange
        var controller = typeof(ProductsController);
        var action = controller.GetMethod("Details");
        // Act
        var hasAuthorization = AttributeHandler.HasAuthorizationAttributes(controller, action, "Admin");
        // Assert
        Assert.IsTrue(hasAuthorization, "Action should not be authorized but it is");
    }

    [TestMethod]
    public void AdminProductsController_Details_Fails_WhenIdIsNull_WithBadRequest()
    {
        // Arrange
        var action = typeof(ProductsController).GetMethod("Details");
        // Act
        var attributes = action?.GetCustomAttributes(typeof(IfArgNullBadRequestFilter), false);
        // Assert
        Assert.IsTrue(!attributes.IsNullOrEmpty(), $"Action does not have {nameof(IfArgNullBadRequestFilter)} attribute even though it should");
    }

    [TestMethod]
    public async Task AdminProductsController_Details_Fails_WhenProductDoesntExist_WithNotFound()
    {
        // Arrange 
            // Id is not null
        const int id = 0;
            // Id does NOT exist, read NOT succeeds.
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductDetailsDto>>>._))
            .Returns((AdminProductDetailsDto?)null);
        // Act
        IActionResult result = await _adminProductsController.Details(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
            // Must have called database
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductDetailsDto>>>._))
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
        A.CallTo(() => _categoryRepository.GetAllAsync(A<string>._, A<Func<IQueryable<Category>, IQueryable<SelectListItem>>>._))
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

    [TestMethod]
    public void AdminProductsController_CreateGet_Succeeds_WhenAuthorizedAsAdmin()
    {
        // Arrange
        var controller = typeof(ProductsController);
        var action = controller.GetMethod("Create", []);
        // Act
        var hasAuthorization = AttributeHandler.HasAuthorizationAttributes(controller, action, checkRole: "Admin");
        // Assert
        Assert.IsTrue(hasAuthorization, "Action should be authorized as admin but it isn't");
    }

    [TestMethod]
    public async Task AdminProductsController_CreatePost_Succeeds_WithRedirect()
    {
        // Arrange
            // Modelstate is correct
            // Create succeeds
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(true);
        // Act
        IActionResult result = await _adminProductsController.Create(A.Fake<AdminProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Details");
        Assert.IsNotNull((int?)redirectResult.RouteValues?["id"]);
    }

    [TestMethod]
    public void AdminProductsController_CreatePost_Succeeds_WhenAuthorized()
    {
        // Arrange
        var controller = typeof(ProductsController);
        var action = controller.GetMethod("Create", [typeof(AdminProductCreateDto)]);
        // Act
        var hasAuthorization = AttributeHandler.HasAuthorizationAttributes(controller, action);
        // Assert
        Assert.IsTrue(hasAuthorization, "Action should be authorized but it isn't");
    }


    [TestMethod]
    public async Task AdminProductsController_CreatePost_Fails_WhenModelStateIsNotValid_WithViewResult_WithWarningMessage()
    {
        // Arrange
            // Modelstate is NOT correct
        _adminProductsController.ModelState.AddModelError("CategoryId", "The CategoryId for this product is invalid");
            // SelectList exists
        List<SelectListItem> selectListItems = (List<SelectListItem>)A.CollectionOfFake<SelectListItem>(4);
        A.CallTo(() => _categoryRepository.GetAllAsync(A<string>._, A<Func<IQueryable<Category>, IQueryable<SelectListItem>>>._))
            .Returns(selectListItems);
        // Act
        IActionResult result = await _adminProductsController.Create(A.Fake<AdminProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<AdminProductCreateDto>(viewResult.Model);
            // Must have accessed database
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustNotHaveHappened();
            // Message exists and is correct
        var messages = _messageHandler.Peek(viewResult.TempData);
        Assert.IsNotNull(messages);
        Assert.IsTrue(messages.Any(m => m.Type is Message.MessageType.Warning));
            // ViewData is correct
        Assert.IsInstanceOfType<IEnumerable<SelectListItem>>(viewResult.ViewData["categorySelectList"]);
        var viewResultViewDataSelectList = (IEnumerable<SelectListItem>)viewResult.ViewData["categorySelectList"]!;
        Assert.IsTrue(viewResultViewDataSelectList.Count() == selectListItems.Count);
    }

    [TestMethod]
    public async Task AdminProductsController_CreatePost_Fails_WhenProductCannotBeCreated_WithRedirect_WithWarningMessage()
    {
        // Arrange
            // Modelstate is correct
            // Create NOT succeeds
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _adminProductsController.Create(A.Fake<AdminProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Create");
            // Must have accessed database
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustHaveHappenedOnceExactly();
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
        A.CallTo(() => _categoryRepository.GetAllAsync(A<string>._, A<Func<IQueryable<Category>, IQueryable<SelectListItem>>>._))
            .Returns(selectListItems);
            // Id exists, Product is found
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductCreateDto>>>._))
            .Returns(A.Fake<AdminProductCreateDto>());
        // Act
        IActionResult result = await _adminProductsController.Edit(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<AdminProductCreateDto>(viewResult.Model);

            // Must have called the database
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductCreateDto>>>._))
            .MustHaveHappenedOnceOrMore();

            // ViewData is valid
        Assert.IsInstanceOfType<IEnumerable<SelectListItem>>(viewResult.ViewData["categorySelectList"]);
        var viewResultViewDataSelectList = (IEnumerable<SelectListItem>)viewResult.ViewData["categorySelectList"]!;
        Assert.IsTrue(viewResultViewDataSelectList.Count() == selectListItems.Count);
    }

    [TestMethod]
    public void AdminProductsController_EditGet_Succeeds_WhenAuthorized()
    {
        // Arrange
        var controller = typeof(ProductsController);
        var action = controller.GetMethod("Edit", [typeof(int?)]);
        // Act
        var hasAuthorization = AttributeHandler.HasAuthorizationAttributes(controller, action);
        // Assert
        Assert.IsTrue(hasAuthorization, "Action should be authorized but it isn't");
    }

    [TestMethod]
    public void AdminProductsController_EditGet_Fails_WhenIdIsNull_WithBadRequest()
    {
        // Arrange
        var action = typeof(ProductsController).GetMethod("Edit", [typeof(int?)]);
        // Act
        var attributes = action?.GetCustomAttributes(typeof(IfArgNullBadRequestFilter), false);
        // Assert
        Assert.IsTrue(!attributes.IsNullOrEmpty(), $"Action does not have {nameof(IfArgNullBadRequestFilter)} attribute even though it should");
    }

    [TestMethod]
    public async Task AdminProductsController_EditGet_Fails_WhenProductDoesntExist_WithNotFound()
    {
        // Arrange 
            // Id is not null
        const int id = 0;
            // Id does NOT exist, product is NOT found
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductCreateDto>>>._))
            .Returns((AdminProductCreateDto?)null);
        // Act
        IActionResult result = await _adminProductsController.Edit(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
            // Must have accessed the database
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductCreateDto>>>._))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task AdminProductsController_EditPost_Succeeds_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 3_242_598;
            // Id exists
        A.CallTo(() => _productRepository.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._)).Returns(true);
            // Modelstate is valid
            // Update succeeds
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(true);
        // Act
        IActionResult result = await _adminProductsController.Edit(id, A.Fake<AdminProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Details");
        Assert.IsTrue((int?)redirectResult.RouteValues?["id"] is not null and id);
            // Must have accessed database
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public void AdminProductsController_EditPost_Succeeds_WhenAuthorized()
    {
        // Arrange
        var controller = typeof(ProductsController);
        var action = controller.GetMethod("Edit", [typeof(int?), typeof(AdminProductCreateDto)]);
        // Act
        var hasAuthorization = AttributeHandler.HasAuthorizationAttributes(controller, action);
        // Assert
        Assert.IsTrue(hasAuthorization, "Action should be authorized but it isn't");
    }

    [TestMethod]
    public void AdminProductsController_EditPost_Fails_WhenIdIsNull_WithBadRequest()
    {
        // Arrange
        var action = typeof(ProductsController).GetMethod("Edit", [typeof(int?), typeof(AdminProductCreateDto)]);
        // Act
        var attributes = action?.GetCustomAttributes(typeof(IfArgNullBadRequestFilter), false);
        // Assert
        Assert.IsTrue(!attributes.IsNullOrEmpty(), $"Action does not have {nameof(IfArgNullBadRequestFilter)} attribute even though it should");
    }

    [TestMethod]
    public async Task AdminProductsController_EditPost_Fails_WhenProductNotExist_WithNotFound()
    {
        // Arrange
            // Id is not null
        const int id = 3_242_598;
            // Id does NOT exist
        A.CallTo(() => _productRepository.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._)).Returns(false);
        // Act
        IActionResult result = await _adminProductsController.Edit(id, A.Fake<AdminProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
            // Mustn't have accessed database
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task AdminProductsController_EditPost_Fails_WhenModelStateNotValid_WithRedirect_WithWarningMessage()
    {
        // Arrange
            // Id is not null
        const int id = 3_242_598;
            // Id exists
        A.CallTo(() => _productRepository.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._)).Returns(true);
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
        var messages = _messageHandler.Peek(_adminProductsController.TempData);
        Assert.IsNotNull(messages);
        Assert.IsTrue(messages.Any(m => m.Type is Message.MessageType.Warning));
            // Musn't have accessed database
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task AdminProductsController_EditPost_Fails_WhenUpdateFails_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 3_242_598;
            // Id exists
        A.CallTo(() => _productRepository.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._)).Returns(true);
            // Modelstate is valid
            // Update does NOT succeed
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _adminProductsController.Edit(id, A.Fake<AdminProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Edit");
        Assert.IsTrue((int?)redirectResult.RouteValues?["id"] is not null and id);
            // Must have accesses database
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustHaveHappenedOnceExactly();
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
        var fakeProduct = A.Fake<AdminProductDetailsDto>();
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductDetailsDto>>>._))
            .Returns(fakeProduct);
        // Act
        IActionResult result = await _adminProductsController.Delete(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<AdminProductDetailsDto>(viewResult.Model);
            // Must have called the database
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductDetailsDto>>>._))
            .MustHaveHappenedOnceOrMore();
    }

    [TestMethod]
    public void AdminProductsController_DeleteGet_Succeeds_WhenAuthorized()
    {
        // Arrange
        var controller = typeof(ProductsController);
        var action = controller.GetMethod("Delete");
        // Act
        var hasAuthorization = AttributeHandler.HasAuthorizationAttributes(controller, action);
        // Assert
        Assert.IsTrue(hasAuthorization, "Action should be authorized but it isn't");
    }

    [TestMethod]
    public void AdminProductsController_DeleteGet_Fails_WhenIdIsNull_WithBadRequest()
    {
        // Arrange
        var action = typeof(ProductsController).GetMethod("Delete");
        // Act
        var attributes = action?.GetCustomAttributes(typeof(IfArgNullBadRequestFilter), false);
        // Assert
        Assert.IsTrue(!attributes.IsNullOrEmpty(), $"Action does not have {nameof(IfArgNullBadRequestFilter)} attribute even though it should");
    }


    [TestMethod]
    public async Task AdminProductsController_DeleteGet_Fails_WhenProductNotFound_WithNotFound()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id does NOT exists, Product is NOT found
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductDetailsDto>>>._))
            .Returns((AdminProductDetailsDto?)null);
        // Act
        IActionResult result = await _adminProductsController.Delete(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
        // Must have called the database
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<AdminProductDetailsDto>>>._))
            .MustHaveHappenedOnceOrMore();
    }

    [TestMethod]
    public async Task AdminProductsController_DeletePost_Succeeds_WithViewResult()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is the same as User id
        var fakeProduct = A.Fake<Product>();
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(fakeProduct);
            // Delete succeeds
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(true);
        // Act
        IActionResult result = await _adminProductsController.DeleteConfirmed(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName is "Index");
        Assert.IsTrue(redirectResult.ControllerName is "User");
        Assert.IsTrue(redirectResult.RouteValues?["id"] as string == _adminProductsController.User.FindFirstValue(ClaimTypes.NameIdentifier));
        // Must have called the database
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public void AdminProductsController_DeletePost_Succeeds_WhenAuthorized()
    {
        // Arrange
        var controller = typeof(ProductsController);
        var action = controller.GetMethod("DeleteConfirmed");
        // Act
        var hasAuthorization = AttributeHandler.HasAuthorizationAttributes(controller, action);
        // Assert
        Assert.IsTrue(hasAuthorization, "Action should be authorized but it isn't");
    }

    [TestMethod]
    public void AdminProductsController_DeletePost_Fails_WhenIdIsNull_WithBadRequest()
    {
        // Arrange
        var action = typeof(ProductsController).GetMethod("DeleteConfirmed");
        // Act
        var attributes = action?.GetCustomAttributes(typeof(IfArgNullBadRequestFilter), false);
        // Assert
        Assert.IsTrue(!attributes.IsNullOrEmpty(), $"Action does not have {nameof(IfArgNullBadRequestFilter)} attribute even though it should");
    }

    [TestMethod]
    public async Task AdminProductsController_DeletePost_Fails_WhenIdNotExist_WithNotFound()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id does NOT exists, Product is NOT found
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns((Product?)null);
        // Act
        IActionResult result = await _adminProductsController.DeleteConfirmed(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
        // Mustn't have called the database
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task AdminProductsController_DeletePost_Fails_WhenDeleteNotSucceeds_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
        var fakeProduct = A.Fake<Product>();
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(fakeProduct);
            // Delete does NOT succeed
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _adminProductsController.DeleteConfirmed(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName is "Delete");
        Assert.IsTrue(redirectResult.RouteValues?["id"] as int? == id);
        // Must have called the database
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustHaveHappenedOnceExactly();
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
        var fakeProduct = A.Fake<Product>();
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(fakeProduct);
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
    public void AdminProductsController_ResetImage_Succeeds_WhenAuthorized()
    {
        // Arrange
        var controller = typeof(ProductsController);
        var action = controller.GetMethod("ResetImage");
        // Act
        var hasAuthorization = AttributeHandler.HasAuthorizationAttributes(controller, action);
        // Assert
        Assert.IsTrue(hasAuthorization, "Action should be authorized but it isn't");
    }

    [TestMethod]
    public void AdminProductsController_ResetImage_Fails_WhenIdIsNull_WithBadRequest()
    {
        // Arrange
        var action = typeof(ProductsController).GetMethod("ResetImage");
        // Act
        var attributes = action?.GetCustomAttributes(typeof(IfArgNullBadRequestFilter), false);
        // Assert
        Assert.IsTrue(!attributes.IsNullOrEmpty(), $"Action does not have {nameof(IfArgNullBadRequestFilter)} attribute even though it should");
    }

    [TestMethod]
    public async Task AdminProductsController_ResetImage_Fails_WhenIdNotExist_WithNotFound()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id does NOT exists, Product is NOT found
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns((Product?)null);
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
        var fakeProduct = A.Fake<Product>();
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(fakeProduct);
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
            // Message not success
        Assert.IsTrue(_messageHandler.Peek(_adminProductsController.TempData)?.Any(m => m.Type is Message.MessageType.Error));
            // Must have attempted image delete
        A.CallTo(() => _productImageHandler.DeleteImage(A<ProductImageKey>._))
            .MustHaveHappenedOnceOrMore();
    }


    #endregion Image

}
