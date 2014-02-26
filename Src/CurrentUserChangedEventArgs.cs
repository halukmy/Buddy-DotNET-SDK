using System;

namespace BuddySDK
{
    public class CurrentUserChangedEventArgs {

        public User PreviousUser {get; private set;}
        public AuthenticatedUser NewUser { get; set; }
        public CurrentUserChangedEventArgs(AuthenticatedUser newUser, User previousUser = null) {
            PreviousUser = previousUser;
            NewUser = newUser;
        }
    }
}

