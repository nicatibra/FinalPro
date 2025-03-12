namespace FinalProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? categoryId, string sortOrder)
        {
            var productsQuery = _context.Products
                .Where(p => !p.IsDeleted)
                .Where(p => p.ProductBatches.Any(pb => DateTime.Now < pb.ExpirationDate && pb.Stock > 0))
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Category.Id == categoryId);
            }

            productsQuery = sortOrder == "bestselling"
                ? productsQuery.OrderByDescending(p => p.SalesCount)
                : productsQuery.OrderByDescending(p => p.CreatedAt);

            var products = await productsQuery.Take(10).ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var productList = products.Select(p => new
                {
                    id = p.Id,
                    name = p.Name,
                    price = p.Price,
                    discountPrice = p.DiscountPrice,
                    discountPercentage = p.DiscountPercentage,
                    imagePrimary = p.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true)?.Image,
                    imageSecondary = p.ProductImages.FirstOrDefault(pi => pi.IsPrimary == false)?.Image
                });

                return Json(productList);
            }

            HomeVM homeVM = new()
            {
                Products = products,
                Slides = await _context.Slides
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.Order)
                    .Take(4)
                    .ToListAsync(),
                Categories = await _context.Categories
                    .Include(c => c.Products)
                    .Where(s => !s.IsDeleted)
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
