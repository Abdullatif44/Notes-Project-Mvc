using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notes.Models {

    public class Note
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Note message is required.")]
        [StringLength(255, ErrorMessage = "Note message must not exceed 255 characters.")]
        public string NoteMessage { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Image")]
        [DataType(DataType.ImageUrl)]
        public string? Image { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsShared { get; set; } = false;

        public string? UserId { get; set; }

        public User? User { get; set; }
    }
}