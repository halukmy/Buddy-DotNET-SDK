using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuddySDK
{
    public class LocationCollection : BuddyCollectionBase<Location>
    {

        internal LocationCollection(BuddyClient client)
            : base(null, client)
        {

        }


        public async Task<BuddyResult<Location>> AddAsync(
            string name, 
            string description, 
            BuddyGeoLocation location,
            string address1,
            string address2,
            string city,
            string region,
            string country,
            string postalCode,
            string phoneNumber,
            string faxNumber,
            string website,
            string category,
            string defaultMetadata = null, 
            BuddyPermissions read = BuddyPermissions.User, 
            BuddyPermissions write = BuddyPermissions.User)
        {

            var c = new Location(null, this.Client)
            {
                Name = name,
                Description = description,
                Location = location,
                Address1 = address1,
                Address2 = address2,
                City = city,
                Region = region,
                Country = country,
                PhoneNumber = phoneNumber,
                FaxNumber = faxNumber,
                PostalCode = postalCode,
                Category = category,
                DefaultMetadata = defaultMetadata,
                Website = new Uri(website)
            };

            var r = await c.SaveAsync();
            return r.Convert(b => c);
        }

        public Task<SearchResult<Location>> FindAsync(string name = null, string category = null,  string ownerUserId = null, BuddyGeoLocationRange locationRange = null, DateRange created = null, DateRange lastModified = null, int pageSize = 100, string pagingToken = null)
        {
            return base.FindAsync(userId: ownerUserId,
                created: created,
                lastModified: lastModified,
                locationRange: locationRange,
                pagingToken: pagingToken,
                pageSize: pageSize,
                parameterCallback: (p) =>
                {
                p["name"] = name;
                p["category"] = category;
            });
        }
    }
}
