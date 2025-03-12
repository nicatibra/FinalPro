using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CategoryController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _env = environment;
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            IQueryable<Category> query = _context.Categories;

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));
            }

            if (page < 1) throw new BadRequestException();

            int count = await query.CountAsync();
            double totalPages = Math.Ceiling((double)count / 5);
            totalPages = Math.Max(totalPages, 1);

            if (page > totalPages) throw new BadRequestException();

            ICollection<GetCategoryVM> categoryVM = await query
                .Skip((page - 1) * 5)
                .Take(5)
                .Select(c => new GetCategoryVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Image = c.Image,
                    Order = c.Order,
                    IsDeleted = c.IsDeleted
                })
                .ToListAsync();

            PaginatedVM<GetCategoryVM> paginatedVM = new()
            {
                TotalPage = totalPages,
                CurrentPage = page,
                Items = categoryVM,
                Search = search
            };

            return View(paginatedVM);
        }


        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateCategoryVM createCategoryVM)
        {
            if (!ModelState.IsValid)
                return View(createCategoryVM);

            if (await _context.Categories.AnyAsync(c => c.Name == createCategoryVM.Name))
            {
                ModelState.AddModelError(nameof(CreateCategoryVM.Name), "Category with this name already exists.");
                return View(createCategoryVM);
            }

            if (await _context.Categories.AnyAsync(c => c.Order == createCategoryVM.Order))
            {
                ModelState.AddModelError(nameof(CreateCategoryVM.Order), "Category with this order already exists.");
                return View(createCategoryVM);
            }


            if (!createCategoryVM.Photo.ValidateType("image"))
            {
                ModelState.AddModelError("Photo", "File type must be image");
                return View(createCategoryVM);
            }

            if (!createCategoryVM.Photo.ValidateSize(EFileSize.MB, 5))
            {
                ModelState.AddModelError("Photo", "File size must be less than 5");
                return View(createCategoryVM);
            }

            Category category = new()
            {
                Name = createCategoryVM.Name,
                Description = createCategoryVM.Description,
                Order = createCategoryVM.Order,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = User.Identity.Name,
                UpdatedBy = User.Identity.Name,
                IsDeleted = false,
            };

            category.Image = await createCategoryVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "imgs", "shop", "categories");

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            Category? category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                throw new NotFoundException();

            UpdateCategoryVM updateCategory = new()
            {
                Name = category.Name,
                Image = category.Image,
                Description = category.Description,
                Order = category.Order
            };

            return View(updateCategory);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateCategoryVM updateCategoryVM, int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            if (!ModelState.IsValid)
                return View(updateCategoryVM);

            if (await _context.Categories.AnyAsync(c => c.Name == updateCategoryVM.Name && c.Id != id))
            {
                ModelState.AddModelError("Name", "Category with this name already exists.");
                return View(updateCategoryVM);
            }

            if (updateCategoryVM.Photo != null)
            {
                if (!updateCategoryVM.Photo.ValidateType("image"))
                {
                    ModelState.AddModelError("Photo", "File type must be image");
                    return View(updateCategoryVM);
                }

                if (!updateCategoryVM.Photo.ValidateSize(EFileSize.MB, 5))
                {
                    ModelState.AddModelError("Photo", "File size must be less than 5");
                    return View(updateCategoryVM);
                }
            }

            Category? category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                throw new NotFoundException();

            category.Name = updateCategoryVM.Name;
            category.Description = updateCategoryVM.Description;
            category.Order = updateCategoryVM.Order;
            category.UpdatedAt = DateTime.Now;
            category.UpdatedBy = User.Identity.Name;

            if (updateCategoryVM.Photo != null)
            {
                category.Image.DeleteFile();

                category.Image = await updateCategoryVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "imgs", "shop", "categories");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            Category? category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                throw new NotFoundException();

            category.Image.DeleteFile(_env.WebRootPath, "assets", "imgs", "shop", "categories");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Archive(int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            Category? category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                throw new NotFoundException();

            if (category.IsDeleted == true)
            {
                category.IsDeleted = false;
            }
            else
            {
                category.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
