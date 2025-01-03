using SP_Shopping.Models;
using SP_Shopping.ServiceDtos.Product;
using SP_Shopping.Utilities.MessageHandler;
using System.Linq.Expressions;

namespace SP_Shopping.Service;
public interface IProductService
{
    List<TDto> GetAll<TDto>();
    List<TDto> GetAll<TDto>(int take);
    List<TDto> GetAll<TDto>(Expression<Func<ProductGetDto, TDto>> select);
    List<TDto> GetAll<TDto>(Expression<Func<ProductGetDto, TDto>> select, int take);
    List<TDto> GetAll<TDto>(string? filterQuery, string? orderQuery, object? filterValue, int? take);
    Task<List<TDto>> GetAllAsync<TDto>();
    Task<List<TDto>> GetAllAsync<TDto>(int take);
    Task<List<TDto>> GetAllAsync<TDto>(Expression<Func<ProductGetDto, TDto>> select);
    Task<List<TDto>> GetAllAsync<TDto>(Expression<Func<ProductGetDto, TDto>> select, int take);
    Task<List<TDto>> GetAllAsync<TDto>(string? filterQuery, string? orderQuery, object? filterValue, int? take);
    TDto? GetById<TDto>(int id);
    TDto? GetById<TDto>(int id, Expression<Func<ProductGetDto, TDto>> select);
    Task<TDto?> GetByIdAsync<TDto>(int id);
    Task<TDto?> GetByIdAsync<TDto>(int id, Expression<Func<ProductGetDto, TDto>> select);
    string? GetByIdSubmitterId(int id);
    Task<string?> GetByIdSubmitterIdAsync(int id);
    bool Exists(int id);
    Task<bool> ExistsAsync(int id);
    (bool succeeded, int? id, ICollection<Message>? errorMessages) TryCreate(ProductCreateDto pdto);
    Task<(bool succeeded, int? id, ICollection<Message>? errorMessages)> TryCreateAsync(ProductCreateDto pdto);
    (bool succeeded, ICollection<Message>? errorMesages) TryUpdate(int id, ProductEditDto pdto);
    Task<(bool succeeded, ICollection<Message>? errorMesages)> TryUpdateAsync(int id, ProductEditDto pdto);
    (bool succeeded, ICollection<Message>? errorMessages) TryDelete(int id);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(int id);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteCascadeAsync(int id);
}