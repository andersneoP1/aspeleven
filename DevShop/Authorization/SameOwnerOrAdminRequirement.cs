using Microsoft.AspNetCore.Authorization;

namespace DevShop.Authorization
{
    /// <summary>
    /// Требование авторизации: доступ разрешен только владельцу ресурса либо пользователю с ролью Admin.
    /// </summary>
    public class SameOwnerOrAdminRequirement : IAuthorizationRequirement
    {
    }
}

