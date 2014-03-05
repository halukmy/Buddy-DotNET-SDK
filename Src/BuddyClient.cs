using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Globalization;
using BuddySDK.BuddyServiceClient;
using System.Reflection;
using System.Collections;
using Newtonsoft.Json;


#if WINDOWS_PHONE
using System.Net;
#else


#endif
using System.Xml.Linq;
using System.Threading.Tasks;
using System.IO;



namespace BuddySDK
{

   
    public class BuddyClient
    {

        public event EventHandler<ServiceExceptionEventArgs> ServiceException;
        public event EventHandler<ConnectivityLevelChangedArgs> ConnectivityLevelChanged;
        public event EventHandler<CurrentUserChangedEventArgs> CurrentUserChanged;
        public event EventHandler LastLocationChanged;
        public event EventHandler AuthorizationLevelChanged;
        public event EventHandler AuthorizationNeedsUserLogin; 


        private bool _gettingToken = false;
        private AuthenticatedUser _user;
        private static bool _crashReportingSet = false;
        private BuddyClientFlags _flags;


        private class AppSettings
        {
            public string AppID {get;set;}
            public string AppKey {get;set;}

            public string ServiceUrl { get; set; }
            public string DeviceToken { get; set; }
            public DateTime? DeviceTokenExpires { get; set; }

            public string UserToken { get; set; }
            public DateTime? UserTokenExpires { get; set; }
            public string UserID {get;set;}
            public string LastUserID {get;set;}

            public AppSettings() {

            }

            public AppSettings(string appId, string appKey) {

                AppID = appId;
                AppKey = appKey;

                if (appId != null) {
                    Load();
                }
            }

            public void Clear() {
                if (AppID != null) {
                    PlatformAccess.Current.ClearUserSetting (AppID);
                    ServiceUrl = null;
                    DeviceToken = null;
                    DeviceTokenExpires = null;
                    LastUserID = null;
                    ClearUser ();
                }
            }

            public void ClearUser() {
                if (AppID != null) {
                    UserToken = null;
                    UserTokenExpires = null;
                    UserID = null;
                    Save ();
                }
            }

            public void Save() {

                if (AppID == null) {
                    return;
                }

                var json = JsonConvert.SerializeObject (this);
                PlatformAccess.Current.SetUserSetting (AppID, json);
            }

            public void Load() {
                if (AppID == null)
                    return;

                var json = PlatformAccess.Current.GetUserSetting (AppID);

                if (json == null)
                    return;

                try {
                    var settings = JsonConvert.DeserializeObject<AppSettings> (json);

                    // copy over the properties
                    //
                    foreach (var prop in settings.GetType().GetProperties()) {
                        prop.SetValue (this, prop.GetValue (settings));
                    }
                }
                catch {
                    // we don't want to have an app not be able to start because settings got corrupted
                }
            }
        }


        /// <summary>
        /// The service we use to call the Buddy backend.
        /// </summary>
        /// 
        private BuddyServiceClientBase _service;

        private static string _WebServiceUrl;
        protected static string WebServiceUrl {
            get {
                return _WebServiceUrl ?? "https://api.buddyplatform.com";
            }
            set {
                _WebServiceUrl = value;
            }

        }

        /// <summary>
        /// Gets the application ID for this client.
        /// </summary>
        public string AppId { get; protected set; }

        /// <summary>
        /// Gets the application secret key for this client.
        /// </summary>
        public string AppKey { get; protected set; }



        protected string AccessToken
        {
            get
            {
                return GetAccessToken ().Result;

            }
        }

        public AuthenticationLevel AuthLevel {
            get;
            private set;
        }

       

        public AuthenticatedUser User
        {
            get
            { 
                return GetUser();
            }
            private set
            {
                string priorId = null;
                if (value != null)
                {
                    _appSettings.UserToken = value.AccessToken;
                    _appSettings.UserID = value.ID;

                    priorId = _appSettings.LastUserID;

                    if (_user == null) {
                        priorId = "";
                    }


                    _appSettings.LastUserID = value.ID;
                    _appSettings.Save ();

                }
                else
                {
                    priorId = _appSettings.LastUserID ?? "";
                    _appSettings.ClearUser ();
                }
                _user = value;

                if (priorId != null) {
                    OnCurrentUserChanged (value, priorId == "" ? null : priorId);
                }

                OnAccessTokenChanged (_appSettings.UserToken, AccessTokenType.User);
            }
        }


