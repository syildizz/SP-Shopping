using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SP_Shopping.Controllers;
using SP_Shopping.Dtos.User;
using SP_Shopping.Models;
using SP_Shopping.Service;
using SP_Shopping.Test.TestingUtilities;
using SP_Shopping.Utilities.Filter;
using System.Security.Claims;

namespace SP_Shopping.Test.Controllers;

[TestClass]
public class UserControllerTests
{
    private readonly ILogger<UserController> _logger;
    private readonly IMapper _mapper;
    private readonly IShoppingServices _shoppingServices;
    private readonly UserController _userController;

    public UserControllerTests()
    {
        _logger = new NullLogger<UserController>();
        _mapper = A.Fake<IMapper>();
        _shoppingServices = A.Fake<IShoppingServices>();


        // SUT

        _userController = new UserController
        (
            logger: _logger,
            mapper: _mapper,
            shoppingServices: _shoppingServices

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

        _userController.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
            {
                // Set fake user
                User = fakeUser
            }
        };
    }

    #region Index

    [TestMethod]
    public async Task UserController_Index_Succeeds_WithViewResult()
    {
        // Arrange 
            // Id is not null
        const string id = "0";
            // Id exists, User is found
        var fakeUserPage = A.Fake<UserPageDto>();
        fakeUserPage.Id = id;
        fakeUserPage.ProductDetails = (List<UserPageDto.UserPageProductDto>)A.CollectionOfFake<UserPageDto.UserPageProductDto>(5);
        A.CallTo(() => _shoppingServices.User.GetSingleAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<UserPageDto>>>._))
            .Returns(fakeUserPage);
        // Act
        IActionResult result = await _userController.Index(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<UserPageDto>(viewResult.Model);
            // Count of Model's products is same as input's products
        var viewResultModel = (UserPageDto)viewResult.Model;
        Assert.IsTrue(viewResultModel.ProductDetails?.Count() == fakeUserPage.ProductDetails.Count());
    }

    [TestMethod]
    public void UserController_Index_Succeeds_WhenNotAuthorized()
    {
        // Arrange
        var controller = typeof(UserController);
        var action = controller.GetMethod("Index");
        // Act
        var hasAuthorization = AttributeHandler.HasAuthorizationAttributes(controller, action);
        // Assert
        Assert.IsTrue(!hasAuthorization, "Action should not be authorized but it is");
    }


    [TestMethod]
    public void UserController_Index_Fails_WhenIdNull_WithBadRequest()
    {
        // Arrange
        var action = typeof(UserController).GetMethod("Index");
        // Act
        var attributes = action?.GetCustomAttributes(typeof(IfArgNullBadRequestFilter), false);
        // Assert
        Assert.IsTrue(!attributes.IsNullOrEmpty(), $"Action does not have {nameof(IfArgNullBadRequestFilter)} attribute even though it should");
    }

    [TestMethod]
    public async Task UserController_Index_Fails_WhenIdNotExists_WithNotFound()
    {
        // Arrange 
            // Id is not null
        const string id = "0";
            // Id does NOT exist, User is NOT found
        A.CallTo(() => _shoppingServices.User.GetSingleAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<UserPageDto>>>._))
            .Returns((UserPageDto?)null);
        // Act
        IActionResult result = await _userController.Index(id);
        // Assert
        // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
    }

    #endregion Index

}

