using System;
using System.Net;
using System.Threading;
using System.Xml.Linq;
using System.Reflection;
using System.IO.IsolatedStorage;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

#if __ANDROID__
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Provider;
#elif __IOS__
using MonoTouch.CoreLocation;
using MonoTouch.CoreFoundation;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.SystemConfiguration;
#endif



namespace BuddySDK
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public abstract class PlatformAccess
    {
        private int? _uiThreadId;
        protected const string PushChannelName = "BuddyChannel";
        protected const string PushChannelSettingName = "PushChannelUri";

        // device info
        //
        public abstract string Platform {get;}
        public abstract string Model {get;}
        public abstract string DeviceUniqueId { get;}
        public abstract string OSVersion { get;}
        public abstract bool   IsEmulator { get; }
        public abstract string ApplicationID {get;}
        public abstract string AppVersion {get;}

        public abstract ConnectivityLevel ConnectionType {get;}
        // TODO: Connection speed?

        private int _activity = 0;
        public bool ShowActivity {
            get {
                return _activity > 0;
            }
            set {
                SetActivityInternal (value);
            }
        }

        protected PlatformAccess() {

            InvokeOnUiThread (() => {
                _uiThreadId = Thread.CurrentThread.ManagedThreadId;
            });
        }

        protected virtual void OnShowActivity(bool show) {

        }

        private void SetActivityInternal(bool isActive) {
            bool wasActive = ShowActivity;

            if (isActive) {
                _activity++;
            } else if (_activity > 0) {
                _activity--;
            }

            if (ShowActivity != wasActive) {
                OnShowActivity (ShowActivity);
            }
        }

		private const string UserSettingExpireEncodeDelimiter = "\t";

		protected string EncodeUserSetting(string value, DateTime? expires = default(DateTime?))
		{
			var dt = expires.GetValueOrDefault (new DateTime (0)); // TODO: why both default(DateTime?) & new DateTime (0)?

			return String.Format ("{0}{1}{2}", dt.Ticks, UserSettingExpireEncodeDelimiter, value);
		}

		protected string DecodeUserSetting(string value)
		{
			if (string.IsNullOrEmpty (value)) {
				return null;
			}

			var tabIndex = value.IndexOf (UserSettingExpireEncodeDelimiter);

            if (tabIndex == -1)
            {
                return null;
            }

			var ticks = Int64.Parse (value.Substring (0, tabIndex));

			if (ticks > 0 && new DateTime(ticks) < DateTime.UtcNow) {
				return null;
			}

			return value.Substring (tabIndex + 1);
		}

        // settings
        public abstract string GetConfigSetting(string key);

        public abstract void SetUserSetting(string key, string value, DateTime? expires = null);
        public abstract string GetUserSetting(string key);

        public abstract void ClearUserSetting (string str);

        // platform
        //

        public bool IsUiThread {
            get {
                return 
#if !WINDOWS_PHONE
                    !Thread.CurrentThread.IsThreadPoolThread &&
#endif
                    !Thread.CurrentThread.IsBackground &&
                    Thread.CurrentThread.ManagedThreadId == _uiThreadId.GetValueOrDefault ();
            }
        }

        protected abstract void InvokeOnUiThreadCore (Action a);

        public void InvokeOnUiThread(Action a) {

            if (IsUiThread) {
                a ();
            } else {
                InvokeOnUiThreadCore (a);
            }
        }


        // 
        // Location
        //
        public event EventHandler<LocationUpdatedEventArgs> LocationUpdated;

        public class LocationUpdatedEventArgs : EventArgs {
            public BuddyGeoLocation Location {
                get;
                private set;
            }

            public BuddyGeoLocation LastLocation {
                get;
                private set;
            }

            public LocationUpdatedEventArgs(BuddyGeoLocation now, BuddyGeoLocation old) {
                Location = now;
                LastLocation = old;
            }
        }

        Tuple<BuddyGeoLocation, DateTime> _lastLoc;
        protected void SetLastLocation (BuddyGeoLocation location) {

            InvokeOnUiThread (() => {
                if (LocationUpdated != null) {
                    BuddyGeoLocation last = null;
                    if (_lastLoc != null) {
                        last = _lastLoc.Item1;
                    }
                    LocationUpdated (this, new LocationUpdatedEventArgs (location, last));
                }
            });

            _lastLoc = new Tuple<BuddyGeoLocation, DateTime> (location, DateTime.Now);
        }

       
        public BuddyGeoLocation LastLocation {
            get {
                if (_lastLoc != null) {

                    return _lastLoc.Item1;
                }
                return null;
            }
        }

        protected abstract void TrackLocationCore (bool track);

        public void TrackLocation(bool track) {

            TrackLocationCore (track);
            if (!track) {
                _lastLoc = null;
            }
        }
        
        // Push identifiers

        public abstract void RegisterForPushToast(Action<string,string> pushTokenCallback);
        public abstract void RegisterForPushAlert(Action<string,string> pushTokenCallback);
        public abstract void RegisterForPushBadge(Action<string,string> pushTokenCallback);
        public abstract void RegisterForRawPush(Action<string,string> pushTokenCallback,Action<string>pushRecievedCallback);

        static PlatformAccess   _current;
        public static PlatformAccess Current {
            get {
                if (_current == null) {
					#if __ANDROID__
					_current = new AndroidPlatformAccess();
					#elif __IOS__
                    _current = new IosPlatformAccess();
                    #else
                    _current = new DotNetPlatformAccess();
                    #endif

                    if (_current == null) {
                        throw new NotSupportedException ("Unknown platform");
                    }
                }
                return _current;
            }
        }

    }

    #if __ANDROID__
	internal class AndroidPlatformAccess : PlatformAccess
    {
        public override string Platform {
			get { return "Android"; }
		}

        public override string Model {
			// TODO: verify this delimiter is a good one for analytics, and that it doesn't stomp on known Manufacturers and\or Models
			get { return Build.Manufacturer + " : " + Build.Model; }
		}

        public override string DeviceUniqueId {
			get {
				// TODO: verify this is sufficient.  See http://developer.samsung.com/android/technical-docs/How-to-retrieve-the-Device-Unique-ID-from-android-device
				// and http://android-developers.blogspot.com/2011/03/identifying-app-installations.html
				return Settings.Secure.GetString (Application.Context.ContentResolver,
					Settings.Secure.AndroidId);
			}
		}

        public override string OSVersion {
			get { return ((int) Build.VERSION.SdkInt).ToString(); }
		}

        public override bool IsEmulator {
			get {
				// The other recommended method is "goldfish".Equals (Android.OS.Build.Hardware.ToLowerInvariant());
				return Android.OS.Build.Fingerprint.StartsWith("generic");
			}
		}

        public override string ApplicationID {
			get { 
				return Application.Context.PackageName;
			}
		}

        public override string AppVersion {
			get {
				var context = Application.Context;
				 
				var packageInfo = context.PackageManager.GetPackageInfo (context.PackageName, 0);
					
				return packageInfo.VersionName;
			}
		}

        public override ConnectivityLevel ConnectionType {
			get {
				var cs = (ConnectivityManager) Android.App.Application.Context.GetSystemService (Context.ConnectivityService);

				if (!cs.ActiveNetworkInfo.IsConnected)
                    return ConnectivityLevel.None;

                if (cs.ActiveNetworkInfo.Subtype == ConnectivityType.Wifi)
                    return ConnectivityLevel.WiFi;
                else
                    return ConnectivityLevel.Carrier;
			}
		}

        public override string GetConfigSetting(string key)
        {
			var context = Application.Context;

			var appInfo = context.PackageManager.GetApplicationInfo (context.PackageName, 
				Android.Content.PM.PackageInfoFlags.MetaData);

			var metaData = appInfo.MetaData;

			var value = metaData != null && metaData.ContainsKey(key) ? metaData.GetString (key) : null;

			return value;
        }

		private Android.Content.ISharedPreferences GetPreferences()
		{
			var preferences = Application.Context.GetSharedPreferences ("com.buddy-" + ApplicationID, FileCreationMode.Private);

			return preferences;
		}

		public override void SetUserSetting (string key, string value, DateTime? expires = default(DateTime?))
		{
			if (key == null) throw new ArgumentNullException ("key");

			var preferences = GetPreferences ();

			var editor = preferences.Edit ();

			string encodedValue = this.EncodeUserSetting (value, expires);

			editor.PutString (key, encodedValue);

			editor.Commit ();
        }

        public override string GetUserSetting(string key)
        {
			var preferences = GetPreferences ();

			object val = null;
			var keyExists = preferences.All.TryGetValue (key, out val);

			if (!keyExists) {
				return null;
			}

			var value = base.DecodeUserSetting ((string) val);

			if (value == null) {
				ClearUserSetting (key);
			}

			return value;
		}

		public override void ClearUserSetting(string key)
        {
			var preferences = GetPreferences ();

			var editor = preferences.Edit ();

			editor.Remove (key);

			editor.Commit ();
        }

        protected override void InvokeOnUiThreadCore(Action a)
		{
			// SynchronizationContext can't be cached
			if (SynchronizationContext.Current != null)
			{
				SynchronizationContext.Current.Post((s) => { a(); }, null);
			}
			else
			{
				a ();
			}
        }

        protected override void TrackLocationCore (bool track)
        {
            // currently not implemneted because of how Android location works.
            // The location service is attached to the Activity Context, it doesn't look
            // like we can access it -- need to work out a plan here.
        }

        public override void RegisterForPushBadge(Action<string, string> pushTokenCallback){
            //need to integrate with Xamarin component for Google Play services before we can integrate with this 
        }

        public override void RegisterForPushAlert (Action<string, string> pushTokenCallback)
        {
            //need to integrate with Xamarin component for Google Play services before we can integrate with this
        }

        public override void RegisterForPushToast (Action<string, string> pushTokenCallback)
        {
            //need to integrate with Xamarin component for Google Play services before we can integrate with this
        }

        public override void RegisterForRawPush (Action<string, string> pushTokenCallback, Action<string> pushRecievedCallback)
        {
            //need to integrate with Xamarin component for Google Play services before we can integrate with this

        }
    }

    #elif __IOS__

    internal class IosPlatformAccess : PlatformAccess {
        #region implemented abstract members of PlatformAccess

        private NSObject invoker = new NSObject();

        public override string GetConfigSetting (string key)
        {
            var val = NSBundle.MainBundle.ObjectForInfoDictionary(key);

            if (val != null) {
                return val.ToString();
            }

            return null;
        }

        public override void SetUserSetting (string key, string value, DateTime? expires = default(DateTime?))
        {
			string encodedValue = base.EncodeUserSetting (value, expires);

            NSUserDefaults.StandardUserDefaults.SetString(encodedValue, key);
        }

        public override void ClearUserSetting (string key)
        {

            NSUserDefaults.StandardUserDefaults.RemoveObject(key);
           
        }

        public override string GetUserSetting (string key)
        {
            var value = NSUserDefaults.StandardUserDefaults.StringForKey (key);
            if (value == null) {
                return null;
            }

			var decodedValue = base.DecodeUserSetting (value.ToString());

			if (decodedValue == null) {
				ClearUserSetting (key);
			}
		
			return decodedValue;
        }

        public override string Platform {
            get {
                return "iOS";
            }
        }

        public override string Model {
            get {
                // TODO: see code at http://pastebin.com/FJfpGRbQ
                return "iPhone";
            }
        }

        public override string DeviceUniqueId {
            get {
                return UIDevice.CurrentDevice.IdentifierForVendor.AsString ();
            }
        }

        public override string OSVersion {
            get {
                return "1.0";
            }
        }

      
        public override bool IsEmulator {
            get {
                return MonoTouch.ObjCRuntime.Runtime.Arch == MonoTouch.ObjCRuntime.Arch.SIMULATOR;
            }
        }

        public override string ApplicationID {
            get {
                return NSBundle.MainBundle.BundleIdentifier;
            }
           
        }

        public override string AppVersion {
            get {
                NSDictionary infoDictionary =  NSBundle.MainBundle.InfoDictionary;

                var val = infoDictionary [new NSString("CFBundleShortVersionString")];
                if (val != null) {
                    return val.ToString ();
                }
                return null;

            }
        }

        CLLocationManager _locMgr;



        CLLocationManager LocationManager {
            get {
                if (_locMgr == null) {
                    _locMgr = new CLLocationManager();
                    _locMgr.LocationsUpdated += (s, e) => {

                        if (e.Locations == null) {
                            return ;
                        }

                        var lastLoc = e.Locations.FirstOrDefault();

                        if (lastLoc != null) {
                            var loc = new BuddyGeoLocation(lastLoc.Coordinate.Latitude, lastLoc.Coordinate.Longitude);
                            SetLastLocation(loc);
                        }
                    };
                }
                return _locMgr;
            }
        }


        protected override void TrackLocationCore(bool track) {

            if (!CLLocationManager.LocationServicesEnabled) {
                return;
            }

            LocationManager.DesiredAccuracy = 1;
            LocationManager.DistanceFilter = 50;

            if (track) {
                LocationManager.StartUpdatingLocation ();
            } else {
                LocationManager.StopUpdatingLocation ();
            }

        }

        protected override void OnShowActivity (bool show)
        {
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = show;
        }


        protected override void InvokeOnUiThreadCore(Action a)
        {
           
            NSAction nsa = () => {
                a ();
            };

            invoker.BeginInvokeOnMainThread (nsa);
            
        }

       
        // FROM: https://github.com/xamarin/monotouch-samples/tree/master/ReachabilitySample
        // Licence: Apache 2.0
        // Author: Miguel de Icaza
        //
        public static class Reachability {
            public enum NetworkStatus {
                NotReachable,
                ReachableViaCarrierDataNetwork,
                ReachableViaWiFiNetwork
            }

            public static string HostName = "www.buddy.com";

            public static bool IsReachableWithoutRequiringConnection (NetworkReachabilityFlags flags)
            {
                // Is it reachable with the current network configuration?
                bool isReachable = (flags & NetworkReachabilityFlags.Reachable) != 0;

                // Do we need a connection to reach it?
                bool noConnectionRequired = (flags & NetworkReachabilityFlags.ConnectionRequired) == 0;

                // Since the network stack will automatically try to get the WAN up,
                // probe that
                if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
                    noConnectionRequired = true;

                return isReachable && noConnectionRequired;
            }

            // Is the host reachable with the current network configuration
            public static bool IsHostReachable (string host)
            {
                if (host == null || host.Length == 0)
                    return false;

                using (var r = new NetworkReachability (host)){
                    NetworkReachabilityFlags flags;

                    if (r.TryGetFlags (out flags)){
                        return IsReachableWithoutRequiringConnection (flags);
                    }
                }
                return false;
            }

            // 
            // Raised every time there is an interesting reachable event, 
            // we do not even pass the info as to what changed, and 
            // we lump all three status we probe into one
            //
            public static event EventHandler ReachabilityChanged;

            static void OnChange (NetworkReachabilityFlags flags)
            {
                var h = ReachabilityChanged;
                if (h != null)
                    h (null, EventArgs.Empty);
            }

            //
            // Returns true if it is possible to reach the AdHoc WiFi network
            // and optionally provides extra network reachability flags as the
            // out parameter
            //
            static NetworkReachability adHocWiFiNetworkReachability;
            public static bool IsAdHocWiFiNetworkAvailable (out NetworkReachabilityFlags flags)
            {
                if (adHocWiFiNetworkReachability == null){
                    adHocWiFiNetworkReachability = new NetworkReachability (new IPAddress (new byte [] {169,254,0,0}));
                    adHocWiFiNetworkReachability.SetCallback (OnChange);
                    adHocWiFiNetworkReachability.Schedule (CFRunLoop.Current, CFRunLoop.ModeDefault);
                }

                if (!adHocWiFiNetworkReachability.TryGetFlags (out flags))
                    return false;

                return IsReachableWithoutRequiringConnection (flags);
            }

            static NetworkReachability defaultRouteReachability;
            static bool IsNetworkAvailable (out NetworkReachabilityFlags flags)
            {
                if (defaultRouteReachability == null){
                    defaultRouteReachability = new NetworkReachability (new IPAddress (0));
                    defaultRouteReachability.SetCallback (OnChange);
                    defaultRouteReachability.Schedule (CFRunLoop.Current, CFRunLoop.ModeDefault);
                }
                if (!defaultRouteReachability.TryGetFlags (out flags))
                    return false;
                return IsReachableWithoutRequiringConnection (flags);
            }        

            static NetworkReachability remoteHostReachability;
            public static NetworkStatus RemoteHostStatus ()
            {
                NetworkReachabilityFlags flags;
                bool reachable;

                if (remoteHostReachability == null){
                    remoteHostReachability = new NetworkReachability (HostName);

                    // Need to probe before we queue, or we wont get any meaningful values
                    // this only happens when you create NetworkReachability from a hostname
                    reachable = remoteHostReachability.TryGetFlags (out flags);

                    remoteHostReachability.SetCallback (OnChange);
                    remoteHostReachability.Schedule (CFRunLoop.Current, CFRunLoop.ModeDefault);
                } else
                    reachable = remoteHostReachability.TryGetFlags (out flags);                        

                if (!reachable)
                    return NetworkStatus.NotReachable;

                if (!IsReachableWithoutRequiringConnection (flags))
                    return NetworkStatus.NotReachable;

                if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
                    return NetworkStatus.ReachableViaCarrierDataNetwork;

                return NetworkStatus.ReachableViaWiFiNetwork;
            }

            public static NetworkStatus InternetConnectionStatus ()
            {
                NetworkReachabilityFlags flags;
                bool defaultNetworkAvailable = IsNetworkAvailable (out flags);
                if (defaultNetworkAvailable){
                    if ((flags & NetworkReachabilityFlags.IsDirect) != 0)
                        return NetworkStatus.NotReachable;
                } else if ((flags & NetworkReachabilityFlags.IsWWAN) != 0)
                    return NetworkStatus.ReachableViaCarrierDataNetwork;
                else if (flags == 0)
                    return NetworkStatus.NotReachable;
                return NetworkStatus.ReachableViaWiFiNetwork;
            }

            public static NetworkStatus LocalWifiConnectionStatus ()
            {
                NetworkReachabilityFlags flags;
                if (IsAdHocWiFiNetworkAvailable (out flags)){
                    if ((flags & NetworkReachabilityFlags.IsDirect) != 0)
                        return NetworkStatus.ReachableViaWiFiNetwork;
                }
                return NetworkStatus.NotReachable;
            }
        }

        public override ConnectivityLevel ConnectionType {
            get {
                switch (Reachability.InternetConnectionStatus()) {
                    case Reachability.NetworkStatus.NotReachable:
                        return ConnectivityLevel.None;
                    case Reachability.NetworkStatus.ReachableViaCarrierDataNetwork:
                        return ConnectivityLevel.Carrier;
                    case Reachability.NetworkStatus.ReachableViaWiFiNetwork:
                        return ConnectivityLevel.WiFi;
                    default:
                    throw new NotSupportedException ();
                }
            }
        }

        public override void RegisterForPushAlert (Action<string, string> pushTokenCallback)
        {
            // @TODO Implement this for .NET iOS
        }

        public override void RegisterForPushBadge (Action<string, string> pushTokenCallback)
        {
            // @TODO Implement this for .NET iOS
        }

        public override void RegisterForPushToast (Action<string, string> pushTokenCallback)
        {
            // @TODO Implement this for .NET iOS
        }

        public override void RegisterForRawPush (Action<string, string> pushTokenCallback, Action<string> pushRecievedCallback)
        {
            // @TODO Implement this for .NET iOS
        }

        #endregion



    }

    public static class IosExtensions  {


        public static BuddyGeoLocation ToBuddyGeoLocation(this MonoTouch.CoreLocation.CLLocation loc) {
            return new BuddyGeoLocation (loc.Coordinate.Latitude, loc.Coordinate.Longitude);
        }

        public static CLLocation ToCLLocation(this BuddyGeoLocation loc) {
            var clLoc = new CLLocation (loc.Latitude, loc.Longitude);
            return clLoc;
        }
    }
