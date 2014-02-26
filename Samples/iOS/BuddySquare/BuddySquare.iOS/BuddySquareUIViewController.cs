using System;
using MonoTouch.UIKit;
using BuddySDK;

namespace BuddySquare.iOS
{
    public class BuddySquareUIViewController: UIViewController
    {
        private bool _userChangeHooked;

        public BuddySquareUIViewController (string xib) :base(xib, null)
        {


        }

        private void HookUserChange(bool hook) {
            if (hook != _userChangeHooked) {
                if (hook) {
                    Buddy.Instance.CurrentUserChanged += HandleCurrentUserChanged;
                } else {
                    Buddy.Instance.CurrentUserChanged -= HandleCurrentUserChanged;
                }
                _userChangeHooked = hook;
            }
        }

        void HandleCurrentUserChanged (object sender, CurrentUserChangedEventArgs e)
        {
            // when the user changes, pop all the way to root!
            //
            if (e.PreviousUser != null) {
                this.NavigationController.PopToRootViewController (false);
            }
        }

        public override void ViewDidAppear (bool animated)
        {
            base.ViewDidAppear (animated);
            HookUserChange (true);
        }

        public override void ViewDidDisappear (bool animated)
        {
            HookUserChange (false);
            base.ViewDidDisappear (animated);
        }
    }
}

