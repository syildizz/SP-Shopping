using Microsoft.EntityFrameworkCore;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities.MessageHandler;
using System.Data;

namespace SP_Shopping.Service;

public class CategoryService
(
    IRepositoryCaching<Category> categoryRepository,
    IRepository<Product> productRepository,
    ProductService productService
)
{

    private readonly IRepositoryCaching<Category> _categoryRepository = categoryRepository;
    private readonly IRepository<Product> _productRepository = productRepository;
    private readonly ProductService _productService = productService;

    public (bool succeeded, ICollection<Message>? errorMessages) TryCreate(Category category)
    {

        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = _categoryRepository.DoInTransaction(() =>
        {

            _categoryRepository.Create(category);

            try
            {
                _categoryRepository.SaveChanges();
            }
            catch (Exception ex)
            {
                if (ex is DBConcurrencyException)
                {
                    #if DEBUG
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = $"Error saving to database: {ex.Message}" });
                    #else
                    errorMessages.Append(new Message { Type = Message.MessageType.Error, Content = "Error saving to database" });
                    #endif
                    return false;
                }
                else if (ex is DbUpdateException)
                { 
                    // Exception occurs when adding same product to same users cart.
                    // This is a desired effect, therefore the below codoe is commented out.
                    // TODO: Analyze update exception for the above mentioned exception and throw 
                    //     otherwise
                    //_logger.LogError("Failed to create CartItem in the database for user of id \"{UserId}\" and for product of \"{ProductId}\".", cartItem.UserId, cartItem.ProductId);
                    //_messageHandler.AddMessages(TempData, [new Message { Type = Message.MessageType.Error, Content = "Error when adding product to cart" }]);
                }
                else
                {
                    throw;
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

    public async Task<(bool succeeded, ICollection<Message>? errorMessages)> TryCreateAsync(Category category)
    {

        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = await _categoryRepository.DoInTransactionAsync(async () =>
        {

            _categoryRepository.Create(category);

            try
            {
                await _categoryRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateException or DBConcurrencyException)
                {
                    #if DEBUG
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = $"Error saving to database: {ex.Message}" });
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

    public (bool succeeded, ICollection<Message>? errorMesages) TryUpdate(Category category)
    {
        ICollection<Message>? errorMessages = [];

        bool transactionSucceeded = _categoryRepository.DoInTransaction(() =>
        {

            try
            {
                _categoryRepository.Update(category);
                _categoryRepository.SaveChanges();
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateException or DBConcurrencyException)
                {
                    #if DEBUG
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = $"Error saving to database: {ex.Message}" });
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

    public async Task<(bool succeeded, ICollection<Message>? errorMesages)> TryUpdateAsync(Category category)
    {
        ICollection<Message>? errorMessages = [];

        bool transactionSucceeded = await _categoryRepository.DoInTransactionAsync(async () =>
        {

            try
            {
                _categoryRepository.Update(category);
                await _categoryRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateException or DBConcurrencyException)
                {
                    #if DEBUG
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = $"Error saving to database: {ex.Message}" });
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

    public (bool succeeded, ICollection<Message>? errorMessages) TryDelete(Category category)
    {

        ICollection<Message> errorMessages = [];

        var productIds = _productRepository.GetAll(q => q.Where(p => p.Category == category).Select(p => p.Id));

        bool transactionSucceeded = _categoryRepository.DoInTransaction(() =>
        {

            try
            {
                _categoryRepository.Delete(category);
                _categoryRepository.SaveChanges();
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateException or DBConcurrencyException)
                {
                    #if DEBUG
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = $"Error saving to database: {ex.Message}" });
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

    public async Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(Category category)
    {

        ICollection<Message> errorMessages = [];

        var productIds = await _productRepository.GetAllAsync(q => q.Where(p => p.Category == category).Select(p => p.Id));

        bool transactionSucceeded = await _categoryRepository.DoInTransactionAsync(async () =>
        {

            try
            {
                _categoryRepository.Delete(category);
                await _categoryRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateException or DBConcurrencyException)
                {
                    #if DEBUG
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = $"Error saving to database: {ex.Message}" });
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
