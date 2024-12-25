﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using MTCG_Project.Handler;
using MTCG_Project.Interfaces;
using MTCG_Project.Misc;
using MTCG_Project.Models.Card;
using MTCG_Project.Models.Users;
using MTCG_Project.Models.Stats;
using MTCG_Project.Network;


namespace MTCG_Project.Models.Battle
{
    public class Battle
    {
        public string BattleLog = string.Empty;

        private static readonly double[,] Effectiveness_Matrix_Spell =
        {
            { 1, 2, 0.5 }, // water
            { 0.5, 1, 2 }, // fire
            { 2, 0.5, 1 }  // normal
        };

        
        private static readonly double[,] Effectiveness_Matrix_Monster =
        {
            { 1, 0, 1, 1, 1, 1, 1 }, // goblin
            { 1, 1, 1, 1, 1, 1, 0 }, // dragon
            { 1, 1, 1, 1, 1, 1, 1 }, // wizard
            { 1, 1, 0, 1, 1, 1, 1 }, // ork
            { 1, 1, 1, 1, 1, 1, 1 }, // knight
            { 1, 1, 1, 1, 1, 1, 1 }, // kraken
            { 1, 1, 1, 1, 1, 1, 1 }  // fireelf
        };

        public async Task Fight(User user_1, User user_2)
        {
            Deck deck_1 = user_1._deck;
            Deck deck_2 = user_2._deck;

            int points_1 = 0, points_2 = 0;
            bool player_1_winner = false, player_2_winner = false;

            Random rnd = new();
            var card_1 = deck_1.cards[rnd.Next(0, deck_1.cards.Count)];
            BattleLog += $"Player 1 selected card \"{card_1.Name}\" for this round.\n";
            
            var card_2 = deck_2.cards[rnd.Next(0, deck_2.cards.Count)];
            BattleLog += $"Player 2 selected card \"{card_2.Name}\" for this round.\n";

            int num_rounds = 0;
            for (int i = 0; i < 100; i++)
            {
                num_rounds = i + 1;
                // pick new random card of previous round's loser
                if (player_1_winner)
                {
                    card_2 = deck_2.cards[rnd.Next(0, deck_2.cards.Count)];
                    BattleLog += $"Player 2 selected card \"{card_2.Name}\" for this round.\n";
                    
                    player_1_winner = false;
                }
                else if (player_2_winner)
                {
                    card_1 = deck_1.cards[rnd.Next(0, deck_1.cards.Count)];
                    BattleLog += $"Player 1 selected card \"{card_1.Name}\" for this round.\n";
                    
                    player_2_winner = false;
                }


                // add point to round winner and add opponent's card to winner's deck
                if (CardsFight(card_1, card_2) == card_1)
                {
                    points_1++;
                    BattleLog += $"Player 1's \"{card_1.Name}\" won this round.\n";
                    
                    deck_2.cards.Remove(card_2);
                    deck_1.cards.Add(card_2);
                    player_1_winner = true;
                }
                else
                {
                    points_2++;
                    BattleLog += $"Player 2's \"{card_2.Name}\" won this round.\n";

                    deck_1.cards.Remove(card_1);
                    deck_2.cards.Add(card_1);
                    player_2_winner = true;
                }

                BattleLog += "=== END OF ROUND ===\n";
                if (deck_1.cards.Count == 0 || deck_2.cards.Count == 0)
                {
                    BattleLog += "The Battle has finished.\n";
                    break;
                }
            }

            if (points_1 == points_2)
            {
                await Stat.Update(user_1.UserName, user_2.UserName, "draw");
                BattleLog += "The battle has ended in a draw.\n";
            }
            else if (points_1 > points_2)
            {
                await Stat.Update(user_1.UserName, user_2.UserName, "win");
                BattleLog += "Player 1 won the battle.\n";
            }
            else
            {
                await Stat.Update(user_1.UserName, user_2.UserName, "loss");
                BattleLog += "Player 2 won the battle.\n";
            }
            BattleLog += $"Player 1 Points: {points_1}\n";
            BattleLog += $"Player 2 Points: {points_2}\n";
            BattleLog += $"Number of Rounds: {num_rounds}\n";

        }

