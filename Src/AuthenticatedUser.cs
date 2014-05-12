using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using BuddySDK.BuddyServiceClient;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;


namespace BuddySDK
{
    public class AuthenticatedUser : User
    {
        /// <summary>
        /// Gets the unique user token that is the secret used to log-in this user. Each user has a unique ID, a secret user token and a user/pass combination.
        /// </summary>
        public string AccessToken {
            get
            {
                return GetValueOrDefault<string>("AccessToken");
            }
            protected set
            {
                SetValue<string>("AccessToken", value);
            }
        }

        internal AuthenticatedUser(string id, string accessToken, BuddyClient client)
            : base(id, client)
        {
            this.AccessToken = accessToken;
        }

        public override string ToString ()
        {
            return base.ToString () + ", Email: " + this.Email;
        }


        public Task<BuddyResult<bool>> AddIdentityAsync(string identityProviderName, string identityID)
        {
            return AddRemoveIdentityCoreAsync("POST", "/users/me/identities/" + Uri.EscapeDataString(identityProviderName), new
                {
                    IdentityID = identityID
                });
        }

        public Task<BuddyResult<bool>> RemoveIdentityAsync(string identityProviderName, string identityID)
        {
            return AddRemoveIdentityCoreAsync("DELETE", "/user/me/identities/" + Uri.EscapeDataString(identityProviderName), new { IdentityID = identityID });
        }

        private Task<BuddyResult<bool>> AddRemoveIdentityCoreAsync(string verb, string path, object parameters)
        {
            return Task.Run<BuddyResult<bool>>(() =>
                {
                    var r = Client.CallServiceMethod<string>(verb, path, parameters);
                    return r.Result.Convert(s  => r.Result.IsSuccess);
                });

        }

        public Task<BuddyResult<IEnumerable<string>>> GetIdentitiesAsync(string identityProviderName)
        {
            return Client.CallServiceMethod<IEnumerable<string>>("GET",  "/users/me/identities/" + Uri.EscapeDataString(identityProviderName));
        }

    }
}
