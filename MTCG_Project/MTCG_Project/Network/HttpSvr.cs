using MTCG_Project.Handler;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MTCG_Project.Network
{
    public sealed class HttpSvr
    {
        private TcpListener? _Listener;
        public event HttpSvrEventHandler? Incoming;
        public bool Active { get; private set; } = false;
        private readonly ConcurrentQueue<TcpClient> waitingClients = new();
        private List<string> temp_data = new List<string>();

        public void Run()
        {
            if (Active) return;
            Active = true;
            _Listener = new(IPAddress.Parse("127.0.0.1"), 12000);
            _Listener.Start();

            byte[] buf = new byte[256];

            while (Active)
            {
                TcpClient client = _Listener.AcceptTcpClient();
                HandleClient(client, buf);
            }
        }
        
        private async void HandleClient(TcpClient client, byte[] buffer)
        {
            string data = string.Empty;

            while (client.GetStream().DataAvailable || string.IsNullOrWhiteSpace(data))
            {
                int read = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);
                data += Encoding.ASCII.GetString(buffer, 0, read);
            }

            HttpSvrEventArgs eventArgs = new(client, data);
            
            // Pairing logic
            if (data.Contains("battle")) // Assuming client sends a "battle" request
            {
                if (!waitingClients.Contains(client)) // Avoid duplicate enqueue
                {
                    waitingClients.Enqueue(client);
                    temp_data.Add(data);
                }
                if (waitingClients.Count >= 2)
                {
                    // Pair two clients
                    if (waitingClients.TryDequeue(out var client1) && waitingClients.TryDequeue(out var client2))
                    {
                        HttpSvrEventArgs e1 = new HttpSvrEventArgs(client1, temp_data[0]);
                        HttpSvrEventArgs e2 = new HttpSvrEventArgs(client2, temp_data[1]);
                        
                        temp_data.RemoveRange(0, 2); 
                        
                        await BattleHandler.joinBattle(e1, e2);
                    }
                }
                else
                {
                    eventArgs.Reply(200, "\"Waiting for another player...\"", false);
                }
            }
            else
            {
                Incoming?.Invoke(this, new(client, data));
            }
        }

        public void Stop()
        {
            Active = false;
        }
    }
}
