using MTCG_Project.Network;
using MTCG_Project.Handler;

namespace MTCG_Project
{
    internal class Program
    {
        public const bool ALLOW_DEBUG_TOKEN = true;
        static void Main(string[] args)
        {

            //DBHandler.connectDB();
            HttpSvr svr = new();
            svr.Incoming += Svr_Incoming; //(sender, e) => { Handler.HandleEvent(e); };

            svr.Run();
        }
        private static void Svr_Incoming(object sender, HttpSvrEventArgs e)
        {
            Handler.Handler.HandleEvent(e);
        }

    }
}