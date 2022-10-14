#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using System;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration.API
{
    public struct RichPresenceEntry
    {
        public string key;
        public string value;
    }

    /// <summary>
    /// Functions for accessing and manipulating Steam user information.
    /// </summary>
    /// <remarks>
    /// https://partner.steamgames.com/doc/api/ISteamUser
    /// </remarks>
    public static class User
    {
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                m_StoreAuthURLResponse_t = null;
            }

            private static CallResult<StoreAuthURLResponse_t> m_StoreAuthURLResponse_t;

            /// <summary>
            /// Gets the Steam ID of the account currently logged into the Steam client. This is commonly called the 'current user', or 'local user'.
            /// </summary>
            public static UserData Id => SteamUser.GetSteamID();
            public static int Level => SteamUser.GetPlayerSteamLevel();
            public static RichPresenceEntry[] RichPresence
            {
                get 
                {
                    var count = SteamFriends.GetFriendRichPresenceKeyCount(SteamUser.GetSteamID());
                    var results = new RichPresenceEntry[count];
                    for (int i = 0; i < count; i++)
                    {
                        var key = SteamFriends.GetFriendRichPresenceKeyByIndex(SteamUser.GetSteamID(), i);
                        var value = SteamFriends.GetFriendRichPresence(SteamUser.GetSteamID(), key);
                        results[i] = new RichPresenceEntry { key = key, value = value };
                    }

                    return results;
                }
                set
                {
                    foreach(var entry in value)
                    {
                        SteamFriends.ClearRichPresence();
                        SteamFriends.SetRichPresence(entry.key, entry.value);
                    }
                }
            }
            public static bool IsBehindNAT => SteamUser.BIsBehindNAT();
            public static bool IsPhoneIdentifying => SteamUser.BIsPhoneIdentifying();
            public static bool IsPhoneRequiringVerification => SteamUser.BIsPhoneRequiringVerification();
            public static bool IsPhoneVerified => SteamUser.BIsPhoneVerified();
            public static bool IsTwoFactorEnabled => SteamUser.BIsTwoFactorEnabled();
            public static bool LoggedOn => SteamUser.BLoggedOn();

            /// <summary>
            /// Set the rich presence data for an unsecured game server that the user is playing on. This allows friends to be able to view the game info and join your game.
            /// </summary>
            /// <param name="gameServerId">This should be k_steamIDNonSteamGS if you're setting the IP/Port, otherwise it should be k_steamIDNil if you're clearing this.</param>
            /// <param name="ip">The IP of the game server in host order</param>
            /// <param name="port">	The connection port of the game server, in host order.</param>
            public static void AdvertiseGame(CSteamID gameServerId, uint ip, ushort port) => SteamUser.AdvertiseGame(gameServerId, ip, port);
            /// <summary>
            /// Set the rich presence data for an unsecured game server that the user is playing on. This allows friends to be able to view the game info and join your game.
            /// </summary>
            /// <param name="gameServerId">This should be k_steamIDNonSteamGS if you're setting the IP/Port, otherwise it should be k_steamIDNil if you're clearing this.</param>
            /// <param name="ip">The IP of the game server in host order</param>
            /// <param name="port">	The connection port of the game server, in host order.</param>
            public static void AdvertiseGame(CSteamID gameServerId, string ip, ushort port) => SteamUser.AdvertiseGame(gameServerId, Utilities.IPStringToUint(ip), port);
            /// <summary>
            /// Gets the level of the users Steam badge for your game.
            /// </summary>
            /// <param name="series">If you only have one set of cards, the series will be 1.</param>
            /// <param name="foil">Check if they have received the foil badge.</param>
            /// <returns>The level of the badge, 0 if they don't have it.</returns>
            public static int GetGameBadgeLevel(int series, bool foil) => SteamUser.GetGameBadgeLevel(series, foil);
            /// <summary>
            /// Requests a URL which authenticates an in-game browser for store check-out, and then redirects to the specified URL. As long as the in-game browser accepts and handles session cookies, Steam microtransaction checkout pages will automatically recognize the user instead of presenting a login page.
            /// </summary>
            /// <remarks>
            /// <para>
            /// NOTE: The URL has a very short lifetime to prevent history-snooping attacks, so you should only call this API when you are about to launch the browser, or else immediately navigate to the result URL using a hidden browser window.
            /// </para>
            /// <para>
            /// NOTE: The resulting authorization cookie has an expiration time of one day, so it would be a good idea to request and visit a new auth URL every 12 hours.
            /// </para>
            /// </remarks>
            /// <param name="redirectUrl"></param>
            /// <param name="callback">Invoked by Steam when the request is resolved</param>
            public static void RequestStoreAuthURL(string redirectUrl, Action<StoreAuthURLResponse_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_StoreAuthURLResponse_t == null)
                    m_StoreAuthURLResponse_t = CallResult<StoreAuthURLResponse_t>.Create();

                var handle = SteamUser.RequestStoreAuthURL(redirectUrl);
                m_StoreAuthURLResponse_t.Set(handle, callback.Invoke);
            }

            public static bool SetRichPresence(string key, string value) => SteamFriends.SetRichPresence(key, value);
            public static void ClearRichPresence() => SteamFriends.ClearRichPresence();
            public static string GetRichPresence(string key) => SteamFriends.GetFriendRichPresence(SteamUser.GetSteamID(), key);
        }
    }
}
#endif