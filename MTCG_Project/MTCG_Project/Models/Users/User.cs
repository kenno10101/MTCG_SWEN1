﻿using System;
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
using MTCG_Project.Models.Stats;
using MTCG_Project.Network;
using MTCG_Project.Repositories;
using Npgsql;
using MTCG_Project.Handler;
using MTCG_Project.Misc;

namespace MTCG_Project.Models.Users
{
    public sealed class User
    {

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
        
        public int Coins
        {
            get; set;
        }

        public readonly Deck _deck;

        public readonly Stack _stack;

        public readonly Stat _stats;

        private User()
        { }

        public User(string username, string password, string fullname, string email, int coins = 20, Deck deck = null, Stack stack = null, Stat stats = null)
        {
            UserName = username;
            Password = password;
            FullName = fullname;
            EMail = email;
            Coins = coins;
            _deck = deck;
            _stack = stack;
            _stats = stats;
        }

        // unused
        public async Task Save(string token)
        {
            (bool Success, User? User) auth = await Token.Authenticate_Token(token);
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
            try
            {
                User user = new()
                {
                    UserName = userName,
                    FullName = fullName,
                    Password = password,
                    EMail = eMail
                };

                await UserRepository.Create(user);
                await Deck.CreateInit(user.UserName);
                await Stat.Create(user.UserName);
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message);
            }
        }

        public static async Task<User> Get(string userName)
        {
            try
            {
                User user = await UserRepository.Get(userName);
                return user;
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message);
            }
        }

        public static async Task Update (string old_userName, string new_userName, string password, string fullName, string eMail)
        {
            try {
                User updated_user = new()
                {
                    UserName = new_userName,
                    Password = password,
                    FullName = fullName,
                    EMail = eMail
                };

                await UserRepository.Update(updated_user, old_userName);
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message);
            }
        }
        
        public static async Task<(bool Success, string Token)> Logon(string userName, string password)
        {
            try
            {
                User user = await Get(userName);
                if (!PasswordHasher.VerifyPassword(password, user.Password))
                {
                    return (false, string.Empty);
                }
                return (true, await Token._CreateTokenFor(user));
            }
            catch (Exception ex)
            {
                throw new UserException(ex.Message);
            }
        }
    }
}
