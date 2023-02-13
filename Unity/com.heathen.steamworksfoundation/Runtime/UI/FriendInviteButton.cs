#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
#endif

namespace HeathenEngineering.SteamworksIntegration.UI
{
    public abstract class FriendInviteButton : MonoBehaviour, IPointerClickHandler
    {
        public UnityUserAndPointerDataEvent Click;
        public UserData UserData { get; protected set; }
        public void OnPointerClick(PointerEventData eventData)
        {
            Click.Invoke(new UserAndPointerData(UserData, eventData));
        }
        public abstract void SetFriend(UserData user);
    }
}
#endif