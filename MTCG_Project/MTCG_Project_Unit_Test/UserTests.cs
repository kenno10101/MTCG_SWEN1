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
            User user = new User("test", PasswordHasher.HashPassword("test"), "test", "test@gmail.com");
            await User.Create(user.UserName, user.Password, user.FullName, user.EMail);
            User user_new = new User("test", PasswordHasher.HashPassword("test2"), "test2", "test2@gmail.com");


            var exception = Assert.ThrowsAsync<UserException>(async () => await User.Create(user_new.UserName, user_new.Password, user_new.FullName, user_new.EMail));


            Assert.AreEqual("A user with this username already exists.", exception.Message);
        }

        [Test]
        public async Task CreateUser_SameEmailExist()
        {
            User user = new User("test", PasswordHasher.HashPassword("test"), "test", "test3@gmail.com");
            await User.Create(user.UserName, user.Password, user.FullName, user.EMail);
            User user_new = new User("test2", PasswordHasher.HashPassword("test"), "test", "test3@gmail.com");


            var exception = Assert.ThrowsAsync<UserException>(async () => await User.Create(user_new.UserName, user_new.Password, user_new.FullName, user_new.EMail));


            Assert.AreEqual("A user with this email already exists.", exception.Message);
        }


        [TearDown]
        public async Task Teardown()
        {
            await using var conn = await DB_connection.connectDB();
            await using (var cmd = new NpgsqlCommand("DELETE FROM users WHERE username = @username", conn))
            {
                cmd.Parameters.AddWithValue("username", "test");
                await cmd.ExecuteNonQueryAsync();
            }
            
        }
    }
}