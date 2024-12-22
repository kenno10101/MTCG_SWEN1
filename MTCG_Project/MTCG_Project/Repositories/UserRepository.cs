using MTCG_Project.Exceptions;
using MTCG_Project.Handler;
using MTCG_Project.Interfaces;
using MTCG_Project.Models.Users;
using Npgsql;

namespace MTCG_Project.Repositories;

public class UserRepository
{
    public static async Task Create(User user)
    {
        try
        {
            await using var conn = await DB_connection.connectDB();
            await using (var cmd = new NpgsqlCommand(
                             "INSERT INTO \"users\" (username, password, fullname, email) VALUES (@u, @pw, @f, @em)",
                             conn))
            {
                cmd.Parameters.AddWithValue("u", user.UserName);
                cmd.Parameters.AddWithValue("pw", user.Password);
                cmd.Parameters.AddWithValue("f", user.FullName);
                cmd.Parameters.AddWithValue("em", user.EMail);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            if (ex.Message.Contains("username", StringComparison.OrdinalIgnoreCase))
            {
                throw new UserException("A user with this username already exists.");
            }
            else if (ex.Message.Contains("email", StringComparison.OrdinalIgnoreCase))
            {
                throw new UserException("A user with this email already exists.");
            }
            else
            {
                throw new UserException("An error with the DB has occurred.");
            }
        }
    }

    public static async Task<User> Get(string username)
    {
        
        try
        {
            string user_name = null, password = null, fullname = null, email = null;
            
            await using var conn = await DB_connection.connectDB();
            await using var cmd = new NpgsqlCommand("SELECT * FROM \"users\" WHERE \"username\" = @u", conn);
            cmd.Parameters.AddWithValue("u", username);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    user_name = reader.GetString(1);
                    password = reader.GetString(2);
                    fullname = reader.GetString(3);
                    email = reader.GetString(4);
                }
            }

            if (user_name == null || password == null || fullname == null || email == null)
            {
                throw new UserException("Users not found or incomplete data.");
            }

            return new User(user_name, password, fullname, email);
        }
        catch (PostgresException ex)
        {
            throw new UserException("Error retrieving data from database");
        }
        catch (UserException ex)
        {
            throw new UserException(ex.Message);
        }
    }

    public static async Task Update(User user, string old_username)
    {
        try
        {
            bool usernameChange = old_username != user.UserName;
            
            await using var conn = await DB_connection.connectDB();
            string queryString = usernameChange
                ? "UPDATE \"users\" SET username = @u, password = @pw, fullname = @f, email = @em WHERE username = @user"
                : "UPDATE \"users\" SET password = @pw, fullname = @f, email = @em WHERE username = @user";

            await using (var cmd = new NpgsqlCommand(
                             queryString,
                             conn))
            {
                cmd.Parameters.AddWithValue("user", old_username);
                cmd.Parameters.AddWithValue("pw", user.Password);
                cmd.Parameters.AddWithValue("f", user.FullName);
                cmd.Parameters.AddWithValue("em", user.EMail);

                if (usernameChange)
                {
                    cmd.Parameters.AddWithValue("u", user.UserName);
                }

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    throw new UserException("The user with the specified username does not exist.");
                }
            }
        }
        catch (PostgresException ex)
        {
            if (ex.SqlState != "23505")
            {
                throw new UserException("Error updating Users in DB.");
            }

            if (ex.Message.Contains("username", StringComparison.OrdinalIgnoreCase))
            {
                throw new UserException("Error updating, user with this username already exists.");
            }
            else if (ex.Message.Contains("email", StringComparison.OrdinalIgnoreCase))
            {
                throw new UserException("Error updating, user with this email already exists.");
            }
        }
    }
}