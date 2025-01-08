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
        private readonly ConcurrentQueue<string> temp_data = new();

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
                Task.Run(() => HandleClient(client, buf));
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
            
            // Pairing logic
            if (data.Contains("battle")) // Assuming client sends a "battle" request
            {
                if (!waitingClients.Contains(client)) // Avoid duplicate enqueue
                {
                    waitingClients.Enqueue(client);
                    temp_data.Enqueue(data);
                }
                if (waitingClients.Count >= 2)
                {
                    // Pair two clients
                    if (waitingClients.TryDequeue(out var client1) &&
                        waitingClients.TryDequeue(out var client2) &&
                        temp_data.TryDequeue(out var data1) &&
                        temp_data.TryDequeue(out var data2)){
                        HttpSvrEventArgs e1 = new HttpSvrEventArgs(client1, data1);
                        HttpSvrEventArgs e2 = new HttpSvrEventArgs(client2, data2);
                        
                        
                        await BattleHandler.joinBattle(e1, e2);
                    }
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
