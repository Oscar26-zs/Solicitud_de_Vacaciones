namespace Vacations.Application.Common;

/// <summary>
/// Resultado paginado. <see cref="AvailablePageSizes"/> expone las opciones
/// [5, 10, 15, 25] para que la vista renderice el selector.
/// </summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    public int TotalCount { get; init; }

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public static IReadOnlyList<int> AvailablePageSizes { get; } = new[] { 5, 10, 15, 25 };

    public int SelectedPageSize => PageSize;
}