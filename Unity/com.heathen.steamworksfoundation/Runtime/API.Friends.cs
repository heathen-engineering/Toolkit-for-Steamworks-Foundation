#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration.API
{

    /// <summary>
    /// Interface to access information about individual users and interact with the Steam Overlay.
    /// </summary>
    /// <remarks>
    /// https://partner.steamgames.com/doc/api/ISteamFriends
    /// </remarks>
    public static class Friends
    {
#if STEAMWORKSNET
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                eventFriendMessageReceived = new GameConnectedFriendChatMsgEvent();
                listeningForFriendMessages = false;
                eventPersonaStateChange = new PersonaStateChangeEvent();
                eventFriendRichPresenceUpdate = new FriendRichPresenceUpdateEvent();
                pendingLinks = new List<ImageRequestCallbackLink>();

                followedIds = new List<CSteamID>();

                if (loadedImages.Count > 0)
                    UnloadAvatarImages();

                loadedImages = new Dictionary<int, Texture2D>();
                userAvatarMapping = new Dictionary<CSteamID, Texture2D>();

                m_FriendsEnumerateFollowingList_t = null;
                m_FriendsGetFollowerCount_t = null;
                m_FriendsIsFollowing_t = null;
                m_SetPersonaNameResponse_t = null;
                m_GameConnectedFriendChatMsg_t = null;
                m_AvatarImageLoaded_t = null;
                m_PersonaStateChange_t = null;
                m_FriendRichPresenceUpdate_t = null;
            }

            private class ImageRequestCallbackLink
            {
                public CSteamID owner;
                public Action<Texture2D> callback;
            }

            /// <summary>
            /// Called when chat message has been received from a friend.
            /// </summary>
            /// <remarks> 
            /// You must enable friend messages by setting ListenForFrendsMessages to true;
            /// </remarks>
            public static GameConnectedFriendChatMsgEvent EventGameConnectedFriendChatMsg
            {
                get
                {
                    if (m_GameConnectedFriendChatMsg_t == null)
                        m_GameConnectedFriendChatMsg_t = Callback<GameConnectedFriendChatMsg_t>.Create((result) =>
                        {
                            var count = SteamFriends.GetFriendMessage(result.m_steamIDUser, result.m_iMessageID, out string message, 8193, out EChatEntryType type);
                            if (count > 0)
                                eventFriendMessageReceived.Invoke(result.m_steamIDUser, message, type);
                        });

                    return eventFriendMessageReceived;
                }
            }

            /// <summary>
            /// Called when Rich Presence data has been updated for a user, this can happen automatically when friends in the same game update their rich presence, or after a call to RequestFriendRichPresence.
            /// </summary>
            public static FriendRichPresenceUpdateEvent EventFriendRichPresenceUpdate
            {
                get
                {
                    if (m_FriendRichPresenceUpdate_t == null)
                        m_FriendRichPresenceUpdate_t = Callback<FriendRichPresenceUpdate_t>.Create(eventFriendRichPresenceUpdate.Invoke);

                    return eventFriendRichPresenceUpdate;
                }
            }

            public static PersonaStateChangeEvent EventPersonaStateChange
            {
                get
                {
                    if (m_PersonaStateChange_t == null)
                        m_PersonaStateChange_t = Callback<PersonaStateChange_t>.Create(HandlePersonaStateChange);

                    return eventPersonaStateChange;
                }
            }

            /// <summary>
            /// Listens for Steam friends chat messages.
            /// </summary>
            public static bool ListenForFrendsMessages
            {
                get => listeningForFriendMessages;
                set
                {
                    SteamFriends.SetListenForFriendsMessages(value);
                    listeningForFriendMessages = value;
                }
            }
            /// <summary>
            /// Returns the local user's persona name
            /// </summary>
            public static string PersonaName => SteamFriends.GetPersonaName();
            /// <summary>
            /// Returns the local user's persona state
            /// </summary>
            public static EPersonaState PersonaState => SteamFriends.GetPersonaState();
            /// <summary>
            /// Checks if current user is chat restricted. See <see cref="EUserRestriction"/>
            /// </summary>
            public static uint Restrictions => SteamFriends.GetUserRestrictions();

            private static GameConnectedFriendChatMsgEvent eventFriendMessageReceived = new GameConnectedFriendChatMsgEvent();
            private static bool listeningForFriendMessages = false;
            private static PersonaStateChangeEvent eventPersonaStateChange = new PersonaStateChangeEvent();
            private static FriendRichPresenceUpdateEvent eventFriendRichPresenceUpdate = new FriendRichPresenceUpdateEvent();

            private static List<ImageRequestCallbackLink> pendingLinks = new List<ImageRequestCallbackLink>();
            private static Dictionary<int, Texture2D> loadedImages = new Dictionary<int, Texture2D>();
            private static Dictionary<CSteamID, Texture2D> userAvatarMapping = new Dictionary<CSteamID, Texture2D>();

            private static CallResult<FriendsEnumerateFollowingList_t> m_FriendsEnumerateFollowingList_t;
            private static CallResult<FriendsGetFollowerCount_t> m_FriendsGetFollowerCount_t;
            private static CallResult<FriendsIsFollowing_t> m_FriendsIsFollowing_t;


            private static CallResult<SetPersonaNameResponse_t> m_SetPersonaNameResponse_t;

            private static Callback<GameConnectedFriendChatMsg_t> m_GameConnectedFriendChatMsg_t;
            private static Callback<AvatarImageLoaded_t> m_AvatarImageLoaded_t;
            private static Callback<PersonaStateChange_t> m_PersonaStateChange_t;
            private static Callback<FriendRichPresenceUpdate_t> m_FriendRichPresenceUpdate_t;

            private static bool loadingFollowed = false;
            private static List<CSteamID> followedIds = new List<CSteamID>();

            /// <summary>
            /// Clears all of the current user's Rich Presence key/values.
            /// </summary>
            public static void ClearRichPresence() => SteamFriends.ClearRichPresence();
            public static void GetFollowed(Action<CSteamID[]> callback)
            {
                if (callback != null)
                {
                    if (followedIds != null && followedIds.Count > 0)
                        callback.Invoke(followedIds.ToArray());
                    else
                        SteamSettings.behaviour.StartCoroutine(FetchAllFollowed(callback));
                }
            }

            private static IEnumerator FetchAllFollowed(Action<CSteamID[]> callback)
            {
                if (!loadingFollowed)
                {
                    loadingFollowed = true;
                    uint index = 0;
                    int read = 0;
                    int total = 0;
                    bool waiting = true;
                    bool hasError = false;

                    EnumerateFollowingList(index, (r, e) =>
                    {
                        if (!e)
                        {
                            followedIds.AddRange(r.m_rgSteamID.Where(p => p != CSteamID.Nil));
                            total = r.m_nTotalResultCount;
                            read = r.m_nResultsReturned;
                        }
                        else
                            hasError = true;

                        waiting = false;
                    });

                    while (waiting)
                        yield return null;

                    if (read < total)
                    {
                        while (read < total && !hasError)
                        {
                            EnumerateFollowingList((uint)read, (r, e) =>
                            {
                                if (!e)
                                {
                                    followedIds.AddRange(r.m_rgSteamID.Where(p => p != CSteamID.Nil));
                                    total = r.m_nTotalResultCount;
                                    read += r.m_nResultsReturned;
                                }
                                else
                                    hasError = true;

                                waiting = false;
                            });

                            while (waiting)
                                yield return null;
                        }
                    }

                    loadingFollowed = false;
                }
                else
                {
                    while (loadingFollowed)
                        yield return null;
                }

                callback.Invoke(followedIds.ToArray());
            }

            /// <summary>
            /// Gets the list of users that the current user is following.
            /// </summary>
            /// <remarks>
            /// NOTE: This returns up to <see cref="Constants.k_cEnumerateFollowersMax"/> users at once. If the current user is following more than that, you will need to call this repeatedly, with startIndex set to the total number of followers that you have received so far. I.E.If you have received 50 followers, and the user is following 105, you will need to call this again with unStartIndex = 50 to get the next 50, and then again with unStartIndex = 100 to get the remaining 5 users.
            /// </remarks>
            /// <param name="index">The index to start at</param>
            /// <param name="callback">Invoked when Steam API responds with the results</param>
            public static void EnumerateFollowingList(uint index, Action<FriendsEnumerateFollowingList_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_FriendsEnumerateFollowingList_t == null)
                    m_FriendsEnumerateFollowingList_t = CallResult<FriendsEnumerateFollowingList_t>.Create();

                var handle = SteamFriends.EnumerateFollowingList(index);
                m_FriendsEnumerateFollowingList_t.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Gets the Steam ID of the recently played with user at the given index.
            /// </summary>
            /// <param name="coplayFriend"></param>
            /// <returns></returns>
            public static UserData GetCoplayFriend(int coplayFriendIndex) => SteamFriends.GetCoplayFriend(coplayFriendIndex);
            /// <summary>
            /// Gets the number of players that the current user has recently played with, across all games.
            /// </summary>
            /// <returns></returns>
            public static int GetCoplayFriendCount() => SteamFriends.GetCoplayFriendCount();
            /// <summary>
            /// Get the list of players the current useer has recently played with across all games
            /// </summary>
            /// <returns></returns>
            public static UserData[] GetCoplayFriends()
            {
                var count = SteamFriends.GetCoplayFriendCount();
                if (count > 0)
                {
                    var results = new UserData[count];
                    for (int i = 0; i < count; i++)
                    {
                        results[i] = GetCoplayFriend(i);
                    }

                    return results;
                }
                else
                    return new UserData[0];
            }
            /// <summary>
            /// Gets the number of users following the specified user.
            /// </summary>
            /// <param name="userId">The user to get the list for</param>
            /// <param name="callback">Invoked when Steam API returns the results</param>
            public static void GetFollowerCount(UserData userId, Action<FriendsGetFollowerCount_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_FriendsGetFollowerCount_t == null)
                    m_FriendsGetFollowerCount_t = CallResult<FriendsGetFollowerCount_t>.Create();

                var handle = SteamFriends.GetFollowerCount(userId);
                m_FriendsGetFollowerCount_t.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Gets the Steam ID of the user at the given index.
            /// </summary>
            /// <param name="index">An index between 0 and GetFriendCount.</param>
            /// <param name="flags">A combined union (binary "or") of EFriendFlags. This must be the same value as used in the previous call to GetFriendCount.</param>
            /// <returns></returns>
            public static UserData GetFriendByIndex(int index, EFriendFlags flags) => SteamFriends.GetFriendByIndex(index, flags);
            /// <summary>
            /// Gets the app ID of the game that user played with someone on their recently-played-with list.
            /// </summary>
            /// <param name="userId">The Steam ID of the user on the recently-played-with list to get the game played.</param>
            /// <returns></returns>
            public static AppId_t GetFriendCoplayGame(UserData userId) => SteamFriends.GetFriendCoplayGame(userId);
            /// <summary>
            /// Gets the timestamp of when the user played with someone on their recently-played-with list.
            /// </summary>
            /// <param name="userId">The Steam ID of the user on the recently-played-with list to get the timestamp for.</param>
            /// <returns></returns>
            public static DateTime GetFriendCoplayTime(UserData userId) => new DateTime(1970, 1, 1).AddSeconds(SteamFriends.GetFriendCoplayTime(userId));
            /// <summary>
            /// Gets the number of users the client knows about who meet a specified criteria. (Friends, blocked, users on the same server, etc)
            /// </summary>
            /// <param name="flags"></param>
            /// <returns></returns>
            public static int GetFriendCount(EFriendFlags flags) => SteamFriends.GetFriendCount(flags);
            /// <summary>
            /// Returns the users the client knows about who meet the specified criteria.
            /// </summary>
            /// <param name="flags"></param>
            /// <returns></returns>
            public static UserData[] GetFriends(EFriendFlags flags)
            {
                var count = SteamFriends.GetFriendCount(flags);
                if (count > 0)
                {
                    var results = new UserData[count];
                    for (int i = 0; i < count; i++)
                    {
                        results[i] = SteamFriends.GetFriendByIndex(i, flags);
                    }
                    return results;
                }
                else
                    return new UserData[0];
            }
            /// <summary>
            /// Get the number of users in a source (Steam group, chat room, lobby, or game server).
            /// </summary>
            /// <param name="source">The Steam group, chat room, lobby or game server to get the user count of.</param>
            /// <returns></returns>
            public static int GetFriendCountFromSource(CSteamID source) => SteamFriends.GetFriendCountFromSource(source);
            /// <summary>
            /// Gets the Steam ID at the given index from a source (Steam group, chat room, lobby, or game server).
            /// </summary>
            /// <param name="source"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            public static UserData GetFriendFromSourceByIndex(CSteamID source, int index) => SteamFriends.GetFriendFromSourceByIndex(source, index);
            /// <summary>
            /// Gets the list of friends the user knows from the given source
            /// </summary>
            /// <param name="source"></param>
            /// <returns></returns>
            public static UserData[] GetFriendsFromSource(CSteamID source)
            {
                var count = SteamFriends.GetFriendCountFromSource(source);
                if (count > 0)
                {
                    var results = new UserData[count];
                    for (int i = 0; i < count; i++)
                    {
                        results[i] = SteamFriends.GetFriendFromSourceByIndex(source, i);
                    }
                    return results;
                }
                else
                    return new UserData[0];
            }
            /// <summary>
            /// Checks if the specified friend is in a game, and gets info about the game if they are.
            /// </summary>
            /// <param name="userId">The Steam ID of the other user.</param>
            /// <param name="results">Fills in the details if the user is in a game.</param>
            /// <returns></returns>
            public static bool GetFriendGamePlayed(UserData userId, out FriendGameInfo_t results) => SteamFriends.GetFriendGamePlayed(userId, out results);
            /// <summary>
            /// Gets the data from a Steam friends message.
            /// </summary>
            /// <param name="userId">The Steam ID of the friend that sent this message.</param>
            /// <param name="index">The index of the message. This should be the m_iMessageID field of GameConnectedFriendChatMsg_t.</param>
            /// <param name="type">Returns the type of chat entry that was received.</param>
            /// <returns></returns>
            public static string GetFriendMessage(UserData userId, int index, out EChatEntryType type)
            {
                SteamFriends.GetFriendMessage(userId, index, out string data, 8193, out type);
                return data;
            }
            /// <summary>
            /// Gets the specified user's persona (display) name.
            /// </summary>
            /// <remarks>
            /// <para>
            /// This will only be known to the current user if the other user is in their friends list, on the same game server, in a chat room or lobby, or in a small Steam group with the local user.
            /// </para>
            /// <para>
            /// NOTE: Upon on first joining a lobby, chat room, or game server the current user will not known the name of the other users automatically; that information will arrive asynchronously via PersonaStateChange_t callbacks.
            /// </para>
            /// <para>
            /// To get the persona name of the current user use GetPersonaName.
            /// </para>
            /// </remarks>
            /// <param name="userId"></param>
            /// <returns></returns>
            public static string GetFriendPersonaName(UserData userId) => SteamFriends.GetFriendPersonaName(userId);
            /// <summary>
            /// Gets one of the previous display names for the specified user.
            /// </summary>
            /// <param name="userId">The Steam ID of the other user.</param>
            /// <param name="index">The index of the history to receive. 0 is their current persona name, 1 is their most recent before they changed it, etc.</param>
            /// <returns></returns>
            public static string GetFriendPersonaNameHistory(UserData userId, int index) => SteamFriends.GetFriendPersonaNameHistory(userId, index);
            /// <summary>
            /// Gets a collection of names the local user knows for the indicated user
            /// </summary>
            /// <param name="userId">user to get the history for</param>
            /// <returns></returns>
            public static string[] GetFriendPersonaNameHistory(UserData userId)
            {
                List<string> collection = new List<string>();
                var index = 0;
                var name = SteamFriends.GetFriendPersonaNameHistory(userId, 0);
                while (!string.IsNullOrEmpty(name))
                {
                    collection.Add(name);
                    index++;
                    name = SteamFriends.GetFriendPersonaNameHistory(userId, index);
                }

                return collection.ToArray();
            }
            /// <summary>
            /// Gets the current status of the specified user.
            /// </summary>
            /// <remarks>
            /// This will only be known to the current user if the other user is in their friends list, on the same game server, in a chat room or lobby, or in a small Steam group with the local user.
            /// </remarks>
            /// <param name="userId">The Steam ID of the other user.</param>
            /// <returns></returns>
            public static EPersonaState GetFriendPersonaState(UserData userId) => SteamFriends.GetFriendPersonaState(userId);
            /// <summary>
            /// Get a Rich Presence value from a specified friend.
            /// </summary>
            /// <param name="userId">The friend to get the Rich Presence value for.</param>
            /// <param name="key">The Rich Presence key to request.</param>
            /// <returns></returns>
            public static string GetFriendRichPresence(UserData userId, string key) => SteamFriends.GetFriendRichPresence(userId, key);
            /// <summary>
            /// 
            /// </summary>
            /// <param name="userId">This should be the same user provided to the previous call to GetFriendRichPresenceKeyCount!</param>
            /// <param name="index">An index between 0 and GetFriendRichPresenceKeyCount.</param>
            /// <returns></returns>
            public static string GetFriendRichPresenceKeyByIndex(UserData userId, int index) => SteamFriends.GetFriendRichPresenceKeyByIndex(userId, index);
            /// <summary>
            /// Gets the number of Rich Presence keys that are set on the specified user.
            /// </summary>
            /// <param name="userId">The Steam ID of the user to get the Rich Presence Key Count of.</param>
            /// <returns></returns>
            public static int GetFriendRichPresenceKeyCount(UserData userId) => SteamFriends.GetFriendRichPresenceKeyCount(userId);
            /// <summary>
            /// Gets a collection of the target users rich presence data
            /// </summary>
            /// <param name="userId">The user to get data for</param>
            /// <returns></returns>
            public static Dictionary<string, string> GetFriendRichPresence(UserData userId)
            {
                var results = new Dictionary<string, string>();
                var count = SteamFriends.GetFriendRichPresenceKeyCount(userId);
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var key = SteamFriends.GetFriendRichPresenceKeyByIndex(userId, i);
                        var value = SteamFriends.GetFriendRichPresence(userId, key);
                        if (!results.ContainsKey(key))
                            results.Add(key, value);
                    }
                }

                return results;
            }
            /// <summary>
            /// Gets the number of friends groups (tags) the user has created.
            /// </summary>
            /// <returns></returns>
            public static int GetFriendsGroupCount() => SteamFriends.GetFriendsGroupCount();
            /// <summary>
            /// Gets the friends group ID for the given index.
            /// </summary>
            /// <param name="index">An index between 0 and GetFriendsGroupCount.</param>
            /// <returns></returns>
            public static FriendsGroupID_t GetFriendsGroupIDByIndex(int index) => SteamFriends.GetFriendsGroupIDByIndex(index);
            /// <summary>
            /// Gets the number of friends groups (tags) the user has created.
            /// </summary>
            /// <returns></returns>
            public static FriendsGroupID_t[] GetFriendsGroups()
            {
                var count = SteamFriends.GetFriendsGroupCount();
                if (count > 0)
                {
                    var results = new FriendsGroupID_t[count];
                    for (int i = 0; i < count; i++)
                    {
                        results[i] = SteamFriends.GetFriendsGroupIDByIndex(i);
                    }
                    return results;
                }
                else
                    return new FriendsGroupID_t[0];
            }
            /// <summary>
            /// Returns the Steam IDs of the friends
            /// </summary>
            /// <param name="groupId"></param>
            /// <returns></returns>
            public static CSteamID[] GetFriendsGroupMembersList(FriendsGroupID_t groupId)
            {
                var count = SteamFriends.GetFriendsGroupMembersCount(groupId);
                if (count > 0)
                {
                    var results = new CSteamID[count];
                    SteamFriends.GetFriendsGroupMembersList(groupId, results, count);
                    return results;
                }
                else
                    return new CSteamID[0];
            }
            /// <summary>
            /// Gets the name for the given friends group.
            /// </summary>
            /// <param name="groupId"></param>
            /// <returns></returns>
            public static string GetFriendsGroupName(FriendsGroupID_t groupId) => SteamFriends.GetFriendsGroupName(groupId);
            /// <summary>
            /// Gets the level of the indicated user if kown by the local user
            /// </summary>
            /// <param name="userId"></param>
            /// <returns></returns>
            public static int GetFriendSteamLevel(UserData userId) => SteamFriends.GetFriendSteamLevel(userId);
            /// <summary>
            /// Gets the "large" avatar for the indicated user if any
            /// </summary>
            /// <param name="userId">The user to look the image up for</param>
            /// <param name="callback">Will be invoked when the process is completed, if no avatar is available the respocne texture will be null</param>
            public static void GetFriendAvatar(UserData userId, Action<Texture2D> callback)
            {
                if (callback == null)
                    return;

                if (m_AvatarImageLoaded_t == null)
                    m_AvatarImageLoaded_t = Callback<AvatarImageLoaded_t>.Create(HandleAvatarImageLoaded);

                if (m_PersonaStateChange_t == null)
                    m_PersonaStateChange_t = Callback<PersonaStateChange_t>.Create(HandlePersonaStateChange);

                if (!SteamFriends.RequestUserInformation(userId, false))
                {
                    var handle = SteamFriends.GetLargeFriendAvatar(userId);
                    if (handle > 0)
                    {
                        if (loadedImages.ContainsKey(handle))
                            callback.Invoke(loadedImages[handle]);
                        else
                        {
                            if (LoadAvatar(handle, userId))
                                callback.Invoke(loadedImages[handle]);
                            else
                            {
                                Debug.LogWarning("Failed to load the requested avatar");
                                callback.Invoke(null);
                            }
                        }
                    }
                    else if (handle < 0)
                    {
                        pendingLinks.Add(new ImageRequestCallbackLink
                        {
                            owner = userId,
                            callback = callback
                        });
                    }
                    else
                    {
                        Debug.LogWarning("No avatar available for this user");
                        callback.Invoke(null);
                    }
                }
                else
                {
                    pendingLinks.Add(new ImageRequestCallbackLink
                    {
                        owner = userId,
                        callback = callback
                    });
                }
            }
            /// <summary>
            /// Unloads all loaded avatar images
            /// </summary>
            public static void UnloadAvatarImages()
            {
                foreach (var kvp in loadedImages)
                {
                    if (kvp.Value != null)
                        GameObject.Destroy(kvp.Value);
                }

                loadedImages.Clear();
                userAvatarMapping.Clear();
            }
            /// <summary>
            /// Unloads a specifci avatar image
            /// </summary>
            /// <param name="image"></param>
            public static void UnloadAvatarImage(Texture2D image)
            {
                var removed = new List<int>();
                foreach (var kvp in loadedImages)
                {
                    if (kvp.Value == image)
                    {
                        removed.Add(kvp.Key);
                    }
                }

                CSteamID user = CSteamID.Nil;
                foreach (var kvp in userAvatarMapping)
                {
                    if (kvp.Value == image)
                    {
                        user = kvp.Key;
                        break;
                    }
                }

                GameObject.Destroy(image);

                foreach (var key in removed)
                    loadedImages.Remove(key);

                userAvatarMapping.Remove(user);
            }
            /// <summary>
            /// Gets the nickname that the current user has set for the specified user.
            /// </summary>
            /// <param name="userId"></param>
            /// <returns></returns>
            public static string GetPlayerNickname(UserData userId) => SteamFriends.GetPlayerNickname(userId);
            /// <summary>
            /// Checks if the user meets the specified criteria. (Friends, blocked, users on the same server, etc)
            /// </summary>
            /// <param name="userId">The Steam user to check the friend status of.</param>
            /// <param name="flags"></param>
            /// <returns>true if the specified user meets any of the criteria specified in iFriendFlags; otherwise, false.</returns>
            public static bool HasFriend(UserData userId, EFriendFlags flags) => SteamFriends.HasFriend(userId, flags);
            /// <summary>
            /// Invites a friend or clan member to the current game using a special invite string.
            /// </summary>
            /// <remarks>
            /// If the target user accepts the invite then the pchConnectString gets added to the command-line when launching the game. If the game is already running for that user, then they will receive a GameRichPresenceJoinRequested_t callback with the connect string.
            /// </remarks>
            /// <param name="userId"></param>
            /// <param name="connectString"></param>
            /// <returns></returns>
            public static bool InviteUserToGame(UserData userId, string connectString) => SteamFriends.InviteUserToGame(userId, connectString);
            /// <summary>
            /// Checks if the current user is following the specified id.
            /// </summary>
            /// <param name="id"></param>
            /// <param name="callback"></param>
            public static void IsFollowing(CSteamID id, Action<FriendsIsFollowing_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_FriendsIsFollowing_t == null)
                    m_FriendsIsFollowing_t = CallResult<FriendsIsFollowing_t>.Create();

                var handle = SteamFriends.IsFollowing(id);
                m_FriendsIsFollowing_t.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Checks if a specified user is in a source (Steam group, chat room, lobby, or game server).
            /// </summary>
            /// <param name="userId"></param>
            /// <param name="sourceId"></param>
            /// <returns></returns>
            public static bool IsUserInSource(UserData userId, CSteamID sourceId) => SteamFriends.IsUserInSource(userId, sourceId);
            /// <summary>
            /// Sends a message to a Steam friend.
            /// </summary>
            /// <param name="userId"></param>
            /// <param name="message"></param>
            /// <returns>true if the message was successfully sent. false if the current user is rate limited or chat restricted.</returns>
            public static bool ReplyToFriendMessage(UserData userId, string message) => SteamFriends.ReplyToFriendMessage(userId, message);
            /// <summary>
            /// Requests Rich Presence data from a specific user.
            /// </summary>
            /// <remarks>
            /// <para>This is used to get the Rich Presence information from a user that is not a friend of the current user, like someone in the same lobby or game server.</para>
            /// <para>This function is rate limited, if you call this too frequently for a particular user then it will just immediately post a callback without requesting new data from the server.</para>
            /// </remarks>
            /// <param name="userId"></param>
            public static void RequestFriendRichPresence(UserData userId) => SteamFriends.RequestFriendRichPresence(userId);
            /// <summary>
            /// Requests the persona name and optionally the avatar of a specified user.
            /// </summary>
            /// <param name="userId"></param>
            /// <param name="nameOnly"></param>
            /// <returns>
            /// true means that the data has being requested, and a PersonaStateChange_t callback will be posted when it's retrieved. false means that we already have all the details about that user, and functions that require this information can be used immediately.
            /// </returns>
            public static bool RequestUserInformation(UserData userId, bool nameOnly) => SteamFriends.RequestUserInformation(userId, nameOnly);
            /// <summary>
            /// Let Steam know that the user is currently using voice chat in game.
            /// </summary>
            /// <remarks>
            /// This will suppress the microphone for all voice communication in the Steam UI.
            /// </remarks>
            /// <param name="speaking"></param>
            public static void SetInGameVoiceSpeaking(bool speaking) => SteamFriends.SetInGameVoiceSpeaking(SteamUser.GetSteamID(), speaking);
            /// <summary>
            /// Listens for Steam friends chat messages.
            /// </summary>
            /// <remarks>
            /// You can then show these chats inline in the game. For example with a Blizzard style chat message system or the chat system in Dota 2.
            /// </remarks>
            /// <param name="enabled"></param>
            /// <returns></returns>
            public static void SetListenForFriendsMessages(bool enabled) => SteamFriends.SetListenForFriendsMessages(enabled);
            /// <summary>
            /// Sets the current user's persona name, stores it on the server and publishes the changes to all friends who are online.
            /// </summary>
            /// <remarks>
            /// <para>
            /// Changes take place locally immediately, and a PersonaStateChange_t callback is posted, presuming success.
            /// </para>
            /// <para>
            /// If the name change fails to happen on the server, then an additional PersonaStateChange_t callback will be posted to change the name back, in addition to the final result available in the call result.
            /// </para>
            /// </remarks>
            /// <param name="name"></param>
            public static void SetPersonaName(string name, Action<SetPersonaNameResponse_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_SetPersonaNameResponse_t == null)
                    m_SetPersonaNameResponse_t = CallResult<SetPersonaNameResponse_t>.Create();

                var handle = SteamFriends.SetPersonaName(name);
                m_SetPersonaNameResponse_t.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Mark a target user as 'played with'.
            /// </summary>
            /// <remarks>
            /// NOTE: The current user must be in game with the other player for the association to work.
            /// </remarks>
            /// <param name="userId"></param>
            public static void SetPlayedWith(UserData userId) => SteamFriends.SetPlayedWith(userId);
            /// <summary>
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
            [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
            public static Texture2D GetLoadedAvatar(UserData id)
            {
                if (userAvatarMapping.ContainsKey(id))
                    return userAvatarMapping[id];
                else
                    return null;
            }

            private static void HandleAvatarImageLoaded(AvatarImageLoaded_t results)
            {
                if (LoadAvatar(results.m_iImage, results.m_steamID))
                {
                    var image = loadedImages[results.m_iImage];

                    foreach (var links in pendingLinks)
                    {
                        if (links.owner == results.m_steamID
                            && links.callback != null)
                            links.callback.Invoke(image);
                    }

                    pendingLinks.RemoveAll(p => p.owner == results.m_steamID);
                }
                else
                {
                    Debug.LogWarning("Steam API responded with an Avatar Loaded [" + results.m_iImage + "] message for user [" + results.m_steamID.m_SteamID + "] however no avatar was found on the local disk.");
                }
            }

            private static void HandlePersonaStateChange(PersonaStateChange_t results)
            {
                switch (results.m_nChangeFlags)
                {
                    case EPersonaChange.k_EPersonaChangeAvatar:
                        var steamId = new CSteamID(results.m_ulSteamID);
                        var avatarData = SteamFriends.GetLargeFriendAvatar(steamId);
                        if (avatarData > 0)
                        {
                            if (LoadAvatar(avatarData, steamId))
                            {
                                var image = loadedImages[avatarData];
                                foreach (var links in pendingLinks)
                                {
                                    if (links.owner == steamId
                                        && links.callback != null)
                                        links.callback.Invoke(image);
                                }

                                pendingLinks.RemoveAll(p => p.owner == steamId);
                            }
                        }
                        break;
                }

                eventPersonaStateChange.Invoke(results);
            }

            private static bool LoadAvatar(int imageHandle, CSteamID user)
            {
                if (SteamUtils.GetImageSize(imageHandle, out uint width, out uint height))
                {
                    Texture2D pointer = null;

                    if (loadedImages.ContainsKey(imageHandle))
                    {
                        pointer = loadedImages[imageHandle];
                    }

                    if (pointer == null)
                    {
                        pointer = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                    }
                    else
                    {
                        GameObject.Destroy(pointer);
                        pointer = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                    }

                    int bufferSize = (int)(width * height * 4);
                    byte[] imageBuffer = new byte[bufferSize];

                    if (SteamUtils.GetImageRGBA(imageHandle, imageBuffer, bufferSize))
                    {
                        pointer.LoadRawTextureData(API.Utilities.FlipImageBufferVertical((int)width, (int)height, imageBuffer));
                        pointer.Apply();
                    }

                    if (loadedImages.ContainsKey(imageHandle))
                        loadedImages[imageHandle] = pointer;
                    else
                        loadedImages.Add(imageHandle, pointer);

                    if (userAvatarMapping.ContainsKey(user))
                        userAvatarMapping[user] = pointer;
                    else
                        userAvatarMapping.Add(user, pointer);

                    return true;
                }
                else
                    return false;
            }

            public static bool PersonaChangeHasFlag(EPersonaChange value, EPersonaChange checkflag)
            {
                return (value & checkflag) == checkflag;
            }

            public static bool PersonaChangeHasAllFlags(EPersonaChange value, params EPersonaChange[] checkflags)
            {
                foreach (var checkflag in checkflags)
                {
                    if ((value & checkflag) != checkflag)
                        return false;
                }
                return true;
            }
        }
