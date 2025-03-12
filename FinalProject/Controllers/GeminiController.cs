namespace FinalProject.Controllers
{
    // Controllers/GeminiController.cs
    public class GeminiController : Controller
    {
        private readonly AppDbContext _context;
        private readonly GeminiService _geminiService;

        public GeminiController(AppDbContext context, GeminiService geminiService)
        {
            _context = context;
            _geminiService = geminiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SuggestProducts(string mealName)
        {
            if (string.IsNullOrEmpty(mealName))
                return BadRequest("Meal name is required");

            var ingredients = await _geminiService.GetMealIngredientsAsync(mealName);

            var products = await _context.Products
                .Where(p => (p.Name != null && ingredients.Any(i => p.Name.Contains(i))))
                .Include(p => p.ProductImages)
                .ToListAsync();


            return View(products);
        }
    }
}