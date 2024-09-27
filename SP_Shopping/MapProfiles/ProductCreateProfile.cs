using AutoMapper;
using SP_Shopping.Dtos;
using SP_Shopping.Models;

namespace SP_Shopping.MapProfiles
{
    public class ProductCreateProfile : Profile
    {
        public ProductCreateProfile()
        {
            CreateMap<Product, ProductCreateDto>().ReverseMap();
        }
    }
}
