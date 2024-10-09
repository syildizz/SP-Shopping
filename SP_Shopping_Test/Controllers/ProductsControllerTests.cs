using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Framework;
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
        _memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
        _mapper = serviceProvider.GetRequiredService<IMapper>();
        _logger = A.Fake<ILogger<ProductsController>>();

        // SUT
        _productsController = new ProductsController(_logger, _mapper, _productRepository, _categoryRepository, _memoryCache);
    }

    [TestMethod]
    public async Task ProductsController_Index_ReturnsSuccess()
    {
        // Arrange
        var products = A.CollectionOfFake<Product>(5) as List<Product>;
        Assert.IsNotNull(products);
        A.CallTo(() => _productRepository.GetAllAsync()).Returns(Task.FromResult(products));
        // Act
        var result = await _productsController.Index() as ViewResult;
        // Assert
        A.CallTo(() => _productRepository.GetAllAsync()).MustHaveHappenedOnceExactly();
        Assert.IsInstanceOfType<ViewResult>(result);
        Assert.IsInstanceOfType<IEnumerable<ProductDetailsDto>>(result.Model);
        Assert.IsTrue(products.Count == ((IEnumerable<ProductDetailsDto>)result.Model).Count());
    }

}