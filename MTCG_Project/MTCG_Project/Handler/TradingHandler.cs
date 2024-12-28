using System.Text.Json.Nodes;
using MTCG_Project.Exceptions;
using MTCG_Project.Interfaces;
using static MTCG_Project.Misc.Enums;
using MTCG_Project.Misc;
using MTCG_Project.Models.Card;
using MTCG_Project.Models.Tradings;
using MTCG_Project.Models.Users;
using MTCG_Project.Network;
using MTCG_Project.Repositories;

namespace MTCG_Project.Handler;

public class TradingHandler : Handler, IHandler
{
    public override bool Handle(HttpSvrEventArgs e)
    {
        bool is_CreateTradeDealRequest = (e.Path.TrimEnd('/', ' ', '\t') == "/tradings") && (e.Method == "POST");
        bool is_GetAllTradeDealsRequest = (e.Path.TrimEnd('/', ' ', '\t') == "/tradings") && (e.Method == "GET");
        bool is_AcceptTradeDealRequest = (e.Path.StartsWith("/tradings/")) && (e.Method == "POST");
        bool is_DeleteTradeDealRequest = (e.Path.StartsWith("/tradings/")) && (e.Method == "DELETE");

        if (is_CreateTradeDealRequest)
        {
            return createTradeDeal(e).GetAwaiter().GetResult();
        }
        else if (is_GetAllTradeDealsRequest)
        {
            return getAllTradeDeals(e).GetAwaiter().GetResult();
        }
        else if (is_AcceptTradeDealRequest)
        {
            return acceptTradeDeal(e).GetAwaiter().GetResult();
        }
        else if (is_DeleteTradeDealRequest)
        {
            return deleteTradeDeal(e).GetAwaiter().GetResult();
        }

        return false;
    }

    private static async Task<bool> createTradeDeal(HttpSvrEventArgs e)
    {
        JsonObject? reply = new JsonObject()
        {
            ["success"] = false,
            ["message"] = "Invalid request."
        };
        int status = HttpStatusCodes.BAD_REQUEST;

        try
        {
            (bool Success, User? User) ses = await Token.Authenticate_Request(e);

            if (!ses.Success)
            {
                status = HttpStatusCodes.UNAUTHORIZED;
                throw new Exception("Unauthorized");
            }

            JsonNode? json = JsonNode.Parse(e.Payload);
            if (json != null)
            {
                Card_Requirement card_requirement = new Card_Requirement(
                    Enum.Parse<Cardtype>((string)json["cardtype"], true),
                    Enum.Parse<Element>((string)json["element"], true),
                    Enum.Parse<Monster>((string)json["monster"], true),
                    (int)json["minDamage"],
                    (int)json["maxDamage"]
                );

                // refactor card ses.User._deck.cards.FirstOrDefault(card => card.Name == (string)json["cardname_to_offer"]) check if null beforehand
                await Trading.Create(ses.User.UserName,
                    ses.User._deck.cards.FirstOrDefault(card => card.Name == (string)json["cardname_to_offer"]),
                    card_requirement);

                reply = new JsonObject() { ["success"] = true, ["message"] = "Create Trade Deal success" };
                status = HttpStatusCodes.OK;
            }
        }
        catch (Exception ex)
        {
            reply = new JsonObject() { ["success"] = false, ["message"] = ex.Message ?? "Error handling request." };
        }

        e.Reply(status, reply?.ToJsonString());
        return true;
    }

