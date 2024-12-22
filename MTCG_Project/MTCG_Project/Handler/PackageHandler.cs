using System.Text.Json.Nodes;
using MTCG_Project.Interfaces;
using MTCG_Project.Models.Card;
using MTCG_Project.Models.Users;
using MTCG_Project.Network;
using MTCG_Project.Repositories;

namespace MTCG_Project.Handler;

public class PackageHandler : Handler, IHandler
{
    public override bool Handle(HttpSvrEventArgs e)
    {
        bool is_BuyPackageRequest =
            (e.Path.TrimEnd('/', ' ', '\t') == "/transactions/packages") && (e.Method == "POST");
        bool is_CreatePackageRequest =
            (e.Path.TrimEnd('/', ' ', '\t') == "/packages") && (e.Method == "POST");
        
        if (is_BuyPackageRequest)
        {
            return buyPackage(e).GetAwaiter().GetResult();
        }
        else if (is_CreatePackageRequest)
        {
            return createPackage(e).GetAwaiter().GetResult();
        }
        

        return false;
    }

    public static async Task<bool> buyPackage(HttpSvrEventArgs e)
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

            if (ses.User.coins < 5)
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
    
    public static async Task<bool> createPackage(HttpSvrEventArgs e)
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

}