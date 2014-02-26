using Newtonsoft.Json;
using System.Reflection;
using System.Threading.Tasks;

namespace BuddySDK
{
    [BuddyObjectPath("/albums")]
	public class Album : BuddyBase
	{
        public Album()
            : base()
        {
        }

		public Album(BuddyClient client)
			: base(client)
		{
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

		[Newtonsoft.Json.JsonProperty("name")]
		public string Name
		{
			get
			{
				return GetValueOrDefault<string>("Name");
			}
			set
			{
				SetValue<string>("Name", value, checkIsProp: false);
			}
		}

		private AlbumItemCollection _items;
        public AlbumItemCollection Items
        {
            get
            {
                if (_items == null)
                {
					_items = new AlbumItemCollection(this.GetObjectPath(), this.Client);
                }

                return _items;
            }
        }
		
        public async Task<AlbumItem> AddItemAsync(string itemId, string caption, BuddyGeoLocation location, string defaultMetadata = null)
		{
	
			var c = new AlbumItem(this.GetObjectPath() + typeof(AlbumItem).GetCustomAttribute<BuddyObjectPathAttribute>(true).Path, this.Client)
			{
				ItemId = itemId,
                Caption = caption,
				Location = location,
				DefaultMetadata = defaultMetadata
			};

            var r = await c.SaveAsync();
					
            return r.Convert<AlbumItem> (b => c).Value;
		}
	}

    public class AlbumCollection : BuddyCollectionBase<Album>
    {
        public AlbumCollection()
            : base()
        {
        }

        internal AlbumCollection(BuddyClient client)
            : base(null, client)
        {
        }

        public async Task<BuddyResult<Album>> AddAsync(string name, string caption,
            BuddyGeoLocation location, string defaultMetadata = null, BuddyPermissions readPermissions = BuddyPermissions.User, BuddyPermissions writePermissions = BuddyPermissions.User)
        {
            var c = new Album(this.Client)
            {
                Name = name,
                Caption = caption,
                Location = location,
                DefaultMetadata = defaultMetadata,
                ReadPermissions = readPermissions,
                WritePermissions = writePermissions
            };

            var r = await c.SaveAsync();
            return r.Convert (b => c);
        }

        public Task<SearchResult<Album>> FindAsync(string name = null, string caption = null,
            BuddyGeoLocationRange location = null, int maxResults = 100, string pagingToken = null)
        {
            return base.FindAsync(null, null, null, location, maxResults, pagingToken, (p) =>
            {
                p["name"] = name;
                p["caption"] = caption;
            });
        }
    }
}