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

namespace CreateLoginUserSample
{
	[Application (Label = "Create-Login User Sample")]
	public class CreateLoginUserApplication : Application
	{
		public CreateLoginUserApplication(IntPtr intPtr, JniHandleOwnership jho) : base(intPtr, jho)
		{
			// TODO: Go to http://dev.buddyplatform.com to get an app ID and app password.
			Buddy.Init ("", "");

			CreateLoginUserLibrary.ActivtyBase.HomeActivityType = typeof(CreateLoginUserSample.ViewUserActivity);
		}

		// Needed for overridden constructor to be called.
		public override void OnCreate ()
		{
			base.OnCreate ();
		}
	}
}