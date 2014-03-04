using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace BuddySDK
{
    public class UserCollection : BuddyCollectionBase<User>
    {
        internal UserCollection(BuddyClient client)
            : base(null, client)
        {
        }

        public Task<SearchResult<User>> FindAsync(string userName = null, string email = null, string firstName = null, string lastName = null, BuddyGeoLocationRange locationRange = null, DateRange created = null, DateRange lastModified = null, int pageSize = 100, string pagingToken = null)
        {

            return base.FindAsync(userId: null,
                created: created,
                lastModified: lastModified,
                locationRange: locationRange,
                pagingToken: pagingToken,
                pageSize: pageSize,
                parameterCallback: (p) =>
                {
                    p["username"] = userName;
                    p["email"] = email;
                    p["firstName"] = firstName;
                    p["lastName"] = lastName;

                });

        } 

        public Task<BuddyResult<IEnumerable<User>>> FindByIdentitiesAsync(string identityProviderName, IEnumerable<string> identityIDs = null)
        {
            return Task.Run<BuddyResult<IEnumerable<User>>>(() =>
            {
                var r = Client.CallServiceMethod<IEnumerable<string>>("GET", Path + "/identities", new
                        {
                            IdentityProviderName = identityProviderName,
                            IdentityIDs = identityIDs == null ? null : string.Join("\t", identityIDs)
                        });

                    return r.Result.Convert(uids => uids.Select(uid => new User(uid, Client)));

                
            });

        }
    }
}
