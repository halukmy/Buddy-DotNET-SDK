#if NETFX_CORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Reflection;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.Networking.PushNotifications;
using Windows.Foundation;
using Windows.ApplicationModel.Activation;
using System.Text.RegularExpressions;

namespace BuddySDK
{

    public partial class BuddyClient
    {

        public void RecordNotificationReceived(LaunchActivatedEventArgs args)
        {
            var id = args.Arguments;
            if (!String.IsNullOrEmpty(id))
            {
                var match = Regex.Match(id, PlatformAccess.BuddyPushKey + "=(?<id>[^;]+)");
                if (match.Success)
                {
                    PlatformAccess.Current.OnNotificationReceived(match.Groups["id"].Value);
                }
            }
        }

        private PushNotificationChannel _channel;

        public PushNotificationChannel PushNotificationChannel {
            get {
                return _channel;
            }
            set {
                if (_channel != value)
                {
                    _channel = null;
                }
                _channel = value;

                if (_channel != null)
                {
                    PlatformAccess.Current.SetPushToken(_channel.Uri);

                    // var timeUntilExpires = _channel.ExpirationTime.UtcDateTime.Subtract(DateTime.UtcNow);
                }
            }
        }

       
    }

    public abstract partial class PlatformAccess {
        public const BuddyClientFlags DefaultFlags = BuddyClientFlags.Default;

        private static PlatformAccess CreatePlatformAccess()
        {
            return new WindowsPlatformAccess();
        }
    }

    internal class WindowsPlatformAccess : DotNetPlatformAccessBase
    {

        public override string Platform
        {
            get { 
                return "WindowsStore"; 
            }
        }

        public override string ApplicationID
        {
            get
            {
                var xDocument = XDocument.Load("AppManifest.xml");

                var xNamespace = XNamespace.Get("http://schemas.microsoft.com/appx/2010/manifest");

                var identityElement = ((XDocument)xDocument).Descendants(xNamespace + "Identity").First();

                return identityElement.Attribute("Name").Value;
            }
        }

        protected override Assembly EntryAssembly
        {
            get { 
                // get the type name
                var xDocument = XDocument.Load("AppManifest.xml");

                var xNamespace = XNamespace.Get("http://schemas.microsoft.com/appx/2010/manifest");

                var appElement = ((XDocument)xDocument).Descendants(xNamespace + "Application").First();

                var typeName = appElement.Attribute("EntryPoint").Value;

                var type = Type.GetType(typeName);

                return type.GetTypeInfo().Assembly;

            }
        }


        private void EnsureSettings(string key)
        {
            Windows.Storage.ApplicationData.Current.LocalSettings.CreateContainer(key, ApplicationDataCreateDisposition.Always);
         
        }

        public override string GetUserSetting(string key)
        {
            EnsureSettings(key);
            var val = Windows.Storage.ApplicationData.Current.LocalSettings.Values[key] as string;
            if (val != null)
            {
                var decoded = PlatformAccess.DecodeUserSetting(val);

                if (decoded != null)
                {
                    val = decoded;
                }
            }

            return val;
        }

        public override void SetUserSetting(string key, string value, DateTime? expires = default(DateTime?))
        {
            EnsureSettings(key);
             Windows.Storage.ApplicationData.Current.LocalSettings.Values[key] = PlatformAccess.EncodeUserSetting(value, expires) ;
        }

        public override void ClearUserSetting(string str)
        {
            EnsureSettings(str);
            Windows.Storage.ApplicationData.Current.LocalSettings.Values.Remove(str);
        }
    }

    internal static class DotNetDeltas
    {
        public static PropertyInfo GetProperty(this System.Type t, string name)
        {
            return t.GetRuntimeProperty(name);
        }
        public static ConstructorInfo GetConstructor(this System.Type t, params Type[] paramTypes)
        {
            return t.GetConstructor(paramTypes);
        }
        public static T GetCustomAttribute<T>(this System.Reflection.PropertyInfo pi) where T : System.Attribute
        {
            return System.Reflection.CustomAttributeExtensions.GetCustomAttribute<T>(pi);
        }
        public static T GetCustomAttribute<T>(this System.Type t) where T : System.Attribute
        {
            return System.Reflection.CustomAttributeExtensions.GetCustomAttribute<T>(t.GetTypeInfo());
        }

        public static int CurrentThreadId
        {
            get
            {
                return Environment.CurrentManagedThreadId;
            }
        }

        public static IEnumerable<PropertyInfo> GetProperties(this Type t)
        {
            return t.GetRuntimeProperties();
        }

        public static bool IsAssignableFrom(this Type t, Type other)
        {
            return t.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());
        }

        public static bool IsInstanceOfType(this Type t, object obj)
        {
            if (obj == null) return false;
            return IsAssignableFrom(t, obj.GetType());
        }

        public static void Sleep(int ms)
        {
            Task.Delay(ms).Wait();
        }

        public static StringComparer InvariantComparer(bool ignoreCase = false)
        {
            if (ignoreCase)
            {
                return StringComparer.OrdinalIgnoreCase;
            }
            else
            {
                return StringComparer.Ordinal;
            }
        }

        public class ExceptionEventArgs {
            public Exception Exception {get;set;}
            public string Message { get; set; }
            public bool IsHandled { get; set; }
        }

        public static event EventHandler<ExceptionEventArgs> UnhandledException;

        static DotNetDeltas()
        {
            Application.Current.UnhandledException += (s, args) =>
            {
                var a = new ExceptionEventArgs { 
                    Exception = args.Exception, 
                    Message = args.Message 
                };
                if (UnhandledException != null)
                {
                    UnhandledException(null, a);
                }
                args.Handled = a.IsHandled;
            };

        }

    }
}
#else 

    using System.Reflection;

internal static class DotNetDeltas
{

    public static T GetCustomAttribute<T>(this System.Reflection.PropertyInfo pi) where T : System.Attribute
    {
        return System.Reflection.CustomAttributeExtensions.GetCustomAttribute<T>(pi);
    }
    public static T GetCustomAttribute<T>(this System.Type t) where T : System.Attribute
    {
        return System.Reflection.CustomAttributeExtensions.GetCustomAttribute<T>(t);
    }

    public static System.Collections.Generic.IEnumerable<PropertyInfo> GetProperties(this System.Type t)
    {
        return t.GetProperties();
    }

    public static bool IsAssignableFrom(this System.Type t, System.Type other)
    {
        return t.IsAssignableFrom(other.GetTypeInfo());
    }

    public static bool IsInstanceOfType(this System.Type t, object obj)
        {
            return t.IsInstanceOfType(obj);
        }

    public static void Sleep(int ms)
    {
        System.Threading.Thread.Sleep(ms);
    }

    public static int CurrentThreadId
    {
        get
        {
            return System.Threading.Thread.CurrentThread.ManagedThreadId;
        }
    }

    public static System.StringComparer InvariantComparer(bool ignoreCase = false)
    {
        if (ignoreCase)
        {
            return System.StringComparer.InvariantCultureIgnoreCase;
        }
        else
        {
            return System.StringComparer.InvariantCulture;
        }
    }

    public class ExceptionEventArgs
    {
        public System.Exception Exception { get; set; }
        public string Message { get; set; }
        public bool IsHandled { get; set; }
    }

    public static event System.EventHandler<ExceptionEventArgs> UnhandledException;

    static DotNetDeltas()
    {
        System.AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            var a = new ExceptionEventArgs
            {
                Exception = args.ExceptionObject as System.Exception
            };

            if (UnhandledException != null)
            {
                UnhandledException(null, a);
            }

        };

    }
}

#endif
