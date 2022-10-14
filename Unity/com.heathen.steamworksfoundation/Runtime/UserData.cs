#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using System;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
#if STEAMWORKSNET
    [Serializable]
    public struct UserData : IEquatable<CSteamID>, IEquatable<ulong>, IEquatable<UserData>
    {
        public static UserData Me => API.User.Client.Id;
        public CSteamID id;
        [Obsolete("Please use id instead")]
        public CSteamID cSteamId
        {
            get => id;
            set => id = value;
        }
        public bool IsMe => id == API.User.Client.Id.id;
        public ulong SteamId
        {
            get => id.m_SteamID;
            set => id = new CSteamID(value);
        }
        /// <summary>
        /// Is this UserData value a valid value.
        /// This does not indicate it is a person simply that structurally the data is possibly a person
        /// </summary>
        public bool IsValid
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
        public Texture2D Avatar => API.Friends.Client.GetLoadedAvatar(id);
        public string Name => API.Friends.Client.GetFriendPersonaName(id);
        public string Nickname
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
        public EPersonaState State => SteamFriends.GetFriendPersonaState(id);
        public bool InGame => SteamFriends.GetFriendGamePlayed(id, out _);
        public FriendGameInfo_t GameInfo
        {
            get
            {
                FriendGameInfo_t result;
                SteamFriends.GetFriendGamePlayed(id, out result);
                return result;
            }
        }
        public int Level => SteamFriends.GetFriendSteamLevel(id);
        /// <summary>
        /// Also known as the "Friend ID" or "Friend Code"
        /// </summary>
        public AccountID_t AccountId
        {
            get => id.GetAccountID();
            set
            {
                id = new CSteamID(value, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeIndividual);
            }
        }
        /// <summary>
        /// Also known as the "Account ID"
        /// </summary>
        public uint FriendId
        { 
            get => AccountId.m_AccountID;
            set
            {
                id = new CSteamID(new AccountID_t(value), EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeIndividual);
            }
        }
        /// <summary>
        /// Gets a collection of names the local user knows for the indicated user
        /// </summary>
        public string[] NameHistory => API.Friends.Client.GetFriendPersonaNameHistory(this);

        public void LoadAvatar(Action<Texture2D> callback) => API.Friends.Client.GetFriendAvatar(id, callback);
        public bool GetGamePlayed(out FriendGameInfo_t gameInfo) => API.Friends.Client.GetFriendGamePlayed(id, out gameInfo);
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
        /// Opens the Overlay Add Friend dialog to add this user as the local user's friend
        /// </summary>
        public void AddFriend() => UserData.AddFriend(this);
        /// <summary>
        /// Opens the Overlay Remove Friend dialog to remove this user as the local user's friend
        /// </summary>
        public void RemoveFriend() => UserData.RemoveFriend(this);
        
        /// <summary>
        /// This clears the rich presence data for the local user
        /// </summary>
        public static void ClearRichPresence() => SteamFriends.ClearRichPresence();
        public static UserData Get(string accountId)
        {
            if (uint.TryParse(accountId, out uint result))
                return Get(result);
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
        public int CompareTo(UserData other)
        {
            return id.CompareTo(other.id);
        }

        public int CompareTo(CSteamID other)
        {
            return id.CompareTo(other);
        }

        public int CompareTo(ulong other)
        {
            return id.m_SteamID.CompareTo(other);
        }

        public override string ToString()
        {
            return id.ToString();
        }

        public bool Equals(UserData other)
        {
            return id.Equals(other.id);
        }

        public bool Equals(CSteamID other)
        {
            return id.Equals(other);
        }

        public bool Equals(ulong other)
        {
            return id.m_SteamID.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return id.m_SteamID.Equals(obj);
        }

        public override int GetHashCode()
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
#elif FACEPUNCH
    [Serializable]
    public struct UserData : IEquatable<SteamId>, IEquatable<Friend>, IEquatable<ulong>, IEquatable<UserData>
    {
        public static UserData Me => API.User.Client.Id;
        public SteamId id;
        [Obsolete("Please use id instead")]
        public SteamId cSteamId
        {
            get => id;
            set => id = value;
        }
        public bool IsMe => id == API.User.Client.Id.id;
        public ulong SteamId
        {
            get => id.Value;
            set => id = new SteamId { Value = value };
        }
        /// <summary>
        /// Is this UserData value a valid value.
        /// This does not indicate it is a person simply that structurally the data is possibly a person
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (id == 0
                    || ExtendedSteamId.GetEAccountType(id) != EAccountType.Individual
                    || ExtendedSteamId.GetEUniverse(id) != Universe.Public)
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
        public Texture2D Avatar => API.Friends.Client.GetLoadedAvatar(this);
        public string Name => API.Friends.Client.GetFriendPersonaName(this);
        public string Nickname
        {
            get
            {
                var value = API.Friends.Client.GetPlayerNickname(this);
                if (!string.IsNullOrEmpty(value))
                    return value;
                else
                    return API.Friends.Client.GetFriendPersonaName(this);
            }
        }
        public FriendState State => new Friend(this.id).State;
        public bool InGame
        {
            get
            {
                var friend = new Friend(this.id);
                if (friend.GameInfo.HasValue)
                    return true;
                else
                    return false;
            }
        }
        public Friend.FriendGameInfo? GameInfo
        {
            get
            {
                var friend = new Friend(this.id);
                return friend.GameInfo;
            }
        }
        public int Level
        {
            get
            {
                var friend = new Friend(this.id);
                return friend.SteamLevel;
            }
        }
        /// <summary>
        /// Also known as the "Friend ID" or "Friend Code"
        /// </summary>
        public uint AccountId
        {
            get => id.AccountId;
            set
            {
                ExtendedSteamId.SetAccountID(id, value);
            }
        }
        /// <summary>
        /// Also known as the "Account ID"
        /// </summary>
        public uint FriendId
        {
            get => id.AccountId;
            set
            {
                ExtendedSteamId.SetAccountID(id, value);
            }
        }
        /// <summary>
        /// Gets a collection of names the local user knows for the indicated user
        /// </summary>
        public string[] NameHistory => API.Friends.Client.GetFriendPersonaNameHistory(this);

        public void LoadAvatar(Action<Texture2D> callback) => API.Friends.Client.GetFriendAvatar(this, callback);
        public bool GetGamePlayed(out Friend.FriendGameInfo gameInfo) => API.Friends.Client.GetFriendGamePlayed(this, out gameInfo);
        [System.Obsolete("You are useing Facepunch which does not support InviteUserToGame features, if you require this feature then remove Facepunch and install Steamworks.NET")]
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

        public bool InviteToLobby(Lobby lobby) => lobby.InviteUserToLobby(this);
        /// <summary>
        /// Opens the Overlay Add Friend dialog to add this user as the local user's friend
        /// </summary>
        public void AddFriend() => UserData.AddFriend(this);
        /// <summary>
        /// Opens the Overlay Remove Friend dialog to remove this user as the local user's friend
        /// </summary>
        public void RemoveFriend() => UserData.RemoveFriend(this);

        /// <summary>
        /// This clears the rich presence data for the local user
        /// </summary>
        public static void ClearRichPresence() => SteamFriends.ClearRichPresence();
        public static UserData Get(string accountId)
        {
            if (uint.TryParse(accountId, out uint result))
                return Get(result);
            else
                return 0;
        }
        public static UserData Get(ulong id) => new UserData { id = new SteamId { Value = id } };
        public static UserData Get(SteamId id) => new UserData { id = id };
        public static UserData Get() => API.User.Client.Id;
        public static UserData Get(uint accountId)
        {
            var result = new UserData();
            result = ExtendedSteamId.SetAccountID(result, accountId);
            result = ExtendedSteamId.SetEAccountType(result, EAccountType.Individual);
            result = ExtendedSteamId.SetEUniverse(result, Universe.Public);
            return result;
        }
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
        /// Opens the Overlay to the Remove Friend dialog
        /// </summary>
        /// <param name="user"></param>
        public static void RemoveFriend(UserData user) => API.Overlay.Client.Activate(FriendDialog.friendremove, user);
        /// <summary>
        /// Opens the Overlay to the Remove Friend dialog
        /// </summary>
        /// <param name="user"></param>
        public static void RemoveFriend(uint user) => API.Overlay.Client.Activate(FriendDialog.friendremove, UserData.Get(user));
        #region Boilerplate
        public int CompareTo(UserData other)
        {
            return id.Value.CompareTo(other.id.Value);
        }

        public int CompareTo(SteamId other)
        {
            return id.Value.CompareTo(other.Value);
        }

        public int CompareTo(ulong other)
        {
            return id.Value.CompareTo(other);
        }

        public override string ToString()
        {
            return id.ToString();
        }

        public bool Equals(UserData other)
        {
            return id.Equals(other.id);
        }

        public bool Equals(SteamId other)
        {
            return id.Equals(other);
        }

        public bool Equals(Friend other)
        {
            return id.Equals(other.Id);
        }

        public bool Equals(ulong other)
        {
            return id.Value.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return id.Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public static bool operator ==(UserData l, UserData r) => l.id == r.id;
        public static bool operator ==(SteamId l, UserData r) => l == r.id;
        public static bool operator ==(UserData l, SteamId r) => l.id == r;
        public static bool operator ==(UserData l, Friend r) => l.id == r.Id;
        public static bool operator !=(UserData l, UserData r) => l.id != r.id;
        public static bool operator !=(SteamId l, UserData r) => l != r.id;
        public static bool operator !=(UserData l, SteamId r) => l.id != r;
        public static bool operator !=(UserData l, Friend r) => l.id != r.Id;

        public static implicit operator ulong(UserData c) => c.id.Value;
        public static implicit operator UserData(ulong id) => new UserData { id = id };
        public static implicit operator SteamId(UserData c) => c.id;
        public static implicit operator UserData(SteamId id) => new UserData { id = id };
        public static implicit operator Friend(UserData c) => new Friend(c.id);
        public static implicit operator UserData(Friend f) => f.Id;
        #endregion
    }

    public static class ExtendedSteamId
    {
        public static SteamId SetAccountID(SteamId steamId, uint other)
        {
            steamId.Value = (steamId.Value & ~(0xFFFFFFFFul << (ushort)0)) | (((ulong)(other) & 0xFFFFFFFFul) << (ushort)0);
            return steamId;
        }

        public static SteamId SetAccountInstance(SteamId steamId, uint other)
        {
            steamId.Value = (steamId.Value & ~(0xFFFFFul << (ushort)32)) | (((ulong)(other) & 0xFFFFFul) << (ushort)32);
            return steamId;
        }

        // This is a non standard/custom function not found in C++ Steamworks
        public static SteamId SetEAccountType(SteamId steamId, EAccountType other)
        {
            steamId.Value = (steamId.Value & ~(0xFul << (ushort)52)) | (((ulong)(other) & 0xFul) << (ushort)52);
            return steamId;
        }

        public static SteamId SetEUniverse(SteamId steamId, Universe other)
        {
            steamId.Value = (steamId.Value & ~(0xFFul << (ushort)56)) | (((ulong)(other) & 0xFFul) << (ushort)56);
            return steamId;
        }

        public static uint GetAccountID(SteamId steamId)
        {
            return (uint)(steamId.Value & 0xFFFFFFFFul);
        }

        public static uint GetUnAccountInstance(SteamId steamId)
        {
            return (uint)((steamId.Value >> 32) & 0xFFFFFul);
        }

        public static EAccountType GetEAccountType(SteamId steamId)
        {
            return (EAccountType)((steamId.Value >> 52) & 0xFul);
        }

        public static Universe GetEUniverse(SteamId steamId)
        {
            return (Universe)((steamId.Value >> 56) & 0xFFul);
        }
    }

    public enum EAccountType
    {
        Invalid = 0,
        Individual = 1,
        Multiseat = 2,
        GameServer = 3,
        AnonGameServer = 4,
        Pending = 5,
        ContentServer = 6,
        Clan = 7,
        Chat = 8,
        ConsoleUser = 9,
        AnonUser = 10,
        Max = 11
    }
#endif
}
#endif