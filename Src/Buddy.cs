using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuddySDK
{

    [Flags]
    public enum BuddyClientFlags
    {
        AutoTrackLocation =  0x00000001,
        AutoCrashReport =    0x00000002,
        AllowReinitialize =  0x00000004,
        Default = AutoCrashReport | AutoTrackLocation
    }

    public static class Buddy
    {
        static BuddyClient _client;
        static Tuple<string, string, BuddyClientFlags> _creds;

        public static BuddyClient Instance
        {
            get
            {
                if (_creds == null)
                {
                    throw new InvalidOperationException("Init must be called before accessing Instance.");
                }
                if (_client == null)
                {
                    _client = new BuddyClient(_creds.Item1, _creds.Item2, _creds.Item3);
                }
                return _client;
            }
        }

        public static Task<BuddyResult<IDictionary<string, object>>> CallServiceMethod(string verb, string path, object parameters = null)
        {
            return Instance.CallServiceMethod<IDictionary<string, object>>(verb, path, parameters);
        }

        public static AuthenticatedUser CurrentUser
        {
            get
            {
                return Instance.User;
            }
        }

        public static void RunOnUiThread(Action a) {
            PlatformAccess.Current.InvokeOnUiThread (a);
        }



        // Global Events
        //

        public static event EventHandler AuthorizationLevelChanged {
            add {
                Instance.AuthorizationLevelChanged += value;
            }
            remove {
                Instance.AuthorizationLevelChanged -= value;
            }
        }

        public static event EventHandler AuthorizationNeedsUserLogin {
            add {
                Instance.AuthorizationNeedsUserLogin += value;
            }
            remove {
                Instance.AuthorizationNeedsUserLogin -= value;
            }
        }

        public static event EventHandler<ConnectivityLevelChangedArgs> ConnectivityLevelChanged {
            add {
                Instance.ConnectivityLevelChanged += value;
            }
            remove {
                Instance.ConnectivityLevelChanged -= value;
            }
        }

        public static event EventHandler<CurrentUserChangedEventArgs> CurrentUserChanged {
            add {
                Instance.CurrentUserChanged += value;
            }
            remove {
                Instance.CurrentUserChanged -= value;
            }
        }

        public static event EventHandler LastLocationChanged {
            add {
                Instance.LastLocationChanged += value;
            }
            remove {
                Instance.LastLocationChanged -= value;
            }
        }


        public static event EventHandler<ServiceExceptionEventArgs> ServiceException {
            add {
                Instance.ServiceException += value;
            }
            remove {
                Instance.ServiceException -= value;
            }
        }

        public static void Init(string appId, string appKey, BuddyClientFlags flags = BuddyClientFlags.Default)
        {
            if (_creds != null && !flags.HasFlag(BuddyClientFlags.AllowReinitialize))
            {
                throw new InvalidOperationException("Already initalized.");
            }
            _creds = new Tuple<string, string, BuddyClientFlags>(appId, appKey, flags);

            _client = null;
        }

        public static Task<BuddyResult<AuthenticatedUser>> CreateUserAsync(string username, string password, string firstName = null, string lastName = null, string email = null, UserGender? gender = null, DateTime? dateOfBirth = null, string defaultMetadata = null) {
            return Instance.CreateUserAsync (username, password, firstName, lastName, email, gender, dateOfBirth, defaultMetadata : defaultMetadata);
        }

        public static Task<BuddyResult<AuthenticatedUser>> LoginUserAsync(string username, string password)
        {
            var t = Instance.LoginUserAsync(username, password);

            return t;
        }

        public static Task<BuddyResult<bool>> LogoutUserAsync ()
        {
            return Instance.LogoutUserAsync ();
        }

        public static Task<BuddyResult<SocialAuthenticatedUser>> SocialLoginUserAsync(string identityProviderName, string identityID, string identityAccessToken)
        {
            var t = Instance.SocialLoginUserAsync(identityProviderName, identityID, identityAccessToken);

            return t;
        }

        // 
        // Metrics
        //

        public static Task<BuddyResult<string>> RecordMetricAsync(string key, IDictionary<string, object> value = null, TimeSpan? timeout = null) {
            return Instance.RecordMetricAsync (key, value, timeout);
        }

        public static Task<BuddyResult<TimeSpan?>> RecordTimedMetricEndAsync(string timedMetricId) {
            return Instance.RecordTimedMetricEndAsync (timedMetricId);
        }

        public static Task AddCrashReportAsync (Exception ex, string message = null)
        {
            return Instance.AddCrashReportAsync (ex, message);
        }

        // 
        // Objects
        //
        public static Metadata AppMetadata
        {
            get
            {
                return Instance.AppMetadata;
            }
        }

        // 
        // Collections.
        //

        public static CheckinCollection Checkins
        {
            get
            {
               
                return Instance.Checkins;
            }
        }


        public static LocationCollection Locations
        {
            get
            {
                
                return Instance.Locations;
            }
        }

        public static MessageCollection Messages
        {
            get
            {
                return Instance.Messages;
            }
        }
      
        public static PictureCollection Pictures
        {
            get
            {
              
                return Instance.Pictures;
            }
        }

        public static AlbumCollection Albums
        {
            get
            {
                return Instance.Albums;
            }
        }

        public static UserCollection Users
        {
            get
            {
                return Instance.Users;
            }
        }

        public static UserListCollection UserLists
        {
            get
            {
                return Instance.UserLists;
            }
        }
      


    }

   

   
}
