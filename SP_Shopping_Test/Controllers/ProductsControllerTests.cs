using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SP_Shopping.Controllers;
using SP_Shopping.Dtos.Product;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Service;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.MessageHandler;
using System.Security.Claims;

namespace SP_Shopping.Test.Controllers;

[TestClass]
public class ProductsControllerTests
{

    private readonly IRepository<Product> _productRepository;
    private readonly ProductService _productService;
    private readonly IRepositoryCaching<Category> _categoryRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductsController> _logger;
    private readonly IImageHandlerDefaulting<ProductImageKey> _productImageHandler;
    private readonly IMessageHandler _messageHandler;
    private readonly ProductsController _productsController;

    public ProductsControllerTests()
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

        _productsController = new ProductsController
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
                new Claim(ClaimTypes.Role, "NotAdmin")
                // other required and custom claims
           ],"TestAuthentication")
        ]);

        _productsController.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
            {
                // Set fake user
                User = fakeUser
            }
        };

        // Set fake TempData
        _productsController.TempData = new TempDataDictionary(_productsController.HttpContext, A.Fake<ITempDataProvider>());
    }

    #region Details

    [TestMethod]
    public async Task ProductsController_Details_Succeeds_WithViewResult()
    {
        // Arrange 
            // Id is not null
        const int id = 0;
            // Id exists, read succeeds
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto?>>>._))!
            .Returns(Task.FromResult(A.Fake<ProductDetailsDto>()));
        // Act
        IActionResult result = await _productsController.Details(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<ProductDetailsDto>(viewResult.Model);
            // Must have accessed database
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task ProductController_Details_Fails_WhenIdIsNull_WithBadRequestResponse()
    {
        // Arrange
            // Id IS null
        // Act
        IActionResult result = await _productsController.Details(null);
        // Assert
            // Result is correct
        Assert.IsTrue(result is BadRequestResult or BadRequestObjectResult);
    }

    [TestMethod]
    public async Task ProductsController_Details_Fails_WhenProductDoesntExist_WithNotFoundResponse()
    {
        // Arrange 
            // Id is not null
        const int id = 0;
            // Id does NOT exist, read NOT succeeds.
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .Returns(Task.FromResult((ProductDetailsDto?)null));
        // Act
        IActionResult result = await _productsController.Details(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
            // Must have called database
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion Details

    #region Create

    [TestMethod]
    public async Task ProductsController_CreateGet_Succeeds_WithViewResult()
    {
        // Arrange
            // SelectList exists
        List<SelectListItem> selectListItems = (List<SelectListItem>)A.CollectionOfFake<SelectListItem>(4);
        A.CallTo(() => _categoryRepository.GetAllAsync(A<string>._, A<Func<IQueryable<Category>, IQueryable<SelectListItem>>>._))
            .Returns(Task.FromResult(selectListItems));
        // Act
        IActionResult result = await _productsController.Create();
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        ViewResult viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<ProductCreateDto>(viewResult.Model);

            // ViewData is correct
        Assert.IsInstanceOfType<IEnumerable<SelectListItem>>(viewResult.ViewData["categorySelectList"]);
        var viewResultViewDataSelectList = (IEnumerable<SelectListItem>)viewResult.ViewData["categorySelectList"]!;
        Assert.IsTrue(viewResultViewDataSelectList.Count() == selectListItems.Count);
    }

    [TestMethod]
    public async Task ProductsController_CreatePost_Succeeds_WithRedirectToActionResult()
    {
        // Arrange
            // Modelstate is correct
            // Create succeeds
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(Task.FromResult(true));
        // Act
        IActionResult result = await _productsController.Create(A.Fake<ProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Details");
        Assert.IsNotNull((int?)redirectResult.RouteValues?["id"]);
    }

    [TestMethod]
    public async Task ProductController_CreatePost_Fails_WhenModelStateIsNotValid_WithViewResult_WithWarningMessage()
    {
        // Arrange
            // Modelstate is NOT correct
        _productsController.ModelState.AddModelError("CategoryId", "The CategoryId for this product is invalid");
            // SelectList exists
        List<SelectListItem> selectListItems = (List<SelectListItem>)A.CollectionOfFake<SelectListItem>(4);
        A.CallTo(() => _categoryRepository.GetAllAsync(A<string>._, A<Func<IQueryable<Category>, IQueryable<SelectListItem>>>._))
            .Returns(Task.FromResult(selectListItems));
        // Act
        IActionResult result = await _productsController.Create(A.Fake<ProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<ProductCreateDto>(viewResult.Model);
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
    public async Task ProductController_CreatePost_Fails_WhenProductCannotBeCreated_WithRedirectResult_WithWarningMessage()
    {
        // Arrange
            // Modelstate is correct
            // Create NOT succeeds
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(Task.FromResult(false));
        // Act
        IActionResult result = await _productsController.Create(A.Fake<ProductCreateDto>());
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
    public async Task ProductController_EditGet_Succeeds_WithViewResult()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // SelectList exists
        List<SelectListItem> selectListItems = Enumerable.Range(0, 4).Select(s => new SelectListItem()).ToList();
        A.CallTo(() => _categoryRepository.GetAllAsync(A<string>._, A<Func<IQueryable<Category>, IQueryable<SelectListItem>>>._))
            .Returns(Task.FromResult(selectListItems));
            // Id exists, Product is found
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductCreateDto>>>._))
            .Returns(A.Fake<ProductCreateDto>());
            // Product.SubmitterId is the same as User id
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<string?>>>._))
            .Returns(_productsController.User.FindFirstValue(ClaimTypes.NameIdentifier));
        // Act
        IActionResult result = await _productsController.Edit(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<ProductCreateDto>(viewResult.Model);

            // Must have called the database
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductCreateDto>>>._))
            .MustHaveHappenedOnceOrMore();

            // ViewData is valid
        Assert.IsInstanceOfType<IEnumerable<SelectListItem>>(viewResult.ViewData["categorySelectList"]);
        var viewResultViewDataSelectList = (IEnumerable<SelectListItem>)viewResult.ViewData["categorySelectList"]!;
        Assert.IsTrue(viewResultViewDataSelectList.Count() == selectListItems.Count);
    }

    [TestMethod]
    public async Task ProductController_EditGet_Fails_WhenIdIsNull_WithBadRequestResult()
    {
        // Arrange
            // Id IS null
        // Act
        IActionResult result = await _productsController.Edit(null);
        // Assert
        Assert.IsTrue(result is BadRequestResult or BadRequestObjectResult);
    }

    [TestMethod]
    public async Task ProductsController_EditGet_Fails_WhenProductDoesntExist_WithNotFoundResult()
    {
        // Arrange 
            // Id is not null
        const int id = 0;
            // Id does NOT exist, product is NOT found
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductCreateDto?>>>._))
            .Returns(Task.FromResult((ProductCreateDto?)null));
        // Act
        IActionResult result = await _productsController.Edit(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
            // Must have accessed the database
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductCreateDto?>>>._))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task ProductsController_EditGet_Fails_WhenUserIdNotSubmitterId_WithUnauthorizedResult()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id does exist, product is found
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductCreateDto?>>>._))
            .Returns(A.Fake<ProductCreateDto>());
            // Product.Submitter is NOT the same as User id
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<string?>>>._))
            .Returns("not " + _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier));
        // Act
        IActionResult result = await _productsController.Edit(id);
        // Assert
            // Result is true
        Assert.IsTrue(result is UnauthorizedResult or UnauthorizedObjectResult);
            // Must have accessed the database
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductCreateDto?>>>._))
            .MustHaveHappenedOnceOrMore();
    }

    [TestMethod]
    public async Task ProductController_EditPost_Succeeds_WithRedirectResult()
    {
        // Arrange
            // Id is not null
        const int id = 3_242_598;
            // Id exists
        A.CallTo(() => _productRepository.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._)).Returns(true);
            // Modelstate is valid
            // Product.SubmitterId is the same as User id
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<string?>>>._))
            .Returns(_productsController.User.FindFirstValue(ClaimTypes.NameIdentifier));
            // Update succeeds
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(true);
        // Act
        IActionResult result = await _productsController.Edit(id, A.Fake<ProductCreateDto>());
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
    public async Task ProductController_EditPost_Fails_WhenIdIsNull_WithBadRequestResult()
    {
        // Arrange
        // Id IS null
        // Act
        IActionResult result = await _productsController.Edit(null, A.Fake<ProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsTrue(result is BadRequestResult or BadRequestObjectResult);
            // Must have accessed database
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task ProductController_EditPost_Fails_WhenProductNotExist_WithNotFoundResult()
    {
        // Arrange
            // Id is not null
        const int id = 3_242_598;
            // Id does NOT exist
        A.CallTo(() => _productRepository.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._)).Returns(false);
        // Act
        IActionResult result = await _productsController.Edit(id, A.Fake<ProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
            // Mustn't have accessed database
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task ProductController_EditPost_Fails_WhenModelStateNotValid_WithRedirectResult_WithWarningMessage()
    {
        // Arrange
            // Id is not null
        const int id = 3_242_598;
            // Id exists
        A.CallTo(() => _productRepository.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._)).Returns(true);
            // Modelstate is NOT valid
        _productsController.ModelState.AddModelError("CategoryId", "The CategoryId for this product is invalid");
        // Act
        IActionResult result = await _productsController.Edit(id, A.Fake<ProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Edit");
        Assert.IsTrue((int?)redirectResult.RouteValues?["id"] is not null and id);
            // Check message exists
        var messages = _messageHandler.Peek(_productsController.TempData);
        Assert.IsNotNull(messages);
        Assert.IsTrue(messages.Any(m => m.Type is Message.MessageType.Warning));
            // Musn't have accessed database
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustNotHaveHappened();
    }


    [TestMethod]
    public async Task ProductController_EditPost_Fails_WhenSubmitterIdNotUserId_WithRedirectResult()
    {
        // Arrange
            // Id is not null
            // Id exists
        A.CallTo(() => _productRepository.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._)).Returns(true);
            // Modelstate is valid
            // Product.SubmitterId is NOT the same as User is
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<string?>>>._))
            .Returns("not " + _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier));
        // Act
        IActionResult result = await _productsController.Edit(0, A.Fake<ProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsTrue(result is UnauthorizedResult or UnauthorizedObjectResult);
            // Musn't have accessed database
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task ProductController_EditPost_Fails_WhenUpdateFails_WithRedirectResult()
    {
        // Arrange
            // Id is not null
        const int id = 3_242_598;
            // Id exists
        A.CallTo(() => _productRepository.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._)).Returns(true);
            // Modelstate is valid
            // Product.SubmitterId is the same as User id
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<string?>>>._))
            .Returns(_productsController.User.FindFirstValue(ClaimTypes.NameIdentifier));
            // Update does NOT succeed
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _productsController.Edit(id, A.Fake<ProductCreateDto>());
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
    public async Task ProductController_DeleteGet_Succeeds_WithViewResult()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is the same as User id
        var fakeProduct = A.Fake<ProductDetailsDto>();
        fakeProduct.SubmitterId = _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .Returns(fakeProduct);
        // Act
        IActionResult result = await _productsController.Delete(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<ProductDetailsDto>(viewResult.Model);
            // Must have called the database
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .MustHaveHappenedOnceOrMore();
    }

    [TestMethod]
    public async Task ProductController_DeleteGet_Fails_WhenIdIsNull_WithBadRequest()
    {
        // Arrange
            // Id IS null
        // Act
        IActionResult result = await _productsController.Delete(null);
        // Assert
            // Result is correct
        Assert.IsTrue(result is BadRequestResult or BadRequestObjectResult);
            // Mustn't have called the database
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task ProductController_DeleteGet_Fails_WhenProductNotFound_WithNotFound()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id does NOT exists, Product is NOT found
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .Returns((ProductDetailsDto?)null);
        // Act
        IActionResult result = await _productsController.Delete(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
        // Must have called the database
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .MustHaveHappenedOnceOrMore();
    }

    [TestMethod]
    public async Task ProductController_DeleteGet_Fails_WhenSubmitterIdNotUserId_WithUnauthorised()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is NOT the same as User id
        var fakeProduct = A.Fake<ProductDetailsDto>();
        fakeProduct.SubmitterId = "Not " + _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .Returns(fakeProduct);
        // Act
        IActionResult result = await _productsController.Delete(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is UnauthorizedResult or UnauthorizedObjectResult);
            // Must have called the database
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .MustHaveHappenedOnceOrMore();
    }

    [TestMethod]
    public async Task ProductController_DeletePost_Succeeds_WithViewResult()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is the same as User id
        var fakeProduct = A.Fake<Product>();
        fakeProduct.SubmitterId = _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(fakeProduct);
            // Delete succeeds
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(true);
        // Act
        IActionResult result = await _productsController.DeleteConfirmed(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName is "Index");
        Assert.IsTrue(redirectResult.ControllerName is "User");
        Assert.IsTrue(redirectResult.RouteValues?["id"] as string == _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier));
        // Must have called the database
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task ProductController_DeletePost_Fails_WhenIdIsNull_WithBadRequest()
    {
        // Arrange
            // Id IS null
        // Act
        IActionResult result = await _productsController.DeleteConfirmed(null);
        // Assert
        // Result is correct
        Assert.IsTrue(result is BadRequestResult or BadRequestObjectResult);
        // Mustn't have called the database
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task ProductController_DeletePost_Fails_WhenIdNotExist_WithNotFound()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id does NOT exists, Product is NOT found
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns((Product?)null);
        // Act
        IActionResult result = await _productsController.DeleteConfirmed(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
        // Mustn't have called the database
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task ProductController_DeletePost_Fails_WhenSubmitterIdNotUserId_WithUnauthorized()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is NOT the same as User id
        var fakeProduct = A.Fake<Product>();
        fakeProduct.SubmitterId = "Not " + _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(fakeProduct);
        // Act
        IActionResult result = await _productsController.DeleteConfirmed(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is UnauthorizedResult or UnauthorizedObjectResult);
        // Mustn't have called the database
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task ProductController_DeletePost_Fails_WhenDeleteNotSucceeds_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is the same as User id
        var fakeProduct = A.Fake<Product>();
        fakeProduct.SubmitterId = _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(fakeProduct);
            // Delete does NOT succeed
        A.CallTo(() => _productRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _productsController.DeleteConfirmed(id);
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
    public async Task ProductController_ResetImage_Succeeds_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is the same as User id
        var fakeProduct = A.Fake<Product>();
        fakeProduct.SubmitterId = _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(fakeProduct);
            // Delete image successful
        A.CallTo(() => _productImageHandler.DeleteImage(A<ProductImageKey>._))
            .DoesNothing();
        // Act
        IActionResult result = await _productsController.ResetImage(id);
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
    public async Task ProductController_ResetImage_Fails_WhenIdIsNull_WithBadRequest()
    {
        // Arrange
            // Id IS null
        // Act
        IActionResult result = await _productsController.ResetImage(null);
        // Assert
            // Result is correct
        Assert.IsTrue(result is BadRequestResult or BadRequestObjectResult);
            // Mustn't have attempted image delete
        A.CallTo(() => _productImageHandler.DeleteImage(A<ProductImageKey>._))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task ProductController_ResetImage_Fails_WhenIdNotExist_WithNotFound()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id does NOT exists, Product is NOT found
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns((Product?)null);
        // Act
        IActionResult result = await _productsController.ResetImage(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
            // Mustn't have attempted image delete
        A.CallTo(() => _productImageHandler.DeleteImage(A<ProductImageKey>._))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task ProductController_ResetImage_Fails_WithSubmitterIdNotUserId_WithUnauthorized()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is the same as User id
        var fakeProduct = A.Fake<Product>();
        fakeProduct.SubmitterId = "Not " + _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(fakeProduct);
        // Act
        IActionResult result = await _productsController.ResetImage(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is UnauthorizedResult or UnauthorizedObjectResult);
            // Mustn't have attempted image delete
        A.CallTo(() => _productImageHandler.DeleteImage(A<ProductImageKey>._))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task ProductController_ResetImage_Fails_WhenDeleteImageFails_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is the same as User id
        var fakeProduct = A.Fake<Product>();
        fakeProduct.SubmitterId = _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        A.CallTo(() => _productRepository.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(fakeProduct);
            // Delete image NOT successful
        A.CallTo(() => _productImageHandler.DeleteImage(A<ProductImageKey>._))
            .Throws<Exception>(e => new Exception("Test exception", e));
        // Act
        IActionResult result = await _productsController.ResetImage(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName is "Edit");
        Assert.IsTrue(redirectResult.RouteValues?["id"] is id);
            // Message not success
        Assert.IsTrue(_messageHandler.Peek(_productsController.TempData)?.Any(m => m.Type is Message.MessageType.Error));
            // Must have attempted image delete
        A.CallTo(() => _productImageHandler.DeleteImage(A<ProductImageKey>._))
            .MustHaveHappenedOnceOrMore();
    }


    #endregion Image

}