    private static async Task<bool> getAllTradeDeals(HttpSvrEventArgs e)
    {
        JsonObject? reply = new JsonObject() { ["success"] = false, ["message"] = "Invalid request." };
        int status = HttpStatusCodes.BAD_REQUEST;

        try
        {
            (bool Success, User? User) ses = await Token.Authenticate_Request(e);

            if (!ses.Success)
            {
                status = HttpStatusCodes.UNAUTHORIZED;
                throw new Exception("Unauthorized");
            }

            List<Trading> tradeDeals = await Trading.GetAll();

            JsonObject? tradeDealsResponse = new JsonObject();

            int i = 1;
            foreach (var tradeDeal in tradeDeals)
            {
                JsonObject? tradeDealObject = new JsonObject()
                {
                    ["trade_id"] = tradeDeal.id,
                    ["trade_creator"] = tradeDeal.trade_creator,
                    ["trade_acceptor"] = tradeDeal.trade_acceptor,
                    ["trading_status"] = tradeDeal.trading_status.ToString(),
                    ["card_offer"] = tradeDeal.card_offer.Name
                };
                JsonObject? tradeDealObjectRequirement = new JsonObject()
                {
                    ["cardtype"] = tradeDeal.card_requirement.cardtype.ToString(),
                    ["element"] = tradeDeal.card_requirement.element.ToString(),
                    ["monster"] = tradeDeal.card_requirement.monster.ToString(),
                    ["minDamage"] = tradeDeal.card_requirement.minDamage.ToString(),
                    ["maxDamage"] = tradeDeal.card_requirement.maxDamage.ToString()
                };
                tradeDealObject.Add("card_requirement", tradeDealObjectRequirement);
                tradeDealsResponse.Add("trade" + (i++), tradeDealObject);
            }

            reply.Add("tradeDealsResponse", tradeDealsResponse);

            status = HttpStatusCodes.OK;
            reply["success"] = true;
            reply["message"] = "Query of trade deals stack success.";
        }
        catch (UserException ex)
        {
            reply = new JsonObject() { ["success"] = false, ["message"] = ex.Message };
        }
        catch (Exception ex)
        {
            reply = new JsonObject() { ["success"] = false, ["message"] = ex.Message ?? "Unexpected error." };
        }

        e.Reply(status, reply?.ToJsonString());
        return true;
    }

    private static async Task<bool> acceptTradeDeal(HttpSvrEventArgs e)
    {
        JsonObject? reply = new JsonObject()
        {
            ["success"] = false,
            ["message"] = "Invalid request."
        };
        int status = HttpStatusCodes.BAD_REQUEST;

        try
        {
            (bool Success, User? User) ses = await Token.Authenticate_Request(e);

            if (!ses.Success)
            {
                status = HttpStatusCodes.UNAUTHORIZED;
                throw new Exception("Unauthorized");
            }

            JsonNode? json = JsonNode.Parse(e.Payload);
            if (json != null)
            {
                string trade_id_from_path = e.Path.Substring(e.Path.LastIndexOf('/') + 1);
                await Trading.AcceptTrade(int.Parse(trade_id_from_path), ses.User.UserName,
                    ses.User._deck.cards.FirstOrDefault(card => card.Name == (string)json["card_name"]).Name);

                reply = new JsonObject() { ["success"] = true, ["message"] = "Trade success" };
                status = HttpStatusCodes.OK;
            }
        }
        catch (Exception ex)
        {
            reply = new JsonObject() { ["success"] = false, ["message"] = ex.Message ?? "Error handling request." };
        }

        e.Reply(status, reply?.ToJsonString());
        return true;
    }

    private static async Task<bool> deleteTradeDeal(HttpSvrEventArgs e)
    {
        JsonObject? reply = new JsonObject()
        {
            ["success"] = false,
            ["message"] = "Invalid request."
        };
        int status = HttpStatusCodes.BAD_REQUEST;

        try
        {
            (bool Success, User? User) ses = await Token.Authenticate_Request(e);

            if (!ses.Success)
            {
                status = HttpStatusCodes.UNAUTHORIZED;
                throw new Exception("Unauthorized");
            }
            
            string trade_id_from_path = e.Path.Substring(e.Path.LastIndexOf('/') + 1);
            
            Trading trade = await TradingRepository.Get(int.Parse(trade_id_from_path));
            if (trade.trade_creator != ses.User.UserName)
            {
                throw new Exception("Cannot delete trade offers of others.");
            }
            await Trading.DeleteTrade(int.Parse(trade_id_from_path));

            reply = new JsonObject() { ["success"] = true, ["message"] = "Delete Trade success" };
            status = HttpStatusCodes.OK;
        }
        catch (Exception ex)
        {
            reply = new JsonObject() { ["success"] = false, ["message"] = ex.Message ?? "Error handling request." };
        }

        e.Reply(status, reply?.ToJsonString());
        return true;
    }
}