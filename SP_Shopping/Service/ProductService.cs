using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SP_Shopping.Hubs;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.ServiceDtos.Product;
using SP_Shopping.Utilities;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.Mappers;
using SP_Shopping.Utilities.MessageHandler;
using System.Data;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace SP_Shopping.Service;

public class ProductService
(
    IRepository<Product> productRepository,
    IImageHandlerDefaulting<ProductImageKey> productImageHandler,
    IMapper mapper,
    IHubContext<ProductHub, IProductHubClient> productHubContext
) : IProductService
{

    private readonly IRepository<Product> _productRepository = productRepository;
    private readonly IImageHandlerDefaulting<ProductImageKey> _productImageHandler = productImageHandler;
    private readonly IMapper _mapper = mapper;
    private readonly IHubContext<ProductHub, IProductHubClient> _productHubContext = productHubContext;

    public List<TDto> GetAll<TDto>()
    {
        return _productRepository.GetAll(q => _mapper.ProjectTo<TDto>(q));
    }

    public List<TDto> GetAll<TDto>(Expression<Func<ProductGetDto, TDto>> select)
    {
        return _productRepository.GetAll(q => _mapper.ProjectTo<ProductGetDto>(q).Select(select));
    }


    public List<TDto> GetAll<TDto>(int take)
    {
        return _productRepository.GetAll(q => q
            .ProjectTo<Product, TDto>(_mapper)
            .Take(take)
        );
    }

    public List<TDto> GetAll<TDto>(Expression<Func<ProductGetDto, TDto>> select, int take)
    {
        return _productRepository.GetAll(q => q
            .Select(Utilities.Mappers.MapToProductGet.Expression.FromProduct())
            .ProjectTo<ProductGetDto, TDto>(_mapper)
            .Take(take)
        );
    }

    public List<TDto> GetAll<TDto>(string? filterQuery, string? orderQuery, object? filterValue, int? take)
    {
        Func<IQueryable<ProductGetDto>, IQueryable<ProductGetDto>> queryFilter = q => q;
        Func<IQueryable<ProductGetDto>, IQueryable<ProductGetDto>> orderFilter = q => q;
        Func<IQueryable<ProductGetDto>, IQueryable<ProductGetDto>> takeFilter = q => q;

        if (filterValue is not null && filterQuery is not null)
        {
            queryFilter = q => q.Where(filterQuery, filterValue);
        }

        if (orderQuery is not null)
        {
            orderFilter = q => q.OrderBy(orderQuery);
        }

        if (take is not null)
        {
            takeFilter = q => q.Take((int)take);
        }

        return _productRepository.GetAll(q => q
            .Select(Utilities.Mappers.MapToProductGet.Expression.FromProduct())
            ._(queryFilter)
            ._(orderFilter)
            ._(takeFilter)
            ._(q => _mapper.ProjectTo<TDto>(q))
        );
    }

    public async Task<List<TDto>> GetAllAsync<TDto>()
    {
        return await _productRepository.GetAllAsync(q => q
            .ProjectTo<Product, TDto>(_mapper)
        );
    }

    public async Task<List<TDto>> GetAllAsync<TDto>(Expression<Func<ProductGetDto, TDto>> select)
    {
        return await _productRepository.GetAllAsync(q => q
            .Select(Utilities.Mappers.MapToProductGet.Expression.FromProduct())
            .Select(select)
        );
    }

    public async Task<List<TDto>> GetAllAsync<TDto>(int take)
    {
        return await _productRepository.GetAllAsync(q => q
            .Select(Utilities.Mappers.MapToProductGet.Expression.FromProduct())
            .ProjectTo<ProductGetDto,TDto>(_mapper)
            .Take(take)
        );
    }
    public async Task<List<TDto>> GetAllAsync<TDto>(Expression<Func<ProductGetDto, TDto>> select, int take)
    {
        return await _productRepository.GetAllAsync(q => q
            .Select(Utilities.Mappers.MapToProductGet.Expression.FromProduct())
            .Select(select)
            .Take(take)
        );
    }

    public async Task<List<TDto>> GetAllAsync<TDto>(string? filterQuery, string? orderQuery, object? filterValue, int? take)
    {
        Func<IQueryable<ProductGetDto>, IQueryable<ProductGetDto>> queryFilter = q => q;
        Func<IQueryable<ProductGetDto>, IQueryable<ProductGetDto>> orderFilter = q => q;
        Func<IQueryable<ProductGetDto>, IQueryable<ProductGetDto>> takeFilter = q => q;

        if (filterValue is not null && filterQuery is not null)
        {
            queryFilter = q => q.Where(filterQuery, filterValue);
        }

        if (orderQuery is not null)
        {
            orderFilter = q => q.OrderBy(orderQuery);
        }

        if (take is not null)
        {
            takeFilter = q => q.Take((int)take);
        }

        return await _productRepository.GetAllAsync(q => q
            .Select(Utilities.Mappers.MapToProductGet.Expression.FromProduct())
            ._(queryFilter)
            ._(orderFilter)
            ._(takeFilter)
            ._(q => _mapper.ProjectTo<TDto>(q))
        );
    }

    public virtual TDto? GetById<TDto>(int id)
    {
        return _productRepository.GetSingle(q => q
            .Where(p => p.Id == id)
            .ProjectTo<Product, TDto>(_mapper)
        );
    }

    public virtual TDto? GetById<TDto>(int id, Expression<Func<ProductGetDto, TDto>> select)
    {
        return _productRepository.GetSingle(q => q
            .Where(p => p.Id == id)
            .Select(p => _mapper.Map<ProductGetDto>(p))
            .Select(select)
        );
    }

    public virtual async Task<TDto?> GetByIdAsync<TDto>(int id)
    {
        return await _productRepository.GetSingleAsync(q => q
            .Where(p => p.Id == id)
            .Select(Utilities.Mappers.MapToProductGet.Expression.FromProduct())
            .ProjectTo<ProductGetDto, TDto>(_mapper)
        );
    }

    public virtual async Task<TDto?> GetByIdAsync<TDto>(int id, Expression<Func<ProductGetDto, TDto>> select)
    {
        return await _productRepository.GetSingleAsync(q => q
            .Where(p => p.Id == id)
            .Select(Utilities.Mappers.MapToProductGet.Expression.FromProduct())
            .Select(select)
        );
    }

    public virtual bool Exists(int id)
    {
        return _productRepository.Exists(q => q.Where(p => p.Id == id));
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _productRepository.ExistsAsync(q => q.Where(p => p.Id == id));
    }

    public virtual string? GetByIdSubmitterId(int id)
    {
        return _productRepository.GetSingle(q => q.Where(p => p.Id == id).Select(p => p.SubmitterId));
    }

    public virtual async Task<string?> GetByIdSubmitterIdAsync(int id)
    {
        return await _productRepository.GetSingleAsync(q => q.Where(p => p.Id == id).Select(p => p.SubmitterId));
    }

    public (bool succeeded, int? id, ICollection<Message>? errorMessages) TryCreate(ProductCreateDto pdto)
    {
        return TryCreateAsync(pdto).Result;
    }

    public async Task<(bool succeeded, int? id, ICollection<Message>? errorMessages)> TryCreateAsync(ProductCreateDto pdto)
    {

        ICollection<Message> errorMessages = [];

        Product product = MapToProduct.From(pdto);

        bool transactionSucceeded = await _productRepository.DoInTransactionAsync(async () =>
        {
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

            if (pdto.Image is not null)
            {
                if (!await _productImageHandler.SetImageAsync(new(product.Id), pdto.Image))
                {
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to set image" });
                    return false;
                }
            }

            return true;
        });

        if (transactionSucceeded)
        {
            return (true, product.Id, null);
        }
        else
        {
            return (false, product.Id, errorMessages);
        }

    }

    public (bool succeeded, ICollection<Message>? errorMesages) TryUpdate(int id, ProductEditDto pdto)
    {
        return TryUpdateAsync(id, pdto).Result;
    }

    public async Task<(bool succeeded, ICollection<Message>? errorMesages)> TryUpdateAsync(int id, ProductEditDto pdto)
    {
        ICollection<Message> errorMessages = [];

        Product product = MapToProduct.From(pdto);

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
                    .Where(p => p.Id == id),
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

            if (pdto.Image is not null)
            {
                if (!await _productImageHandler.SetImageAsync(new(id), pdto.Image))
                {
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to set image" });
                    return false;
                }
            }

            return true;
        });

        if (transactionSucceeded)
        {
            await _productHubContext.Clients.All.NotifyChangeInProductWithId(id);
            return (true, null);
        }
        else
        {
            return (false, errorMessages);
        }

    }

    public (bool succeeded, ICollection<Message>? errorMessages) TryDelete(int id)
    {
        return TryDeleteAsync(id).Result;
    }

    public async Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(int id)
    {

        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = await _productRepository.DoInTransactionAsync(async () =>
        {
            try
            {
                var count = await _productRepository.DeleteCertainEntriesAsync(q => q.Where(p => p.Id == id));
                if (count != 1)
                {
#if DEBUG
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Error saving to database" });
#endif
                    return false;
                }
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
                _productImageHandler.DeleteImage(new(id));
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
            await _productHubContext.Clients.All.NotifyChangeInProductWithId(id);
            return (true, null);
        }
        else
        {
            return (false, errorMessages);
        }

    }

    public (bool succeeded, ICollection<Message>? errorMessages) TryDeleteCascade(int id)
    {

        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = false;
        try
        {
            _productImageHandler.DeleteImage(new(id));
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
            _productHubContext.Clients.All.NotifyChangeInProductWithId(id).RunSynchronously();
            return (true, null);
        }
        else
        {
            return (false, errorMessages);
        }

    }

}
