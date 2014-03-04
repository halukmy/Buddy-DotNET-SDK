using System;

namespace BuddySDK
{
    public class MetadataItem
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public BuddyGeoLocation Location { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastModified { get; set; }
    }

    [BuddyObjectPath("/metadata")]
    public class Metadata : BuddyMetadataBase
    {
        public const string App = "app";

        public Metadata(string id, BuddyClient client = null)
            : base(id, client)
        {
         
        }
    }
}