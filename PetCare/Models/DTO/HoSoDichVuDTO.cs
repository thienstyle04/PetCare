using System.ComponentModel.DataAnnotations;

namespace PetCare.Models.DTO
{
    public class HoSoDichVuDTO
    {
        public int MaHoSo { get; set; }

        [Required(ErrorMessage = "Mã lịch hẹn là bắt buộc")]
        public int MaLichHen { get; set; }

        [Required(ErrorMessage = "Mã nhân viên là bắt buộc")]
        public int MaNhanVien { get; set; }

        [MaxLength(255, ErrorMessage = "Đường dẫn ảnh không vượt quá 255 ký tự")]
        public string? AnhTruoc { get; set; }

        [MaxLength(255, ErrorMessage = "Đường dẫn ảnh không vượt quá 255 ký tự")]
        public string? AnhSau { get; set; }

        [MaxLength(1000, ErrorMessage = "Ghi chú không vượt quá 1000 ký tự")]
        public string? GhiChuDichVu { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Thông tin bổ sung (nếu cần hiển thị)
        public string? TenNhanVien { get; set; }
        public DateTime? NgayHen { get; set; }
    }
}
