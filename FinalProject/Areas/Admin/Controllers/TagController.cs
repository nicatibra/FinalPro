using Microsoft.AspNetCore.Mvc;

namespace FinalProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class TagController : Controller
    {
        private readonly AppDbContext _context;

        public TagController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            IQueryable<Tag> query = _context.Tags;

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));
            }

            if (page < 1) throw new BadRequestException();

            int count = await query.CountAsync();
            double totalPages = Math.Ceiling((double)count / 5);
            totalPages = Math.Max(totalPages, 1);

            if (page > totalPages) throw new BadRequestException();

            ICollection<GetTagVM> tagVM = await query
                .Skip((page - 1) * 5)
                .Take(5)
                .Select(c => new GetTagVM
                {
                    Id = c.Id,
                    Name = c.Name,
                    IsDeleted = c.IsDeleted
                })
                .ToListAsync();

            PaginatedVM<GetTagVM> paginatedVM = new()
            {
                TotalPage = totalPages,
                CurrentPage = page,
                Items = tagVM,
                Search = search
            };

            return View(paginatedVM);
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTagVM createTagVM)
        {
            if (!ModelState.IsValid)
                return View(createTagVM);

            if (await _context.Tags.AnyAsync(t => t.Name == createTagVM.Name))
            {
                ModelState.AddModelError(nameof(CreateTagVM.Name), "Tag with this name already exists.");
                return View(createTagVM);
            }


            Tag tag = new()
            {
                Name = createTagVM.Name,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                CreatedBy = User.Identity.Name,
                UpdatedBy = User.Identity.Name,
                IsDeleted = false,
            };


            await _context.Tags.AddAsync(tag);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            Tag? tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
            if (tag == null)
                throw new NotFoundException();

            UpdateTagVM updateTagVM = new()
            {
                Name = tag.Name
            };

            return View(updateTagVM);
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateTagVM updateTagVM, int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            if (!ModelState.IsValid)
                return View(updateTagVM);

            if (await _context.Tags.AnyAsync(t => t.Name == updateTagVM.Name && t.Id != id))
            {
                ModelState.AddModelError("Name", "Tag with this name already exists.");
                return View(updateTagVM);
            }

            Tag? tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
            if (tag == null)
                throw new NotFoundException();

            tag.Name = updateTagVM.Name;
            tag.UpdatedAt = DateTime.Now;
            tag.UpdatedBy = User.Identity.Name;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            Tag? tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
            if (tag == null)
                throw new NotFoundException();

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Archive(int? id)
        {
            if (id < 1 || id == null)
                throw new BadRequestException();

            Tag? tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == id);
            if (tag == null)
                throw new NotFoundException();

            if (tag.IsDeleted == true)
            {
                tag.IsDeleted = false;
            }
            else
            {
                tag.IsDeleted = true;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
