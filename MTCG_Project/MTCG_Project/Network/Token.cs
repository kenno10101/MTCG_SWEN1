using MTCG_Project.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG_Project.Network
{
    public static class Token
    {
        private const string _ALPHABET = "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        internal static Dictionary<string, User> _Tokens = new();

        internal static string _CreateTokenFor(User user)
        {
            string rval = string.Empty;
            Random rnd = new();

            for (int i = 0; i < 24; i++)
            {
                rval += _ALPHABET[rnd.Next(0, 62)];
            }

            _Tokens.Add(rval, user);

            return rval;
        }

        public static async Task<(bool Success, User? User)> Authenticate_Token(string token)
        {
            if (Program.ALLOW_DEBUG_TOKEN && token.EndsWith("-debug"))
            {
                string username_debug_token = token.Split('-')[0];
                User? user = await User.Get(username_debug_token);

                return ((user != null), user);
            }

            if (_Tokens.ContainsKey(token))
            {
                return (true, _Tokens[token]);
            }

            return (false, null);
        }

        public static async Task<(bool Success, User? User)> Authenticate_Request(HttpSvrEventArgs e)
        {
            foreach (HttpHeader i in e.Headers)
            {
                if (i.Name == "Authorization")
                {
                    if (i.Value[..7] == "Bearer ")
                    {
                        return await Authenticate_Token(i.Value[7..].Trim());
                    }
                    break;
                }
            }

            return (false, null);
        }
    }
}
