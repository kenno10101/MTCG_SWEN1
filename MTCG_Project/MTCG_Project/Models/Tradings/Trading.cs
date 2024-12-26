using MTCG_Project.Interfaces;
using MTCG_Project.Misc;
using MTCG_Project.Models.Users;
using MTCG_Project.Repositories;
using static MTCG_Project.Misc.Enums;

namespace MTCG_Project.Models.Tradings;

public class Trading
{
    
    public String trade_creator;
    public String trade_acceptor;
    public Tradingstatus trading_status;
    public ICard card_offer;
    public Card_Requirement card_requirement;

    public Trading(string tradeCreator, string tradeAcceptor, Tradingstatus tradingStatus, ICard cardOffer, Card_Requirement cardRequirement)
    {
        trade_creator = tradeCreator;
        trade_acceptor = tradeAcceptor;
        trading_status = tradingStatus;
        card_offer = cardOffer;
        card_requirement = cardRequirement;
    }

    public static async Task Create(string username, ICard card_offer, Card_Requirement card_request)
    {
        try
        {
            if (card_request.element != null && card_request.minDamage != null && card_request.maxDamage != null && card_request.minDamage > card_request.maxDamage)
            {
                throw new Exception("Requirements incomplete.");
            }
            await TradingRepository.Create(username, card_offer, card_request);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    
    public static async Task<List<Trading>> GetAll()
    {
        try
        {
            return await TradingRepository.GetAll();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    
    // public static async Task Trade(int trade_id)
    // {
    //     try
    //     {
    //         await TradingRepository.Trade(trade_id);
    //     }
    //     catch (Exception e)
    //     {
    //         throw new Exception(e.Message);
    //     }
    // }
    //
    // public static async Task Delete(int trade_id)
    // {
    //     try
    //     {
    //         await TradingRepository.Delete(trade_id);
    //     }
    //     catch (Exception e)
    //     {
    //         throw new Exception(e.Message);
    //     }
    // }
}