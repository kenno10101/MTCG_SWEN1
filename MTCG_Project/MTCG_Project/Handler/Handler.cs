using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using MTCG_Project.Interfaces;
using System.Net;
using MTCG_Project.Network;

namespace MTCG_Project.Handler
{
    public abstract class Handler : IHandler
    {
        private static List<IHandler>? _Handlers = null;

        private static List<IHandler> _GetHandlers()
        {
            List<IHandler> rval = new();

            foreach (Type i in Assembly.GetExecutingAssembly().GetTypes()
                         .Where(m => m.IsAssignableTo(typeof(IHandler)) && !m.IsAbstract))
            {
                IHandler? h = (IHandler?)Activator.CreateInstance(i);
                if (h != null)
                {
                    rval.Add(h);
                }
            }

            return rval;
        }

        public static void HandleEvent(HttpSvrEventArgs e)
        {
            _Handlers ??= _GetHandlers();

            foreach (IHandler i in _Handlers)
            {
                if (i.Handle(e)) return;
            }
            e.Reply(HttpStatusCodes.BAD_REQUEST);
        }

        public abstract bool Handle(HttpSvrEventArgs e);
    }
}
