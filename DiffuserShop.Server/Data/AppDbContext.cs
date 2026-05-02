using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiffuserShop.Server.Services;
using DiffuserShop.Shared.Models;
using Microsoft.EntityFrameworkCore;


// это якобы мост между програмой и бд

namespace DiffuserShop.Server.Data
{
    public class AppDbContext : DbContext
    {
        // отсылка к нашим таблицам из бд
        public DbSet<Diffuser> Diffusers { get; set; }
        public DbSet<User> Users { get; set; }

        // указываем где будем хранить файл бд
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=diffusers.db");
        }


        // по заданию создаем тесотвого пользователя чтобы проверить авторизацию
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            string hashedPassword = PasswordHasher.HashPassword("admin123");

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = hashedPassword,
                    Role = "admin"
                }
            );
        }
    }
}
