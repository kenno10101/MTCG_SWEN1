using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cardbase = MTCG_Project.Classes.Card.Card;

namespace MTCG_Project.Classes.User
{
    internal class User
    {
        private readonly string _username;
        private readonly string _password;
        public List<Cardbase> _stack { get; private set; }

        public User(string username, string password)
        {
            _username = username;
            _password = password;
            _stack = new List<Cardbase>();
        }
    }
}
