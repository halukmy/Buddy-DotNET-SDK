using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Gcm.Client;
using BuddySDK;

namespace PushAndroidTest
{
    [Activity (Label = "PushAndroidTest", MainLauncher = true)]
    public class MainActivity : Activity
    {
        int count = 1;
        private const String APP_ID = "bbbbbc.sDhbvlKNrBHl";
        private const String APP_KEY = "4B82209C-108D-483F-9B41-CA006792FEAB"; 

        private void NavigateToPush(AuthenticatedUser user){
            Intent push = new Intent (this, typeof(PushActivity));
            push.PutExtra ("displayName", user.FirstName);
            push.PutExtra ("userId", user.ID);
            StartActivity (push);
        }

        private void RegisterForPushNotifications(){
            GcmClient.CheckDevice(this);
            GcmClient.CheckManifest(this);

            GcmClient.Register (this, GcmBroadcastReceiver.SENDER_IDS);

        }


        protected override void OnCreate (Bundle bundle)
        {

            base.OnCreate (bundle);
            try{
                Buddy.Init (APP_ID, APP_KEY, BuddyClientFlags.AutoCrashReport);
            } catch(InvalidOperationException){
                //already intitialized
            }
            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            Buddy.AuthorizationNeedsUserLogin += (object sender, EventArgs e) => {
                Intent loginIntent = new Intent(this, typeof(LoginActivity));
                StartActivity(loginIntent);
            };


            if (null == Buddy.CurrentUser) {
                Intent loginIntent = new Intent(this, typeof(LoginActivity));
                StartActivity(loginIntent);
                return;
            }
            NavigateToPush (Buddy.CurrentUser);
        }
    }
}


