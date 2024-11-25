﻿using MTCG_Project.Network;
using MTCG_Project.Handler;

namespace MTCG_Project
{
    internal class Program
    {
        public const bool ALLOW_DEBUG_TOKEN = true;
        static void Main(string[] args)
        {
            HttpSvr svr = new();
            svr.Incoming += Svr_Incoming; //(sender, e) => { Handler.HandleEvent(e); };

            svr.Run();
        }
        private static void Svr_Incoming(object sender, HttpSvrEventArgs e)
        {
            Handler.Handler.HandleEvent(e);

            /*
            Console.WriteLine(e.Method);
            Console.WriteLine(e.Path);
            Console.WriteLine();
            foreach(HttpHeader i in e.Headers)
            {
                Console.WriteLine(i.Name + ": " + i.Value);
            }
            Console.WriteLine();
            Console.WriteLine(e.Payload);

            e.Reply(HttpStatusCode.OK, "Yo Baby!");
            */
        }

    }
}