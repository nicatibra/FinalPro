namespace FinalProject.ViewModels
{
    public class DashboardViewModel
    {
        public List<Order> Orders { get; set; }
        public List<Category> Categories { get; set; }
        public int? SelectedCategoryId { get; set; }
        public DateTime? FilterDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public decimal MonthlyEarning { get; set; }
    }
}