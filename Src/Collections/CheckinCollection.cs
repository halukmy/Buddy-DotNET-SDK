using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuddySDK
{
    public class CheckinCollection : BuddyCollectionBase<Checkin>
    {
        internal CheckinCollection(BuddyClient client)
            : base(null, client)
        {
        }

        public Task<BuddyResult<Checkin>> AddAsync(string comment, string description, BuddyGeoLocation location, string defaultMetadata = null, BuddyPermissions read = BuddyPermissions.User, BuddyPermissions write = BuddyPermissions.User)
        {
            return Task.Run<BuddyResult<Checkin>>(() =>{
                var c = new Checkin(null, this.Client)
                    {
                        Comment = comment,
                        Description = description,
                        Location = location,
                        DefaultMetadata = defaultMetadata,
                        ReadPermissions = read,
                        WritePermissions = write
                    };

                var t = c.SaveAsync();

                return t.Result.Convert(f => c);
            });
           
        }

        public Task<SearchResult<Checkin>> FindAsync(string comment = null, string ownerUserId = null, BuddyGeoLocationRange locationRange = null, DateRange created = null, DateRange lastModified = null, int pageSize = 100, string pagingToken = null)
        {

            return base.FindAsync (userId: ownerUserId,
                created: created,
                lastModified: lastModified,
                locationRange: locationRange,
                pagingToken: pagingToken,
                pageSize: pageSize,
                parameterCallback:  (p) => {
                p["comment"] = comment;
            });

        } 
    }
}
