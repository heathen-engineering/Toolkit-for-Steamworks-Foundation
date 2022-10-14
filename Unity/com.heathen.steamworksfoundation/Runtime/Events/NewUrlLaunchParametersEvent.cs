#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
#if STEAMWORKSNET
    [System.Serializable]
    public class NewUrlLaunchParametersEvent : UnityEvent<NewUrlLaunchParameters_t> { }
#elif FACEPUNCH
    [System.Serializable]
    public class NewUrlLaunchParametersEvent : UnityEvent { }
#endif
}
#endif