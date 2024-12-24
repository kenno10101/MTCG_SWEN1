using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG_Project.Interfaces;
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

        public static async Task Create(string username, string[] cards = null)
        {
            try
            {
                // cards should be cards that can only come from stack
                await CardRepository.CreateDeck(username, cards);
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

        public static async Task Update(string username, string[] cards = null)
        {
            try
            {
                // cards should be cards that can only come from stack
                await CardRepository.UpdateDeck(username, cards);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