        /// <summary>
        /// The last location value for this device.  Location tracking
        /// must be enabled to use this property.
        /// </summary>
        /// <value>The last location.</value>
        public BuddyGeoLocation LastLocation {
            get {
                if (!ShouldTrackLocation) {
                    throw new InvalidOperationException ("Location tracking must be enabled.");
                }
                return PlatformAccess.Current.LastLocation;
            }
        }


        /// <summary>
        /// Enables or disables tracking of device location.
        /// </summary>
        /// <value><c>true</c> if should track location; otherwise, <c>false</c>.</value>
        public bool ShouldTrackLocation {
            get {
                return _flags.HasFlag(BuddyClientFlags.AutoTrackLocation);
            }
            set {
                if (value != ShouldTrackLocation) {
                    if (value) {
                        _flags |= BuddyClientFlags.AutoTrackLocation;
                    } else {
                        _flags &= ~BuddyClientFlags.AutoTrackLocation;
                    }
                    PlatformAccess.Current.TrackLocation (value);
                }
            }
        }

        private AppSettings _appSettings;
        bool _userInitialized;

        public BuddyClient(string appid, string appkey, BuddyClientFlags flags = BuddyClientFlags.Default)
        {
            if (String.IsNullOrEmpty(appid))
                throw new ArgumentException("Can't be null or empty.", "appName");
            if (String.IsNullOrEmpty(appkey))
                throw new ArgumentException("Can't be null or empty.", "AppKey");

            this.AppId = appid.Trim();
            this.AppKey = appkey.Trim();
            //this._flags = flags;

            _appSettings = new AppSettings (appid, appkey);


            UpdateAccessLevel();

            if (flags.HasFlag (BuddyClientFlags.AutoCrashReport)) {
                InitCrashReporting ();
            }
            _flags = flags;
            if (ShouldTrackLocation) {
                PlatformAccess.Current.TrackLocation (true);
            }

            PlatformAccess.Current.LocationUpdated += (sender, e) => {

                if (LastLocationChanged != null) {
                    LastLocationChanged(this,e);
                }
            };
            
        }

      

        internal class DeviceRegistration
        {
            public string AccessToken { get; set; }
            public string ServiceRoot { get; set; }
        }


        internal async Task<string> GetAccessToken() {

            if (!_gettingToken)
            {
                try
                {
                    _gettingToken = true;

                    if (_appSettings.UserToken != null) {
                        return _appSettings.UserToken;
                    }
                    else if (_appSettings.DeviceToken != null) {
                        return _appSettings.DeviceToken;
                    }

                    _appSettings.DeviceToken = await GetDeviceToken();
                    _appSettings.Save();
                    return _appSettings.DeviceToken;
                }
                finally
                {
                    _gettingToken = false;
                }
            }
            else
            {
                return _appSettings.UserToken ?? _appSettings.DeviceToken;
            }
        }


        private async Task<string> GetDeviceToken()
        {

            var dr = await CallServiceMethodHelper<DeviceRegistration, DeviceRegistration> (
                "POST",
                "/devices",
                new
                {
                    AppId = AppId,
                    AppKey = AppKey,
                    ApplicationId = PlatformAccess.Current.ApplicationID,
                    Platform = PlatformAccess.Current.Platform,
                    UniqueID = PlatformAccess.Current.DeviceUniqueId,
                    Model = PlatformAccess.Current.Model,
                    OSVersion = PlatformAccess.Current.OSVersion
                },
                completed: (r1, r2) => { 
                    if (r2.IsSuccess && r2.Value.ServiceRoot != null)
                    {
                        _service.ServiceRoot = r2.Value.ServiceRoot;
                        _appSettings.ServiceUrl = r2.Value.ServiceRoot;
                    }
                    else if (!r2.IsSuccess){
                        ClearCredentials();
                    }
                });

           
            if (!dr.IsSuccess) {
                return null;
            }
            return dr.Value.AccessToken;
        }

        private AuthenticatedUser GetUser() {


            if (!_userInitialized) {
                _userInitialized = true;
                if (_appSettings.UserID != null && _appSettings.UserToken != null) {
                    User = new AuthenticatedUser (_appSettings.UserID, _appSettings.UserToken, this);
                    return User;
                }
            }

            if (_user == null) {
                this.OnAuthorizationFailure (null);
            } else if (_user != null && !_user.IsPopulated) {
                // make sure the user exists.
                //
                _user.FetchAsync ().ContinueWith ((r) => {
                });

               
            }
            return _user;
        }

