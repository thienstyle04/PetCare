// PetCare.Repositories/IThuCungRepositories.cs (Phiên bản Đồng bộ hóa)
using PetCare.Models.Domain;
using PetCare.Models.DTO;
using System.Collections.Generic;
// Loại bỏ using System.Threading.Tasks;

namespace PetCare.Repositories
{
    public interface IThuCungRepositories
    {
        ThuCung AddPet(ThuCungDTO petDto);
        IEnumerable<ThuCung> GetAllPets( 
        string? filterOn = null, string? filterQuery = null,
        string? sortBy = null, bool isAscending = true,
        int pageNumber = 1, int pageSize = 10);
        ThuCung? GetPetById(int maThuCung);
        ThuCung? UpdatePet(int maThuCung, ThuCungDTO petDto);
        ThuCung? DeletePet(int maThuCung);
    }
}