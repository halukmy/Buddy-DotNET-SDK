using System;

namespace BuddySDK
{
    public class BuddyResultBase
    {
        public BuddyServiceException Error { 
            get; 
            internal set;
        }

      

        public bool IsSuccess {
            get {
                return null == Error;
            }
        }


        public string RequestID { get; 
            internal set;
        }

        internal BuddyResultBase() {

        }

        internal BuddyResultBase(string requestId, BuddyServiceException error) {
            RequestID = requestId;
            Error = error;
        }

       
    }

    public class BuddyResult<T> : BuddyResultBase {

        public T Value {
            get;
            internal set;
        }

        internal BuddyResult() {

        }

        internal BuddyResult(string requestId, BuddyServiceException error):
             base(requestId, error) {
           
        }

        internal  BuddyResult<T2> Convert<T2>(Func<T, T2> map) {

            var br = new BuddyResult<T2>(RequestID, Error);

            if (IsSuccess && map != null) {
                br.Value = map (Value);
            }
            return br;
        }
    }
}

