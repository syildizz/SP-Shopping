using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SP_Shopping.Areas.Admin.Controllers;
using SP_Shopping.Areas.Admin.Dtos.User;
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
public class AdminUserControllerTests
{
    private readonly ILogger<UserController> _logger;
    private readonly IMapper _mapper;
    private readonly IShoppingServices _shoppingServices;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IMessageHandler _messageHandler;
    private readonly IImageHandlerDefaulting<UserProfileImageKey> _profileImageHandler;
    private readonly UserController _adminUserController;

    public AdminUserControllerTests()
    {
        _logger = new NullLogger<UserController>();
        _mapper = A.Fake<IMapper>();
        _shoppingServices = A.Fake<IShoppingServices>();
        _userManager = A.Fake<UserManager<ApplicationUser>>();
        _signInManager = A.Fake<SignInManager<ApplicationUser>>();
        _messageHandler = new MessageHandler();
        _profileImageHandler = A.Fake<ImageHandlerDefaulting<UserProfileImageKey>>();

        _adminUserController = new UserController
        (
            logger: _logger,
            mapper: _mapper,
            shoppingServices: _shoppingServices,
            userManager: _userManager,
            signInManager: _signInManager,
            messageHandler: _messageHandler,
            profileImageHandler: _profileImageHandler
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

        _adminUserController.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
            {
                // Set fake user
                User = fakeUser
            }
        };

        // Set fake TempData
        _adminUserController.TempData = new TempDataDictionary(_adminUserController.HttpContext, A.Fake<ITempDataProvider>());
    }

    [DataRow("Edit", (Type[])[typeof(string)])]
    [DataRow("Edit", (Type[])[typeof(AdminUserEditDto)])]
    [DataRow("Delete", (Type[])[typeof(string)])]
    [DataRow("Adminize", (Type[])[typeof(string)])]
    [DataRow("Unadminize", (Type[])[typeof(string)])]
    [DataTestMethod]
    public void AdminUserController_All_Succeeds_WhenAuthorizedAsAdmin(string methodName, Type[] types)
    {
        // Arrange
        var controller = typeof(UserController);
        var action = controller.GetMethod(methodName, types);
        // Act
        var hasAuthorization = AttributeHandler.HasAuthorizationAttributes(controller, action, checkRole: "Admin");
        // Assert
        Assert.IsTrue(hasAuthorization, $"Action {methodName} should be authorized as admin but it isn't");
    }

    [DataRow("Edit", (Type[])[typeof(string)])]
    [DataRow("Delete", (Type[])[typeof(string)])]
    [DataRow("Adminize", (Type[])[typeof(string)])]
    [DataRow("Unadminize", (Type[])[typeof(string)])]
    [DataRow("ResetImage", (Type[])[typeof(string)])]
    [DataTestMethod]
    public void UserController_Some_Fails_WhenIdIsNull_WithBadRequest(string methodName, Type[] types)
    {
        // Arrange
        var controller = typeof(UserController);
        var action = controller.GetMethod(methodName, types);
        const string checkedArgument = "id";
        // Act
        var attributes = action?.GetCustomAttribute<IfArgNullBadRequestFilter>(false);
        // Assert
        Assert.IsTrue(attributes is not null and IfArgNullBadRequestFilter, $"Action {methodName} does not have {nameof(IfArgNullBadRequestFilter)} attribute even though it should");
        Assert.IsTrue(attributes.argument is checkedArgument, $"Action {methodName} has {nameof(IfArgNullBadRequestFilter)} attribute but it checks for {checkedArgument} instead of id");
    }

    #region Edit

    [TestMethod]
    public async Task UserController_EditGet_Succeeds_WithViewResult()
    {
        // Arrange
            // Id exists
        const string id = "0";
            // User exists, read succeeds
        A.CallTo(() => _shoppingServices.User.GetSingleAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<AdminUserEditDto>>>._))
            .Returns(A.Fake<AdminUserEditDto>());
        // Act
        IActionResult result = await _adminUserController.Edit(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<AdminUserEditDto>(viewResult.Model); 
    }

