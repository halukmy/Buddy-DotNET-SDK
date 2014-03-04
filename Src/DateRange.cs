using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuddySDK
{
    [Newtonsoft.Json.JsonConverter(typeof(BuddySDK.BuddyServiceClient.DateRangeJsonConverter))]
    public class DateRange
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    
}
