using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Microsoft.Toolkit.Uwp.Connectivity;

namespace Dota2灵魂小助手
{
    public class NetworkCheckHelper
    {
        public static bool NetworkAvailable = false;

        public static bool CheckNetwork()
        {
            try
            {
                NetworkAvailable = NetworkHelper.Instance.ConnectionInformation.IsInternetAvailable;
            }
            catch
            {
                NetworkAvailable = false;
            }
            return NetworkAvailable;
        }
    }
}
