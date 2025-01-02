using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SP_Shopping.Controllers.API;
using SP_Shopping.Dtos.Product;
using SP_Shopping.Models;
using SP_Shopping.Service;
using SP_Shopping.Test.TestingUtilities;
using SP_Shopping.Utilities.Filters;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.MessageHandler;
using System.Reflection;
using System.Security.Claims;

namespace SP_Shopping.Test.Controllers.API;

[TestClass]
public class ProductsControllerAPITests
{

    private readonly IMapper _mapper;
    private readonly ILogger<ProductsController> _logger;
    private readonly IShoppingServices _shoppingServices;
    private readonly IImageHandlerDefaulting<ProductImageKey> _productImageHandler;
    private readonly IMessageHandler _messageHandler;
    private readonly ProductsController _productsController;

    public ProductsControllerAPITests()
    {
        _mapper = A.Fake<IMapper>();
        _logger = new NullLogger<ProductsController>();
        _shoppingServices = A.Fake<IShoppingServices>();
        _productImageHandler = A.Fake<IImageHandlerDefaulting<ProductImageKey>>();
        _messageHandler = new MessageHandler();

        // SUT

        _productsController = new ProductsController
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

    [DataRow("ProductCard", (Type[])[typeof(int?)], false)]
    [DataTestMethod]
    public void ProductsController_All_Succeeds_WhenAuthorizedAsSucceeds(string methodName, Type[] types, bool succeeds)
    {
        // Arrange
        var controller = typeof(ProductsController);
        var action = controller.GetMethod(methodName, types);
        // Act
        var hasAuthorization = AttributeHandler.HasAuthorizationAttributes(controller, action);
        // Assert
        Assert.IsTrue(succeeds == hasAuthorization, $"Action {methodName} should {(succeeds ? "" : "not " )}be authorized but it is{(!succeeds ? "" : ("n't"))}");
    }

    [DataRow("ProductCard", (Type[])[typeof(int?)])]
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

    #region ProductCard

    [TestMethod]
    public async Task ProductsController_ProductCard_Succeeds_WithPartialView()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .Returns(A.Fake<ProductDetailsDto>());
        // Act
        IActionResult result = await _productsController.ProductCard(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<PartialViewResult>(result);
        var partialViewResult = (PartialViewResult)result;
        Assert.IsInstanceOfType<ProductDetailsDto>(partialViewResult.Model);
    }

    [TestMethod]
    public async Task ProductsController_ProductCard_Fails_WhenProductIsNotExist_WithNotFound()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product is found
        A.CallTo(() => _shoppingServices.Product.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<ProductDetailsDto>>>._))
            .Returns((ProductDetailsDto?)null);
        // Act
        IActionResult result = await _productsController.ProductCard(id);
        // Assert
            // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult, $"Result should be {nameof(NotFoundResult)} or {nameof(NotFoundObjectResult)} but is {result.GetType().Name}");
    }

    #endregion ProductCard
}
