
using Elfie.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace SP_Shopping.Repository;

public interface IRepository<TEntity> where TEntity : class
{
    bool Create(TEntity entity);
    Task<bool> CreateAsync(TEntity entity);
    bool Delete(TEntity entity);
    Task<bool> DeleteAsync(TEntity entity);
    List<TEntity> GetAll();
    Task<List<TEntity>> GetAllAsync();
    TEntity? GetById(int id);
    Task<TEntity?> GetByIdAsync(int id);
    bool Update(TEntity entity);

    Task<bool> UpdateAsync(TEntity entity);
    Task<bool> ExecuteUpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate, Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> setPropertyCalls);
}