using System;
using DevShop.Models;

namespace DevShop.Services
{
    public interface IJwtTokenService
    {
        (string Token, DateTime Expiration) GenerateToken(User user);
    }
}

