using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using TicketManagement.Application.Contracts.Authentication;
using TicketManagement.Application.Contracts.Categories;
using TicketManagement.Application.Contracts.Tickets;
using TicketManagement.Domain.Entities;

namespace TicketManagement.Application.Common.Mappings;

/// <summary>
/// Configuración de mapeos de AutoMapper
/// Entity → DTO (nunca DTO → Entity, eso es responsabilidad de Handlers)
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ==================== USER MAPPINGS ====================
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

        // ==================== CATEGORY MAPPINGS ====================
        CreateMap<Category, CategoryDto>();

        // ==================== TICKET MAPPINGS ====================

        // Ticket → TicketDto (para listas)
        CreateMap<Ticket, TicketDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => src.Creator.FullName))
            .ForMember(dest => dest.CreatorEmail, opt => opt.MapFrom(src => src.Creator.Email))
            .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => src.AssignedTo != null ? src.AssignedTo.FullName : null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.CommentsCount, opt => opt.MapFrom(src => src.Comments.Count))
            .ForMember(dest => dest.AttachmentsCount, opt => opt.MapFrom(src => src.Attachments.Count));

        // Ticket → TicketDetailsDto (para detalle completo)
        CreateMap<Ticket, TicketDetailsDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => src.Creator.FullName))
            .ForMember(dest => dest.CreatorEmail, opt => opt.MapFrom(src => src.Creator.Email))
            .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => src.AssignedTo != null ? src.AssignedTo.FullName : null))
            .ForMember(dest => dest.AssignedToEmail, opt => opt.MapFrom(src => src.AssignedTo != null ? src.AssignedTo.Email : null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.Comments, opt => opt.MapFrom(src => src.Comments))
            .ForMember(dest => dest.Attachments, opt => opt.MapFrom(src => src.Attachments));

        // ==================== COMMENT MAPPINGS ====================
        CreateMap<Comment, CommentDto>()
            .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.FullName))
            .ForMember(dest => dest.AuthorEmail, opt => opt.MapFrom(src => src.Author.Email));

        // ==================== ATTACHMENT MAPPINGS ====================
        CreateMap<Attachment, AttachmentDto>()
            .ForMember(dest => dest.FileSizeFormatted, opt => opt.MapFrom(src => src.GetFileSizeFormatted()))
            .ForMember(dest => dest.DownloadUrl, opt => opt.MapFrom(src => $"/api/attachments/{src.Id}/download"));
    }
}
