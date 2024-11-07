using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MTCG_Project
{
    public class HttpSvrEventArgs : EventArgs
    {
        protected TcpClient _Client;
        
        public HttpSvrEventArgs(TcpClient client, string data)
        {
            _Client = client;
            _Data = data;
        }
        public string _Data
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

        public HttpHeaders[] Headers
        {
            get; protected set;
        } = Array.Empty<HttpHeaders>();

        public string Payload
        {
            get; protected set;
        } = string.Empty;
    }
}
