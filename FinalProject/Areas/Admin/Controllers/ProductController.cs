
namespace FinalProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, UserManager<AppUser> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _env = environment;
        }


        public async Task<IActionResult> Index(string status, string? search, int page = 1, int? categoryId = null)
        {
            IQueryable<Product> query = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductBatches);

            List<Category> categories = await _context.Categories.ToListAsync();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Active")
                {
                    query = query.Where(p => !p.IsDeleted);
                }
                else if (status == "Disabled")
                {
                    query = query.Where(p => p.IsDeleted);
                }
            }

            if (page < 1) throw new BadRequestException();

            int count = await query.CountAsync();
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling((double)count / pageSize);
            totalPages = Math.Max(totalPages, 1);

            if (page > totalPages) throw new BadRequestException();

            ICollection<GetProductVM> productVM = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new GetProductVM
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    DiscountPrice = p.DiscountPrice,
                    Image = p.ProductImages.FirstOrDefault(img => img.IsPrimary == true).Image,
                    CategoryName = p.Category.Name,
                    BrandName = p.Brand.Name,
                    InStock = p.ProductBatches.Sum(pb => pb.Stock),
                    IsDeleted = p.IsDeleted,
                })
                .ToListAsync();

            PaginatedVM<GetProductVM> paginatedVM = new()
            {
                TotalPage = totalPages,
                CurrentPage = page,
                Items = productVM,
                Search = search
            };

            ProductListVM model = new()
            {
                PaginatedProducts = paginatedVM,
                Categories = categories,
                SelectedCategoryId = categoryId,
                SelectedStatus = status
            };

            return View(model);
        }


        public async Task<IActionResult> Details(int id)
        {
            Product? product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductBatches)
                .Include(p => p.ProductTags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.ProductColors)
                    .ThenInclude(pc => pc.Color)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                throw new NotFoundException();
            }

            ProductDetailsVM productDetailsVM = new ProductDetailsVM
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                DiscountPrice = product.DiscountPrice,
                CategoryName = product.Category?.Name,
                BrandName = product.Brand?.Name,

                Image = product.ProductImages.FirstOrDefault(img => img.IsPrimary == true)?.Image,
                SecondaryImage = product.ProductImages.FirstOrDefault(img => img.IsPrimary == false)?.Image,

                InStock = product.ProductBatches.Sum(pb => pb.Stock),
                IsDeleted = product.IsDeleted,

                DiscountPercentage = product.DiscountPercentage,
                Description = product.Description,
                SKU = product.SKU,
                Weight = product.Weight,
                Volume = product.Volume,

                ExpirationDate = product.ProductBatches.FirstOrDefault()?.ExpirationDate,
                ManufacturingDate = product.ProductBatches.FirstOrDefault()?.ManufacturingDate,

                Ingredients = product.Ingredients,

                AdditionalImages = product.ProductImages
                    .Where(img => img.IsPrimary == null)
                    .Select(img => img.Image)
                    .ToList(),

                Tags = product.ProductTags
                    .Select(pt => pt.Tag.Name)
                    .ToList(),

                Colors = product.ProductColors?
                    .Select(pc => pc.Color.Name)
                    .ToList()

            };

            return View(productDetailsVM);
        }

        public async Task<IActionResult> Create()
        {
            CreateProductVM createProductVM = new()
            {
                Categories = await _context.Categories.ToListAsync(),
                Brands = await _context.Brands.ToListAsync(),
                Tags = await _context.Tags.ToListAsync(),
                Colors = await _context.Colors.ToListAsync()
            };

            return View(createProductVM);
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVM createProductVM)
        {
            createProductVM.Categories = await _context.Categories.ToListAsync();
            createProductVM.Brands = await _context.Brands.ToListAsync();
            createProductVM.Tags = await _context.Tags.ToListAsync();
            createProductVM.Colors = await _context.Colors.ToListAsync();


            if (!ModelState.IsValid)
                return View(createProductVM);


            if (!createProductVM.PrimaryPhoto.ValidateType("image"))
            {
                ModelState.AddModelError(nameof(createProductVM.PrimaryPhoto), "File type must be an image.");
                return View(createProductVM);
            }
            if (!createProductVM.PrimaryPhoto.ValidateSize(EFileSize.MB, 5))
            {
                ModelState.AddModelError(nameof(createProductVM.PrimaryPhoto), "File size must be less than 5 MB.");
                return View(createProductVM);
            }

            if (!createProductVM.HoverPhoto.ValidateType("image"))
            {
                ModelState.AddModelError(nameof(createProductVM.HoverPhoto), "File type must be an image.");
                return View(createProductVM);
            }
            if (!createProductVM.HoverPhoto.ValidateSize(EFileSize.MB, 5))
            {
                ModelState.AddModelError(nameof(createProductVM.HoverPhoto), "File size must be less than 5 MB.");
                return View(createProductVM);
            }



            if (!createProductVM.Categories.Any(c => c.Id == createProductVM.CategoryId))
            {
                ModelState.AddModelError(nameof(CreateProductVM.CategoryId), "Category does not exist.");
                return View(createProductVM);
            }

            if (!createProductVM.Brands.Any(b => b.Id == createProductVM.BrandId))
            {
                ModelState.AddModelError(nameof(CreateProductVM.BrandId), "Brand does not exist.");
                return View(createProductVM);
            }

            if (createProductVM.TagIds is not null)
            {
                if (createProductVM.TagIds.Any(tId => !createProductVM.Tags.Exists(t => t.Id == tId)))
                {
                    ModelState.AddModelError(nameof(CreateProductVM.TagIds), "Tags does not exist.");
                    return View();
                }
            }

            if (createProductVM.ColorIds is not null)
            {
                if (createProductVM.ColorIds.Any(cId => !createProductVM.Colors.Exists(c => c.Id == cId)))
                {
                    ModelState.AddModelError(nameof(CreateProductVM.ColorIds), "Colors does not exist.");
                    return View();
                }
            }


            ProductImage primaryImage = new()
            {
                Image = await createProductVM.PrimaryPhoto.CreateFileAsync(_env.WebRootPath, "assets", "imgs", "shop", "products", "primary"),
                IsPrimary = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = User.Identity.Name,
                UpdatedBy = User.Identity.Name,
                IsDeleted = false
            };

            ProductImage hoverImage = new()
            {
                Image = await createProductVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "imgs", "shop", "products", "secondary"),
                IsPrimary = false,
                CreatedAt = DateTime.Now,
                CreatedBy = User.Identity.Name,
                UpdatedBy = User.Identity.Name,
                IsDeleted = false
            };

            Product product = new()
            {
                Name = createProductVM.Name,
                Price = createProductVM.Price,
                DiscountPercentage = createProductVM.DiscountPercentage,
                DiscountPrice = createProductVM.Price - (createProductVM.Price * createProductVM.DiscountPercentage / 100),
                Description = createProductVM.Description,
                SKU = createProductVM.SKU,
                Ingredients = createProductVM.Ingredients,
                Weight = createProductVM.Weight,
                Volume = createProductVM.Volume,
                BrandId = createProductVM.BrandId,
                CategoryId = createProductVM.CategoryId,
                ProductBatches = new List<ProductBatch>
                {
                    new()
                    {
                        Stock = createProductVM.Stock,
                        ExpirationDate = createProductVM.ExpirationDate,
                        ManufacturingDate = createProductVM.ManufacturingDate,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        CreatedBy = User.Identity.Name,
                        UpdatedBy = User.Identity.Name
                    }
                },

                ProductImages = new List<ProductImage> { primaryImage, hoverImage },
                CreatedAt = DateTime.Now,
                CreatedBy = User.Identity.Name,
                UpdatedAt = DateTime.Now,
                UpdatedBy = User.Identity.Name,
                IsDeleted = false
            };

            if (createProductVM.TagIds is not null)
            {
                product.ProductTags = createProductVM.TagIds.Select(tId => new ProductTag { TagId = tId }).ToList();
            }

            if (createProductVM.ColorIds is not null)
            {
                product.ProductColors = createProductVM.ColorIds.Select(cId => new ProductColor { ColorId = cId }).ToList();
            }



            if (createProductVM.AdditionalPhotos is not null)
            {
                string text = string.Empty;

                foreach (IFormFile file in createProductVM.AdditionalPhotos)
                {

                    if (!file.ValidateType("image/"))
                    {
                        text +=
                            $"<p class=\"text-danger\">Type of {file.FileName} must be image!</p>";
                        continue;
                    }

                    if (!file.ValidateSize(EFileSize.MB, 2))
                    {
                        text += $"<p class=\"text-danger\">Size of {file.FileName} must be less than 2 MB!</p>";
                        continue;
                    }

                    product.ProductImages.Add(new ProductImage
                    {
                        Image = await file.CreateFileAsync(_env.WebRootPath, "assets", "imgs", "shop", "products", "additional"),
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        CreatedBy = User.Identity.Name,
                        UpdatedBy = User.Identity.Name,
                        IsDeleted = false,
                        IsPrimary = null
                    });
                }
                TempData["FileWarning"] = text;
            }

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id < 1)
                throw new BadRequestException();

            Product? product = await _context.Products
                .Include(p => p.ProductTags)
                .Include(p => p.ProductColors)
                .Include(p => p.ProductBatches)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new NotFoundException();

            UpdateProductVM updateProductVM = new()
            {
                Name = product.Name,
                Price = product.Price,
                DiscountPercentage = product.DiscountPercentage,
                Description = product.Description,
                SKU = product.SKU,
                Ingredients = product.Ingredients,
                Weight = product.Weight,
                Volume = product.Volume,
                Stock = product.ProductBatches.Sum(pb => pb.Stock),
                ExpirationDate = product.ProductBatches.FirstOrDefault()?.ExpirationDate,
                ManufacturingDate = product.ProductBatches.FirstOrDefault()?.ManufacturingDate,

                ImageIds = product.ProductImages
                .Where(pi => pi.IsPrimary == null)
                .Select(pi => pi.Id)
                .ToList(),
                ProductImages = product.ProductImages,

                BrandId = product.BrandId,
                CategoryId = product.CategoryId,
                TagIds = product.ProductTags.Select(pt => pt.TagId).ToList(),
                ColorIds = product.ProductColors.Select(pc => pc.ColorId).ToList(),

                Categories = await _context.Categories.ToListAsync(),
                Brands = await _context.Brands.ToListAsync(),
                Tags = await _context.Tags.ToListAsync(),
                Colors = await _context.Colors.ToListAsync()
            };

            return View(updateProductVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int? id, UpdateProductVM updateProductVM)
        {
            if (id == null || id < 1)
                throw new BadRequestException();

            Product? product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductTags)
                .Include(p => p.ProductColors)
                .Include(p => p.ProductBatches)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                throw new NotFoundException();

            updateProductVM.Categories = await _context.Categories.ToListAsync();
            updateProductVM.Brands = await _context.Brands.ToListAsync();
            updateProductVM.Tags = await _context.Tags.ToListAsync();
            updateProductVM.Colors = await _context.Colors.ToListAsync();

            if (!ModelState.IsValid)
                return View(updateProductVM);

            updateProductVM.ImageIds ??= new List<int>();

            if (updateProductVM.PrimaryPhoto != null)
            {
                if (!updateProductVM.PrimaryPhoto.ValidateType("image"))
                {
                    ModelState.AddModelError(nameof(updateProductVM.PrimaryPhoto), "File type must be an image.");
                    return View(updateProductVM);
                }
                if (!updateProductVM.PrimaryPhoto.ValidateSize(EFileSize.MB, 5))
                {
                    ModelState.AddModelError(nameof(updateProductVM.PrimaryPhoto), "File size must be less than 5 MB.");
                    return View(updateProductVM);
                }
            }

            if (updateProductVM.HoverPhoto != null)
            {
                if (!updateProductVM.HoverPhoto.ValidateType("image"))
                {
                    ModelState.AddModelError(nameof(updateProductVM.HoverPhoto), "File type must be an image.");
                    return View(updateProductVM);
                }
                if (!updateProductVM.HoverPhoto.ValidateSize(EFileSize.MB, 5))
                {
                    ModelState.AddModelError(nameof(updateProductVM.HoverPhoto), "File size must be less than 5 MB.");
                    return View(updateProductVM);
                }
            }

            if (product.CategoryId != updateProductVM.CategoryId)
            {
                if (!updateProductVM.Categories.Any(c => c.Id == updateProductVM.CategoryId))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.CategoryId), "Invalid category.");
                    return View(updateProductVM);
                }
            }
            if (product.BrandId != updateProductVM.BrandId)
            {
                if (!updateProductVM.Brands.Any(b => b.Id == updateProductVM.BrandId))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.BrandId), "Invalid brand.");
                    return View(updateProductVM);
                }
            }


            if (updateProductVM.TagIds is not null)
            {
                if (updateProductVM.TagIds.Any(tId => !updateProductVM.Tags.Any(t => t.Id == tId)))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.TagIds), "Tags do not exist.");
                    return View(updateProductVM);
                }
            }

            if (updateProductVM.ColorIds is not null)
            {
                if (updateProductVM.ColorIds.Any(cId => !updateProductVM.Colors.Any(c => c.Id == cId)))
                {
                    ModelState.AddModelError(nameof(UpdateProductVM.ColorIds), "Colors do not exist.");
                    return View(updateProductVM);
                }
            }


            if (updateProductVM.PrimaryPhoto is not null)
            {
                string fileName = await updateProductVM.PrimaryPhoto.CreateFileAsync(_env.WebRootPath, "assets", "imgs", "shop", "products", "primary");

                ProductImage primary = product.ProductImages.FirstOrDefault(p => p.IsPrimary == true);
                if (primary != null)
                {
                    primary.Image.DeleteFile(_env.WebRootPath, "assets", "imgs", "shop", "products", "primary");
                    product.ProductImages.Remove(primary);
                }
                product.ProductImages.Add(new ProductImage
                {
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = User.Identity.Name,
                    CreatedAt = DateTime.Now,
                    CreatedBy = User.Identity.Name,
                    IsDeleted = false,
                    IsPrimary = true,
                    Image = fileName
                });
            }

            if (updateProductVM.HoverPhoto is not null)
            {
                string fileName = await updateProductVM.HoverPhoto.CreateFileAsync(_env.WebRootPath, "assets", "imgs", "shop", "products", "secondary");

                ProductImage secondary = product.ProductImages.FirstOrDefault(p => p.IsPrimary == false);
                if (secondary != null)
                {
                    secondary.Image.DeleteFile(_env.WebRootPath, "assets", "imgs", "shop", "products", "secondary");
                    product.ProductImages.Remove(secondary);
                }
                product.ProductImages.Add(new ProductImage
                {
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = User.Identity.Name,
                    CreatedAt = DateTime.Now,
                    CreatedBy = User.Identity.Name,
                    IsDeleted = false,
                    IsPrimary = false,
                    Image = fileName
                });
            }

            var deletedImages = product.ProductImages
                .Where(pi => !updateProductVM.ImageIds.Contains(pi.Id) && pi.IsPrimary == null).ToList();

            deletedImages.ForEach(di => di.Image.DeleteFile(_env.WebRootPath, "assets", "imgs", "shop", "products", "additional"));

            _context.ProductImages.RemoveRange(deletedImages);

            if (updateProductVM.AdditionalPhotos is not null)
            {
                string text = string.Empty;

                foreach (IFormFile file in updateProductVM.AdditionalPhotos)
                {
                    if (!file.ValidateType("image/"))
                    {
                        text +=
                            $"<p class=\"text-danger\">Type of {file.FileName} must be image!</p>";
                        continue;
                    }

                    if (!file.ValidateSize(EFileSize.MB, 2))
                    {
                        text += $"<p class=\"text-danger\">Size of {file.FileName} must be less than 2 MB!</p>";
                        continue;
                    }

                    product.ProductImages.Add(new ProductImage
                    {
                        Image = await file.CreateFileAsync(_env.WebRootPath, "assets", "imgs", "shop", "products", "additional"),
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = User.Identity.Name,
                        CreatedBy = User.Identity.Name,
                        CreatedAt = DateTime.Now,
                        IsDeleted = false,
                        IsPrimary = null
                    });
                }
                TempData["FileWarning"] = text;
            }

            // Update ProductColors
            product.ProductColors.Clear(); // Remove existing colors
            if (updateProductVM.ColorIds != null && updateProductVM.ColorIds.Any())
            {
                product.ProductColors = updateProductVM.ColorIds
                    .Select(cId => new ProductColor { ColorId = cId })
                    .ToList();
            }

            // Update ProductTags
            product.ProductTags.Clear(); // Remove existing tags
            if (updateProductVM.TagIds != null && updateProductVM.TagIds.Any())
            {
                product.ProductTags = updateProductVM.TagIds
                    .Select(tId => new ProductTag { TagId = tId })
                    .ToList();
            }


            product.Name = updateProductVM.Name;
            product.Price = updateProductVM.Price;
            product.DiscountPercentage = updateProductVM.DiscountPercentage;
            product.DiscountPrice = updateProductVM.Price - (updateProductVM.Price * updateProductVM.DiscountPercentage / 100);
            product.Description = updateProductVM.Description;
            product.SKU = updateProductVM.SKU;
            product.Ingredients = updateProductVM.Ingredients;
            product.Weight = updateProductVM.Weight;
            product.Volume = updateProductVM.Volume;
            product.BrandId = updateProductVM.BrandId;
            product.CategoryId = updateProductVM.CategoryId;
            product.UpdatedAt = DateTime.Now;
            product.UpdatedBy = User.Identity.Name;
            foreach (var batch in product.ProductBatches)
            {
                batch.Stock = updateProductVM.Stock;
                batch.ExpirationDate = updateProductVM.ExpirationDate;
                batch.ManufacturingDate = updateProductVM.ManufacturingDate;
                batch.UpdatedAt = DateTime.Now;
                batch.UpdatedBy = User.Identity.Name;
            }


            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            Product? product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
                throw new NotFoundException();

            foreach (var image in product.ProductImages)
            {
                image.Image.DeleteFile(_env.WebRootPath, "assets", "imgs", "shop", "products", "primary");
                image.Image.DeleteFile(_env.WebRootPath, "assets", "imgs", "shop", "products", "secondary");
                image.Image.DeleteFile(_env.WebRootPath, "assets", "imgs", "shop", "products", "additional");
            }

            _context.ProductImages.RemoveRange(product.ProductImages);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Archive(int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            Product? product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
                throw new NotFoundException();

            product.IsDeleted = !product.IsDeleted;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
