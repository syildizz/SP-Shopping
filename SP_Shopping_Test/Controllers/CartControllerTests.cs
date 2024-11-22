﻿using AutoMapper;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SP_Shopping.Controllers;
using SP_Shopping.Dtos.Cart;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Service;
using SP_Shopping.Utilities.MessageHandler;
using System.Security.Claims;

namespace SP_Shopping.Test.Controllers;

[TestClass]
public class CartControllerTests
{

    private readonly ILogger<CartController> _logger;
    private readonly IMapper _mapper;
    private readonly IRepository<CartItem> _cartItemRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IMessageHandler _messageHandler;
    private readonly CartItemService _cartItemService;
    private readonly CartController _cartController;

    public CartControllerTests()
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

    #region Index

    [TestMethod]
    public async Task CartController_Index_Succeeds_WithViewResult()
    {
        // Arrange 
            // Cart for User exists
        var cartItems = (List<CartItemDetailsDto>)A.CollectionOfFake<CartItemDetailsDto>(5);
        var prices = Enumerable.Range(1, 5);
        cartItems = cartItems.Zip(prices, (c, p) => { c.Price = p; c.Count = 1; return c; }).ToList();
        A.CallTo(() => _cartItemRepository.GetAllAsync(A<Func<IQueryable<CartItem>, IQueryable<CartItemDetailsDto>>>._))
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
    public async Task CartController_CreatePost_Succeeds_WithRedirectResult()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product found
        A.CallTo(() => _productRepository.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(true);
        // Create succeeds
        A.CallTo(() => _cartItemRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(true);
        // Act
        IActionResult result = await _cartController.Create(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index");
    }

    [TestMethod]
    public async Task CartController_CreatePost_Fails_WhenIdIsNull_WithBadRequest()
    {
        // Arrange
            // Id IS null
            // Id exists, Product found
        // Act
        IActionResult result = await _cartController.Create(null);
        // Assert
            // Result is correct
        Assert.IsTrue(result is BadRequestResult or BadRequestObjectResult);
    }

    [TestMethod]
    public async Task CartController_CreatePost_Fails_WhenIdNotExist_WithNotFoundResult()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id does NOT exist, Product is NOT found
        A.CallTo(() => _productRepository.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _cartController.Create(id);
        // Assert
        // Result is correct
        Assert.IsTrue(result is NotFoundResult or NotFoundObjectResult);
    }

    [TestMethod]
    public async Task CartController_CreatePost_Fails_WhenCreateNotSucceed_WithRedirectResult()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Id exists, Product found
        A.CallTo(() => _productRepository.ExistsAsync(A<Func<IQueryable<Product>, IQueryable<Product>>>._))
            .Returns(true);
        // Create succeeds
        A.CallTo(() => _cartItemRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _cartController.Create(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index");
            //TODO:
            /*
                For now, we have no way of mocking the cartItemService
                When we do mock the cartItemService directly,
                add a check for error Messages.
            */
    }

    #endregion Create

    #region Edit

    [TestMethod]
    public async Task CartController_EditPost_Succeeds_WithRedirectResult()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Modelstate valid
            // Update succeeds
        A.CallTo(() => _cartItemRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(true);
        // Act
        IActionResult result = await _cartController.Edit(id, A.Fake<CartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index");
    }

    [TestMethod]
    public async Task CartController_EditPost_Fails_WhenIdIsNull_WithBadRequest()
    {
        // Arrange
            // Id IS null
        // Act
        IActionResult result = await _cartController.Edit(null, A.Fake<CartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsTrue(result is BadRequestResult or BadRequestObjectResult, "Action result is not BadRequest");
    }

    [TestMethod]
    public async Task CartController_EditPost_Fails_WhenModelStateNotValid_WithRedirectResult()
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
        Assert.IsTrue(_messageHandler.Peek(_cartController.TempData)?.Any(m => m.Type is Message.MessageType.Warning));
    }

    [TestMethod]
    public async Task CartController_EditPost_Fails_WhenEditNotSucceed_WithRedirectResult()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Modelstate valid
            // Update does NOT succeed
        A.CallTo(() => _cartItemRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _cartController.Edit(id, A.Fake<CartItemCreateDto>());
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result);
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index");
            //TODO:
            /*
                For now, we have no way of mocking the cartItemService
                When we do mock the cartItemService directly,
                add a check for error Messages.
            */
    }

    #endregion Edit

    #region Delete

    [TestMethod]
    public async Task CartController_DeletePost_Succeeds_WithRedirectResult()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Delete succeeds
        A.CallTo(() => _cartItemRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(true);
        // Act
        IActionResult result = await _cartController.Delete(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result, "Action result is not redirectToAction");
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", "Does not redirect to Index");
            // Must have called the database
        A.CallTo(() => _cartItemRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustHaveHappenedOnceExactly();
    }

    [TestMethod]
    public async Task CartController_DeletePost_Fails_WhenIdIsNull_WithBadRequest()
    {
        // Arrange
            // Id IS null
        // Act
        IActionResult result = await _cartController.Delete(null);
        // Assert
        Assert.IsTrue(result is BadRequestResult or BadRequestObjectResult, "Action result is not BadRequest");
            // Mustn't have called the database
        A.CallTo(() => _cartItemRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustNotHaveHappened();
    }

    [TestMethod]
    public async Task CartController_DeletePost_Fails_WhenDeleteFails_WithRedirectResult()
    {
        // Arrange
            // Id is not null
        const int id = 0;
            // Delete does NOT succeed
        A.CallTo(() => _cartItemRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .Returns(false);
        // Act
        IActionResult result = await _cartController.Delete(id);
        // Assert
            // Result is correct
        Assert.IsInstanceOfType<RedirectToActionResult>(result, "Action result is not redirectToAction");
        var redirectResult = (RedirectToActionResult)result;
        Assert.IsTrue(redirectResult.ActionName == "Index", "Does not redirect to Index");
            // Must have called the database
        A.CallTo(() => _cartItemRepository.DoInTransactionAsync(A<Func<Task<bool>>>._))
            .MustHaveHappenedOnceExactly();
            //TODO:
            /*
                For now, we have no way of mocking the cartItemService
                When we do mock the cartItemService directly,
                add a check for error Messages.
            */
    }

    #endregion Delete

}
