using AutoMapper;
using Ecommerce.API.Models.Products;
using AppServices = Ecommerce.Application.Services.Products;

namespace Ecommerce.API.Mappings;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<CreateProductRequest, AppServices.CreateProductRequest>();
        CreateMap<UpdateProductRequest, AppServices.UpdateProductRequest>();
        CreateMap<AppServices.ProductDto, ProductResponse>();
    }
}
