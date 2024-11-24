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

namespace MTCG_Project.Handler
{
    public class UserHandler : Handler, IHandler
    {
        public override bool Handle(HttpSvrEventArgs e)
        {
            bool is_CreateUserRequest = (e.Path.TrimEnd('/', ' ', '\t') == "/users") && (e.Method == "POST");
            bool is_QueryUsersRequest = e.Path.StartsWith("/users") && (e.Method == "GET");

            if (is_CreateUserRequest)
            {                                                                   // POST /users will create a user object
                return _CreateUser(e);
            }
            else if (is_QueryUsersRequest)        // GET /users/UserName will query a user
            {
                return _QueryUser(e);
            }

            return false;
        }   

        private static bool _CreateUser(HttpSvrEventArgs e)
        {

            JsonObject? reply = new JsonObject() { ["success"] = false, ["message"] = "Invalid request." };
            int status = HttpStatusCodes.BAD_REQUEST;

            try
            {
                JsonNode? json = JsonNode.Parse(e.Payload);
                if (json != null)
                {
                    // create user object
                    User.Create(
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
            catch (Exception)
            {
                reply = new JsonObject() { ["success"] = false, ["message"] = "Unexpected error." };
            }

            e.Reply(status, reply?.ToJsonString());
            return true;
        }

        private static bool _QueryUser(HttpSvrEventArgs e)
        {
            JsonObject? reply = new JsonObject() { ["success"] = false, ["message"] = "Invalid request." };
            int status = HttpStatusCodes.BAD_REQUEST;

            try
            {
                if (User._Users.Count < 1)
                {
                    throw new Exception("No users registered.");
                }

                int i = 0;
                foreach (KeyValuePair<string, User> user in User._Users)
                {
                    JsonObject? userResponse = new JsonObject(){
                        ["user_UserName"] = user.Value.UserName,
                        ["user_Password"] = user.Value.Password,
                        ["user_FullName"] = user.Value.FullName,
                        ["user_EMail"] = user.Value.EMail
                    };
                    reply.Add("user" + (i++), userResponse);
                }
                status = HttpStatusCodes.OK;
                reply["success"] = true;
                reply["message"] = "Query success";
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
