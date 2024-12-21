using MTCG_Project.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG_Project.Repositories;

namespace MTCG_Project.Network
{
    public static class Token
    {
        private const string _ALPHABET = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";


        internal static async Task<string> _CreateTokenFor(User user)
        {
            try 
            {

                string rval = string.Empty;
                Random rnd = new();

                for (int i = 0; i < 24; i++)
                {
                    rval += _ALPHABET[rnd.Next(0, 62)];
                }

                await TokenRepository.Add(rval, user);

                    return rval;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<(bool Success, User? User)> Authenticate_Token(string token)
        {
            {
                string username = "";

                if (Program.ALLOW_DEBUG_TOKEN && token.EndsWith("-debug"))
                {
                    username = token.Split('-')[0];
                }

                return (await TokenRepository.HasSession(username), await User.Get(username));
            }
        }

        public static async Task<(bool Success, User? User)> Authenticate_Request(HttpSvrEventArgs e)
        {

            string username_from_path = e.Path.Substring(e.Path.LastIndexOf('/') + 1);

            foreach (HttpHeader i in e.Headers)
            {
                if (i.Name != "Authorization")
                {
                    continue;
                }
                if (i.Value[..7] != "Bearer ")
                {
                    break;
                }
                string username_from_token = i.Value[7..].Trim();
                if (username_from_path != username_from_token)
                {
                    break;
                }
                return await Authenticate_Token(i.Value[7..].Trim());
            }

            return (false, null);
        }
    }
}
