using PetCare.Data;
using PetCare.Models.Domain;
using PetCare.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace PetCare.Repositories
{
    public class HoSoDichVuRepositories : IHoSoDichVuRepositories
    {
        private readonly AppDbContext _dbContext;

        public HoSoDichVuRepositories(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<HoSoDichVuDTO> AllHoSoDichVu(string? filterOn = null,
            string? filterQuery = null, string? sortBy = null, bool isAscending = true,
            int pageNumber = 1, int pageSize = 100)
        {
            var query = _dbContext.HoSoDichVu
                .Include(x => x.NhanVien)
                .Include(x => x.LichHen)
                .Select(n => new HoSoDichVuDTO
                {
                    MaHoSo = n.MaHoSo,
                    MaLichHen = n.MaLichHen,
                    MaNhanVien = n.MaNhanVien,
                    AnhTruoc = n.AnhTruoc,
                    AnhSau = n.AnhSau,
                    GhiChuDichVu = n.GhiChuDichVu,
                    NgayTao = n.NgayTao,
                    TenNhanVien = n.NhanVien.HoTen,
                    NgayHen = n.LichHen.NgayGioHen
                }).AsQueryable();

            // Filtering
            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                if (filterOn.Equals("tennhanvien", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(x => x.TenNhanVien.Contains(filterQuery));
                }
            }

            // Sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.Equals("ngaytao", StringComparison.OrdinalIgnoreCase))
                {
                    query = isAscending ? query.OrderBy(x => x.NgayTao) : query.OrderByDescending(x => x.NgayTao);
                }
            }

            // Pagination
            var skipResults = (pageNumber - 1) * pageSize;
            return query.Skip(skipResults).Take(pageSize).ToList();
        }

        public HoSoDichVuDTO AddHoSoDichVu(HoSoDichVuDTO dto)
        {
            var entity = new HoSoDichVu
            {
                MaLichHen = dto.MaLichHen,
                MaNhanVien = dto.MaNhanVien,
                AnhTruoc = dto.AnhTruoc,
                AnhSau = dto.AnhSau,
                GhiChuDichVu = dto.GhiChuDichVu,
                NgayTao = DateTime.Now
            };

            _dbContext.HoSoDichVu.Add(entity);
            _dbContext.SaveChanges();

            dto.MaHoSo = entity.MaHoSo;
            return dto;
        }

        public HoSoDichVuDTO? GetHoSoDichVuById(int MaHoSo)
        {
            var entity = _dbContext.HoSoDichVu
                .Include(n => n.NhanVien)
                .Include(n => n.LichHen)
                .FirstOrDefault(n => n.MaHoSo == MaHoSo);

            if (entity == null) return null;

            return new HoSoDichVuDTO
            {
                MaHoSo = entity.MaHoSo,
                MaLichHen = entity.MaLichHen,
                MaNhanVien = entity.MaNhanVien,
                AnhTruoc = entity.AnhTruoc,
                AnhSau = entity.AnhSau,
                GhiChuDichVu = entity.GhiChuDichVu,
                NgayTao = entity.NgayTao,
                TenNhanVien = entity.NhanVien?.HoTen,
                NgayHen = entity.LichHen?.NgayGioHen
            };
        }

        public HoSoDichVuDTO? UpdateHoSoDichVu(int MaHoSo, HoSoDichVuDTO dto)
        {
            var entity = _dbContext.HoSoDichVu.FirstOrDefault(n => n.MaHoSo == MaHoSo);
            if (entity == null) return null;

            entity.MaLichHen = dto.MaLichHen;
            entity.MaNhanVien = dto.MaNhanVien;
            entity.AnhTruoc = dto.AnhTruoc;
            entity.AnhSau = dto.AnhSau;
            entity.GhiChuDichVu = dto.GhiChuDichVu;
            _dbContext.SaveChanges();

            return dto;
        }

        public HoSoDichVu? DeleteHoSoDichVu(int MaHoSo)
        {
            var entity = _dbContext.HoSoDichVu.FirstOrDefault(n => n.MaHoSo == MaHoSo);
            if (entity != null)
            {
                _dbContext.HoSoDichVu.Remove(entity);
                _dbContext.SaveChanges();
            }
            return entity;
        }
    }
}
