#if HE_SYSCORE && STEAMWORKS_NET
using Steamworks;
using System;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    [Serializable]
    public struct UserData : IEquatable<CSteamID>, IEquatable<ulong>, IEquatable<UserData>
    {
        public static UserData Me => API.User.Client.Id;
        public CSteamID cSteamId;
        public bool IsMe => cSteamId == API.User.Client.Id.cSteamId;
        public ulong SteamId
        {
            get => cSteamId.m_SteamID;
            set => cSteamId = new CSteamID(value);
        }
        /// <summary>
        /// This will be null if you have not yet called <see cref="LoadAvatar(Action{Texture2D})"/> for this user.
        /// </summary>
        /// <remarks>
        /// <para>Example:</para>
        /// <code>
        /// if(user.Avatar != null)
        ///     rawImage.Texture = user.Avatar;
        /// else
        ///     user.LoadAvatar((result) =&gt;
        ///     {
        ///        rawImage.Texture = result;
        ///     });
        /// </code>
        /// </remarks>
        public Texture2D Avatar => API.Friends.Client.GetLoadedAvatar(cSteamId);
        public string Name => API.Friends.Client.GetFriendPersonaName(cSteamId);
        public string Nickname
        {
            get
            {
                var value = API.Friends.Client.GetPlayerNickname(cSteamId);
                if (!string.IsNullOrEmpty(value))
                    return value;
                else
                    return API.Friends.Client.GetFriendPersonaName(cSteamId);
            }
        }
        public EPersonaState State => SteamFriends.GetFriendPersonaState(cSteamId);
        public bool InGame => SteamFriends.GetFriendGamePlayed(cSteamId, out _);
        public FriendGameInfo_t GameInfo
        {
            get
            {
                FriendGameInfo_t result;
                SteamFriends.GetFriendGamePlayed(cSteamId, out result);
                return result;
            }
        }
        public int Level => SteamFriends.GetFriendSteamLevel(cSteamId);
        /// <summary>
        /// Also known as the "Friend ID" or "Friend Code"
        /// </summary>
        public AccountID_t AccountId => cSteamId.GetAccountID();
        /// <summary>
        /// Also known as the "Account ID"
        /// </summary>
        public uint FriendId => AccountId.m_AccountID;
        /// <summary>
        /// Gets a collection of names the local user knows for the indicated user
        /// </summary>
        public string[] NameHistory => API.Friends.Client.GetFriendPersonaNameHistory(this);

        public void LoadAvatar(Action<Texture2D> callback) => API.Friends.Client.GetFriendAvatar(cSteamId, callback);
        public bool GetGamePlayed(out FriendGameInfo_t gameInfo) => API.Friends.Client.GetFriendGamePlayed(cSteamId, out gameInfo);
        public void InviteToGame(string connectString) => API.Friends.Client.InviteUserToGame(this, connectString);
        public bool SendMessage(string message) => API.Friends.Client.ReplyToFriendMessage(this, message);
        /// <summary>
        /// Requests the persona name and avatar of a specified user.
        /// </summary>
        /// <returns>
        /// true means that the data has being requested, and a PersonaStateChange_t callback will be posted when it's retrieved. false means that we already have all the details about that user, and functions that require this information can be used immediately.
        /// </returns>
        public bool RequestInformation() => API.Friends.Client.RequestUserInformation(this, false);
        public string GetRichPresenceValue(string key) => API.Friends.Client.GetFriendRichPresence(this, key);
                
        /// <summary>
        /// This clears the rich presence data for the local user
        /// </summary>
        public static void ClearRichPresence() => SteamFriends.ClearRichPresence();
        public static UserData Get(ulong id) => new UserData { cSteamId = new CSteamID(id) };
        public static UserData Get(CSteamID id) => new UserData { cSteamId = id };
        public static UserData Get() => API.User.Client.Id;
        public static UserData Get(AccountID_t friendId) => new CSteamID(friendId, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeIndividual);
        /// <summary>
        /// This only updates the local user's rich presence you cannot set the rich presence for other users.
        /// Sets a Rich Presence key/value for the current user that is automatically shared to all friends playing the same game.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Each user can have up to 20 keys set as defined by <see cref="Constants.k_cchMaxRichPresenceKeys"/>.
        /// </para>
        /// <para>
        /// There are two special keys used for viewing/joining games:
        /// </para>
        /// <list type="bullet">
        /// <item>"status"
        /// <para>A UTF-8 string that will show up in the 'view game info' dialog in the Steam friends list.</para></item>
        /// <item>"connect"
        /// <para>A UTF-8 string that contains the command-line for how a friend can connect to a game. This enables the 'join game' button in the 'view game info' dialog, in the steam friends list right click menu, and on the players Steam community profile. Be sure your app implements <see cref="App.LaunchCommandLine"/> so you can disable the popup warning when launched via a command line.</para></item>
        /// </list>
        /// <para>There are three additional special keys used by the new Steam Chat:</para>
        /// <list type="bullet">
        /// <item>"steam_display"
        /// <para>Names a rich presence localization token that will be displayed in the viewing user's selected language in the Steam client UI. See Rich Presence Localization for more info, including a link to a page for testing this rich presence data. If steam_display is not set to a valid localization tag, then rich presence will not be displayed in the Steam client.</para></item>
        /// <item> "steam_player_group"
        /// <para>When set, indicates to the Steam client that the player is a member of a particular group.Players in the same group may be organized together in various places in the Steam UI.This string could identify a party, a server, or whatever grouping is relevant for your game. The string itself is not displayed to users.</para></item>
        /// <item> "steam_player_group_size"
        /// <para>When set, indicates the total number of players in the steam_player_group. The Steam client may use this number to display additional information about a group when all of the members are not part of a user's friends list. (For example, "Bob, Pete, and 4 more".)</para></item>
        /// </list>
        /// <para>
        /// You can clear all of the keys for the current user with ClearRichPresence. To get rich presence keys for friends see: GetFriendRichPresence.
        /// </para>
        /// </remarks>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>
        /// <para>true if the rich presence was set successfully.</para>
        /// <para>false under the following conditions:</para>
        /// <list type="bullet">
        /// <item>Key was longer than <see cref="Constants.k_cchMaxRichPresenceKeyLength"/> or had a length of 0.</item>
        /// <item>Value was longer than <see cref="Constants.k_cchMaxRichPresenceValueLength"/>.</item>
        /// <item>The user has reached the maximum amount of rich presence keys as defined by <see cref="Constants.k_cchMaxRichPresenceKeys"/>.</item>
        /// </list>
        /// </returns>
        public static bool SetRichPresence(string key, string value) => SteamFriends.SetRichPresence(key, value);

        #region Boilerplate
        public int CompareTo(UserData other)
        {
            return cSteamId.CompareTo(other.cSteamId);
        }

        public int CompareTo(CSteamID other)
        {
            return cSteamId.CompareTo(other);
        }

        public int CompareTo(ulong other)
        {
            return cSteamId.m_SteamID.CompareTo(other);
        }

        public override string ToString()
        {
            return cSteamId.ToString();
        }

        public bool Equals(UserData other)
        {
            return cSteamId.Equals(other.cSteamId);
        }

        public bool Equals(CSteamID other)
        {
            return cSteamId.Equals(other);
        }

        public bool Equals(ulong other)
        {
            return cSteamId.m_SteamID.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return cSteamId.m_SteamID.Equals(obj);
        }

        public override int GetHashCode()
        {
            return cSteamId.GetHashCode();
        }

        public static bool operator ==(UserData l, UserData r) => l.cSteamId == r.cSteamId;
        public static bool operator ==(CSteamID l, UserData r) => l == r.cSteamId;
        public static bool operator ==(UserData l, CSteamID r) => l.cSteamId == r;
        public static bool operator !=(UserData l, UserData r) => l.cSteamId != r.cSteamId;
        public static bool operator !=(CSteamID l, UserData r) => l != r.cSteamId;
        public static bool operator !=(UserData l, CSteamID r) => l.cSteamId != r;

        public static implicit operator ulong(UserData c) => c.cSteamId.m_SteamID;
        public static implicit operator UserData(ulong id) => new UserData { cSteamId = new CSteamID(id) };
        public static implicit operator CSteamID(UserData c) => c.cSteamId;
        public static implicit operator UserData(CSteamID id) => new UserData { cSteamId = id };
#endregion
    }
}
#endif