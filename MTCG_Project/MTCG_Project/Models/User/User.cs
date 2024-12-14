using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MTCG_Project.Exceptions;
using MTCG_Project.Interfaces;
using MTCG_Project.Models.Card;
using MTCG_Project.Network;
using MTCG_Project.Repository;
using Npgsql;

namespace MTCG_Project.Models.User
{
    public sealed class User
    {
        // TO BE REPLACED WITH DB, temporary in memory DB
        private static Dictionary<string, User> _Users = new();

        /// <summary>Gets the user name.</summary>
        public string UserName
        {
            get; private set;
        } = string.Empty;

        public string Password
        {
            get; private set;
        } = string.Empty;


        /// <summary>Gets or sets the user's full name.</summary>
        public string FullName
        {
            get; set;
        } = string.Empty;


        /// <summary>Gets or sets the user's e-mail address.</summary>
        public string EMail
        {
            get; set;
        } = string.Empty;

        private readonly Deck _deck;

        public List<ICard> _stack { get; private set; }


        private User()
        { }

        public void Save(string token)
        {
            (bool Success, User? User) auth = Token.Authenticate_Token(token);
            if (!auth.Success)
            {
                throw new AuthenticationException("Not authenticated.");
            }
            // if user uses another user's token
            if (auth.User!.UserName != UserName)
            {
                throw new SecurityException("Trying to change other user's data.");
            }

            // Save data.
        }

        public static async Task Create(string userName, string password, string fullName = "", string eMail = "")
        {
            if (_Users.ContainsKey(userName))
            {
                return;
            }

            User user = new()
            {
                UserName = userName,
                FullName = fullName,
                Password = password,
                EMail = eMail
            };

            // TO BE REPLACED WITH DB, add to in memory DB
            _Users.Add(user.UserName, user);
            
            var connString = "Host=localhost;Port=5431;Username=kenn;Password=kenn1234;Database=MTCG_project";
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            
            UserRepository user_db = new UserRepository(conn);
            await user_db.Create(user);
        }

        public static User Get(string userName)
        {
            if (!_Users.ContainsKey(userName))
            {
                return null;
            }

            User User_to_edit = _Users[userName];

            return User_to_edit;
        }

        public async static Task Update (string old_userName, string new_userName, string password, string fullName, string eMail)
        {
            User updated_user;
            updated_user = new()
            {
                UserName = new_userName,
                Password = password,
                FullName = fullName,
                EMail = eMail
            };
            var connString = "Host=localhost;Port=5431;Username=kenn;Password=kenn1234;Database=MTCG_project";
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            UserRepository user_db = new UserRepository(conn);
            await user_db.Update(updated_user, old_userName);
        }

        public static (bool Success, string Token) Logon(string userName, string password)
        {
            if (!_Users.ContainsKey(userName))
            {
                return (false, null);
            }
            if (_Users[userName].Password != password)
            {
                return (false, string.Empty);
            }

            return (true, Token._CreateTokenFor(_Users[userName]));
            
        }
    }
}
