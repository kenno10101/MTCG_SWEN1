using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG_Project.Interfaces;
using MTCG_Project.Models.Users;
using MTCG_Project.Repositories;

namespace MTCG_Project.Models.Card
{

    /*
     CREATE TABLE deck (
        id SERIAL PRIMARY KEY,
        username VARCHAR(255) NOT NULL UNIQUE,
        card_1_name VARCHAR(255),
        card_2_name VARCHAR(255),
        card_3_name VARCHAR(255),
        card_4_name VARCHAR(255),
        CONSTRAINT deck_username_fkey FOREIGN KEY (username) REFERENCES users (username),
        CONSTRAINT deck_card_1_name_fkey FOREIGN KEY (card_1_name) REFERENCES cards (name),
        CONSTRAINT deck_card_2_name_fkey FOREIGN KEY (card_2_name) REFERENCES cards (name),
        CONSTRAINT deck_card_3_name_fkey FOREIGN KEY (card_3_name) REFERENCES cards (name),
        CONSTRAINT deck_card_4_name_fkey FOREIGN KEY (card_4_name) REFERENCES cards (name)
    )
     */
    public class Deck
    {
        public List<ICard> cards { get; set; }

        public Deck()
        {
            cards = new List<ICard>();
        }

        public static async Task CreateInit(string username)
        {
            try
            {
                await CardRepository.CreateDeckEmpty(username);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<Deck> Get(string username)
        {
            try
            {
                return await CardRepository.GetDeck(username);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task Update(string username, string[] cards)
        {
            try
            {
                if (!await checkIfCardsInStack(username, cards))
                {
                    throw new Exception("Error: trying to add card that doesn't belong to your stack.");
                }
                await CardRepository.UpdateDeck(username, cards);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<bool> checkIfCardsInStack(string username, string[] cards)
        {
            User user = await User.Get(username);
            if (cards != null)
            {
                foreach (var deck_card_name in cards)
                {
                    ICard deck_card = await ICard.getCard(deck_card_name);
                    if (!user._stack.cards.Contains(deck_card))
                    {
                        return false;
                    }
                    // prevents using same card twice in deck
                    user._stack.cards.Remove(deck_card);
                }
            }
            return true;
        }
        
        
    }
}
