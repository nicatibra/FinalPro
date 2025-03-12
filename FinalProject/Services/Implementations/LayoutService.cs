namespace FinalProject.Services.Implementations
{
    public class LayoutService : ILayoutService
    {
        private readonly AppDbContext _context;

        public LayoutService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            Dictionary<string, string> settings = await _context.LayoutSettings.ToDictionaryAsync(s => s.Key, s => s.Value);

            return settings;
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.Products.Count)
                .ToListAsync();
        }
    }
}
