﻿using SP_Shopping.Models;
using SP_Shopping.ServiceDtos;
using SP_Shopping.Utilities.MessageHandler;
using System.Linq.Expressions;

namespace SP_Shopping.Service;
public interface IProductService
{
    List<TDto> GetAll<TDto>();
    Task<List<TDto>> GetAllAsync<TDto>();
    TDto? GetById<TDto>(int id);
    TDto? GetById<TDto>(int id, Expression<Func<ProductGetDto, TDto>> select);
    Task<TDto?> GetByIdAsync<TDto>(int id);
    Task<TDto?> GetByIdAsync<TDto>(int id, Expression<Func<ProductGetDto, TDto>> select);
    (bool succeeded, int? id, ICollection<Message>? errorMessages) TryCreate(ProductCreateDto pdto);
    Task<(bool succeeded, int? id, ICollection<Message>? errorMessages)> TryCreateAsync(ProductCreateDto pdto);
    (bool succeeded, ICollection<Message>? errorMesages) TryUpdate(int id, ProductEditDto pdto);
    Task<(bool succeeded, ICollection<Message>? errorMesages)> TryUpdateAsync(int id, ProductEditDto pdto);
    (bool succeeded, ICollection<Message>? errorMessages) TryDelete(int id);
    Task<(bool succeeded, ICollection<Message>? errorMessages)> TryDeleteAsync(int id);
    (bool succeeded, ICollection<Message>? errorMessages) TryDeleteCascade(int id);
    bool Exists(int id);
    Task<bool> ExistsAsync(int id);
    string? GetByIdSubmitterId(int id);
    Task<string?> GetByIdSubmitterIdAsync(int id);
    Task<List<TDto>> GetAllAsync<TDto>(int take);
    List<TDto> GetAll<TDto>(int take);
    Task<List<TDto>> GetAllAsync<TDto>(Expression<Func<ProductGetDto, TDto>> select);
    Task<List<TDto>> GetAllAsync<TDto>(Expression<Func<ProductGetDto, TDto>> select, int take);
    Task<List<TDto>> GetAllAsync<TDto, TValue>(string filterQuery, string orderQuery, TValue filterValue, int take);
}