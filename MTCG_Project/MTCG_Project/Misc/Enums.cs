using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG_Project.Misc
{
    public class Enums
    {
        public enum Cardtype
        {
            Spellcard,
            Monstercard
        }
        public enum Monster
        {
            Null,
            Goblin,
            Dragon,
            Wizard,
            Ork,
            Knight,
            Kraken,
            Elf
        }
        public enum Element
        {
            Water,
            Fire,
            Normal
        }
        public enum Battleresult
        {
            Win,
            Loss,
            Draw
        }

        public enum Tradingstatus
        {
            Open,
            Done,
            Deleted
        }
    }
}
