using SP_Shopping.Models;

namespace SP_Shopping.Repository
{
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
    }
}