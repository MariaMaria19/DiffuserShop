using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffuserShop.Shared.Dtos
{
    // DTO это "конверты" и этот для отправления своего логина и пароля
    public class AuthRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

    }
}
