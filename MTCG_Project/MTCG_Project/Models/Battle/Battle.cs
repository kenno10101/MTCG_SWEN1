using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG_Project.Interfaces;

namespace MTCG_Project.Models.Battle
{
    internal class Battle
    {
        public bool Fight(ICard card_1, ICard card_2)
        {

            return card_1.Damage > card_2.Damage;
        }
    }
}
