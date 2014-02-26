using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using BuddySDK;

namespace BuddySquare.iOS
{
    public partial class LoadingViewController : UIViewController
    {
        public LoadingViewController () : base ("LoadingViewController", null)
        {
        }

        public override void DidReceiveMemoryWarning ()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning ();
            
            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            

        }

        public override async void ViewDidAppear (bool animated)
        {
            base.ViewDidAppear (animated);

           
            if (Buddy.CurrentUser != null) {

                try {

                    // make sure we are logged in.
                    await Buddy.CurrentUser.FetchAsync ();

                } catch {

                }

            }

        }
    }
}

