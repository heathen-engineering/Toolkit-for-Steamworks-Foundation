#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;

namespace HeathenEngineering.SteamworksIntegration
{
    [Serializable]
    public struct FavoriteGame
    {
        public AppId_t appId;
        public string IpAddress
        {
            get => API.Utilities.IPUintToString(ipAddress);
            set => ipAddress = API.Utilities.IPStringToUint(value);
        }
        public uint ipAddress;
        public ushort connectionPort;
        public ushort queryPort;
        public DateTime lastPlayedOnServer;
        public bool isHistory;
    }
}
#endif