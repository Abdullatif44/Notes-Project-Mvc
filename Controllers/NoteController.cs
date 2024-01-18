using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes.Data;
using Notes.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Hosting;

[Authorize]
public class NoteController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _webHostEnvironment;


    public NoteController(AppDbContext context, UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _userManager = userManager;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var notes = _context.Notes.Where(n => n.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier) && !n.IsDeleted).ToList();
        return View(notes);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Note/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("NoteMessage,ImageFile")] Note note)
    {
        if (ModelState.IsValid)
        {
            note.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var Images = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
            if (note.ImageFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(note.ImageFile.FileName);
                string FullPath = Path.Combine(Images, fileName);
                note.ImageFile.CopyTo(new FileStream(FullPath, FileMode.Create));
                note.Image = fileName;
            }

            _context.Add(note);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(note);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var note = await _context.Notes
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier) && !m.IsDeleted);

        if (note == null)
        {
            return NotFound();
        }

        return View(note);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Note note)
    {
        if (id != note.Id || !_context.Notes.Any(e => e.Id == id && e.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)))
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            if (note.ImageFile != null)
            {
                var Images = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
                if (note.ImageFile != null)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(note.ImageFile.FileName);
                    string FullPath = Path.Combine(Images, fileName);
                    note.ImageFile.CopyTo(new FileStream(FullPath, FileMode.Create));
                    note.Image = fileName;
                }
            }
            
            _context.Update(note);
            await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        return View(note);
    }


    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var note = await _context.Notes
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

        if (note != null)
        {
            note.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Json(new { success = true});
        }
        else
        {
            return Json(new { success = false});
        }
    }


    [AllowAnonymous]
    public IActionResult Share(int id)
    {

        var note = _context.Notes
                    .Include(n => n.User)  
                    .FirstOrDefault(m => m.Id == id && !m.IsDeleted && m.IsShared);

        if (note == null)
        {
            return NotFound();
        }

        return View(note);
    }

}
