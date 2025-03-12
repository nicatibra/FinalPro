namespace FinalProject.Areas.Admin.ViewModels
{
    public class GetSlideVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Title { get; set; }
        public string? SubTitle { get; set; }
        public string Image { get; set; }
        public int Order { get; set; }
        public bool IsDeleted { get; set; }

    }
}
