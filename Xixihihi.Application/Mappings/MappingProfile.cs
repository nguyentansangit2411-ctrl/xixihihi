using AutoMapper;
using Xixihihi.Application.DTOs.Responses;
using Xixihihi.Domain.Entities;

namespace Xixihihi.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();
        
        CreateMap<User, SellerPublicDto>();
        
        CreateMap<ProductImage, ProductImageDto>();
        
        CreateMap<Category, CategoryDto>();
        
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images.OrderBy(i => i.SortOrder)));
    }
}
