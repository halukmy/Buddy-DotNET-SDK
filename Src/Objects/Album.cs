using Newtonsoft.Json;
using System.Reflection;
using System.Threading.Tasks;

namespace BuddySDK
{
    [BuddyObjectPath("/albums")]
	public class Album : BuddyBase
	{
        internal Album(BuddyClient client = null)
            : base(client)
        {
        }

		public Album(string id, BuddyClient client= null)
			: base(id, client)
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
		
        public async Task<AlbumItem> AddItemAsync(string itemId, string caption, BuddyGeoLocation location, string tag = null)
		{
	
			var c = new AlbumItem(this.GetObjectPath() + typeof(AlbumItem).GetCustomAttribute<BuddyObjectPathAttribute>(true).Path, this.Client)
			{
				ItemId = itemId,
                Caption = caption,
				Location = location,
				Tag = tag
			};

            var r = await c.SaveAsync();
					
            return r.Convert<AlbumItem> (b => c).Value;
		}
	}

   
}