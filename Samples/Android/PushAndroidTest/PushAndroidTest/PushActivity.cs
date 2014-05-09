using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public  class PushActivity : Activity
    {

        private string SelectedUser { get; set; }
        private Dialog SendDialog { get; set; }
        private class UserChatAdapter : ArrayAdapter<User> {

            public UserChatAdapter(Context context, int resource, List<User> objects)
                :base(context, resource, objects){

            }

            public override View GetView (int position, View convertView, ViewGroup parent)
            {
                LinearLayout userChatListItem = new LinearLayout (Context);
                userChatListItem.Orientation = Orientation.Vertical;
                TextView usernameView = new TextView (Context);
                TextView fullnameView = new TextView (Context);
                User userAtPosition = GetItem (position);
                usernameView.Text = userAtPosition.Username;
                fullnameView.Text = String.Format ("{0} {1}", userAtPosition.FirstName, userAtPosition.LastName);
                userChatListItem.AddView (usernameView);
                userChatListItem.AddView (fullnameView);
                return userChatListItem;
                 
            }

        }

        private async Task<BuddySDK.Notification> HandleSend(){
            EditText messageBody = (EditText)SendDialog.FindViewById (Resource.Id.messageBody);

            if (null != SelectedUser && null != messageBody) {
                var result = await Buddy.SendPushNotificationAsync (new string[] { SelectedUser },
                    null,
                    messageBody.Text);
                if (result.IsSuccess) {
                    return result.Value;
                }
            }
            return null;

        }

        protected async override void OnCreate (Bundle bundle)
        {
            base.OnCreate (bundle);
            SetContentView (Resource.Layout.Push);
            var users = await Buddy.Users.FindAsync ();
            ListView userList = FindViewById<ListView> (Resource.Id.userList);
            UserChatAdapter userListAdapter = new UserChatAdapter (this, Resource.Layout.UserListItem, users.PageResults.ToList());
            userList.Adapter = userListAdapter;
            userList.ItemLongClick += (object sender, AdapterView.ItemLongClickEventArgs e) => {
                User user = users.PageResults.ElementAt(e.Position);
                LayoutInflater inflater = (LayoutInflater) GetSystemService(Context.LayoutInflaterService);
                AlertDialog.Builder senderBuilder = new AlertDialog.Builder(this);
                senderBuilder.SetView(inflater.Inflate(Resource.Layout.PushSendDialog,null));
                SendDialog =  senderBuilder.Create();
                SendDialog.Show();
                SelectedUser = user.ID;
                Button pusher = SendDialog.FindViewById<Button> (Resource.Id.sendMessage);
                pusher.Click += async (clickSender, pushere) => {
                    var note = HandleSend();
                    if(null != note){
                        Toast sendConfirmed = Toast.MakeText(this,"Message sent",ToastLength.Long);
                        sendConfirmed.Show();
                    }
                    if(null != SendDialog){
                        SendDialog.Hide();
                    }

                };
            };





        }
    }
}

