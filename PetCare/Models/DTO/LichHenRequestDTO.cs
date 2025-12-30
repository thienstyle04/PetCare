using System.ComponentModel.DataAnnotations;

namespace PetCare.Models.DTO
{
    public class LichHenRequestDTO
    {
        [Required]
        public int MaKhachHang { get; set; }
        
        public int? MaNhanVien { get; set; } // ✅ Nullable vì khách hàng đặt lịch chưa có nhân viên

        [Required]
        public int MaThuCung { get; set; }

        [Required]
        public int MaDichVu { get; set; }

        [Required]
        public DateTime NgayGioHen { get; set; }

        public string? GhiChu { get; set; }
        public decimal? TongTien { get; set; }
        public string TrangThai { get; set; } = "Chờ xác nhận"; // ✅ Mặc định
    }
}