    [TestMethod]
    public async Task UserController_EditGet_Fails_WhenUserNotExist_WithNotFound()
    {
        // Arrange
            // Id exists
        const string id = "0";
            // User does NOT exists read fails
        A.CallTo(() => _shoppingServices.User.GetSingleAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<AdminUserEditDto>>>._))
            .Returns((AdminUserEditDto?)null);
        // Act
        IActionResult result = await _adminUserController.Edit(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult, $"Expected type {nameof(NotFoundResult)} or {nameof(NotFoundObjectResult)} but got type {result.GetType().Name}");
    }

    [TestMethod]
    public async Task UserController_EditPost_Succeeds_WithRedirect()
    {
        // Arrange
            // Modelstate is correct
            // Update succeeds
        A.CallTo(() => _shoppingServices.User.TryUpdateAsync(An<ApplicationUser>._, An<IFormFile?>._))
            .Returns((true, null));
        A.CallTo(() => _shoppingServices.Role.GetAllAsync(A<Func<IQueryable<ApplicationRole>, IQueryable<ApplicationRole>>>._))
            .Returns(A.Fake<List<ApplicationRole>>());
        
        // Act
        IActionResult result = await _adminUserController.Edit(A.Fake<AdminUserEditDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", $"Redirected is not Index, but {redirectResult.ActionName}");
            // Update attempted
        A.CallTo(() => _shoppingServices.User.TryUpdateAsync(An<ApplicationUser>._, An<IFormFile?>._))
            .MustHaveHappened();
    }

    [TestMethod]
    public async Task UserController_EditPost_Fails_WhenModelStateIsNotValid_WithRedirect()
    {
        // Arrange
        const string id = "000";
        var user = A.Fake<AdminUserEditDto>();
        user.Id = id;
            // Modelstate is NOT correct
        _adminUserController.ModelState.AddModelError(nameof(AdminUserEditDto.UserName), "Invalid username");
        // Act
        IActionResult result = await _adminUserController.Edit(user);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Edit", $"Redirected is not Edit, but {redirectResult.ActionName}");
        Assert.IsTrue((string?)redirectResult.RouteValues?["id"] is not null and id, "Wrong route value for Edit");
    }

    [TestMethod]
    public async Task UserController_EditPost_Fails_WhenUserNotExist_WithRedirect()
    {
        // Arrange
        const string id = "000";
        var user = A.Fake<AdminUserEditDto>();
        user.Id = id;
            // Modelstate is correct
            // Update does NOT succeed
        A.CallTo(() => _shoppingServices.User.TryUpdateAsync(An<ApplicationUser>._, An<IFormFile?>._))
            .Returns((false, [ new Message { Type = Message.MessageType.Error, Content = "blabla" }]));
        // Act
        IActionResult result = await _adminUserController.Edit(user);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Edit", $"Redirected is not Edit, but {redirectResult.ActionName}");
        Assert.IsTrue((string?)redirectResult.RouteValues?["id"] is not null and id, "Wrong route value for Edit");
            // Update attempted
        A.CallTo(() => _shoppingServices.User.TryUpdateAsync(An<ApplicationUser>._, An<IFormFile?>._))
            .MustHaveHappened();
            // Message error
        Assert.IsTrue(_messageHandler.Peek(_adminUserController.TempData)?.Any(m => m.Type is Message.MessageType.Error), "Expected error mesage was not returned");
    }

    #endregion Edit

    #region Delete

    [TestMethod]
    public async Task UserController_DeletePost_Succeeds_WithRedirect()
    {
        // Arrange
            // User exists
        A.CallTo(() => _shoppingServices.User.GetSingleAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(A.Fake<ApplicationUser>());
            // Delete succeeds
        A.CallTo(() => _shoppingServices.User.TryDeleteAsync(An<ApplicationUser>._))
            .Returns((true, null));
        // Act
        IActionResult result = await _adminUserController.Delete("0");
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", $"Redirected is not Index, but {redirectResult.ActionName}");
            // Delete attempted
        A.CallTo(() => _shoppingServices.User.TryDeleteAsync(An<ApplicationUser>._))
            .MustHaveHappened();
    }

    [TestMethod]
    public async Task UserController_DeletePost_Fails_WhenUserNotExist_WithRedirect()
    {
        // Arrange
            // User does NOT exist
        A.CallTo(() => _shoppingServices.User.GetSingleAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns((ApplicationUser?)null);
        // Act
        IActionResult result = await _adminUserController.Delete("0");
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", $"Redirected is not Index, but {redirectResult.ActionName}");
    }

    [TestMethod]
    public async Task UserController_DeletePost_Fails_WhenDeleteFails_WithRedirect()
    {
        // Arrange
            // User exists
        A.CallTo(() => _shoppingServices.User.GetSingleAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(A.Fake<ApplicationUser>());
            // Delete does NOT succeed
        A.CallTo(() => _shoppingServices.User.TryDeleteAsync(An<ApplicationUser>._))
            .Returns((false, [ new Message { Type = Message.MessageType.Error, Content = "blabla" }]));
        // Act
        IActionResult result = await _adminUserController.Delete("0");
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", $"Redirected is not Index, but {redirectResult.ActionName}");
            // Delete attempted
        A.CallTo(() => _shoppingServices.User.TryDeleteAsync(An<ApplicationUser>._))
            .MustHaveHappened();
            // Message error
        Assert.IsTrue(_messageHandler.Peek(_adminUserController.TempData)?.Any(m => m.Type is Message.MessageType.Error), "Expected error mesage was not returned");
    }

    #endregion Delete

    #region Image

    public async Task UserController_ResetImage_Succeeds_WithRedirect()
    {
        // Arrange
            // Id exists
        const string id = "000";
            // User exists
        A.CallTo(() => _shoppingServices.User.ExistsAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(true);
            // Delete succeeds
        A.CallTo(() => _profileImageHandler.DeleteImage(A<UserProfileImageKey>._))
            .DoesNothing();
        // Act
        IActionResult result = await _adminUserController.ResetImage(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Edit", $"Redirected is not Edit, but {redirectResult.ActionName}");
        Assert.IsTrue((string?)redirectResult.RouteValues?["id"] is not null and id, "Wrong route value for Edit");
            // Delete attempted
        A.CallTo(() => _profileImageHandler.DeleteImage(A<UserProfileImageKey>._))
            .MustHaveHappened();
    }

    public async Task UserController_ResetImage_Fails_WhenUserNotExist_WithNotFound()
    {
        // Arrange
            // Id exists
        const string id = "000";
            // User does NOT exist
        A.CallTo(() => _shoppingServices.User.ExistsAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _adminUserController.ResetImage(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult, $"Expected type {nameof(NotFoundResult)} or {nameof(NotFoundObjectResult)} but got type {result.GetType().Name}");
    }

    public async Task UserController_ResetImage_Fails_WhenDeleteFails_WithRedirect_SameasSucceed()
    {
        // Arrange
            // Id exists
        const string id = "000";
            // User exists
        A.CallTo(() => _shoppingServices.User.ExistsAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(true);
            // Delete does NOT succeed
        A.CallTo(() => _profileImageHandler.DeleteImage(A<UserProfileImageKey>._))
            .Throws<Exception>();
        // Act
        IActionResult result = await _adminUserController.ResetImage(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Edit", $"Redirected is not Edit, but {redirectResult.ActionName}");
        Assert.IsTrue((string?)redirectResult.RouteValues?["id"] is not null and id, "Wrong route value for Edit");
            // Delete attempted
        A.CallTo(() => _profileImageHandler.DeleteImage(A<UserProfileImageKey>._))
            .MustHaveHappened();
            // Message error
        Assert.IsTrue(_messageHandler.Peek(_adminUserController.TempData)?.Any(m => m.Type is Message.MessageType.Error), "Expected error mesage was not returned");
    }

    #endregion Image

}
