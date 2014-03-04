using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuddySDK
{
    public class PictureCollection : BuddyCollectionBase<Picture>
    {
        internal PictureCollection(BuddyClient client)
            : base(null, client)
        {
        }

        internal static Task<BuddyResult<Picture>> AddAsync(BuddyClient client, string caption, Stream pictureData, string contentType, BuddyGeoLocation location = null, BuddyPermissions read = BuddyPermissions.User, BuddyPermissions write = BuddyPermissions.User)
        {
            return Task.Run<BuddyResult<Picture>>(() =>
            {
                var c = new Picture(null, client)
                {
                    Caption = caption,
                    Location = location,
                    Data = new BuddyServiceClient.BuddyFile()
                    {
                        ContentType = contentType,
                        Data = pictureData,
                        Name = "data"
                    },
                    ReadPermissions = read,
                    WritePermissions = write
                };

                var t = c.SaveAsync();
                
                return t.Result.Convert(r => c);
            });    
        }

        public Task<BuddyResult<Picture>> AddAsync(string caption, Stream pictureData, string contentType, BuddyGeoLocation location = null, BuddyPermissions read = BuddyPermissions.User, BuddyPermissions write = BuddyPermissions.User)
        {
            return PictureCollection.AddAsync(this.Client, caption, pictureData, contentType, location, read, write);
        }

        public Task<SearchResult<Picture>> FindAsync(string caption = null, string contentType = null, string ownerUserId = null, BuddyGeoLocationRange locationRange = null, DateRange created = null, DateRange lastModified = null, int pageSize = 100, string pagingToken = null)
        {
            return base.FindAsync(userId: ownerUserId,
                created: created,
                lastModified: lastModified,
                locationRange: locationRange,
                pagingToken: pagingToken,
                pageSize: pageSize,
                parameterCallback: (p) =>
            {
                p["caption"] = caption;
                p["contentType"] = contentType;
            });
        }
    }
}
