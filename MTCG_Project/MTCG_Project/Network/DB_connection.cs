﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG_Project.Network
{
    public class DB_connection
    {
        public static string connString = "Host=localhost;Port=5431;Username=kenn;Password=kenn1234;Database=MTCG_project";
        public static async Task<NpgsqlConnection> connectDB()
        {
            var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();
            return conn;
        }
    }
}
