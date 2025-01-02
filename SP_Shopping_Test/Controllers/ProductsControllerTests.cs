using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SP_Shopping.Controllers;
using SP_Shopping.Dtos.Product;
using SP_Shopping.Hubs;
using SP_Shopping.Models;
using SP_Shopping.Service;
using SP_Shopping.Test.TestingUtilities;
using SP_Shopping.Utilities.Filters;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.MessageHandler;
using System.Reflection;
using System.Security.Claims;

namespace SP_Shopping.Test.Controllers;

[TestClass]
public class ProductsControllerTests
{

    private readonly IMapper _mapper;
    private readonly ILogger<ProductsController> _logger;
    private readonly IShoppingServices _shoppingServices;
    private readonly IImageHandlerDefaulting<ProductImageKey> _productImageHandler;
    private readonly IMessageHandler _messageHandler;
    private readonly IHubContext<ProductHub, IProductHubClient> _productHub;
    private readonly ProductsController _productsController;

    public ProductsControllerTests()
    {
        _mapper = A.Fake<IMapper>();
        _logger = new NullLogger<ProductsController>();
        _shoppingServices = A.Fake<IShoppingServices>();
        _productImageHandler = A.Fake<IImageHandlerDefaulting<ProductImageKey>>();
        _messageHandler = new MessageHandler();
        _productHub = A.Fake<IHubContext<ProductHub, IProductHubClient>>();

        // SUT

        _productsController = new ProductsController
        (
            logger: _logger,
            mapper: _mapper,
            shoppingServices: _shoppingServices,
            productImageHandler: _productImageHandler,
            messageHandler: _messageHandler,
            productHub: _productHub
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

    [DataRow("Details", (Type[])[typeof(int?)], false)]
    [DataRow("Create", (Type[])[], true)]
    [DataRow("Create", (Type[])[typeof(ProductCreateDto)], true)]
    [DataRow("Edit", (Type[])[typeof(int?)], true)]
    [DataRow("Edit", (Type[])[typeof(int?), typeof(ProductCreateDto)], true)]
    [DataRow("Delete", (Type[])[typeof(int?)], true)]
    [DataRow("DeleteConfirmed", (Type[])[typeof(int?)], true)]
    [DataRow("ResetImage", (Type[])[typeof(int?)], true)]
    [DataTestMethod]
    public void ProductssController_All_Succeeds_WhenAuthorizedAsSucceeds(string methodName, Type[] types, bool succeeds)
    {
        // Arrange
        var controller = typeof(ProductsController);
        var action = controller.GetMethod(methodName, types);
        // Act
        var hasAuthorization = AttributeHandler.HasAuthorizationAttributes(controller, action);
        // Assert
        Assert.IsTrue(succeeds == hasAuthorization, $"Action {methodName} should {(succeeds ? "" : "not " )}be authorized but it is{(!succeeds ? "" : ("n't"))}");
    }

    [DataRow("Details", (Type[])[typeof(int?)])]
    [DataRow("Edit", (Type[])[typeof(int?)])]
    [DataRow("Edit", (Type[])[typeof(int?), typeof(ProductCreateDto)])]
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
    public async Task ProductsController_Details_Succeeds_WithViewResult()
    {
        // Arrange 
            // Id is not null
        const int id = 0;
            // Id exists, read succeeds
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .Returns(A.Fake<ProductDetailsDto>());
        // Act
        IActionResult result = await _productsController.Details(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<ProductDetailsDto>(viewResult.Model);
            // Must have accessed database
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public void ProductsController_Details_Succeeds_WhenNotAuthorized()
    {
        // Arrange
        var controller = typeof(ProductsController);
        var action = controller.GetMethod("Details");
        // Act
        var hasAuthorization = AttributeHandler.HasAuthorizationAttributes(controller, action);
        // Assert
        Assert.IsTrue(!hasAuthorization, "Action should not be authorized but it is");
    }

    [TestMethod]
    public async Task ProductsController_Details_Fails_WhenProductDoesntExist_WithNotFound()
    {
        // Arrange 
            // Id is not null
        const int id = 0;
            // Id does NOT exist, read NOT succeeds.
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .Returns((ProductDetailsDto?)null);
        // Act
        IActionResult result = await _productsController.Details(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
            // Must have called database
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
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
        A.CallTo(() => _shoppingServices.Category.GetAllAsync(A<string>._, A<Func<IQueryable<Category>, IQueryable<SelectListItem>>>._))
            .Returns(selectListItems);
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
    public async Task ProductsController_CreatePost_Succeeds_WithRedirect()
    {
        // Arrange
            // Modelstate is correct
            // Create succeeds
        A.CallTo(() => _shoppingServices.Product.TryCreateAsync(A<Product>._, An<IFormFile?>._))
            .Returns((true, null));
        // Act
        IActionResult result = await _productsController.Create(A.Fake<ProductCreateDto>());
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
    public async Task ProductsController_CreatePost_Fails_WhenModelStateIsNotValid_WithViewResult_WithWarningMessage()
    {
        // Arrange
            // Modelstate is NOT correct
        _productsController.ModelState.AddModelError("CategoryId", "The CategoryId for this product is invalid");
            // SelectList exists
        List<SelectListItem> selectListItems = (List<SelectListItem>)A.CollectionOfFake<SelectListItem>(4);
        A.CallTo(() => _shoppingServices.Category.GetAllAsync(A<string>._, A<Func<IQueryable<Category>, IQueryable<SelectListItem>>>._))
            .Returns(selectListItems);
        // Act
        IActionResult result = await _productsController.Create(A.Fake<ProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<ProductCreateDto>(viewResult.Model);
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
    public async Task ProductsController_CreatePost_Fails_WhenProductCannotBeCreated_WithRedirect_WithWarningMessage()
    {
        // Arrange
            // Modelstate is correct
            // Create NOT succeeds
        A.CallTo(() => _shoppingServices.Product.TryCreateAsync(A<Product>._, An<IFormFile?>._))
            .Returns((false, [ new Message { Type = Message.MessageType.Error, Content = "blabla" }]));
        // Act
        IActionResult result = await _productsController.Create(A.Fake<ProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Create");
            // Create attempted
        A.CallTo(() => _shoppingServices.Product.TryCreateAsync(A<Product>._, An<IFormFile?>._))
            .MustHaveHappened();
    }

    #endregion Create

    #region Edit

    [TestMethod]
    public async Task ProductsController_EditGet_Succeeds_WithViewResult()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // SelectList exists
        List<SelectListItem> selectListItems = Enumerable.Range(0, 4).Select(s => new SelectListItem()).ToList();
        A.CallTo(() => _shoppingServices.Category.GetAllAsync(A<string>._, A<Func<IQueryable<Category>, IQueryable<SelectListItem>>>._))
            .Returns(selectListItems);
            // Id exists, Product is found
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductCreateDto>>>._))
            .Returns(A.Fake<ProductCreateDto>());
            // Product.SubmitterId is the same as User id
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<string>>>._))
            .Returns(_productsController.User.FindFirstValue(ClaimTypes.NameIdentifier));
        // Act
        IActionResult result = await _productsController.Edit(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<ProductCreateDto>(viewResult.Model);

            // Must have called the database
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductCreateDto>>>._))
            .MustHaveHappenedOnceOrMore();

            // ViewData is valid
        Assert.IsInstanceOfType<IEnumerable<SelectListItem>>(viewResult.ViewData["categorySelectList"]);
        var viewResultViewDataSelectList = (IEnumerable<SelectListItem>)viewResult.ViewData["categorySelectList"]!;
        Assert.IsTrue(viewResultViewDataSelectList.Count() == selectListItems.Count);
    }

    [TestMethod]
    public async Task ProductsController_EditGet_Fails_WhenProductDoesntExist_WithNotFound()
    {
        // Arrange 
            // Id is not null
        const int id = 0;
            // Id does NOT exist, product is NOT found
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductCreateDto>>>._))
            .Returns((ProductCreateDto?)null);
        // Act
        IActionResult result = await _productsController.Edit(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
            // Must have accessed the database
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductCreateDto>>>._))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task ProductsController_EditGet_Fails_WhenUserIdNotSubmitterId_WithUnauthorized()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id does exist, product is found
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductCreateDto>>>._))
            .Returns(A.Fake<ProductCreateDto>());
            // Product.Submitter is NOT the same as User id
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<string>>>._))
            .Returns("not " + _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier));
        // Act
        IActionResult result = await _productsController.Edit(id);
        // Assert
            // Result is true
        Assert.IsTrue(result is UnauthorizedResult or UnauthorizedObjectResult);
            // Must have accessed the database
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductCreateDto>>>._))
            .MustHaveHappenedOnceOrMore();
    }

    [TestMethod]
    public async Task ProductsController_EditPost_Succeeds_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 3_242_598;
            // Id exists
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._)).Returns(true);
            // Modelstate is valid
            // Product.SubmitterId is the same as User id
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<string>>>._))
            .Returns(_productsController.User.FindFirstValue(ClaimTypes.NameIdentifier));
            // Update succeeds
        A.CallTo(() => _shoppingServices.Product.TryUpdateAsync(A<Product>._, An<IFormFile?>._))
            .Returns((true, null));
        // Act
        IActionResult result = await _productsController.Edit(id, A.Fake<ProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Details");
        Assert.IsTrue((int?)redirectResult.RouteValues?["id"] is not null and id);
            // Edit attempted
        A.CallTo(() => _shoppingServices.Product.TryUpdateAsync(A<Product>._, An<IFormFile?>._))
            .MustHaveHappened();
    }

    [TestMethod]
    public async Task ProductsController_EditPost_Fails_WhenProductNotExist_WithNotFound()
    {
        // Arrange
            // Id is not null
        const int id = 3_242_598;
            // Id does NOT exist
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _productsController.Edit(id, A.Fake<ProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
    }

    [TestMethod]
    public async Task ProductsController_EditPost_Fails_WhenModelStateNotValid_WithRedirect_WithWarningMessage()
    {
        // Arrange
            // Id is not null
        const int id = 3_242_598;
            // Id exists
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(true);
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
    }


    [TestMethod]
    public async Task ProductsController_EditPost_Fails_WhenSubmitterIdNotUserId_WithRedirect()
    {
        // Arrange
            // Id is not null
            // Id exists
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._)).Returns(true);
            // Modelstate is valid
            // Product.SubmitterId is NOT the same as User is
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<string>>>._))
            .Returns("not " + _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier));
        // Act
        IActionResult result = await _productsController.Edit(0, A.Fake<ProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsTrue(result is UnauthorizedResult or UnauthorizedObjectResult);
    }

    [TestMethod]
    public async Task ProductsController_EditPost_Fails_WhenUpdateFails_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 3_242_598;
            // Id exists
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._)).Returns(true);
            // Modelstate is valid
            // Product.SubmitterId is the same as User id
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<string>>>._))
            .Returns(_productsController.User.FindFirstValue(ClaimTypes.NameIdentifier));
            // Update does NOT succeed
        A.CallTo(() => _shoppingServices.Product.TryUpdateAsync(A<Product>._, An<IFormFile?>._))
            .Returns((false, [ new Message { Type = Message.MessageType.Error, Content = "blabla" }]));
        // Act
        IActionResult result = await _productsController.Edit(id, A.Fake<ProductCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Edit");
        Assert.IsTrue((int?)redirectResult.RouteValues?["id"] is not null and id);
            // Update attempted
        A.CallTo(() => _shoppingServices.Product.TryUpdateAsync(A<Product>._, An<IFormFile?>._))
            .MustHaveHappenedOnceExactly();
        // Message error
        Assert.IsTrue(_messageHandler.Peek(_productsController.TempData)?.Any(m => m.Type is Message.MessageType.Error), "Expected error mesage was not returned");
    }

    #endregion Edit

    #region Delete

    [TestMethod]
    public async Task ProductsController_DeleteGet_Succeeds_WithViewResult()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is the same as User id
        var fakeProduct = A.Fake<ProductDetailsDto>();
        fakeProduct.SubmitterId = _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .Returns(fakeProduct);
        // Act
        IActionResult result = await _productsController.Delete(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<ProductDetailsDto>(viewResult.Model);
            // Must have called the database
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .MustHaveHappenedOnceOrMore();
    }

    [TestMethod]
    public async Task ProductsController_DeleteGet_Fails_WhenProductNotFound_WithNotFound()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id does NOT exists, Product is NOT found
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .Returns((ProductDetailsDto?)null);
        // Act
        IActionResult result = await _productsController.Delete(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
        // Must have called the database
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .MustHaveHappenedOnceOrMore();
    }

    [TestMethod]
    public async Task ProductsController_DeleteGet_Fails_WhenSubmitterIdNotUserId_WithUnauthorized()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is NOT the same as User id
        var fakeProduct = A.Fake<ProductDetailsDto>();
        fakeProduct.SubmitterId = "Not " + _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .Returns(fakeProduct);
        // Act
        IActionResult result = await _productsController.Delete(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is UnauthorizedResult or UnauthorizedObjectResult);
            // Must have called the database
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .MustHaveHappenedOnceOrMore();
    }

    [TestMethod]
    public async Task ProductsController_DeletePost_Succeeds_WithViewResult()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is the same as User id
        var fakeProduct = A.Fake<Product>();
        fakeProduct.SubmitterId = _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(fakeProduct);
            // Delete succeeds
        A.CallTo(() => _shoppingServices.Product.TryDeleteAsync(A<Product>._))
            .Returns((true, null));
        // Act
        IActionResult result = await _productsController.DeleteConfirmed(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName is "Index");
        Assert.IsTrue(redirectResult.ControllerName is "User");
        Assert.IsTrue(redirectResult.RouteValues?["id"] as string == _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier));
        // Delete attempted
        A.CallTo(() => _shoppingServices.Product.TryDeleteAsync(A<Product>._))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task ProductsController_DeletePost_Fails_WhenIdNotExist_WithNotFound()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id does NOT exists, Product is NOT found
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns((Product?)null);
        // Act
        IActionResult result = await _productsController.DeleteConfirmed(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
    }

    [TestMethod]
    public async Task ProductsController_DeletePost_Fails_WhenSubmitterIdNotUserId_WithUnauthorized()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is NOT the same as User id
        var fakeProduct = A.Fake<Product>();
        fakeProduct.SubmitterId = "Not " + _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(fakeProduct);
        // Act
        IActionResult result = await _productsController.DeleteConfirmed(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is UnauthorizedResult or UnauthorizedObjectResult);
    }

    [TestMethod]
    public async Task ProductsController_DeletePost_Fails_WhenDeleteNotSucceeds_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is the same as User id
        var fakeProduct = A.Fake<Product>();
        fakeProduct.SubmitterId = _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(fakeProduct);
            // Delete does NOT succeed
        A.CallTo(() => _shoppingServices.Product.TryDeleteAsync(A<Product>._))
            .Returns((false, [ new Message { Type = Message.MessageType.Error, Content = "blablabla" } ]));
        // Act
        IActionResult result = await _productsController.DeleteConfirmed(id);
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
        Assert.IsTrue(_messageHandler.Peek(_productsController.TempData)?.Any(m => m.Type is Message.MessageType.Error), "Expected error mesage was not returned");
    }

    #endregion Delete

    #region Image

    [TestMethod]
    public async Task ProductsController_ResetImakge_Succeeds_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is the same as User id
        var fakeProduct = A.Fake<Product>();
        fakeProduct.SubmitterId = _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
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
    public async Task ProductsController_ResetImage_Fails_WhenIdNotExist_WithNotFound()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id does NOT exists, Product is NOT found
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
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
    public async Task ProductsController_ResetImage_Fails_WithSubmitterIdNotUserId_WithUnauthorized()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is the same as User id
        var fakeProduct = A.Fake<Product>();
        fakeProduct.SubmitterId = "Not " + _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
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
    public async Task ProductsController_ResetImage_Fails_WhenDeleteImageFails_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
            // Product.SubmitterId is the same as User id
        var fakeProduct = A.Fake<Product>();
        fakeProduct.SubmitterId = _productsController.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
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
