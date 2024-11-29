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
using SP_Shopping.Repository;
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
    private readonly IRepository<CartItem> _cartItemRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IMessageHandler _messageHandler;
    private readonly CartItemService _cartItemService;
    private readonly CartController _cartController;

    public AdminCartControllerTests()
    {
        _logger = new NullLogger<CartController>();
        _mapper = A.Fake<IMapper>();
        _cartItemRepository = A.Fake<IRepository<CartItem>>();
        _userRepository = A.Fake<IRepository<ApplicationUser>>();
        _productRepository = A.Fake<IRepository<Product>>();
        _messageHandler = new MessageHandler();
        _cartItemService = new CartItemService(_cartItemRepository);

        // SUT

        _cartController = new CartController
        (
            logger: _logger,
            mapper: _mapper,
            cartItemRepository: _cartItemRepository,
            userRepository: _userRepository,
            productRepository: _productRepository,
            messageHandler: _messageHandler,
            cartItemService: _cartItemService
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
        IActionResult result = _cartController.Create();
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
        A.CallTo(() => _userRepository.ExistsAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(true);
            // Id exists, Product found
        A.CallTo(() => _productRepository.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(true);
            // Create succeeds
        A.CallTo(() => _cartItemRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(true);
        // Act
        IActionResult result = await _cartController.Create(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index");
    }

    [TestMethod]
    public async Task CartController_CreatePost_Fails_WhenModelStateNotValid_WithRedirect()
    {
        // Arrange
            // ModelState is NOT correct
        _cartController.ModelState.AddModelError(nameof(AdminCartItemCreateDto.Count), "Count is invalid");
        // Act
        IActionResult result = await _cartController.Create(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", $"Redirected is not index, but {redirectResult.ActionName}");
            // Message not succeed
        Assert.IsTrue(_messageHandler.Peek(_cartController.TempData)?.Any(m => m.Type is Message.MessageType.Warning), "Expected warning message does not exist");
    }

    [TestMethod]
    public async Task CartController_CreatePost_Fails_WhenUserIdNotExist_WithBadRequest()
    {
        // Arrange
            // ModelState is correct
            // Id does NOT exist, User is NOT found
        A.CallTo(() => _userRepository.ExistsAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _cartController.Create(A.Fake<AdminCartItemCreateDto>());
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
        A.CallTo(() => _userRepository.ExistsAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(true);
            // Id does NOT exist, Product is NOT found
        A.CallTo(() => _productRepository.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _cartController.Create(A.Fake<AdminCartItemCreateDto>());
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
        A.CallTo(() => _userRepository.ExistsAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(true);
            // Id exists, Product found
        A.CallTo(() => _productRepository.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(true);
            // Create does NOT succeed
        A.CallTo(() => _cartItemRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _cartController.Create(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<AdminCartItemCreateDto>(viewResult.Model);
    }

    #endregion Create

    #region Edit

    [TestMethod]
    public async Task CartController_EditPost_Succeeds_WithRedirect()
    {
        // Arrange
            // Modelstate valid
            // Update succeeds
        A.CallTo(() => _cartItemRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(true);
        // Act
        IActionResult result = await _cartController.Edit(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", $"Redirected is not index, but {redirectResult.ActionName}");
    }

    [TestMethod]
    public async Task CartController_EditPost_Fails_WhenModelStateNotValid_WithRedirect()
    {
        // Arrange
            // Modelstate is NOT valid
        _cartController.ModelState.AddModelError(nameof(AdminCartItemCreateDto.Count), "errmsg");
        // Act
        IActionResult result = await _cartController.Edit(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", $"Redirected is not index, but {redirectResult.ActionName}");
            // Message not succeed
        Assert.IsTrue(_messageHandler.Peek(_cartController.TempData)?.Any(m => m.Type is Message.MessageType.Warning), "Expected warning message does not exist");
    }

    [TestMethod]
    public async Task CartController_EditPost_Fails_WhenEditFails_WithViewResult()
    {
        // Arrange
            // Modelstate valid
            // Update does NOT succeed
        A.CallTo(() => _cartItemRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _cartController.Edit(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<AdminCartItemCreateDto>(viewResult.Model);
    }

    #endregion Edit

    #region Delete

    [TestMethod]
    public async Task CartController_DeletePost_Succeeds_WithRedirect()
    {
        // Arrange
            // Modelstate valid
            // Update succeeds
        A.CallTo(() => _cartItemRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(true);
        // Act
        IActionResult result = await _cartController.Delete(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", $"Redirected is not index, but {redirectResult.ActionName}");
    }

    [TestMethod]
    public async Task CartController_DeletePost_Fails_WhenModelStateNotValid_WithRedirect()
    {
        // Arrange
            // Modelstate is NOT valid
        _cartController.ModelState.AddModelError(nameof(AdminCartItemCreateDto.Count), "errmsg");
        // Act
        IActionResult result = await _cartController.Delete(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", $"Redirected is not index, but {redirectResult.ActionName}");
            // Message not succeed
        Assert.IsTrue(_messageHandler.Peek(_cartController.TempData)?.Any(m => m.Type is Message.MessageType.Warning), "Expected warning message does not exist");
    }

    [TestMethod]
    public async Task CartController_DeletePost_Fails_WhenDeleteFails_WithViewResult()
    {
        // Arrange
            // Modelstate valid
            // Update does NOT succeed
        A.CallTo(() => _cartItemRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _cartController.Delete(A.Fake<AdminCartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<AdminCartItemCreateDto>(viewResult.Model);
    }

    #endregion Delete

}
