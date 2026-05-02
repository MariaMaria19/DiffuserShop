using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DiffuserShop.Server.Data;
using DiffuserShop.Server.Services;
using DiffuserShop.Shared.Dtos;
using DiffuserShop.Shared.Models;
using Microsoft.Data.Sqlite;

// написание TCP сервера ( инструкция для продавца)
// 1. Слушать порт 8888
// 2. Принимать команды от клиента
// 3. Что делать на складе (искать, добавлять, удалять)

namespace DiffuserShop.Server
{
    class Program
    {
        private static AppDbContext _db = new AppDbContext();

        // Запуск сервера продавцом 
        static void Main(string[] args)
        {
            // Создаём базу данных, если её нет
            _db.Database.EnsureCreated();

            // тестовые товары
            if (!_db.Diffusers.Any())
            {
                _db.Diffusers.Add(new Diffuser { Name = "Элегант", Scent = "Лаванда", Price = 1290, InStock = 15 });
                _db.Diffusers.Add(new Diffuser { Name = "Морской бриз", Scent = "Океан", Price = 890, InStock = 23 });
                _db.Diffusers.Add(new Diffuser { Name = "Лесной орех", Scent = "Древесный", Price = 1590, InStock = 8 });
                _db.SaveChanges();
                Console.WriteLine("Добавлены тестовые товары!");
            }

            var server = new TcpListener(IPAddress.Any, 8888);
            server.Start();

            Console.WriteLine("=== СЕРВЕР ЗАПУЩЕН ===");
            Console.WriteLine("Порт: 8888");
            Console.WriteLine("Ожидание подключений...\n");

            while (true)
            {
                var client = server.AcceptTcpClient();
                Console.WriteLine($"[+] Подключился клиент");

                var thread = new Thread(() => HandleClient(client));
                thread.Start();
            }
        }

        // Обслуживание одного покупателя
        static void HandleClient(TcpClient client)
        {
            using var stream = client.GetStream();
            var buffer = new byte[4096];
            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            var request = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Console.WriteLine($"Команда: {request}");

            var response = ProcessRequest(request);
            var responseBytes = Encoding.UTF8.GetBytes(response);
            stream.Write(responseBytes, 0, responseBytes.Length);

            client.Close();
            Console.WriteLine($"[-] Клиент отключился\n");
        }

        // Что хотел от нас покупатель 
        static string ProcessRequest(string request)
        {
            var parts = request.Split('|');
            var command = parts[0];
            var data = parts.Length > 1 ? parts[1] : "";

            switch (command)
            {
                case "AUTH": return Authenticate(data);
                case "GET_ALL": return GetAllDiffusers();
                case "ADD_ORM": return AddDiffuserOrm(data);
                case "ADD_SQL": return AddDiffuserSql(data);
                case "UPDATE_ORM": return UpdateDiffuserOrm(data);
                case "UPDATE_SQL": return UpdateDiffuserSql(data);
                case "DELETE_ORM": return DeleteDiffuserOrm(data);
                case "DELETE_SQL": return DeleteDiffuserSql(data);
                case "SEARCH_ORM": return SearchDiffusersOrm(data);
                case "SEARCH_SQL": return SearchDiffusersSql(data);
                default: return "ERROR|Неизвестная команда"; 
            }
        }

        // Авторизация 
        static string Authenticate(string data)
        {
            if (data.StartsWith("{"))
                data = data.Substring(1);

            var authReq = JsonSerializer.Deserialize<AuthRequest>(data);

            if (authReq == null)
            {
                return "AUTH_FAILED";
            }

            // Ищем пользователя по логину
            var user = _db.Users.FirstOrDefault(u => u.Username == authReq.Username);

            // Проверяем, что пользователь найден и пароль совпадает
            if (user != null && user.PasswordHash == PasswordHasher.HashPassword(authReq.Password))
            {
                var response = new AuthResponse { Success = true, Message = "OK", Role = user.Role };
                return $"AUTH_SUCCESS|{JsonSerializer.Serialize(response)}";
            }

            return "AUTH_FAILED";
        }

        // Работа со складом (БД) 
        static string GetAllDiffusers()
        {
            var diffusers = _db.Diffusers.ToList();
            return $"DATA|{JsonSerializer.Serialize(diffusers)}";
        }

