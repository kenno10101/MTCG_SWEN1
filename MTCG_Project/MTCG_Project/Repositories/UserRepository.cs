using MTCG_Project.Exceptions;
using MTCG_Project.Handler;
using MTCG_Project.Network;
using MTCG_Project.Interfaces;
using static MTCG_Project.Misc.Enums;
using MTCG_Project.Models.Stats;
using MTCG_Project.Models.Users;
using Npgsql;
using System.Data.Common;
using System.Diagnostics;
using MTCG_Project.Misc;
using NpgsqlTypes;

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
        queryString += usernameChange ? "," : "";
        queryString += passwordChange ? passwordString : "";
        queryString += usernameChange || passwordChange ? "," : "";
        queryString += nameChange ? nameString : "";
        queryString += usernameChange || passwordChange || nameChange ? "," : "";
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
                "SELECT battles_played, wins, losses, draws, elo, rank FROM \"stats\" WHERE username = @u",
                conn);
            cmd.Parameters.AddWithValue("u", username);

            Stat stat = null;

            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int battles_played = reader.GetInt32(0);
                    int wins = reader.GetInt32(1);
                    int losses = reader.GetInt32(2);
                    int draws = reader.GetInt32(3);
                    int elo = reader.GetInt32(4);
                    Rank rank = Enum.Parse<Rank>(reader.GetString(5), true);

                    stat = new Stat(battles_played, wins, losses, draws, elo, rank);
                }
            }

            if (stat == null)
            {
                throw new Exception("Stat not found.");
            }

            return stat;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public static async Task UpdateStats(string username1, string username2, Battleresult result)
    {
        string queryString = string.Empty;
        switch (result)
        {
            case (Battleresult.Win):
                queryString = @"
UPDATE ""stats"" SET
battles_played = battles_played + 1,
wins = wins + 1,
elo = elo + 5
";
                break;
            case (Battleresult.Loss):
                queryString = @"
UPDATE ""stats"" SET
battles_played = battles_played + 1,
losses = losses + 1,
elo = elo - 3
";
                break;
            case (Battleresult.Draw):
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
            case (Battleresult.Win):
                queryString += @"
UPDATE ""stats"" SET
battles_played = battles_played + 1,
losses = losses + 1,
elo = elo - 3
";
                break;
            case (Battleresult.Loss):
                queryString += @"
UPDATE ""stats"" SET
battles_played = battles_played + 1,
wins = wins + 1,
elo = elo + 5
";
                break;
            case (Battleresult.Draw):
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

            await updateRank(username1);
            await updateRank(username2);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    private static async Task updateRank(string username)
    {
        try {

            Stat user_stat = await GetStats(username);

            Rank rank_after_update = EloRank.getRankByElo(user_stat.Elo);

            string queryString = "UPDATE \"stats\" SET rank = @rank WHERE username = @u";

            await using var conn = await DB_connection.connectDB();
            await using (var cmd = new NpgsqlCommand(queryString, conn))
            {
                cmd.Parameters.AddWithValue("rank", NpgsqlDbType.Varchar, rank_after_update.ToString());
                cmd.CommandText = cmd.CommandText.Replace("@rank", "@rank::enum_ranks");
                cmd.Parameters.AddWithValue("u", username);

                await cmd.ExecuteNonQueryAsync();
            }
        } catch (Exception e)
        {
            throw new Exception(e.Message);
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
                    Rank rank = Enum.Parse<Rank>(reader.GetString(7), true);
                    Stat stat = new Stat(battles_played, wins, losses, draws, elo, rank);
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