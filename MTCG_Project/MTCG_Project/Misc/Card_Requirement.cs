using MTCG_Project.Interfaces;
using MTCG_Project.Models.Card;
using static MTCG_Project.Misc.Enums;

namespace MTCG_Project.Misc;

public class Card_Requirement(Cardtype cardtype, Element element, Monster monster, int minDamage, int maxDamage)
{
    public readonly Cardtype cardtype = cardtype;
    public readonly Element element = element;
    public readonly Monster monster = monster;
    public readonly int minDamage = minDamage;
    public readonly int maxDamage = maxDamage;

    public bool meetsRequirement(ICard card)
    {
        // check cardtype
        if (!((card is Monster_Card && cardtype == Cardtype.Monstercard) ||
             (card is Spell_Card && cardtype == Cardtype.Spellcard)))
        {
            return false;
        }

        // check element
        if (card.Element != element)
        {
            return false;
        }

        // check monstertype
        if (card is Monster_Card monster_card && monster != Monster.Null)
        {
            if (monster_card.Monster != monster)
            {
                return false;
            }
        }

        // check damage
        if (card.Damage < minDamage || card.Damage > maxDamage)
        {
            return false;
        }
        
        return true;
    }
}

