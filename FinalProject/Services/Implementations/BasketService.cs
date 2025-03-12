namespace Pronia.Services.Implementations
{
    public class BasketService : IBasketService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _http;
        private readonly ClaimsPrincipal _user;

        public BasketService(AppDbContext context, IHttpContextAccessor http)
        {
            _context = context;
            _http = http;
            _user = http.HttpContext.User;
        }

        public async Task<List<BasketItemVM>> GetBasketAsync()
        {
            List<BasketItemVM> basketVM = new();

            if (_user.Identity.IsAuthenticated)
            {
                basketVM = await _context.BasketItems
                    .Where(bi => bi.UserId == _user.FindFirstValue(ClaimTypes.NameIdentifier))
                    .Select(bi => new BasketItemVM
                    {
                        Id = bi.ProductId,
                        Quantity = bi.Quantity,
                        Price = bi.Product.DiscountPrice,
                        Image = bi.Product.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Image,
                        Name = bi.Product.Name,
                        SubTotal = bi.Product.DiscountPrice * bi.Quantity,
                    })
                    .ToListAsync();
            }
            else
            {
                List<BasketCookieItemVM> cookiesVM;
                string cookie = _http.HttpContext.Request.Cookies["basket"];

                if (cookie == null)
                {
                    return basketVM;
                }

                cookiesVM = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookie);

                List<int> cookieIds = cookiesVM.Select(c => c.Id).ToList();

                basketVM = await _context.Products.Where(p => cookieIds.Contains(p.Id))
                    .Select(p => new BasketItemVM
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.DiscountPrice,
                        Image = p.ProductImages[0].Image,

                    }).ToListAsync();

                basketVM.ForEach(bi =>
                {

                    bi.Quantity = cookiesVM.FirstOrDefault(c => c.Id == bi.Id).Count;
                    bi.SubTotal = cookiesVM.Count * bi.Price;
                });
            }

            return basketVM;

        }
    }
}