        protected virtual void OnCurrentUserChanged (AuthenticatedUser newUser, string lastUserId)
        {
            User lastUser = null;

            if (lastUserId != null) {
                lastUser = new User (lastUserId);
            }
            if (CurrentUserChanged != null) {
                CurrentUserChanged(this, new CurrentUserChangedEventArgs(newUser, lastUser));
            }
        }

        private string GetRootUrl() {
            string setting = PlatformAccess.Current.GetConfigSetting("RootUrl");
            var userSetting = _appSettings.ServiceUrl;
            return userSetting ?? setting ?? WebServiceUrl;
        }

        private void InitCrashReporting() {

            if (!_crashReportingSet) {

                _crashReportingSet = true;
                AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
                    var ex = e.ExceptionObject as Exception;

                    // need to do this synchronously or the OS won't wait for us.
                    var t = AddCrashReportAsync (ex);
                   
                    // wait up to a second to let it go out
                    t.Wait(TimeSpan.FromSeconds(2));

                };
            }

        }

    
        internal Task<BuddyResult<T>> CallServiceMethod<T>(string verb, string path, object parameters = null, bool allowThrow = false) {


            return Task.Run<BuddyResult<T>> (async () => {

                var dictionary = BuddyServiceClientBase.ParametersToDictionary(parameters);
                var loc = PlatformAccess.Current.LastLocation;
                if (!dictionary.ContainsKey("location") && loc != null) {
                    dictionary["location"] = loc.ToString();
                }

                var service = await Service();
                var bcrTask = service.CallMethodAsync<T>(verb, path, dictionary);

                var bcr = bcrTask.Result;

                var result = new BuddyResult<T> ();
                result.RequestID = bcr.RequestID;

                if (bcr.Error != null) {
                    BuddyServiceException buddyException = null;

                    switch (bcr.StatusCode) {
                        case 0: 
                            buddyException = new BuddyNoInternetException (bcr.Error);
                            break;
                        case 401:
                        case 403:
                            buddyException = new BuddyUnauthorizedException (bcr.Error, bcr.Message, bcr.ErrorNumber);
                            break;
                        default:
                            buddyException = new BuddySDK.BuddyServiceException (bcr.Error, bcr.Message, bcr.ErrorNumber);
                            break;
                    }

                    var tsc = new TaskCompletionSource<bool>();

                   
                    PlatformAccess.Current.InvokeOnUiThread(() => {

                        var r = false;
                        if (OnServiceException(this, buddyException)) {
                            r = true;
                        }
                        tsc.TrySetResult(r);
                    });

                    if (tsc.Task.Result && allowThrow) {
                        throw buddyException;
                    }

                    buddyException.StatusCode = bcr.StatusCode;
                    result.Error = buddyException;

                } else {
                    result.Value = bcr.Result;
                }
                return result;
            });

        }


        internal async Task<BuddyResult<T2>> CallServiceMethodHelper<T1, T2>(
            string verb, 
            string path, 
            object parameters = null, 
            Func<T1, T2> map = null, 
            Action<BuddyResult<T1>, BuddyResult<T2>> completed = null) {

            BuddyResult<T1> r1 = null;
            BuddyResult<T2> r2 = null;

            if (typeof(T1) == typeof(T2)) {
                r2 = await CallServiceMethod<T2> (verb, path, parameters);
            } else {
                r1 = await CallServiceMethod<T1> (verb, path, parameters);

               
                if (map == null) {
                    map = (t1) => {
                        return (T2)(object)r1.Value;
                    };
                }

                r2 = r1.Convert<T2> (map);

            }

            if (completed != null) {
                PlatformAccess.Current.InvokeOnUiThread (() => completed (r1, r2));
            }
            return r2;
        }

      

     

        private void ClearCredentials(bool clearUser = true, bool clearDevice = true) {

            if (clearDevice) {

                _appSettings.Clear ();
                if (_service != null)
                {
                    _service.ServiceRoot = GetRootUrl();
                }
            }

            if (clearUser) {
                _appSettings.ClearUser ();
            }

            UpdateAccessLevel ();
        }

        private void LoadState() {

            if (_appSettings.DeviceToken != null) {
                OnAccessTokenChanged (_appSettings.DeviceToken, AccessTokenType.Device);
            }
            var id = _appSettings.LastUserID;
            if (_appSettings.UserToken != null && id != null) {
                User = new AuthenticatedUser (id, _appSettings.UserToken, this);
            }
        }

