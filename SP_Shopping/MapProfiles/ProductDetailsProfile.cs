using AutoMapper;
using SP_Shopping.Dtos;
using SP_Shopping.Models;

namespace SP_Shopping.MapProfiles;

public class ProductDetailsProfile : Profile
{
    public ProductDetailsProfile()
    {
        CreateMap<Product, ProductDetailsDto>()
            .ForMember(p => p.SubmitterName, opt => opt.MapFrom(pp => pp.Submitter!.UserName))
            .ReverseMap();
    }
}
