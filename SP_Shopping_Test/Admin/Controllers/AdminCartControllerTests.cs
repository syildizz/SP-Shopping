using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SP_Shopping.Areas.Admin.Controllers;
using SP_Shopping.Areas.Admin.Dtos.Cart;
using SP_Shopping.Models;
using SP_Shopping.Service;
using SP_Shopping.Test.TestingUtilities;
using SP_Shopping.Utilities.MessageHandler;
using System.Security.Claims;

namespace SP_Shopping.Test.Admin.Controllers;

[TestClass]
public class AdminCartControllerTests
{

    private readonly ILogger<CartController> _logger;
    private readonly IMapper _mapper;
    private readonly IShoppingServices _shoppingServices;
    private readonly IMessageHandler _messageHandler;
    private readonly CartController _adminCartController;

    public AdminCartControllerTests()
    {
        _logger = new NullLogger<CartController>();
        _mapper = A.Fake<IMapper>();
        _shoppingServices = A.Fake<IShoppingServices>();
        _messageHandler = new MessageHandler();

        // SUT

        _adminCartController = new CartController
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

        _adminCartController.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
            {
                // Set fake user
                User = fakeUser
            }
        };

        // Set fake TempData
        _adminCartController.TempData = new TempDataDictionary(_adminCartController.HttpContext, A.Fake<ITempDataProvider>());
    }

    [DataRow("Index", (Type[])[])]
    [DataRow("Create", (Type[])[])]
    [DataRow("Create", (Type[])[typeof(AdminCartItemCreateDto)])]
    [DataRow("Edit", (Type[])[typeof(int?), typeof(AdminCartItemCreateDto)])]
    [DataRow("Delete", (Type[])[typeof(AdminCartItemCreateDto)])]
    [DataTestMethod]
    public void CartController_All_Succeeds_WhenAuthorizedAsAdmin(string methodName, Type[] types)
    {
        // Arrange
        var controller = typeof(CartController);
        var action = controller.GetMethod(methodName, types);
        // Act
        var hasAuthorization = AttributeHandler.HasAuthorizationAttributes(controller, action, checkRole: "Admin");
        // Assert
        Assert.IsTrue(hasAuthorization, $"Action {methodName} should be authorized as admin but it isn't");
    }

    #region Create

    [TestMethod]
    public void CartController_CreateGet_Succeeds_WithViewResult()
    {
        // Arrange 
        // Act
        IActionResult result = _adminCartController.Create();
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<AdminCartItemCreateDto>(viewResult.Model);
    }

    [TestMethod]
    public async Task CartController_CreatePost_Succeeds_WithRedirect()
    {
        // Arrange
            // ModelState is correct
            // Id exists, User is found
        A.CallTo(() => _shoppingServices.User.ExistsAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(true);
            // Id exists, Product found
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(true);
            // Create succeeds
        A.CallTo(() => _shoppingServices.CartItem.TryCreateAsync(A<CartItem>._))
            .Returns((true, null));
        // Act
        IActionResult result = await _adminCartController.Create(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index");
            // Create attempted
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .MustHaveHappened();
    }

    [TestMethod]
    public async Task CartController_CreatePost_Fails_WhenModelStateNotValid_WithRedirect()
    {
        // Arrange
            // ModelState is NOT correct
        _adminCartController.ModelState.AddModelError(nameof(AdminCartItemCreateDto.Count), "Count is invalid");
        // Act
        IActionResult result = await _adminCartController.Create(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", $"Redirected is not index, but {redirectResult.ActionName}");
            // Message not succeed
        Assert.IsTrue(_messageHandler.Peek(_adminCartController.TempData)?.Any(m => m.Type is Message.MessageType.Warning), "Expected warning message does not exist");
    }

    [TestMethod]
    public async Task CartController_CreatePost_Fails_WhenUserIdNotExist_WithBadRequest()
    {
        // Arrange
            // ModelState is correct
            // Id does NOT exist, User is NOT found
        A.CallTo(() => _shoppingServices.User.ExistsAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _adminCartController.Create(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsTrue(result is BadRequestResult or BadRequestObjectResult, $"Expected type {nameof(BadRequestResult)} or {nameof(BadRequestObjectResult)} but got type {result.GetType().Name}");
    }

    [TestMethod]
    public async Task CartController_CreatePost_Fails_WhenProductIdNotExist_WithBadRequest()
    {
        // Arrange
            // ModelState is correct
            // Id exists, User is found
        A.CallTo(() => _shoppingServices.User.ExistsAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(true);
            // Id does NOT exist, Product is NOT found
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _adminCartController.Create(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsTrue(result is BadRequestResult or BadRequestObjectResult, $"Expected type {nameof(BadRequestResult)} or {nameof(BadRequestObjectResult)} but got type {result.GetType().Name}");
    }

    [TestMethod]
    public async Task CartController_CreatePost_Fails_WhenCreateFails_WithViewResult()
    {
        // Arrange
            // ModelState is correct
            // Id exists, User is found
        A.CallTo(() => _shoppingServices.User.ExistsAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(true);
            // Id exists, Product found
        A.CallTo(() => _shoppingServices.Product.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(true);
            // Create does NOT succeed
        A.CallTo(() => _shoppingServices.CartItem.TryCreateAsync(A<CartItem>._))
            .Returns((false, [ new Message { Type = Message.MessageType.Error, Content = "blabla" }]));
        // Act
        IActionResult result = await _adminCartController.Create(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<AdminCartItemCreateDto>(viewResult.Model);
            // Create attempted
        A.CallTo(() => _shoppingServices.CartItem.TryCreateAsync(A<CartItem>._))
            .MustHaveHappened();
            // Message error
        Assert.IsTrue(_messageHandler.Peek(_adminCartController.TempData)?.Any(m => m.Type is Message.MessageType.Error), "Expected error mesage was not returned");
    }

    #endregion Create

    #region Edit

    [TestMethod]
    public async Task CartController_EditPost_Succeeds_WithRedirect()
    {
        // Arrange
            // Modelstate valid
            // Update succeeds
        A.CallTo(() => _shoppingServices.CartItem.TryUpdateAsync(A<CartItem>._))
            .Returns((true, null));
        // Act
        IActionResult result = await _adminCartController.Edit(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", $"Redirected is not index, but {redirectResult.ActionName}");
            // Update attempted
        A.CallTo(() => _shoppingServices.CartItem.TryUpdateAsync(A<CartItem>._))
            .MustHaveHappened();
    }

    [TestMethod]
    public async Task CartController_EditPost_Fails_WhenModelStateNotValid_WithRedirect()
    {
        // Arrange
            // Modelstate is NOT valid
        _adminCartController.ModelState.AddModelError(nameof(AdminCartItemCreateDto.Count), "errmsg");
        // Act
        IActionResult result = await _adminCartController.Edit(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", $"Redirected is not index, but {redirectResult.ActionName}");
            // Message not succeed
        Assert.IsTrue(_messageHandler.Peek(_adminCartController.TempData)?.Any(m => m.Type is Message.MessageType.Warning), "Expected warning message does not exist");
    }

    [TestMethod]
    public async Task CartController_EditPost_Fails_WhenEditFails_WithViewResult()
    {
        // Arrange
            // Modelstate valid
            // Update does NOT succeed
        A.CallTo(() => _shoppingServices.CartItem.TryUpdateAsync(A<CartItem>._))
            .Returns((false, [ new Message { Type = Message.MessageType.Error, Content = "blabla" }]));
        // Act
        IActionResult result = await _adminCartController.Edit(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<AdminCartItemCreateDto>(viewResult.Model);
            // Update attempted
        A.CallTo(() => _shoppingServices.CartItem.TryUpdateAsync(A<CartItem>._))
            .MustHaveHappened();
            // Message error
        Assert.IsTrue(_messageHandler.Peek(_adminCartController.TempData)?.Any(m => m.Type is Message.MessageType.Error), "Expected error mesage was not returned");
    }

    #endregion Edit

    #region Delete

    [TestMethod]
    public async Task CartController_DeletePost_Succeeds_WithRedirect()
    {
        // Arrange
            // Modelstate valid
            // Delete succeeds
        A.CallTo(() => _shoppingServices.CartItem.TryDeleteAsync(A<CartItem>._))
            .Returns((true, null));
        // Act
        IActionResult result = await _adminCartController.Delete(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", $"Redirected is not index, but {redirectResult.ActionName}");
            // Delete attempted
        A.CallTo(() => _shoppingServices.CartItem.TryDeleteAsync(A<CartItem>._))
            .MustHaveHappened();
    }

    [TestMethod]
    public async Task CartController_DeletePost_Fails_WhenModelStateNotValid_WithRedirect()
    {
        // Arrange
            // Modelstate is NOT valid
        _adminCartController.ModelState.AddModelError(nameof(AdminCartItemCreateDto.Count), "errmsg");
        // Act
        IActionResult result = await _adminCartController.Delete(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", $"Redirected is not index, but {redirectResult.ActionName}");
            // Message not succeed
        Assert.IsTrue(_messageHandler.Peek(_adminCartController.TempData)?.Any(m => m.Type is Message.MessageType.Warning), "Expected warning message does not exist");
    }

    [TestMethod]
    public async Task CartController_DeletePost_Fails_WhenDeleteFails_WithViewResult()
    {
        // Arrange
            // Modelstate valid
            // Delete does NOT succeed
        A.CallTo(() => _shoppingServices.CartItem.TryDeleteAsync(A<CartItem>._))
            .Returns((false, [ new Message { Type = Message.MessageType.Error, Content = "blabla" }]));
        // Act
        IActionResult result = await _adminCartController.Delete(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<AdminCartItemCreateDto>(viewResult.Model);
            // Delete attempted
        A.CallTo(() => _shoppingServices.CartItem.TryDeleteAsync(A<CartItem>._))
            .MustHaveHappened();
            // Message error
        Assert.IsTrue(_messageHandler.Peek(_adminCartController.TempData)?.Any(m => m.Type is Message.MessageType.Error), "Expected error mesage was not returned");
    }

    #endregion Delete

}
