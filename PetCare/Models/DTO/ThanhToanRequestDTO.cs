using System.ComponentModel.DataAnnotations;

namespace PetCare.Models.DTO
{
    public class ThanhToanRequestDTO
    {
        [Required]
        public int MaLichHen { get; set; }

        [Required]
        public decimal SoTien { get; set; }

        [Required]
        public string PhuongThuc { get; set; } // TienMat, TheNganHang, ChuyenKhoan

        public string? GhiChu { get; set; }
    }
}
