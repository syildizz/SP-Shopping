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
using SP_Shopping.Repository;
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
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IMessageHandler _messageHandler;
    private readonly ILogger<UserController> _logger;
    private readonly IMapper _mapper;
    private readonly IImageHandlerDefaulting<UserProfileImageKey> _profileImageHandler;
    private readonly UserService _userService;
    private readonly UserController _adminUserController;

    public AdminUserControllerTests()
    {
        _userRepository = A.Fake<IRepository<ApplicationUser>>();
        _userManager = A.Fake<UserManager<ApplicationUser>>();
        _signInManager = A.Fake<SignInManager<ApplicationUser>>();
        _messageHandler = new MessageHandler();
        _logger = new NullLogger<UserController>();
        _mapper = A.Fake<IMapper>();
        _profileImageHandler = A.Fake<ImageHandlerDefaulting<UserProfileImageKey>>();
        _userService = new UserService
        (
            _userRepository,
            A.Fake<IRepository<Product>>(),
            _userManager,
            _profileImageHandler,
            _messageHandler,
            new ProductService
            (
                A.Fake<IRepository<Product>>(),
                A.Fake<ImageHandlerDefaulting<ProductImageKey>>()
            )
        );

        _adminUserController = new UserController
        (
            userRepository: _userRepository,
            userManager: _userManager,
            signInManager: _signInManager,
            messageHandler: _messageHandler,
            logger: _logger,
            mapper: _mapper,
            profileImageHandler: _profileImageHandler,
            userService: _userService
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
        A.CallTo(() => _userRepository.GetSingleAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<AdminUserEditDto>>>._))
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
        A.CallTo(() => _userRepository.GetSingleAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<AdminUserEditDto>>>._))
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
        A.CallTo(() => _userRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(true);
        // Act
        IActionResult result = await _adminUserController.Edit(A.Fake<AdminUserEditDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", $"Redirected is not Index, but {redirectResult.ActionName}");
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
        A.CallTo(() => _userRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _adminUserController.Edit(user);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Edit", $"Redirected is not Edit, but {redirectResult.ActionName}");
        Assert.IsTrue((string?)redirectResult.RouteValues?["id"] is not null and id, "Wrong route value for Edit");
    }

    #endregion Edit

    #region Delete

    [TestMethod]
    public async Task UserController_DeletePost_Succeeds_WithRedirect()
    {
        // Arrange
            // User exists
        A.CallTo(() => _userRepository.GetSingleAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(A.Fake<ApplicationUser>());
            // Delete succeeds
        A.CallTo(() => _userRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(true);
        // Act
        IActionResult result = await _adminUserController.Delete("0");
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", $"Redirected is not Index, but {redirectResult.ActionName}");
    }

    [TestMethod]
    public async Task UserController_DeletePost_Fails_WhenUserNotExist_WithRedirect()
    {
        // Arrange
            // User does NOT exist
        A.CallTo(() => _userRepository.GetSingleAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
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
        A.CallTo(() => _userRepository.GetSingleAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(A.Fake<ApplicationUser>());
            // Delete does NOT succeed
        A.CallTo(() => _userRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _adminUserController.Delete("0");
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", $"Redirected is not Index, but {redirectResult.ActionName}");
    }

    #endregion Delete

    #region Image

    public async Task UserController_ResetImage_Succeeds_WithRedirect()
    {
        // Arrange
            // Id exists
        const string id = "000";
            // User exists
        A.CallTo(() => _userRepository.ExistsAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(true);
            // Delete succeeds
        A.CallTo(() => _userRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(true);
        // Act
        IActionResult result = await _adminUserController.ResetImage(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Edit", $"Redirected is not Edit, but {redirectResult.ActionName}");
        Assert.IsTrue((string?)redirectResult.RouteValues?["id"] is not null and id, "Wrong route value for Edit");
    }

    public async Task UserController_ResetImage_Fails_WhenUserNotExist_WithNotFound()
    {
        // Arrange
            // Id exists
        const string id = "000";
            // User does NOT exist
        A.CallTo(() => _userRepository.ExistsAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
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
        A.CallTo(() => _userRepository.ExistsAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._))
            .Returns(true);
            // Delete does NOT succeed
        A.CallTo(() => _userRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _adminUserController.ResetImage(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Edit", $"Redirected is not Edit, but {redirectResult.ActionName}");
        Assert.IsTrue((string?)redirectResult.RouteValues?["id"] is not null and id, "Wrong route value for Edit");
    }

    #endregion Image

}
