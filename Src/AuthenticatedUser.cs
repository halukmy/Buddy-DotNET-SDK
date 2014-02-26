using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using BuddyServiceClient;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;

namespace BuddySDK
{
    /// <summary>
    /// Represents a user that has been authenticated with the Buddy Platform. Use this object to interact with the service on behalf of the user.
    /// <example>
    /// <code>
    ///     BuddyClient client = new BuddyClient("APPNAME", "APPPASS");
    ///
    ///     AuthenticatedUser user;
    ///     client.CreateUserAsync((u, state) => {
    ///         user = u;
    ///     }, "username", "password");
    ///     
    ///     AuthenticatedUser user2;
    ///     client.LoginAsync((u, state) => {
    ///         user2 = u;
    ///     }, "username2", "password2");
    /// </code>
    /// </example>
    /// </summary>
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

        [JsonProperty("email")]
        public string Email
        {
            get
            {
                return GetValueOrDefault<string>("Email");
            }
            set
            {
                SetValue("Email", value, checkIsProp: false);
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

    }
}
