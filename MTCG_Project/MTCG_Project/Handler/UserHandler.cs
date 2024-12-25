using MTCG_Project.Interfaces;
using MTCG_Project.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG_Project.Models.Users;
using MTCG_Project.Models.Stats;
using MTCG_Project.Models.Card;
using System.Net;
using System.Text.Json.Nodes;
using MTCG_Project.Exceptions;
using System.Reflection.Metadata;
using Npgsql;

namespace MTCG_Project.Handler
{
    public class UserHandler : Handler, IHandler
    {
        public override bool Handle(HttpSvrEventArgs e)
        {
            bool is_CreateUserRequest = (e.Path.TrimEnd('/', ' ', '\t') == "/users") && (e.Method == "POST");
            bool is_QueryUserRequest = e.Path.StartsWith("/users/") && (e.Method == "GET");
            bool is_UpdateUserRequest = e.Path.StartsWith("/users/") && (e.Method == "PUT");
            bool is_GetUserStatsRequest = (e.Path.TrimEnd('/', ' ', '\t') == "/stats") && (e.Method == "GET");
            

            if (is_CreateUserRequest)
            {                                                                   // POST /users will create a user object
                return _CreateUser(e).GetAwaiter().GetResult();
            }
            else if (is_QueryUserRequest)        // GET /users/UserName will query a user
            {
                return _QueryUser(e).GetAwaiter().GetResult();
            }
            else if (is_UpdateUserRequest)
            {
                return _UpdateUser(e).GetAwaiter().GetResult();
            }
            else if (is_GetUserStatsRequest)
            {
                return _GetUserStats(e).GetAwaiter().GetResult();
            }
            

            return false;
        }

        private static async Task<bool> _CreateUser(HttpSvrEventArgs e)
        {
            JsonObject? reply = new JsonObject() { ["success"] = false, ["message"] = "Invalid request." };
            int status = HttpStatusCodes.BAD_REQUEST;

            try
            {
                JsonNode? json = JsonNode.Parse(e.Payload);
                if (json != null)
                {
                    // create user object
                    await User.Create(
                        (string) json["username"],
                        (string) json["password"],
                        (string) json["name"],
                        (string) json["email"]);
                    
                    status = HttpStatusCodes.OK;
                    reply = new JsonObject()
                    {
                        ["success"] = true,
                        ["message"] = "User created."
                    };
                }
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

        private static async Task<bool> _QueryUser(HttpSvrEventArgs e)
        {
            JsonObject? reply = new JsonObject() { ["success"] = false, ["message"] = "Invalid request."};
            int status = HttpStatusCodes.BAD_REQUEST;

            try
            {

                string username_from_path = e.Path.Substring(e.Path.LastIndexOf('/') + 1);
                
                (bool Success, User? User) ses = await Token.Authenticate_Request(e);

                if (!ses.Success)
                {
                    status = HttpStatusCodes.UNAUTHORIZED;
                    throw new Exception("Unauthorized");
                }
                
                User? user = await User.Get(username_from_path);

                
                JsonObject? userResponse = new JsonObject(){
                    ["user_UserName"] = user.UserName,
                    ["user_Password"] = user.Password,
                    ["user_FullName"] = user.FullName,
                    ["user_EMail"] = user.EMail,
                    ["user_coin"] = user.Coins
                };

                status = HttpStatusCodes.OK;
                reply.Add("user", userResponse);
                reply["success"] = true;
                reply["message"] = "Query success";
            }
            catch (UserException ex)
            {
                reply = new JsonObject() { ["success"] = false, ["message"] = ex.Message };
            }
            catch (Exception ex)
            {
                reply = new JsonObject() { ["success"] = false, ["message"] = ex.Message ?? "Unexpected error."};
            }

            e.Reply(status, reply?.ToJsonString());
            return true;
        }

        private static async Task<bool> _UpdateUser(HttpSvrEventArgs e)
        {

            JsonObject? reply = new JsonObject() { ["success"] = false, ["message"] = "Invalid request." };
            int status = HttpStatusCodes.BAD_REQUEST;

            try
            {
                string username_from_path = e.Path.Substring(e.Path.LastIndexOf('/') + 1);

                (bool Success, User? User) ses = await Token.Authenticate_Request(e);

                if (!ses.Success)
                {
                    status = HttpStatusCodes.UNAUTHORIZED;
                    throw new Exception("Unauthorized");
                }
                JsonNode? json = JsonNode.Parse(e.Payload);
                if (json != null)
                {
                    // create user object
                    await User.Update(
                        username_from_path,
                        (string?)json["username"],
                        (string?)json["password"],
                        (string?)json["name"],
                        (string?)json["email"]
                        );
                }

                status = HttpStatusCodes.OK;
                reply["success"] = true;
                reply["message"] = "Update success";
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

        private static async Task<bool> _GetUserStats(HttpSvrEventArgs e)
        {
            JsonObject? reply = new JsonObject() { ["success"] = false, ["message"] = "Invalid request."};
            int status = HttpStatusCodes.BAD_REQUEST;

            try
            {
                
                (bool Success, User? User) ses = await Token.Authenticate_Request(e);

                if (!ses.Success)
                {
                    status = HttpStatusCodes.UNAUTHORIZED;
                    throw new Exception("Unauthorized");
                }

                Stat stat = await Stat.Get(ses.User.UserName);
                
                JsonObject? statsObject = new JsonObject(){
                    ["battles_played"] = stat.Battles_played,
                    ["wins"] = stat.Wins,
                    ["losses"] = stat.Losses,
                    ["draws"] = stat.Draws,
                    ["elo"] = stat.Elo
                };
                
                JsonObject? statsResponse = new JsonObject(){
                    ["username"] = ses.User.UserName,
                    ["stats"] = statsObject
                };

                status = HttpStatusCodes.OK;
                reply.Add("statsResponse", statsResponse);
                reply["success"] = true;
                reply["message"] = "Query success";
            }
            catch (UserException ex)
            {
                reply = new JsonObject() { ["success"] = false, ["message"] = ex.Message };
            }
            catch (Exception ex)
            {
                reply = new JsonObject() { ["success"] = false, ["message"] = ex.Message ?? "Unexpected error."};
            }

            e.Reply(status, reply?.ToJsonString());
            return true;
        }

    }
}