        internal async Task<BuddyServiceClientBase> Service()
        {
            using (await new AsyncLock().LockAsync())
            {
                if (this._service != null) return this._service;

                var root = GetRootUrl();

                this._service = BuddyServiceClientBase.CreateServiceClient(this, root);

                this._service.ServiceException += async (object sender, ExceptionEventArgs e) =>
                {

                    if (e.Exception is BuddyUnauthorizedException)
                    {
                        ClearCredentials(true, true);
                    }

                };
                return _service;
            }
        }

        protected enum AccessTokenType {
            Device,
            User
        }

        protected virtual void OnAccessTokenChanged(string token, AccessTokenType tokenType, DateTime? expires = null) {

           
            UpdateAccessLevel();
        }

        private ConnectivityLevel? _connectivity;
        public ConnectivityLevel ConnectivityLevel {
            get {
                if (_connectivity == null) {
                    return PlatformAccess.Current.ConnectionType;
                }
                return _connectivity.GetValueOrDefault(ConnectivityLevel.None);
            }
            private set {
                _connectivity = value;
            }
        }


        private async Task CheckConnectivity(TimeSpan waitTime) {
            var r = await CallServiceMethod<string>("GET", "/service/ping");

            if (r != null && r.IsSuccess)
            {
                PlatformAccess.Current.InvokeOnUiThread(async () => {
                    await OnConnectivityChanged(PlatformAccess.Current.ConnectionType);
                });
            }
            else
            {
                // wait a bit and try again
                //
                Thread.Sleep(waitTime);
                await CheckConnectivity(waitTime);
            }
        }

        protected virtual async Task OnConnectivityChanged(ConnectivityLevel level) {
            using (await new AsyncLock().LockAsync())
            {
                if (level == _connectivity)
                {
                    return;
                }

                if (ConnectivityLevelChanged != null)
                {
                    ConnectivityLevelChanged(this, new ConnectivityLevelChangedArgs
                    {
                        ConnectivityLevel = level
                    });
                }

                _connectivity = level;
                
                switch (level)
                {
                    case ConnectivityLevel.None:
                        await CheckConnectivity(TimeSpan.FromSeconds(1));
                        break;
                }
            }
        }
      
        protected bool OnServiceException(BuddyClient client, BuddyServiceException buddyException) {


            // first see if it's an auth failure.
            //
            if (buddyException is BuddyUnauthorizedException) {
                client.OnAuthorizationFailure ((BuddyUnauthorizedException)buddyException);
                return false;
            } else if (buddyException is BuddyNoInternetException) {
#pragma warning disable 4014
                OnConnectivityChanged (ConnectivityLevel.None); // We don't care about async here.
#pragma warning restore 4014
                return false;
            }

            bool result = false;

            if (ServiceException != null) {
                var args = new ServiceExceptionEventArgs (buddyException);
                ServiceException (this, args);
                result = args.ShouldThrow;
            } 
            return result;
        }

        private int _processingAuthFailure = 0;

        internal virtual void OnAuthorizationFailure(BuddyUnauthorizedException exception) {

            if (_processingAuthFailure > 0) {
                return;
            }

            lock (this) {

                _processingAuthFailure++;
                try {
                    bool showLoginDialog = exception == null;
                    #pragma warning disable 4014
                    if (exception != null) {
                        switch (exception.Error) {

                        case "AuthAppCredentialsInvalid":
                        case "AuthAccessTokenInvalid":
                            ClearCredentials(false, true);
                            break;
                        case "AuthUserAccessTokenRequired":
                            ClearCredentials(true, false);
                            showLoginDialog = true;
                            break;
                        }
                    }
                    #pragma warning restore 4014

                    if (showLoginDialog) {
                        _processingAuthFailure++;
                        PlatformAccess.Current.InvokeOnUiThread (() => {

                            if (this.AuthorizationNeedsUserLogin != null) {
                                this.AuthorizationNeedsUserLogin (this, new EventArgs ());
                            }
                            _processingAuthFailure--;
                        });
                    }
                }
                finally {
                    _processingAuthFailure--;
                }
            }
        }

        protected virtual void OnAuthLevelChanged() {
           
            PlatformAccess.Current.InvokeOnUiThread (() => {

                if (this.AuthorizationLevelChanged != null) {
                    this.AuthorizationLevelChanged (this, EventArgs.Empty);
                }
            });
        }

