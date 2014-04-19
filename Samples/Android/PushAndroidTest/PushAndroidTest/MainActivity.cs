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
        private const String APP_ID = "APP_ID";
        private const String APP_KEY = "API_KEY"; 

        private async void CreateBuddyUser(){
            var userResult = await Buddy.Instance.CreateUserAsync (String.Format ("testAndroidUser{0}", System.Environment.TickCount), "apassword");
            Android.App.AlertDialog dialog = new AlertDialog.Builder (this)
                .SetTitle ("User Created")
                .SetMessage (String.Format ("User {0} created", userResult.Value.ID))
                .SetPositiveButton ("Sweet", (sender, args) => { return; })
                .SetCancelable (true)
                .Create ();
            dialog.Show ();
            RegisterForPushNotifications ();
        }

        private async void BuddyUserLogin(){
            EditText username = FindViewById<EditText> (Resource.Id.username);
            EditText password = FindViewById<EditText> (Resource.Id.password);
            BuddyResult<AuthenticatedUser> user = await Buddy.Instance.LoginUserAsync (username.Text.Trim(), password.Text.Trim());
            if (user.IsSuccess) {
                RegisterForPushNotifications ();
                NavigateToPush (user.Value);
            }
        }

        private async void NavigateToPush(AuthenticatedUser user){
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
                Buddy.Init (APP_ID, APP_KEY);
            } catch(InvalidOperationException){
                //already intitialized
            }
            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button> (Resource.Id.myButton);
            			
            button.Click += delegate {
                BuddyUserLogin();
            };
        }
    }
}


