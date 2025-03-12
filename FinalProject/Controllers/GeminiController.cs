namespace FinalProject.Controllers
{
    public class GeminiController : Controller
    {
        private readonly GeminiService _geminiService;

        public GeminiController(GeminiService geminiService)
        {
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
            {
                return BadRequest("Meal name is required");
            }

            // Call the service to get products based on the ingredients
            var products = await _geminiService.GetProductsFromIngredientsAsync(mealName);

            if (products == null || !products.Any())
            {
                return View("Error", "No products found for the given ingredients.");
            }

            // Return the list of products to the view
            ViewBag.Products = products;
            return View();
        }
    }
}
