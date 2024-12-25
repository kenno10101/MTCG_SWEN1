using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG_Project.Repositories;

namespace MTCG_Project.Models.Stats
{
    public class Scoreboard
    {
        public static async Task<List<(string, Stat)>> Get()
        {
            try
            {
                return await UserRepository.GetScoreboard();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
