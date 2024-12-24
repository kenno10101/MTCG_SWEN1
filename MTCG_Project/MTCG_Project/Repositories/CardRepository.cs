using MTCG_Project.Handler;
using MTCG_Project.Interfaces;
using static MTCG_Project.Misc.Enums;
using MTCG_Project.Models.Card;
using Npgsql;
using MTCG_Project.Exceptions;

namespace MTCG_Project.Repositories;

public class CardRepository
{

    private const int num_cards_in_package = 5;
    private const int num_cards_in_deck = 4;
    public static async Task<Stack> GetStack(string username)
    {
        try
        {
            Stack stack = new();
            await using var conn = await DB_connection.connectDB();

            string queryString = @"
                SELECT c.card_type, c.name, c.damage, c.element, c.monster FROM cards c
                INNER JOIN stack s ON c.id = s.card_id
                WHERE s.user_id = (SELECT id FROM users WHERE username = @un)
                ";
            await using var cmd = new NpgsqlCommand(queryString, conn);
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
            if (cards == null || cards.Length != num_cards_in_package)
            {
                throw new Exception($"Number of cards should be {num_cards_in_package}.");
            }

            await using var conn = await DB_connection.connectDB();
            await using (var cmd = new NpgsqlCommand(
                             "INSERT INTO \"packages\" (card_1_name, card_2_name, card_3_name, card_4_name, card_5_name) VALUES (@c1, @c2, @c3, @c4, @c5)",
                         conn))
            {
                for (int i = 0; i < num_cards_in_package; i++)
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
            
            string[] cards = {};
            
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
                    for(int i = 0; i <= num_cards_in_package; i++)
                    {
                        cards[i] = reader.GetString(i);
                    }
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

                for(int i = 0; i <= num_cards_in_package; i++)
                {
                    cmd_2.Parameters.AddWithValue("c"+i, cards[i]);
                }

                await cmd_2.ExecuteNonQueryAsync();
            }
            
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public static async Task CreateDeck(string username, string[] cards)
    {
        try
        {
            await using var conn = await DB_connection.connectDB();
            await using (var cmd = new NpgsqlCommand("INSERT INTO \"deck\" (username, card_1_name, card_2_name, card_3_name, card_4_name) VALUES (@u, @c1, @c2, @c3, @c4)", conn))
            {
                cmd.Parameters.AddWithValue("u", username);
                for (int i = 0; i < num_cards_in_deck; i++)
                {
                    cmd.Parameters.AddWithValue("c" + (i + 1), (cards != null && i < cards.Length && cards[i] != null) ? cards[i] : DBNull.Value);
                }

                await cmd.ExecuteNonQueryAsync();
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public static async Task<Deck> GetDeck(string username)
    {
        try
        {
            Deck deck = new Deck();
            await using var conn = await DB_connection.connectDB();

            string queryString = @"
                SELECT c.card_type, c.name, c.damage, c.element, c.monster FROM cards c JOIN deck d ON d.card_1_name = c.name WHERE username = @u
                UNION ALL
                SELECT c.card_type, c.name, c.damage, c.element, c.monster FROM cards c JOIN deck d ON d.card_2_name = c.name WHERE username = @u
                UNION ALL
                SELECT c.card_type, c.name, c.damage, c.element, c.monster FROM cards c JOIN deck d ON d.card_3_name = c.name WHERE username = @u
                UNION ALL
                SELECT c.card_type, c.name, c.damage, c.element, c.monster FROM cards c JOIN deck d ON d.card_4_name = c.name WHERE username = @u
                ";
            await using var cmd = new NpgsqlCommand(queryString, conn);
            cmd.Parameters.AddWithValue("u", username);
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

                        deck.cards.Add(card);
                    }
                    else if (card_type == "Monstercard")
                    {
                        Monster monster = (Monster)Enum.Parse(typeof(Monster), reader.GetString(4), true);
                        Monster_Card card = new Monster_Card(name, damage, element, monster);

                        deck.cards.Add(card);
                    }
                }
            }

            return deck;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
    
    public static async Task UpdateDeck(string username, string[] cards)
    {
        try
        {
            await using var conn = await DB_connection.connectDB();
            await using (var cmd = new NpgsqlCommand("UPDATE \"deck\" SET card_1_name = @c1, card_2_name = @c2, card_3_name = @c3, card_4_name = @c4 WHERE username = @u", conn))
            {
                cmd.Parameters.AddWithValue("u", username);
                for (int i = 0; i < num_cards_in_deck; i++)
                {
                    cmd.Parameters.AddWithValue("c" + (i + 1), cards[i]);
                }

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    throw new UserException("There was an error updating the user's deck");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}