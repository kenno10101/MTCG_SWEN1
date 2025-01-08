using System.Net.Sockets;
using System.Text.Json.Nodes;
using MTCG_Project.Exceptions;
using MTCG_Project.Interfaces;
using MTCG_Project.Models.Battle;
using MTCG_Project.Models.Card;
using MTCG_Project.Models.Users;
using MTCG_Project.Network;
using MTCG_Project.Repositories;

namespace MTCG_Project.Handler;

public class BattleHandler : Handler, IHandler
{
    public override bool Handle(HttpSvrEventArgs e)
    {
        return false;
    }
    
    public static async Task joinBattle(HttpSvrEventArgs e1, HttpSvrEventArgs e2)
    {
        
        JsonObject? reply = new JsonObject() { ["success"] = false, ["message"] = "Invalid request." };
        int status = HttpStatusCodes.BAD_REQUEST;
        
        try
        {
            
            (bool Success, User? User) ses1 = await Token.Authenticate_Request(e1);
            (bool Success, User? User) ses2 = await Token.Authenticate_Request(e2);

            if (!ses1.Success || !ses2.Success)
            {
                status = HttpStatusCodes.UNAUTHORIZED;
                throw new Exception("Unauthorized");
            }

            Battle battle = new Battle();
            JsonObject battleLogJson = await battle.JoinBattle(ses1.User, ses2.User);
            
            reply = new JsonObject()
            {
                ["success"] = true,
                ["message"] = "Battle has finished.",
                ["battle_log"] = battleLogJson
            };
            status = HttpStatusCodes.OK;
            
        }
        catch (Exception ex)
        {
            reply = new JsonObject() { ["success"] = false, ["message"] = ex.Message ?? "Error handling request." };
        }
        
        e1.Reply(status, reply?.ToJsonString());
        e2.Reply(status, reply?.ToJsonString());

    }

    
}