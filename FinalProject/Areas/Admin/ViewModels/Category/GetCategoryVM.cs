namespace FinalProject.Areas.Admin.ViewModels
{
    public class GetCategoryVM
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Count { get; set; }
        public int Order { get; set; }
        public bool IsDeleted { get; set; }
    }
}
