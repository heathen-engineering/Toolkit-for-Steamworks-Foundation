#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using System;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
#endif

namespace HeathenEngineering.SteamworksIntegration.UI
{
    [Serializable]
    public class UserAndPointerData
    {
        public UserData user;
        public PointerEventData pointerEventData;

        public UserAndPointerData(UserData userData, PointerEventData data)
        {
            user = userData;
            pointerEventData = data;
        }
    }
}
#endif