        private void UpdateAccessLevel() {

            var old = AuthLevel;
            AuthenticationLevel authLevel = AuthenticationLevel.None;
            if (_appSettings.DeviceToken != null) authLevel = AuthenticationLevel.Device;
            if (_appSettings.UserToken != null) authLevel = AuthenticationLevel.User;
            AuthLevel = authLevel;

            if (old != authLevel) {
                OnAuthLevelChanged ();
            }

        }


        // service
        //
        public Task<BuddyResult<string>> PingAsync()
        {
            return CallServiceMethod<string>("GET", "/service/ping");
        }

        // User auth.
        public System.Threading.Tasks.Task<BuddyResult<AuthenticatedUser>> CreateUserAsync(
            string username, 
            string password, 
            string firstName = null, 
            string lastName = null,
            string email = null,
            BuddySDK.UserGender? gender = null, 
            DateTime? dateOfBirth = null, 
            string defaultMetadata = null)
        {

            if (String.IsNullOrEmpty(username))
                throw new ArgumentException("Can't be null or empty.", "username");
            if (password == null)
                throw new ArgumentNullException("password");
            if (dateOfBirth > DateTime.Now)
                throw new ArgumentException("dateOfBirth must be in the past.", "dateOfBirth");

           
            var task = new Task<BuddyResult<AuthenticatedUser>>(() =>
            {

                var rt = CallServiceMethod<IDictionary<string, object>>("POST", "/users", new
                {
                    firstName = firstName,
                    lastName = lastName,
                    username = username,
                    password = password,
                    email = email,
                    gender = gender,
					dateOfBirth = dateOfBirth,
                    defaultMetadata = defaultMetadata
                });

                var r = rt.Result;

                return r.Convert(d => {

                        var user = new AuthenticatedUser( (string)r.Value["ID"], (string)r.Value["accessToken"], this);
                    this.User = user;
                    return user;
                });
            });
            task.Start ();
            return task;
         
        }

        /// <summary>
        /// Login an existing user with their username and password. Note that this method internally does two web-service calls, and the IAsyncResult object
        /// returned is only valid for the first one.
        /// </summary>
        /// <param name="username">The username of the user. Can't be null or empty.</param>
        /// <param name="password">The password of the user. Can't be null.</param>
        /// <returns>A Task&lt;AuthenticatedUser&gt;that can be used to monitor progress on this call.</returns>
        public System.Threading.Tasks.Task<BuddyResult<AuthenticatedUser>> LoginUserAsync(string username, string password)
        {
            return LoginUserCoreAsync<AuthenticatedUser>("/users/login", new
            {
                Username = username,
                Password = password
                }, (result) => new AuthenticatedUser((string)result["ID"], (string)result["accessToken"], this));
        }

        public System.Threading.Tasks.Task<BuddyResult<SocialAuthenticatedUser>> SocialLoginUserAsync(string identityProviderName, string identityID, string identityAccessToken)
        {
            return LoginUserCoreAsync<SocialAuthenticatedUser>("/users/login/social", new
                    {
                        IdentityProviderName = identityProviderName,
                        IdentityID = identityID,
                        IdentityAccessToken = identityAccessToken
                }, (result) => new SocialAuthenticatedUser((string)result["ID"], (string)result["accessToken"], (bool)result["isNew"], this));
        }

        private async System.Threading.Tasks.Task<BuddyResult<T>> LoginUserCoreAsync<T>(string path, object parameters, Func<IDictionary<string, object>, T> createUser) where T : AuthenticatedUser
        {
            return await CallServiceMethodHelper<IDictionary<string, object>, T>(
                "POST",
                path,
                parameters,
                map: d => createUser (d),
                completed: (r1, r2) => {

                    var u = r2.Value;

                    if (u != null){
                        u.Update(r1.Value);
                        User = u;
                    }

                });
        }

        private async Task<BuddyResult<bool>> LogoutInternal() {

            IDictionary<string,object> dresult = null;

            var r = await CallServiceMethodHelper<IDictionary<string,object>, bool>(
                "POST",
                "/users/me/logout",
                map: (d) => {
                    dresult = d;
                    return d != null;

                });

            if (r.IsSuccess) {

                this.User = null;
                ClearCredentials (true, false);
              
                if (dresult != null && dresult.ContainsKey("accessToken")) {
                    var token = dresult ["accessToken"] as string;
                    DateTime? expires = null;
                    if (dresult.ContainsKey("accessTokenExpires")) {
                        object dt = dresult ["accessTokenExpires"];
                        expires =  (DateTime)dt;
                    }
                    _appSettings.DeviceToken = token;
                    _appSettings.Save ();
                    OnAccessTokenChanged(token, AccessTokenType.Device, expires);
                }
            }
            return r;
        }

