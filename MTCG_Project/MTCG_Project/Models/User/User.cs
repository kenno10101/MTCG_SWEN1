using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG_Project.Interfaces;
using MTCG_Project.Models.Card;

namespace MTCG_Project.Models.User
{
    internal class User
    {
        private readonly string _username;
        private readonly string _password;
        private readonly Deck _deck;

        public List<ICard> _stack { get; private set; }

        public User(string username, string password)
        {
            _username = username;
            _password = password;
            _deck = new Deck();
            _stack = new List<ICard>();
        }
    }
}
