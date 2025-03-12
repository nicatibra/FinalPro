namespace FinalProject.Controllers
{
    public class ShopController : Controller
    {
        private readonly AppDbContext _context;

        public ShopController(AppDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(
    string? search,
    int? categoryId,
    int? brandId,
    [FromQuery] List<int> colorIds,
    [FromQuery] List<int> tagIds, // Add tagIds parameter
    int key = (int)ESortType.Name,
    int page = 1,
    int pageSize = 15)
        {
            // Base query for products
            IQueryable<Product> query = _context.Products
                .Where(p => !p.IsDeleted)
                .Where(p => p.ProductBatches.Any(pb => pb.Stock > 0))
                .Where(p => p.ProductBatches.Any(pb => DateTime.Now < pb.ExpirationDate))
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null));

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            // Apply category, brand, color, and tag filters
            query = query.Where(p =>
                (!categoryId.HasValue || p.CategoryId == categoryId) &&
                (!brandId.HasValue || p.BrandId == brandId) &&
                (!colorIds.Any() || p.ProductColors.Any(pc => colorIds.Contains(pc.ColorId))) &&
                (!tagIds.Any() || p.ProductTags.Any(pt => tagIds.Contains(pt.TagId)))
            );

            // Apply sorting
            query = key switch
            {
                (int)ESortType.Name => query.OrderBy(p => p.Name),
                (int)ESortType.PriceHighToLow => query.OrderByDescending(p => p.Price),
                (int)ESortType.PriceLowToHigh => query.OrderBy(p => p.Price),
                (int)ESortType.ReleaseDate => query.OrderBy(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            // Calculate total items
            int totalItems = await query.CountAsync();
            int totalPages = 1;

            // Handle pagination
            if (pageSize <= 0) pageSize = 15; // Default page size

            if (totalItems > 0)
            {
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                totalPages = Math.Max(totalPages, 1); // Ensure totalPages is at least 1
                page = Math.Clamp(page, 1, totalPages); // Clamp page to valid range
                query = query.Skip((page - 1) * pageSize).Take(pageSize);
            }
            else
            {
                // No items to display
                totalPages = 1;
                page = 1;
            }

            // Get filtered and paginated products
            var products = await query
                .Select(p => new GetProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Image = p.ProductImages.FirstOrDefault(pi => pi.IsPrimary == true).Image,
                    SecondaryImage = p.ProductImages.FirstOrDefault(pi => pi.IsPrimary == false).Image,
                    Price = p.Price,
                    DiscountPercentage = p.DiscountPercentage,
                    DiscountPrice = p.DiscountPrice,
                    BrandName = p.Brand.Name
                })
                .ToListAsync();

            // Get filtered counts for categories, brands, and tags
            var categoryQuery = _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new GetCategoryVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Count = c.Products.Count(p =>
                        (!brandId.HasValue || p.BrandId == brandId) &&
                        (!colorIds.Any() || p.ProductColors.Any(pc => colorIds.Contains(pc.ColorId))) &&
                        (!tagIds.Any() || p.ProductTags.Any(pt => tagIds.Contains(pt.TagId)))), // Include tag filter
                    Image = c.Image

                });

            var brandQuery = _context.Brands
                .Where(b => !b.IsDeleted)
                .Select(b => new GetBrandVM
                {
                    Id = b.Id,
                    Name = b.Name,
                    Count = b.Products.Count(p =>
                        (!categoryId.HasValue || p.CategoryId == categoryId) &&
                        (!colorIds.Any() || p.ProductColors.Any(pc => colorIds.Contains(pc.ColorId))) &&
                        (!tagIds.Any() || p.ProductTags.Any(pt => tagIds.Contains(pt.TagId)))), // Include tag filter
                    Image = b.Image

                });

            // Create view model
            var shopVM = new ShopVM
            {
                Products = products,
                Categories = await categoryQuery.ToListAsync(),
                Brands = await brandQuery.ToListAsync(),
                Tags = await _context.Tags
                    .Where(t => !t.IsDeleted)
                    .Select(t => new GetTagVM
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Count = t.ProductTags.Count(pt =>
                            (!categoryId.HasValue || pt.Product.CategoryId == categoryId) &&
                            (!brandId.HasValue || pt.Product.BrandId == brandId) &&
                            (!colorIds.Any() || pt.Product.ProductColors.Any(pc => colorIds.Contains(pc.ColorId)))), // Include color filter

                    }).ToListAsync(),
                Colors = await _context.Colors
                    .Where(c => !c.IsDeleted)
                    .Select(c => new GetColorVM
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Count = c.ProductColors.Count(pc =>
                            (!categoryId.HasValue || pc.Product.CategoryId == categoryId) &&
                            (!brandId.HasValue || pc.Product.BrandId == brandId) &&
                            (!tagIds.Any() || pc.Product.ProductTags.Any(pt => tagIds.Contains(pt.TagId)))), // Include tag filter

                    }).ToListAsync(),

                // Filter parameters
                BrandId = brandId,
                ColorIds = colorIds,
                TagIds = tagIds, // Include selected tag IDs
                Search = search,
                CategoryId = categoryId,
                Key = key,

                // Pagination parameters
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPage = totalPages,
                CurrentPage = page,

                // Total products count
                TotalProducts = await _context.Products.CountAsync()
            };

            return View(shopVM);
        }


        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null || id < 1) { throw new BadRequestException($"There is no product with {id} id."); };

            Product? product = await _context.Products
                .Include(p => p.ProductImages.OrderByDescending(pi => pi.IsPrimary)) //true,false,null ardcilligi
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductBatches)
                .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
                .Include(p => p.ProductColors).ThenInclude(pt => pt.Color)

                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) { throw new NotFoundException($"Couldn't find product with {id} id."); }

            DetailVM detailVM = new DetailVM
            {
                Product = product,

                RelatedProducts = await _context.Products
                .Take(8)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Include(p => p.ProductImages.Where(pi => pi.IsPrimary != null))
                .ToListAsync()
            };

            return View(detailVM);
        }
    }
}
