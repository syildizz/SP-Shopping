using SP_Shopping.Models;

namespace SP_Shopping.Dtos
{
    public interface IMapper<T,V> where T : class where V : class
    {
        T MapTo(V product);
        V MapFrom(T productCreateDto);
    }
}