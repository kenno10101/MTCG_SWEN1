using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG_Project.Network
{
    public static class HttpStatusCodes
    {
        /// <summary>Status code OK.</summary>
        public const int OK = 200;

        /// <summary>Status code BAD REQUEST.</summary>
        public const int BAD_REQUEST = 400;

        /// <summary>Status code UNAUTHORIZED.</summary>
        public const int UNAUTHORIZED = 401;

        /// <summary>Status code NOT FOUND.</summary>
        public const int NOT_FOUND = 404;

        public const int NOT_UNIQUE = 409;
    }
}
