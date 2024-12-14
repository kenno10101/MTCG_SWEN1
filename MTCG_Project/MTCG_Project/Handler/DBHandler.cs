using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG_Project.Handler
{
    public class DBHandler
    {
        public static async void connectDB()
        {
            var connString = "Host=localhost;Port=5431;Username=kenn;Password=kenn1234;Database=MTCG_project";

            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
        }
    }
}
