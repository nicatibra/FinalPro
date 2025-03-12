
using Microsoft.AspNetCore.Authorization;

namespace FinalProject.Controllers
{
    public class BasketController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBasketService _basketService;

        public BasketController(AppDbContext context, UserManager<AppUser> userManager, IBasketService basketService)
        {
            _context = context;
            _userManager = userManager;
            _basketService = basketService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _basketService.GetBasketAsync());
        }

        public async Task<IActionResult> AddBasket(int? id, int quantity = 1)
        {
            if (id == null || id < 1 || quantity < 1)
                throw new BadRequestException();

            var product = await _context.Products
                .Include(p => p.ProductBatches)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new NotFoundException();

            // Check stock availability
            var totalStock = product.ProductBatches.Sum(pb => pb.Stock);
            if (totalStock < quantity)
            {
                TempData["Error"] = "Not enough stock available";
                return RedirectToAction("Details", "Products", new { id });
            }

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.Users
                    .Include(u => u.BasketItems)
                    .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                var item = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);

                if (item == null)
                {
                    user.BasketItems.Add(new BasketItem
                    {
                        ProductId = id.Value,
                        Quantity = quantity
                    });
                }
                else
                {
                    item.Quantity += quantity;
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                List<BasketCookieItemVM> basket;
                var cookie = Request.Cookies["basket"];

                if (!string.IsNullOrEmpty(cookie))
                {
                    basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookie);
                }
                else
                {
                    basket = new List<BasketCookieItemVM>();
                }

                var existingItem = basket.FirstOrDefault(b => b.Id == id);
                if (existingItem != null)
                {
                    existingItem.Count += quantity;
                }
                else
                {
                    basket.Add(new BasketCookieItemVM
                    {
                        Id = id.Value,
                        Count = quantity
                    });
                }

                Response.Cookies.Append("basket",
                    JsonConvert.SerializeObject(basket),
                    new CookieOptions { Expires = DateTime.Now.AddDays(7) });
            }

            return RedirectToAction(nameof(GetBasket));
        }

        public async Task<IActionResult> GetBasket()
        {
            //return PartialView("BasketPartialView", await _basketService.GetBasketAsync());
            return RedirectToAction("Index", "Home", await _basketService.GetBasketAsync());
        }


        public async Task<IActionResult> RemoveBasketItem(int? id)
        {
            if (id == null || id < 1)
                throw new BadRequestException();

            if (User.Identity.IsAuthenticated)
            {
                // Authenticated user: Remove item from the database
                AppUser? user = await _userManager.Users
                    .Include(u => u.BasketItems)
                    .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                if (user == null)
                    return Unauthorized();

                BasketItem? item = user.BasketItems.FirstOrDefault(bi => bi.ProductId == id);

                if (item == null)
                    throw new NotFoundException();

                user.BasketItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Guest user: Remove item from the cookie
                string? cookies = Request.Cookies["basket"];
                if (cookies == null)
                    throw new NotFoundException();

                List<BasketCookieItemVM> basket = JsonConvert.DeserializeObject<List<BasketCookieItemVM>>(cookies);
                BasketCookieItemVM? item = basket.FirstOrDefault(b => b.Id == id);

                if (item == null)
                    throw new NotFoundException();

                basket.Remove(item);

                string json = JsonConvert.SerializeObject(basket);
                Response.Cookies.Append("basket", json);
            }

            return RedirectToAction(nameof(GetBasket));
            //return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ClearBasket()
        {
            if (User.Identity.IsAuthenticated)
            {
                AppUser? user = await _userManager.Users
                    .Include(u => u.BasketItems)
                    .FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

                if (user == null)
                    return Unauthorized();

                user.BasketItems.Clear();
                await _context.SaveChangesAsync();
            }
            else
            {
                Response.Cookies.Delete("basket");
            }

            return RedirectToAction(nameof(Index));
        }



        [Authorize(Roles = "Member,Admin")]
        public async Task<IActionResult> Checkout()
        {
            OrderVM orderVM = new()
            {
                BasketInOrderItemsVMs = await _context.BasketItems
                .Where(bi => bi.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                .Select(bi => new BasketInOrderItemVM
                {
                    Image = bi.Product.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Image,
                    Quantity = bi.Quantity,
                    Name = bi.Product.Name,
                    Price = bi.Product.DiscountPrice,
                    Subtotal = bi.Product.DiscountPrice * bi.Quantity
                })
                .ToListAsync()
            };
            return View(orderVM);
        }


        [HttpPost]
        public async Task<IActionResult> Checkout(OrderVM orderVM, string? stripeEmail, string? stripeToken)
        {
            AppUser user = await _userManager.GetUserAsync(User);

            List<BasketItem> basketItems = await _context.BasketItems
            .Where(bi => bi.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
            .Include(bi => bi.Product).ThenInclude(p => p.ProductImages)
            .Include(bi => bi.Product).ThenInclude(p => p.ProductBatches)
            .ToListAsync();

            if (!ModelState.IsValid)
            {
                orderVM.BasketInOrderItemsVMs = basketItems
                    .Select(bi => new BasketInOrderItemVM
                    {
                        Image = bi.Product.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Image,
                        Quantity = bi.Quantity,
                        Name = bi.Product.Name,
                        Price = bi.Product.DiscountPrice,
                        Subtotal = bi.Product.DiscountPrice * bi.Quantity
                    })
                .ToList();

                return View(orderVM);
            }

            foreach (var basketItem in basketItems)
            {
                int requiredQty = basketItem.Quantity;
                var product = basketItem.Product;

                // Order batches by expiration date (oldest first)
                var batches = product.ProductBatches?
                    .OrderBy(pb => pb.ExpirationDate)
                    .ToList();

                if (batches == null || batches.Sum(b => b.Stock) < requiredQty)
                {
                    ModelState.AddModelError(string.Empty,
                        $"Insufficient stock for product {product.Name}. You ordered {basketItem.Quantity}, but only {batches?.Sum(b => b.Stock) ?? 0} available.");
                    orderVM.BasketInOrderItemsVMs = basketItems
                        .Select(bi => new BasketInOrderItemVM
                        {
                            Image = bi.Product.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true)?.Image,
                            Quantity = bi.Quantity,
                            Name = bi.Product.Name,
                            Price = bi.Product.DiscountPrice,
                            Subtotal = bi.Product.DiscountPrice * bi.Quantity
                        })
                        .ToList();
                    return View(orderVM);
                }

                foreach (var batch in batches)
                {
                    if (requiredQty <= 0)
                        break;

                    int deduct = Math.Min(batch.Stock, requiredQty);
                    batch.Stock -= deduct;
                    requiredQty -= deduct;
                }
            }

            decimal total = basketItems.Sum(bi => bi.Product.DiscountPrice * bi.Quantity);

            Order order = new Order()
            {
                Country = orderVM.Country.ToString(),
                CityOrTown = orderVM.CityOrTown,
                Address = orderVM.Address,
                State = orderVM.State,
                PostOrZipCode = orderVM.PostOrZipCode,
                AdditionalInfo = orderVM.AdditionalInfo,

                Email = orderVM.Email != null ? orderVM.Email : user.Email,
                FirstName = orderVM.FirstName,
                LastName = orderVM.LastName,
                AppUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),

                Status = null,
                CreatedBy = User.Identity.Name,
                CreatedAt = DateTime.Now,
                DateString = DateTime.Now.ToString("f"),
                IsDeleted = false,

                OrderItems = basketItems.Select(bi => new OrderItem
                {
                    AppUserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                    Count = bi.Quantity,
                    Price = bi.Product.DiscountPrice,
                    ProductId = bi.ProductId,

                }).ToList(),

                TotalPrice = total
            };


            ////Stripe
            //var optionCust = new CustomerCreateOptions
            //{
            //    Email = stripeEmail,
            //    Name = user.Firstname + " " + user.Lastname,
            //    Phone = "+994 51 66"
            //};
            //var serviceCust = new CustomerService();
            //Customer customer = serviceCust.Create(optionCust);

            //total = total * 100;
            //var optionsCharge = new ChargeCreateOptions
            //{

            //    Amount = (long)total,
            //    Currency = "USD",
            //    Description = "Product Selling amount",
            //    Source = stripeToken,
            //    ReceiptEmail = stripeEmail
            //};

            //var serviceCharge = new ChargeService();
            //Charge charge = serviceCharge.Create(optionsCharge);
            //if (charge.Status != "succeeded")
            //{
            //    ViewBag.BasketItems = basketItems;
            //    ModelState.AddModelError("Address", "There is a problem with payment.");
            //    return View();
            //}


            await _context.Orders.AddAsync(order);
            _context.BasketItems.RemoveRange(basketItems);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
