using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.ServiceDtos.Category;
using SP_Shopping.Utilities;
using SP_Shopping.Utilities.MessageHandler;
using System.Data;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace SP_Shopping.Service;

public class CategoryService
(
    IRepositoryCaching<Category> categoryRepository,
    IRepository<Product> productRepository,
    IProductService productService,
    IMapper mapper
) : ICategoryService
{

    private readonly IRepositoryCaching<Category> _categoryRepository = categoryRepository;
    private readonly IRepository<Product> _productRepository = productRepository;
    private readonly IProductService _productService = productService;
    private readonly IMapper _mapper = mapper;

    public List<TDto> GetAll<TDto>()
    {
        return _categoryRepository.GetAll($"{nameof(Category)}_All", q => q
            .Select(c => new CategoryGetDto { Id = c.Id, Name = c.Name })
            .ProjectTo<CategoryGetDto, TDto>(_mapper)
        );
    }

    public List<TDto> GetAll<TDto>(int take)
    {
        return _categoryRepository.GetAll($"{nameof(Category)}_All", q => q
            .Take(take)
            .Select(c => new CategoryGetDto { Id = c.Id, Name = c.Name })
            .ProjectTo<CategoryGetDto, TDto>(_mapper)
        );
    }

    public List<TDto> GetAll<TDto>(Expression<Func<CategoryGetDto, TDto>> select)
    {
        return _categoryRepository.GetAll($"{nameof(Category)}_All_Select_{Guid.NewGuid()}", q => q
            .Select(c => new CategoryGetDto { Id = c.Id, Name = c.Name })
            .ProjectTo<CategoryGetDto, TDto>(_mapper)
        );
    }

    public List<TDto> GetAll<TDto>(Expression<Func<CategoryGetDto, TDto>> select, int take)
    {
        return _categoryRepository.GetAll($"{nameof(Category)}_All_Select_{Guid.NewGuid()}", q => q
            .Select(c => new CategoryGetDto { Id = c.Id, Name = c.Name })
            .ProjectTo<CategoryGetDto, TDto>(_mapper)
        );
    }

    public List<TDto> GetAll<TDto>(string? filterQuery, string? orderQuery, object? filterValue, int? take)
    {
        Func<IQueryable<CategoryGetDto>, IQueryable<CategoryGetDto>> queryFilter = q => q;
        Func<IQueryable<CategoryGetDto>, IQueryable<CategoryGetDto>> orderFilter = q => q;
        Func<IQueryable<CategoryGetDto>, IQueryable<CategoryGetDto>> takeFilter = q => q;

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

        return _categoryRepository.GetAll($"{nameof(Category)}_All_{filterQuery}_{filterValue}_{orderQuery}_{take}", q => q
            .Select(c => new CategoryGetDto { Id = c.Id, Name = c.Name })
            ._(queryFilter)
            ._(orderFilter)
            ._(takeFilter)
            ._(q => _mapper.ProjectTo<TDto>(q))
        );
    }

    public async Task<List<TDto>> GetAllAsync<TDto>()
    {
        return await _categoryRepository.GetAllAsync($"{nameof(Category)}_All", q => q
            .Select(c => new CategoryGetDto { Id = c.Id, Name = c.Name })
            .ProjectTo<CategoryGetDto, TDto>(_mapper)
        );
    }

    public async Task<List<TDto>> GetAllAsync<TDto>(int take)
    {
        return await _categoryRepository.GetAllAsync($"{nameof(Category)}_All", q => q
            .Take(take)
            .Select(c => new CategoryGetDto { Id = c.Id, Name = c.Name })
            .ProjectTo<CategoryGetDto, TDto>(_mapper)
        );
    }

    public async Task<List<TDto>> GetAllAsync<TDto>(Expression<Func<CategoryGetDto, TDto>> select)
    {
        return await _categoryRepository.GetAllAsync($"{nameof(Category)}_All_Select_{Guid.NewGuid()}", q => q
            .Select(c => new CategoryGetDto { Id = c.Id, Name = c.Name })
            .ProjectTo<CategoryGetDto, TDto>(_mapper)
        );
    }

    public async Task<List<TDto>> GetAllAsync<TDto>(Expression<Func<CategoryGetDto, TDto>> select, int take)
    {
        return await _categoryRepository.GetAllAsync($"{nameof(Category)}_All_Select_{Guid.NewGuid()}", q => q
            .Select(c => new CategoryGetDto { Id = c.Id, Name = c.Name })
            .ProjectTo<CategoryGetDto, TDto>(_mapper)
        );
    }

    public async Task<List<TDto>> GetAllAsync<TDto>(string? filterQuery, string? orderQuery, object? filterValue, int? take)
    {
        Func<IQueryable<CategoryGetDto>, IQueryable<CategoryGetDto>> queryFilter = q => q;
        Func<IQueryable<CategoryGetDto>, IQueryable<CategoryGetDto>> orderFilter = q => q;
        Func<IQueryable<CategoryGetDto>, IQueryable<CategoryGetDto>> takeFilter = q => q;

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

        return await _categoryRepository.GetAllAsync($"{nameof(Category)}_All_{filterQuery}_{filterValue}_{orderQuery}_{take}", q => q
            .Select(c => new CategoryGetDto { Id = c.Id, Name = c.Name })
            ._(queryFilter)
            ._(orderFilter)
            ._(takeFilter)
            ._(q => _mapper.ProjectTo<TDto>(q))
        );
    }

    public TDto? GetById<TDto>(int id)
    {
        return _categoryRepository.GetSingle($"{nameof(Category)}_Single_{id}", q => q
            .Select(c => new CategoryGetDto { Id = c.Id, Name = c.Name })
            .ProjectTo<CategoryGetDto, TDto>(_mapper)
        );
    }

    public TDto? GetById<TDto>(int id, Expression<Func<CategoryGetDto, TDto>> select)
    {
        return _categoryRepository.GetSingle($"{nameof(Category)}_Single_{id}", q => q
            .Select(c => new CategoryGetDto { Id = c.Id, Name = c.Name })
            .Select(select)
        );
    }

    public async Task<TDto?> GetByIdAsync<TDto>(int id)
    {
        return await _categoryRepository.GetSingleAsync($"{nameof(Category)}_Single_{id}", q => q
            .Select(c => new CategoryGetDto { Id = c.Id, Name = c.Name })
            .ProjectTo<CategoryGetDto, TDto>(_mapper)
        );
    }

    public async Task<TDto?> GetByIdAsync<TDto>(int id, Expression<Func<CategoryGetDto, TDto>> select)
    {
        return await _categoryRepository.GetSingleAsync($"{nameof(Category)}_Single_{id}", q => q
            .Select(c => new CategoryGetDto { Id = c.Id, Name = c.Name })
            .Select(select)
        );
    }

    public bool Exists(int id)
    {
        return _categoryRepository.Exists(q => q.Where(c => c.Id == id));
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _categoryRepository.ExistsAsync(q => q.Where(c => c.Id == id));
    }
    public (bool succeeded, int? id, ICollection<Message>? errorMessages) TryCreate(CategoryCreateDto cdto)
    {
        return TryCreateAsync(cdto).Result;
    }

    public async Task<(bool succeeded, int? id, ICollection<Message>? errorMessages)> TryCreateAsync(CategoryCreateDto cdto)
    {
        ICollection<Message> errorMessages = [];

        Category category = new() { Name = cdto.Name };

        bool transactionSucceeded = await _categoryRepository.DoInTransactionAsync(async () =>
        {

            await _categoryRepository.CreateAsync(category);

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
            return (true, category.Id, null);
        }
        else
        {
            return (false, null, errorMessages);
        }
    }

    public (bool succeeded, ICollection<Message>? errorMesages) TryUpdate(int id, CategoryEditDto cdto)
    {
        return TryUpdateAsync(id, cdto).Result;
    }

    public async Task<(bool succeeded, ICollection<Message>? errorMesages)> TryUpdateAsync(int id, CategoryEditDto cdto)
    {
        ICollection<Message>? errorMessages = [];

        bool transactionSucceeded = await _categoryRepository.DoInTransactionAsync(async () =>
        {

            try
            {
                int result = await _categoryRepository.UpdateCertainFieldsAsync(q => q
                    .Where(c => c.Id == id),
                    s => s
                        .SetProperty(c => c.Name, cdto.Name)
                );
                if (result == 0) throw new DbUpdateException("Updated zero entries");

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

    public (bool succeeded, ICollection<Message>? errorMessages) TryDelete(int id)
    {
        return TryDeleteAsync(id).Result;
    }

    public async Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(int id)
    {
        ICollection<Message> errorMessages = [];

        var productIds = await _productRepository.GetAllAsync(q => q.Where(c => c.Id == id).Select(p => p.Id));

        bool transactionSucceeded = await _categoryRepository.DoInTransactionAsync(async () =>
        {
            try
            {
                int result = await _categoryRepository.DeleteCertainEntriesAsync(q => q.Where(c => c.Id == id));
                if (result == 0) throw new DbUpdateException("Deleted zero entries");
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
                _productService.TryDeleteCascade(productId);
            }
            return (true, null);
        }
        else
        {
            return (false, errorMessages);
        }

    }

    public (bool succeeded, ICollection<Message>? errorMessages) TryDeleteCascade(int id)
    {
        throw new NotImplementedException("Category does not have a cascading effect");
    }

}
