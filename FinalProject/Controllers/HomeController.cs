namespace FinalProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            HomeVM homeVM = new()
            {
                Products = await _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .Where(p => p.IsDeleted == false)
                .Where(p => p.ProductBatches.Any(pb => DateTime.Now < pb.ExpirationDate))
                .Where(p => p.ProductBatches.Any(pb => pb.Stock > 0))

                .Take(10)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null))
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .ToListAsync(),

                Slides = await _context.Slides
                .Where(s => s.IsDeleted == false)
                .OrderBy(s => s.Order)
                .Take(4)
                .ToListAsync(),

                Categories = await _context.Categories
                .Include(c => c.Products)
                .Where(s => s.IsDeleted == false)
                .OrderByDescending(s => s.Products.Count)
                .ToListAsync()
            };
            return View(homeVM);
        }

        public IActionResult Error(string errorMessage)
        {
            return View(model: errorMessage);
        }

        public IActionResult Contact()
        {
            return View();
        }

    }
}
