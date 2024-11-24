using MTCG_Project.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG_Project.Interfaces
{
    public interface IHandler
    {
        public bool Handle(HttpSvrEventArgs e);
    }
}
