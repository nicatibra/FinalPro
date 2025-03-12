using Newtonsoft.Json.Linq;
using RestSharp;

namespace FinalProject.Services.Implementations
{
    public class GeminiService
    {
        private readonly AppDbContext _context;

        public GeminiService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetProductsFromIngredientsAsync(string meal)
        {
            try
            {
                // Call AI to get ingredients for the meal
                var ingredients = await AskCohereAsync(meal);
                if (string.IsNullOrEmpty(ingredients) || ingredients.Contains("Error"))
                {
                    return new List<Product>(); // Return an empty list if there is an error
                }

                // Split ingredients into individual components
                var ingredientList = ingredients.Split(',')
                                                .Select(i => i.Trim())
                                                .ToList();

                // Query the database for products matching any ingredient
                var products = await _context.Products
                                              .Where(p => ingredientList.Any(i => p.Name.Contains(i)))
                                              .ToListAsync();

                return products;
            }
            catch (Exception ex)
            {
                // Handle any errors and return an empty list if needed
                return new List<Product>();
            }
        }

        private async Task<string> AskCohereAsync(string meal)
        {
            try
            {
                var client = new RestClient("https://api.cohere.ai/generate");
                var request = new RestRequest("", Method.Post);

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer RryI2ry8jq23sIkdt1NQb8ScQ7JDXVLCAZn2fqE3");

                var body = new
                {
                    prompt = $@"Analyze {meal} through these 12 dimensions:\r\n1. Core Structural Components: [Flour types for dough/pasta/bread, liquid bases like broths/milks]\r\n2. Primary Proteins: [All meat cuts, seafood varieties, plant-based alternatives, eggs]\r\n3. Vegetable Components: [Fresh, frozen, pickled, roasted variants; leafy greens, roots, alliums]\r\n4. Sauces & Condiments: [Base sauces, dressings, dips, marinades, glazes, finishing oils]\r\n5. Dairy/Non-Dairy: [Cheeses, creams, butters, yogurts, non-dairy substitutes]\r\n6. Grain/Starch Elements: [Rice varieties, pasta shapes, potato forms, alternative starches]\r\n7. Spice/Herb Profile: [Whole spices, ground spices, fresh/dried herbs, spice blends]\r\n8. Sweeteners: [Granulated, liquid, natural, artificial, finishing sugars]\r\n9. Fermented/Preserved: [Pickles, kimchi, preserved lemons, capers, olives]\r\n10. Texture Elements: [Nuts, seeds, crispy toppings, crunchy vegetables]\r\n11. Regional Variations: [Include ingredients from 3 main regional versions]\r\n12. Modern Twists: [Upscale, fusion, and molecular gastronomy additions]\r\n\r\nOutput Rules:\r\n- List 150-300 ingredients across all categories\r\n- Include technical terms (e.g., '00 flour' not just 'flour')\r\n- Add measurement-specific forms: 'minced garlic', 'crushed tomatoes'\r\n- Include alternative dietary ingredients: gluten-free, vegan substitutes\r\n- Add byproducts: whey, buttermilk, duck fat\r\n- Include equipment-needed items: 'pizza stone', 'wood chips' if essential\r\n- List chemical additives: baking soda, citric acid, gelatin\r\n- Add seasonal/vintage variants: heirloom tomatoes, winter squash\r\n\r\nFormat: Strict comma-separated lowercase list, no numbering. Never explain.\r\nExample pattern for 'pizza': \r\n00 flour,active dry yeast,evoo,san marzano tomatoes,fior di latte,nduja,calabrian chilis,buffalo mozzarella,...",
                    model = "command-r-plus-08-2024",
                    temperature = 0.8
                };
                request.AddJsonBody(body);

                var response = await client.ExecuteAsync(request);
                if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
                {
                    return ParseAIResponse(response.Content);
                }
                else
                {
                    return "AI response error.";
                }
            }
            catch (Exception ex)
            {
                return "Error communicating with AI: " + ex.Message;
            }
        }

        private string ParseAIResponse(string content)
        {
            try
            {
                var jsonObject = JObject.Parse(content);
                return jsonObject["text"]?.ToString() ?? "No response text.";
            }
            catch (Exception ex)
            {
                return "Error parsing AI response: " + ex.Message;
            }
        }
    }
}
