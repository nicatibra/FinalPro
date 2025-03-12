using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BrandController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BrandController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _env = environment;
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            IQueryable<Brand> query = _context.Brands;

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.Name.ToLower().Contains(search.ToLower()));
            }

            if (page < 1) throw new BadRequestException();

            int count = await query.CountAsync();
            double totalPages = Math.Ceiling((double)count / 5);
            totalPages = Math.Max(totalPages, 1);

            if (page > totalPages) throw new BadRequestException();

            ICollection<GetBrandVM> brandVM = await query
                .Skip((page - 1) * 5)
                .Take(5)
                .Select(c => new GetBrandVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Image = c.Image,
                    IsDeleted = c.IsDeleted
                })
                .ToListAsync();

            PaginatedVM<GetBrandVM> paginatedVM = new()
            {
                TotalPage = totalPages,
                CurrentPage = page,
                Items = brandVM,
                Search = search
            };

            return View(paginatedVM);
        }


        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateBrandVM createBrandVM)
        {
            if (!ModelState.IsValid)
                return View(createBrandVM);

            if (await _context.Brands.AnyAsync(b => b.Name == createBrandVM.Name))
            {
                ModelState.AddModelError(nameof(CreateBrandVM.Name), "Brand with this name already exists.");
                return View(createBrandVM);
            }


            if (!createBrandVM.Photo.ValidateType("image"))
            {
                ModelState.AddModelError("Photo", "File type must be image");
                return View(createBrandVM);
            }

            if (!createBrandVM.Photo.ValidateSize(EFileSize.MB, 5))
            {
                ModelState.AddModelError("Photo", "File size must be less than 5");
                return View(createBrandVM);
            }

            Brand brand = new()
            {
                Name = createBrandVM.Name,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = User.Identity.Name,
                UpdatedBy = User.Identity.Name,
                IsDeleted = false,
            };

            brand.Image = await createBrandVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "imgs", "shop", "brands");

            await _context.Brands.AddAsync(brand);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            Brand? brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id);
            if (brand == null)
                throw new NotFoundException();

            UpdateBrandVM updateBrandVM = new()
            {
                Name = brand.Name,
                Image = brand.Image,
            };

            return View(updateBrandVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateBrandVM updateBrandVM, int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            if (!ModelState.IsValid)
                return View(updateBrandVM);

            if (await _context.Brands.AnyAsync(b => b.Name == updateBrandVM.Name && b.Id != id))
            {
                ModelState.AddModelError("Name", "Brand with this name already exists.");
                return View(updateBrandVM);
            }

            if (updateBrandVM.Photo != null)
            {
                if (!updateBrandVM.Photo.ValidateType("image"))
                {
                    ModelState.AddModelError("Photo", "File type must be image");
                    return View(updateBrandVM);
                }

                if (!updateBrandVM.Photo.ValidateSize(EFileSize.MB, 5))
                {
                    ModelState.AddModelError("Photo", "File size must be less than 5");
                    return View(updateBrandVM);
                }
            }

            Brand? brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id);
            if (brand == null)
                throw new NotFoundException();

            brand.Name = updateBrandVM.Name;
            brand.UpdatedAt = DateTime.Now;
            brand.UpdatedBy = User.Identity.Name;

            if (updateBrandVM.Photo != null)
            {
                brand.Image.DeleteFile();

                brand.Image = await updateBrandVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "imgs", "shop", "brands");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            Brand? brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id);
            if (brand == null)
                throw new NotFoundException();

            brand.Image.DeleteFile(_env.WebRootPath, "assets", "imgs", "shop", "brands");

            _context.Brands.Remove(brand);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Archive(int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            Brand? brand = await _context.Brands.FirstOrDefaultAsync(b => b.Id == id);
            if (brand == null)
                throw new NotFoundException();

            if (brand.IsDeleted == true)
            {
                brand.IsDeleted = false;
            }
            else
            {
                brand.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
