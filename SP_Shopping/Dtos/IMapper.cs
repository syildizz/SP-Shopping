using SP_Shopping.Models;

namespace SP_Shopping.Dtos
{
    public interface IMapper<T,V> where T : class where V : class
    {
        T Map(V v);
        V Map(T t);
    }
}