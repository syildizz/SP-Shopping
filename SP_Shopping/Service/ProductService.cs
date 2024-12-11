﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.MessageHandler;
using System.Data;
using System.Linq.Expressions;

namespace SP_Shopping.Service;

public class ProductService
(
    IRepository<Product> productRepository,
    IImageHandlerDefaulting<ProductImageKey> productImageHandler
)
{

    private readonly IRepository<Product> _productRepository = productRepository;
    private readonly IImageHandlerDefaulting<ProductImageKey> _productImageHandler = productImageHandler;
    public virtual List<Product> GetAll()
    {
        return _productRepository.GetAll();
    }

    public virtual List<TResult> GetAll<TResult>(Func<IQueryable<Product>, IQueryable<TResult>> query)
    {
        return _productRepository.GetAll(query);
    }

    public virtual async Task<List<Product>> GetAllAsync()
    {
        return await _productRepository.GetAllAsync();
    }

    public virtual async Task<List<TResult>> GetAllAsync<TResult>(Func<IQueryable<Product>, IQueryable<TResult>> query)
    {
        return await _productRepository.GetAllAsync(query);
    }

    public virtual Product? GetByKey(params object?[]? keyValues)
    {
        return _productRepository.GetByKey(keyValues);
    }

    public virtual async Task<Product?> GetByKeyAsync(params object?[]? keyValues)
    {
        return await _productRepository.GetByKeyAsync(keyValues);
    }

    public virtual TResult? GetSingle<TResult>(Func<IQueryable<Product>, IQueryable<TResult>> query)
    {
        return _productRepository.GetSingle(query);
    }

    public virtual async Task<TResult?> GetSingleAsync<TResult>(Func<IQueryable<Product>, IQueryable<TResult>> query)
    {
        return await _productRepository.GetSingleAsync(query);
    }

    public virtual bool Exists(Func<IQueryable<Product>, IQueryable<Product>> query)
    {
        return _productRepository.Exists(query);
    }

    public virtual async Task<bool> ExistsAsync(Func<IQueryable<Product>, IQueryable<Product>> query)
    {
        return await _productRepository.ExistsAsync(query);
    }

    public (bool succeeded, ICollection<Message>? errorMessages) TryCreate(Product product, IFormFile? image)
    {

        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = _productRepository.DoInTransaction(() =>
        {
            // Set submitter to null 
            // to avoid generating new ApplicationUser with auto-generated id.
            product.Submitter = null;

            product.InsertionDate = DateTime.Now;
            _productRepository.Create(product);

            try
            {
                _productRepository.SaveChanges();
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateException or DBConcurrencyException)
                {
                    #if DEBUG
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = $"Error saving to database: {ex.StackTrace}" });
                    #else
                    errorMessages.Append(new Message { Type = Message.MessageType.Error, Content = "Error saving to database" });
                    #endif
                    return false;
                }
                else
                {
                    throw;
                }
            }

            if (image is not null)
            {
                using var ImageStream = image.OpenReadStream();
                if (!_productImageHandler.SetImage(new(product.Id), ImageStream))
                {
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to set image" });
                    return false;
                }
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

    public async Task<(bool succeeded, ICollection<Message>? errorMessages)> TryCreateAsync(Product product, IFormFile? image)
    {

        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = await _productRepository.DoInTransactionAsync(async () =>
        {
            // Set submitter to null 
            // to avoid generating new ApplicationUser with auto-generated id.
            product.Submitter = null;

            product.InsertionDate = DateTime.Now;
            await _productRepository.CreateAsync(product);

            try
            {
                await _productRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateException or DBConcurrencyException)
                {
                    #if DEBUG
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = $"Error saving to database: {ex.StackTrace}" });
                    #else
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Error saving to database" });
                    #endif
                    return false;
                }
                else
                {
                    throw;
                }
            }

            if (image is not null)
            {
                using var ImageStream = image.OpenReadStream();
                if (!await _productImageHandler.SetImageAsync(new(product.Id), ImageStream))
                {
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to set image" });
                    return false;
                }
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

    public (bool succeeded, ICollection<Message>? errorMesages) TryUpdate(Product product, IFormFile? image)
    {
        ICollection<Message>? errorMessages = [];

        bool transactionSucceeded = _productRepository.DoInTransaction(() =>
        {

            try
            {

                Expression<Func<SetPropertyCalls<Product>, SetPropertyCalls<Product>>> setProperties;

                // We only update the submitterId if the submitterId is null,
                // otherwise, don't update it.
                // If the check is not done, it is set to null and we have a product
                // that does not have an uploader.
                if (product.SubmitterId is null)
                {
                    setProperties = s => s
                        .SetProperty(p => p.Name, product.Name)
                        .SetProperty(p => p.Price, product.Price)
                        .SetProperty(p => p.CategoryId, product.CategoryId)
                        .SetProperty(p => p.Description, product.Description)
                        .SetProperty(p => p.ModificationDate, DateTime.Now)
                    ;
                }
                else
                {
                    setProperties = s => s
                        .SetProperty(p => p.Name, product.Name)
                        .SetProperty(p => p.Price, product.Price)
                        .SetProperty(p => p.CategoryId, product.CategoryId)
                        .SetProperty(p => p.Description, product.Description)
                        .SetProperty(p => p.ModificationDate, DateTime.Now)
                        .SetProperty(p => p.SubmitterId, product.SubmitterId)
                    ;
                }

                _productRepository.UpdateCertainFields(q => q
                    .Where(p => p.Id == product.Id),
                    setPropertyCalls: setProperties
                );
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateException or DBConcurrencyException)
                {
                    #if DEBUG
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = $"Error saving to database: {ex.StackTrace}" });
                    #else
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Error saving to database" });
                    #endif
                    return false;
                }
                else
                {
                    throw;
                }
            }

            if (image is not null)
            {
                using var ImageStream = image.OpenReadStream();
                if (!_productImageHandler.SetImage(new(product.Id), ImageStream))
                {
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to set image" });
                    return false;
                }
            }

            return true;
        });

        if (transactionSucceeded)
        {
            errorMessages = null;
            return (true, errorMessages);
        }
        else
        {
            return (false, errorMessages);
        }

    }

    public async Task<(bool succeeded, ICollection<Message>? errorMesages)> TryUpdateAsync(Product product, IFormFile? image)
    {
        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = await _productRepository.DoInTransactionAsync(async () =>
        {

            try
            {

                Expression<Func<SetPropertyCalls<Product>, SetPropertyCalls<Product>>> setProperties;

                // We only update the submitterId if the submitterId is null,
                // otherwise, don't update it.
                // If the check is not done, it is set to null and we have a product
                // that does not have an uploader.
                if (product.SubmitterId is null)
                {
                    setProperties = s => s
                        .SetProperty(p => p.Name, product.Name)
                        .SetProperty(p => p.Price, product.Price)
                        .SetProperty(p => p.CategoryId, product.CategoryId)
                        .SetProperty(p => p.Description, product.Description)
                        .SetProperty(p => p.ModificationDate, DateTime.Now)
                    ;
                }
                else
                {
                    setProperties = s => s
                        .SetProperty(p => p.Name, product.Name)
                        .SetProperty(p => p.Price, product.Price)
                        .SetProperty(p => p.CategoryId, product.CategoryId)
                        .SetProperty(p => p.Description, product.Description)
                        .SetProperty(p => p.ModificationDate, DateTime.Now)
                        .SetProperty(p => p.SubmitterId, product.SubmitterId)
                    ;
                }

                await _productRepository.UpdateCertainFieldsAsync(q => q
                    .Where(p => p.Id == product.Id),
                    setPropertyCalls: setProperties
                );
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateException or DBConcurrencyException)
                {
                    #if DEBUG
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = $"Error saving to database: {ex.StackTrace}" });
                    #else
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Error saving to database" });
                    #endif
                    return false;
                }
                else
                {
                    throw;
                }
            }

            if (image is not null)
            {
                using var ImageStream = image.OpenReadStream();
                if (!await _productImageHandler.SetImageAsync(new(product.Id), ImageStream))
                {
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to set image" });
                    return false;
                }
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

    public (bool succeeded, ICollection<Message>? errorMessages) TryDelete(Product product)
    {

        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = _productRepository.DoInTransaction(() =>
        {
            _productRepository.Delete(product);

            try
            {
                _productRepository.SaveChanges();
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateException or DBConcurrencyException)
                {
                    #if DEBUG
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = $"Error saving to database: {ex.StackTrace}" });
                    #else
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Error saving to database" });
                    #endif
                    return false;
                }
                else
                {
                    throw;
                }
            }

            try
            {
                _productImageHandler.DeleteImage(new(product.Id));
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
            return (true, null);
        }
        else
        {
            return (false, errorMessages);
        }

    }

    public async Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(Product product)
    {

        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = await _productRepository.DoInTransactionAsync(async () =>
        {
            _productRepository.Delete(product);

            try
            {
                await _productRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateException or DBConcurrencyException)
                {
                    #if DEBUG
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = $"Error saving to database: {ex.StackTrace}" });
                    #else
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Error saving to database" });
                    #endif
                    return false;
                }
                else
                {
                    throw;
                }
            }

            try
            {
                _productImageHandler.DeleteImage(new(product.Id));
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
            return (true, null);
        }
        else
        {
            return (false, errorMessages);
        }

    }

    public (bool succeeded, ICollection<Message>? errorMessages) TryDeleteCascade(Product product)
    {

        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = false;
        try
        {
            _productImageHandler.DeleteImage(new(product.Id));
            transactionSucceeded = true;
        }
        catch (Exception ex)
        {
            #if DEBUG
            errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = $"Failed to delete image: {ex.StackTrace}" });
            #else
            errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to delete image" });
            #endif
        }

        if (transactionSucceeded)
        {
            return (true, null);
        }
        else
        {
            return (false, errorMessages);
        }

    }


}
