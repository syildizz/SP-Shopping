using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities.MessageHandler;
using System.Data;

namespace SP_Shopping.Service;

public class RoleService
(
    IRepository<ApplicationRole> roleRepository,
    RoleManager<ApplicationRole> rolemanager
) : IRoleService
{
    private readonly IRepository<ApplicationRole> _roleRepository = roleRepository;
    private readonly RoleManager<ApplicationRole> _roleManager = rolemanager;

    public bool Exists(Func<IQueryable<ApplicationRole>, IQueryable<ApplicationRole>> query)
    {
       return  _roleRepository.Exists(query);
    }

    public Task<bool> ExistsAsync(Func<IQueryable<ApplicationRole>, IQueryable<ApplicationRole>> query)
    {
        return _roleRepository.ExistsAsync(query);
    }

    public List<ApplicationRole> GetAll()
    {
        return _roleRepository.GetAll();
    }

    public List<TResult> GetAll<TResult>(Func<IQueryable<ApplicationRole>, IQueryable<TResult>> query)
    {
        return _roleRepository.GetAll(query);
    }

    public Task<List<ApplicationRole>> GetAllAsync()
    {
        return _roleRepository.GetAllAsync();
    }

    public Task<List<TResult>> GetAllAsync<TResult>(Func<IQueryable<ApplicationRole>, IQueryable<TResult>> query)
    {
        return _roleRepository.GetAllAsync(query);
    }

    public ApplicationRole? GetByKey(params object?[]? keyValues)
    {
        return _roleRepository.GetByKey(keyValues);
    }

    public Task<ApplicationRole?> GetByKeyAsync(params object?[]? keyValues)
    {
        return _roleRepository.GetByKeyAsync(keyValues);
    }

    public TResult? GetSingle<TResult>(Func<IQueryable<ApplicationRole>, IQueryable<TResult>> query)
    {
        return _roleRepository.GetSingle(query);
    }

    public Task<TResult?> GetSingleAsync<TResult>(Func<IQueryable<ApplicationRole>, IQueryable<TResult>> query)
    {
        return _roleRepository.GetSingleAsync(query);
    }

    public (bool succeeded, ICollection<Message>? errorMessages) TryCreate(ApplicationRole role)
    {
        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = true;

        try
        {
            var succeeds = _roleManager.CreateAsync(role).Result;
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

    public (bool succeeded, ICollection<Message>? errorMessages) TryDelete(ApplicationRole role)
    {
        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = true;

        try
        {
            var succeeds = _roleManager.DeleteAsync(role).Result;
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

    public (bool succeeded, ICollection<Message>? errorMesages) TryUpdate(ApplicationRole role)
    {
        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = true;

        try
        {
            var succeeds = _roleManager.UpdateAsync(role).Result;
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
}
