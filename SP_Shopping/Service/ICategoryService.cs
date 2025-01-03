using SP_Shopping.Models;
using SP_Shopping.ServiceDtos.Category;
using SP_Shopping.Utilities.MessageHandler;
using System.Linq.Expressions;

namespace SP_Shopping.Service;
public interface ICategoryService
{
    List<CategoryGetDto> GetAll();
    List<TDto> GetAll<TDto>();
    List<CategoryGetDto> GetAll(int take);
    List<TDto> GetAll<TDto>(int take);
    List<TDto> GetAll<TDto>(Expression<Func<CategoryGetDto, TDto>> select);
    List<TDto> GetAll<TDto>(Expression<Func<CategoryGetDto, TDto>> select, int take);
    List<CategoryGetDto> GetAll(string? filterQuery, string? orderQuery, object? filterValue, int? take);
    Task<List<CategoryGetDto>> GetAllAsync(string? filterQuery, string? orderQuery, object? filterValue, int? take);
    List<TDto> GetAll<TDto>(string? filterQuery, string? orderQuery, object? filterValue, int? take);
    Task<List<CategoryGetDto>> GetAllAsync();
    Task<List<TDto>> GetAllAsync<TDto>();
    Task<List<CategoryGetDto>> GetAllAsync(int take);
    Task<List<TDto>> GetAllAsync<TDto>(int take);
    Task<List<TDto>> GetAllAsync<TDto>(Expression<Func<CategoryGetDto, TDto>> select);
    Task<List<TDto>> GetAllAsync<TDto>(Expression<Func<CategoryGetDto, TDto>> select, int take);
    Task<List<TDto>> GetAllAsync<TDto>(string? filterQuery, string? orderQuery, object? filterValue, int? take);
    CategoryGetDto? GetById(int id);
    TDto? GetById<TDto>(int id);
    TDto? GetById<TDto>(int id, Expression<Func<CategoryGetDto, TDto>> select);
    Task<CategoryGetDto?> GetByIdAsync(int id);
    Task<TDto?> GetByIdAsync<TDto>(int id);
    Task<TDto?> GetByIdAsync<TDto>(int id, Expression<Func<CategoryGetDto, TDto>> select);
    bool Exists(int id);
    Task<bool> ExistsAsync(int id);
    (bool succeeded, int? id, ICollection<Message>? errorMessages) TryCreate(CategoryCreateDto cdto);
    Task<(bool succeeded, int? id, ICollection<Message>? errorMessages)> TryCreateAsync(CategoryCreateDto cdto);
    (bool succeeded, ICollection<Message>? errorMesages) TryUpdate(int id, CategoryEditDto cdto);
    Task<(bool succeeded, ICollection<Message>? errorMesages)> TryUpdateAsync(int id, CategoryEditDto cdto);
    (bool succeeded, ICollection<Message>? errorMessages) TryDelete(int id);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(int id);
    (bool succeeded, ICollection<Message>? errorMessages) TryDeleteCascade(int id);
}