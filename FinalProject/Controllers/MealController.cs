// Controllers/MealController.cs
public class MealController : Controller
{
    private readonly DeepSeekService _deepSeekService;

    public MealController(DeepSeekService deepSeekService)
    {
        _deepSeekService = deepSeekService;
    }

    public IActionResult Index()
    {
        return View(new MealViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Index(MealViewModel model)
    {
        if (ModelState.IsValid)
        {
            model.Products = await _deepSeekService.GetMealIngredientsAsync(model.MealName);
        }
        return View(model);
    }
}