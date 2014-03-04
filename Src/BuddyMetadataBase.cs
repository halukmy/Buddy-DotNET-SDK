using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuddySDK
{
    public abstract class BuddyMetadataBase
    {
        private BuddyClient _client;
        protected BuddyClient Client
        {
            get
            {
                return _client ?? Buddy.Instance;
            }
        }

        private string _metadataId;
        protected virtual string MetadataID
        {
            get
            {
                return _metadataId;
            }
            private set
            {
                _metadataId = value;
            }
        }

        protected BuddyMetadataBase(string id, BuddyClient client = null)
        {
            this._metadataId = id;
            
            this._client = client;
            
        }

        private void EnsureID()
        {
            if (MetadataID == null) throw new ArgumentNullException("An ID value is required to set metadata.");
        }

        private string GetMetadataPath(string key = null)
        {
            EnsureID();
            var id = MetadataID;

            
            var path = string.Format("/metadata/{0}", id);

            if (!string.IsNullOrEmpty(key))
            {
                path += string.Format("/{0}", key);
            }

            return path;
        }

      
        private Task<BuddyResult<bool>> SetMetadataCore(string key, object value, BuddyPermissions visibility = BuddyPermissions.Default)
        {
            return Client.CallServiceMethod<bool>("PUT", GetMetadataPath(key), new
            {
                value = value,
                visibility = visibility
            });
        }

        private Task<BuddyResult<bool>> SetMetadataCore(IDictionary<string, object> values, BuddyPermissions visiblity = BuddyPermissions.Default)
        {
            return Client.CallServiceMethod<bool>("PUT", GetMetadataPath(), new
            {
                values = values,
                visibility = visiblity
            });
        }

        public Task<BuddyResult<bool>> SetMetadataAsync(string key, object value, BuddyPermissions visibilty = BuddyPermissions.Default)
        {
            return SetMetadataCore(key, value, visibilty);
        }

        public Task<BuddyResult<bool>> SetMetadataAsync(IDictionary<string, object> values, BuddyPermissions visibility = BuddyPermissions.Default)
        {
            return SetMetadataCore(values, visibility);
        }



        public Task<BuddyResult<object>> GetMetadataValueAsync(string key, BuddyPermissions visibility = BuddyPermissions.Default)
        {
            return Task.Run<BuddyResult<object>>(() =>
            {
                var t2 = GetMetadataItemAsync(key, visibility);

                return t2.Result.Convert<object>(i => i == null ? null : i.Value);


            });
        }

        public Task<BuddyResult<MetadataItem>> GetMetadataItemAsync(string key, BuddyPermissions visibility = BuddyPermissions.Default)
        {
            return Client.CallServiceMethod<MetadataItem>("GET", GetMetadataPath(key), new {visibility = visibility});
        }

        public Task<BuddyResult<MetadataItem>> IncrementMetadataAsync(string key, double? delta = null, BuddyPermissions visibility = BuddyPermissions.Default)
        {
            var path = GetMetadataPath(key) + "/increment";

            var r = Client.CallServiceMethod<MetadataItem>("POST", path,
                     new
                     {
                         delta = delta,
                         visibility = visibility
                     });

            return r;
        }

        public Task<BuddyResult<bool>> DeleteMetadataAsync(string key, BuddyPermissions visibility = BuddyPermissions.Default)
        {
            var t = Client.CallServiceMethod<bool>("DELETE", GetMetadataPath(key), new {visibility = visibility});

            return t;
        }

        public Task<SearchResult<MetadataItem>> FindMetadataAsync(
            string key = null, 
            string keyPrefix = null,
            BuddyGeoLocationRange locationRange = null,
            DateRange created = null,
            DateRange lastModified = null,
            BuddyPermissions visibility = BuddyPermissions.Default
            )
        {
           
            return Task.Run<SearchResult<MetadataItem>>(() =>
            {
                IDictionary<string, object> obj = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase){
                        {"visibility", visibility},
                        {"created", created},
                        {"lastModified", lastModified},
                        {"locationRange", locationRange},
                        {"key", key},
                        {"keyPrefix", keyPrefix}
                    };
               
                var r = Client.CallServiceMethod<SearchResult<MetadataItem>>("GET",
                        GetMetadataPath(), obj
                        ).Result;

                if (r.IsSuccess)
                {
                    return r.Value;
                }
                else
                {
                    return new SearchResult<MetadataItem>(r.RequestID, r.Error);
                }
            });
           
        }
    }

}
