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

namespace CreateLoginUserLibrary
{
	[Activity (Label = "Create User")]
	public class CreateUserActivity : ActivtyBase
	{
		private EditText _userNameEditText, _passwordEditText, _firstNameEditText, _lastNameEditText, _emailAddressEditText;
		private Spinner _genderSpinner;
		private TextView _birthdateTextView;
		private Button _createUserButton;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Create);

			InitializeEditTexts ();

			InitializeGenderSpinner ();

			InitializeDatePicker ();

			InitializeCreateUserButton ();
		}

		private void InitializeCreateUserButton()
		{
			_createUserButton = FindViewById<Button> (Resource.Id.clulCreateUserButton2);

			_createUserButton.Click += async (sender, eventArgs) => {

				try {
					await Buddy.CreateUserAsync(_userNameEditText.Text,
							_passwordEditText.Text, _firstNameEditText.Text + " " + _lastNameEditText.Text, 
							_emailAddressEditText.Text,
						GetGenderSpinnerValue(), DateTime.Parse(_birthdateTextView.Text).ToUniversalTime());

					StartHomeActivity();
				}
				catch {
					Android.Widget.Toast.MakeText(this, "Invalid user information. Please try again.", ToastLength.Short).Show();
				}
			};
		}

		private UserGender? GetGenderSpinnerValue()
		{
			if (genderSpinnerSelectedItem == null) {
				return null;
			}

			return (UserGender?) Enum.ToObject (typeof(UserGender), genderSpinnerSelectedItem.Value);
		}

		private int? genderSpinnerSelectedItem;
		private void InitializeGenderSpinner()
		{
			_genderSpinner = FindViewById<Spinner> (Resource.Id.clulCreateGenderSpinner);

			var adapter = ArrayAdapter.CreateFromResource (
				this, Resource.Array.Gender, Android.Resource.Layout.SimpleSpinnerItem);

			adapter.SetDropDownViewResource (Android.Resource.Layout.SimpleSpinnerDropDownItem);

			_genderSpinner.Adapter = adapter;

			_genderSpinner.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) => 
			{
				genderSpinnerSelectedItem = e.Position;
			};
		}

		protected override Dialog OnCreateDialog (int id)
		{
			return new DatePickerDialog (this, delegate (object sender, DatePickerDialog.DateSetEventArgs e) {
					_birthdateTextView.Text = e.Date.ToLocalTime().ToShortDateString();
				}, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		}

		private void InitializeDatePicker()
		{
			_birthdateTextView = FindViewById<TextView> (Resource.Id.clulCreateBirthdateTextView);

			_birthdateTextView.Text = DateTime.Today.ToShortDateString();

			_birthdateTextView.Click += (object sender, EventArgs e) => {
				ShowDialog(0);
			};
		}

		private void InitializeEditTexts()
		{
			_userNameEditText = FindViewById<EditText> (Resource.Id.clulCreateUserNameEditText);

			_passwordEditText = FindViewById<EditText> (Resource.Id.clulCreatePasswordEditText);

			_firstNameEditText = FindViewById<EditText> (Resource.Id.clulCreateFirstNameEditText);

			_lastNameEditText = FindViewById<EditText> (Resource.Id.clulCreateLastNameEditText);

			_emailAddressEditText = FindViewById<EditText> (Resource.Id.clulCreateEmailAddressEditText);

			EventHandler<View.KeyEventArgs> enableCreateUserButton = (object sender, View.KeyEventArgs keyEventArgs) => {
				_createUserButton.Enabled = _userNameEditText.Text.Length > 0 && _passwordEditText.Text.Length > 0;

				keyEventArgs.Handled = false;
			};

			_userNameEditText.KeyPress += enableCreateUserButton;

			_passwordEditText.KeyPress += enableCreateUserButton;
		}
	}
}