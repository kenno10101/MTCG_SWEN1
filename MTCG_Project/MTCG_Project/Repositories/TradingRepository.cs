using System.Data;
using System.Security.Cryptography;
using static MTCG_Project.Misc.Enums;
using MTCG_Project.Handler;
using MTCG_Project.Interfaces;
using MTCG_Project.Misc;
using MTCG_Project.Models.Card;
using MTCG_Project.Models.Tradings;
using Npgsql;
using NpgsqlTypes;
using System.Reflection.Metadata.Ecma335;

namespace MTCG_Project.Repositories;

public class TradingRepository
{
    public static async Task Create(string username, ICard card_offer, Card_Requirement card_requirement)
    {
        // lock card_offer from your deck?
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
                    card_requirement.cardtype == Cardtype.Monstercard
                        ? card_requirement.monster.ToString()
                        : DBNull.Value);
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
            await using var cmd = new NpgsqlCommand("SELECT * FROM tradings WHERE status = 'Open'::enum_trading_status", conn);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int id = reader.GetInt32(0);
                    String trade_creator = reader.GetString(1);
                    string trade_acceptor = reader.IsDBNull(2) ? null : reader.GetString(2);
                    Tradingstatus tradingstatus = Enum.Parse<Tradingstatus>(reader.GetString(3), true);

                    ICard card_offer = await CardRepository.GetCard(reader.GetString(4));
                    Card_Requirement card_requirement = new Card_Requirement(
                        Enum.Parse<Cardtype>(reader.GetString(5), true),
                        Enum.Parse<Element>(reader.GetString(6), true),
                        reader.IsDBNull(7) ? Monster.Null : Enum.Parse<Monster>(reader.GetString(7), true),
                        reader.GetInt32(8),
                        reader.GetInt32(9)
                    );
                    ICard card_received =
                        reader.IsDBNull(10) ? null : await CardRepository.GetCard(reader.GetString(10));

                    trading_deals.Add(new Trading(id, trade_creator, trade_acceptor, tradingstatus, card_offer,
                        card_requirement, card_received));
                }
            }

            return trading_deals;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    
    public static async Task<Trading> Get(int trade_id)
    {
        try
        {
            Trading trade = null;
            
            await using var conn = await DB_connection.connectDB();
            await using var cmd = new NpgsqlCommand("SELECT * FROM tradings WHERE id = @t_id", conn);
            cmd.Parameters.AddWithValue("t_id", trade_id);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int id = reader.GetInt32(0);
                    string trade_creator = reader.GetString(1);
                    string trade_acceptor = reader.IsDBNull(2) ? null : reader.GetString(2);
                    Tradingstatus tradingstatus = Enum.Parse<Tradingstatus>(reader.GetString(3), true);

                    ICard card_offer = await CardRepository.GetCard(reader.GetString(4));
                    Card_Requirement card_requirement = new Card_Requirement(
                        Enum.Parse<Cardtype>(reader.GetString(5), true),
                        Enum.Parse<Element>(reader.GetString(6), true),
                        reader.IsDBNull(7) ? Monster.Null : Enum.Parse<Monster>(reader.GetString(7), true),
                        reader.GetInt32(8),
                        reader.GetInt32(9)
                    );
                    ICard card_received =
                        reader.IsDBNull(10) ? null : await CardRepository.GetCard(reader.GetString(10));

                    trade = new Trading(id, trade_creator, trade_acceptor, tradingstatus, card_offer,
                        card_requirement, card_received);
                }
            }
            
            if (trade == null)
            {
                throw new Exception("Trade not found.");
            }

            return trade;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public static async Task Trade(int trade_id, string username, string cardname)
    {
        // update each users deck after trade

        try
        {
            await using var conn = await DB_connection.connectDB();
            await using (var cmd = new NpgsqlCommand(
                             "UPDATE tradings SET status = 'Done'::enum_trading_status, trade_acceptor = @u, card_received = @c WHERE id = @id AND status = 'Open'::enum_trading_status;",
                             conn))
            {
                cmd.Parameters.AddWithValue("u", username);
                cmd.Parameters.AddWithValue("c", cardname);
                cmd.Parameters.AddWithValue("id", trade_id);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    throw new Exception("Trade ID doesn't exist or Trade is not Open");
                }
            }
            Trading trade = await Trading.Get(trade_id);
            // get each user's decks
            Deck deck_creator = await Deck.Get(trade.trade_creator);
            Deck deck_acceptor = await Deck.Get(trade.trade_acceptor);

            // find the card in each deck
            int index_creator = deck_creator.cards.FindIndex(card => card.Name == trade.card_offer.Name);
            int index_acceptor = deck_acceptor.cards.FindIndex(card => card.Name == trade.card_received.Name);
            // swap the cards in the decks
            ICard temp = deck_creator.cards[index_creator];
            deck_creator.cards[index_creator] = deck_acceptor.cards[index_acceptor];
            deck_acceptor.cards[index_acceptor] = temp;

            // update stack
            Stack.Update(trade.trade_creator, trade.card_offer.Name, trade.card_received.Name);
            Stack.Update(trade.trade_acceptor, trade.card_received.Name, trade.card_offer.Name);

            // update deck
            Deck.Update(trade.trade_creator, deck_creator.cards.Select(card => card.Name).ToArray());
            Deck.Update(trade.trade_acceptor, deck_acceptor.cards.Select(card => card.Name).ToArray());

            
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public static async Task Delete(int trade_id)
    {
        // can only be deleted by trade creator or admin
        // unlock card_offer from your deck
        try
        {
            await using var conn = await DB_connection.connectDB();
            await using (var cmd = new NpgsqlCommand(
                             "UPDATE tradings SET status = 'Deleted'::enum_trading_status WHERE id = @id AND status = 'Open'::enum_trading_status;", conn))
            {
                cmd.Parameters.AddWithValue("id", trade_id);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    throw new Exception("Trade ID doesn't exist or Trade is not Open");
                }
            }
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}