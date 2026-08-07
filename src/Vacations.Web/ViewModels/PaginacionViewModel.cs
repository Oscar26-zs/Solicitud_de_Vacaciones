namespace Vacations.Web.ViewModels;

public class PaginacionViewModel
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public string ActionName { get; set; } = "Index";
    public Dictionary<string, string?> RouteValues { get; set; } = new();
    public List<int> AvailablePageSizes { get; set; } = [5, 10, 15, 25];
    public int SelectedPageSize { get; set; } = 10;
}
