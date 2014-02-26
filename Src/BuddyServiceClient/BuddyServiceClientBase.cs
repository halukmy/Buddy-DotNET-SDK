﻿using BuddySDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BuddyServiceClient
{
    public class BuddyCallResult<T>
    {
        public string Error { get; set; }
        public int? ErrorNumber{ get;set;}
        public string Message { get; set; }
        public int    StatusCode { get; set; }
        public T Result { get; set; }
        public string RequestID { get; set; }

        public BuddyCallResult()
        {
        }
    }

    internal class BuddyFile
    {
        public Stream Data;

        private byte[] _bytes;
        public byte[] Bytes
        {
            get
            {
                if (Data != null && _bytes == null)
                {
                    _bytes = new byte[Data.Length];
                    Data.Read(_bytes, 0, _bytes.Length);

                }
                return _bytes;
            }
        }
        public string Name;
        public string ContentType = "application/octet-stream";

        public BuddyFile()
        {

        }
    }


    public class ExceptionEventArgs : EventArgs {
        public Exception Exception { get; set; }

        public bool ThrowException { get; set; }
        public ExceptionEventArgs(Exception bex) {
            Exception = bex;
            ThrowException = true;
        }
    }
  

    public abstract partial class BuddyServiceClientBase
    {
        public BuddySDK.BuddyClient Client
        {
            get;
            protected set;
        }

        public static BuddyServiceClientBase CreateServiceClient(BuddySDK.BuddyClient client, string serviceRoot)
        {
            var type = typeof(BuddyServiceClientHttp);
            var typeName = PlatformAccess.Current.GetConfigSetting("BuddyServiceClientType");

            if (typeName != null)
            {
                type = Type.GetType(typeName, true);
            }

            if (!typeof(BuddyServiceClientBase).IsAssignableFrom(type))
            {
                throw new ArgumentException(type.FullName + " is not a BuddyServiceClientBase implementor.");
            }

            var bsc = (BuddyServiceClientBase)Activator.CreateInstance(type, serviceRoot);
            bsc.Client = client;
            return bsc;
        }

        protected abstract string ClientName { get; }
        protected abstract string ClientVersion { get; }

        public event EventHandler<ExceptionEventArgs> ServiceException;

        public virtual bool IsLocal
        {
            get
            {
                return false;
            }
        }

        protected BuddyServiceClientBase()
        {
        }

        internal static IDictionary<string, object> ParametersToDictionary(object parameters)
        {
            IDictionary<string, object> d = parameters as IDictionary<string,object>;
            if (d != null) {
                return d;
            }
            else
            {
                d = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
                if (parameters != null)
                {
                    var props = parameters.GetType().GetProperties();
                    foreach (var prop in props)
                    {
                        d[prop.Name] = prop.GetValue(parameters, null);
                    }
                }
            }

            return d;
        }

        internal void CallOnUiThread(Action callback)
        {
            PlatformAccess.Current.InvokeOnUiThread (callback);
        }     

        public System.Threading.Tasks.Task<BuddyCallResult<T1>> CallMethodAsync<T1>(string verb, string path, object parameters = null)
        {   
            var tcs = new TaskCompletionSource<BuddyCallResult<T1>>();


            CallMethodAsync<T1>(verb, path, parameters, (bcr) =>
            {

                    tcs.TrySetResult(bcr);

               
            });
            return tcs.Task;
        }   

        public abstract void CallMethodAsync<T>(string verb, string path, object parameters, Action<BuddyCallResult<T>> callback);

        private string serviceRoot;
        public string ServiceRoot
        {
            get
            {
                return serviceRoot;
            }
            set
            {
                // TODO: should we be doing this here, or should setters be changed to not pass in trailing slashes?

                serviceRoot = value == null ? null : value.TrimEnd('/');
            }
        }
    }

    internal static class BuddyResultCreator
    {
        public static BuddyCallResult<T> Create<T>(T result, object err)
        {
           // return new BuddyCallResult<T>(){Result = result, Error = err};
            return null;
        }
    }

    public static class BuddyError
    {
        public const string None = "";
        public const string UnknownServiceError = "UnknownServiceError";
        public const string InternetConnectionError = "InternetConnectionError";

        public static string UserEmailTaken { get; set; }

        public static string UserNameAvailble { get; set; }

        public static string UserNameAlreadyInUse { get; set; }
    }

}
