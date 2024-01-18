using System.ComponentModel.DataAnnotations;

namespace Notes.Models.ViewModels
{
    public class NoteCreateViewModel
    {
        [Required]
        public string NoteMessage { get; set; }

        // Adjust the property type as needed
        public IFormFile? ImageFile { get; set; }
    }
}
