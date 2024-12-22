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
}