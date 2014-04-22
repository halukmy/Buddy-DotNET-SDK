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

namespace PushAndroidTest
{
    [Activity (Label = "PushActivity")]			
    public class PushActivity : Activity
    {
        private static Random r = new Random ();
        protected override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);
            SetContentView (Resource.Layout.Push);

            string[] some_messages = new string[] {
                "You da boss.",
                "None will survive!",
                "Why you poking me again?",
                "Do not push me or I will impale you on my horns.",
                "Why don't you lead an army instead of touching me!?"
            };

            Button pusher = FindViewById<Button> (Resource.Id.push);
            pusher.Click += async (sender, e) => {
                Console.Write (String.Format ("Starting push to {0}", Intent.GetStringExtra ("userId")));
                BuddyResult<IDictionary<string,object>> result = await Buddy.CallServiceMethod ("POST", "/notifications/alert",
                                                                     new {
                        message = some_messages [r.Next (some_messages.Count() - 1)],
                        counterValue = 0,
                        payload = "Payload",
                        osCustomData = "{}",
                        recipients = new string[] { Intent.GetStringExtra ("userId") }
                        }
                );
                if(result.IsSuccess){
                    Console.Write("push sent");
                }
            };
        }
    }
}

