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
            bool is_QueryUserRequest = e.Path.StartsWith("/users/") && (e.Method == "GET");
            bool is_UpdateUserRequest = e.Path.StartsWith("/users/") && (e.Method == "PUT");

            if (is_CreateUserRequest)
            {                                                                   // POST /users will create a user object
                return _CreateUser(e);
            }
            else if (is_QueryUserRequest)        // GET /users/UserName will query a user
            {
                return _QueryUser(e);
            }
            else if (is_UpdateUserRequest)
            {
                return _UpdateUser(e);
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
                    bool user_created = User.Create(
                        (string) json["username"]!,
                        (string)json["password"]!,
                        (string?) json["name"] ?? "Max Mustermann",
                        (string?) json["email"] ?? "test@test.at");
                    if (!user_created)
                    {
                        status = HttpStatusCodes.NOT_UNIQUE;
                        throw new UserException("User already exists.");
                    }
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
            JsonObject? reply = new JsonObject() { ["success"] = false, ["message"] = "Invalid request."};
            int status = HttpStatusCodes.BAD_REQUEST;

            try
            {
                (bool Success, User? User) ses = Token.Authenticate_Request(e);

                if (!ses.Success)
                {
                    status = HttpStatusCodes.UNAUTHORIZED;
                    throw new Exception("Unauthorized");
                }

                string username_from_path = e.Path.Substring(e.Path.LastIndexOf('/') + 1);
                User? user = User.Get(username_from_path);

                if(user == null)
                {
                    status = HttpStatusCodes.NOT_FOUND;
                    throw new UserException("User doesn't exist.");
                }

                JsonObject? userResponse = new JsonObject(){
                    ["user_UserName"] = user.UserName,
                    ["user_Password"] = user.Password,
                    ["user_FullName"] = user.FullName,
                    ["user_EMail"] = user.EMail
                };
                reply.Add("user", userResponse);
                
                status = HttpStatusCodes.OK;
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

        private static bool _UpdateUser(HttpSvrEventArgs e)
        {

            JsonObject? reply = new JsonObject() { ["success"] = false, ["message"] = "Invalid request." };
            int status = HttpStatusCodes.BAD_REQUEST;

            try
            {
                (bool Success, User? User) ses = Token.Authenticate_Request(e);

                if (!ses.Success)
                {
                    status = HttpStatusCodes.UNAUTHORIZED;
                    throw new Exception("Unauthorized");
                }

                string username_from_path = e.Path.Substring(e.Path.LastIndexOf('/') + 1);
                User? user_to_edit = User.Get(username_from_path);

                if (user_to_edit == null)
                {
                    status = HttpStatusCodes.NOT_FOUND;
                    throw new UserException("User doesn't exist.");
                }

                JsonNode? json = JsonNode.Parse(e.Payload);
                if (json != null)
                {
                    // create user object
                    bool user_updated = User.Update(
                        user_to_edit,
                        (string?)json["username"] ?? user_to_edit.UserName,
                        (string?)json["password"] ?? user_to_edit.Password,
                        (string?)json["name"] ?? user_to_edit.FullName,
                        (string?)json["email"] ?? user_to_edit.EMail
                        );

                    if (!user_updated)
                    {
                        status = HttpStatusCodes.NOT_FOUND;
                        throw new UserException("User doesn't exist.");
                    }

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
