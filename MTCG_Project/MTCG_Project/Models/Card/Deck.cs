using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG_Project.Interfaces;

namespace MTCG_Project.Models.Card
{
    internal class Deck
    {
        public List<ICard> _cards { get; private set; }
    }
}
