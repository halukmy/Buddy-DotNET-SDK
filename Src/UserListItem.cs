using System.Reflection;
using System.Threading.Tasks;

namespace BuddySDK
{
    [BuddyObjectPath("/items")]
    public class UserListItem : BuddyBase
    {
        public UserListItem()
            : base()
        {
        }

        public UserListItem(string path, BuddyClient client = null)
            : base(null, client)
        {
            this.path = path;
        }

        [Newtonsoft.Json.JsonProperty("userID")]
        public string UserID
        {
            get
            {
                return GetValueOrDefault<string>("UserID");
            }
            set
            {
                SetValue<string>("UserID", value, checkIsProp: false);
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
    }

    public class UserListItemCollection : BuddyCollectionBase<UserListItem>
    {
        internal UserListItemCollection(string parentObjectPath, BuddyClient client)
            : base(parentObjectPath + typeof(UserListItem).GetCustomAttribute<BuddyObjectPathAttribute>(true).Path, client)
        {
        }

        public Task<SearchResult<UserListItem>> FindAsync(BuddyGeoLocationRange location = null, int maxResults = 100, string pagingToken = null)
        {
            return base.FindAsync(null, null, null, location, maxResults, pagingToken, (p) =>
            {
            });
        }
    }
}