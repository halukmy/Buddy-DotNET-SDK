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
	[Activity (Label = "View User")]			
	public class ViewUserActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.View);

			InitializeEditTexts ();
		}

		private void InitializeEditTexts()
		{
			var userNameTextView = FindViewById<TextView> (Resource.Id.userNameTextView);
			userNameTextView.Text = Buddy.CurrentUser.Username;

			var firstLastNameTextView = FindViewById<TextView> (Resource.Id.firstLastNameTextView);
			firstLastNameTextView.Text = Buddy.CurrentUser.FirstName + " " + Buddy.CurrentUser.LastName;

			var  emailAddressTextView = FindViewById<TextView> (Resource.Id.emailAddressTextView);
			emailAddressTextView.Text = Buddy.CurrentUser.Email;

			var genderTextView = FindViewById<TextView> (Resource.Id.genderTextView);
			if (Buddy.CurrentUser.Gender.HasValue) {
				genderTextView.Text = Buddy.CurrentUser.Gender.Value.ToString ();
			}

			var birthdateTextView = FindViewById<TextView> (Resource.Id.birthdateTextView);
			if (Buddy.CurrentUser.DateOfBirth.HasValue) {
				birthdateTextView.Text = Buddy.CurrentUser.DateOfBirth.Value.ToLocalTime ().ToString ();
			}
		}
	}
}