        // кнопка ДОБАВИТЬ ТОВАР 
        static string AddDiffuserOrm(string data)
        {
            var diffuser = JsonSerializer.Deserialize<Diffuser>(data);
            _db.Diffusers.Add(diffuser!);
            _db.SaveChanges();
            return "SUCCESS|Добавлено через ORM";
        }

        static string AddDiffuserSql(string data)
        {
            var diffuser = JsonSerializer.Deserialize<Diffuser>(data);
            using var connection = new SqliteConnection("Data Source=diffusers.db");
            connection.Open();
            var command = new SqliteCommand(
                "INSERT INTO Diffusers (Name, Scent, Price, InStock) VALUES (@name, @scent, @price, @stock)",
                connection);
            command.Parameters.AddWithValue("@name", diffuser!.Name);
            command.Parameters.AddWithValue("@scent", diffuser.Scent);
            command.Parameters.AddWithValue("@price", diffuser.Price);
            command.Parameters.AddWithValue("@stock", diffuser.InStock);
            command.ExecuteNonQuery();
            return "SUCCESS|Добавлено через SQL";
        }

        // кнопка ОБНОВИТЬ ТОВАР 
        static string UpdateDiffuserOrm(string data)
        {
            var diffuser = JsonSerializer.Deserialize<Diffuser>(data);
            _db.Diffusers.Update(diffuser!);
            _db.SaveChanges();
            return "SUCCESS|Обновлено через ORM";
        }

        static string UpdateDiffuserSql(string data)
        {
            var diffuser = JsonSerializer.Deserialize<Diffuser>(data);
            using var connection = new SqliteConnection("Data Source=diffusers.db");
            connection.Open();
            var command = new SqliteCommand(
                "UPDATE Diffusers SET Name=@name, Scent=@scent, Price=@price, InStock=@stock WHERE Id=@id",
                connection);
            command.Parameters.AddWithValue("@id", diffuser!.Id);
            command.Parameters.AddWithValue("@name", diffuser.Name);
            command.Parameters.AddWithValue("@scent", diffuser.Scent);
            command.Parameters.AddWithValue("@price", diffuser.Price);
            command.Parameters.AddWithValue("@stock", diffuser.InStock);
            command.ExecuteNonQuery();
            return "SUCCESS|Обновлено через SQL";
        }

        // кнопка УДАЛИТЬ ТОВАР 
        static string DeleteDiffuserOrm(string id)
        {
            var diffuser = _db.Diffusers.Find(int.Parse(id));
            if (diffuser != null)
            {
                _db.Diffusers.Remove(diffuser);
                _db.SaveChanges();
            }
            return "SUCCESS|Удалено через ORM";
        }

        static string DeleteDiffuserSql(string id)
        {
            using var connection = new SqliteConnection("Data Source=diffusers.db");
            connection.Open();
            var command = new SqliteCommand("DELETE FROM Diffusers WHERE Id=@id", connection);
            command.Parameters.AddWithValue("@id", int.Parse(id));
            command.ExecuteNonQuery();
            return "SUCCESS|Удалено через SQL";
        }

        // кнопка ПОИСК 
        static string SearchDiffusersOrm(string searchTerm)
        {
            string term = searchTerm ?? "";
            var results = _db.Diffusers
                .Where(d => d.Name.Contains(term) || d.Scent.Contains(term))
                .ToList();
            return $"DATA|{JsonSerializer.Serialize(results)}";
        }

        static string SearchDiffusersSql(string searchTerm)
        {
            using var connection = new SqliteConnection("Data Source=diffusers.db");
            connection.Open();
            var command = new SqliteCommand(
                "SELECT * FROM Diffusers WHERE Name LIKE @search OR Scent LIKE @search",
                connection);
            command.Parameters.AddWithValue("@search", $"%{searchTerm}%");
            using var reader = command.ExecuteReader();
            var results = new List<Diffuser>();
            while (reader.Read())
            {
                results.Add(new Diffuser
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Scent = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    InStock = reader.GetInt32(4)
                });
            }
            return $"DATA|{JsonSerializer.Serialize(results)}";
        }
    }
}