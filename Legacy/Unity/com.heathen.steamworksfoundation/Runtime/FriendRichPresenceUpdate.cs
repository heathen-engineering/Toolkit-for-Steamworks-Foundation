#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;

namespace HeathenEngineering.SteamworksIntegration
{
    [Serializable]
    public struct FriendRichPresenceUpdate
    {
        public FriendRichPresenceUpdate_t data;
        public UserData Friend => data.m_steamIDFriend;
        public AppData App => data.m_nAppID;

        public static implicit operator FriendRichPresenceUpdate(FriendRichPresenceUpdate_t native) => new FriendRichPresenceUpdate { data = native };
        public static implicit operator FriendRichPresenceUpdate_t(FriendRichPresenceUpdate heathen) => heathen.data;
    }
}
#endif