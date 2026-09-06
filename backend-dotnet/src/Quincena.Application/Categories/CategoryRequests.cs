namespace Quincena.Application.Categories;

public class CreateCategoryRequest
{
    public string Name { get; set; } = null!;
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int? SortOrder { get; set; }
}

// name/color siempre se sobrescriben (igual que Node, sin COALESCE); icon/is_active/
// sort_order se preservan si no vienen en el body — ver CategoryService.UpdateAsync.
public class UpdateCategoryRequest
{
    public string Name { get; set; } = null!;
    public string Color { get; set; } = null!;
    public string? Icon { get; set; }
    public bool? IsActive { get; set; }
    public int? SortOrder { get; set; }
}
