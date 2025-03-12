// ViewModels/MealViewModel.cs
public class MealViewModel
{
    public string MealName { get; set; }
    public List<Product> Products { get; set; } = new List<Product>();
}