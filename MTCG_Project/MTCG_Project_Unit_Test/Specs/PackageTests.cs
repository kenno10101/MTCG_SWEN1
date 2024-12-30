using MTCG_Project.Models.Users;
using MTCG_Project.Misc;
using MTCG_Project.Handler;
using MTCG_Project.Network;
using System.Reflection.Metadata;
using MTCG_Project.Exceptions;
using System.Data.Common;
using MTCG_Project.Models.Card;
using Npgsql;
using System;

namespace MTCG_Project_Unit_Test.Spec
{
    public class PackageTests
    {

        [SetUp]
        public async Task Setup()
        {
            DB_connection.connString = "Host=localhost;Port=5431;Username=kenn;Password=kenn1234;Database=MTCG_project_test";

            // Create first user
            User user = new User("user", PasswordHasher.HashPassword("test"), "test", "user@gmail.com");
            await User.Create(user.UserName, user.Password, user.FullName, user.EMail);
        }

        [Test]
        public async Task Create_Package_Success()
        {
            // Arrange
            string[] cards = { "Charizard", "Pikachu", "Waterfall", "Lapras", "Mewtwo" };

            // Act
            // Assert
            Assert.DoesNotThrowAsync(async () => await Package.CreatePackage(cards));

        }

        [Test]
        public async Task Create_Package_Fail_Card_Not_Exist()
        {
            // Arrange
            string[] cards = { "Charizard", "Pikachu", "Waterfall", "MEGA Gengar", "Mewtwo" };

            // Act
            var exception = Assert.ThrowsAsync<Exception>(async () => await Package.CreatePackage(cards));

            // Assert
            Assert.IsTrue(exception.Message.StartsWith("23503: insert or update on table \"packages\" violates foreign key constraint"));

        }

        [Test]
        public async Task Buy_Package_Success()
        {
            // Arrange

            // Act
            await Package.BuyPackage("user");
            User user = await User.Get("user");

            // Assert
            Assert.AreEqual(user._stack.cards.Count, 5);
        }

        //[Test]
        //public async Task Buy_Package_No_Coins()
        //{
        //    // Arrange

        //    // Act
        //    for(int i = 0; i < 5; i++)
        //    {
        //        await Package.BuyPackage("user");
        //    }

        //    var exception = Assert.ThrowsAsync<Exception>(async () => await Package.BuyPackage("user"));

        //    // Assert
        //    Assert.AreEqual("Not enough money for buying a card package", exception.Message);
        //}


        [TearDown]
        public async Task Teardown()
        {
            // add clean db function
            await using var conn = await DB_connection.connectDB();
            await using (var cmd = new NpgsqlCommand("DELETE FROM users;", conn))
            {
                await cmd.ExecuteNonQueryAsync();
            }

        }
    }
}