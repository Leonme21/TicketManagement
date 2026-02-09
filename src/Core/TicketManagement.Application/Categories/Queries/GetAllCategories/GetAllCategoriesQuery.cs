using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using TicketManagement.Application.Contracts.Categories;

namespace TicketManagement.Application.Categories.Queries.GetAllCategories;

/// <summary>
/// Query para obtener todas las categorías activas
/// </summary>
public record GetAllCategoriesQuery : IRequest<List<CategoryDto>>;
