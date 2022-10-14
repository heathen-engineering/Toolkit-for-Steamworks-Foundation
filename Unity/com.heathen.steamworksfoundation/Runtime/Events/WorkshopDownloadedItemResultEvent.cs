#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using System;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    [Serializable]
    public class WorkshopDownloadedItemResultEvent : UnityEvent<DownloadItemResult_t>
    { }
}
#endif
