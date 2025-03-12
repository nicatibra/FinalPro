namespace FinalProject.Areas.Admin.ViewModels
{
    public class ProductListVM
    {
        public PaginatedVM<GetProductVM> PaginatedProducts { get; set; }
        public List<Category> Categories { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string SelectedStatus { get; set; }
    }
}
