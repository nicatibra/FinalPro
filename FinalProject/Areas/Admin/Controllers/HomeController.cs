using Microsoft.AspNetCore.Authorization;

namespace FinalProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,Moderator")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId, DateTime? filterDate)
        {
            // Fetch all categories from the database
            var categories = await _context.Categories.ToListAsync();

            // Fetch orders based on the selected category and date
            IQueryable<Order> ordersQuery = _context.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Include(o => o.OrderItems)
                .Include(o => o.AppUser);

            // Apply category filter if a category is selected
            if (categoryId.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderItems.Any(oi => oi.Product.CategoryId == categoryId));
            }

            // Apply date filter if a date is selected
            if (filterDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.CreatedAt.Date == filterDate.Value.Date);
            }

            var orders = await ordersQuery.ToListAsync();

            // Calculate dynamic data for the dashboard
            var totalRevenue = orders.Sum(o => o.TotalPrice); // Total revenue from all orders
            var totalOrders = orders.Count; // Total number of orders
            var totalProducts = await _context.Products.CountAsync(); // Total number of products
            var monthlyEarning = orders
                .Where(o => o.CreatedAt.Month == DateTime.Now.Month && o.CreatedAt.Year == DateTime.Now.Year)
                .Sum(o => o.TotalPrice); // Monthly earnings (current month)

            // Create the ViewModel
            var viewModel = new DashboardViewModel
            {
                Orders = orders,
                Categories = categories,
                SelectedCategoryId = categoryId,
                FilterDate = filterDate,
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                TotalProducts = totalProducts,
                MonthlyEarning = monthlyEarning
            };

            return View(viewModel);
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product) // Include Product details for each OrderItem
                .Include(o => o.AppUser)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
