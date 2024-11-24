using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG_Project.Misc;
using static MTCG_Project.Misc.Enums;

namespace MTCG_Project.Interfaces
{
    public interface ICard
    {
        string Name { get; }
        int Damage { get; }
        Element Element { get; }

        Monster? GetMonsterType();
    }
}
