using System.Reflection;
using System.Threading.Tasks;

namespace BuddySDK
{
    [BuddyObjectPath("/items")]
    public class UserListItem : BuddyBase
    {
        internal UserListItem(BuddyClient client = null)
            : base(client)
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

   
}