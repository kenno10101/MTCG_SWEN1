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
                return _Login(e).GetAwaiter().GetResult();
            }
          
            return false;
        }

        private static async Task<bool> _Login(HttpSvrEventArgs e)
        {
            JsonObject? reply = new JsonObject() { ["success"] = false, ["message"] = "Invalid request."};
            int status = HttpStatusCodes.BAD_REQUEST;

            try
            {
                JsonNode? json = JsonNode.Parse(e.Payload);
                if (json != null)
                {
                    (bool success, string token) login = await User.Logon((string)json["username"]!, (string)json["password"]!);

                    
                    if (!login.success)
                    {
                        status = HttpStatusCodes.UNAUTHORIZED;
                        throw new UserException("Login failed");
                    }

                    reply = new JsonObject() { ["success"] = true, ["message"] = "Login success", ["token"] = login.token };
                    status = HttpStatusCodes.OK;
            }
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
