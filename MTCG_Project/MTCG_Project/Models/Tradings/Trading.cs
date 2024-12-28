using MTCG_Project.Interfaces;
using MTCG_Project.Misc;
using MTCG_Project.Models.Users;
using MTCG_Project.Repositories;
using static MTCG_Project.Misc.Enums;

namespace MTCG_Project.Models.Tradings;

public class Trading
{
    public readonly int id;
    public readonly String trade_creator;
    public readonly String trade_acceptor;
    public readonly Tradingstatus trading_status;
    public readonly ICard card_offer;
    public readonly Card_Requirement card_requirement;
    public readonly ICard card_received;

    public Trading(int id, string tradeCreator, string tradeAcceptor, Tradingstatus tradingStatus, ICard cardOffer, Card_Requirement cardRequirement, ICard cardReceived)
    {
        this.id = id;
        trade_creator = tradeCreator;
        trade_acceptor = tradeAcceptor;
        trading_status = tradingStatus;
        card_offer = cardOffer;
        card_requirement = cardRequirement;
        card_received = cardReceived;
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

    public static async Task<Trading> Get(int trade_id)
    {
        try
        {
            return await TradingRepository.Get(trade_id);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public static async Task AcceptTrade(int trade_id, string username, string cardname)
    {
        try
        {
            Trading trade = await TradingRepository.Get(trade_id);
            if (trade.trade_creator == username)
            {
                throw new Exception("Cannot trade with yourself.");
            }

            ICard card_requirement = await ICard.getCard(cardname);
            if (!trade.card_requirement.meetsRequirement(card_requirement))
            {
                throw new Exception("Card does not meet requirements.");
            }
            await TradingRepository.Trade(trade_id, username, cardname);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    
    public static async Task DeleteTrade(int trade_id)
    {
        try
        {
            await TradingRepository.Delete(trade_id);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}