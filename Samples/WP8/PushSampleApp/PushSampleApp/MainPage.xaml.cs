using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneApp5.Resources;
using Microsoft.Phone.Notification;

using BuddySDK;

namespace PhoneApp5
{
    public partial class MainPage : PhoneApplicationPage
    {
        private const string AppId = "APP_ID";
        private const string AppKey = "APP_KEY";

        // Constructor
        public  MainPage()
        {
            InitializeComponent();
            BuddyPushSetup();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private async void BuddyPushSetup()
        {
            Buddy.Init(AppId, AppKey);
            BuddyResult<AuthenticatedUser> user = await Buddy.CreateUserAsync("testWpUser" + Environment.TickCount, "password");
            PlatformAccess.Current.RegisterForPushToast((error, channelUri) =>
            {
                if (error == null)
                {
                    MessageBox.Show(String.Format("Channel URI is {0}", channelUri));
                }
                else
                {
                    MessageBox.Show(String.Format("A push notification error occurred: {0}", error));
                }
            });
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
     
    }

    
}