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
            if (_creds != null)
            {
                throw new InvalidOperationException("Already initalized.");
            }
            _creds = new Tuple<string, string, BuddyClientFlags>(appId, appKey, flags);

        }

        public static Task<BuddyResult<AuthenticatedUser>> CreateUserAsync(string username, string password, string name = null, string email = null, UserGender? gender = null, DateTime? dateOfBirth = null, string defaultMetadata = null) {
            return Instance.CreateUserAsync (username, password, name, email, gender, dateOfBirth, defaultMetadata : defaultMetadata);
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
        // Collections.
        //

        private static CheckinCollection _checkins;
        public static CheckinCollection Checkins
        {
            get
            {
                if (_checkins == null)
                {
                    _checkins = new CheckinCollection(Instance);
                }
                return _checkins;
            }
        }


        private static LocationCollection _locations;

        public static LocationCollection Locations
        {
            get
            {
                if (_locations == null)
                {
                    _locations = new LocationCollection(Instance);
                }
                return _locations;
            }
        }

        private static PictureCollection _pictures;

        public static PictureCollection Pictures
        {
            get
            {
                if (_pictures == null)
                {
                    _pictures = new PictureCollection(Instance);
                }
                return _pictures;
            }
        }

        private static AlbumCollection _albums;

        public static AlbumCollection Albums
        {
            get
            {
                if (_albums == null)
                {
                    _albums = new AlbumCollection(Instance);
                }

                return _albums;
            }
        }


        private static UserListCollection _userLists;

        public static UserListCollection UserLists
        {
            get
            {
                if (_userLists == null)
                {
                    _userLists = new UserListCollection(Instance);
                }
                return _userLists;
            }
        }
      


    }

   

   
}
