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
  
    public class Spell_Card : ICard
    {
        public string Name { get; private set; }
        public int Damage { get; private set; }
        public Element Element { get; private set; }

        public Spell_Card (string name, int damage, Element element)
        {
            Name = name;
            Damage = damage;
            Element = element;
        }
        
        public override bool Equals(object obj)
        {
            if (obj is not Spell_Card otherCard)
                return false;

            return Name == otherCard.Name &&
                   Element == otherCard.Element &&
                   Damage == otherCard.Damage;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Element, Damage);
        }
    }
}
