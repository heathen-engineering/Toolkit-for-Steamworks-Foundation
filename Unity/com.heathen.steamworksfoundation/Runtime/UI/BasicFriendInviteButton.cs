#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
#if ENABLE_INPUT_SYSTEM
#endif

using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration.UI
{
    /// <summary>
    /// A basic implementation of <see cref="UserInviteButton"/> for use in the <see cref="FriendInviteDropDown"/>
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/programming-tools/friendinvitebutton")]
    public class BasicFriendInviteButton : UserInviteButton
    {
        /// <summary>
        /// The user avatar icon for this friend
        /// </summary>
        public SetUserAvatar avatar;
        /// <summary>
        /// The display name of the friend
        /// </summary>
        public SetUserName displayName;
        /// <summary>
        /// The friend's status 
        /// </summary>
        public SetUserStatus status;
        /// <summary>
        /// Sets the <see cref="UserData"/> for the <see cref="avatar"/>, <see cref="displayName"/> and <see cref="status"/> attributes
        /// </summary>
        /// <param name="user"></param>
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