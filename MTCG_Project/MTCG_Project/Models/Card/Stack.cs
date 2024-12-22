using MTCG_Project.Interfaces;
using MTCG_Project.Repositories;

namespace MTCG_Project.Models.Card;

public class Stack
{
    public List<ICard> cards;

    public Stack()
    {
        cards = new List<ICard>();
    }

    public static async Task<Stack> GetStack(string username)
    {
        try
        {
            return await CardRepository.GetStack(username);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}