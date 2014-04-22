using System;
using System.Net;
using System.Threading;
using System.Xml.Linq;
using System.Reflection;

using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace BuddySDK
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public abstract partial class PlatformAccess
    {
        internal const string BuddyPushKey = "_bId";
        private int? _uiThreadId;
     
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
                _uiThreadId = DotNetDeltas.CurrentThreadId;
            });
        }

        public virtual bool SupportsFlags(BuddyClientFlags flags)
        {
            var unsupported = BuddyClientFlags.AutoTrackLocation;
            return (unsupported & flags) == BuddyClientFlags.None;
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

		internal static string EncodeUserSetting(string value, DateTime? expires = default(DateTime?))
		{
			var dt = expires.GetValueOrDefault (new DateTime (0)); // TODO: why both default(DateTime?) & new DateTime (0)?

			return String.Format ("{0}{1}{2}", dt.Ticks, UserSettingExpireEncodeDelimiter, value);
		}

		internal static string DecodeUserSetting(string value)
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

        public virtual bool IsUiThread {
            get {
                return 

                    
                   DotNetDeltas.CurrentThreadId == _uiThreadId.GetValueOrDefault ();
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


        internal string PushToken { get; private set; }

        public virtual Task<string> GetPushTokenAsync()
        {
            if (PushToken == null)
            {
                PushToken = GetUserSetting("__PushToken");
            }

            return Task.FromResult(PushToken);
        }

        public event EventHandler PushTokenChanged;

        public virtual void SetPushToken(string pushToken)
        {
            if (PushToken != pushToken)
            {
                SetUserSetting("__PushToken", pushToken);
                if (PushTokenChanged != null)
                {
                    PushTokenChanged(this, EventArgs.Empty);
                }
            }
        }


        public class NotificationReceivedEventArgs
        {
            public string ID { get; set; }
        }

        public event EventHandler<NotificationReceivedEventArgs> NotificationReceived;


        internal void OnNotificationReceived(string id)
        {
            if (NotificationReceived != null && !String.IsNullOrEmpty(id))
            {
                NotificationReceived(this, new NotificationReceivedEventArgs { ID = id });
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

        static PlatformAccess _current;

        public static PlatformAccess Current
        {
            get
            {
                if (_current == null)
                {
                    // this is implemented in the derived per-platform files.
                    _current = CreatePlatformAccess();
                }
                return _current;
            }
        }
        internal static T GetCustomAttribute<T>(Type t) where T : Attribute
        {
            #if !NETFX_CORE
        
                        return t.GetCustomAttribute<T>();
            #else 
                        return t.GetTypeInfo().GetCustomAttribute<T>();
            #endif
        }

    }



}

