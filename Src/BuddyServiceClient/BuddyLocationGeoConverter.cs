using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuddySDK.BuddyServiceClient
{
    internal class BuddyLocationGeoConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = (JObject)serializer.Deserialize(reader);
            return new BuddyGeoLocation()
            {
                Latitude = (double)obj.Property("lat").Value,
                Longitude = (double)obj.Property("lng").Value
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            BuddyGeoLocation bcl = (BuddyGeoLocation)value;

            writer.WriteValue(bcl.ToString());
            
        }
    }
}
