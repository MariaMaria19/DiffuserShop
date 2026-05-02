using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffuserShop.Shared.Dtos
{
    // а этот для ответа верный ли пароль 
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

    }
}
