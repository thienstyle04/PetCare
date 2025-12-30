using PetCare.Data;
using PetCare.Models.Domain;
using PetCare.Models.DTO;
using System.Globalization;

namespace PetCare.Repositories
{
    public class DichVuRepositories : IDichVuRepositories
    {
        private readonly AppDbContext _dbContext;
        public DichVuRepositories(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public DichVuDTO addichvu(DichVuDTO dto)
        {
            var entity = new DichVu
            {
                TenDichVu = dto.TenDichVu,
                MoTa = dto.MoTa,
                Gia = dto.Gia,
                ThoiGian = null,
                TrangThai = true,
                NgayTao = DateTime.Now
            };

            _dbContext.DichVu.Add(entity);
            _dbContext.SaveChanges();

            dto.MaDichVu = entity.MaDichVu;
            return dto;
        }

        public List<DichVuDTO> AllDichVu(string? filterOn = null,
            string? filterQuery = null, string? sortBy = null, bool isAscending = true,
            int pageNumber = 1, int pageSize = 100)
        {
            var allDichVu = _dbContext.DichVu.Select(n => new DichVuDTO
            {
                MaDichVu = n.MaDichVu,
                TenDichVu = n.TenDichVu,
                MoTa = n.MoTa,
                Gia = n.Gia,
                ThoiGian = n.ThoiGian
            }).AsQueryable();
            // FILTERING (Lọc theo TenDichVu)
            if (string.IsNullOrWhiteSpace(filterOn) == false &&
                string.IsNullOrWhiteSpace(filterQuery) == false)
            {
                if (filterOn.Equals("tendichvu", StringComparison.OrdinalIgnoreCase))
                {
                    allDichVu = allDichVu.Where(x => x.TenDichVu.Contains(filterQuery));
                }
            }
            //sorting 
            if (string.IsNullOrWhiteSpace(sortBy) == false)
            {
                if (sortBy.Equals("title", StringComparison.OrdinalIgnoreCase))
                {
                    allDichVu = isAscending 
                        ? allDichVu.OrderBy(x => x.TenDichVu) 
                        : allDichVu.OrderByDescending(x => x.TenDichVu);
                }
            }
            // Pagination
            var skipResults = (pageNumber - 1) * pageSize;
            return allDichVu.Skip(skipResults).Take(pageSize).ToList();

        }

        public DichVu deletedichvu(int MaDichVu)
        {
            var alldichvu = _dbContext.DichVu.FirstOrDefault(n => n.MaDichVu == MaDichVu);
            if (alldichvu != null)
            {
                // Kiểm tra xem có lịch hẹn nào đang sử dụng dịch vụ này không
                var hasLichHen = _dbContext.LichHen.Any(lh => lh.MaDichVu == MaDichVu);
                if (hasLichHen)
                {
                    throw new InvalidOperationException($"Không thể xóa dịch vụ '{alldichvu.TenDichVu}' vì đang có lịch hẹn sử dụng dịch vụ này. Vui lòng xóa các lịch hẹn liên quan trước.");
                }

                _dbContext.DichVu.Remove(alldichvu);
                _dbContext.SaveChanges();
            }
            return alldichvu;
        }

        public DichVuDTO? GetDichVuById(int MaDichVU)
        {
            var entity = _dbContext.DichVu.Where(n => n.MaDichVu == MaDichVU);
           
            var dichvus = entity.Select(dichVu => new DichVuDTO ()
                {
                MaDichVu = dichVu.MaDichVu,
                TenDichVu = dichVu.TenDichVu,
                MoTa = dichVu.MoTa,
                Gia = dichVu.Gia
            }).FirstOrDefault();
            if (entity == null)
            {
                return null;
            }
            return dichvus;
        }

        public List<DichVuComBoDTO> GetDichVuComBoById(int MaDichVu)
        {
            var dichvuchinh = _dbContext.DichVu.Where(n => n.MaDichVu == MaDichVu).FirstOrDefault();
            if (dichvuchinh == null)
            {
                return new List<DichVuComBoDTO>();
            }
            var random = new Random();
            var dichvukhac = _dbContext.DichVu
                .Where(n => n.MaDichVu != MaDichVu && n.TrangThai == true)
                .OrderBy(x => random.Next())
                .Take(3)
                .ToList();
            var combo = new List<DichVu> { dichvuchinh };
            combo.AddRange(dichvukhac);
            return combo.Select(n=> new DichVuComBoDTO()
                { 
                MaDichVu = n.MaDichVu,
                TenDichVu = n.TenDichVu,
                Gia = n.Gia,
                ThoiGian = n.ThoiGian
            }).ToList();
        }

        public List<DichVUPriceDTO> GetDichVuPriceById(int MaDichVu)
        {
            var entity = _dbContext.DichVu.Where(n => n.MaDichVu == MaDichVu);
            var priceDichVu = entity.Select(dichvu => new DichVUPriceDTO()
            {
                TenDichVu = dichvu.TenDichVu,
                Gia = dichvu.Gia,
            }).ToList();
            if (entity == null)
            {
                return null;
            }
            return priceDichVu;
        }

        public DichVuDTO updatedichvu(int MaDichVu, DichVuDTO dichVu)
        {
            var entity = _dbContext.DichVu.Where(n => n.MaDichVu == MaDichVu);
            if (entity != null)
            { 
                entity.ToList().ForEach(n =>
                {
                    n.TenDichVu = dichVu.TenDichVu;
                    n.MoTa = dichVu.MoTa;
                    n.Gia = dichVu.Gia;
                });
                _dbContext.SaveChanges();
                return new DichVuDTO
                {
                    MaDichVu = MaDichVu,
                    TenDichVu = dichVu.TenDichVu,
                    MoTa = dichVu.MoTa,
                    Gia = dichVu.Gia
                };
            }
            return null;
        }
    }
}
