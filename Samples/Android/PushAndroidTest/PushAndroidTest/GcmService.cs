using System;
using Android;
using Android.App;
using Android.Content;
using Android.Service;
using Gcm.Client;
using BuddySDK;

namespace PushAndroidTest
{
    [Service]
    public class GcmService : GcmServiceBase
    {
        public GcmService() : base(GcmBroadcastReceiver.SENDER_IDS) { }

        protected override void OnRegistered (Context context, string registrationId)
        {
            //Receive registration Id for sending GCM Push Notifications to
            Buddy.Instance.UpdateDevice (registrationId);
        }

        protected override void OnUnRegistered (Context context, string registrationId)
        {
            //Receive notice that the app no longer wants notifications
        }

        protected override void OnMessage (Context context, Intent intent)
        {
            //Push Notification arrived - print out the keys/values
            Intent received = new Intent (context, typeof(RecievedPush));

            received.AddFlags (ActivityFlags.ReorderToFront);
            received.AddFlags (ActivityFlags.NewTask);
            if (intent == null || intent.Extras == null) {
                received.PutExtras (intent.Extras);
                foreach (var key in intent.Extras.KeySet()) {
                    Console.WriteLine ("Key: {0}, Value: {1}");
                }
            }
            StartActivity (received);
        }

        protected override bool OnRecoverableError (Context context, string errorId)
        {
            //Some recoverable error happened
            return true;
        }

        protected override void OnError (Context context, string errorId)
        {
            //Some more serious error happened
        }
    }
}

