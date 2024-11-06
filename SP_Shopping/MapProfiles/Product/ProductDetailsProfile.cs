using AutoMapper;
using SP_Shopping.Dtos.Product;
using SP_Shopping.Models;

namespace SP_Shopping.MapProfiles.Product;

public class ProductDetailsProfile : Profile
{
    public ProductDetailsProfile()
    {
        CreateMap<Models.Product, ProductDetailsDto>()
            .ForMember(p => p.SubmitterName, opt => opt.MapFrom(pp => pp.Submitter!.UserName))
            .ReverseMap();
    }
}
