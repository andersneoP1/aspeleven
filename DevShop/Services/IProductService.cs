using System.Threading.Tasks;
using DevShop.Common;
using DevShop.DTOs;

namespace DevShop.Services
{
    public interface IProductService
    {
        Task<ServiceResult<PagedResult<ProductReadDto>>> GetPagedProductsAsync(int page, int pageSize, int? categoryId = null, string? search = null);
        Task<ServiceResult<ProductReadDto>> GetProductByIdAsync(int id);
        Task<ServiceResult<ProductReadDto>> CreateProductAsync(ProductCreateDto dto);
        Task<ServiceResult<ProductReadDto>> UpdateProductAsync(int id, ProductUpdateDto dto);
        Task<ServiceResult<ProductReadDto>> PatchProductAsync(int id, PatchProductDTO dto);
        Task<ServiceResult<bool>> DeleteProductAsync(int id);
    }
}
