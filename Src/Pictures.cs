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
                    }
                    //  ReadPermissions = read,
                    //  WritePemissions = write
                };

                var t = c.SaveAsync();
                
                return t.Result.Convert(r => c);
            });    
        }

        public Task<BuddyResult<Picture>> AddAsync(string caption, Stream pictureData, string contentType, BuddyGeoLocation location, BuddyPermissions read = BuddyPermissions.User, BuddyPermissions write = BuddyPermissions.User)
        {
            return PictureCollection.AddAsync(this.Client, caption, pictureData, contentType, location, read, write);
        }

        public Task<SearchResult<Picture>> FindAsync(string caption = null, string contentType = null, string ownerID = null, BuddyGeoLocationRange location = null, int maxResults = 100)
        {
            return base.FindAsync(ownerID, null, null, location, maxResults, null, (p) =>
            {
                p["caption"] = caption;
                p["contentType"] = contentType;
            });
        }
    }
}
