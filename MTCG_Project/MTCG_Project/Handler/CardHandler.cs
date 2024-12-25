using System.Text.Json.Nodes;
using MTCG_Project.Exceptions;
using MTCG_Project.Interfaces;
using MTCG_Project.Models.Card;
using MTCG_Project.Models.Users;
using MTCG_Project.Network;
using MTCG_Project.Repositories;

namespace MTCG_Project.Handler;

public class CardHandler : Handler, IHandler
{
    public override bool Handle(HttpSvrEventArgs e)
    {
        bool is_BuyPackageRequest =
            (e.Path.TrimEnd('/', ' ', '\t') == "/transactions/packages") && (e.Method == "POST");
        bool is_CreatePackageRequest = (e.Path.TrimEnd('/', ' ', '\t') == "/packages") && (e.Method == "POST");
        bool is_GetStackFromUserRequest = (e.Path.TrimEnd('/', ' ', '\t') == "/cards") && (e.Method == "GET");
        bool is_GetDeckFromUserRequest = (e.Path.TrimEnd('/', ' ', '\t') == "/deck") && (e.Method == "GET");
        bool is_UpdateDeckRequest = (e.Path.TrimEnd('/', ' ', '\t') == "/deck") && (e.Method == "PUT");

        if (is_BuyPackageRequest)
        {
            return buyPackage(e).GetAwaiter().GetResult();
        }
        else if (is_CreatePackageRequest)
        {
            return createPackage(e).GetAwaiter().GetResult();
        }
        else if (is_GetStackFromUserRequest)
        {
            return _GetStackFromUser(e).GetAwaiter().GetResult();
        }
        else if (is_GetDeckFromUserRequest)
        {
            return _GetDeckFromUser(e).GetAwaiter().GetResult();
        }
        else if (is_UpdateDeckRequest)
        {
            return _UpdateDeck(e).GetAwaiter().GetResult();
        }


        return false;
    }

    private static async Task<bool> buyPackage(HttpSvrEventArgs e)
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

            if (ses.User.Coins < 5)
            {
                status = 403;
                throw new Exception("Not enough money for buying a card package");
            }
            await Package.BuyPackage(ses.User.UserName);
            reply = new JsonObject() { ["success"] = true, ["message"] = "Buy Package success" };
            status = HttpStatusCodes.OK;
            
        }
        catch (Exception ex)
        {
            reply = new JsonObject() { ["success"] = false, ["message"] = ex.Message ?? "Error handling request." };
        }

        e.Reply(status, reply?.ToJsonString());
        return true;
    }
    
    private static async Task<bool> createPackage(HttpSvrEventArgs e)
    {
        JsonObject? reply = new JsonObject()
        {
            ["success"] = false,
            ["message"] = "Invalid request."
        };
        int status = HttpStatusCodes.BAD_REQUEST;
        
        try
        {
            JsonNode? json = JsonNode.Parse(e.Payload);
            if (json != null)
            {
                
                string[]? package_cards = json?["cardnames"]?.AsArray()
                    .Select(node => node.ToString())
                    .ToArray();

                await Package.CreatePackage(package_cards);
                
                reply = new JsonObject() { ["success"] = true, ["message"] = "Create Package success" };
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
    
    private static async Task<bool> _GetStackFromUser(HttpSvrEventArgs e)
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

            Stack stack = await Stack.GetStack(ses.User.UserName);

            JsonObject? stackObject = new JsonObject();

            int i = 0;
            foreach (var card in stack.cards)
            {
                JsonObject? stackCard = new JsonObject();
                stackCard.Add("card_type", card is Monster_Card ? "Monstercard" : "Spellcard");
                stackCard.Add("card_name", card.Name);
                stackCard.Add("damage", card.Damage);
                stackCard.Add("element", card.Element.ToString());
                if (card is Monster_Card monster_card)
                {
                    stackCard.Add("monster", monster_card.Monster.ToString());
                }
                stackObject.Add($"card_{i++}", stackCard);
            }

            JsonObject? stackResponse = new JsonObject()
            {
                ["username"] = ses.User.UserName,
                ["stack"] = stackObject
            };
            reply.Add("stackResponse", stackResponse);


            status = HttpStatusCodes.OK;
            reply["success"] = true;
            reply["message"] = "Query of user's stack success.";
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

    private static async Task<bool> _GetDeckFromUser(HttpSvrEventArgs e)
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

            Deck deck = await Deck.Get(ses.User.UserName);

            JsonObject? stackObject = new JsonObject();

            int i = 0;
            foreach (var card in deck.cards)
            {
                JsonObject? deckCard = new JsonObject();
                deckCard.Add("card_type", card is Monster_Card ? "Monstercard" : "Spellcard");
                deckCard.Add("card_name", card.Name);
                deckCard.Add("damage", card.Damage);
                deckCard.Add("element", card.Element.ToString());
                if (card is Monster_Card monster_card)
                {
                    deckCard.Add("monster", monster_card.Monster.ToString());
                }
                stackObject.Add($"card_{i++}", deckCard);
            }

            JsonObject? deckResponse = new JsonObject()
            {
                ["username"] = ses.User.UserName,
                ["deck"] = stackObject
            };
            reply.Add("deckResponse", deckResponse);


            status = HttpStatusCodes.OK;
            reply["success"] = true;
            reply["message"] = "Query of user's deck success.";
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
    
    private static async Task<bool> _UpdateDeck(HttpSvrEventArgs e)
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

            JsonNode? json = JsonNode.Parse(e.Payload);
            if (json != null)
            {
                string[] cards = { (string)json["card_1_name"], (string)json["card_2_name"], (string)json["card_3_name"], (string)json["card_4_name"] };

                await Deck.Update(ses.User.UserName, cards);
            }

            status = HttpStatusCodes.OK;
            reply["success"] = true;
            reply["message"] = "Update deck success";
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
}