using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuddySDK
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BuddyPermissions
    {
        App,
        User,
        Default = User
    }
}
