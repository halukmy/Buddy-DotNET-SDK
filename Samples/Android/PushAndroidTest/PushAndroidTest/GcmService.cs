using System;
using Android;
using Android.Media;
using Android.App;
using Android.Content;
using Android.Service;
using Android.Support.V4.App;
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
            Buddy.Instance.UpdateDeviceAsync(registrationId);
        }

        protected override void OnUnRegistered (Context context, string registrationId)
        {
            //Receive notice that the app no longer wants notifications
        }

        protected override void OnMessage (Context context, Intent intent)
        {
            //Push Notification arrived - print out the keys/values
            Intent received = new Intent (context, typeof(RecievedPush));
            string pushMessage = intent.GetStringExtra ("message");
            received.AddFlags (ActivityFlags.ReorderToFront);
            received.AddFlags (ActivityFlags.NewTask);
            received.PutExtra ("pushedMessage", pushMessage);
            if (intent == null || intent.Extras == null) {
                received.PutExtras (intent.Extras);
                foreach (var key in intent.Extras.KeySet()) {
                    Console.WriteLine ("Key: {0}, Value: {1}");
                }
            }
            PendingIntent notificationLaunch = PendingIntent.GetActivity (context, 1000, received, PendingIntentFlags.CancelCurrent );
            NotificationManager manager = (NotificationManager)GetSystemService (Context.NotificationService);
            NotificationCompat.Builder builder = new NotificationCompat.Builder (context);
            builder.SetContentText (pushMessage);
            builder.SetContentIntent ( notificationLaunch);
            builder.SetContentTitle ("New Message");
            builder.SetSmallIcon (Resource.Drawable.ic_action_chat);
            builder.SetStyle ( new NotificationCompat.InboxStyle());
            builder.SetSound (RingtoneManager.GetDefaultUri (RingtoneType.Notification));
            builder.SetVibrate (new long[] { 500, 300, 100, 1000, 300, 300, 300 ,300 });
            manager.Notify (1000, builder.Build ());

            Buddy.Instance.RecordNotificationReceived (intent);
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