#else
    // default
    internal class DotNetPlatformAccess : PlatformAccess
    {
        public override string Platform
        {
            get { return ".NET"; }
        }

        public override string Model
        {
            get {

#if WINDOWS_PHONE
            return Microsoft.Phone.Info.DeviceStatus.DeviceName;
#else
            return null;
#endif
            
            }
        }

        public override string DeviceUniqueId
        {
            get {
#if WINDOWS_PHONE
                try
                {
                    byte[] id = (byte[])Microsoft.Phone.Info.DeviceExtendedProperties.GetValue("DeviceUniqueId");

                    if (id != null)
                    {
                        return Convert.ToBase64String(id);
                    }
                }
                catch
                {
                    System.Diagnostics.Debug.WriteLine("You must enable the ID_CAP_IDENTITY_DEVICE capability in WMAppManifest.xml to enable retrieval of DeviceExtendedProperties' DeviceUniqueId.");
                }

#endif
                var uniqueId = GetUserSetting("UniqueId");
                if (uniqueId == null)
                {
                    uniqueId = Guid.NewGuid().ToString();
                    SetUserSetting("UniqueId", uniqueId);
                }
                return uniqueId;
            }
        }

        public override string OSVersion
        {
            get {
#if WINDOWS_PHONE
            return System.Environment.OSVersion.Version.ToString();
#else
                try
                {
                    // .NET
                    var osVersionProperty = typeof(Environment).GetRuntimeProperty("OSVersion");
                    object osVersion = osVersionProperty.GetValue(null, null);
                    var versionStringProperty = osVersion.GetType().GetRuntimeProperty("VersionString");
                    var versionString = versionStringProperty.GetValue(osVersion, null);
                    return (string)versionString;
                }
                catch
                {
                }

                return "DeviceOSVersion not found";
#endif
            
            
            }
        }

        public override bool IsEmulator
        {
            get {
#if WINDOWS_PHONE
                return Microsoft.Devices.Environment.DeviceType == Microsoft.Devices.DeviceType.Emulator;
#else
                return false;
#endif
                }
        }

        public override string ApplicationID
        {
            get {
#if WINDOWS_PHONE
            var xml = XElement.Load("WMAppManifest.xml");
            var prodId = (from app in xml.Descendants("App")
                            select app.Attribute("ProductID").Value).FirstOrDefault();
            if (string.IsNullOrEmpty(prodId)) return string.Empty;
            return new Guid(prodId).ToString();
#else
                try
                {
                    // .NET
                    var assemblyFullName = typeof(System.Diagnostics.Debug).GetTypeInfo().Assembly.FullName;
                    var process = Type.GetType("System.Diagnostics.Process, " + assemblyFullName);
                    var getCurrentProcessMethod = process.GetRuntimeMethod("GetCurrentProcess", new Type[0]);
                    var currentProcess = getCurrentProcessMethod.Invoke(null, null);
                    var processNameProperty = currentProcess.GetType().GetRuntimeProperty("ProcessName");
                    var processName = processNameProperty.GetValue(currentProcess, null);
                    return (string)processName;
                }
                catch
                {
                }

                try
                {
                    // Windows Store
                    var loadMethod = typeof(XDocument).GetRuntimeMethod("Load", new Type[] {
                    typeof(string),
                    typeof(LoadOptions)
                });
                    var xDocument = loadMethod.Invoke(null, new object[] {
                    "AppxManifest.xml",
                    LoadOptions.None
                });

                    var xNamespace = XNamespace.Get("http://schemas.microsoft.com/appx/2010/manifest");

                    var identityElement = ((XDocument)xDocument).Descendants(xNamespace + "Identity").First();

                    return identityElement.Attribute("Name").Value;
                }
                catch
                {
                }

                return "ApplicationId not found";
#endif
            
            
            }
        }

        public override string AppVersion
        {
            get {

                // TODO: Capture calling assembly from Buddy.Init call.
#if WINDOWS_PHONE
                var entryAssembly = Assembly.GetCallingAssembly();
#else
                var entryAssembly = Assembly.GetEntryAssembly();
#endif
                if (entryAssembly != null) {
                    var attr = entryAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
                    return attr.Version;
                }

                return "1.0";
            
            }
        }


        protected override void TrackLocationCore(bool track)
        {
           // TBD
        }

   
        public override ConnectivityLevel ConnectionType
        {
            get {
                return ConnectivityLevel.Carrier;
            }
        }

        public override string GetConfigSetting(string key)
        {
#if WINDOWS_PHONE
            return null;
#else
            return System.Configuration.ConfigurationManager.AppSettings[key];
#endif
        }

        private IDictionary<string, string> LoadSettings(IsolatedStorageFile isoStore)
        {
            string existing = "";
            if (isoStore.FileExists("_buddy"))
            {
                using (var fs = isoStore.OpenFile("_buddy", FileMode.Open))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        existing = sr.ReadToEnd();
                    }
                }
            }

            var d = new Dictionary<string, string>();
            var parts = Regex.Match(existing, "(?<key>\\w*)=(?<value>.*?);");

            while (parts.Success)
            {
                d[parts.Groups["key"].Value] = parts.Groups["value"].Value;

                parts = parts.NextMatch();
            }

            return d;
        }

        private void SaveSettings(IsolatedStorageFile isoStore, IDictionary<string, string> values)
        {  
            var sb = new StringBuilder();

            foreach (var kvp in values)
            {
                sb.AppendFormat("{0}={1};", kvp.Key, kvp.Value ?? "");
            }

            using (var fs = isoStore.OpenFile("_buddy", FileMode.Create))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.WriteLine(sb.ToString());

                    sw.Flush();
                    fs.Flush();
                }
            }
        }

        public override void SetUserSetting(string key, string value, DateTime? expires = default(DateTime?))
        {
            if (key == null) throw new ArgumentNullException("key");

            var isoStore = GetIsolatedStorageFile();

            // parse it
            var parsed = LoadSettings(isoStore);
            string encodedValue = this.EncodeUserSetting(value, expires);
            parsed[key] = encodedValue;

            SaveSettings(isoStore, parsed);      
        }

        public override string GetUserSetting(string key)
        {
            var isoStore = GetIsolatedStorageFile();

            var parsed = LoadSettings(isoStore);

            if (parsed.ContainsKey(key))
            {
                var value = base.DecodeUserSetting((string)parsed[key]);

                if (value == null)
                {
                    ClearUserSetting(key);
                }

                return value;
            }

            return null;
        }

        public override void ClearUserSetting(string key)
        {
            var isoStore = GetIsolatedStorageFile();

            var parsed = LoadSettings(isoStore);

            if (parsed.ContainsKey(key))
            {
                parsed.Remove(key);
                SaveSettings(isoStore, parsed);
            }        
        }

        private IsolatedStorageFile GetIsolatedStorageFile()
        {
#if WINDOWS_PHONE
            return IsolatedStorageFile.GetUserStoreForApplication();
#else
            try
            {
                return IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            }
            catch (IsolatedStorageException)
            {
                return IsolatedStorageFile.GetUserStoreForDomain();
            }
#endif
        }

        protected override void InvokeOnUiThreadCore(Action a)
        {
            var context = SynchronizationContext.Current;

            if (context != null)
            {
                context.Post((s) => { a(); }, null);
            }
            else
            {
                a();
            }
        }

        public override void RegisterForPushAlert(Action<string,string> pushTokenCallback)
        {
            //MPNS supports Toast, Tile, and Raw
            throw new NotImplementedException();
        }

        public override void RegisterForPushBadge(Action<string,string> pushTokenCallback)
        {
            //For now, we'll use tile notifications as badges
            throw new NotImplementedException();
        }

        public override void RegisterForPushToast(Action<string,string> pushTokenCallback)
        {
            Action<object, Microsoft.Phone.Notification.NotificationChannelUriEventArgs> channelHandler
                = new Action<object, Microsoft.Phone.Notification.NotificationChannelUriEventArgs>((sender, args) =>
                {
                    Task.Run(async () =>
                    {
                        SetUserSetting(PushChannelSettingName, args.ChannelUri.AbsoluteUri);
                        //send channel to buddy
                        await Buddy.Instance.UpdateDevice(args.ChannelUri.AbsoluteUri);
                        //send channel to app
                        pushTokenCallback(null, args.ChannelUri.AbsoluteUri);
                    });
                });
            Action<object, Microsoft.Phone.Notification.NotificationChannelErrorEventArgs> errorHandler
                = new Action<object, Microsoft.Phone.Notification.NotificationChannelErrorEventArgs>((sender, args) =>
                {
                    Task.Run(() =>
                    {
                        pushTokenCallback(args.Message, null);
                    });
                });

            Microsoft.Phone.Notification.HttpNotificationChannel channel;
            channel = Microsoft.Phone.Notification.HttpNotificationChannel.Find(PushChannelName);
            if (channel == null)
            {
                channel = new Microsoft.Phone.Notification.HttpNotificationChannel(PushChannelName);
                channel.Open();

                channel.ChannelUriUpdated += new EventHandler<Microsoft.Phone.Notification.NotificationChannelUriEventArgs>(channelHandler);
                channel.ErrorOccurred += new EventHandler<Microsoft.Phone.Notification.NotificationChannelErrorEventArgs>(errorHandler);


            }
            else
            {
                channel.ChannelUriUpdated += new EventHandler<Microsoft.Phone.Notification.NotificationChannelUriEventArgs>(channelHandler);
                channel.ErrorOccurred += new EventHandler<Microsoft.Phone.Notification.NotificationChannelErrorEventArgs>(errorHandler);
            }

            if (!channel.IsShellToastBound)
            {
                channel.BindToShellToast();
            }
            string storedPushChannelName = GetUserSetting(PushChannelSettingName);
            if(null != storedPushChannelName){
                pushTokenCallback(null, storedPushChannelName);
                Buddy.Instance.UpdateDevice(storedPushChannelName);
            }
        }


        public override void RegisterForRawPush(Action<string, string> pushTokenCallback, Action<string> pushRecievedCallback)
        {
            Action<object, Microsoft.Phone.Notification.NotificationChannelUriEventArgs> channelHandler
                = new Action<object, Microsoft.Phone.Notification.NotificationChannelUriEventArgs>((sender, args) =>
                    {
                        Task.Run(async () =>
                        {
                            await Buddy.Instance.UpdateDevice(args.ChannelUri.AbsoluteUri);
                            pushTokenCallback(null, args.ChannelUri.AbsoluteUri);
                        });
                    });
            Action<object, Microsoft.Phone.Notification.NotificationChannelErrorEventArgs> errorHandler
                = new Action<object, Microsoft.Phone.Notification.NotificationChannelErrorEventArgs>((sender, args) =>
                {
                    Task.Run(() =>
                    {
                        pushTokenCallback(args.Message, null);
                    });
                });
            Action<object, Microsoft.Phone.Notification.HttpNotificationEventArgs> notificationHandler 
                = new Action<object,Microsoft.Phone.Notification.HttpNotificationEventArgs>((sender,args) =>
                {
                    Task.Run(async () =>
                    {
                        if (args.Notification.Body.CanSeek)
                        {
                            args.Notification.Body.Seek(0, SeekOrigin.Begin);
                        }
                        StreamReader notificationReader = new System.IO.StreamReader(args.Notification.Body);
                        pushRecievedCallback(await notificationReader.ReadToEndAsync());
                        //pull off batch id and send to buddy

                    });
                });
        }

    }
    #endif
}

