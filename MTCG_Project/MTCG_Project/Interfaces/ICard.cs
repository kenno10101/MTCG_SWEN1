using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG_Project.Misc;
using MTCG_Project.Repositories;
using static MTCG_Project.Misc.Enums;

namespace MTCG_Project.Interfaces
{
    public interface ICard
    {
        string Name { get; }
        int Damage { get; }
        Element Element { get; }

        public static async Task<ICard> getCard(string cardname)
        {
            try
            {
                return await CardRepository.GetCard(cardname);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
