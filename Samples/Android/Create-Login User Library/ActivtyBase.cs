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

namespace CreateLoginUserLibrary
{		
	public class ActivtyBase : Activity
	{
		private static Type homeActivityType;
		public static Type HomeActivityType
		{
			private get {
				return homeActivityType;
			}

			set {
				homeActivityType = value;
			}
		}
			
		protected void StartHomeActivity()
		{
			var mainIntent = new Intent (ApplicationContext, HomeActivityType);

			StartActivity (mainIntent);
		}
	}
}