using System.Reflection;
using System.Threading.Tasks;

namespace BuddySDK
{
    [BuddyObjectPath("/users/lists")]
    public class UserList : BuddyBase
    {
        internal UserList(BuddyClient client = null)
            : base(client)
        {
        }

        public UserList(string id, BuddyClient client = null)
            : base(id, client)
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

        public async Task<UserListItem> AddUserAsync(User user, BuddyGeoLocation location, string tag = null)
        {
            var c = new UserListItem(this.GetObjectPath() + typeof(UserListItem).GetCustomAttribute<BuddyObjectPathAttribute>(true).Path, this.Client)
            {
                UserID = user.ID,
                Location = location,
                Tag = tag
            };

            var r = await c.SaveAsync();

            return r.Convert<UserListItem>(b => c).Value;
        }
    }

    
}