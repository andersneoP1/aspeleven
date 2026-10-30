using System.Threading.Tasks;
using DevShop.Common;
using DevShop.DTOs;

namespace DevShop.Services
{
    public interface IAuthService
    {
        Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto);
        Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto);
    }
}

