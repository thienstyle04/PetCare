using System.ComponentModel.DataAnnotations;

namespace PetCare.Models.DTO
{
    public class UpdateStatusDTO
    {
        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        public string TrangThai { get; set; } = string.Empty;
    }
}
