#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;

namespace HeathenEngineering.SteamworksIntegration
{
    [Serializable]
    public struct FriendGameInfo
    {
        public FriendGameInfo_t data;
        public GameData Game => data.m_gameID;
        public string IpAddress => API.Utilities.IPUintToString(data.m_unGameIP);
        public uint IpInt => data.m_unGameIP;
        public ushort GamePort => data.m_usGamePort;
        public ushort QueryPort => data.m_usQueryPort;
        public CSteamID Lobby => data.m_steamIDLobby;

        public static implicit operator FriendGameInfo(FriendGameInfo_t native) => new FriendGameInfo { data = native };
        public static implicit operator FriendGameInfo_t(FriendGameInfo heathen) => heathen.data;
    }
}
#endif