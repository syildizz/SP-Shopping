
using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SP_Shopping.Controllers;
using SP_Shopping.Dtos.Product;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Test.TestingUtilities;
using System.Security.Claims;

namespace SP_Shopping.Test.Controllers;

[TestClass]
public class HomeControllerTests
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRepository<Product> _productRepository;
    private readonly IMapper _mapper;
    private readonly HomeController _homeController;
    public HomeControllerTests()
    {
        _logger = new NullLogger<HomeController>();
        _productRepository = A.Fake<IRepository<Product>>();
        _mapper = A.Fake<IMapper>();

        // SUT

        _homeController = new HomeController
        (
            logger: _logger,
            productRepository: _productRepository,
            mapper: _mapper
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

        _homeController.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
            {
                // Set fake user
                User = fakeUser
            }
        };
    }

    [TestMethod]
    public async Task HomeController_Index_Succeeds_WithViewResult()
    {
        // Arrange 
            // Get Products
        List<ProductDetailsDto> products = (List<ProductDetailsDto>)A.CollectionOfFake<ProductDetailsDto>(5);
        A.CallTo(() => _productRepository.GetAllAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .Returns(products);
        // Act
        IActionResult result = await _homeController.Index();
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<IEnumerable<ProductDetailsDto>>(viewResult.Model);
            // Count of Model is same as input
        var viewResultModel = (IEnumerable<ProductDetailsDto>)viewResult.Model;
        Assert.IsTrue(viewResultModel.Count() == products.Count);
    }

    [TestMethod]
    public void HomeController_Index_Succeeds_WhenNotAuthorized()
    {
        // Arrange
        var controller = typeof(HomeController);
        var action = controller.GetMethod("Index");
        // Act
        var hasAuthorization = AttributeHandler.HasAuthorizationAttributes(controller, action);
        // Assert
        Assert.IsTrue(!hasAuthorization, "Action should not be authorized but it is");
    }



}
