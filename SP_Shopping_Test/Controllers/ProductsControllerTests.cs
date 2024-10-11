using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SP_Shopping.Controllers;
using SP_Shopping.Dtos;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP_Shopping.Controllers.Tests;

[TestClass]
public class ProductsControllerTests
{

    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductsController> _logger;
    private readonly ProductsController _productsController;

    public ProductsControllerTests()
    {
        // Dependencies
        var services = new ServiceCollection();

        services.AddAutoMapper(typeof(Program));
        services.AddMemoryCache();

        var serviceProvider = services.BuildServiceProvider();

        _productRepository = A.Fake<IRepository<Product>>();
        _categoryRepository = A.Fake<IRepository<Category>>();
        _userRepository = A.Fake<IRepository<ApplicationUser>>();
        _memoryCache = A.Fake<IMemoryCache>();
        _mapper = serviceProvider.GetRequiredService<IMapper>();
        _logger = A.Fake<ILogger<ProductsController>>();

        // SUT
        _productsController = new ProductsController
        (
            logger: _logger,
            mapper: _mapper,
            productRepository: _productRepository,
            categoryRepository:  _categoryRepository,
            userRepository: _userRepository,
            memoryCache: _memoryCache
        );
    }

    [TestMethod]
    public async Task ProductsController_Index_Succeeds_WithViewResult()
    {
        // Arrange
        var products = A.CollectionOfFake<Product>(5) as List<Product>;
        Assert.IsNotNull(products);
        A.CallTo(() => _productRepository.GetAllAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._)).Returns(Task.FromResult(products));
        // Act
        IActionResult result = await _productsController.Index();
        // Assert
        A.CallTo(() => _productRepository.GetAllAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._)).MustHaveHappenedOnceExactly();
        Assert.IsInstanceOfType<ViewResult>(result);
        Assert.IsInstanceOfType<IEnumerable<ProductDetailsDto>>(((ViewResult)result).Model);
        Assert.IsTrue(products.Count == ((IEnumerable<ProductDetailsDto>)((ViewResult)result).Model!).Count());
    }

    [TestMethod]
    public async Task ProductsController_Details_Succeeds_WithViewResult()
    {
        // Arrange 
        Product product = A.Fake<Product>();
        A.CallTo(() => _productRepository!.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))!.Returns(Task.FromResult(product));
        // Act
        IActionResult result = await _productsController.Details(0);
        // Assert
        A.CallTo(() => _productRepository!.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))!.MustHaveHappenedOnceExactly();
        Assert.IsInstanceOfType<ViewResult>(result);
        Assert.IsInstanceOfType<ProductDetailsDto>(((ViewResult)result).Model);
    }

    [TestMethod]
    public async Task ProductController_Details_Fails_WhenIdIsNull_WithBadRequestResponse()
    {
        // Arrange
        // Act
        IActionResult result = await _productsController.Details(null);
        // Assert
        Assert.IsTrue(new[] {typeof(BadRequestResult), typeof(BadRequestObjectResult)}.Contains(result.GetType()));
    }

    [TestMethod]
    public async Task ProductsController_Details_Fails_WhenProductDoesntExist_WithNotFoundResponse()
    {
        // Arrange 
        Product? product = null;
        A.CallTo(() => _productRepository!.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))!.Returns(Task.FromResult(product));
        // Act
        IActionResult result = await _productsController.Details(0);
        // Assert
        A.CallTo(() => _productRepository!.GetSingleAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))!.MustHaveHappenedOnceExactly();
        Assert.IsTrue(new[] {typeof(NotFoundResult), typeof(NotFoundObjectResult)}.Contains(result.GetType()));
    }

    [TestMethod]
    public async Task ProductsController_CreateGet_ReturnsSuccess()
    {
        // Arrange
        List<Category>? categories = A.CollectionOfFake<Category>(4) as List<Category>;
        A.CallTo(() => _memoryCache.CreateEntry(A<string>._)).Returns(A.Fake<ICacheEntry>());
        A.CallTo(() => _categoryRepository.GetAllAsync()).Returns(Task.FromResult(categories)!);
        // Act
        IActionResult result = await _productsController.Create();
        // Assert
        Assert.IsInstanceOfType<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.IsInstanceOfType<IEnumerable<SelectListItem>>(viewResult.ViewData["categorySelectList"]);
        IEnumerable<SelectListItem> selectList = (IEnumerable<SelectListItem>)viewResult.ViewData["categorySelectList"]!;
        Assert.IsTrue(selectList.Count() == categories!.Count);
    }

    [TestMethod]
    public async Task ProductsController_CreatePost_ReturnsSuccess()
    {
        // Arrange
        var sentWithPost = A.Fake<ProductCreateDto>();
        A.CallTo(() => _productRepository.CreateAsync(A<Product>._)).Returns(Task.FromResult(true));
        // Act
        IActionResult result = await _productsController.Create(sentWithPost);
        // Assert
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
    }

    [TestMethod]
    public async Task ProductController_CreatePost_Fails_WhenModelStateIsNotValid_ReturnsViewResult()
    {
        // Arrange
        var sentWithPost = A.Fake<ProductCreateDto>();
        _productsController.ModelState.AddModelError("CategoryId", "The CategoryId for this product is invalid");
        // Act
        IActionResult result = await _productsController.Create(sentWithPost);
        // Assert
        Assert.IsInstanceOfType<ViewResult>(result);
        A.CallTo(() => _productRepository.CreateAsync(A<Product>._)).MustNotHaveHappened();
    }

    [TestMethod]
    public async Task ProductController_CreatePost_Fails_WhenProductCannotBeCreated_ReturnsBadRequest()
    {
        // Arrange
        var sentWithPost = A.Fake<ProductCreateDto>();
        A.CallTo(() => _productRepository.CreateAsync(A<Product>._)).Throws<DbUpdateException>();
        // Act
        IActionResult result = await _productsController.Create(sentWithPost);
        // Assert
        Assert.IsTrue(new[] {typeof(BadRequestResult), typeof(BadRequestObjectResult)}.Contains(result.GetType()));
        A.CallTo(() => _productRepository.CreateAsync(A<Product>._)).MustHaveHappenedOnceOrMore();
    }

    [TestMethod]
    public async Task ProductController_CreatePost_Fails_WhenSubmitterIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var sentWithPost = A.Fake<ProductCreateDto>();
        ApplicationUser? nullUser = null;
        A.CallTo(() => _userRepository.GetSingleAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._)).Returns(Task.FromResult(nullUser));
        // Act
        IActionResult result = await _productsController.Create(sentWithPost);
        // Assert
        Assert.IsTrue(new[] {typeof(BadRequestResult), typeof(BadRequestObjectResult)}.Contains(result.GetType()));
        A.CallTo(() => _userRepository.GetSingleAsync(A<Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>>>._)).MustHaveHappenedOnceOrMore();
    }

}