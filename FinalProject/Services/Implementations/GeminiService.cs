namespace FinalProject.Services.Implementations
{
    // Services/GeminiService.cs
    using Newtonsoft.Json;
    using System.Text;

    public class GeminiService
    {
        private readonly string _apiKey;
        private readonly string _url;

        public GeminiService(IConfiguration config)
        {
            _apiKey = config["Gemini:ApiKey"];
            _url = config["Gemini:Url"];
        }

        public async Task<List<string>> GetMealIngredientsAsync(string mealName)
        {
            using var httpClient = new HttpClient();
            var request = new
            {
                contents = new[]
                {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = $@"Analyze {mealName} through these 12 dimensions:\r\n1. Core Structural Components: [Flour types for dough/pasta/bread, liquid bases like broths/milks]\r\n2. Primary Proteins: [All meat cuts, seafood varieties, plant-based alternatives, eggs]\r\n3. Vegetable Components: [Fresh, frozen, pickled, roasted variants; leafy greens, roots, alliums]\r\n4. Sauces & Condiments: [Base sauces, dressings, dips, marinades, glazes, finishing oils]\r\n5. Dairy/Non-Dairy: [Cheeses, creams, butters, yogurts, non-dairy substitutes]\r\n6. Grain/Starch Elements: [Rice varieties, pasta shapes, potato forms, alternative starches]\r\n7. Spice/Herb Profile: [Whole spices, ground spices, fresh/dried herbs, spice blends]\r\n8. Sweeteners: [Granulated, liquid, natural, artificial, finishing sugars]\r\n9. Fermented/Preserved: [Pickles, kimchi, preserved lemons, capers, olives]\r\n10. Texture Elements: [Nuts, seeds, crispy toppings, crunchy vegetables]\r\n11. Regional Variations: [Include ingredients from 3 main regional versions]\r\n12. Modern Twists: [Upscale, fusion, and molecular gastronomy additions]\r\n\r\nOutput Rules:\r\n- List 150-300 ingredients across all categories\r\n- Include technical terms (e.g., '00 flour' not just 'flour')\r\n- Add measurement-specific forms: 'minced garlic', 'crushed tomatoes'\r\n- Include alternative dietary ingredients: gluten-free, vegan substitutes\r\n- Add byproducts: whey, buttermilk, duck fat\r\n- Include equipment-needed items: 'pizza stone', 'wood chips' if essential\r\n- List chemical additives: baking soda, citric acid, gelatin\r\n- Add seasonal/vintage variants: heirloom tomatoes, winter squash\r\n\r\nFormat: Strict comma-separated lowercase list, no numbering. Never explain.\r\nExample pattern for 'pizza': \r\n00 flour,active dry yeast,evoo,san marzano tomatoes,fior di latte,nduja,calabrian chilis,buffalo mozzarella,..."
                        }
                    }
                }
            }
            };

            var response = await httpClient.PostAsync($"{_url}?key={_apiKey}",
                new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<dynamic>(content);

            try
            {
                var ingredientsText = (string)result.candidates[0].content.parts[0].text;
                return ingredientsText.Split(',').Select(i => i.Trim()).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}