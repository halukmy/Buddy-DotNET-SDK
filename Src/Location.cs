using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuddySDK
{
    [BuddyObjectPath("/locations")]
    public class Location : BuddyBase
    {
        [Newtonsoft.Json.JsonProperty("name")]
        public string Name
        {
            get
            {
                return GetValueOrDefault<string>("Name");
            }
            set
            {
                SetValue<string>("Name", value,checkIsProp:false);
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
                SetValue<string>("Description", value,checkIsProp:false);
            }
        }

        [Newtonsoft.Json.JsonProperty("address1")]
        public string Address1
        {
            get
            {
                return GetValueOrDefault<string>("Address1");
            }
            set
            {
                SetValue<string>("Address1", value, checkIsProp: false);
            }
        }

        [Newtonsoft.Json.JsonProperty("address2")]
        public string Address2
        {
            get
            {
                return GetValueOrDefault<string>("Address2");
            }
            set
            {
                SetValue<string>("Address2", value, checkIsProp: false);
            }
        }

        [Newtonsoft.Json.JsonProperty("city")]
        public string City
        {
            get
            {
                return GetValueOrDefault<string>("City");
            }
            set
            {
                SetValue<string>("City", value,checkIsProp:false);
            }
        }

        [Newtonsoft.Json.JsonProperty("region")]
        public string Region
        {
            get
            {
                return GetValueOrDefault<string>("Region");
            }
            set
            {
                SetValue<string>("Region", value, checkIsProp: false);
            }
        }

        [Newtonsoft.Json.JsonProperty("country")]
        public string Country
        {
            get
            {
                return GetValueOrDefault<string>("Country");
            }
            set
            {
                SetValue<string>("Country", value,checkIsProp:false);
            }
        }

        [Newtonsoft.Json.JsonProperty("postalCode")]
        public string PostalCode
        {
            get
            {
                return GetValueOrDefault<string>("PostalCode");
            }
            set
            {
                SetValue<string>("PostalCode", value,checkIsProp:false);
            }
        }
      
        [Newtonsoft.Json.JsonProperty("fax")]
        public string FaxNumber
        {
            get
            {
                return GetValueOrDefault<string>("FaxNumber");
            }
            set
            {
                SetValue<string>("FaxNumber", value,checkIsProp:false);
            }
        }

        [Newtonsoft.Json.JsonProperty("phone")]
        public string PhoneNumber
        {
            get
            {
                return GetValueOrDefault<string>("PhoneNumber");
            }
            set
            {
                SetValue<string>("PhoneNumber", value,checkIsProp:false);
            }
        }

        [Newtonsoft.Json.JsonProperty("website")]
        public Uri Website
        {
            get
            {
                return GetValueOrDefault<Uri>("Website");
            }
            set
            {
                SetValue<Uri>("Website", value, checkIsProp:false);
            }
        }

        [Newtonsoft.Json.JsonProperty("category")]
        public string Category
        {
            get
            {
                return GetValueOrDefault<string>("Category");
            }
            set
            {
                SetValue<string>("Category", value,checkIsProp:false);
            }
        }

        [Newtonsoft.Json.JsonProperty("distanceFromSearch")]
        public double Distance
        {
            get
            {
                return GetValueOrDefault<double>("Distance");
            }
            internal set
            {
                SetValue<double>("Distance", value,checkIsProp:false);
            }
        }

        public Location()
        {
        }

        public Location(string id= null, BuddyClient client = null)
            : base(id, client)
        {
        }

        public override Task<BuddyResult<bool>> SaveAsync()
        {
            if (Location == null)
            {
                throw new ArgumentException("Location is required.");
            }
            return base.SaveAsync();
        }
    }
}
