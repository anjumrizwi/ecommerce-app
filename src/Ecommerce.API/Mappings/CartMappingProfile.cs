using AutoMapper;
using Ecommerce.API.Models.Carts;
using AppServices = Ecommerce.Application.Services.Carts;

namespace Ecommerce.API.Mappings;

public class CartMappingProfile : Profile
{
    public CartMappingProfile()
    {
        CreateMap<AppServices.CartItemDto, CartItemResponse>();
        CreateMap<AppServices.CartDto, CartResponse>();
    }
}