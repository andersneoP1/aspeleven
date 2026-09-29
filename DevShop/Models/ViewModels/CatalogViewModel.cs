using System.Collections.Generic;

namespace DevShop.Models.ViewModels;

public class CatalogViewModel
{
    public List<ProductViewModel> Products { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
    public int Page { get; set; }
    public int TotalPages { get; set; }
    public string? SearchString { get; set; }
    public int? CategoryId { get; set; }
    public string? SortBy { get; set; }
}
