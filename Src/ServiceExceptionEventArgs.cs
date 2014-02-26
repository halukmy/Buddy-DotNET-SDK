using System;

namespace BuddySDK
{
    public class ServiceExceptionEventArgs : EventArgs {
        public BuddyServiceException Exception { get; private set;}

        public bool ShouldThrow { get; set; }

        public ServiceExceptionEventArgs(BuddyServiceException ex) {
            Exception = ex;
        }
    }
}

