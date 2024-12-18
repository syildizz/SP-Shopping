using SP_Shopping.Models;
using SP_Shopping.Utilities.MessageHandler;

namespace SP_Shopping.Service;

public interface IRoleService
{
    List<TResult> GetAll<TResult>();
    List<TResult> GetAll<TResult>(Func<IQueryable<ApplicationRole>, IQueryable<TResult>> query);
    Task<List<TResult>> GetAllAsync<TResult>();
    Task<List<TResult>> GetAllAsync<TResult>(Func<IQueryable<ApplicationRole>, IQueryable<TResult>> query);
    TResult? GetSingle<TResult>(Func<IQueryable<ApplicationRole>, IQueryable<TResult>> query);
    Task<TResult?> GetSingleAsync<TResult>(Func<IQueryable<ApplicationRole>, IQueryable<TResult>> query);
    bool Exists(Func<IQueryable<ApplicationRole>, IQueryable<ApplicationRole>> query);
    Task<bool> ExistsAsync(Func<IQueryable<ApplicationRole>, IQueryable<ApplicationRole>> query);
    (bool succeeded, ICollection<Message>? errorMessages) TryCreate(ApplicationRole role);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryCreateAsync(ApplicationRole role);
    (bool succeeded, ICollection<Message>? errorMesages) TryUpdate(ApplicationRole role);
    Task<(bool succeeded, ICollection<Message>? errorMesages)> TryUpdateAsync(ApplicationRole role);
    (bool succeeded, ICollection<Message>? errorMessages) TryDelete(ApplicationRole role);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(ApplicationRole role);
}
