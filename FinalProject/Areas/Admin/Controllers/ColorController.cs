namespace FinalProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ColorController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ColorController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _env = environment;
        }

        public async Task<IActionResult> Index(string? search, int page = 2)
        {
            IQueryable<Color> query = _context.Colors;

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));
            }

            if (page < 1) throw new BadRequestException();

            int count = await query.CountAsync();
            double totalPages = Math.Ceiling((double)count / 5);
            totalPages = Math.Max(totalPages, 1);

            if (page > totalPages) throw new BadRequestException();

            ICollection<GetColorVM> getColorVM = await query
                .Skip((page - 1) * 5)
                .Take(5)
                .Select(c => new GetColorVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    Image = c.Image,
                    IsDeleted = c.IsDeleted
                })
                .ToListAsync();

            PaginatedVM<GetColorVM> paginatedVM = new()
            {
                TotalPage = totalPages,
                CurrentPage = page,
                Items = getColorVM,
                Search = search
            };

            return View(paginatedVM);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateColorVM createColorVM)
        {
            if (!ModelState.IsValid)
                return View(createColorVM);

            if (await _context.Colors.AnyAsync(c => c.Name.Trim() == createColorVM.Name.Trim()))
            {
                ModelState.AddModelError(nameof(createColorVM.Name), $"{createColorVM.Name} color already exist.");
                return View();
            }

            if (!createColorVM.Photo.ValidateType("image"))
            {
                ModelState.AddModelError("Photo", "File type must be image");
                return View(createColorVM);
            }

            if (!createColorVM.Photo.ValidateSize(EFileSize.MB, 5))
            {
                ModelState.AddModelError("Photo", "File size must be less than 5");
                return View(createColorVM);
            }

            Color color = new()
            {
                Name = createColorVM.Name,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                UpdatedBy = User.Identity.Name,
                CreatedBy = User.Identity.Name,
                IsDeleted = false
            };

            color.Image = await createColorVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "imgs", "shop", "colors");

            await _context.Colors.AddAsync(color);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int? id)
        {
            if (id is null || id < 1)
                throw new BadRequestException();

            Color? color = await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);
            if (color is null)
                throw new NotFoundException();

            UpdateColorVM colorVM = new UpdateColorVM()
            {
                Name = color.Name
            };

            return View(colorVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int? id, UpdateColorVM colorVM)
        {
            if (id is null || id < 1)
                throw new BadRequestException();

            if (!ModelState.IsValid)
                return View(colorVM);

            if (await _context.Colors.AnyAsync(c => c.Name == colorVM.Name && c.Id != id))
            {
                ModelState.AddModelError("Name", "Color with this name already exists.");
                return View(colorVM);
            }

            if (colorVM.Photo != null)
            {
                if (!colorVM.Photo.ValidateType("image"))
                {
                    ModelState.AddModelError("Photo", "File type must be image");
                    return View(colorVM);
                }

                if (!colorVM.Photo.ValidateSize(EFileSize.MB, 5))
                {
                    ModelState.AddModelError("Photo", "File size must be less than 5");
                    return View(colorVM);
                }
            }

            Color? color = await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);
            if (color is null)
                throw new NotFoundException();

            color.Name = colorVM.Name;
            color.UpdatedBy = User.Identity.Name;
            color.UpdatedAt = DateTime.Now;

            if (colorVM.Photo != null)
            {
                color.Image.DeleteFile();

                color.Image = await colorVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "imgs", "shop", "colors");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            Color? color = await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);
            if (color == null)
                throw new NotFoundException();

            color.Image.DeleteFile(_env.WebRootPath, "assets", "imgs", "shop", "colors");

            _context.Colors.Remove(color);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Archive(int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            Color? color = await _context.Colors.FirstOrDefaultAsync(c => c.Id == id);
            if (color == null)
                throw new NotFoundException();

            if (color.IsDeleted == true)
            {
                color.IsDeleted = false;
            }
            else
            {
                color.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
