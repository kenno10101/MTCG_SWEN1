using MTCG_Project.Interfaces;
using MTCG_Project.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG_Project.Models.User;
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
                return _UpdateUser(e).GetAwaiter().GetResult();;
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
                        (string) json["username"]!,
                        (string)json["password"]!,
                        (string?) json["name"] ?? "Max Mustermann",
                        (string?) json["email"] ?? "test@test.at");
                    
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
                // (bool Success, User? User) ses = Token.Authenticate_Request(e);
                //
                // if (!ses.Success)
                // {
                //     status = HttpStatusCodes.UNAUTHORIZED;
                //     throw new Exception("Unauthorized");
                // }

                string username_from_path = e.Path.Substring(e.Path.LastIndexOf('/') + 1);
                User? user = await User.Get(username_from_path);

                status = HttpStatusCodes.OK;
                JsonObject? userResponse = new JsonObject(){
                    ["user_UserName"] = user.UserName,
                    ["user_Password"] = user.Password,
                    ["user_FullName"] = user.FullName,
                    ["user_EMail"] = user.EMail
                };
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
                (bool Success, User? User) ses = await Token.Authenticate_Request(e);

                if (!ses.Success)
                {
                    status = HttpStatusCodes.UNAUTHORIZED;
                    //throw new Exception("Unauthorized");
                }

                string username_from_path = e.Path.Substring(e.Path.LastIndexOf('/') + 1);
                
                JsonNode? json = JsonNode.Parse(e.Payload);
                if (json != null)
                {
                    // create user object
                    await User.Update(
                        username_from_path,
                        (string?)json["username"] ?? username_from_path,
                        (string?)json["password"],
                        (string?)json["name"],
                        (string?)json["email"]
                        );

                    status = HttpStatusCodes.OK;
                    reply = new JsonObject()
                    {
                        ["success"] = true,
                        ["message"] = "User updated."
                    };
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
    }
}
