#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
#endif

namespace HeathenEngineering.SteamworksIntegration.UI
{
    /// <summary>
    /// An abstract class representing a user button for use in the <see cref="FriendInviteDropDown"/>
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/programming-tools/friendinvitebutton")]
    public abstract class UserInviteButton : MonoBehaviour, IPointerClickHandler
    {
        /// <summary>
        /// A <see cref="UnityEngine.Events.UnityEvent"/> that will be invoked when the user clicks the UI element
        /// </summary>
        public UnityUserAndPointerDataEvent Click;
        /// <summary>
        /// A field representing the <see cref="UserData"/> related to this button
        /// </summary>
        public UserData UserData { get; protected set; }
        /// <summary>
        /// An implementation of <see cref="IPointerClickHandler"/> this will be invoked when the user clicks on the UI element
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            Click.Invoke(new UserAndPointerData(UserData, eventData));
        }
        /// <summary>
        /// Implement this method to set the <see cref="UserData"/> related to this item. This method would be used to set up the UI elements that will display information about this user. For a simple example see the <see cref="BasicFriendInviteButton"/>
        /// </summary>
        /// <param name="user"></param>
        public abstract void SetFriend(UserData user);
    }
}
#endif