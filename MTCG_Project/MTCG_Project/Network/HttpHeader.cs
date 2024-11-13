using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG_Project.Network
{
    public class HttpHeader
    {
        public HttpHeader(string header)
        {
            Name = Value = string.Empty;

            try
            {
                int n = header.IndexOf(':');
                Name = header.Substring(0, n).Trim();
                Value = header.Substring(n + 1).Trim();
            }
            catch (Exception)
            {
                Name = header;
            }
        }

        public string Name
        {
            get; protected set;
        }

        public string Value
        {
            get; protected set;
        }
    }
}
