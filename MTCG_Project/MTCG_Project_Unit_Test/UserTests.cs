using MTCG_Project.Models.Users;
using MTCG_Project.Misc;
using MTCG_Project.Handler;
using MTCG_Project.Network;
using System.Reflection.Metadata;
using MTCG_Project.Exceptions;
using System.Data.Common;
using Npgsql;

namespace MTCG_Project_Unit_Test
{
    public class UserTests
    {

        [SetUp]
        public async Task Setup()
        {
            DB_connection.connString = "Host=localhost;Port=5431;Username=kenn;Password=kenn1234;Database=MTCG_project_test";
        }

        [Test]
        public async Task CreateUser_SameUsernameExist()
        {
            string same_username = "user";
            User user = new User(same_username, PasswordHasher.HashPassword("test"), "test", "user@gmail.com");
            await User.Create(user.UserName, user.Password, user.FullName, user.EMail);
            User user_new = new User(same_username, PasswordHasher.HashPassword("test2"), "test2", "user2@gmail.com");
        
        
            var exception = Assert.ThrowsAsync<UserException>(async () => await User.Create(user_new.UserName, user_new.Password, user_new.FullName, user_new.EMail));
        
        
            Assert.AreEqual("A user with this username already exists.", exception.Message);
        }

        [Test]
        public async Task CreateUser_SameEmailExist()
        {
            string same_email = "email@gmail.com";
            User user = new User("email", PasswordHasher.HashPassword("test"), "test", same_email);
            await User.Create(user.UserName, user.Password, user.FullName, user.EMail);
            User user_new = new User("email1", PasswordHasher.HashPassword("test"), "test", same_email);
        
        
            var exception = Assert.ThrowsAsync<UserException>(async () => await User.Create(user_new.UserName, user_new.Password, user_new.FullName, user_new.EMail));
        
        
            Assert.AreEqual("A user with this email already exists.", exception.Message);
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