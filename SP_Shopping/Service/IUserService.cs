using SP_Shopping.ServiceDtos.User;
using SP_Shopping.Utilities.MessageHandler;
using System.Linq.Expressions;

namespace SP_Shopping.Service;
public interface IUserService
{
    List<UserGetDto> GetAll();
    List<TDto> GetAll<TDto>();
    List<UserGetDto> GetAll(int take);
    List<TDto> GetAll<TDto>(int take);
    List<TDto> GetAll<TDto>(Expression<Func<UserGetDto, TDto>> select);
    List<TDto> GetAll<TDto>(Expression<Func<UserGetDto, TDto>> select, int take);
    List<UserGetDto> GetAll(string? filterQuery, string? orderQuery, object? filterValue, int? take);
    List<TDto> GetAll<TDto>(string? filterQuery, string? orderQuery, object? filterValue, int? take);
    Task<List<UserGetDto>> GetAllAsync();
    Task<List<TDto>> GetAllAsync<TDto>();
    Task<List<UserGetDto>> GetAllAsync(int take);
    Task<List<TDto>> GetAllAsync<TDto>(int take);
    Task<List<TDto>> GetAllAsync<TDto>(Expression<Func<UserGetDto, TDto>> select);
    Task<List<TDto>> GetAllAsync<TDto>(Expression<Func<UserGetDto, TDto>> select, int take);
    Task<List<UserGetDto>> GetAllAsync(string? filterQuery, string? orderQuery, object? filterValue, int? take);
    Task<List<TDto>> GetAllAsync<TDto>(string? filterQuery, string? orderQuery, object? filterValue, int? take);
    UserGetDto? GetById(string id);
    TDto? GetById<TDto>(string id);
    TDto? GetById<TDto>(string id, Expression<Func<UserGetDto, TDto>> select);
    Task<UserGetDto?> GetByIdAsync(string id);
    Task<TDto?> GetByIdAsync<TDto>(string id);
    Task<TDto?> GetByIdAsync<TDto>(string id, Expression<Func<UserGetDto, TDto>> select);
    bool Exists(string id);
    Task<bool> ExistsAsync(string id);
    (bool succeeded, string? id, ICollection<Message>? errorMessages) TryCreate(UserCreateDto udto);
    Task<(bool succeeded, string? id, ICollection<Message>? errorMessages)> TryCreateAsync(UserCreateDto udto);
    (bool succeeded, ICollection<Message>? errorMesages) TryUpdate(string id, UserEditDto udto);
    Task<(bool succeeded, ICollection<Message>? errorMesages)> TryUpdateAsync(string id, UserEditDto udto);
    (bool succeeded, ICollection<Message>? errorMessages) TryDelete(string id);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(string id);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteCascadeAsync(string id);
}