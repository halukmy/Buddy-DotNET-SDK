using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace BuddySDK
{
    public enum AlbumItemType
    {
        Picture,
        Video
    }
    
    [BuddyObjectPath("/items")]
	public class AlbumItem : BuddyBase
	{
        public AlbumItem()
            : base()
        {
        }

        public AlbumItem(string path, BuddyClient client = null)
            : base(null, client)
        {
            this.path = path;
        }

        [Newtonsoft.Json.JsonProperty("caption")]
        public string Caption
		{
			get
			{
                return GetValueOrDefault<string>("Caption");
			}
			set
			{
                SetValue<string>("Caption", value, checkIsProp: false);
			}
		}

        [Newtonsoft.Json.JsonProperty("itemId")]
        public string ItemId
        {
            get
            {
                return GetValueOrDefault<string>("ItemId");
            }
            set
            {
                SetValue<string>("ItemId", value, checkIsProp: false);
            }
        }

        [Newtonsoft.Json.JsonProperty("itemType")]
        public AlbumItemType ItemType
        {
            get
            {
                return GetValueOrDefault<AlbumItemType>("ItemType");
            }
        }

        private readonly string path;
        protected override string Path
        {
            get
            {

                return this.path;
            }
        }

        public Task<BuddyResult<Stream>> GetFileAsync(int? size = null)
		{
            return base.GetFileCoreAsync(GetObjectPath() + "/file", new {size = size});
		}
	}

    public class AlbumItemCollection : BuddyCollectionBase<AlbumItem>
    {
		internal AlbumItemCollection(string parentObjectPath, BuddyClient client)
			: base(parentObjectPath + typeof(AlbumItem).GetCustomAttribute<BuddyObjectPathAttribute>(true).Path, client)
        {
        }

        public Task<SearchResult<AlbumItem>> FindAsync(AlbumItemType? itemType = null, string caption = null,
            BuddyGeoLocationRange location = null, int maxResults = 100, string pagingToken = null)
        {
            return base.FindAsync(null, null, null, location, maxResults, pagingToken, (p) =>
            {
                if (itemType.HasValue) { p["itemType"] = itemType; }
                p["caption"] = caption;
            });
        }
    }
}