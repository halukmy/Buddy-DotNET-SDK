using System;

namespace BuddySDK
{

    public enum ConnectivityLevel {
        None,
        Connected,
        Carrier,
        WiFi
    }

    public class ConnectivityLevelChangedArgs : EventArgs {

        public ConnectivityLevel ConnectivityLevel {
            get;
            internal set;
        }
    }
}

