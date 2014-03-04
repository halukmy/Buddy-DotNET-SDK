using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace BuddySDK
{
    public class UserListItemCollection : BuddyCollectionBase<UserListItem>
    {
        internal UserListItemCollection(string parentObjectPath, BuddyClient client)
            : base(parentObjectPath + typeof(UserListItem).GetCustomAttribute<BuddyObjectPathAttribute>(true).Path, client)
        {
        }

        public Task<SearchResult<UserListItem>> FindAsync(string ownerUserId = null, BuddyGeoLocationRange locationRange = null, DateRange created = null, DateRange lastModified = null, int pageSize = 100, string pagingToken = null)
        {
            return base.FindAsync(
                userId: ownerUserId,
                created: created,
                lastModified: lastModified,
                locationRange: locationRange,
                pagingToken: pagingToken,
                pageSize: pageSize,
                parameterCallback: (p) =>
                {
                });
        }
    }
}
