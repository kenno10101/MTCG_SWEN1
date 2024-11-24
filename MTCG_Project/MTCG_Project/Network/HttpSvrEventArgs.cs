using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MTCG_Project.Network
{
    public class HttpSvrEventArgs : EventArgs
    {
        protected TcpClient _Client;

        public HttpSvrEventArgs(TcpClient client, string plainMessage)
        {
            _Client = client;

            PlainMessage = plainMessage;
            Payload = string.Empty;

            string[] lines = plainMessage.Replace("\r\n", "\n").Split('\n');
            bool inheaders = true;
            List<HttpHeader> headers = new();

            for (int i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                {
                    string[] inc = lines[0].Split(' ');
                    Method = inc[0];
                    Path = inc[1];
                    continue;
                }

                if (inheaders)
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                    {
                        inheaders = false;
                    }
                    else { headers.Add(new(lines[i])); }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(Payload)) { Payload += "\r\n"; }
                    Payload += lines[i];
                }
            }

            Headers = headers.ToArray();
        }
        public string PlainMessage
        {
            get; protected set;
        } = string.Empty;

        public virtual string Method
        {
            get; protected set;
        } = string.Empty;

        public string Path
        {
            get; protected set;
        } = string.Empty;

        public virtual HttpHeader[] Headers
        {
            get; protected set;
        } = Array.Empty<HttpHeader>();

        public string Payload
        {
            get; protected set;
        } = string.Empty;

        public void Reply(int status, string? body = null)
        {
            string data;

            switch (status)
            {
                case 200:
                    data = "HTTP/1.1 200 OK\n"; break;
                case 400:
                    data = "HTTP/1.1 400 Bad Request\n"; break;
                case 401:
                    data = "HTTP/1.1 401 Unauthorized\n"; break;
                case 404:
                    data = "HTTP/1.1 404 Not found\n"; break;
                default:
                    data = $"HTTP/1.1 {status} Status unknown\n"; break;
            }

            if (string.IsNullOrEmpty(body))
            {
                data += "Content-Length: 0\n";
            }
            data += "Content-Type: text/plain\n\n";
            if (!string.IsNullOrEmpty(body)) { data += body; }

            byte[] buf = Encoding.ASCII.GetBytes(data);
            _Client.GetStream().Write(buf, 0, buf.Length);
            _Client.Close();
            _Client.Dispose();
        }
    }
}
