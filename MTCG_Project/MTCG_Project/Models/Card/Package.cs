using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG_Project.Interfaces;
using MTCG_Project.Repositories;

namespace MTCG_Project.Models.Card
{
    public class Package
    {

        public static async Task BuyPackage(string username)
        {
            try
            {
                await CardRepository.BuyPackage(username);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        
        public static async Task CreatePackage(string[] cards)
        {
            try
            {
                await CardRepository.CreatePackage(cards);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        
    }
}
