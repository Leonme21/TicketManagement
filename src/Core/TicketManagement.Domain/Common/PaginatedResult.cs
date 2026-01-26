using System;
using System.Collections.Generic;

namespace TicketManagement.Domain.Common;

/// <summary>
/// Resultado paginado genérico para evitar usar Tuplas en Repositorios
/// </summary>
public class PaginatedResult<T>
{
    public List<T> Items { get; private set; }
    public int TotalCount { get; private set; }
    public int PageNumber { get; private set; }
    public int PageSize { get; private set; }
    public int TotalPages { get; private set; }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize > 0 ? pageSize : 10;
        TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
    }
}
