﻿using AutoMapper;
using SP_Shopping.Dtos;
using SP_Shopping.Models;

namespace SP_Shopping.MapProfiles
{
    public class ProductDetailsProfile : Profile
    {
        public ProductDetailsProfile()
        {
            CreateMap<Product, ProductDetailsDto>()
                .ForMember(p => p.CategoryName, opt => opt.MapFrom(p => p.Category.Name));
        }
    }
}
