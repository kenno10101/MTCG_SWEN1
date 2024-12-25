using MTCG_Project.Exceptions;
using MTCG_Project.Handler;
using MTCG_Project.Interfaces;
using MTCG_Project.Models.Stats;
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
            int coins = 0;

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
                }
            }

            if (user_name == null || password == null || fullname == null || email == null)
            {
                throw new UserException("Users not found or incomplete data.");
            }

            return new User(user_name, password, fullname, email, coins, await CardRepository.GetDeck(username),
                await CardRepository.GetStack(username), await GetStats(username));
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

    public static async Task CreateStats(string username)
    {
        try
        {
            await using var conn = await DB_connection.connectDB();
            await using (var cmd = new NpgsqlCommand(
                             "INSERT INTO \"stats\" (username, battles_played, wins, losses, draws, elo) VALUES (@u, @b, @w, @l, @d, @e)",
                             conn))
            {
                cmd.Parameters.AddWithValue("u", username);
                cmd.Parameters.AddWithValue("b", 0);
                cmd.Parameters.AddWithValue("w", 0);
                cmd.Parameters.AddWithValue("l", 0);
                cmd.Parameters.AddWithValue("d", 0);
                cmd.Parameters.AddWithValue("e", 100);

                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public static async Task<Stat> GetStats(string username)
    {
        try
        {
            await using var conn = await DB_connection.connectDB();
            await using var cmd = new NpgsqlCommand(
                "SELECT battles_played, wins, losses, draws, elo FROM \"stats\" WHERE username = @u",
                conn);
            cmd.Parameters.AddWithValue("u", username);

            string user = null;
            int battles_played = 0, wins = 0, losses = 0, draws = 0, elo = 0;
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    battles_played = reader.GetInt32(0);
                    wins = reader.GetInt32(1);
                    losses = reader.GetInt32(2);
                    draws = reader.GetInt32(3);
                    elo = reader.GetInt32(4);
                }
            }

            return new Stat(battles_played, wins, losses, draws, elo);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public static async Task UpdateStats(string username1, string username2, string result)
    {
        string queryString = string.Empty;
        switch (result)
        {
            case ("win"):
                queryString = @"
UPDATE ""stats"" SET
battles_played = battles_played + 1,
wins = wins + 1,
elo = elo + 5
";
                break;
            case ("loss"):
                queryString = @"
UPDATE ""stats"" SET
battles_played = battles_played + 1,
losses = losses + 1,
elo = elo - 3
";
                break;
            case ("draw"):
                queryString = @"
UPDATE ""stats"" SET
battles_played = battles_played + 1,
draws = draws + 1,
";
                break;
            default:
                break;
        }

        queryString += " WHERE username = @user1;";

        switch (result)
        {
            case ("win"):
                queryString += @"
UPDATE ""stats"" SET
battles_played = battles_played + 1,
losses = losses + 1,
elo = elo - 3
";
                break;
            case ("loss"):
                queryString += @"
UPDATE ""stats"" SET
battles_played = battles_played + 1,
wins = wins + 1,
elo = elo + 5
";
                break;
            case ("draw"):
                queryString += @"
UPDATE ""stats"" SET
battles_played = battles_played + 1,
draws = draws + 1,
";
                break;
            default:
                break;
        }

        queryString += " WHERE username = @user2;";

        try
        {
            await using var conn = await DB_connection.connectDB();
            await using (var cmd = new NpgsqlCommand(queryString, conn))
            {
                cmd.Parameters.AddWithValue("user1", username1);
                cmd.Parameters.AddWithValue("user2", username2);

                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public static async Task<List<(string, Stat)>> GetScoreboard()
    {
        try
        {
            List<(string, Stat)> scoreboard = new List<(string, Stat)>();

            await using var conn = await DB_connection.connectDB();
            await using var cmd = new NpgsqlCommand("SELECT * FROM stats ORDER BY elo DESC", conn);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    string username = reader.GetString(1);
                    int battles_played = reader.GetInt32(2);
                    int wins = reader.GetInt32(3);
                    int losses = reader.GetInt32(4);
                    int draws = reader.GetInt32(5);
                    int elo = reader.GetInt32(6);
                    Stat stat = new Stat(battles_played, wins, losses, draws, elo);
                    scoreboard.Add((username, stat));
                }
            }
            return scoreboard;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}