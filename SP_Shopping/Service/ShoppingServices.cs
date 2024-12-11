﻿using Microsoft.AspNetCore.Identity;
using SP_Shopping.Data;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
namespace SP_Shopping.Service;

public class ShoppingServices : IServices
{
    public ProductService Product { get; }
    public CategoryService Category { get; }
    public CartItemService CartItem { get; }
    public UserService User { get; }

    private readonly ApplicationDbContext _context;

    public ShoppingServices
    (
        ApplicationDbContext context,
        IImageHandlerDefaulting<ProductImageKey> productImageHandler,
        IImageHandlerDefaulting<UserProfileImageKey> profileImageHandler,
        IMemoryCacher<string> memoryCacher,
        ILogger<RepositoryBaseCaching<Category>> memoryCacherLogger,
        UserManager<ApplicationUser> userManager
    )
    {

        _context = context;

        IRepository<Product> productRepository = new RepositoryBase<Product>(_context);
        Product = new ProductService(productRepository, productImageHandler);

        IRepositoryCaching<Category> categoryRepository = new RepositoryBaseCaching<Category>(_context, memoryCacher, memoryCacherLogger);
        Category = new CategoryService(categoryRepository, productRepository, Product);

        IRepository<CartItem> cartItemRepository = new RepositoryBase<CartItem>(_context);
        CartItem = new CartItemService(cartItemRepository);

        IRepository<ApplicationUser> userRepository = new RepositoryBase<ApplicationUser>(_context);
        User = new UserService(userRepository, productRepository, userManager, profileImageHandler, Product);
    }

    public bool DoInTransaction(Func<bool> action)
    {
        using var transact = _context.Database.BeginTransaction();
        try
        {
            var succeeded = action();
            if (succeeded)
            {
                transact.Commit();
                return true;
            }
            else
            {
                transact.Rollback();
                return false;
            }
        }
        catch
        {
            transact.Rollback();
            throw;
        }
    }

    public async Task<bool> DoInTransactionAsync(Func<Task<bool>> action)
    {
        using var transact = await _context.Database.BeginTransactionAsync();
        try
        {
            var succeeded = await action();
            if (succeeded)
            {
                await transact.CommitAsync();
                return true;
            }
            else
            {
                await transact.RollbackAsync();
                return false;
            }
        }
        catch
        {
            await transact.RollbackAsync();
            throw;
        }
    }

}
