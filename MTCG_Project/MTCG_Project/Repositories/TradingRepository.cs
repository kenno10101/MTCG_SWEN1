using System.Data;
using static MTCG_Project.Misc.Enums;
using MTCG_Project.Handler;
using MTCG_Project.Interfaces;
using MTCG_Project.Misc;
using MTCG_Project.Models.Card;
using MTCG_Project.Models.Tradings;
using Npgsql;
using NpgsqlTypes;

namespace MTCG_Project.Repositories;

public class TradingRepository
{
    public static async Task Create(string username, ICard card_offer, Card_Requirement card_requirement)
    {
        try
        {
            await using var conn = await DB_connection.connectDB();
            await using (var cmd = new NpgsqlCommand(
                             "INSERT INTO tradings (trade_creator, card_offer, card_requirement_type, card_requirement_element, card_requirement_monster, card_requirement_min_dmg, card_requirement_max_dmg) VALUES (@tc, @co, @type, @element, @monster, @mindmg, @maxdmg)",
                             conn))
            {
                cmd.Parameters.AddWithValue("tc", username);
                cmd.Parameters.AddWithValue("co", card_offer.Name);
                cmd.Parameters.AddWithValue("type", NpgsqlDbType.Varchar, card_requirement.cardtype.ToString());
                cmd.CommandText = cmd.CommandText.Replace("@type", "@type::enum_card_type");
                cmd.Parameters.AddWithValue("element", card_requirement.element.ToString());
                cmd.CommandText = cmd.CommandText.Replace("@element", "@element::enum_element");
                cmd.Parameters.AddWithValue("monster",
                    card_requirement.cardtype == Cardtype.Monstercard ? card_requirement.monster.ToString() : DBNull.Value);
                cmd.CommandText = cmd.CommandText.Replace("@monster", "@monster::enum_monster");
                cmd.Parameters.AddWithValue("mindmg", card_requirement.minDamage);
                cmd.Parameters.AddWithValue("maxdmg", card_requirement.maxDamage);

                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public static async Task<List<Trading>> GetAll()
    {
        try
        {
            List<Trading> trading_deals = new List<Trading>();
            await using var conn = await DB_connection.connectDB();
            await using var cmd = new NpgsqlCommand("SELECT * FROM tradings", conn);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    String trade_creator = reader.GetString(1);
                    string trade_acceptor = reader.IsDBNull(2) ? null : reader.GetString(2);
                    Tradingstatus tradingstatus = Enum.Parse<Tradingstatus>(reader.GetString(3), true);

                    ICard card_offer = await CardRepository.GetCard(reader.GetString(4));
                    Card_Requirement card_requirement = new Card_Requirement(
                        Enum.Parse<Cardtype>(reader.GetString(5), true),
                        Enum.Parse<Element>(reader.GetString(6), true),
                        Enum.Parse<Monster>(reader.GetString(7), true),
                        reader.GetInt32(8),
                        reader.GetInt32(9)
                    );
                    
                    trading_deals.Add(new Trading(trade_creator, trade_acceptor, tradingstatus, card_offer,
                        card_requirement));
                }
            }

            return trading_deals;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    // public static async Task Trade(int trade_id)
    // {
    //     try
    //     {
    //         await using var conn = await DB_connection.connectDB();
    //         await using (var cmd = new NpgsqlCommand(
    //                          "INSERT INTO battle_history (number_rounds, points_player_1, points_player_2, winner, loser) VALUES (@r, @p1, @p2, @w, @l)",
    //                          conn))
    //         {
    //             cmd.Parameters.AddWithValue("r", battle.Rounds);
    //             cmd.Parameters.AddWithValue("p1", battle.Player_1_points);
    //             cmd.Parameters.AddWithValue("p2", battle.Player_2_points);
    //             cmd.Parameters.AddWithValue("w", battle.Winner);
    //             cmd.Parameters.AddWithValue("l", battle.Loser);
    //
    //             await cmd.ExecuteNonQueryAsync();
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         throw new Exception(e.Message);
    //     }
    // }
    //
    // public static async Task Delete(int trade_id)
    // {
    //     try
    //     {
    //         await using var conn = await DB_connection.connectDB();
    //         await using (var cmd = new NpgsqlCommand(
    //                          "INSERT INTO battle_history (number_rounds, points_player_1, points_player_2, winner, loser) VALUES (@r, @p1, @p2, @w, @l)",
    //                          conn))
    //         {
    //             cmd.Parameters.AddWithValue("r", battle.Rounds);
    //             cmd.Parameters.AddWithValue("p1", battle.Player_1_points);
    //             cmd.Parameters.AddWithValue("p2", battle.Player_2_points);
    //             cmd.Parameters.AddWithValue("w", battle.Winner);
    //             cmd.Parameters.AddWithValue("l", battle.Loser);
    //
    //             await cmd.ExecuteNonQueryAsync();
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         throw new Exception(e.Message);
    //     }
    // }
}