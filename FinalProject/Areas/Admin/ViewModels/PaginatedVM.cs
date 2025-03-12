namespace FinalProject.Areas.Admin.ViewModels
{
    public class PaginatedVM<T>
    {
        public double TotalPage { get; set; }

        public int CurrentPage { get; set; }

        public ICollection<T> Items { get; set; }

        public string Search { get; set; }
    }
}
