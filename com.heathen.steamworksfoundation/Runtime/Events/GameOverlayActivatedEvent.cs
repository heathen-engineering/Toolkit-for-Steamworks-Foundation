#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    [System.Serializable]
    public class GameOverlayActivatedEvent : UnityEvent<bool> { }
}
#endif