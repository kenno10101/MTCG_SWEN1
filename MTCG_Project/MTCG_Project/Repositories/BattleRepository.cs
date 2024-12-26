using MTCG_Project.Handler;
using MTCG_Project.Models.Battle;
using Npgsql;

namespace MTCG_Project.Repositories;

public class BattleRepository
{
    public static async Task Save(Battle battle)
    {
        try
        {
            await using var conn = await DB_connection.connectDB();
            await using (var cmd = new NpgsqlCommand(
                             "INSERT INTO battle_history (number_rounds, points_player_1, points_player_2, winner, loser) VALUES (@r, @p1, @p2, @w, @l)",
                             conn))
            {
                cmd.Parameters.AddWithValue("r", battle.Rounds);
                cmd.Parameters.AddWithValue("p1", battle.Player_1_points);
                cmd.Parameters.AddWithValue("p2", battle.Player_2_points);
                cmd.Parameters.AddWithValue("w", battle.Winner);
                cmd.Parameters.AddWithValue("l", battle.Loser);

                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}