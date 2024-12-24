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
            int coins = 0, elo = 0;
            
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
                    coins = reader.GetInt32(5);
                    elo = reader.GetInt32(6);
                }
            }

            if (user_name == null || password == null || fullname == null || email == null)
            {
                throw new UserException("Users not found or incomplete data.");
            }

            return new User(user_name, password, fullname, email, coins, elo, await CardRepository.GetDeck(username), await CardRepository.GetStack(username));
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
        bool usernameChange = user.UserName != null && old_username != user.UserName;
        bool passwordChange = user.Password != null;
        bool nameChange = user.FullName != null;
        bool emailChange = user.EMail != null;

        string usernameString = " username = @u";
        string passwordString = " password = @pw";
        string nameString = " fullname = @f";
        string emailString = " email = @em";

        string queryString = "UPDATE \"users\" SET";

        queryString += usernameChange ? usernameString : "";
        queryString += passwordChange ? passwordString : "";
        queryString += nameChange ? nameString : "";
        queryString += emailChange ? emailString : "";

        queryString += " WHERE username = @user";

        if (!usernameChange && !passwordChange && !nameChange && !emailChange)
        {
            throw new Exception("Error no parameters given.");
        }

        try
        {

            await using var conn = await DB_connection.connectDB();
            await using (var cmd = new NpgsqlCommand(queryString, conn))
            {
                cmd.Parameters.AddWithValue("user", old_username);
                
                if (usernameChange)
                {
                    cmd.Parameters.AddWithValue("u", user.UserName);
                }
                if (passwordChange)
                {
                    cmd.Parameters.AddWithValue("pw", user.Password);
                }
                if (nameChange)
                {
                    cmd.Parameters.AddWithValue("f", user.FullName);
                }
                if (emailChange)
                {
                    cmd.Parameters.AddWithValue("em", user.EMail);
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