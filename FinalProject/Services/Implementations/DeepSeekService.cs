// Services/DeepSeekService.cs
using System.Net.Http.Headers;
using System.Text;

public class DeepSeekService
{
    private readonly string _apiKey;
    private readonly string _apiUrl;
    private readonly AppDbContext _context;

    public DeepSeekService(IConfiguration config, AppDbContext context)
    {
        _apiKey = config["DeepSeek:ApiKey"];
        _apiUrl = config["DeepSeek:ApiUrl"];
        _context = context;
    }

    public async Task<List<Product>> GetMealIngredientsAsync(string mealName)
    {
        var prompt = $@"Analyze {mealName} and list ONLY the common ingredients separated by commas. 
                      Return ONLY ingredient names in lowercase without explanations. 
                      Example response for 'pizza': flour,yeast,tomato sauce,cheese,pepperoni,olive oil";

        var ingredients = await GetIngredientsFromAI(prompt);
        return await FindMatchingProducts(ingredients);
    }

    private async Task<List<string>> GetIngredientsFromAI(string prompt)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var request = new
        {
            model = "deepseek-chat",
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.3
        };

        var response = await client.PostAsync(_apiUrl,
            new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<dynamic>(content);

        try
        {
            string ingredients = result.choices[0].message.content;
            return ingredients.Split(',').Select(i => i.Trim().ToLower()).ToList();
        }
        catch
        {
            return new List<string>();
        }
    }

    private async Task<List<Product>> FindMatchingProducts(List<string> ingredients)
    {
        return await _context.Products
            .Where(p => ingredients.Any(i => p.Name.ToLower().Contains(i)))
            .Include(p => p.ProductImages)
            .ToListAsync();
    }
}