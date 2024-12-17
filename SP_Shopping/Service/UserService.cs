using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.MessageHandler;
using System.Data;

namespace SP_Shopping.Service;

public class UserService
(
    IRepository<ApplicationUser> userRepository,
    IRepository<Product> productRepository,
    UserManager<ApplicationUser> userManager,
    IImageHandlerDefaulting<UserProfileImageKey> profileImageHandler,
    IProductService productService
) : IUserService
{
    private readonly IRepository<ApplicationUser> _userRepository = userRepository;
    private readonly IRepository<Product> _productRepository = productRepository;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IImageHandlerDefaulting<UserProfileImageKey> _profileImageHandler = profileImageHandler;
    private readonly IProductService _productService = productService;

    public virtual List<ApplicationUser> GetAll()
    {
        return _userRepository.GetAll();
    }

    public virtual List<TResult> GetAll<TResult>(Func<IQueryable<ApplicationUser>, IQueryable<TResult>> query)
    {
        return _userRepository.GetAll(query);
    }

    public virtual async Task<List<ApplicationUser>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public virtual async Task<List<TResult>> GetAllAsync<TResult>(Func<IQueryable<ApplicationUser>, IQueryable<TResult>> query)
    {
        return await _userRepository.GetAllAsync(query);
    }

    public virtual ApplicationUser? GetByKey(params object?[]? keyValues)
    {
        return _userRepository.GetByKey(keyValues);
    }

    public virtual async Task<ApplicationUser?> GetByKeyAsync(params object?[]? keyValues)
    {
        return await _userRepository.GetByKeyAsync(keyValues);
    }

    public virtual TResult? GetSingle<TResult>(Func<IQueryable<ApplicationUser>, IQueryable<TResult>> query)
    {
        return _userRepository.GetSingle(query);
    }

    public virtual async Task<TResult?> GetSingleAsync<TResult>(Func<IQueryable<ApplicationUser>, IQueryable<TResult>> query)
    {
        return await _userRepository.GetSingleAsync(query);
    }

    public virtual bool Exists(Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>> query)
    {
        return _userRepository.Exists(query);
    }

    public virtual async Task<bool> ExistsAsync(Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>> query)
    {
        return await _userRepository.ExistsAsync(query);
    }

    public (bool succeeded, ICollection<Message>? errorMessages) TryCreate(ApplicationUser user, string password, IFormFile? image)
    {
        return TryCreateAsync(user, password, image).Result;
    }

    public async Task<(bool succeeded, ICollection<Message>? errorMessages)> TryCreateAsync(ApplicationUser user, string password, IFormFile? image)
    {
        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = await _userRepository.DoInTransactionAsync(async () =>
        {

            IdentityResult succeeded;

            user.EmailConfirmed = true;
            user.PhoneNumberConfirmed = true;
            succeeded = await _userManager.CreateAsync(user, password);
            if (!succeeded.Succeeded)
            {
                errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to create user" });
                return false;
            }

            if (image is not null)
            {
                using var imageStream = image.OpenReadStream();
                if (!await _profileImageHandler.SetImageAsync(new(user.Id), imageStream))
                {
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to set profile picture" });
                    return false;
                }

                return true;
            }

            return true;

        });

        if (transactionSucceeded)
        {
            return (true, null);
        }
        else
        {
            return (false, errorMessages);
        }

    }

    public (bool succeeded, ICollection<Message>? errorMessages) TryUpdate(ApplicationUser user, IFormFile? image)
    {
        return TryUpdateAsync(user, image).Result;
    }

    public async Task<(bool succeeded, ICollection<Message>? errorMessages)> TryUpdateAsync(ApplicationUser user, IFormFile? image)
    {
        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = await _userRepository.DoInTransactionAsync(async () =>
        {

            ApplicationUser? _user = await _userRepository.GetSingleAsync(q => q.Where(u => u.Id == user.Id).Include(u => u.Roles));
            if (_user is null)
            {
                errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "User has invalid id" });
                return false;
            }

            _user.UserName = user.UserName;
            _user.Email = user.Email;
            _user.PhoneNumber = user.PhoneNumber;
            _user.Description = user.Description;
            _user.Roles = user.Roles;

            IdentityResult succeeded;
            string errorMessage = "";
            try
            {
                errorMessage = "Unable to update user";
                succeeded = await _userManager.UpdateAsync(_user);
            }
            catch (InvalidOperationException ex) 
            { 
                #if DEBUG
                errorMessage = $"{errorMessage}: {ex.StackTrace}";
                #endif
                succeeded = IdentityResult.Failed();
            }
            if (!succeeded.Succeeded)
            {
                #if DEBUG
                errorMessages = errorMessages.Concat(succeeded.Errors.Select(e => new Message { Type = Message.MessageType.Error, Content = $"{errorMessage}: {e.Description}" })).ToList();
                #else
                errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = errorMessage });
                #endif
                return false;
            }

            if (image is not null)
            {
                using var imageStream = image.OpenReadStream();
                if (!await _profileImageHandler.SetImageAsync(new(user.Id), imageStream))
                {
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to set profile picture" });
                    return false;
                }

                return true;
            }

            return true;

        });

        if (transactionSucceeded)
        {
            return (true, null);
        }
        else
        {
            return (false, errorMessages);
        }

    }

    public (bool succeeded, ICollection<Message>? errorMessages) TryDelete(ApplicationUser user)
    {
        return TryDeleteAsync(user).Result;
    }

    public async Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(ApplicationUser user)
    {

        ICollection<Message> errorMessages = [];

        var productIds = await _productRepository.GetAllAsync(q => q.Where(p => p.SubmitterId == user.Id).Select(p => p.Id));

        bool transactionSucceeded = await _userRepository.DoInTransactionAsync(async () =>
        {

            IdentityResult result;

            result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to delete user" });
                return false;
            }

            try
            {
                _profileImageHandler.DeleteImage(new(user.Id));
            }
            catch (Exception ex)
            {
#if DEBUG
                errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = $"Failed to delete image: {ex.StackTrace}" });
#else
                errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to delete image" });
#endif
                return false;
            }

            return true;

        });

        if (transactionSucceeded)
        {
            foreach (var productId in productIds)
            {
                _productService.TryDeleteCascade(new Product { Id = productId });
            }
            return (true, null);
        }
        else
        {
            return (false, errorMessages);
        }

    }


}

