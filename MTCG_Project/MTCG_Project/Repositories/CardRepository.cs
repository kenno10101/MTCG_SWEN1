using MTCG_Project.Handler;
using MTCG_Project.Interfaces;
using static MTCG_Project.Misc.Enums;
using MTCG_Project.Models.Card;
using Npgsql;

namespace MTCG_Project.Repositories;

public class CardRepository
{
    public static async Task<Stack> GetStack(string username)
    {
        try
        {
            Stack stack = new();
            await using var conn = await DB_connection.connectDB();
            await using var cmd = new NpgsqlCommand(
                "SELECT c.card_type, c.name, c.damage, c.element, c.monster FROM cards c " +
                "INNER JOIN stack s ON c.id = s.card_id " +
                "WHERE s.user_id = (SELECT id FROM users WHERE username = @un)",
                conn);
            cmd.Parameters.AddWithValue("un", username);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    string card_type = reader.GetString(0);
                    string name = reader.GetString(1);
                    int damage = reader.GetInt32(2);
                    Element element = (Element)Enum.Parse(typeof(Element), reader.GetString(3), true);
                    
                    if (card_type == "Spellcard")
                    {
                        Spell_Card card = new Spell_Card(name, damage, element);
                        
                        stack.cards.Add(card);
                    }
                    else if (card_type == "Monstercard")
                    {
                        Monster monster = (Monster)Enum.Parse(typeof(Monster), reader.GetString(4), true);
                        Monster_Card card = new Monster_Card(name, damage, element, monster);
                        
                        stack.cards.Add(card);
                    }
                }
            }

            return stack;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public static async Task CreatePackage(string[] cards)
    {
        try
        {
            await using var conn = await DB_connection.connectDB();
            await using (var cmd = new NpgsqlCommand(
                             "INSERT INTO \"packages\" (card_1_name, card_2_name, card_3_name, card_4_name, card_5_name) VALUES (@c1, @c2, @c3, @c4, @c5)",
                         conn))
            {
                for (int i = 0; i < cards.Length; i++)
                {
                    cmd.Parameters.AddWithValue("c" + (i + 1), cards[i]);
                }

                await cmd.ExecuteNonQueryAsync();
            };
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
    
    public static async Task BuyPackage(string username)
    {
        try
        {
            
            string card_1_name = null, card_2_name = null, card_3_name = null, card_4_name = null, card_5_name = null;
            await using var conn = await DB_connection.connectDB();
            
            // subtract 5 coins
            await using (var cmd_1 = new NpgsqlCommand(
                             "UPDATE \"users\" SET coins = coins - 5 WHERE username = @u",
                             conn))
            {
                cmd_1.Parameters.AddWithValue("u", username);
                
                await cmd_1.ExecuteNonQueryAsync();
            };
            
            // get cards from random package
            await using var cmd = new NpgsqlCommand(
                "SELECT card_1_name, card_2_name, card_3_name, card_4_name, card_5_name FROM \"packages\" ORDER BY RANDOM() LIMIT 1",
                conn);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    card_1_name = reader.GetString(0);
                    card_2_name = reader.GetString(1);
                    card_3_name = reader.GetString(2);
                    card_4_name = reader.GetString(3);
                    card_5_name = reader.GetString(4);
                }
            }
            
            // add cards from package to user's stack
            await using (var cmd_2 = new NpgsqlCommand(
                             "INSERT INTO \"stack\" (user_id, card_id) " +
                             "VALUES ((SELECT id FROM users WHERE username = @u), (SELECT id FROM cards WHERE name = @c1)), " +
                             "((SELECT id FROM users WHERE username = @u), (SELECT id FROM cards WHERE name = @c2)), " +
                             "((SELECT id FROM users WHERE username = @u), (SELECT id FROM cards WHERE name = @c3)), " +
                             "((SELECT id FROM users WHERE username = @u), (SELECT id FROM cards WHERE name = @c4)), " +
                             "((SELECT id FROM users WHERE username = @u), (SELECT id FROM cards WHERE name = @c5))",
                             conn))
            {
                cmd_2.Parameters.AddWithValue("u", username);
                cmd_2.Parameters.AddWithValue("c1", card_1_name);
                cmd_2.Parameters.AddWithValue("c2", card_2_name);
                cmd_2.Parameters.AddWithValue("c3", card_3_name);
                cmd_2.Parameters.AddWithValue("c4", card_4_name);
                cmd_2.Parameters.AddWithValue("c5", card_5_name);

                await cmd_2.ExecuteNonQueryAsync();
            }
            
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}