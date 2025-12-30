using System.ComponentModel.DataAnnotations;

namespace PetCare.Models.DTO
{
    public class ImageUploadRequestDTO
    {
        [Required]
        public IFormFile File { get; set; }

        [Required]
        public string FileName { get; set; }

        public string? FileDescription { get; set; }
        public int? MaThuCung { get; set; }
    }
}
