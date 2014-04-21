using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace BuddySDK
{
    public class AlbumItemCollection : BuddyCollectionBase<AlbumItem>
    {
        internal AlbumItemCollection(string parentObjectPath, BuddyClient client)
            : base(parentObjectPath + PlatformAccess.GetCustomAttribute<BuddyObjectPathAttribute>(typeof(AlbumItem)).Path, client)
        {
        }

        public Task<SearchResult<AlbumItem>> FindAsync(AlbumItemType? itemType = null, string caption = null, string ownerUserId = null,
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
                    if (itemType.HasValue) { p["itemType"] = itemType; }
                    p["caption"] = caption;
                });
        }
    }
}
