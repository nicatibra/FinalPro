using Microsoft.AspNetCore.Authorization;

namespace FinalProject.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWishlistService _wishlistService;

        public WishlistController(AppDbContext context,
                                 UserManager<AppUser> userManager,
                                 IWishlistService wishlistService)
        {
            _context = context;
            _userManager = userManager;
            _wishlistService = wishlistService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _wishlistService.GetWishlistAsync());
        }

        public async Task<IActionResult> AddToWishlist(int? id)
        {
            if (id == null || id < 1)
                throw new BadRequestException();

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
                throw new NotFoundException();

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.Users
                    .Include(u => u.WishlistItems)
                    .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                if (!user.WishlistItems.Any(wi => wi.ProductId == id))
                {
                    user.WishlistItems.Add(new WishlistItem { ProductId = id.Value });
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                List<WishlistCookieItemVM> wishlist;
                var cookie = Request.Cookies["wishlist"];

                if (!string.IsNullOrEmpty(cookie))
                {
                    wishlist = JsonConvert.DeserializeObject<List<WishlistCookieItemVM>>(cookie);
                    if (!wishlist.Any(w => w.Id == id))
                    {
                        wishlist.Add(new WishlistCookieItemVM { Id = id.Value });
                    }
                }
                else
                {
                    wishlist = new List<WishlistCookieItemVM> { new() { Id = id.Value } };
                }

                Response.Cookies.Append("wishlist",
                    JsonConvert.SerializeObject(wishlist),
                    new CookieOptions { Expires = DateTime.Now.AddDays(30) });
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RemoveFromWishlist(int? id)
        {
            if (id == null || id < 1)
                throw new BadRequestException();

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.Users
                    .Include(u => u.WishlistItems)
                    .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                var item = user.WishlistItems.FirstOrDefault(wi => wi.ProductId == id);
                if (item != null)
                {
                    user.WishlistItems.Remove(item);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                string cookie = Request.Cookies["wishlist"];
                if (!string.IsNullOrEmpty(cookie))
                {
                    var wishlist = JsonConvert.DeserializeObject<List<WishlistCookieItemVM>>(cookie);
                    var item = wishlist.FirstOrDefault(w => w.Id == id);
                    if (item != null)
                    {
                        wishlist.Remove(item);
                        Response.Cookies.Append("wishlist",
                            JsonConvert.SerializeObject(wishlist),
                            new CookieOptions { Expires = DateTime.Now.AddDays(30) });
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}