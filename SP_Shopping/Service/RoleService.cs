using AutoMapper;
using Microsoft.AspNetCore.Identity;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities.MessageHandler;

namespace SP_Shopping.Service;

public class RoleService
(
    IRepository<ApplicationRole> roleRepository,
    RoleManager<ApplicationRole> rolemanager,
    IMapper mapper
) : IRoleService
{
    private readonly IRepository<ApplicationRole> _roleRepository = roleRepository;
    private readonly RoleManager<ApplicationRole> _roleManager = rolemanager;
    private readonly IMapper _mapper = mapper;

    public List<TResult> GetAll<TResult>()
    {
        return _roleRepository.GetAll(q => _mapper.ProjectTo<TResult>(q));
    }

    public List<TResult> GetAll<TResult>(Func<IQueryable<ApplicationRole>, IQueryable<TResult>> query)
    {
        return _roleRepository.GetAll(query);
    }

    public async Task<List<TResult>> GetAllAsync<TResult>()
    {
        return await _roleRepository.GetAllAsync(q => _mapper.ProjectTo<TResult>(q));
    }

    public async Task<List<TResult>> GetAllAsync<TResult>(Func<IQueryable<ApplicationRole>, IQueryable<TResult>> query)
    {
        return await _roleRepository.GetAllAsync(query);
    }

    public TResult? GetSingle<TResult>(Func<IQueryable<ApplicationRole>, IQueryable<TResult>> query)
    {
        return _roleRepository.GetSingle(query);
    }

    public async Task<TResult?> GetSingleAsync<TResult>(Func<IQueryable<ApplicationRole>, IQueryable<TResult>> query)
    {
        return await _roleRepository.GetSingleAsync(query);
    }

    public bool Exists(Func<IQueryable<ApplicationRole>, IQueryable<ApplicationRole>> query)
    {
       return _roleRepository.Exists(query);
    }

    public async Task<bool> ExistsAsync(Func<IQueryable<ApplicationRole>, IQueryable<ApplicationRole>> query)
    {
        return await _roleRepository.ExistsAsync(query);
    }

    public (bool succeeded, ICollection<Message>? errorMessages) TryCreate(ApplicationRole role)
    {
        return TryCreateAsync(role).Result;
    }

    public async Task<(bool succeeded, ICollection<Message>? errorMessages)> TryCreateAsync(ApplicationRole role)
    {
        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = true;

        try
        {
            var succeeds = await _roleManager.CreateAsync(role);
            if (succeeds is not null and IdentityResult) transactionSucceeded = true;
        }
        catch (InvalidOperationException ex)
        {
#if DEBUG
            errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = $"Error saving to database: {ex.StackTrace} {ex.Data}" });
#else
            errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Error saving to database" });
#endif
            transactionSucceeded = false;
        }

        if (transactionSucceeded)
        {
            return (true, null);
        }
        else
        {
            return (false, errorMessages);
        }
    }

    public (bool succeeded, ICollection<Message>? errorMesages) TryUpdate(ApplicationRole role)
    {
        return TryUpdateAsync(role).Result;
    }

    public async Task<(bool succeeded, ICollection<Message>? errorMesages)> TryUpdateAsync(ApplicationRole role)
    {
        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = true;

        try
        {
            var succeeds = await _roleManager.UpdateAsync(role);
            if (succeeds is not null and IdentityResult) transactionSucceeded = true;
        }
        catch (InvalidOperationException ex)
        {
#if DEBUG
            errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = $"Error saving to database: {ex.StackTrace} {ex.Data}" });
#else
            errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Error saving to database" });
#endif
            transactionSucceeded = false;
        }

        if (transactionSucceeded)
        {
            return (true, null);
        }
        else
        {
            return (false, errorMessages);
        }
    }

    public (bool succeeded, ICollection<Message>? errorMessages) TryDelete(ApplicationRole role)
    {
        return TryDeleteAsync(role).Result;
    }

    public async Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(ApplicationRole role)
    {
        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = true;

        try
        {
            var succeeds = await _roleManager.DeleteAsync(role);
            if (succeeds is not null and IdentityResult) transactionSucceeded = true;
        }
        catch (InvalidOperationException ex)
        {
#if DEBUG
            errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = $"Error saving to database: {ex.StackTrace} {ex.Data}" });
#else
            errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Error saving to database" });
#endif
            transactionSucceeded = false;
        }

        if (transactionSucceeded)
        {
            return (true, null);
        }
        else
        {
            return (false, errorMessages);
        }
    }

}
