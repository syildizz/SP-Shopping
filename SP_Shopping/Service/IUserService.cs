using SP_Shopping.Models;
using SP_Shopping.Utilities.MessageHandler;

namespace SP_Shopping.Service;
public interface IUserService
{
    bool Exists(Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>> query);
    Task<bool> ExistsAsync(Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>> query);
    List<ApplicationUser> GetAll();
    List<TResult> GetAll<TResult>(Func<IQueryable<ApplicationUser>, IQueryable<TResult>> query);
    Task<List<ApplicationUser>> GetAllAsync();
    Task<List<TResult>> GetAllAsync<TResult>(Func<IQueryable<ApplicationUser>, IQueryable<TResult>> query);
    ApplicationUser? GetByKey(params object?[]? keyValues);
    Task<ApplicationUser?> GetByKeyAsync(params object?[]? keyValues);
    TResult? GetSingle<TResult>(Func<IQueryable<ApplicationUser>, IQueryable<TResult>> query);
    Task<TResult?> GetSingleAsync<TResult>(Func<IQueryable<ApplicationUser>, IQueryable<TResult>> query);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryCreateAsync(ApplicationUser user, string password, IFormFile? image);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(ApplicationUser user);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryUpdateAsync(ApplicationUser user, IFormFile? image);
}