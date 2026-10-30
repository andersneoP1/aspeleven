using System.Collections.Generic;
using System.Threading.Tasks;
using DevShop.Common;
using DevShop.DTOs;

namespace DevShop.Services
{
    public interface ICategoryService
    {
        Task<ServiceResult<IEnumerable<CategoryReadDto>>> GetAllCategoriesAsync();
        Task<ServiceResult<CategoryReadDto>> GetCategoryByIdAsync(int id);
        Task<ServiceResult<CategoryReadDto>> CreateCategoryAsync(CategoryCreateDto dto);
        Task<ServiceResult<CategoryReadDto>> UpdateCategoryAsync(int id, CategoryUpdateDto dto);
        Task<ServiceResult<CategoryReadDto>> PatchCategoryAsync(int id, PatchCategoryDTO dto);
        Task<ServiceResult<bool>> DeleteCategoryAsync(int id);
    }
}
