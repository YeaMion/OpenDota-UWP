using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedData
{
    public static class Share
    {
        public static string PlayerID { set; get; }

        public static string GetID()
        {
            return PlayerID;
        }
    }

}
