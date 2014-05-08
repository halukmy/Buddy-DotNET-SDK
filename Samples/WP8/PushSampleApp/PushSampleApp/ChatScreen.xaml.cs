using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Threading.Tasks;

using BuddySDK;
using System.Windows.Controls.Primitives;

namespace PhoneApp5
{
    public partial class ChatScreen : PhoneApplicationPage
    {
        private string Recipient { get; set; }
        public ChatScreen()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Buddy.Instance.RecordNotificationReceived(this.NavigationContext);
            LoadUsers();
        }

        public async Task LoadUsers()
        {
            Buddy.AuthorizationNeedsUserLogin += Buddy_AuthorizationNeedsUserLogin;
            if (null == Buddy.CurrentUser)
            {
                NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
            }
            SearchResult<User> users = await Buddy.Users.FindAsync();
            userList.ItemsSource = users.PageResults.ToList();
        }

        public void DisplaySend(object sender, RoutedEventArgs args)
        {
            string sendId = (sender as Button).Tag as string;
            Recipient = sendId;
            
            try
            {
                MessagePopup.IsOpen = true;
            }
            catch (Exception e)
            {

            }

        }

        public async void SendMessage(object sender, RoutedEventArgs args)
        {
            MessagePopup.IsOpen = false;
            MessageBox.Show(String.Format("Sending to {0}", Recipient), "Sending...", MessageBoxButton.OK);
            BuddyResult<Notification> result = await Buddy.SendPushNotificationAsync(new string[] { Recipient }, String.Format("Message from {0}", Buddy.CurrentUser.FirstName)  , MessageBody.Text);
            
            if (result.IsSuccess)
            {
                var pushedAggregate = result.Value.SentByPlatform.Aggregate<KeyValuePair<string, int>, int>(0, (agg, pt) => agg + pt.Value);
                if (pushedAggregate < 1)
                {
                    MessageBox.Show("This person isn't logged into any devices that support chat");
                }
                else
                {
                    MessageBox.Show("Sent!");
                }
            }
        }

        private void Buddy_AuthorizationNeedsUserLogin(object sender, EventArgs e)
        {
            NavigationService.Navigate( new Uri( "/LoginPage.xaml", UriKind.Relative));
        }
    }
}