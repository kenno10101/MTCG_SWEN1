using MTCG_Project.Exceptions;
using MTCG_Project.Handler;
using MTCG_Project.Models.Users;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG_Project.Repositories
{
    public class SessionRepository
    {
        public static async Task Add (string rval, User user)
        {
            try
            {
                await using var conn = await DB_connection.connectDB();

                await using (var cmd = new NpgsqlCommand(
                    "INSERT INTO \"sessions\" (token, user_id)" +
                    "VALUES (@r, (SELECT users.id FROM users WHERE username = @u LIMIT 1))" +
                    "ON CONFLICT (user_id) DO UPDATE SET token = EXCLUDED.token"
                    , conn))
                {
                    cmd.Parameters.AddWithValue("r", rval);
                    cmd.Parameters.AddWithValue("u", user.UserName);
                    await cmd.ExecuteNonQueryAsync();
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<bool> HasSession (string username)
        {
            try
            {
                int rows = 0;
                
                await using var conn = await DB_connection.connectDB();
                await using var cmd = new NpgsqlCommand("SELECT * FROM \"sessions\" WHERE user_id = (SELECT id FROM users WHERE username = @u LIMIT 1) LIMIT 1", conn);
                cmd.Parameters.AddWithValue("u", username);
                
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        rows++;
                    }
                }

                if (rows != 1)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception (ex.Message);
            }
        }
    }
}
