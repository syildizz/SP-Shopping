﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SP_Shopping.Models;
using SP_Shopping.Repository;
using SP_Shopping.Utilities.MessageHandler;
using System.Data;

namespace SP_Shopping.Service;

public class CartItemService
(
    IRepository<CartItem> cartItemRepository,
    IMapper mapper
) : ICartItemService
{

    private readonly IRepository<CartItem> _cartItemRepository = cartItemRepository;
    private readonly IMapper _mapper = mapper;

    public virtual List<TResult> GetAll<TResult>()
    {
        return _cartItemRepository.GetAll(q => _mapper.ProjectTo<TResult>(q));
    }

    public virtual List<TResult> GetAll<TResult>(Func<IQueryable<CartItem>, IQueryable<TResult>> query)
    {
        return _cartItemRepository.GetAll(query);
    }

    public virtual async Task<List<TResult>> GetAllAsync<TResult>()
    {
        return await _cartItemRepository.GetAllAsync(q => _mapper.ProjectTo<TResult>(q));
    }

    public virtual async Task<List<TResult>> GetAllAsync<TResult>(Func<IQueryable<CartItem>, IQueryable<TResult>> query)
    {
        return await _cartItemRepository.GetAllAsync(query);
    }

    public virtual TResult? GetSingle<TResult>(Func<IQueryable<CartItem>, IQueryable<TResult>> query)
    {
        return _cartItemRepository.GetSingle(query);
    }

    public virtual async Task<TResult?> GetSingleAsync<TResult>(Func<IQueryable<CartItem>, IQueryable<TResult>> query)
    {
        return await _cartItemRepository.GetSingleAsync(query);
    }

    public virtual bool Exists(Func<IQueryable<CartItem>, IQueryable<CartItem>> query)
    {
        return _cartItemRepository.Exists(query);
    }

    public virtual async Task<bool> ExistsAsync(Func<IQueryable<CartItem>, IQueryable<CartItem>> query)
    {
        return await _cartItemRepository.ExistsAsync(query);
    }

    public (bool succeeded, ICollection<Message>? errorMessages) TryCreate(CartItem cartItem)
    {
        return TryCreateAsync(cartItem).Result;
    }

    public async Task<(bool succeeded, ICollection<Message>? errorMessages)> TryCreateAsync(CartItem cartItem)
    {
        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = await _cartItemRepository.DoInTransactionAsync(async () =>
        {

            _cartItemRepository.Create(cartItem);

            try
            {
                await _cartItemRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex is DBConcurrencyException)
                {
#if DEBUG
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = $"Error saving to database: {ex.StackTrace}" });
#else
                    errorMessages.Add(new Message { Type = Message.MessageType.Error, Content = "Error saving to database" });
#endif
                    return false;
                }
                else if (ex is DbUpdateException)
                {
                    // Exception occurs when adding same product to same users cart.
                    // This is a desired effect, therefore the below code is commented out.
                    // TODO: Analyze update exception for the above mentioned exception and throw 
                    //     otherwise
                    //_logger.LogError("Failed to create CartItem in the database for user of id \"{UserId}\" and for product of \"{ProductId}\".", cartItem.UserId, cartItem.ProductId);
                    //_messageHandler.AddMessages(TempData, [new Message { Type = Message.MessageType.Error, Content = "Error when adding product to cart" }]);
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
            return (true, null);
        }
        else
        {
            return (false, errorMessages);
        }
    }

    public (bool succeeded, ICollection<Message>? errorMesages) TryUpdate(CartItem cartItem)
    {
        return TryUpdateAsync(cartItem).Result;
    }

    public async Task<(bool succeeded, ICollection<Message>? errorMesages)> TryUpdateAsync(CartItem cartItem)
    {
        ICollection<Message>? errorMessages = [];

        bool transactionSucceeded = await _cartItemRepository.DoInTransactionAsync(async () =>
        {
            try
            {
                await _cartItemRepository.UpdateCertainFieldsAsync(
                q => q
                    .Where(c => c.UserId == cartItem.UserId && c.ProductId == cartItem.ProductId),
                s => s
                    .SetProperty(c => c.Count, cartItem.Count)
                );
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

    public (bool succeeded, ICollection<Message>? errorMessages) TryDelete(CartItem cartItem)
    {
        return TryDeleteAsync(cartItem).Result;
    }

    public async Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(CartItem cartItem)
    {

        ICollection<Message> errorMessages = [];

        bool transactionSucceeded = await _cartItemRepository.DoInTransactionAsync(async () =>
        {
            _cartItemRepository.Delete(cartItem);

            try
            {
                await _cartItemRepository.SaveChangesAsync();
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
            return (true, null);
        }
        else
        {
            return (false, errorMessages);
        }

    }

}
