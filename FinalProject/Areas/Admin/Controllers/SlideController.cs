namespace FinalProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SlideController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SlideController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _env = environment;
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            IQueryable<Slide> query = _context.Slides;

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.Name.ToLower().Contains(search.ToLower()));
            }

            if (page < 1) throw new BadRequestException();

            int count = await query.CountAsync();
            double totalPages = Math.Ceiling((double)count / 5);
            totalPages = Math.Max(totalPages, 1);

            if (page > totalPages) throw new BadRequestException();

            ICollection<GetSlideVM> slideVM = await query
                .Skip((page - 1) * 5)
                .Take(5)
                .Select(s => new GetSlideVM
                {
                    Id = s.Id,
                    Name = s.Name,
                    Title = s.Title,
                    SubTitle = s.SubTitle,
                    Image = s.Image,
                    Order = s.Order,
                    IsDeleted = s.IsDeleted
                })
                .ToListAsync();

            PaginatedVM<GetSlideVM> paginatedVM = new()
            {
                TotalPage = totalPages,
                CurrentPage = page,
                Items = slideVM,
                Search = search
            };

            return View(paginatedVM);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(CreateSlideVM createSlideVM)
        {
            if (!ModelState.IsValid)
            {
                return View(createSlideVM);
            }

            if (await _context.Slides.AnyAsync(s => s.Name == createSlideVM.Name))
            {
                ModelState.AddModelError(nameof(CreateSlideVM.Name), "Slide with this name already exists.");
                return View(createSlideVM);
            }

            if (await _context.Slides.AnyAsync(s => s.Order == createSlideVM.Order))
            {
                ModelState.AddModelError(nameof(CreateSlideVM.Order), "Slide with this order already exists.");
                return View(createSlideVM);
            }


            if (!createSlideVM.Photo.ValidateType("image"))
            {
                ModelState.AddModelError("Photo", "File type must be image.");
                return View(createSlideVM);
            }

            if (!createSlideVM.Photo.ValidateSize(EFileSize.MB, 5))
            {
                ModelState.AddModelError("Photo", "File size must be less than 5.");
                return View(createSlideVM);
            }

            Slide slide = new()
            {
                Name = createSlideVM.Name,
                Title = createSlideVM.Title,
                SubTitle = createSlideVM.SubTitle,
                Order = createSlideVM.Order,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                UpdatedBy = User.Identity.Name,
                CreatedBy = User.Identity.Name,
                IsDeleted = false,
            };

            slide.Image = await createSlideVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "imgs", "slider");

            await _context.Slides.AddAsync(slide);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            Slide? slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (slide == null)
                throw new NotFoundException();

            UpdateSlideVM updateSlideVM = new()
            {
                Name = slide.Name,
                Image = slide.Image,
                Title = slide.Title,
                SubTitle = slide.SubTitle,
                Order = slide.Order
            };

            return View(updateSlideVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateSlideVM updateSlideVM, int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            if (!ModelState.IsValid)
                return View(updateSlideVM);

            if (await _context.Slides.AnyAsync(s => s.Name == updateSlideVM.Name && s.Id != id))
            {
                ModelState.AddModelError("Name", "Slide with this name already exists.");
                return View(updateSlideVM);
            }

            if (updateSlideVM.Photo != null)
            {
                if (!updateSlideVM.Photo.ValidateType("image"))
                {
                    ModelState.AddModelError("Photo", "File type must be image.");
                    return View(updateSlideVM);
                }

                if (!updateSlideVM.Photo.ValidateSize(EFileSize.MB, 5))
                {
                    ModelState.AddModelError("Photo", "File size must be less than 5.");
                    return View(updateSlideVM);
                }
            }

            Slide? slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (slide == null)
                throw new NotFoundException();

            slide.Name = updateSlideVM.Name;
            slide.Title = updateSlideVM.Title;
            slide.SubTitle = updateSlideVM.SubTitle;
            slide.Order = updateSlideVM.Order;
            slide.UpdatedAt = DateTime.Now;
            slide.UpdatedBy = User.Identity.Name;

            if (updateSlideVM.Photo != null)
            {
                slide.Image.DeleteFile();

                slide.Image = await updateSlideVM.Photo.CreateFileAsync(_env.WebRootPath, "assets", "imgs", "slider");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            Slide? slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (slide == null)
                throw new NotFoundException();

            slide.Image.DeleteFile(_env.WebRootPath, "assets", "imgs", "slider");

            _context.Slides.Remove(slide);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Archive(int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            Slide? slide = await _context.Slides.FirstOrDefaultAsync(s => s.Id == id);
            if (slide == null)
                throw new NotFoundException();

            if (slide.IsDeleted == true)
            {
                slide.IsDeleted = false;
            }
            else
            {
                slide.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
