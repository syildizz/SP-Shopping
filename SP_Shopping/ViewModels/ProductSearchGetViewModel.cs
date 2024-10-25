using AutoMapper;
using SP_Shopping.Dtos;
using SP_Shopping.Models;
using SP_Shopping.Utilities.ImageHandler;
using SP_Shopping.Utilities.ImageHandlerKeys;
using System.Security.Policy;

namespace SP_Shopping.ViewModels;

public class ProductSearchGetViewModel
{
    public class ProductDetailsWithURL
    {
        public required ProductDetailsDto pdto;
        public required string url;
    }

    public required IEnumerable<ProductDetailsWithURL> model;

    public ProductSearchGetViewModel(IEnumerable<ProductDetailsDto> products, IImageHandlerDefaulting<ProductImageKey> productImageHandler, IMapper mapper)
    {
        model = products
            .Select(p => new ProductDetailsWithURL 
            { 
                pdto = p,
                url = productImageHandler.GetImageOrDefaultURL(mapper.Map<ProductImageKey>(p))
            })
        ;
    }



}
