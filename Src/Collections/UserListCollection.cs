using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuddySDK
{
    public class UserListCollection : BuddyCollectionBase<UserList>
    {
        public UserListCollection()
            : base()
        {
        }

        internal UserListCollection(BuddyClient client)
            : base(null, client)
        {
        }

        public async Task<BuddyResult<UserList>> AddAsync(string name,
            BuddyGeoLocation location, string defaultMetadata = null, BuddyPermissions readPermissions = BuddyPermissions.User, BuddyPermissions writePermissions = BuddyPermissions.User)
        {
            var c = new UserList(this.Client)
            {
                Name = name,
                Location = location,
                DefaultMetadata = defaultMetadata,
                ReadPermissions = readPermissions,
                WritePermissions = writePermissions
            };

            var r = await c.SaveAsync();
            return r.Convert(b => c);
        }

        public Task<SearchResult<UserList>> FindAsync(string name = null, string ownerUserId = null,
            BuddyGeoLocationRange locationRange = null, DateRange created = null, DateRange lastModified = null, int pageSize = 100, string pagingToken = null)
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
                });
        }
    }
}
