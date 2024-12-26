using AutoMapper;
using SP_Shopping.Areas.Admin.Dtos.Product;
using SP_Shopping.Dtos.Product;
using SP_Shopping.ServiceDtos;
using ProductCreateDto = SP_Shopping.ServiceDtos.ProductCreateDto;

namespace SP_Shopping.MapProfiles.ServiceDtos;

public class ProductServiceDtoProfile : Profile
{
    public ProductServiceDtoProfile()
    {
        CreateMap<ProductCreateDto, Models.Product>();
        CreateMap<ProductEditDto, Models.Product>();

        CreateMap<Models.Product, ProductGetDto>();
     
        CreateMap<AdminProductCreateDto, ProductCreateDto>()
            .ForMember(p => p.Image, opt => opt.MapFrom(p => p.ProductImage.OpenReadStream()));
        CreateMap<AdminProductCreateDto, ProductEditDto>()
            .ForMember(p => p.Image, opt => opt.MapFrom(p => p.ProductImage.OpenReadStream()));

        CreateMap<Dtos.Product.ProductCreateDto, ProductCreateDto>()
            .ForMember(p => p.Image, opt => opt.MapFrom(p => p.ProductImage.OpenReadStream()));
        CreateMap<Dtos.Product.ProductCreateDto, ProductEditDto>()
            .ForMember(p => p.Image, opt => opt.MapFrom(p => p.ProductImage.OpenReadStream()));

        CreateMap<ProductGetDto, Models.Product>().ReverseMap();
        CreateMap<ProductGetDto, AdminProductDetailsDto>()
            .ForMember(p => p.SubmitterName, opt => opt.MapFrom(p => p.Submitter.UserName));
        CreateMap<ProductGetDto, ProductDetailsDto>()
            .ForMember(p => p.SubmitterName, opt => opt.MapFrom(p => p.Submitter.UserName));
        CreateMap<ProductGetDto, Dtos.Product.ProductCreateDto>();
        CreateMap<ProductGetDto, AdminProductCreateDto>();
    }
}
