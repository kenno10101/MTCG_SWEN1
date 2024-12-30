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

namespace MTCG_Project_Unit_Test
{
    public class BattleTests
    {

        [SetUp]
        public async Task Setup()
        {
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


        [TearDown]
        public async Task Teardown()
        {

        }
    }
}