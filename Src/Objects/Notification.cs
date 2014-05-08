using System;
using System.Collections;
using System.Collections.Generic;

namespace BuddySDK
{
    [BuddyObjectPath("/notifications")]
    public class Notification : BuddyBase
    {
        [Newtonsoft.Json.JsonProperty("sentByPlatform")]
        public IDictionary<string,int> SentByPlatform { get; set; }

        internal Notification(BuddyClient client) : base(client){
        }

        public Notification(string id, BuddyClient client = null) : base(id, client){
        }
    }
}

