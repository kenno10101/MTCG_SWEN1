using MTCG_Project.Models.Users;
using MTCG_Project.Misc;
using MTCG_Project.Handler;
using MTCG_Project.Network;
using System.Reflection.Metadata;
using MTCG_Project.Exceptions;
using System.Data.Common;
using Npgsql;
using System;
using MTCG_Project.Models.Card;

namespace MTCG_Project_Unit_Test.Specs
{
    public class DeckTest
    {

        [SetUp]
        public async Task Setup()
        {
            DB_connection.connString = "Host=localhost;Port=5431;Username=kenn;Password=kenn1234;Database=MTCG_project_test";

            // Create first user
            User user = new User("kenn", PasswordHasher.HashPassword("test"), "test", "user@gmail.com");
            await User.Create(user.UserName, user.Password, user.FullName, user.EMail);
            // Get Cards / Buy Package
            await Package.BuyPackage(user.UserName);
        }


        [Test]
        public async Task Update_Deck_Fail_Card_Not_In_Stack()
        {
            // Arrange
            User user = await User.Get("kenn");
            string[] cards = { "Megacard", user._stack.cards[1].Name, user._stack.cards[2].Name, user._stack.cards[3].Name };

            // Act
            var exception = Assert.ThrowsAsync<Exception>(async () => await Deck.Update("kenn", cards));

            // Assert
            Assert.AreEqual("Error: trying to add card that doesn't belong to your stack.", exception.Message);
        }

        [Test]
        public async Task Update_Deck_Fail_Missing_Param()
        {
            // Arrange
            string[] cards = { null, null, null, null };

            // Act
            var exception = Assert.ThrowsAsync<Exception>(async () => await Deck.Update("kenn", cards));

            // Assert
            Assert.AreEqual("Parameter 'n' must have either its NpgsqlDbType or its DataTypeName or its Value set.", exception.Message);
        }

        [Test]
        public async Task Update_Deck_Success()
        {
            // Arrange
            User user = await User.Get("kenn");
            string[] cards = { user._stack.cards[0].Name, user._stack.cards[1].Name, user._stack.cards[2].Name, user._stack.cards[3].Name };

            // Act
            // Assert
            Assert.DoesNotThrowAsync(async () => await Deck.Update("kenn", cards));
        }


        [TearDown]
        public async Task Teardown()
        {
            // add clean db function
            await using var conn = await DB_connection.connectDB();
            await using (var cmd = new NpgsqlCommand("DELETE FROM users", conn))
            {
                await cmd.ExecuteNonQueryAsync();
            }

        }
    }
}