#elif FACEPUNCH
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                SteamFriends.OnFriendRichPresenceUpdate -= eventFriendRichPresenceUpdate.Invoke;
                SteamFriends.OnPersonaStateChange -= eventPersonaStateChange.Invoke;
                SteamFriends.OnChatMessage -= eventFriendMessageReceived.Invoke;

                eventFriendMessageReceived = new GameConnectedFriendChatMsgEvent();
                listeningForFriendMessages = false;
                eventPersonaStateChange = new PersonaStateChangeEvent();
                eventFriendRichPresenceUpdate = new FriendRichPresenceUpdateEvent();
                pendingLinks = new List<ImageRequestCallbackLink>();

                followedIds = new List<SteamId>();

                if (loadedImages.Count > 0)
                    UnloadAvatarImages();

                loadedImages = new Dictionary<ulong, Texture2D>();
                userAvatarMapping = new Dictionary<SteamId, Texture2D>();
            }

            private class ImageRequestCallbackLink
            {
                public SteamId owner;
                public Action<Texture2D> callback;
            }

            /// <summary>
            /// Called when chat message has been received from a friend.
            /// </summary>
            /// <remarks> 
            /// You must enable friend messages by setting ListenForFrendsMessages to true;
            /// </remarks>
            public static GameConnectedFriendChatMsgEvent EventGameConnectedFriendChatMsg
            {
                get
                {
                    SteamFriends.OnChatMessage -= eventFriendMessageReceived.Invoke;
                    SteamFriends.OnChatMessage += eventFriendMessageReceived.Invoke;

                    return eventFriendMessageReceived;
                }
            }

            private static void SteamFriends_OnChatMessage(Friend friend, string messageType, string message)
            {
                    eventFriendMessageReceived.Invoke(friend, message, messageType);
            }

            /// <summary>
            /// Called when Rich Presence data has been updated for a user, this can happen automatically when friends in the same game update their rich presence, or after a call to RequestFriendRichPresence.
            /// </summary>
            public static FriendRichPresenceUpdateEvent EventFriendRichPresenceUpdate
            {
                get
                {
                    SteamFriends.OnFriendRichPresenceUpdate -= eventFriendRichPresenceUpdate.Invoke;
                    SteamFriends.OnFriendRichPresenceUpdate += eventFriendRichPresenceUpdate.Invoke;

                    return eventFriendRichPresenceUpdate;
                }
            }

            public static PersonaStateChangeEvent EventPersonaStateChange
            {
                get
                {
                    SteamFriends.OnPersonaStateChange -= eventPersonaStateChange.Invoke;
                    SteamFriends.OnPersonaStateChange += eventPersonaStateChange.Invoke;

                    return eventPersonaStateChange;
                }
            }

            /// <summary>
            /// Listens for Steam friends chat messages.
            /// </summary>
            public static bool ListenForFrendsMessages
            {
                get => listeningForFriendMessages;
                set
                {
                    SteamFriends.ListenForFriendsMessages = value;
                }
            }
            /// <summary>
            /// Returns the local user's persona name
            /// </summary>
            public static string PersonaName => UserData.Me.Name;
            /// <summary>
            /// Returns the local user's persona state
            /// </summary>
            public static FriendState PersonaState => SteamClient.State;
            [Obsolete("Facepunch doesn't expose this properly, if you need it we recomend you use Steamwork.NET instead of Facepunch")]
            public static uint Restrictions => 0;

            private static GameConnectedFriendChatMsgEvent eventFriendMessageReceived = new GameConnectedFriendChatMsgEvent();
            private static bool listeningForFriendMessages = false;
            private static PersonaStateChangeEvent eventPersonaStateChange = new PersonaStateChangeEvent();
            private static FriendRichPresenceUpdateEvent eventFriendRichPresenceUpdate = new FriendRichPresenceUpdateEvent();

            private static List<ImageRequestCallbackLink> pendingLinks = new List<ImageRequestCallbackLink>();
            private static Dictionary<ulong, Texture2D> loadedImages = new Dictionary<ulong, Texture2D>();
            private static Dictionary<SteamId, Texture2D> userAvatarMapping = new Dictionary<SteamId, Texture2D>();

            private static bool loadingFollowed = false;
            private static List<SteamId> followedIds = new List<SteamId>();

            /// <summary>
            /// Clears all of the current user's Rich Presence key/values.
            /// </summary>
            public static void ClearRichPresence() => SteamFriends.ClearRichPresence();
            public static void GetFollowed(Action<SteamId[]> callback)
            {
                if (callback != null)
                {
                    if (followedIds != null && followedIds.Count > 0)
                        callback.Invoke(followedIds.ToArray());
                    else
                        SteamSettings.behaviour.StartCoroutine(FetchAllFollowed(callback));
                }
            }

            private static IEnumerator FetchAllFollowed(Action<SteamId[]> callback)
            {
                var task = SteamFriends.GetFollowingList();
                yield return new WaitUntil(() => { return task.IsCompleted; });

                callback?.Invoke(task.Result);
            }

            /// <summary>
            /// Gets the Steam ID of the recently played with user at the given index.
            /// </summary>
            /// <param name="coplayFriend"></param>
            /// <returns></returns>
            [Obsolete("Not currently supported for Facepunch, use Steamworks.NET if you need this feature")]
            public static UserData GetCoplayFriend(int coplayFriendIndex) => default;
            /// <summary>
            /// Gets the number of players that the current user has recently played with, across all games.
            /// </summary>
            /// <returns></returns>
            [Obsolete("Not currently supported for Facepunch, use Steamworks.NET if you need this feature")]
            public static int GetCoplayFriendCount() => 0;
            /// <summary>
            /// Get the list of players the current useer has recently played with across all games
            /// </summary>
            /// <returns></returns>
            [Obsolete("Not currently supported for Facepunch, use Steamworks.NET if you need this feature")]
            public static UserData[] GetCoplayFriends() => null;
            /// <summary>
            /// Gets the number of users following the specified user.
            /// </summary>
            /// <param name="userId">The user to get the list for</param>
            /// <param name="callback">Invoked when Steam API returns the results</param>
            public static void GetFollowerCount(UserData userId, Action<int> callback)
            {
                if (callback == null)
                    return;

                SteamSettings.behaviour.StartCoroutine(GetFollowersCount(userId, callback));
            }
            private static IEnumerator GetFollowersCount(UserData userId, Action<int> callback)
            {
                var task = SteamFriends.GetFollowerCount(userId);
                yield return new WaitUntil(() => { return task.IsCompleted; });

                callback?.Invoke(task.Result);
            }
            [Obsolete("Not currently supported for Facepunch, use Steamworks.NET if you need this feature")]
            public static UserData GetFriendByIndex(int index, EFriendFlags flags) => default;
            [Obsolete("Not currently supported for Facepunch, use Steamworks.NET if you need this feature")]
            public static AppId GetFriendCoplayGame(UserData userId) => default;
            [Obsolete("Not currently supported for Facepunch, use Steamworks.NET if you need this feature")]
            public static DateTime GetFriendCoplayTime(UserData userId) => default;// new DateTime(1970, 1, 1).AddSeconds(SteamFriends.GetFriendCoplayTime(userId));
            [Obsolete("Not currently supported for Facepunch, use Steamworks.NET if you need this feature")]
            public static int GetFriendCount(EFriendFlags flags) => 0;// SteamFriends.GetFriendCount(flags);
            /// <summary>
            /// Returns the users the client knows about who meet the specified criteria.
            /// </summary>
            /// <param name="flags"></param>
            /// <returns></returns>
            public static UserData[] GetFriends(EFriendFlags flags)
            {
                switch(flags)
                {
                    case EFriendFlags.Immediate:
                        {
                            var results = SteamFriends.GetFriends().ToArray();
                            var output = new UserData[results.Length];
                            for (int i = 0; i < results.Length; i++)
                            {
                                output[i] = results[i];
                            }
                            return output;
                        }
                    case EFriendFlags.ClanMember:
                        {
                            var results = SteamFriends.GetFriendsClanMembers().ToArray();
                            var output = new UserData[results.Length];
                            for (int i = 0; i < results.Length; i++)
                            {
                                output[i] = results[i];
                            }
                            return output;
                        }
                    case EFriendFlags.OnGameServer:
                        {
                            var results = SteamFriends.GetFriendsOnGameServer().ToArray();
                            var output = new UserData[results.Length];
                            for (int i = 0; i < results.Length; i++)
                            {
                                output[i] = results[i];
                            }
                            return output;
                        }
                    case EFriendFlags.FriendshipRequested:
                        {
                            var results = SteamFriends.GetFriendsRequested().ToArray();
                            var output = new UserData[results.Length];
                            for (int i = 0; i < results.Length; i++)
                            {
                                output[i] = results[i];
                            }
                            return output;
                        }
                    case EFriendFlags.RequestingFriendship:
                        {
                            var results = SteamFriends.GetFriendsRequestingFriendship().ToArray();
                            var output = new UserData[results.Length];
                            for (int i = 0; i < results.Length; i++)
                            {
                                output[i] = results[i];
                            }
                            return output;
                        }
                    default:
                        Debug.LogWarning("The requested friend flag is not supported by Facepunch, use Steamworks.NET if this is a required feature!");
                        return null;
                }
            }
            /// <summary>
            /// Get the number of users in a source (Steam group, chat room, lobby, or game server).
            /// </summary>
            /// <param name="source">The Steam group, chat room, lobby or game server to get the user count of.</param>
            /// <returns></returns>
            [Obsolete("Not fully supported for Facepunch, use Steamworks.NET for the full range of features or use GetFriendsFromSource for better performance.")]
            public static int GetFriendCountFromSource(SteamId source) => SteamFriends.GetFromSource(source).Count();
            /// <summary>
            /// Gets the Steam ID at the given index from a source (Steam group, chat room, lobby, or game server).
            /// </summary>
            /// <param name="source"></param>
            /// <param name="index"></param>
            /// <returns></returns>
            [Obsolete("Not fully supported for Facepunch, use Steamworks.NET for the full range of features or use GetFriendsFromSource for better performance.")]
            public static UserData GetFriendFromSourceByIndex(SteamId source, int index) => SteamFriends.GetFromSource(source).ToArray()[index];
            /// <summary>
            /// Gets the list of friends the user knows from the given source
            /// </summary>
            /// <param name="source"></param>
            /// <returns></returns>
            public static UserData[] GetFriendsFromSource(SteamId source)
            {
                var sourceArray = SteamFriends.GetFromSource(source).ToArray();
                if (sourceArray.Length > 0)
                {
                    var results = new UserData[sourceArray.Length];
                    for (int i = 0; i < sourceArray.Length; i++)
                    {
                        results[i] = sourceArray[i];
                    }
                    return results;
                }
                else
                    return new UserData[0];
            }
            /// <summary>
            /// Checks if the specified friend is in a game, and gets info about the game if they are.
            /// </summary>
            /// <param name="userId">The Steam ID of the other user.</param>
            /// <param name="results">Fills in the details if the user is in a game.</param>
            /// <returns></returns>
            public static bool GetFriendGamePlayed(UserData userId, out Friend.FriendGameInfo results)
            {
                if (userId.GameInfo.HasValue)
                    results = userId.GameInfo.Value;
                else
                    results = default;

                return userId.GameInfo.HasValue;
            }
            /// <summary>
            /// Gets the data from a Steam friends message.
            /// </summary>
            /// <param name="userId">The Steam ID of the friend that sent this message.</param>
            /// <param name="index">The index of the message. This should be the m_iMessageID field of GameConnectedFriendChatMsg_t.</param>
            /// <param name="type">Returns the type of chat entry that was received.</param>
            /// <returns></returns>
            [Obsolete("Not supported for Facepunch, use Steamworks.NET if this is required.")]
            public static string GetFriendMessage(UserData userId, int index, out string type)
            {
                type = string.Empty;
                return string.Empty;
            }
            /// <summary>
            /// Gets the specified user's persona (display) name.
            /// </summary>
            /// <remarks>
            /// <para>
            /// This will only be known to the current user if the other user is in their friends list, on the same game server, in a chat room or lobby, or in a small Steam group with the local user.
            /// </para>
            /// <para>
            /// NOTE: Upon on first joining a lobby, chat room, or game server the current user will not known the name of the other users automatically; that information will arrive asynchronously via PersonaStateChange_t callbacks.
            /// </para>
            /// <para>
            /// To get the persona name of the current user use GetPersonaName.
            /// </para>
            /// </remarks>
            /// <param name="userId"></param>
            /// <returns></returns>
            public static string GetFriendPersonaName(UserData userId)
            {
                var friend = new Friend(userId.id);
                return friend.Name;
            }
            /// <summary>
            /// Gets one of the previous display names for the specified user.
            /// </summary>
            /// <param name="userId">The Steam ID of the other user.</param>
            /// <param name="index">The index of the history to receive. 0 is their current persona name, 1 is their most recent before they changed it, etc.</param>
            /// <returns></returns>
            public static string GetFriendPersonaNameHistory(UserData userId, int index) => new Friend(userId.id).NameHistory.ToArray()[index];
            /// <summary>
            /// Gets a collection of names the local user knows for the indicated user
            /// </summary>
            /// <param name="userId">user to get the history for</param>
            /// <returns></returns>
            public static string[] GetFriendPersonaNameHistory(UserData userId) => new Friend(userId.id).NameHistory.ToArray();
            /// <summary>
            /// Gets the current status of the specified user.
            /// </summary>
            /// <remarks>
            /// This will only be known to the current user if the other user is in their friends list, on the same game server, in a chat room or lobby, or in a small Steam group with the local user.
            /// </remarks>
            /// <param name="userId">The Steam ID of the other user.</param>
            /// <returns></returns>
            public static FriendState GetFriendPersonaState(UserData userId) => userId.State;
            /// <summary>
            /// Get a Rich Presence value from a specified friend.
            /// </summary>
            /// <param name="userId">The friend to get the Rich Presence value for.</param>
            /// <param name="key">The Rich Presence key to request.</param>
            /// <returns></returns>
            public static string GetFriendRichPresence(UserData userId, string key) => new Friend(userId.id).GetRichPresence(key);
            /// <summary>
            /// 
            /// </summary>
            /// <param name="userId">This should be the same user provided to the previous call to GetFriendRichPresenceKeyCount!</param>
            /// <param name="index">An index between 0 and GetFriendRichPresenceKeyCount.</param>
            /// <returns></returns>
            [Obsolete("Not supported for Facepunch, use Steamworks.NET if this is required.")]
            public static string GetFriendRichPresenceKeyByIndex(UserData userId, int index) => String.Empty;
            /// <summary>
            /// Gets the number of Rich Presence keys that are set on the specified user.
            /// </summary>
            /// <param name="userId">The Steam ID of the user to get the Rich Presence Key Count of.</param>
            /// <returns></returns>
            [Obsolete("Not supported for Facepunch, use Steamworks.NET if this is required.")]
            public static int GetFriendRichPresenceKeyCount(UserData userId) => 0;
            /// <summary>
            /// Gets a collection of the target users rich presence data
            /// </summary>
            /// <param name="userId">The user to get data for</param>
            /// <returns></returns>
            [Obsolete("Not supported for Facepunch, use Steamworks.NET if this is required.")]
            public static Dictionary<string, string> GetFriendRichPresence(UserData userId) => new Dictionary<string, string>();
            /// <summary>
            /// Gets the number of friends groups (tags) the user has created.
            /// </summary>
            /// <returns></returns>
            [System.Obsolete("You are useing Facepunch which does not support friend groups features, if you require this feature then remove Facepunch and install Steamworks.NET")]
            public static int GetFriendsGroupCount() => 0;//SteamFriends.GetFriendsGroupCount();
            /// <summary>
            /// Gets the friends group ID for the given index.
            /// </summary>
            /// <param name="index">An index between 0 and GetFriendsGroupCount.</param>
            /// <returns></returns>
            [System.Obsolete("You are useing Facepunch which does not support friend groups features, if you require this feature then remove Facepunch and install Steamworks.NET")]
            public static void GetFriendsGroupIDByIndex(int index)
            { }
            /// <summary>
            /// Gets the number of friends groups (tags) the user has created.
            /// </summary>
            /// <returns></returns>
            [System.Obsolete("You are useing Facepunch which does not support friend groups features, if you require this feature then remove Facepunch and install Steamworks.NET")]
            public static void GetFriendsGroups()
            { }
            /// <summary>
            /// Returns the Steam IDs of the friends
            /// </summary>
            /// <param name="groupId"></param>
            /// <returns></returns>
            [System.Obsolete("You are useing Facepunch which does not support friend groups features, if you require this feature then remove Facepunch and install Steamworks.NET")]
            public static SteamId[] GetFriendsGroupMembersList(int groupId)
            {
                return null;
            }
            /// <summary>
            /// Gets the name for the given friends group.
            /// </summary>
            /// <param name="groupId"></param>
            /// <returns></returns>
            [System.Obsolete("You are useing Facepunch which does not support friend grroups features, if you require this feature then remove Facepunch and install Steamworks.NET")]
            public static string GetFriendsGroupName(int groupId) => String.Empty;
            /// <summary>
            /// Gets the level of the indicated user if kown by the local user
            /// </summary>
            /// <param name="userId"></param>
            /// <returns></returns>
            public static int GetFriendSteamLevel(UserData userId) => userId.Level;
            /// <summary>
            /// Gets the "large" avatar for the indicated user if any
            /// </summary>
            /// <param name="userId">The user to look the image up for</param>
            /// <param name="callback">Will be invoked when the process is completed, if no avatar is available the respocne texture will be null</param>
            public static void GetFriendAvatar(UserData userId, Action<Texture2D> callback)
            {
                if (callback == null)
                    return;

                if (loadedImages.ContainsKey(userId))
                    callback.Invoke(loadedImages[userId.id.Value]);
                else
                    SteamSettings.behaviour.StartCoroutine(FacepunchImageLoad(userId, callback));
            }

            private static IEnumerator FacepunchImageLoad(UserData user, Action<Texture2D> callback)
            {
                var handle = SteamFriends.GetLargeAvatarAsync(user);
                yield return new WaitUntil(() => handle.IsCompleted);

                LoadAvatar(handle.Result, user);

                if (loadedImages.ContainsKey(user))
                    callback.Invoke(loadedImages[user]);
                else
                    callback.Invoke(null);
            }

            /// <summary>
            /// Unloads all loaded avatar images
            /// </summary>
            public static void UnloadAvatarImages()
            {
                foreach (var kvp in loadedImages)
                {
                    if (kvp.Value != null)
                        GameObject.Destroy(kvp.Value);
                }

                loadedImages.Clear();
                userAvatarMapping.Clear();
            }
            /// <summary>
            /// Unloads a specifci avatar image
            /// </summary>
            /// <param name="image"></param>
            public static void UnloadAvatarImage(Texture2D image)
            {
                var removed = new List<ulong>();
                foreach (var kvp in loadedImages)
                {
                    if (kvp.Value == image)
                    {
                        removed.Add(kvp.Key);
                    }
                }

                SteamId user = 0;
                foreach (var kvp in userAvatarMapping)
                {
                    if (kvp.Value == image)
                    {
                        user = kvp.Key;
                        break;
                    }
                }

                GameObject.Destroy(image);

                foreach (var key in removed)
                    loadedImages.Remove(key);

                userAvatarMapping.Remove(user);
            }
            /// <summary>
            /// Gets the nickname that the current user has set for the specified user.
            /// </summary>
            /// <param name="userId"></param>
            /// <returns></returns>
            public static string GetPlayerNickname(UserData userId) => ((Friend)userId).Name;
            /// <summary>
            /// Checks if the user meets the specified criteria. (Friends, blocked, users on the same server, etc)
            /// </summary>
            /// <param name="userId">The Steam user to check the friend status of.</param>
            /// <param name="flags"></param>
            /// <returns>true if the specified user meets any of the criteria specified in iFriendFlags; otherwise, false.</returns>
            [System.Obsolete("You are useing Facepunch which does not support HasFriend features, if you require this feature then remove Facepunch and install Steamworks.NET")]
            public static bool HasFriend(UserData userId, EFriendFlags flags) => false;
            /// <summary>
            /// Invites a friend or clan member to the current game using a special invite string.
            /// </summary>
            /// <remarks>
            /// If the target user accepts the invite then the pchConnectString gets added to the command-line when launching the game. If the game is already running for that user, then they will receive a GameRichPresenceJoinRequested_t callback with the connect string.
            /// </remarks>
            /// <param name="userId"></param>
            /// <param name="connectString"></param>
            /// <returns></returns>
            [System.Obsolete("You are useing Facepunch which does not support InviteUserToGame features, if you require this feature then remove Facepunch and install Steamworks.NET")]
            public static bool InviteUserToGame(UserData userId, string connectString) => false;
            /// <summary>
            /// Checks if the current user is following the specified id.
            /// </summary>
            /// <param name="id"></param>
            /// <param name="callback"></param>
            public static void IsFollowing(UserData id, Action<bool> callback)
            {
                if (callback == null)
                    return;

                SteamSettings.behaviour.StartCoroutine(FacepunchIsFollowing(id, callback));
            }
            private static IEnumerator FacepunchIsFollowing(UserData id, Action<bool> callback)
            {
                var handle = SteamFriends.IsFollowing(id);
                yield return new WaitUntil(() => handle.IsCompleted);

                callback?.Invoke(handle.Result);
            }

            /// <summary>
            /// Checks if a specified user is in a source (Steam group, chat room, lobby, or game server).
            /// </summary>
            /// <param name="userId"></param>
            /// <param name="sourceId"></param>
            /// <returns></returns>
            public static bool IsUserInSource(UserData userId, SteamId sourceId) => SteamFriends.GetFromSource(sourceId).Contains(userId);
            /// <summary>
            /// Sends a message to a Steam friend.
            /// </summary>
            /// <param name="userId"></param>
            /// <param name="message"></param>
            /// <returns>true if the message was successfully sent. false if the current user is rate limited or chat restricted.</returns>
            public static bool ReplyToFriendMessage(UserData userId, string message) => ((Friend)userId).SendMessage(message);
            /// <summary>
            /// Requests Rich Presence data from a specific user.
            /// </summary>
            /// <remarks>
            /// <para>This is used to get the Rich Presence information from a user that is not a friend of the current user, like someone in the same lobby or game server.</para>
            /// <para>This function is rate limited, if you call this too frequently for a particular user then it will just immediately post a callback without requesting new data from the server.</para>
            /// </remarks>
            /// <param name="userId"></param>
            [System.Obsolete("You are useing Facepunch which does not support RequestFriendRichPresence features, if you require this feature then remove Facepunch and install Steamworks.NET")]
            public static void RequestFriendRichPresence(UserData userId)
            { }
            /// <summary>
            /// Requests the persona name and optionally the avatar of a specified user.
            /// </summary>
            /// <param name="userId"></param>
            /// <param name="nameOnly"></param>
            /// <returns>
            /// true means that the data has being requested, and a PersonaStateChange_t callback will be posted when it's retrieved. false means that we already have all the details about that user, and functions that require this information can be used immediately.
            /// </returns>
            public static bool RequestUserInformation(UserData userId, bool nameOnly) => SteamFriends.RequestUserInformation(userId, nameOnly);
            /// <summary>
            /// Let Steam know that the user is currently using voice chat in game.
            /// </summary>
            /// <remarks>
            /// This will suppress the microphone for all voice communication in the Steam UI.
            /// </remarks>
            /// <param name="speaking"></param>
            [System.Obsolete("You are useing Facepunch which does not support SetInGameVoiceSpeaking features, if you require this feature then remove Facepunch and install Steamworks.NET")]
            public static void SetInGameVoiceSpeaking(bool speaking)
            { }
            /// <summary>
            /// Listens for Steam friends chat messages.
            /// </summary>
            /// <remarks>
            /// You can then show these chats inline in the game. For example with a Blizzard style chat message system or the chat system in Dota 2.
            /// </remarks>
            /// <param name="enabled"></param>
            /// <returns></returns>
            public static void SetListenForFriendsMessages(bool enabled) => SteamFriends.ListenForFriendsMessages = enabled;
            /// <summary>
            /// Sets the current user's persona name, stores it on the server and publishes the changes to all friends who are online.
            /// </summary>
            /// <remarks>
            /// <para>
            /// Changes take place locally immediately, and a PersonaStateChange_t callback is posted, presuming success.
            /// </para>
            /// <para>
            /// If the name change fails to happen on the server, then an additional PersonaStateChange_t callback will be posted to change the name back, in addition to the final result available in the call result.
            /// </para>
            /// </remarks>
            /// <param name="name"></param>
            [System.Obsolete("You are useing Facepunch which does not support SetPersonaName features, if you require this feature then remove Facepunch and install Steamworks.NET")]
            public static void SetPersonaName()
            { }
            /// <summary>
            /// Mark a target user as 'played with'.
            /// </summary>
            /// <remarks>
            /// NOTE: The current user must be in game with the other player for the association to work.
            /// </remarks>
            /// <param name="userId"></param>
            public static void SetPlayedWith(UserData userId) => SteamFriends.SetPlayedWith(userId);
            /// <summary>
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
            [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
            public static Texture2D GetLoadedAvatar(UserData id)
            {
                if (userAvatarMapping.ContainsKey(id))
                    return userAvatarMapping[id];
                else
                    return null;
            }

            private static bool LoadAvatar(Steamworks.Data.Image? image, UserData user)
            {
                if (!image.HasValue)
                    return false;

                uint width = image.Value.Width;
                uint height = image.Value.Height;

                Texture2D pointer = null;

                if (loadedImages.ContainsKey(user))
                {
                    pointer = loadedImages[user];
                }

                if (pointer == null)
                {
                    pointer = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                }
                else
                {
                    GameObject.Destroy(pointer);
                    pointer = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                }

                int bufferSize = (int)(width * height * 4);
                byte[] imageBuffer = image.Value.Data;

                pointer.LoadRawTextureData(API.Utilities.FlipImageBufferVertical((int)width, (int)height, imageBuffer));
                pointer.Apply();

                if (loadedImages.ContainsKey(user))
                    loadedImages[user] = pointer;
                else
                    loadedImages.Add(user, pointer);

                if (userAvatarMapping.ContainsKey(user))
                    userAvatarMapping[user] = pointer;
                else
                    userAvatarMapping.Add(user, pointer);

                return true;
            }

            public static bool PersonaChangeHasFlag(EPersonaChange value, EPersonaChange checkflag)
            {
                return (value & checkflag) == checkflag;
            }

            public static bool PersonaChangeHasAllFlags(EPersonaChange value, params EPersonaChange[] checkflags)
            {
                foreach (var checkflag in checkflags)
                {
                    if ((value & checkflag) != checkflag)
                        return false;
                }
                return true;
            }
        }

        public enum EFriendFlags
        {
            None = 0,
            Blocked = 1,
            FriendshipRequested = 2,
            Immediate = 4,
            ClanMember = 8,
            OnGameServer = 16,
            RequestingFriendship = 128,
            RequestingInfo = 256,
            Ignored = 512,
            IgnoredFriend = 1024,
            ChatMember = 4096,
            All = 65535
        }

        public enum EPersonaChange
        {
            Name = 1,
            Status = 2,
            ComeOnline = 4,
            GoneOffline = 8,
            GamePlayed = 16,
            GameServer = 32,
            Avatar = 64,
            JoinedSource = 128,
            LeftSource = 256,
            RelationshipChanged = 512,
            NameFirstSet = 1024,
            Broadcast = 2048,
            Nickname = 4096,
            SteamLevel = 8192,
            RichPresence = 16384
        }
#endif
    }
}
#endif