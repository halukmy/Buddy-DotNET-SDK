using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using BuddySDK;

namespace CreateLoginUserLibrary
{
	[Activity (Label = "Login User", MainLauncher = true)]
	public class LoginActivity : ActivtyBase
	{
		private Button _loginButton, _createUserButton;
		private EditText _userNameEditText, _passwordEditText;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Login);
		
			InitializeLoginButton ();

			InitializeCreateUserButton ();

			InitializeEditTexts ();
		}

		private void InitializeLoginButton()
		{
			_loginButton = FindViewById<Button> (Resource.Id.clulLoginButton);

			_loginButton.Click += async (sender, eventArgs) => {

				try {
					await Buddy.LoginUserAsync(_userNameEditText.Text, _passwordEditText.Text);

					StartHomeActivity();
				}
				catch {
					Android.Widget.Toast.MakeText(this, "Incorrect user name or password.", ToastLength.Short).Show();
				}
			};
		}

		private void InitializeCreateUserButton ()
		{
			_createUserButton = FindViewById<Button> (Resource.Id.clulCreateUserButton1);

			_createUserButton.Click += (sender, eventArgs) => {

				StartActivity(typeof(CreateUserActivity));
			};
		}

		private void InitializeEditTexts()
		{
			_userNameEditText = FindViewById<EditText> (Resource.Id.clulLoginUserNameEditText);

			_passwordEditText = FindViewById<EditText> (Resource.Id.clulLoginPasswordEditText);

			EventHandler<View.KeyEventArgs> enableLoginButton = (object sender, View.KeyEventArgs keyEventArgs) => {
				_loginButton.Enabled = _userNameEditText.Text.Length > 0 && _passwordEditText.Text.Length > 0;

				keyEventArgs.Handled = false;
			};

			_userNameEditText.KeyPress += enableLoginButton;

			_passwordEditText.KeyPress += enableLoginButton;
		}
	}
}