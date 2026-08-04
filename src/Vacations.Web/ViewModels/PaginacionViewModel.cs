namespace Vacations.Web.ViewModels;

public class PaginacionViewModel
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public string ActionName { get; set; } = "Index";
    public Dictionary<string, string?> RouteValues { get; set; } = new();
}
