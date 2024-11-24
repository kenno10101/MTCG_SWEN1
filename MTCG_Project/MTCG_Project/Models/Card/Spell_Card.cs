using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG_Project.Interfaces;
using MTCG_Project.Misc;
using static MTCG_Project.Misc.Enums;

namespace MTCG_Project.Models.Card
{
    internal class Spell_Card : ICard
    {
        public string Name { get; private set; }
        public int Damage { get; private set; }
        public Element Element { get; private set; }

        public Spell_Card(string name, int damage, Element element)
        {
            Name = name;
            Damage = damage;
            Element = element;
        }
        public Monster? GetMonsterType() => null;
    }
}
