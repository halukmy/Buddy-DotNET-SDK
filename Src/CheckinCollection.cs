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
                        DefaultMetadata = defaultMetadata
                        //  ReadPermissions = read,
                        //  WritePemissions = write
                    };

                var t = c.SaveAsync();

                return t.Result.Convert(f => c);
            });
           
        }

        public Task<SearchResult<Checkin>> FindAsync(string comment = null, BuddyGeoLocationRange location = null, string userId = null, int maxResults = 100) {

            return base.FindAsync (userId, null, null, location, maxResults, null, (p) => {
                p["comment"] = comment;
            });

        } 
    }
}
