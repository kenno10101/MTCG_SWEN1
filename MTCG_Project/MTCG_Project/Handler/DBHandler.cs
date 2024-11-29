using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG_Project.Handler
{
    public class DBHandler
    {
        public static async void connectDB()
        {
            var connString = "Host=localhost;Username=kenn;Password=kenn1234;Database=mtcg_swen1";

            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            // Insert some data
            //await using (var cmd = new NpgsqlCommand("INSERT INTO \"user\" (id, username, password, fullname, email) VALUES (@id, @u, @pw, @f, @em)", conn))
            //{
            //    cmd.Parameters.AddWithValue("id", 3);
            //    cmd.Parameters.AddWithValue("u", "test");
            //    cmd.Parameters.AddWithValue("pw", "11111111");
            //    cmd.Parameters.AddWithValue("f", "test test");
            //    cmd.Parameters.AddWithValue("em", "test@test.at");
            //    await cmd.ExecuteNonQueryAsync();
            //}

            // Retrieve all rows
            await using (var cmd = new NpgsqlCommand("SELECT * FROM \"user\"", conn))
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int id = reader.GetInt32(0);
                    string username = reader.GetString(1);
                    string password= reader.GetString(2);
                    string fullname = reader.GetString(3);
                    string email = reader.GetString(4);
                    Console.WriteLine($"ID: {id}, Username: {username}, Password: {password}, Fullname: {fullname}, Email: {email}");
                }
                    
            }
        }
    }
}
