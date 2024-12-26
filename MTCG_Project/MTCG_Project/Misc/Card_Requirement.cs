using static MTCG_Project.Misc.Enums;

namespace MTCG_Project.Misc;

public struct Card_Requirement(Cardtype cardtype, Element element, Monster monster, int minDamage, int maxDamage)
{
    public readonly Cardtype cardtype = cardtype;
    public readonly Element element = element;
    public readonly Monster monster = monster;
    public readonly int minDamage = minDamage;
    public readonly int maxDamage = maxDamage;
}

