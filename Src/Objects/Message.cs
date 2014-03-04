using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuddySDK
{
    [BuddyObjectPath("/messages")]
    public class Message : BuddyBase
    {
       

        [Newtonsoft.Json.JsonProperty("subject")]
        public string Subject
        {
            get
            {
                return GetValueOrDefault<string>("Subject");
            }
            set
            {
                SetValue<string>("Subject", value, checkIsProp: false);
            }
        }

        [Newtonsoft.Json.JsonProperty("body")]
        public string Body
        {
            get
            {
                return GetValueOrDefault<string>("Body");
            }
            set
            {
                SetValue<string>("Body", value, checkIsProp: false);
            }
        }

        [Newtonsoft.Json.JsonProperty("thread")]
        public string ThreadId
        {
            get
            {
                return GetValueOrDefault<string>("ThreadId");
            }
            set
            {
                SetValue<string>("ThreadId", value, checkIsProp: false);
            }
        }

        [Newtonsoft.Json.JsonProperty("from")]
        public string FromUserId
        {
            get
            {
                return GetValueOrDefault<string>("FromUserId");
            }
            internal set
            {
                SetValue<string>("FromUserId", value, checkIsProp: false);
            }
        }

        [Newtonsoft.Json.JsonProperty("fromName")]
        public string FromUserName
        {
            get
            {
                return GetValueOrDefault<string>("FromUserName");
            }
            internal set
            {
                SetValue<string>("ToUserName", value, checkIsProp: false);
            }
        }

        [Newtonsoft.Json.JsonProperty("to")]
        public string ToUserId
        {
            get
            {
                return GetValueOrDefault<string>("ToUserId");
            }
            internal set
            {
                SetValue<string>("ToUserId", value, checkIsProp: false);
            }
        }

        [Newtonsoft.Json.JsonProperty("toName")]
        public string ToUserName
        {
            get
            {
                return GetValueOrDefault<string>("ToUserName");
            }
            internal set
            {
                SetValue<string>("ToUserName", value, checkIsProp: false);
            }
        }


        [Newtonsoft.Json.JsonProperty("send")]
        public DateTime Sent
        {
            get
            {
                return GetValueOrDefault<DateTime>("Sent");
            }
            set
            {
                SetValue<DateTime>("Sent", value, checkIsProp: false);
            }
        }

        [Newtonsoft.Json.JsonProperty("addressees")]
        public IEnumerable<string> Recipients
        {
            get
            {
                return GetValueOrDefault<IEnumerable<string>>("Recipients");
            }
            set
            {
                SetValue<IEnumerable<string>>("Recipients", value, checkIsProp: false);
            }
        }

        [Newtonsoft.Json.JsonProperty("messageType")]
        public MessageType MessageType
        {
            get
            {
                return GetValueOrDefault<MessageType>("MessageType");
            }
            internal set
            {
                SetValue<MessageType>("MessageType", value, checkIsProp: false);
            }
        }

        [Newtonsoft.Json.JsonProperty("isNew")]
        public bool IsNew
        {
            get
            {
                return GetValueOrDefault<bool>("IsNew");
            }
            set
            {
                SetValue<bool>("IsNew", value, checkIsProp: false);
            }
        }

        internal Message(BuddyClient client = null)
            : base(client)
        {

        }

        public Message(string id, BuddyClient client = null)
            : base(id, client)
        {

        }
    }
}
