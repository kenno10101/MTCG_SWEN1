using MTCG_Project.Models.Users;
using MTCG_Project.Misc;
using MTCG_Project.Handler;
using MTCG_Project.Network;
using System.Reflection.Metadata;
using MTCG_Project.Exceptions;
using System.Data.Common;
using Npgsql;
using System;

namespace MTCG_Project_Unit_Test.Specs
{
    public class UserTests
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
        public async Task CreateUser_SameUsernameExist()
        {
            // Arrange
            User user_new = new User("user", PasswordHasher.HashPassword("test2"), "test2", "user2@gmail.com");

            // Act
            var exception = Assert.ThrowsAsync<UserException>(async () => await User.Create(user_new.UserName, user_new.Password, user_new.FullName, user_new.EMail));

            // Assert
            Assert.AreEqual("A user with this username already exists.", exception.Message);
        }

        [Test]
        public async Task CreateUser_SameEmailExist()
        {
            // Arrange
            User user_new = new User("new_user", PasswordHasher.HashPassword("test"), "test", "user@gmail.com");

            // Act
            var exception = Assert.ThrowsAsync<UserException>(async () => await User.Create(user_new.UserName, user_new.Password, user_new.FullName, user_new.EMail));

            // Assert
            Assert.AreEqual("A user with this email already exists.", exception.Message);
        }

        [Test]
        public async Task CreateUser_Success()
        {
            // Arrange
            User user_new = new User("new_user", PasswordHasher.HashPassword("test"), "test", "new_user@gmail.com");

            // Act
            // Assert
            Assert.DoesNotThrowAsync(async () => await User.Create(user_new.UserName, user_new.Password, user_new.FullName, user_new.EMail));
        }

        [Test]
        public async Task GetUser()
        {
            // Arrange
            User kenn = new User("kenn", PasswordHasher.HashPassword("kenn"), "Kenn", "kenn@gmail.com");
            await User.Create(kenn.UserName, kenn.Password, kenn.FullName, kenn.EMail);
            User queryUser;

            // Act
            queryUser = await User.Get("kenn");

            // Assert
            Assert.AreEqual(queryUser.UserName, kenn.UserName);
            Assert.AreEqual(queryUser.Password, kenn.Password);
            Assert.AreEqual(queryUser.FullName, kenn.FullName);
            Assert.AreEqual(queryUser.EMail, kenn.EMail);
        }

        [Test]
        public async Task UpdateUser_1()
        {
            // Arrange
            User kenn = new User("kenn", PasswordHasher.HashPassword("kenn"), "Kenn", "kenn@gmail.com");
            await User.Create(kenn.UserName, kenn.Password, kenn.FullName, kenn.EMail);

            User updated_user = new User(null, null, "Kenn Sanga", "kennsanga@gmail.com");

            // Act
            await User.Update("kenn", updated_user.UserName, updated_user.Password, updated_user.FullName, updated_user.EMail);


            // Assert
            User queryUser = await User.Get("kenn");
            Assert.AreNotEqual(queryUser.FullName, kenn.FullName);
            Assert.AreNotEqual(queryUser.EMail, kenn.EMail);

            Assert.AreEqual(queryUser.FullName, updated_user.FullName);
            Assert.AreEqual(queryUser.EMail, updated_user.EMail);
        }

        [Test]
        public async Task UpdateUser_2()
        {
            // Arrange
            User kenn = new User("kenn", PasswordHasher.HashPassword("kenn"), "Kenn", "kenn@gmail.com");
            await User.Create(kenn.UserName, kenn.Password, kenn.FullName, kenn.EMail);

            User updated_user = new User(null, null, null, null);

            // Act
            var exception = Assert.ThrowsAsync<UserException>(async () => await User.Update("kenn", updated_user.UserName, updated_user.Password, updated_user.FullName, updated_user.EMail));

            // Assert
            Assert.AreEqual("Error no parameters given.", exception.Message);
        }

        [Test]
        public async Task Login_Success()
        {
            // Arrange
            User kenn = new User("kenn", PasswordHasher.HashPassword("kenn"), "Kenn", "kenn@gmail.com");
            await User.Create(kenn.UserName, kenn.Password, kenn.FullName, kenn.EMail);

            // Act
            (bool success, string token) login = await User.Logon("kenn", "kenn");

            // Assert
            Assert.IsTrue(login.success);
        }

        [Test]
        public async Task Login_Fail()
        {
            // Arrange
            User kenn = new User("kenn", PasswordHasher.HashPassword("kenn"), "Kenn", "kenn@gmail.com");
            await User.Create(kenn.UserName, kenn.Password, kenn.FullName, kenn.EMail);

            // Act
            (bool success, string token) login = await User.Logon("kenn", "wrongpw");

            // Assert
            Assert.IsFalse(login.success);
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