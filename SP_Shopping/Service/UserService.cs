using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.ServiceDtos.User;
using SP_Shopping.Utilities;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using SP_Shopping.Utilities.MessageHandler;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace SP_Shopping.Service;

public class UserService
(
    IRepository<ApplicationUser> userRepository,
    IRepository<Product> productRepository,
    UserManager<ApplicationUser> userManager,
    IImageHandlerDefaulting<UserProfileImageKey> profileImageHandler,
    IProductService productService,
    IMapper mapper
) : IUserService
{
    private readonly IRepository<ApplicationUser> _userRepository = userRepository;
    private readonly IRepository<Product> _productRepository = productRepository;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IImageHandlerDefaulting<UserProfileImageKey> _profileImageHandler = profileImageHandler;
    private readonly IProductService _productService = productService;
    private readonly IMapper _mapper = mapper;

    public async Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(ApplicationUser user)
    {

        ICollection<Message> errorMessages = [];

        var productIds = await _productRepository.GetAllAsync(q => q.Where(p => p.SubmitterId == user.Id).Select(p => p.Id));

        bool transactionSucceeded = await _userRepository.DoInTransactionAsync(async () =>
        {

            IdentityResult result;

            result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to delete user" });
                return false;
            }

            try
            {
                _profileImageHandler.DeleteImage(new(user.Id));
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
            foreach (var productId in productIds)
            {
                if (!(await _productService.TryDeleteCascadeAsync(productId)).TryOut(out var errMsgs)) {
                    return (false, errMsgs);
                }
            }
            return (true, null);
        }
        else
        {
            return (false, errorMessages);
        }

    }

    public List<UserGetDto> GetAll()
    {
        return _userRepository.GetAll(q => q.Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser()));
    }

    public List<TDto> GetAll<TDto>()
    {
        return _userRepository.GetAll(q => q
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
            .ProjectTo<UserGetDto, TDto>(_mapper)
        );
    }

    public List<UserGetDto> GetAll(int take)
    {
        return _userRepository.GetAll(q => q
            .Take(take)
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
        );
    }

    public List<TDto> GetAll<TDto>(int take)
    {
        return _userRepository.GetAll(q => q
            .Take(take)
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
            .ProjectTo<UserGetDto, TDto>(_mapper)
        );
    }

    public List<TDto> GetAll<TDto>(Expression<Func<UserGetDto, TDto>> select)
    {
        return _userRepository.GetAll(q => q
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
            .Select(select)
        );
    }

    public List<TDto> GetAll<TDto>(Expression<Func<UserGetDto, TDto>> select, int take)
    {
        return _userRepository.GetAll(q => q
            .Take(take)
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
            .Select(select)
        );
    }

    public List<UserGetDto> GetAll(string? filterQuery, string? orderQuery, object? filterValue, int? take)
    {
        Func<IQueryable<UserGetDto>, IQueryable<UserGetDto>> queryFilter = q => q;
        Func<IQueryable<UserGetDto>, IQueryable<UserGetDto>> orderFilter = q => q;
        Func<IQueryable<UserGetDto>, IQueryable<UserGetDto>> takeFilter = q => q;

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

        return _userRepository.GetAll(q => q
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
            ._(queryFilter)
            ._(orderFilter)
            ._(takeFilter)
        );
    }

    public List<TDto> GetAll<TDto>(string? filterQuery, string? orderQuery, object? filterValue, int? take)
    {
        Func<IQueryable<UserGetDto>, IQueryable<UserGetDto>> queryFilter = q => q;
        Func<IQueryable<UserGetDto>, IQueryable<UserGetDto>> orderFilter = q => q;
        Func<IQueryable<UserGetDto>, IQueryable<UserGetDto>> takeFilter = q => q;

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

        return _userRepository.GetAll(q => q
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
            ._(queryFilter)
            ._(orderFilter)
            ._(takeFilter)
            .ProjectTo<UserGetDto, TDto>(_mapper)
        );
    }

    public async Task<List<UserGetDto>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync(q => q.Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser()));
    }

    public async Task<List<TDto>> GetAllAsync<TDto>()
    {
        return await _userRepository.GetAllAsync(q => q
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
            .ProjectTo<UserGetDto, TDto>(_mapper)
        );
    }

    public async Task<List<UserGetDto>> GetAllAsync(int take)
    {
        return await _userRepository.GetAllAsync(q => q
            .Take(take)
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
        );
    }

    public async Task<List<TDto>> GetAllAsync<TDto>(int take)
    {
        return await _userRepository.GetAllAsync(q => q
            .Take(take)
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
            .ProjectTo<UserGetDto, TDto>(_mapper)
        );
    }

    public async Task<List<TDto>> GetAllAsync<TDto>(Expression<Func<UserGetDto, TDto>> select)
    {
        return await _userRepository.GetAllAsync(q => q
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
            .Select(select)
        );
    }

    public async Task<List<TDto>> GetAllAsync<TDto>(Expression<Func<UserGetDto, TDto>> select, int take)
    {
        return await _userRepository.GetAllAsync(q => q
            .Take(take)
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
            .Select(select)
        );
    }

    public async Task<List<UserGetDto>> GetAllAsync(string? filterQuery, string? orderQuery, object? filterValue, int? take)
    {
        Func<IQueryable<UserGetDto>, IQueryable<UserGetDto>> queryFilter = q => q;
        Func<IQueryable<UserGetDto>, IQueryable<UserGetDto>> orderFilter = q => q;
        Func<IQueryable<UserGetDto>, IQueryable<UserGetDto>> takeFilter = q => q;

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

        return await _userRepository.GetAllAsync(q => q
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
            ._(queryFilter)
            ._(orderFilter)
            ._(takeFilter)
        );
    }

    public async Task<List<TDto>> GetAllAsync<TDto>(string? filterQuery, string? orderQuery, object? filterValue, int? take)
    {
        Func<IQueryable<UserGetDto>, IQueryable<UserGetDto>> queryFilter = q => q;
        Func<IQueryable<UserGetDto>, IQueryable<UserGetDto>> orderFilter = q => q;
        Func<IQueryable<UserGetDto>, IQueryable<UserGetDto>> takeFilter = q => q;

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

        return await _userRepository.GetAllAsync(q => q
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
            ._(queryFilter)
            ._(orderFilter)
            ._(takeFilter)
            .ProjectTo<UserGetDto, TDto>(_mapper)
        );
    }

    public UserGetDto? GetById(string id)
    {
        return _userRepository.GetSingle(q => q
            .Where(u => u.Id == id)
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
        );
    }

    public TDto? GetById<TDto>(string id)
    {
        return _userRepository.GetSingle(q => q
            .Where(u => u.Id == id)
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
            .ProjectTo<UserGetDto, TDto>(_mapper)
        );
    }

    public TDto? GetById<TDto>(string id, Expression<Func<UserGetDto, TDto>> select)
    {
        return _userRepository.GetSingle(q => q
            .Where(u => u.Id == id)
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
            .Select(select)
        );
    }

    public async Task<UserGetDto?> GetByIdAsync(string id)
    {
        return await _userRepository.GetSingleAsync(q => q
            .Where(u => u.Id == id)
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
        );
    }

    public async Task<TDto?> GetByIdAsync<TDto>(string id)
    {
        return await _userRepository.GetSingleAsync(q => q
            .Where(u => u.Id == id)
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
            .ProjectTo<UserGetDto, TDto>(_mapper)
        );
    }

    public async Task<TDto?> GetByIdAsync<TDto>(string id, Expression<Func<UserGetDto, TDto>> select)
    {
        return await _userRepository.GetSingleAsync(q => q
            .Where(u => u.Id == id)
            .Select(Utilities.Mappers.MapToUserGetDto.Expression.FromApplicationUser())
            .Select(select)
        );
    }

    public bool Exists(string id)
    {
        return _userRepository.Exists(q => q.Where(u => u.Id == id));
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _userRepository.ExistsAsync(q => q.Where(u => u.Id == id));
    }

    public (bool succeeded, string? id, ICollection<Message>? errorMessages) TryCreate(UserCreateDto udto)
    {
        return TryCreateAsync(udto).Result;
    }

    public async Task<(bool succeeded, string? id, ICollection<Message>? errorMessages)> TryCreateAsync(UserCreateDto udto)
    {
        ICollection<Message> errorMessages = [];

        string? userId = null;

        bool transactionSucceeded = await _userRepository.DoInTransactionAsync(async () =>
        {

            ApplicationUser user = Utilities.Mappers.MapToApplicationUser.From(udto);

            IdentityResult succeeded;

            user.InsertionDate = DateTime.Now;
            user.EmailConfirmed = true;
            user.PhoneNumberConfirmed = true;
            succeeded = await _userManager.CreateAsync(user, udto.Password);
            if (!succeeded.Succeeded)
            {
                errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to create user" });
                return false;
            }

            if (udto.Image is not null)
            {
                if (!await _profileImageHandler.SetImageAsync(new(user.Id), udto.Image))
                {
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to set profile picture" });
                    return false;
                }
            }

            userId = user.Id;
            return true;

        });

        if (transactionSucceeded)
        {
            return (true, userId, null);
        }
        else
        {
            return (false, userId, errorMessages);
        }

    }

    public (bool succeeded, ICollection<Message>? errorMesages) TryUpdate(string id, UserEditDto udto)
    {
        return TryUpdateAsync(id, udto).Result;
    }

    public async Task<(bool succeeded, ICollection<Message>? errorMesages)> TryUpdateAsync(string id, UserEditDto udto)
    {
        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = await _userRepository.DoInTransactionAsync(async () =>
        {

            ApplicationUser? _user = await _userRepository.GetSingleAsync(q => q.Where(u => u.Id == id).Include(u => u.Roles));
            if (_user is null)
            {
                errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "User has invalid id" });
                return false;
            }

            ApplicationUser user = Utilities.Mappers.MapToApplicationUser.From(udto);

            _user.UserName = udto.UserName;
            _user.Email = udto.Email;
            _user.PhoneNumber = udto.PhoneNumber;
            _user.Description = udto.Description;
            _user.Roles = udto.Roles;

            IdentityResult succeeded;

            // Password
            string errorMessage = "";
            if (udto.Password is not null and string password)
            {
                if (!await _userManager.CheckPasswordAsync(_user, password))
                {
                    try
                    {
                        errorMessage = "Unable to change user password";
                        succeeded = await _userManager.ResetPasswordAsync(
                            _user, 
                            await _userManager.GeneratePasswordResetTokenAsync(_user), 
                            password
                        );
                    }
                    catch (InvalidOperationException ex) 
                    { 
                        #if DEBUG
                        errorMessage = $"{errorMessage}: {ex.StackTrace}";
                        #endif
                        succeeded = IdentityResult.Failed();
                    }
                    if (!succeeded.Succeeded)
                    {
                        #if DEBUG
                        errorMessages = errorMessages.Concat(succeeded.Errors.Select(e => new Message { Type = Message.MessageType.Error, Content = $"{errorMessage}: {e.Description}" })).ToList();
                        #else
                        errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = errorMessage });
                        #endif
                        return false;
                    }
                }
            }

            try
            {
                errorMessage = "Unable to update user";
                succeeded = await _userManager.UpdateAsync(_user);
            }
            catch (InvalidOperationException ex) 
            { 
                #if DEBUG
                errorMessage = $"{errorMessage}: {ex.StackTrace}";
                #endif
                succeeded = IdentityResult.Failed();
            }
            if (!succeeded.Succeeded)
            {
                #if DEBUG
                errorMessages = errorMessages.Concat(succeeded.Errors.Select(e => new Message { Type = Message.MessageType.Error, Content = $"{errorMessage}: {e.Description}" })).ToList();
                #else
                errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = errorMessage });
                #endif
                return false;
            }

            if (udto.Image is not null)
            {
                if (!await _profileImageHandler.SetImageAsync(new(user.Id), udto.Image))
                {
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Failed to set profile picture" });
                    return false;
                }

                return true;
            }

            return true;

        });

        if (transactionSucceeded)
        {
            return (true, null);
        }
        else
        {
            return (false, errorMessages);
        }

    }

    public (bool succeeded, ICollection<Message>? errorMessages) TryDelete(string id)
    {
        return TryDeleteAsync(id).Result;
    }

    public async Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(string id)
    {
        ICollection<Message> errorMessages = [];

        var productIds = await _productRepository.GetAllAsync(q => q.Where(p => p.SubmitterId == id).Select(p => p.Id));

        bool transactionSucceeded = await _userRepository.DoInTransactionAsync(async () =>
        {

            try
            {

                var count = await _userRepository.DeleteCertainEntriesAsync(q => q.Where(p => p.Id == id));
                if (count != 1)
                {
#if DEBUG
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Error saving to database" });
#endif
                    return false;
                }

                _profileImageHandler.DeleteImage(new(id));

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
            foreach (var productId in productIds)
            {
                if (!(await _productService.TryDeleteCascadeAsync(productId)).TryOut(out var errMsgs)) {
                    return (false, errMsgs);
                }
            }
            return (true, null);
        }
        else
        {
            return (false, errorMessages);
        }

    }

    public Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteCascadeAsync(string id)
    {
        throw new NotImplementedException("Category does not have a cascading effect");
    }
}

