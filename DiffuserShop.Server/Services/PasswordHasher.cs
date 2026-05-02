using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DiffuserShop.Server.Services
{
    public class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLower();
        }
    }
}

// "пароли хранятся в виде хеша"
// то есть этот класс превращает пароль в рандомный набор символов