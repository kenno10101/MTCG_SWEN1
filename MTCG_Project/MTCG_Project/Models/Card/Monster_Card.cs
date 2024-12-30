using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG_Project.Interfaces;
using static MTCG_Project.Misc.Enums;

namespace MTCG_Project.Models.Card
{
    public class Monster_Card : ICard
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

        public override bool Equals(object obj)
        {
            if (obj is not Monster_Card otherCard)
                return false;

            return Name == otherCard.Name &&
                   Element == otherCard.Element &&
                   Damage == otherCard.Damage &&
                   Monster == otherCard.Monster;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Element, Damage, Monster);
        }
    }
}
