using static MTCG_Project.Misc.Enums;
using MTCG_Project.Repositories;

namespace MTCG_Project.Models.Stats;

public class Stat
{
    public readonly int Battles_played = 0;
    public readonly int Wins = 0;
    public readonly int Losses = 0;
    public readonly int Draws = 0;
    public readonly int Elo = 100;
    public readonly Rank Rank;

    public Stat(int battles_played, int wins, int losses, int draws, int elo, Rank rank)
    {
        Battles_played = battles_played;
        Wins = wins;
        Losses = losses;
        Draws = draws;
        Elo = elo;
        Rank = rank;
    }

    public static async Task Create(string username)
    {
        try
        {
            await UserRepository.CreateStats(username);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    
    public static async Task<Stat> Get(string username)
    {
        try
        {
            return await UserRepository.GetStats(username);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public static async Task Update(string username1, string username2, Battleresult result)
    {
        try
        {   
            await UserRepository.UpdateStats(username1, username2, result);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}