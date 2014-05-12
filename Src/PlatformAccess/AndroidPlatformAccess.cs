
#if __ANDROID__

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Provider;


namespace BuddySDK
{

    public partial class BuddyClient {

        public void RecordNotificationReceived(Intent message) {
            var id = message.GetStringExtra(PlatformAccess.BuddyPushKey);
            if (!String.IsNullOrEmpty(id)) {
                PlatformAccess.Current.OnNotificationReceived(id);
            }
        }

      
    }


    public partial class Buddy {
        public static void RecordNotificationReceived(Intent message) {

            Instance.RecordNotificationReceived (message);
        }


    }


    public abstract partial class PlatformAccess {


        public const BuddyClientFlags DefaultFlags = BuddyClientFlags.AutoCrashReport;

        static PlatformAccess CreatePlatformAccess()
        {
            return new AndroidPlatformAccess();
        }
    }

   
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

            string encodedValue = PlatformAccess.EncodeUserSetting (value, expires);

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

            var value = PlatformAccess.DecodeUserSetting ((string) val);

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
			if (System.Threading.SynchronizationContext.Current != null)
			{
				System.Threading.SynchronizationContext.Current.Post((s) => { a(); }, null);
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

       
    }

}


  

#endif