﻿using Microsoft.AspNetCore.Identity;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.MessageHandler;
using System.Data;
using System.Linq.Expressions;

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

    public async Task<(bool succeeded, ICollection<Message>? errorMessages)> TryCreateAsync(ApplicationUser user, string password, IFormFile? image, IEnumerable<string>? roles)
    {
        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = await _userRepository.DoInTransactionAsync(async () =>
        {

            IdentityResult succeeded;

            succeeded = await _userManager.CreateAsync(user, password);
            if (!succeeded.Succeeded)
            {
                errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to create user" });
                return false;
            }

            if (roles is not null)
            {
                var errorMessage = "Failed to set user's roles";
                bool wentToCatch = false;
                try
                {
                    succeeded = await _userManager.AddToRolesAsync(user, roles);
                }
                catch (InvalidOperationException ex) 
                { 
                    wentToCatch = true;
                    #if DEBUG
                    errorMessage = $"{errorMessage}: {ex.StackTrace}";
                    #endif
                }
                if (wentToCatch || !succeeded.Succeeded)
                {
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = errorMessage });
                    return false;
                }

            }

            int result2 = await _userRepository.UpdateCertainFieldsAsync(q => q
                .Where(u => u.Id == user.Id),
                s => s
                    .SetProperty(u => u.Description, user.Description)
                    .SetProperty(u => u.InsertionDate, DateTime.Now)
                    // Confirm e-mail and phone number when changed by admin.
                    // Maybe change this functionality in the future idk.
                    // TODO: Add panels in admin panel to confirm / unconfirm user email / phone number
                    .SetProperty(u => u.EmailConfirmed, true)
                    .SetProperty(u => u.PhoneNumberConfirmed, true)
                    .SetProperty(u => u.ConcurrencyStamp, Guid.NewGuid().ToString())
            );

            if (result2 < 1)
            {
                errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to set description" });
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

    public async Task<(bool succeeded, ICollection<Message>? errorMessages)> TryUpdateAsync(ApplicationUser user, IFormFile? image, IEnumerable<string>? roles)
    {
        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = await _userRepository.DoInTransactionAsync(async () =>
        {
            IdentityResult result;

            ApplicationUser? _user = _userRepository.GetByKey(user.Id);
            if (_user is null)
            {
                errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "User doesn't exist but should. Code should be unreachable. Contact developer" });
                return false;
            }

            result = await _userManager.SetUserNameAsync(_user, user.UserName);
            if (!result.Succeeded)
            {
                errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to set username" });
                return false;
            }

            result = await _userManager.SetEmailAsync(_user, user.Email);
            if (!result.Succeeded)
            {
                errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to set email" });
                return false;
            }


            result = await _userManager.SetPhoneNumberAsync(_user, user.PhoneNumber);
            if (!result.Succeeded)
            {
                errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to set phone number" });
                return false;
            }

            if (roles is not null)
            {
                // userRoles: The role the user already has
                // requestRoles: The roles that the request specifies
                // user/RequestRoles: The roles that the user has that isn't in the request (roles that should be deleted from user)
                // request/userRoles: The roles that the request has that the user doesn't have  (roles that should be added to user)
                var userRoles = (await _userManager.GetRolesAsync(_user)).Select(s => s.ToUpperInvariant());
                var requestRoles = roles.Where(r => !string.IsNullOrWhiteSpace(r)).Select(s => s.ToUpperInvariant());
                var userDifferenceRequestRoles = userRoles.Except(requestRoles);
                var requestDifferenceUserRoles = requestRoles.Except(userRoles);

                var wentToCatch = false;
                string errorMessage = "Failed to set roles";
                try
                {
                    result = await _userManager.AddToRolesAsync(_user, requestDifferenceUserRoles);
                }
                catch (InvalidOperationException ex) 
                { 
                    wentToCatch = true;
                    #if DEBUG
                        errorMessage = $"{errorMessage}: {ex.StackTrace}";
                    #endif
                }
                if (wentToCatch || !result.Succeeded)
                {
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = errorMessage });
                    return false;
                }

                wentToCatch = false;
                try
                {
                    result = await _userManager.RemoveFromRolesAsync(_user, userDifferenceRequestRoles);
                }
                catch (InvalidOperationException ex) 
                { 
                    wentToCatch = true;
                    #if DEBUG
                        errorMessage = $"{errorMessage}: {ex.StackTrace}";
                    #endif
                }
                if (wentToCatch || !result.Succeeded)
                {
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = errorMessage });
                    return false;
                }
            }
            // User roles is empty therefore delete all roles from the user
            else
            {
                var wentToCatch = false;
                string errorMessage = "Failed to set roles";
                try
                {
                    result = await _userManager.RemoveFromRolesAsync(_user, await _userManager.GetRolesAsync(_user));
                }
                catch (InvalidOperationException ex) 
                { 
                    wentToCatch = true;
                    #if DEBUG
                        errorMessage = $"{errorMessage} : {ex.StackTrace}";
                    #endif
                }
                if (wentToCatch || !result.Succeeded)
                {
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = errorMessage });
                    return false;
                }
            }

            int result2 = await _userRepository.UpdateCertainFieldsAsync(q => q
                .Where(u => u.Id == _user.Id),
                s => s
                    .SetProperty(u => u.Description, user.Description)
                    // Confirm e-mail and phone number when changed by admin.
                    // Maybe change this functionality in the future idk.
                    // TODO: Add panels in admin panel to confirm / unconfirm user email / phone number
                    .SetProperty(u => u.EmailConfirmed, true)
                    .SetProperty(u => u.PhoneNumberConfirmed, true)
            );

            if (result2 < 1)
            {
                errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to set description" });
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

