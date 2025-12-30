namespace Petcare_web.Models.DTO
{
    public class NguoiDungResponseDTO
    {
        public string JwtToken { get; set; }
        public List<string> Roles { get; set; }
        public int MaNguoiDung { get; set; }
    }
    public class NguoiDungDTO
    {
        public int MaNguoiDung { get; set; }
        public string IdentityUserId { get; set; }
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
        public string Email { get; set; }
        public string MatKhauHash { get; set; }
        public string VaiTro { get; set; }
        public bool TrangThai { get; set; } = true;
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime NgayCapNhat { get; set; } = DateTime.Now;

    }
}
