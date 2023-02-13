#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;

namespace HeathenEngineering.SteamworksIntegration
{
    [Serializable]
    public struct StringFilter
    {
        public string key;
        public string value;
        public ELobbyComparison comparison;
    }
}
#endif