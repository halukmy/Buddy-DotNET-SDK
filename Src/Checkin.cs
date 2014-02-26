using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BuddySDK
{
    [BuddyObjectPath("/checkins")]
    public class Checkin : BuddyBase
    {

        public class BuddyCheckinLocation : BuddyGeoLocation {
            [JsonProperty("name")]
            public string Name { get; set; }
        }

       
        [Newtonsoft.Json.JsonProperty("comment")]
        public string Comment
        {
            get
            {
                return GetValueOrDefault<string>("Comment");
            }
            set
            {
                SetValue<string>("Comment", value,checkIsProp:false);
            }
        }

        [Newtonsoft.Json.JsonProperty("description")]
        public string Description
        {
            get
            {
                return GetValueOrDefault<string>("Description");
            }
            set
            {
                SetValue<string>("Description", value, checkIsProp: false);
            }
        }


        public Checkin()
        {

        }
       
        public Checkin(string id= null, BuddyClient client = null)
            : base(id, client)
        {

        }

        protected override void SetValueCore<T> (string key, T value)
        {
//            if (key == "Location" && !(value is BuddyGeoLocation)) {
//
//                value = (T)(object) JsonConvert.DeserializeObject<BuddyCheckinLocation>(value.ToString());
//            }

            base.SetValueCore (key, value);
        }

        public override Task<BuddyResult<bool>> SaveAsync()
        {
            var location = GetValueOrDefault<BuddyGeoLocation>("Location", autoPopulate: false);

            if (location == null)
            {
                throw new ArgumentException("Location is required.");
            }

            return base.SaveAsync();
        }
    }

    
}
