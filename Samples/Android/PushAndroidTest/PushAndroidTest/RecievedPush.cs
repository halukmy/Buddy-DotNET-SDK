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

namespace PushAndroidTest
{
    [Activity (Label = "RecievedPush")]			
    public class RecievedPush : Activity
    {
        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);
            SetContentView (Resource.Layout.ReceivedPush);
            string pushedMessage = this.Intent.GetStringExtra ("pushedMessage");
            TextView receivedMessage = FindViewById<TextView> (Resource.Id.receivedMessage);
            if (null != pushedMessage) {
                receivedMessage.Text = pushedMessage;
            }
        }
    }
}

