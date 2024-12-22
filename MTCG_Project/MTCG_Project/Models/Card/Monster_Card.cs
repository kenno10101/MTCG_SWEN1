using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG_Project.Interfaces;
using static MTCG_Project.Misc.Enums;

namespace MTCG_Project.Models.Card
{
    internal class Monster_Card : ICard
    {
        public string Name { get; private set; }
        public int Damage { get; private set; }
        public Element Element { get; private set; }
        public Monster Monster { get; private set; }

        public Monster_Card(string name, int damage, Element element, Monster monster)
        {
            Name = name;
            Damage = damage;
            Element = element;
            Monster = monster;
        }
        public Monster? GetMonsterType() => Monster;

    }
}
