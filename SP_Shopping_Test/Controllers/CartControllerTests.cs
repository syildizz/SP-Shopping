using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SP_Shopping.Controllers;
using SP_Shopping.Dtos.Cart;
using SP_Shopping.Models;
using SP_Shopping.Service;
using SP_Shopping.Test.TestingUtilities;
using SP_Shopping.Utilities.Filters;
using SP_Shopping.Utilities.MessageHandler;
using System.Reflection;
using System.Security.Claims;

namespace SP_Shopping.Test.Controllers;

[TestClass]
public class CartControllerTests
{

    private readonly ILogger<CartController> _logger;
    private readonly IMapper _mapper;
    private readonly IShoppingServices _shoppingServices;
    private readonly IMessageHandler _messageHandler;
    private readonly CartController _cartController;

    public CartControllerTests()
    {
        _logger = new NullLogger<CartController>();
        _mapper = A.Fake<IMapper>();
        _shoppingServices = A.Fake<IShoppingServices>();
        _messageHandler = new MessageHandler();

        // SUT

        _cartController = new CartController
        (
            logger: _logger,
            mapper: _mapper,
            shoppingServices: _shoppingServices,
            messageHandler: _messageHandler
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

        _cartController.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
            {
                // Set fake user
                User = fakeUser
            }
        };

        // Set fake TempData
        _cartController.TempData = new TempDataDictionary(_cartController.HttpContext, A.Fake<ITempDataProvider>());
    }

    [DataRow("Index", (Type[])[])]
    [DataRow("Create",  (Type[])[typeof(int?)])]
    [DataRow("Edit",  (Type[])[typeof(int?), typeof(CartItemCreateDto)])]
    [DataRow("Delete",  (Type[])[typeof(int?)])]
    [DataTestMethod]
    public void CartController_All_Succeeds_WhenAuthorized(string methodName, Type[] types)
    {
        // Arrange
        var controller = typeof(CartController);
        var action = controller.GetMethod(methodName, types);
        // Act
        var hasAuthorization = AttributeHandler.HasAuthorizationAttributes(controller, action);
        // Assert
        Assert.IsTrue(hasAuthorization, $"Action {methodName} should be authorized but it isn't");
    }

    [DataRow("Create", (Type[])[typeof(int?)])]
    [DataRow("Edit", (Type[])[typeof(int?), typeof(CartItemCreateDto)])]
    [DataRow("Delete", (Type[])[typeof(int?)])]
    [DataTestMethod]
    public void CartController_Some_Fails_WhenIdIsNull_WithBadRequest(string methodName, Type[] types)
    {
        // Arrange
        var controller = typeof(CartController);
        var action = controller.GetMethod(methodName, types);
        const string checkedArgument = "id";
        // Act
        var attributes = action?.GetCustomAttribute<IfArgNullBadRequestFilter>(false);
        // Assert
        Assert.IsTrue(attributes is not null and IfArgNullBadRequestFilter, $"Action {methodName} does not have {nameof(IfArgNullBadRequestFilter)} attribute even though it should");
        Assert.IsTrue(attributes.argument is checkedArgument, $"Action {methodName} has {nameof(IfArgNullBadRequestFilter)} attribute but it checks for {checkedArgument} instead of id");
    }


    #region Index

    [TestMethod]
    public async Task CartController_Index_Succeeds_WithViewResult()
    {
        // Arrange 
            // Cart for User exists
        var cartItems = (List<CartItemDetailsDto>)A.CollectionOfFake<CartItemDetailsDto>(5);
        var prices = Enumerable.Range(1, 5);
        cartItems = cartItems.Zip(prices, (c, p) => { c.Price = p; c.Count = 1; return c; }).ToList();
        A.CallTo(() => _shoppingServices.CartItem.GetAllAsync(A<Func<IQueryable<CartItem>, IQueryable<CartItemDetailsDto>>>._))
            .Returns(cartItems);
        // Act
        IActionResult result = await _cartController.Index();
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<IEnumerable<CartItemDetailsDto>>(viewResult.Model);
            // Count of Model is same as input
        var viewResultModel = (IEnumerable<CartItemDetailsDto>)viewResult.Model;
        Assert.IsTrue(viewResultModel.Count() == cartItems.Count);
            // ViewData exists
        Assert.IsInstanceOfType<decimal>(viewResult.ViewData["TotalPrice"]);
        Assert.IsTrue((decimal)viewResult.ViewData["TotalPrice"]! == prices.Sum());
    }

