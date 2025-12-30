using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetCare.Models.Domain;

namespace PetCare.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Các bảng trong hệ thống
        public DbSet<NguoiDung> NguoiDung { get; set; }
        public DbSet<KhachHang> KhachHang { get; set; }
        public DbSet<NhanVien> NhanVien { get; set; }
        public DbSet<ThuCung> ThuCung { get; set; }
        public DbSet<DichVu> DichVu { get; set; }
        public DbSet<LichHen> LichHen { get; set; }
        public DbSet<HoSoDichVu> HoSoDichVu { get; set; }
        public DbSet<ThanhToan> ThanhToan { get; set; }
        public DbSet<DanhGia> DanhGia { get; set; }
        public DbSet<Image> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==============================
            // 1️⃣ Liên kết IdentityUser ↔ NguoiDung
            // ==============================
            modelBuilder.Entity<NguoiDung>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(n => n.IdentityUserId)
                .OnDelete(DeleteBehavior.Cascade); // 🔥 Xóa IdentityUser → xóa luôn NguoiDung

            // ==============================
            // 2️⃣ NguoiDung ↔ KhachHang (1:1)
            // ==============================
            modelBuilder.Entity<KhachHang>()
                .HasOne(k => k.NguoiDung)
                .WithOne(n => n.KhachHang)
                .HasForeignKey<KhachHang>(k => k.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade); // 🔥 Xóa NguoiDung → xóa luôn KhachHang

            // ==============================
            // 3️⃣ NguoiDung ↔ NhanVien (1:1)
            // ==============================
            modelBuilder.Entity<NhanVien>()
                .HasOne(nv => nv.NguoiDung)
                .WithOne(n => n.NhanVien)
                .HasForeignKey<NhanVien>(nv => nv.MaNguoiDung)
                .OnDelete(DeleteBehavior.Cascade);

            // ==============================
            // 4️⃣ KhachHang ↔ ThuCung (1:N)
            // ==============================
            modelBuilder.Entity<ThuCung>()
                .HasOne(t => t.KhachHang)
                .WithMany(k => k.ThuCung)
                .HasForeignKey(t => t.MaKhachHang)
                .OnDelete(DeleteBehavior.Cascade);

            // ==============================
            // 5️⃣ LichHen ↔ KhachHang, ThuCung, DichVu, NhanVien
            // ==============================
            modelBuilder.Entity<LichHen>()
                .HasOne(l => l.KhachHang)
                .WithMany(k => k.LichHen)
                .HasForeignKey(l => l.MaKhachHang)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LichHen>()
                .HasOne(l => l.ThuCung)
                .WithMany(t => t.LichHen)
                .HasForeignKey(l => l.MaThuCung)
                .OnDelete(DeleteBehavior.Restrict); // tránh vòng lặp xóa

            modelBuilder.Entity<LichHen>()
                .HasOne(l => l.DichVu)
                .WithMany(d => d.LichHen)
                .HasForeignKey(l => l.MaDichVu)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LichHen>()
                .HasOne(l => l.NhanVien)
                .WithMany(nv => nv.LichHen)
                .HasForeignKey(l => l.MaNhanVien)
                .OnDelete(DeleteBehavior.Restrict);

            // ==============================
            // 6️⃣ HoSoDichVu ↔ LichHen, NhanVien
            // ==============================
            modelBuilder.Entity<HoSoDichVu>()
                .HasOne(h => h.LichHen)
                .WithMany(l => l.HoSoDichVu)
                .HasForeignKey(h => h.MaLichHen)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HoSoDichVu>()
                .HasOne(h => h.NhanVien)
                .WithMany(nv => nv.HoSoDichVu)
                .HasForeignKey(h => h.MaNhanVien)
                .OnDelete(DeleteBehavior.Restrict);

            // ==============================
            // 7️⃣ DanhGia ↔ LichHen, KhachHang
            // ==============================
            modelBuilder.Entity<DanhGia>()
                .HasOne(dg => dg.LichHen)
                .WithMany(l => l.DanhGia)
                .HasForeignKey(dg => dg.MaLichHen)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DanhGia>()
                .HasOne(dg => dg.KhachHang)
                .WithMany(k => k.DanhGia)
                .HasForeignKey(dg => dg.MaKhachHang)
                .OnDelete(DeleteBehavior.Cascade);

            // ==============================
            // 8️⃣ Image ↔ ThuCung (N:1)
            // ==============================
            modelBuilder.Entity<Image>()
                .HasOne(i => i.ThuCung)
                .WithMany()
                .HasForeignKey(i => i.MaThuCung)
                .OnDelete(DeleteBehavior.SetNull);

            // ==============================
            // 9️⃣ ThanhToan ↔ LichHen
            // ==============================
            modelBuilder.Entity<ThanhToan>()
                .HasOne(t => t.LichHen)
                .WithMany(l => l.ThanhToan)
                .HasForeignKey(t => t.MaLichHen)
                .OnDelete(DeleteBehavior.Cascade);

            
        }
    }
}
