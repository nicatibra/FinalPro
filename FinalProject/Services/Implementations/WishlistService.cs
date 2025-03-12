namespace FinalProject.Services.Interfaces
{
    public interface IWishlistService
    {
        Task<List<Product>> GetWishlistAsync();
    }
}

namespace FinalProject.Services.Implementations
{
    public class WishlistService : IWishlistService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _http;
        private readonly ClaimsPrincipal _user;

        public WishlistService(AppDbContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
            _user = http.HttpContext.User;
        }

        public async Task<List<Product>> GetWishlistAsync()
        {
            if (_user.Identity.IsAuthenticated)
            {
                return await _context.WishlistItems
                    .Where(wi => wi.UserId == _user.FindFirstValue(ClaimTypes.NameIdentifier))
                    .Include(wi => wi.Product)
                        .ThenInclude(p => p.ProductImages)
                    .Include(wi => wi.Product)
                        .ThenInclude(p => p.ProductBatches) // Include ProductBatches
                    .Select(wi => wi.Product)
                    .ToListAsync();
            }
            else
            {
                string cookie = _http.HttpContext.Request.Cookies["wishlist"];
                if (string.IsNullOrEmpty(cookie))
                {
                    return new List<Product>();
                }

                var wishlistIds = JsonConvert.DeserializeObject<List<WishlistCookieItemVM>>(cookie)?
                    .Select(w => w.Id)
                    .ToList() ?? new List<int>();

                return await _context.Products
                    .Where(p => wishlistIds.Contains(p.Id))
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductBatches) // Include ProductBatches
                    .ToListAsync();
            }
        }
    }
}