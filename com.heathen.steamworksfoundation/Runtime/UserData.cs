#if !DISABLESTEAMWORKS && STEAMWORKS_NET
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

#if HE_STEAMCOMPLETE
        public bool InviteToLobby(Lobby lobby) => lobby.InviteUserToLobby(this);
#endif

        public static UserData Get(ulong id) => new UserData { cSteamId = new CSteamID(id) };
        public static UserData Get(CSteamID id) => new UserData { cSteamId = id };
        public static UserData Get() => API.User.Client.Id;
        public static UserData Get(AccountID_t friendId) => new CSteamID(friendId, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeIndividual);

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