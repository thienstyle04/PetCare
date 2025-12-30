// PetCare.Models.DTO/ThuCungDTO.cs (Phiên bản tinh gọn)

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetCare.Models.DTO
{
    public class ThuCungDTO
    {
        public int MaThuCung { get; set; }
        [Required(ErrorMessage = "Mã Khách hàng là bắt buộc.")]
        public int MaKhachHang { get; set; }

        [Required(ErrorMessage = "Tên Thú cưng là bắt buộc.")]
        [MaxLength(50)]
        public string TenThuCung { get; set; } = string.Empty;

        [MaxLength(30)]
        public string? Loai { get; set; }

        [MaxLength(30)]
        public string? Giong { get; set; }

        [MaxLength(10)]
        public string? GioiTinh { get; set; }

        public DateTime? NgaySinh { get; set; }
        
        [Range(1900, 2100, ErrorMessage = "Năm sinh phải từ 1900 đến 2100.")]
        public int? NamSinh { get; set; }

        [MaxLength(100)]
        public string? ChuThich { get; set; }

        [Range(0, 50, ErrorMessage = "Tuổi phải từ 0 đến 50.")]
        public int? Tuoi { get; set; } = 0; // ✅ Giá trị mặc định = 0 (thú cưng mới sinh)

        // ✅ SỬA: Tăng lên decimal(7,2) để match với Domain Model
        [Column(TypeName = "decimal(7,2)")]
        [Range(0.01, 99999.99, ErrorMessage = "Cân nặng phải từ 0.01 đến 99999.99 kg.")]
        public decimal? CanNang { get; set; }
    }
}