#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using System;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
#endif

namespace HeathenEngineering.SteamworksIntegration.UI
{
    [Serializable]
    public class UnityUserAndPointerDataEvent : UnityEvent<UserAndPointerData>
    { }
}
#endif