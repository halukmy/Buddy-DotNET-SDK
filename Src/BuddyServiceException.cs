using System;
using System.Net;

namespace BuddySDK
{
    /// <summary>
    /// Occurs when there is an error processing the service request.
    /// </summary>
    public class BuddyServiceException : Exception
    {
        /// <summary>
        /// The error that occured.
        /// </summary>
        public string Error { get; protected set; }
        public int ErrorNumber { get; protected set;}

        public int StatusCode {
            get;
            internal set;
        }
      

        internal BuddyServiceException(string error, string message, int? number = null): base(message)
        {
            this.Error = error;
            this.ErrorNumber = number.GetValueOrDefault ();
        }
    }

    public class BuddyUnauthorizedException : BuddyServiceException {

        internal BuddyUnauthorizedException(string e, string m, int? n): base(e,m, n) {


        }
    }

    public class BuddyNoInternetException : BuddyServiceException {

        internal BuddyNoInternetException(string e): base(e, null) {

        }
    }
}
