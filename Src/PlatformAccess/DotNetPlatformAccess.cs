#if DOTNET
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BuddySDK
{


    public abstract partial class PlatformAccess {


        public const BuddyClientFlags DefaultFlags = BuddyClientFlags.AllowReinitialize;

        static PlatformAccess CreatePlatformAccess()
        {
            return new DotNetPlatformAccess();
        }
    }
    

   internal class DotNetPlatformAccess: DotNetPlatformAccessBase
    {
        public override string Platform
        {
            get { return ".NET"; }
        }

        public override string Model
        {
            get { return null; }
        }

        public override string DeviceUniqueId
        {
            get {

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

                 
                    var osVersionProperty = typeof(Environment).GetRuntimeProperty("OSVersion");
                    object osVersion = osVersionProperty.GetValue(null, null);
                    var versionStringProperty = osVersion.GetType().GetRuntimeProperty("VersionString");
                    var versionString = versionStringProperty.GetValue(osVersion, null);
                    return (string)versionString;
                
            }
        }

        

        public override bool IsEmulator
        {
            get {
                return false;
                }
        }

        public override bool IsUiThread
        {
            get
            {
                return !Thread.CurrentThread.IsThreadPoolThread && base.IsUiThread;
            }
        }

        protected override Assembly EntryAssembly
        {
	        get { return Assembly.GetEntryAssembly(); }
        }

        public override string AppVersion
        {
            get {

                if (EntryAssembly != null) {
                    var attr = EntryAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
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
                return ConnectivityLevel.WiFi;
            }
        }

             

        private class DotNetIsoStore : IsolatedStorageSettings
        {

            protected override IsolatedStorageFile GetIsolatedStorageFile()
            {
                return IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            }
        }

        private IsolatedStorageSettings _settings = new DotNetIsoStore();

        public override void ClearUserSetting(string str)
        {
            _settings.ClearUserSetting(str);
        }

        public override void SetUserSetting(string key, string value, DateTime? expires = null)
        {
            _settings.SetUserSetting(key, value, expires);
        }

        public override string GetUserSetting(string key)
        {
            return _settings.GetUserSetting(key);
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



      

    }


     // default
 
}

#endif
