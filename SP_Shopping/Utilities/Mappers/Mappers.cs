using SP_Shopping.Models;
using ProductCreateDto = SP_Shopping.ServiceDtos.Product.ProductCreateDto;
using SP_Shopping.Areas.Admin.Dtos.Product;
using System.Linq.Expressions;
using SP_Shopping.Dtos.Product;
using SP_Shopping.ServiceDtos.Product;
using SP_Shopping.ServiceDtos.Category;
using SP_Shopping.ServiceDtos.User;
using System.Linq;

namespace SP_Shopping.Utilities.Mappers;

#region ToApplicationUser

public static class MapToApplicationUser
{
    public static ApplicationUser From(this UserGetDto u) => Expression.FromUserGetDto().Compile()(u);
    public static ApplicationUser From(this UserCreateDto u) => Expression.FromUserCreateDto().Compile()(u);
    public static ApplicationUser From(this UserEditDto u) => Expression.FromUserEditDto().Compile()(u);

    public static class Expression
    {
        public static Expression<Func<UserGetDto, ApplicationUser>> FromUserGetDto()
        {
            return u => new ApplicationUser
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Roles = u.Roles,
                Description = u.Description,
                InsertionDate = u.InsertionDate,
                CartItem = u.CartItems,
                Products = u.Products.Select(MapToProduct.Expression.FromProductGetDto().Compile()).ToList()
            };
        }

        public static Expression<Func<UserCreateDto, ApplicationUser>> FromUserCreateDto()
        {
            return u => new ApplicationUser
            {
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Roles = u.Roles,
                Description = u.Description,
                InsertionDate = DateTime.Now
            };
        }

        public static Expression<Func<UserEditDto, ApplicationUser>> FromUserEditDto()
        {
            return u => new ApplicationUser
            {
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Roles = u.Roles,
                Description = u.Description,
            };
        }

    }
}

#endregion ToApplicationUser

#region ToUserGetDto

public static class MapToUserGetDto
{
    public static UserGetDto From(this ApplicationUser u) => Expression.FromApplicationUser().Compile()(u);
    public static class Expression
    {
        public static Expression<Func<ApplicationUser, UserGetDto>> FromApplicationUser()
        {
            return u => new UserGetDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Roles = u.Roles,
                Description = u.Description,
                InsertionDate = u.InsertionDate,
                CartItems = u.CartItem,
                Products = u.Products.Select(MapToProductGet.Expression.FromProduct().Compile()).ToList()
            };
        }
    }
}

#endregion ToUserGetDto

#region ToProductDetailsDto

public static class MapToProductDetailsDto
{
    public static ProductDetailsDto From(this ProductGetDto p) => Expression.FromProductGetDto().Compile()(p);
    public static class Expression
    {
        public static Expression<Func<ProductGetDto, ProductDetailsDto>> FromProductGetDto()
        {
            return p => new ProductDetailsDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryName = p.Category.Name,
                Description = p.Description,
                SubmitterId = p.SubmitterId!,
                SubmitterName = p.Submitter!.UserName!,
                InsertionDate = p.InsertionDate,
                ModificationDate = p.ModificationDate
            };
        }
    }
}

#endregion

#region ToProductCreateDto

public static class MaptoProductCreateDto
{
    public static ProductCreateDto From(this Dtos.Product.ProductCreateDto p) => Expression.FromProductCreateDtoController().Compile()(p);
    public static ProductCreateDto From(this AdminProductCreateDto p) => Expression.FromAdminProductCreateDto().Compile()(p);

    public static class Expression
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
}

#endregion ToProductCreateDto

#region ToProductEditDto

public static class MaptoProductEditDto
{
    public static ProductEditDto From(this Dtos.Product.ProductCreateDto p) => Expression.FromProductEditDtoController().Compile()(p);
    public static ProductEditDto From(this AdminProductCreateDto p) => Expression.FromAdminProductEditDto().Compile()(p);

    public static class Expression
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
}

#endregion ToProductEditDto

#region ToProduct

public static class MapToProduct {

    public static Product From(this ProductGetDto p) => Expression.FromProductGetDto().Compile()(p);
    public static Product From(this ProductCreateDto p) => Expression.FromProductCreateDto().Compile()(p);
    public static Product From(this ProductEditDto p) => Expression.FromProductEditDto().Compile()(p);

    public class Expression
    {
        public static Expression<Func<ProductGetDto, Product>> FromProductGetDto()
        {
            return p => new Product
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                Category = new Category { Id = p.Category.Id, Name = p.Category.Name },
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
                InsertionDate = DateTime.Now,
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
                ModificationDate = DateTime.Now,
            };
        }
    }

}

#endregion ToProduct

#region ToProductGet

public static class MapToProductGet
{
    public static ProductGetDto From(this Product p) => Expression.FromProduct().Compile()(p);

    public static class Expression
    {
        public static Expression<Func<Product, ProductGetDto>> FromProduct()
        {
            return p => new ProductGetDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryId = p.CategoryId,
                Category = new CategoryGetDto { Id = p.Category.Id, Name = p.Category.Name },
                Description = p.Description,
                SubmitterId = p.SubmitterId,
                Submitter = p.Submitter,
                CartItem = p.CartItem,
                InsertionDate = p.InsertionDate,
                ModificationDate = p.ModificationDate
            };
        }
    }
}

#endregion ToProductGet

