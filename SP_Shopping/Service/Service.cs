using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.Win32;
using SP_Shopping.Data;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SQLitePCL;

namespace SP_Shopping.Service;

public class Service
{
    private readonly ApplicationDbContext _context;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IImageHandlerDefaulting<UserProfileImageKey> _profileImageHandler;
    private readonly IImageHandlerDefaulting<ProductImageKey> _productImageHandler;
    public Service
    (
        ApplicationDbContext context, 
        IImageHandlerDefaulting<UserProfileImageKey> profileImageHandler,
        IImageHandlerDefaulting<ProductImageKey> productImageHandler
    )
    {
        _context = context;
        _productRepository = new RepositoryBase<Product>(_context);
        _categoryRepository = new RepositoryBase<Category>(_context);
        _userRepository = new RepositoryBase<ApplicationUser>(_context);
        _profileImageHandler = profileImageHandler;
        _productImageHandler = productImageHandler;
    }


    public void ProductDeleteByKey(int key)
    {
        if (_productRepository.DeleteCertainEntries(q => q.Where(p => p.Id == key)) > 0)
        {
            _productImageHandler.DeleteImage(new(key));
        }
    }


    public void UserDeleteByKey(string key)
    {
        if (_userRepository.DeleteCertainEntries(q => q.Where(p => p.Id == key)) > 0)
        {
            _profileImageHandler.DeleteImage(new(key));
        }
    }

    

}

