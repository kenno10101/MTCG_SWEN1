using MTCG_Project.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using MTCG_Project.Interfaces;
using MTCG_Project.Models.User;
using MTCG_Project.Exceptions;
using System.Net;

namespace MTCG_Project.Handler
{
    public class SessionHandler : Handler, IHandler
    {
        public override bool Handle(HttpSvrEventArgs e)
        {
            bool is_LoginRequest = (e.Path.TrimEnd('/', ' ', '\t') == "/sessions") && (e.Method == "POST");
            if (is_LoginRequest)
            {                                                                   // POST /sessions will create a user object
                return _Login(e);
            }
          
            return false;
        }

        private static bool _Login(HttpSvrEventArgs e)
        {
            JsonObject? reply = new JsonObject() { ["success"] = false, ["message"] = "Invalid request."};
            int status = HttpStatusCodes.BAD_REQUEST;

            try
            {
                JsonNode? json = JsonNode.Parse(e.Payload);
                if (json != null)
                {
                    (bool success, string token) login = User.Logon((string)json["username"]!, (string)json["password"]!);

                    
                    if (login.success)
                    {
                        reply = new JsonObject() { ["success"] = true, ["message"] = "Login success", ["token"] = login.token};
                        status = HttpStatusCodes.OK;
                    }
                    else if (login.token == null)
                    {
                        status = HttpStatusCodes.NOT_FOUND;
                        throw new UserException("User doesn't exist.");
                    }
                    else
                    {
                        status = HttpStatusCodes.UNAUTHORIZED;
                        throw new UserException("Login failed");
                    }
                }
            }
            catch (UserException ex)
            {
                reply = new JsonObject() { ["success"] = false, ["message"] = ex.Message };
            }
            catch (Exception)
            {
                reply = new JsonObject() { ["success"] = false, ["message"] = "Unexpected error."};
            }

            e.Reply(status, reply?.ToJsonString());
            return true;
        }

        
    }
}
