using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuddySDK.BuddyServiceClient
{
    internal class DateRangeJsonConverter : Newtonsoft.Json.JsonConverter
    {
        static readonly DateTime UnixStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static long ToUnixTicks(DateTime dt)
        {
            return (long)dt.ToUniversalTime().Subtract(UnixStart).TotalMilliseconds;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            var dr = (DateRange)value;
            var val = "";
            if (dr.StartDate.HasValue) {
                val += String.Format("/Date({0})/", ToUnixTicks(dr.StartDate.Value));
            }
            val += "-";

            if (dr.EndDate.HasValue) {
                val += String.Format("/Date({0})/", ToUnixTicks(dr.StartDate.Value));
            }
            writer.WriteValue(val);
        }
    }
}
