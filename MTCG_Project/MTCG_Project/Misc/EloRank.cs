using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MTCG_Project.Misc.Enums;

namespace MTCG_Project.Misc
{
    public class EloRank
    {
        public static Rank getRankByElo(int elo)
        {

            if (elo >= 110 && elo < 125)
            {
                return Rank.Silver;
            }
            if (elo >= 125 && elo < 140)
            {
                return Rank.Gold;
            }
            if (elo >= 140 && elo < 155)
            {
                return Rank.Platinum;
            }
            if (elo >= 155 && elo < 170)
            {
                return Rank.Diamond;
            }
            if (elo >= 170)
            {
                return Rank.Master;
            }

            return Rank.Bronze;
        }
    }
}
