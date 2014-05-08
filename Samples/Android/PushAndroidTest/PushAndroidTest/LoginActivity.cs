using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BuddySDK;
using Gcm.Client;

namespace PushAndroidTest
{
    [Activity (Label = "LoginActivity")]			
    public class LoginActivity : Activity
    {

        private void NavigateToPush(AuthenticatedUser user){
            Intent push = new Intent (this, typeof(PushActivity));
            push.PutExtra ("displayName", user.FirstName);
            push.PutExtra ("userId", user.ID);
            StartActivity (push);
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

        private void RegisterForPushNotifications(){
            GcmClient.CheckDevice(this);
            GcmClient.CheckManifest(this);

            GcmClient.Register (this, GcmBroadcastReceiver.SENDER_IDS);

        }

        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);

            SetContentView (Resource.Layout.Login);

            Button button = FindViewById<Button> (Resource.Id.myButton);


            button.Click += delegate {
                BuddyUserLogin();
            };
        }
    }
}