    #endregion Index

    #region Create

    [TestMethod]
    public async Task CartController_CreatePost_Succeeds_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product found
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(true);
            // Create succeeds
        A.CallTo(() => _shoppingServices.CartItem.TryCreateAsync(A<CartItem>._))
            .Returns((true, null));
        // Act
        IActionResult result = await _cartController.Create(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index");
            // Create attempted
        A.CallTo(() => _shoppingServices.CartItem.TryCreateAsync(A<CartItem>._))
            .MustHaveHappened();
    }

    [TestMethod]
    public async Task CartController_CreatePost_Fails_WhenIdNotExist_WithNotFound()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id does NOT exist, Product is NOT found
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _cartController.Create(id);
        // Assert
        // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
    }

    [TestMethod]
    public async Task CartController_CreatePost_Fails_WhenCreateNotSucceed_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product found
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(true);
        // Create does NOT succeed
        A.CallTo(() => _shoppingServices.CartItem.TryCreateAsync(A<CartItem>._))
            .Returns((false, [new Message { Type = Message.MessageType.Error, Content = "blabla"}]));
        // Act
        IActionResult result = await _cartController.Create(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index");
            // Message is error
        Assert.IsTrue(_messageHandler.Peek(_cartController.TempData)?.Any(m => m.Type is Message.MessageType.Error), "Expected error mesage was not returned");
    }

    #endregion Create

    #region Edit

    [TestMethod]
    public async Task CartController_EditPost_Succeeds_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Modelstate valid
            // Update succeeds
        A.CallTo(() => _shoppingServices.CartItem.TryUpdateAsync(A<CartItem>._))
            .Returns((true, null));
        // Act
        IActionResult result = await _cartController.Edit(id, A.Fake<CartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index");
    }

    [TestMethod]
    public async Task CartController_EditPost_Fails_WhenModelStateNotValid_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Modelstate is NOT valid
        _cartController.ModelState.AddModelError("userId", "Invalid user id");
        // Act
        IActionResult result = await _cartController.Edit(id, A.Fake<CartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index");
            // Message not succeed
        Assert.IsTrue(_messageHandler.Peek(_cartController.TempData)?.Any(m => m.Type is Message.MessageType.Warning), "Expected warning mesage was not returned");
    }

    [TestMethod]
    public async Task CartController_EditPost_Fails_WhenEditNotSucceed_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Modelstate valid
            // Update does NOT succeed
        A.CallTo(() => _shoppingServices.CartItem.TryUpdateAsync(A<CartItem>._))
            .Returns((false, [new Message { Type = Message.MessageType.Error, Content = "blabla" }]));
        // Act
        IActionResult result = await _cartController.Edit(id, A.Fake<CartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index");
            // Update attempted
        A.CallTo(() => _shoppingServices.CartItem.TryUpdateAsync(A<CartItem>._))
            .MustHaveHappened();
            // Message error
        Assert.IsTrue(_messageHandler.Peek(_cartController.TempData)?.Any(m => m.Type is Message.MessageType.Error), "Expected error mesage was not returned");
    }

    #endregion Edit

    #region Delete

    [TestMethod]
    public async Task CartController_DeletePost_Succeeds_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Delete succeeds
        A.CallTo(() => _shoppingServices.CartItem.TryDeleteAsync(A<CartItem>._))
            .Returns((true, null));
        // Act
        IActionResult result = await _cartController.Delete(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result, "Action result is not redirectToAction");
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", "Does not redirect to Index");
            // Delete attempted
        A.CallTo(() => _shoppingServices.CartItem.TryDeleteAsync(A<CartItem>._))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task CartController_DeletePost_Fails_WhenDeleteFails_WithRedirect()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Delete does NOT succeed
        A.CallTo(() => _shoppingServices.CartItem.TryDeleteAsync(A<CartItem>._))
            .Returns((false, [new Message { Type = Message.MessageType.Error, Content = "blabla" }]));
        // Act
        IActionResult result = await _cartController.Delete(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result, "Action result is not redirectToAction");
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", "Does not redirect to Index");
            // Attempt delete
        A.CallTo(() => _shoppingServices.CartItem.TryDeleteAsync(A<CartItem>._))
            .MustHaveHappenedOnceExactly();
            // Delete attempted
        Assert.IsTrue(_messageHandler.Peek(_cartController.TempData)?.Any(m => m.Type is Message.MessageType.Error), "Expected error mesage was not returned");
    }

    #endregion Delete

}
