using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Threading.Tasks;
using System.Linq;
using BuddySDK;

namespace BuddySquare.iOS
{
    public partial class LoginViewController : UIViewController
    {
        UIViewController _parent;
        Action _done;

        public LoginViewController (UIViewController parent, Action done) : base ("LoginViewController", null)
        {
            _done = done;
            _parent = parent;
        }

        private void Finish() {
            _parent.DismissViewController (true, null);
            _done ();
        }
       
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            
            this.btnLogin.TouchUpInside += HandleTouchUpInside;
        
           
            txtUsername.ShouldReturn += (tf) => {
                tf.ResignFirstResponder();
                return false;
            };
            txtUsername.ShouldEndEditing += (tf) => {
                return true;
            };

            txtPassword.ShouldReturn += (tf) => {
                tf.ResignFirstResponder();
                return false;
            };
            txtPassword.ShouldEndEditing += (tf) => {
                return true;
            };
         
        }

        async void HandleTouchUpInside (object sender, EventArgs e)
        {
            string username = this.txtUsername.Text;
            string password = this.txtPassword.Text;
            bool isLogin = this.isLogin.On;


            Action<BuddyServiceException> showError = (ex) => {
               
                UIAlertView uav =  new UIAlertView("Buddy Login", "Unknown username or password, do you need to sign up?", null, "OK");
                uav.Show();
            };

            BuddyResult<AuthenticatedUser> userTask = null;
             if (isLogin) {
                userTask =  await Buddy.LoginUserAsync (username, password); 
                }
            else {
                userTask = await Buddy.CreateUserAsync (username, password);
            }

            if (userTask.IsSuccess && userTask.Value != null) {
                Finish();
            }
            else {
                showError(userTask.Error);
            }

           
           
        }



    }
}

