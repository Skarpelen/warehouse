using AutoMapper;

namespace Warehouse.Server
{
    using Warehouse.BusinessLogic.Models;
    using Warehouse.Shared.DTO;

    public static class AutoMapperConfiguration
    {
        public static void ConfigureMapping(this WebApplicationBuilder builder)
        {
            var services = builder.Services;
            services.AddAutoMapper(typeof(MappingProfile).Assembly);
        }

        public class MappingProfile : Profile
        {
            public MappingProfile()
            {
                // --- Domain → DTO ---

                CreateMap<Balance, BalanceDTO>()
                    .ForMember(dest => dest.Resource, o => o.MapFrom(src => src.Resource))
                    .ForMember(dest => dest.Unit, o => o.MapFrom(src => src.Unit));

                CreateMap<Client, ClientDTO>();
                CreateMap<Resource, ResourceDTO>();
                CreateMap<UnitOfMeasure, UnitDTO>();

                CreateMap<ShipmentItem, ShipmentItemDTO>()
                    .ForMember(dest => dest.Resource, o => o.MapFrom(src => src.Resource))
                    .ForMember(dest => dest.Unit, o => o.MapFrom(src => src.Unit));

                CreateMap<ShipmentDocument, ShipmentDocumentDTO>()
                    .ForMember(dest => dest.Client, o => o.MapFrom(src => src.Client))
                    .ForMember(dest => dest.Items, o => o.MapFrom(src => src.Items));

                CreateMap<SupplyItem, SupplyItemDTO>()
                    .ForMember(dest => dest.Resource, o => o.MapFrom(src => src.Resource))
                    .ForMember(dest => dest.Unit, o => o.MapFrom(src => src.Unit));

                CreateMap<SupplyDocument, SupplyDocumentDTO>()
                    .ForMember(dest => dest.Items, o => o.MapFrom(src => src.Items));

                // --- DTO → Domain ---

                CreateMap<BalanceDTO, Balance>()
                    .ForMember(dest => dest.Resource, o => o.Ignore())
                    .ForMember(dest => dest.Unit, o => o.Ignore());

                CreateMap<ClientDTO, Client>();
                CreateMap<ResourceDTO, Resource>();
                CreateMap<UnitDTO, UnitOfMeasure>();

                CreateMap<ShipmentItemDTO, ShipmentItem>()
                    .ForMember(dest => dest.Resource, o => o.Ignore())
                    .ForMember(dest => dest.Unit, o => o.Ignore())
                    .ForMember(dest => dest.Document, o => o.Ignore());

                CreateMap<ShipmentDocumentDTO, ShipmentDocument>()
                    .ForMember(dest => dest.Client, o => o.Ignore())
                    .ForMember(dest => dest.Items, o => o.MapFrom(src => src.Items));

                CreateMap<SupplyItemDTO, SupplyItem>()
                    .ForMember(dest => dest.Resource, o => o.Ignore())
                    .ForMember(dest => dest.Unit, o => o.Ignore())
                    .ForMember(dest => dest.Document, o => o.Ignore());

                CreateMap<SupplyDocumentDTO, SupplyDocument>()
                    .ForMember(dest => dest.Items, o => o.MapFrom(src => src.Items));
            }
        }
    }
}
