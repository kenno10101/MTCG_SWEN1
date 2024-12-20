using MTCG_Project.Exceptions;
using MTCG_Project.Interfaces;
using MTCG_Project.Models.User;
using Npgsql;

namespace MTCG_Project.Repository;

public class UserRepository : IUserRepository
{
    
    private readonly NpgsqlConnection _conn;

    public UserRepository(NpgsqlConnection conn)
    {
        _conn = conn;
    }
    
    public async Task Create (User user)
    {
        try
        {
            await using (var cmd = new NpgsqlCommand(
                             "INSERT INTO \"users\" (username, password, fullname, email) VALUES (@u, @pw, @f, @em)",
                             _conn))
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
    
    public async Task<User> Get (string username)
    {
        try
        {
            string user_name = null, password = null, fullname = null, email = null;
            await using var cmd = new NpgsqlCommand("SELECT * FROM \"users\" WHERE \"username\" = @u", _conn);
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
                throw new UserException("User not found or incomplete data.");
            }
            
            User user = new User(user_name, password, fullname, email);
            return user;
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
    
    public async Task Update (User user, string old_username)
    {
        await using (var cmd = new NpgsqlCommand("UPDATE \"users\" SET username = @u, password = @pw, fullname = @f, email = @em WHERE username = @user", _conn))
        {
            cmd.Parameters.AddWithValue("user", old_username);
            cmd.Parameters.AddWithValue("u", user.UserName);
            cmd.Parameters.AddWithValue("pw", user.Password);
            cmd.Parameters.AddWithValue("f", user.FullName);
            cmd.Parameters.AddWithValue("em", user.EMail);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}