using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes.Data;
using Notes.Models;
using Notes.Models.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Notes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoteApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public NoteApiController(AppDbContext context, UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var notes = _context.Notes.Where(n => n.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier) && !n.IsDeleted).ToList();
            return Ok(notes);
        }

        [HttpGet("{id}")]
        public IActionResult GetSharedNoteById(int id)
        {
            var note = _context.Notes
                .FirstOrDefault(m => m.Id == id && !m.IsDeleted && m.IsShared);

            if (note == null)
            {
                return NotFound();
            }

            return Ok(note);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] NoteCreateViewModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var note = new Note
            {
                NoteMessage = model.NoteMessage,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            var Images = Path.Combine(_webHostEnvironment.WebRootPath, "Images");

            if (model.ImageFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                string FullPath = Path.Combine(Images, fileName);
                using (var stream = new FileStream(FullPath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }
                note.Image = fileName;
            }

            _context.Add(note);
            await _context.SaveChangesAsync();

            return Ok(note);
        }


        [HttpPut("Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] NoteCreateViewModel model)
        {
            if (!_context.Notes.Any(e => e.Id == id && e.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var note = await _context.Notes.FindAsync(id);

            if (note == null)
            {
                return NotFound();
            }

            note.NoteMessage = model.NoteMessage;

            var Images = Path.Combine(_webHostEnvironment.WebRootPath, "Images");

            if (model.ImageFile != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                string FullPath = Path.Combine(Images, fileName);

                using (var stream = new FileStream(FullPath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                note.Image = fileName;
            }

            _context.Update(note);
            await _context.SaveChangesAsync();

            return Ok(note);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var note = await _context.Notes
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (note != null)
            {
                note.IsDeleted = true;
                await _context.SaveChangesAsync();

                return Ok(new { success = true });
            }
            else
            {
                return NotFound(new { success = false });
            }
        }
    }
}
