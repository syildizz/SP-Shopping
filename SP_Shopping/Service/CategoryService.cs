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
    IProductService productService
) : ICategoryService
{

    private readonly IRepositoryCaching<Category> _categoryRepository = categoryRepository;
    private readonly IRepository<Product> _productRepository = productRepository;
    private readonly IProductService _productService = productService;

    public virtual List<Category> GetAll()
    {
        return _categoryRepository.GetAll();
    }

    public virtual List<TResult> GetAll<TResult>(string cacheKey, Func<IQueryable<Category>, IQueryable<TResult>> query)
    {
        return _categoryRepository.GetAll(cacheKey, query);
    }

    public virtual async Task<List<Category>> GetAllAsync()
    {
        return await _categoryRepository.GetAllAsync();
    }

    public virtual async Task<List<TResult>> GetAllAsync<TResult>(string cacheKey, Func<IQueryable<Category>, IQueryable<TResult>> query)
    {
        return await _categoryRepository.GetAllAsync(cacheKey, query);
    }

    public virtual Category? GetByKey(string cacheKey, params object?[]? keyValues)
    {
        return _categoryRepository.GetByKey(cacheKey, keyValues);
    }

    public virtual async Task<Category?> GetByKeyAsync(string cacheKey, params object?[]? keyValues)
    {
        return await _categoryRepository.GetByKeyAsync(cacheKey, keyValues);
    }

    public virtual TResult? GetSingle<TResult>(string cacheKey, Func<IQueryable<Category>, IQueryable<TResult>> query)
    {
        return _categoryRepository.GetSingle(cacheKey, query);
    }
    public virtual async Task<TResult?> GetSingleAsync<TResult>(string cacheKey, Func<IQueryable<Category>, IQueryable<TResult>> query)
    {
        return await (_categoryRepository.GetSingleAsync(cacheKey, query));
    }

    public virtual bool Exists(Func<IQueryable<Category>, IQueryable<Category>> query)
    {
        return _categoryRepository.Exists(query);
    }

    public virtual async Task<bool> ExistsAsync(Func<IQueryable<Category>, IQueryable<Category>> query)
    {
        return await _categoryRepository.ExistsAsync(query);
    }

    public (bool succeeded, ICollection<Message>? errorMessages) TryCreate(Category category)
    {
        return TryCreateAsync(category).Result;
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
        return TryUpdateAsync(category).Result;
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
        return TryDeleteAsync(category).Result;
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