        public Task<BuddyResult<bool>> LogoutUserAsync() {
            return LogoutInternal ();
           
        }

      

        private Metadata _appMetadata;

        public Metadata AppMetadata
        {
            get
            {
                if (_appMetadata == null)
                {
                    _appMetadata = new Metadata("app", this);
                }
                return _appMetadata;
            }
        }

        //
        // Collections
        //

        private  CheckinCollection _checkins;
        public  CheckinCollection Checkins
        {
            get
            {
                if (_checkins == null)
                {
                    _checkins = new CheckinCollection(this);
                }
                return _checkins;
            }
        }


        private  LocationCollection _locations;

        public  LocationCollection Locations
        {
            get
            {
                if (_locations == null)
                {
                    _locations = new LocationCollection(this);
                }
                return _locations;
            }
        }

        private MessageCollection _messages;

        public MessageCollection Messages
        {
            get
            {
                if (_messages == null)
                {
                    _messages = new MessageCollection(this);
                }
                return _messages;
            }
        }

        private  PictureCollection _pictures;

        public  PictureCollection Pictures
        {
            get
            {
                if (_pictures == null)
                {
                    _pictures = new PictureCollection(this);
                }
                return _pictures;
            }
        }

        private  AlbumCollection _albums;

        public  AlbumCollection Albums
        {
            get
            {
                if (_albums == null)
                {
                    _albums = new AlbumCollection(this);
                }
                return this._albums;
            }
        }


        private UserCollection _users;

     
        public  UserCollection Users
        {
            get
            {
                if (_users == null)
                {
                    _users = new UserCollection(this);
                }
                return this._users;
            }
        }


        private UserListCollection _userLists;

        public UserListCollection UserLists
        {
            get
            {
                if (_userLists == null)
                {
                    _userLists = new UserListCollection(this);
                }
                return this._userLists;
            }
        }
      

        //
        // Metrics
        //

        private class MetricsResult
        {
            public string id { get; set; }
            public bool success { get; set; }
        }


        public Task<BuddyResult<string>> RecordMetricAsync(string key, IDictionary<string, object> value = null, TimeSpan? timeout = null)
        {

            int? timeoutInSeconds = null;

            if (timeout != null)
            {
                timeoutInSeconds = (int)timeout.Value.TotalSeconds;
            }

            return Task.Run<BuddyResult<string>>(() =>
            {

                var r = CallServiceMethod<MetricsResult>("POST", String.Format("/metrics/events/{0}", Uri.EscapeDataString(key)), new
                {
                    value = value,
                    timeoutInSeconds = timeoutInSeconds
                });
               
                
                return r.Result.Convert((mr) => mr.id);

              
            });
        }

        private class CompleteMetricResult
        {
            public long? elaspedTimeInMs { get; set; }
        }

        public Task<BuddyResult<TimeSpan?>> RecordTimedMetricEndAsync(string timedMetricId)
        {
            return Task<TimeSpan?>.Run(() =>
            {

                 var r = CallServiceMethod<CompleteMetricResult>("DELETE", String.Format("/metrics/events/{0}", Uri.EscapeDataString(timedMetricId)));


                    return r.Result.Convert(cmr =>  {
                        TimeSpan? elapsedTime = null;

                        if (cmr.elaspedTimeInMs != null) {
                            elapsedTime = TimeSpan.FromMilliseconds(cmr.elaspedTimeInMs.Value);
                        }

                        return elapsedTime;

                    });
                
            });
        }

        public Task<BuddyResult<bool>> AddCrashReportAsync (Exception ex, string message = null)
        {

            return Task.Run<BuddyResult<bool>> (() => {
                if (ex == null) return new BuddyResult<bool>();



                try {
                    var r = CallServiceMethod<string>(
                        "POST", 
                        "/devices/current/crashreports", 
                            new {
                                stackTrace = ex.ToString(),
                                message = message
                        }, allowThrow:false);
                    return r.Result.Convert(s => true);
                }
                catch {

                }
                return new BuddyResult<bool> {
                    Value = false
                };
            });
        }

    }

    public enum AuthenticationLevel {
        None = 0,
        Device,
        User
    }
}
