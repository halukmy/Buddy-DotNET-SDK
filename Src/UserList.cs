using System.Reflection;
using System.Threading.Tasks;

namespace BuddySDK
{
    [BuddyObjectPath("/users/lists")]
    public class UserList : BuddyBase
    {
        public UserList()
            : base()
        {
        }

        public UserList(BuddyClient client)
            : base(null, client)
        {

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

        private UserListItemCollection _users;
        public UserListItemCollection Users
        {
            get
            {
                if (_users == null)
                {
                    _users = new UserListItemCollection(this.GetObjectPath(), this.Client);
                }

                return _users;
            }
        }

        public async Task<UserListItem> AddUserAsync(User user, BuddyGeoLocation location, string defaultMetadata = null)
        {
            var c = new UserListItem(this.GetObjectPath() + typeof(UserListItem).GetCustomAttribute<BuddyObjectPathAttribute>(true).Path, this.Client)
            {
                UserID = user.ID,
                Location = location,
                DefaultMetadata = defaultMetadata
            };

            var r = await c.SaveAsync();

            return r.Convert<UserListItem>(b => c).Value;
        }
    }

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

        public Task<SearchResult<UserList>> FindAsync(string name = null,
            BuddyGeoLocationRange location = null, int maxResults = 100, string pagingToken = null)
        {
            return base.FindAsync(null, null, null, location, maxResults, pagingToken, (p) =>
            {
                p["name"] = name;
            });
        }
    }
}