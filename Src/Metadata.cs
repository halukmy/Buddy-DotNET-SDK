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
        public Metadata()
            : base()
        {
        }

        private string id;
        internal Metadata(string id, BuddyClient client)
            : base(client)
        {
            this.id = id;
        }

        protected override string GetMetadataID()
        {
            return this.id;
        }
    }
}