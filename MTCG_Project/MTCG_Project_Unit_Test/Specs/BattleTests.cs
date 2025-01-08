using MTCG_Project.Models.Users;
using MTCG_Project.Models.Card;
using MTCG_Project.Models.Battle;
using static MTCG_Project.Misc.Enums;
using MTCG_Project.Misc;
using MTCG_Project.Handler;
using MTCG_Project.Network;
using System.Reflection.Metadata;
using MTCG_Project.Exceptions;
using System.Data.Common;
using Npgsql;
using MTCG_Project.Interfaces;
using System.Text.Json.Nodes;
using MTCG_Project.Models.Stats;

namespace MTCG_Project_Unit_Test.Specs
{
    public class BattleTests
    {

        [SetUp]
        public async Task Setup()
        {
            DB_connection.connString = "Host=localhost;Port=5431;Username=kenn;Password=kenn1234;Database=MTCG_project_test";
            
            await using var conn = await DB_connection.connectDB();
            await using (var cmd = new NpgsqlCommand("DELETE FROM users", conn))
            {
                await cmd.ExecuteNonQueryAsync();
            }
        }

        [Test]
        public async Task RoundFight_1_Water_Fire()
        {
            // Arrange
            Battle battle = new Battle();
            Spell_Card watercard = new Spell_Card("Water", 100, Element.Water);
            Spell_Card firecard = new Spell_Card("Fire", 100, Element.Fire);

            // Act
            ICard winner = battle.CardsRoundFight(watercard, firecard);

            // Assert
            Assert.IsTrue(winner.Equals(watercard));
        }

        [Test]
        public async Task RoundFight_2_Normal_Fire()
        {
            // Arrange
            Battle battle = new Battle();
            Spell_Card normalcard = new Spell_Card("Normal", 100, Element.Normal);
            Spell_Card firecard = new Spell_Card("Fire", 100, Element.Fire);

            // Act
            ICard winner = battle.CardsRoundFight(normalcard, firecard);

            // Assert
            Assert.IsTrue(winner.Equals(firecard));
        }

        [Test]
        public async Task RoundFight_3_Normal_Water()
        {
            // Arrange
            Battle battle = new Battle();
            Spell_Card normalcard = new Spell_Card("Normal", 100, Element.Normal);
            Spell_Card watercard = new Spell_Card("Water", 100, Element.Water);

            // Act
            ICard winner = battle.CardsRoundFight(normalcard, watercard);

            // Assert
            Assert.IsTrue(winner.Equals(normalcard));
        }

        [Test]
        public async Task RoundFight_4_Knight_Water()
        {
            // Arrange
            Battle battle = new Battle();
            Monster_Card knightcard = new Monster_Card("Normal", 100, Element.Normal, Monster.Knight);
            Spell_Card watercard = new Spell_Card("Water", 100, Element.Water);

            // Act
            ICard winner = battle.CardsRoundFight(knightcard, watercard);

            // Assert
            Assert.IsTrue(winner.Equals(watercard));
        }

        [Test]
        public async Task RoundFight_5_Kraken_Spell()
        {
            // Arrange
            Battle battle = new Battle();
            Monster_Card krakencard = new Monster_Card("Normal", 100, Element.Normal, Monster.Kraken);
            Spell_Card watercard = new Spell_Card("Water", 100, Element.Water);

            // Act
            ICard winner = battle.CardsRoundFight(krakencard, watercard);

            // Assert
            Assert.IsTrue(winner.Equals(krakencard));
        }

        [Test]
        public async Task RoundFight_6_Monster_Monster()
        {
            // Arrange
            Battle battle = new Battle();
            Monster_Card dragoncard = new Monster_Card("Dragon", 100, Element.Fire, Monster.Dragon);
            Monster_Card goblincard = new Monster_Card("Goblin", 100, Element.Fire, Monster.Goblin);


            // Act
            ICard winner = battle.CardsRoundFight(dragoncard, goblincard);

            // Assert
            Assert.IsTrue(winner.Equals(dragoncard));
        }

        [Test]
        public async Task RoundFight_7_Monster_Monster()
        {
            // Arrange
            Battle battle = new Battle();
            Monster_Card dragoncard = new Monster_Card("Dragon", 100, Element.Fire, Monster.Dragon);
            Monster_Card wizardcard = new Monster_Card("Wizard", 150, Element.Fire, Monster.Wizard);


            // Act
            ICard winner = battle.CardsRoundFight(dragoncard, wizardcard);

            // Assert
            Assert.IsTrue(winner.Equals(wizardcard));
        }

        [Test]
        public async Task Full_Battle_Check_Stats()
        {
            // Arrange
            Battle battle = new Battle();
            User user = new User("user1", PasswordHasher.HashPassword("test"), "test", "user1@gmail.com");
            await User.Create(user.UserName, user.Password, user.FullName, user.EMail);
            await Package.BuyPackage(user.UserName);
            user = await User.Get(user.UserName);
            string[] cards = { user._stack.cards[0].Name, user._stack.cards[1].Name, user._stack.cards[2].Name, user._stack.cards[3].Name };
            await Deck.Update(user.UserName, cards);
            user = await User.Get(user.UserName);

            User user2 = new User("user2", PasswordHasher.HashPassword("test"), "test", "user2@gmail.com");
            await User.Create(user2.UserName, user2.Password, user2.FullName, user2.EMail);
            await Package.BuyPackage(user2.UserName);
            user2 = await User.Get(user2.UserName);
            string[] cards2 = { user2._stack.cards[0].Name, user2._stack.cards[1].Name, user2._stack.cards[2].Name, user2._stack.cards[3].Name };
            await Deck.Update(user2.UserName, cards2);
            user2 = await User.Get(user2.UserName);

            // Act
            JsonObject battle_log_parsed = await battle.JoinBattle(user, user2);

            // Assert
            Stat stats1 = await Stat.Get(user.UserName);
            Stat stats2 = await Stat.Get(user2.UserName);
            Assert.AreEqual(stats1.Battles_played, 1);
            Assert.AreEqual(stats2.Battles_played, 1);

        }

        [Test]
        public async Task Full_Battle_Check_Stats_Fail()
        {
            // Arrange
            Battle battle = new Battle();
            User user = new User("user1", PasswordHasher.HashPassword("test"), "test", "user1@gmail.com");
            await User.Create(user.UserName, user.Password, user.FullName, user.EMail);
            await Package.BuyPackage(user.UserName);
            user = await User.Get(user.UserName);
            string[] cards = { user._stack.cards[0].Name, user._stack.cards[1].Name, user._stack.cards[2].Name, user._stack.cards[3].Name };
            await Deck.Update(user.UserName, cards);


            // Act
            var exception = Assert.ThrowsAsync<Exception>(async () => await battle.JoinBattle(user, user));

            // Assert
            Stat stats1 = await Stat.Get(user.UserName);
            Assert.AreEqual(stats1.Battles_played, 0);
            Assert.AreEqual("Cannot battle yourself.", exception.Message);

        }

        [TearDown]
        public async Task Teardown()
        {
            await using var conn = await DB_connection.connectDB();
            await using (var cmd = new NpgsqlCommand("DELETE FROM users", conn))
            {
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}