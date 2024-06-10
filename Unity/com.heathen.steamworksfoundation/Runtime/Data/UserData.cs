#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// Represents a user and exposes key data in relation to that user
    /// </summary>
    [Serializable]
    public struct UserData : IEquatable<CSteamID>, IEquatable<ulong>, IEquatable<UserData>
    {
        /// <summary>
        /// Get the <see cref="UserData"/> for the local user
        /// </summary>
        public static UserData Me => API.User.Client.Id;
        /// <summary>
        /// The native <see cref="CSteamID"/> of the user
        /// </summary>
        public CSteamID id;
        /// <summary>
        /// Is this user the local user
        /// </summary>
        public readonly bool IsMe => id == API.User.Client.Id.id;
        /// <summary>
        /// The primitive value of the user's id
        /// </summary>
        public readonly ulong SteamId
        {
            get => id.m_SteamID;
        }
        /// <summary>
        /// Is this UserData value a valid value.
        /// This does not indicate it is a person simply that structurally the data is possibly a person
        /// </summary>
        public readonly bool IsValid
        {
            get
            {
                if (id == CSteamID.Nil
                    || id.GetEAccountType() != EAccountType.k_EAccountTypeIndividual
                    || id.GetEUniverse() != EUniverse.k_EUniversePublic)
                    return false;
                else
                    return true;
            }
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
        public readonly Texture2D Avatar => API.Friends.Client.GetLoadedAvatar(id);
        /// <summary>
        /// The user's displayed name
        /// </summary>
        public readonly string Name => API.Friends.Client.GetFriendPersonaName(id);
        /// <summary>
        /// The nick name assigned to this user by the local user if any, if none this will be the same as <see cref="Name"/>
        /// </summary>
        public readonly string Nickname
        {
            get
            {
                var value = API.Friends.Client.GetPlayerNickname(id);
                if (!string.IsNullOrEmpty(value))
                    return value;
                else
                    return API.Friends.Client.GetFriendPersonaName(id);
            }
        }
        /// <summary>
        /// The user's persona state value
        /// </summary>
        public readonly EPersonaState State => SteamFriends.GetFriendPersonaState(id);
        /// <summary>
        /// Is this user in a game
        /// </summary>
        public readonly bool InGame => SteamFriends.GetFriendGamePlayed(id, out _);
        /// <summary>
        /// The details about the game the user is in if any
        /// </summary>
        public readonly FriendGameInfo_t GameInfo
        {
            get
            {
                FriendGameInfo_t result;
                SteamFriends.GetFriendGamePlayed(id, out result);
                return result;
            }
        }
        /// <summary>
        /// The Steam Level of the user
        /// </summary>
        public readonly int Level => SteamFriends.GetFriendSteamLevel(id);
        /// <summary>
        /// Also known as the "Friend ID" or "Friend Code"
        /// </summary>
        public readonly AccountID_t AccountId
        {
            get => id.GetAccountID();
        }
        /// <summary>
        /// Also known as the "Account ID"
        /// </summary>
        public readonly uint FriendId
        { 
            get => AccountId.m_AccountID;
        }
        public readonly string HexId => FriendId.ToString("X");
        /// <summary>
        /// Gets a collection of names the local user knows for the indicated user
        /// </summary>
        public readonly string[] NameHistory => API.Friends.Client.GetFriendPersonaNameHistory(this);
        /// <summary>
        /// Requests the user's avatar be loaded from Steam's backend
        /// </summary>
        /// <param name="callback"></param>
        public readonly void LoadAvatar(Action<Texture2D> callback) => API.Friends.Client.GetFriendAvatar(id, callback);
        /// <summary>
        /// Request information about the game the user is playing
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <returns></returns>
        public readonly bool GetGamePlayed(out FriendGameInfo gameInfo) => API.Friends.Client.GetFriendGamePlayed(id, out gameInfo);
        /// <summary>
        /// Invite the user to play on the connection string provided
        /// </summary>
        /// <param name="connectString"></param>
        public readonly void InviteToGame(string connectString) => API.Friends.Client.InviteUserToGame(this, connectString);
        /// <summary>
        /// Send this user a frined chat message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public readonly bool SendMessage(string message) => API.Friends.Client.ReplyToFriendMessage(this, message);
        /// <summary>
        /// Requests the persona name and avatar of a specified user.
        /// </summary>
        /// <returns>
        /// true means that the data has being requested, and a PersonaStateChange_t callback will be posted when it's retrieved. false means that we already have all the details about that user, and functions that require this information can be used immediately.
        /// </returns>
        public readonly bool RequestInformation() => API.Friends.Client.RequestUserInformation(this, false);
        /// <summary>
        /// Get the rich presence value from this user
        /// </summary>
        /// <param name="key">The key to read</param>
        /// <returns>The value that was read</returns>
        public readonly string GetRichPresenceValue(string key) => API.Friends.Client.GetFriendRichPresence(this, key);
        /// <summary>
        /// Opens the Overlay Add Friend dialog to add this user as the local user's friend
        /// </summary>
        public readonly void AddFriend() => UserData.AddFriend(this);
        /// <summary>
        /// Opens the Overlay Remove Friend dialog to remove this user as the local user's friend
        /// </summary>
        public readonly void RemoveFriend() => UserData.RemoveFriend(this);
        /// <summary>
        /// Marks this player as "played with"
        /// </summary>
        public readonly void SetPlayedWith() => API.Friends.Client.SetPlayedWith(this);
        
        /// <summary>
        /// This clears the rich presence data for the local user
        /// </summary>
        public static void ClearRichPresence() => SteamFriends.ClearRichPresence();
        public static UserData Get(string accountId)
        {
            uint id = Convert.ToUInt32(accountId, 16);
            if (id > 0)
                return Get(id);
            else
                return CSteamID.Nil;
        }
        public static UserData Get(ulong id) => new UserData { id = new CSteamID(id) };
        public static UserData Get(CSteamID id) => new UserData { id = id };
        public static UserData Get() => API.User.Client.Id;
        public static UserData Get(uint accountId) => Get(new AccountID_t(accountId));
        public static UserData Get(AccountID_t accountId) => new CSteamID(accountId, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeIndividual);
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
        /// <summary>
        /// Opens the Overlay to the Add Friend dialog
        /// </summary>
        /// <param name="friendId"></param>
        /// <returns>True if ID was parsed, false otherwise</returns>
        public static bool AddFriend(string friendId)
        {
            if (uint.TryParse(friendId, out var friend))
            {
                AddFriend(friend);
                return true;
            }
            else
                return false;
        }
        /// <summary>
        /// Opens the Overlay to the Add Friend dialog
        /// </summary>
        /// <param name="friendId"></param>
        public static void AddFriend(uint friendId) => AddFriend(UserData.Get(friendId));  
        /// <summary>
        /// Opens the Overlay to the Add Friend dialog
        /// </summary>
        /// <param name="user"></param>
        public static void AddFriend(UserData user) => API.Overlay.Client.Activate(FriendDialog.friendadd, user);
        /// <summary>
        /// Opens the Overlay to the Add Friend dialog
        /// </summary>
        /// <param name="user"></param>
        public static void AddFriend(AccountID_t user) => API.Overlay.Client.Activate(FriendDialog.friendadd, UserData.Get(user));
        /// <summary>
        /// Opens the Overlay to the Remove Friend dialog
        /// </summary>
        /// <param name="user"></param>
        public static void RemoveFriend(UserData user) => API.Overlay.Client.Activate(FriendDialog.friendremove, user);
        /// <summary>
        /// Opens the Overlay to the Remove Friend dialog
        /// </summary>
        /// <param name="user"></param>
        public static void RemoveFriend(AccountID_t user) => API.Overlay.Client.Activate(FriendDialog.friendremove, UserData.Get(user));
    #region Boilerplate
        public readonly int CompareTo(UserData other)
        {
            return id.CompareTo(other.id);
        }

        public readonly int CompareTo(CSteamID other)
        {
            return id.CompareTo(other);
        }

        public readonly int CompareTo(ulong other)
        {
            return id.m_SteamID.CompareTo(other);
        }

        public readonly override string ToString()
        {
            return HexId;
        }

        public readonly bool Equals(UserData other)
        {
            return id.Equals(other.id);
        }

        public readonly bool Equals(CSteamID other)
        {
            return id.Equals(other);
        }

        public readonly bool Equals(ulong other)
        {
            return id.m_SteamID.Equals(other);
        }

        public readonly override bool Equals(object obj)
        {
            return id.m_SteamID.Equals(obj);
        }

        public readonly override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public static bool operator ==(UserData l, UserData r) => l.id == r.id;
        public static bool operator ==(CSteamID l, UserData r) => l == r.id;
        public static bool operator ==(UserData l, CSteamID r) => l.id == r;
        public static bool operator !=(UserData l, UserData r) => l.id != r.id;
        public static bool operator !=(CSteamID l, UserData r) => l != r.id;
        public static bool operator !=(UserData l, CSteamID r) => l.id != r;

        public static implicit operator ulong(UserData c) => c.id.m_SteamID;
        public static implicit operator UserData(ulong id) => new UserData { id = new CSteamID(id) };
        public static implicit operator CSteamID(UserData c) => c.id;
        public static implicit operator UserData(CSteamID id) => new UserData { id = id };
    #endregion
    }
}
#endif