        private ICard CardsFight(ICard card_1, ICard card_2)
        {
            double card_1_battle_damage = 0;
            double card_2_battle_damage = 0;

            bool monster_monster_battle = card_1 is Monster_Card && card_2 is Monster_Card;
            bool monster_spell_battle = card_1 is Monster_Card && card_2 is Spell_Card;
            bool spell_spell_battle = card_1 is Spell_Card && card_2 is Spell_Card;
            bool spell_monster_battle = card_1 is Spell_Card && card_2 is Monster_Card;

            if (monster_monster_battle)
            {
                Monster_Card monster_card_1 = (Monster_Card)card_1;
                Monster_Card monster_card_2 = (Monster_Card)card_2;

                card_1_battle_damage = monster_card_1.Damage *
                                       Effectiveness_Matrix_Monster[(int)monster_card_1.Monster,
                                           (int)monster_card_2.Monster];
                card_2_battle_damage = monster_card_2.Damage *
                                       Effectiveness_Matrix_Monster[(int)monster_card_2.Monster,
                                           (int)monster_card_1.Monster];
            }
            else if (monster_spell_battle)
            {
                Monster_Card monster_card_1 = (Monster_Card)card_1;

                if (monster_card_1.Monster == Enums.Monster.Kraken)
                {
                    return monster_card_1;
                }

                if (monster_card_1.Monster == Enums.Monster.Knight && card_2.Element == Enums.Element.Water)
                {
                    return card_2;
                }

                card_1_battle_damage = monster_card_1.Damage *
                                       Effectiveness_Matrix_Spell[(int)monster_card_1.Element, (int)card_2.Element];
                card_2_battle_damage = card_2.Damage *
                                       Effectiveness_Matrix_Spell[(int)card_2.Element, (int)monster_card_1.Element];
            }
            else if (spell_spell_battle)
            {
                card_1_battle_damage =
                    card_1.Damage * Effectiveness_Matrix_Spell[(int)card_1.Element, (int)card_2.Element];
                card_2_battle_damage =
                    card_2.Damage * Effectiveness_Matrix_Spell[(int)card_2.Element, (int)card_1.Element];
            }
            else if (spell_monster_battle)
            {
                Monster_Card monster_card_2 = (Monster_Card)card_2;

                if (monster_card_2.Monster == Enums.Monster.Kraken)
                {
                    return monster_card_2;
                }

                if (card_1.Element == Enums.Element.Water && monster_card_2.Monster == Enums.Monster.Knight)
                {
                    return card_1;
                }

                card_1_battle_damage = card_1.Damage *
                                       Effectiveness_Matrix_Spell[(int)card_1.Element, (int)monster_card_2.Element];
                card_2_battle_damage = monster_card_2.Damage *
                                       Effectiveness_Matrix_Spell[(int)monster_card_2.Element, (int)card_1.Element];
            }

            BattleLog += $"Player 1's \"{card_1.Name}\" dealt {card_1_battle_damage} Damage to Player 2's \"{card_2.Name}\"\n";
            BattleLog += $"Player 2's \"{card_2.Name}\" dealt {card_2_battle_damage} Damage to Player 1's \"{card_1.Name}\"\n";
            
            // choose random in case of draw
            if (card_1_battle_damage == card_2_battle_damage)
            {
                Random random_winner = new();
                List<ICard> cards = new List<ICard> { card_1, card_2 };

                BattleLog += "Both Player's cards deal an equal amount of damage and the round resulted in a draw. A round winner will be chosen randomly.\n";

                return cards[random_winner.Next(0, cards.Count)];
            }

            return card_1_battle_damage > card_2_battle_damage ? card_1 : card_2;
        }

        public async Task<JsonObject> JoinBattle(string username_1, string username_2)
        {
            User user_1 = await User.Get(username_1);
            User user_2 = await User.Get(username_2);
            this.BattleLog = string.Empty;

            await this.Fight(user_1, user_2);
            return BattleLogParser(this.BattleLog);
        }
        
        private static JsonObject BattleLogParser(string battleLog)
    {
        var rounds = new JsonArray();
        var lines = battleLog.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var currentRound = new JsonObject();
        var roundActions = new JsonArray();
        
        foreach (var line in lines)
        {
            if (line.StartsWith("Player 1 selected card") || line.StartsWith("Player 2 selected card"))
            {
                roundActions.Add(line);
            }
            else if (line.Contains("dealt"))
            {
                roundActions.Add(line);
            }
            else if (line.Contains("draw"))
            {
                roundActions.Add(line);
            }
            else if (line.Contains("won this round"))
            {
                currentRound["round"] = rounds.Count + 1;
                currentRound["actions"] = roundActions;
                currentRound["winner"] = line.Replace(" won this round.", "").Trim();
            }
            else if (line == "=== END OF ROUND ===")
            {
                if (currentRound.Count > 0)
                {
                    rounds.Add(currentRound);
                    currentRound = new JsonObject();
                    roundActions = new JsonArray();
                }
            }
            else if (line.StartsWith("The Battle has finished"))
            {
                break; // End of the parsing for rounds
            }
        }

        var summary = new JsonObject
        {
            ["player1Points"] = ExtractValueFromLine(lines[^3], "Player 1 Points: "),
            ["player2Points"] = ExtractValueFromLine(lines[^2], "Player 2 Points: "),
            ["numberOfRounds"] = ExtractValueFromLine(lines[^1], "Number of Rounds: ")
        };

        return new JsonObject
        {
            ["rounds"] = rounds,
            ["summary"] = summary
        };
    }

    private static int ExtractValueFromLine(string line, string prefix)
    {
        if (line.StartsWith(prefix))
        {
            return int.Parse(line.Replace(prefix, "").Trim());
        }
        return 0;
    }
    }
}