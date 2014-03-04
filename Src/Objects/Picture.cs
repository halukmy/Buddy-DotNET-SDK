
using BuddySDK.BuddyServiceClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BuddySDK
{
    [BuddyObjectPath("/pictures")]
    public class Picture : BuddyBase
    {
        [Newtonsoft.Json.JsonProperty("caption")]
        public string Caption
        {
            get
            {
                return GetValueOrDefault<string>("Caption");
            }
            set
            {
                SetValue<string>("Caption", value, checkIsProp: false);
            }
        }

        [JsonIgnore]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        internal BuddyFile Data
        {
            get
            {
                return GetValueOrDefault<BuddyFile>("Data");
            }
            set
            {
                SetValue<BuddyFile>("Data", value, checkIsProp: false);
            }
        }

        [Newtonsoft.Json.JsonProperty("signedUrl")]
        public string SignedUrl
        {
            get
            {
                return GetValueOrDefault<string>("signedUrl");
            }
        }

        internal Picture(BuddyClient client = null) : base(client)
        {
        }

        public Picture(string id, BuddyClient client = null)
            : base(id, client)
        {
        }

        public Task<BuddyResult<Stream>> GetFileAsync(int? size = null)
        {
            return base.GetFileCoreAsync (GetObjectPath() + "/file", new { size = size });
        }
    }
}
