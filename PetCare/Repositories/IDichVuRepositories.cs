using PetCare.Models.Domain;
using PetCare.Models.DTO;

namespace PetCare.Repositories
{
    public interface IDichVuRepositories
    {
        List<DichVuDTO> AllDichVu(string? filterOn = null, string? filterQuery = null,string? sortBy = null, bool isAscending = true, int pageNumber = 1, int pageSize = 100);
        DichVuDTO? GetDichVuById(int MaDichVU);
        List<DichVUPriceDTO> GetDichVuPriceById(int MaDichVu);
        List<DichVuComBoDTO> GetDichVuComBoById(int MaDichVu);
        DichVuDTO addichvu(DichVuDTO dichVu);
        DichVuDTO updatedichvu(int MaDichVu,DichVuDTO dichVu);
        DichVu deletedichvu(int MaDichVu);
    }
}
