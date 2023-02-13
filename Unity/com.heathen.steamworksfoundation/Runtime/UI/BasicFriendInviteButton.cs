#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
#if ENABLE_INPUT_SYSTEM
#endif

namespace HeathenEngineering.SteamworksIntegration.UI
{
    public class BasicFriendInviteButton : FriendInviteButton
    {
        public SetUserAvatar avatar;
        public SetUserName displayName;
        public SetUserStatus status;

        public override void SetFriend(UserData user)
        {
            this.UserData = user;
            avatar.UserData = user;
            displayName.UserData = user;
            status.UserData = user;
        }
    }
}
#endif