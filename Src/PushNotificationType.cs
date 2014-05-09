using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuddySDK
{
    [Flags]
    public enum PushNotificationType
    {
        None = 0,
        Raw   = 1,
        Badge = 2,
        Alert = 4,
        Custom = 8
    }
}
