using SP_Shopping.Models;
using SP_Shopping.Utilities.MessageHandler;

namespace SP_Shopping.Service;
public interface IUserService
{
    List<TResult> GetAll<TResult>();
    List<TResult> GetAll<TResult>(Func<IQueryable<ApplicationUser>, IQueryable<TResult>> query);
    Task<List<TResult>> GetAllAsync<TResult>();
    Task<List<TResult>> GetAllAsync<TResult>(Func<IQueryable<ApplicationUser>, IQueryable<TResult>> query);
    TResult? GetSingle<TResult>(Func<IQueryable<ApplicationUser>, IQueryable<TResult>> query);
    Task<TResult?> GetSingleAsync<TResult>(Func<IQueryable<ApplicationUser>, IQueryable<TResult>> query);
    bool Exists(Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>> query);
    Task<bool> ExistsAsync(Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>> query);
    (bool succeeded, ICollection<Message>? errorMessages) TryCreate(ApplicationUser user, string password, IFormFile? image);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryCreateAsync(ApplicationUser user, string password, IFormFile? image);
    (bool succeeded, ICollection<Message>? errorMessages) TryUpdate(ApplicationUser user, IFormFile? image);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryUpdateAsync(ApplicationUser user, IFormFile? image);
    (bool succeeded, ICollection<Message>? errorMessages) TryDelete(ApplicationUser user);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(ApplicationUser user);
}