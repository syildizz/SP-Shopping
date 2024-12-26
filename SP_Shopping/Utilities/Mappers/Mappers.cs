using SP_Shopping.ServiceDtos;
using SP_Shopping.Models;
using ProductCreateDto = SP_Shopping.ServiceDtos.ProductCreateDto;
using SP_Shopping.Areas.Admin.Dtos.Product;
using System.Linq.Expressions;

namespace SP_Shopping.Utilities.Mappers;

#region ToProductCreateDto

public static class MaptoProductCreateDto
{
    public static ProductCreateDto From(this Dtos.Product.ProductCreateDto p) => MapToProductCreateDtoExpression.FromProductCreateDtoController().Compile()(p);
    public static ProductCreateDto From(this AdminProductCreateDto p) => MapToProductCreateDtoExpression.FromAdminProductCreateDto().Compile()(p);
}

public static class MapToProductCreateDtoExpression
{
    public static Expression<Func<Dtos.Product.ProductCreateDto, ProductCreateDto>> FromProductCreateDtoController()
    {
        return p => new ProductCreateDto
        { 
            Name = p.Name, 
            Price = p.Price,
            CategoryId = p.CategoryId,
            Description = p.Description,
            SubmitterId = null,
            Image = p.ProductImage == null ? null : p.ProductImage.OpenReadStream()
        };
    }

    public static Expression<Func<AdminProductCreateDto, ProductCreateDto>> FromAdminProductCreateDto()
    {
        return p => new ProductCreateDto
        { 
            Name = p.Name,
            Price = p.Price,
            CategoryId = p.CategoryId,
            Description = p.Description,
            SubmitterId = p.SubmitterId,
            Image = p.ProductImage == null ? null : p.ProductImage.OpenReadStream()
        };
    }

}

#endregion ToProductCreateDto

#region ToProductEditDto

public static class MaptoProductEditDto
{
    public static ProductEditDto From(this Dtos.Product.ProductCreateDto p) => MapToProductEditDtoExpression.FromProductEditDtoController().Compile()(p);
    public static ProductEditDto From(this AdminProductCreateDto p) => MapToProductEditDtoExpression.FromAdminProductEditDto().Compile()(p);
}

public static class MapToProductEditDtoExpression
{
    public static Expression<Func<Dtos.Product.ProductCreateDto, ProductEditDto>> FromProductEditDtoController()
    {
        return p => new ProductEditDto
        { 
            Name = p.Name, 
            Price = p.Price,
            CategoryId = p.CategoryId,
            Description = p.Description,
            SubmitterId = null,
            Image = p.ProductImage == null ? null : p.ProductImage.OpenReadStream()
        };
    }

    public static Expression<Func<AdminProductCreateDto, ProductEditDto>> FromAdminProductEditDto()
    {
        return p => new ProductEditDto
        { 
            Name = p.Name,
            Price = p.Price,
            CategoryId = p.CategoryId,
            Description = p.Description,
            SubmitterId = p.SubmitterId,
            Image = p.ProductImage == null ? null : p.ProductImage.OpenReadStream()
        };
    }
}

#endregion ToProductEditDto

#region ToProduct

public static class MapToProduct {
    public static Product From(this ProductGetDto p) => MapToProductExpression.FromProductGetDto().Compile()(p);
    public static Product From(this ProductCreateDto p) => MapToProductExpression.FromProductCreateDto().Compile()(p);
    public static Product From(this ProductEditDto p) => MapToProductExpression.FromProductEditDto().Compile()(p);
}

public static class MapToProductExpression
{
    public static Expression<Func<ProductGetDto, Product>> FromProductGetDto()
    {
        return p => new Product
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            CategoryId = p.CategoryId,
            Category = p.Category,
            Description = p.Description,
            SubmitterId = p.SubmitterId,
            Submitter = p.Submitter,
            CartItem = p.CartItem,
            InsertionDate = p.InsertionDate,
            ModificationDate = p.ModificationDate
        };
    }

    public static Expression<Func<ProductCreateDto, Product>> FromProductCreateDto()
    {
        return p => new Product
        {
            Name = p.Name,
            Price = p.Price,
            CategoryId = p.CategoryId,
            Description = p.Description,
            SubmitterId = p.SubmitterId,
        };
    }

    public static Expression<Func<ProductEditDto, Product>> FromProductEditDto()
    {
        return p => new Product
        {
            Name = p.Name,
            Price = p.Price,
            CategoryId = p.CategoryId,
            Description = p.Description,
            SubmitterId = p.SubmitterId,
        };
    }
}

#endregion ToProduct

#region ToProductGet

public static class MapToProductGet
{
    public static ProductGetDto From(this Product p) => MapToProductGetDtoExpression.FromProduct().Compile()(p);
}

public static class MapToProductGetDtoExpression
{
    public static Expression<Func<Product, ProductGetDto>> FromProduct()
    {
        return p => new ProductGetDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            CategoryId = p.CategoryId,
            Category = p.Category,
            Description = p.Description,
            SubmitterId = p.SubmitterId,
            Submitter = p.Submitter,
            CartItem = p.CartItem,
            InsertionDate = p.InsertionDate,
            ModificationDate = p.ModificationDate
        };
    }

}

#endregion ToProductGet

