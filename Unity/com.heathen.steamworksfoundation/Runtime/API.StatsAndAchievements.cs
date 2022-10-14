#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration.API
{

    /// <summary>
    /// Provides functions for accessing and submitting stats, achievements, and leaderboards.
    /// </summary>
    public static class StatsAndAchievements
    {
        /// <summary>
        /// Provides functions for accessing and submitting stats, achievements, and leaderboards.
        /// </summary>
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                pendingLinks = new List<ImageRequestCallbackLink>();

                if(loadedImages != null)
                {
                    foreach(var kvp in loadedImages)
                    {
                        var target = kvp.Value;
                        GameObject.Destroy(target);
                    }
                }

                loadedImages = new Dictionary<int, Texture2D>();
                eventUserStatsReceived = new UserStatsReceivedEvent();
                eventUserStatsUnloaded = new UserStatsUnloadedEvent();
                eventUserStatsStored = new UserStatsStoredEvent();
                eventUserAchievementStored = new UserAchievementStoredEvent();

                m_UserAchievementIconFetched_t = null;
                m_UserStatsReceived_t = null;
                m_UserStatsUnload_t = null;
                m_UserAchievementStored_t = null;
                m_UserStatsStored_t = null;
                m_NumberOfCurrentPlayers_t = null;
                m_GlobalAchievementPercentagesReady_t = null;
                m_GlobalStatsReceived_t = null;
                m_UserStatsReceived_t2 = null;
            }

            private class ImageRequestCallbackLink
            {
                public bool isAchievement;
                public string apiName;
                public Action<Texture2D> callback;
            }

            /// <summary>
            /// Called when the latest stats and achievements for a specific user (including the local user) have been received from the server.
            /// </summary>
            public static UserStatsReceivedEvent EventUserStatsReceived
            {
                get
                {
                    if (m_UserStatsReceived_t == null)
                        m_UserStatsReceived_t = Callback<UserStatsReceived_t>.Create(eventUserStatsReceived.Invoke);

                    return eventUserStatsReceived;
                }
            }
            /// <summary>
            /// Callback indicating that a user's stats have been unloaded.
            /// </summary>
            public static UserStatsUnloadedEvent EventUserStatsUnloaded
            {
                get
                {
                    if (m_UserStatsUnload_t == null)
                        m_UserStatsUnload_t = Callback<UserStatsUnloaded_t>.Create(eventUserStatsUnloaded.Invoke);

                    return eventUserStatsUnloaded;
                }
            }

            public static UserStatsStoredEvent EventUserStatsStored
            {
                get
                {
                    if (m_UserStatsStored_t == null)
                        m_UserStatsStored_t = Callback<UserStatsStored_t>.Create(eventUserStatsStored.Invoke);

                    return eventUserStatsStored;
                }
            }

            public static UserAchievementStoredEvent EventUserAchievementStored
            {
                get
                {
                    if (m_UserAchievementStored_t == null)
                        m_UserAchievementStored_t = Callback<UserAchievementStored_t>.Create(eventUserAchievementStored.Invoke);

                    return eventUserAchievementStored;
                }
            }

            private static List<ImageRequestCallbackLink> pendingLinks = new List<ImageRequestCallbackLink>();
            private static Dictionary<int, Texture2D> loadedImages = new Dictionary<int, Texture2D>();
            private static UserStatsReceivedEvent eventUserStatsReceived = new UserStatsReceivedEvent();
            private static UserStatsUnloadedEvent eventUserStatsUnloaded = new UserStatsUnloadedEvent();
            private static UserStatsStoredEvent eventUserStatsStored = new UserStatsStoredEvent();
            private static UserAchievementStoredEvent eventUserAchievementStored = new UserAchievementStoredEvent();

            private static Callback<UserAchievementIconFetched_t> m_UserAchievementIconFetched_t;
            private static Callback<UserStatsReceived_t> m_UserStatsReceived_t;
            private static Callback<UserStatsUnloaded_t> m_UserStatsUnload_t;
            private static Callback<UserAchievementStored_t> m_UserAchievementStored_t;
            private static Callback<UserStatsStored_t> m_UserStatsStored_t;

            private static CallResult<NumberOfCurrentPlayers_t> m_NumberOfCurrentPlayers_t;
            private static CallResult<GlobalAchievementPercentagesReady_t> m_GlobalAchievementPercentagesReady_t;
            private static CallResult<GlobalStatsReceived_t> m_GlobalStatsReceived_t;
            private static CallResult<UserStatsReceived_t> m_UserStatsReceived_t2;

            /// <summary>
            /// Resets the unlock status of an achievement.
            /// </summary>
            /// <param name="achievementApiName"></param>
            /// <returns></returns>
            public static bool ClearAchievement(string achievementApiName) => SteamUserStats.ClearAchievement(achievementApiName);
            /// <summary>
            /// Gets the unlock status of the Achievement.
            /// </summary>
            /// <param name="achievementApiName">The 'API Name' of the achievement.</param>
            /// <param name="achieved">Returns the unlock status of the achievement.</param>
            /// <returns></returns>
            public static bool GetAchievement(string achievementApiName, out bool achieved) => SteamUserStats.GetAchievement(achievementApiName, out achieved);
            /// <summary>
            /// Returns the percentage of users who have unlocked the specified achievement.
            /// </summary>
            /// <param name="achievementApiName">The 'API Name' of the achievement.</param>
            /// <param name="achieved">Returns whether the current user has unlocked the achievement.</param>
            /// <param name="unlockTime">Returns the time that the achievement was unlocked;</param>
            /// <returns></returns>
            public static bool GetAchievement(string achievementApiName, out bool achieved, out DateTime unlockTime)
            {
                var result = SteamUserStats.GetAchievementAndUnlockTime(achievementApiName, out achieved, out uint epoch);
                unlockTime = new DateTime(1970, 1, 1).AddSeconds(epoch);
                return result;
            }
            /// <summary>
            /// Gets the unlock status of the Achievement.
            /// </summary>
            /// <param name="userId">The Steam ID of the user to get the achievement for.</param>
            /// <param name="achievementApiName">The 'API Name' of the achievement.</param>
            /// <param name="achieved">Returns the unlock status of the achievement.</param>
            /// <returns></returns>
            public static bool GetAchievement(CSteamID userId, string achievementApiName, out bool achieved) => SteamUserStats.GetUserAchievement(userId, achievementApiName, out achieved);
            /// <summary>
            /// Gets the achievement status, and the time it was unlocked if unlocked.
            /// </summary>
            /// <param name="userId"></param>
            /// <param name="achievementApiName">The 'API Name' of the achievement.</param>
            /// <param name="achieved">Returns the unlock status of the achievement.</param>
            /// <param name="unlockTime">Returns the time that the achievement was unlocked.</param>
            /// <returns></returns>
            public static bool GetAchievement(CSteamID userId, string achievementApiName, out bool achieved, out DateTime unlockTime)
            {
                var result = SteamUserStats.GetUserAchievementAndUnlockTime(userId, achievementApiName, out achieved, out uint epoch);
                unlockTime = new DateTime(1970, 1, 1).AddSeconds(epoch);
                return result;
            }
            /// <summary>
            /// Returns the percentage of users who have unlocked the specified achievement.
            /// </summary>
            /// <param name="achievementApiName">The 'API Name' of the achievement.</param>
            /// <param name="percent">Variable to return the percentage of people that have unlocked this achievement from 0 to 100.</param>
            /// <returns></returns>
            public static bool GetAchievementAchievedPercent(string achievementApiName, out float percent) => SteamUserStats.GetAchievementAchievedPercent(achievementApiName, out percent);
            /// <summary>
            /// Get general attributes for an achievement. Currently provides: Name, Description, and Hidden status.
            /// </summary>
            /// <param name="achievementApiName">The 'API Name' of the achievement.</param>
            /// <param name="key">The 'key' to get a value for.</param>
            /// <returns></returns>
            public static string GetAchievementDisplayAttribute(string achievementApiName, string key) => SteamUserStats.GetAchievementDisplayAttribute(achievementApiName, key);
            /// <summary>
            /// Get general attributes for an achievement. Currently provides: Name, Description, and Hidden status.
            /// </summary>
            /// <param name="achievementApiName">The 'API Name' of the achievement.</param>
            /// <param name="attribute">The 'attribute' to get a value for.</param>
            /// <returns></returns>
            public static string GetAchievementDisplayAttribute(string achievementApiName, AchievementAttributes attribute) => SteamUserStats.GetAchievementDisplayAttribute(achievementApiName, attribute.ToString());
            /// <summary>
            /// Gets the icon for an achievement.
            /// </summary>
            /// <param name="achievementApiName"></param>
            /// <param name="callback"></param>
            public static void GetAchievementIcon(string achievementApiName, Action<Texture2D> callback)
            {
                if (callback == null)
                    return;

                if (m_UserAchievementIconFetched_t == null)
                    m_UserAchievementIconFetched_t = Callback<UserAchievementIconFetched_t>.Create(HandleIconImageLoaded);

                var handle = SteamUserStats.GetAchievementIcon(achievementApiName);
                if (handle > 0)
                {
                    if (loadedImages.ContainsKey(handle))
                        callback.Invoke(loadedImages[handle]);
                    else
                    {
                        if (LoadImage(handle))
                            callback.Invoke(loadedImages[handle]);
                        else
                        {
                            Debug.LogWarning("Failed to load the requested avatar");
                            callback.Invoke(null);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("No avatar available for this user");
                    pendingLinks.Add(new ImageRequestCallbackLink
                    {
                        isAchievement = true,
                        apiName = achievementApiName,
                        callback = callback
                    });
                }
            }
            /// <summary>
            /// Gets the 'API name' for an achievement index between 0 and GetNumAchievements.
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public static string GetAchievementName(uint index) => SteamUserStats.GetAchievementName(index);
            /// <summary>
            /// Gets a list of all achievement names registered to the app
            /// </summary>
            /// <returns></returns>
            public static string[] GetAchievementNames()
            {
                var count = SteamUserStats.GetNumAchievements();
                var results = new string[count];
                for (int i = 0; i < count; i++)
                {
                    results[i] = SteamUserStats.GetAchievementName((uint)i);
                }
                return results;
            }
            
            /// <summary>
            /// Gets the lifetime totals for an aggregated stat.
            /// </summary>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than k_cchStatNameMax.</param>
            /// <param name="data">The variable to return the stat value into.</param>
            /// <returns></returns>
            public static bool GetGlobalStat(string statApiName, out long data) => SteamUserStats.GetGlobalStat(statApiName, out data);
            /// <summary>
            /// Gets the lifetime totals for an aggregated stat.
            /// </summary>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than k_cchStatNameMax.</param>
            /// <param name="data">The variable to return the stat value into.</param>
            /// <returns></returns>
            public static bool GetGlobalStat(string statApiName, out double data) => SteamUserStats.GetGlobalStat(statApiName, out data);
            /// <summary>
            /// Gets the info on the most achieved achievement for the game.
            /// </summary>
            /// <param name="achievementApiName"></param>
            /// <param name="percent"></param>
            /// <param name="achieved"></param>
            /// <returns>Returns -1 if RequestGlobalAchievementPercentages has not been called or if there are no global achievement percentages for this app Id. If the call is successful it returns an iterator which should be used with GetNextMostAchievedAchievementInfo.</returns>
            public static int GetMostAchievedAchievementInfo(out string achievementApiName, out float percent, out bool achieved) => SteamUserStats.GetMostAchievedAchievementInfo(out achievementApiName, 8193, out percent, out achieved);
            /// <summary>
            /// Gets the info on the next most achieved achievement for the game.
            /// </summary>
            /// <remarks>
            /// You must have called RequestGlobalAchievementPercentages and it needs to return successfully via its callback prior to calling this.
            /// </remarks>
            /// <param name="previousIndex">Iterator returned from the previous call to this function or from GetMostAchievedAchievementInfo</param>
            /// <param name="achievementApiName">String buffer to return the 'API Name' of the achievement into.</param>
            /// <param name="percent">Variable to return the percentage of people that have unlocked this achievement from 0 to 100.</param>
            /// <param name="achieved">Variable to return whether the current user has unlocked this achievement.</param>
            /// <returns></returns>
            public static int GetNextMostAchievedAchievementInfo(int previousIndex, out string achievementApiName, out float percent, out bool achieved) => SteamUserStats.GetNextMostAchievedAchievementInfo(previousIndex, out achievementApiName, 8193, out percent, out achieved);
            /// <summary>
            /// Get the number of achievements defined in the App Admin panel of the Steamworks website.
            /// </summary>
            /// <returns></returns>
            public static uint GetNumAchievements() => SteamUserStats.GetNumAchievements();
            /// <summary>
            /// Asynchronously retrieves the total number of players currently playing the current game. Both online and in offline mode.
            /// </summary>
            /// <param name="callback"></param>
            public static void GetNumberOfCurrentPlayers(Action<NumberOfCurrentPlayers_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_NumberOfCurrentPlayers_t == null)
                    m_NumberOfCurrentPlayers_t = CallResult<NumberOfCurrentPlayers_t>.Create();

                var handle = SteamUserStats.GetNumberOfCurrentPlayers();
                m_NumberOfCurrentPlayers_t.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Gets the current value of the a stat for the current user.
            /// </summary>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than k_cchStatNameMax.</param>
            /// <param name="data">The variable to return the stat value into.</param>
            /// <returns></returns>
            public static bool GetStat(string statApiName, out int data) => SteamUserStats.GetStat(statApiName, out data);
            /// <summary>
            /// Gets the current value of the a stat for the current user.
            /// </summary>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than k_cchStatNameMax.</param>
            /// <param name="data">The variable to return the stat value into.</param>
            /// <returns></returns>
            public static bool GetStat(string statApiName, out float data) => SteamUserStats.GetStat(statApiName, out data);
            /// <summary>
            /// Gets the current value of the a stat for the specified user.
            /// </summary>
            /// <param name="userId">The Steam ID of the user to get the stat for.</param>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than k_cchStatNameMax.</param>
            /// <param name="data">The variable to return the stat value into.</param>
            /// <returns></returns>
            public static bool GetStat(CSteamID userId, string statApiName, out int data) => SteamUserStats.GetUserStat(userId, statApiName, out data);
            /// <summary>
            /// Gets the current value of the a stat for the specified user.
            /// </summary>
            /// <param name="userId">The Steam ID of the user to get the stat for.</param>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than k_cchStatNameMax.</param>
            /// <param name="data">The variable to return the stat value into.</param>
            /// <returns></returns>
            public static bool GetStat(CSteamID userId, string statApiName, out float data) => SteamUserStats.GetUserStat(userId, statApiName, out data);
            /// <summary>
            /// Shows the user a pop-up notification with the current progress of an achievement.
            /// </summary>
            /// <remarks>
            /// Calling this function will NOT set the progress or unlock the achievement, the game must do that manually by calling SetStat!
            /// </remarks>
            /// <param name="achievementApiName">The 'API Name' of the achievement.</param>
            /// <param name="progress">The current progress.</param>
            /// <param name="maxProgress">The progress required to unlock the achievement.</param>
            /// <returns></returns>
            public static bool IndicateAchievementProgress(string achievementApiName, uint progress, uint maxProgress) => SteamUserStats.IndicateAchievementProgress(achievementApiName, progress, maxProgress);
            /// <summary>
            /// Asynchronously request the user's current stats and achievements from the server.
            /// </summary>
            /// <remarks>
            /// You must always call this first to get the initial status of stats and achievements. Only after the resulting callback comes back can you start calling the rest of the stats and achievement functions for the current user.
            /// </remarks>
            /// <returns></returns>
            public static bool RequestCurrentStats() => SteamUserStats.RequestCurrentStats();
            /// <summary>
            /// Asynchronously fetch the data for the percentage of players who have received each achievement for the current game globally.
            /// </summary>
            /// <remarks>
            /// You must have called RequestCurrentStats and it needs to return successfully via its callback prior to calling this!
            /// </remarks>
            public static void RequestGlobalAchievementPercentages(Action<GlobalAchievementPercentagesReady_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_GlobalAchievementPercentagesReady_t == null)
                    m_GlobalAchievementPercentagesReady_t = CallResult<GlobalAchievementPercentagesReady_t>.Create();

                var handle = SteamUserStats.RequestGlobalAchievementPercentages();
                m_GlobalAchievementPercentagesReady_t.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Asynchronously fetches global stats data, which is available for stats marked as "aggregated" in the App Admin panel of the Steamworks website.
            /// </summary>
            /// <param name="historyDays">How many days of day-by-day history to retrieve in addition to the overall totals. The limit is 60.</param>
            /// <param name="callback"></param>
            public static void RequestGlobalStats(int historyDays, Action<GlobalStatsReceived_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_GlobalStatsReceived_t == null)
                    m_GlobalStatsReceived_t = CallResult<GlobalStatsReceived_t>.Create();

                var handle = SteamUserStats.RequestGlobalStats(historyDays);
                m_GlobalStatsReceived_t.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Asynchronously downloads stats and achievements for the specified user from the server.
            /// </summary>
            /// <remarks>
            /// To keep from using too much memory, an least recently used cache (LRU) is maintained and other user's stats will occasionally be unloaded. When this happens a UserStatsUnloaded_t callback is sent. After receiving this callback the user's stats will be unavailable until this function is called again.
            /// </remarks>
            /// <param name="userId"></param>
            /// <param name="callback"></param>
            public static void RequestUserStats(CSteamID userId, Action<UserStatsReceived_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_UserStatsReceived_t2 == null)
                    m_UserStatsReceived_t2 = CallResult<UserStatsReceived_t>.Create();

                var handle = SteamUserStats.RequestUserStats(userId);
                m_UserStatsReceived_t2.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Resets the current users stats and, optionally achievements.
            /// </summary>
            /// <remarks>
            /// This automatically calls StoreStats to persist the changes to the server. This should typically only be used for testing purposes during development. Ensure that you sync up your stats with the new default values provided by Steam after calling this by calling RequestCurrentStats.
            /// </remarks>
            /// <param name="achievementsToo"></param>
            /// <returns></returns>
            public static bool ResetAllStats(bool achievementsToo) => SteamUserStats.ResetAllStats(achievementsToo);
            /// <summary>
            /// Unlocks an achievement.
            /// </summary>
            /// <param name="achievementApiName"></param>
            /// <returns></returns>
            public static bool SetAchievement(string achievementApiName) => SteamUserStats.SetAchievement(achievementApiName);
            /// <summary>
            /// Sets / updates the value of a given stat for the current user.
            /// </summary>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than k_cchStatNameMax.</param>
            /// <param name="data">The new value of the stat. This must be an absolute value, it will not increment or decrement for you.</param>
            /// <returns></returns>
            public static bool SetStat(string statApiName, int data) => SteamUserStats.SetStat(statApiName, data);
            /// <summary>
            /// Sets / updates the value of a given stat for the current user.
            /// </summary>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than k_cchStatNameMax.</param>
            /// <param name="data">The new value of the stat. This must be an absolute value, it will not increment or decrement for you.</param>
            /// <returns></returns>
            public static bool SetStat(string statApiName, float data) => SteamUserStats.SetStat(statApiName, data);
            /// <summary>
            /// Send the changed stats and achievements data to the server for permanent storage.
            /// </summary>
            /// <remarks>
            /// If this fails then nothing is sent to the server. It's advisable to keep trying until the call is successful. This call can be rate limited.Call frequency should be on the order of minutes, rather than seconds.You should only be calling this during major state changes such as the end of a round, the map changing, or the user leaving a server. This call is required to display the achievement unlock notification dialog though, so if you have called SetAchievement then it's advisable to call this soon after that.
            /// </remarks>
            /// <returns></returns>
            public static bool StoreStats() => SteamUserStats.StoreStats();
            /// <summary>
            /// Updates an AVGRATE stat with new values.
            /// </summary>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than k_cchStatNameMax.</param>
            /// <param name="countThisSession">The value accumulation since the last call to this function.</param>
            /// <param name="sessionLength">The amount of time in seconds since the last call to this function.</param>
            /// <returns></returns>
            public static bool UpdateAvgRateStat(string statApiName, float countThisSession, double sessionLength) => SteamUserStats.UpdateAvgRateStat(statApiName, countThisSession, sessionLength);
            
            private static void HandleIconImageLoaded(UserAchievementIconFetched_t param)
            {
                if (LoadImage(param.m_nIconHandle))
                {
                    var target = loadedImages[param.m_nIconHandle];
                    var apiName = param.m_rgchAchievementName;
                    foreach (var link in pendingLinks)
                    {
                        if (link.isAchievement && link.apiName == apiName)
                            link.callback?.Invoke(target);
                    }

                    pendingLinks.RemoveAll(p => p.isAchievement && p.apiName == apiName);
                }
                else
                {
                    var apiName = param.m_rgchAchievementName;
                    foreach (var link in pendingLinks)
                    {
                        if (link.isAchievement && link.apiName == apiName)
                            link.callback?.Invoke(null);
                    }

                    pendingLinks.RemoveAll(p => p.isAchievement && p.apiName == apiName);
                }
            }
            private static bool LoadImage(int imageHandle)
            {
                if (SteamUtils.GetImageSize(imageHandle, out uint width, out uint height))
                {
                    Texture2D pointer = null;

                    if (loadedImages.ContainsKey(imageHandle))
                        pointer = loadedImages[imageHandle];

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

                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Functions to allow game servers to set stats and achievements on players.
        /// </summary>
        public static class Server
        {
            private static CallResult<GSStatsReceived_t> m_GSStatsReceived_t;
            private static CallResult<GSStatsStored_t> m_GSStatsStored_t;

            /// <summary>
            /// Resets the unlock status of an achievement for the specified user.
            /// </summary>
            /// <remarks>
            /// You must have called RequestUserStats and it needs to return successfully via its callback prior to calling this!
            /// </remarks>
            /// <param name="userId">The Steam ID of the user to clear the achievement for.</param>
            /// <param name="achievementApiName">The 'API Name' of the Achievement to reset.</param>
            /// <returns>
            /// <para>
            /// This function returns true upon success if all of the following conditions are met; otherwise, false.
            /// </para>
            /// <list type="bullet">
            /// <item>The specified achievement "API Name" exists in App Admin on the Steamworks website, and the changes are published.</item>
            /// <item>RequestUserStats has completed and successfully returned its callback for the specified user.</item>
            /// <item>The stat must be allowed to be set by game server.</item>
            /// </list>
            /// </returns>
            public static bool ClearUserAchievement(CSteamID userId, string achievementApiName) => SteamGameServerStats.ClearUserAchievement(userId, achievementApiName);
            /// <summary>
            /// Gets the unlock status of the Achievement.
            /// </summary>
            /// <param name="userId">The Steam ID of the user to get the achievement for.</param>
            /// <param name="achievementApiName">The 'API Name' of the achievement.</param>
            /// <param name="achieved">Returns the unlock status of the achievement.</param>
            /// <returns>
            /// <para>This function returns true upon success if all of the following conditions are met; otherwise, false.</para>
            /// <list type="bullet">
            /// <item>RequestUserStats has completed and successfully returned its callback.</item>
            /// <item>The 'API Name' of the specified achievement exists in App Admin on the Steamworks website, and the changes are published.</item>
            /// </list>
            /// </returns>
            public static bool GetUserAchievement(CSteamID userId, string achievementApiName, out bool achieved) => SteamGameServerStats.GetUserAchievement(userId, achievementApiName, out achieved);
            /// <summary>
            /// Gets the current value of the a stat for the specified user.
            /// </summary>
            /// <remarks>
            /// You must have called RequestUserStats and it needs to return successfully via its callback prior to calling this.
            /// </remarks>
            /// <param name="userId">The Steam ID of the user to get the stat for.</param>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than <see cref="Constants.k_cchStatNameMax"/>.</param>
            /// <param name="data">The variable to return the stat value into.</param>
            /// <returns>
            /// This function returns true upon success if all of the following conditions are met; otherwise, false.
            /// <list type="bullet">
            /// <item>The specified stat exists in App Admin on the Steamworks website, and the changes are published.</item>
            /// <item>RequestUserStats has completed and successfully returned its callback.</item>
            /// <item>The type passed to this function must match the type listed in the App Admin panel of the Steamworks website.</item>
            /// </list>
            /// </returns>
            public static bool GetUserStat(CSteamID userId, string statApiName, out int data) => SteamGameServerStats.GetUserStat(userId, statApiName, out data);
            /// <summary>
            /// Gets the current value of the a stat for the specified user.
            /// </summary>
            /// <remarks>
            /// You must have called RequestUserStats and it needs to return successfully via its callback prior to calling this.
            /// </remarks>
            /// <param name="userId">The Steam ID of the user to get the stat for.</param>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than <see cref="Constants.k_cchStatNameMax"/>.</param>
            /// <param name="data">The variable to return the stat value into.</param>
            /// <returns>
            /// This function returns true upon success if all of the following conditions are met; otherwise, false.
            /// <list type="bullet">
            /// <item>The specified stat exists in App Admin on the Steamworks website, and the changes are published.</item>
            /// <item>RequestUserStats has completed and successfully returned its callback.</item>
            /// <item>The type passed to this function must match the type listed in the App Admin panel of the Steamworks website.</item>
            /// </list>
            /// </returns>
            public static bool GetUserStat(CSteamID userId, string statApiName, out float data) => SteamGameServerStats.GetUserStat(userId, statApiName, out data);
            /// <summary>
            /// Asynchronously downloads stats and achievements for the specified user from the server.
            /// </summary>
            /// <remarks>
            /// These stats will only be auto-updated for clients currently playing on the server. For other users you'll need to call this function again to refresh any data.
            /// </remarks>
            /// <param name="userId"></param>
            /// <param name="callback"></param>
            public static void RequestUserStats(CSteamID userId, Action<GSStatsReceived_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_GSStatsReceived_t == null)
                    m_GSStatsReceived_t = CallResult<GSStatsReceived_t>.Create();

                var handle = SteamGameServerStats.RequestUserStats(userId);
                m_GSStatsReceived_t.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Unlocks an achievement for the specified user.
            /// </summary>
            /// <remarks>
            /// You must have called RequestUserStats and it needs to return successfully via its callback prior to calling this!
            /// <para>
            /// This call only modifies Steam's in-memory state and is very cheap. To submit the stats to the server you must call StoreUserStats.
            /// </para>
            /// <para>
            /// NOTE: This will work only on achievements that game servers are allowed to set. If the "Set By" field for this achievement is "Official GS" then only game servers that have been declared as officially controlled by you will be able to set it. To do this you must set the IP range of your official servers in the Dedicated Servers section of App Admin.
            /// </para>
            /// </remarks>
            /// <param name="userId">The Steam ID of the user to unlock the achievement for.</param>
            /// <param name="achievementApiName">The 'API Name' of the Achievement to unlock.</param>
            /// <returns>
            /// This function returns true upon success if all of the following conditions are met; otherwise, false.
            /// <list type="bullet">
            /// <item>The specified achievement "API Name" exists in App Admin on the Steamworks website, and the changes are published.</item>
            /// <item>RequestUserStats has completed and successfully returned its callback for the specified user.</item>
            /// <item>The stat must be allowed to be set by game server.</item>
            /// </list>
            /// </returns>
            public static bool SetUserAchievement(CSteamID userId, string achievementApiName) => SteamGameServerStats.SetUserAchievement(userId, achievementApiName);
            /// <summary>
            /// Sets / updates the value of a given stat for the specified user.
            /// </summary>
            /// <remarks>
            /// You must have called RequestUserStats and it needs to return successfully via its callback prior to calling this!
            /// <para>
            /// This call only modifies Steam's in-memory state and is very cheap. To submit the stats to the server you must call StoreUserStats.
            /// </para>
            /// <para>
            /// NOTE: These updates will work only on stats that game servers are allowed to edit. If the "Set By" field for this stat is "Official GS" then only game servers that have been declared as officially controlled by you will be able to set it. To do this you must set the IP range of your official servers in the Dedicated Servers section of App Admin.
            /// </para>
            /// </remarks>
            /// <param name="userId">The Steam ID of the user to set the stat on.</param>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than <see cref="Constants.k_cchStatNameMax"/>.</param>
            /// <param name="data">The new value of the stat. This must be an absolute value, it will not increment or decrement for you.</param>
            /// <returns>
            /// This function returns true upon success if all of the following conditions are met; otherwise, false.
            /// <list type="bullet">
            /// <item>The specified stat exists in App Admin on the Steamworks website, and the changes are published.</item>
            /// <item>RequestUserStats has completed and successfully returned its callback for the specified user.</item>
            /// <item>The type passed to this function must match the type listed in the App Admin panel of the Steamworks website.</item>
            /// <item>The stat must be allowed to be set by game server.</item>
            /// </list>
            /// </returns>
            public static bool SetUserStat(CSteamID userId, string statApiName, int data) => SteamGameServerStats.SetUserStat(userId, statApiName, data);
            /// <summary>
            /// Sets / updates the value of a given stat for the specified user.
            /// </summary>
            /// <remarks>
            /// You must have called RequestUserStats and it needs to return successfully via its callback prior to calling this!
            /// <para>
            /// This call only modifies Steam's in-memory state and is very cheap. To submit the stats to the server you must call StoreUserStats.
            /// </para>
            /// <para>
            /// NOTE: These updates will work only on stats that game servers are allowed to edit. If the "Set By" field for this stat is "Official GS" then only game servers that have been declared as officially controlled by you will be able to set it. To do this you must set the IP range of your official servers in the Dedicated Servers section of App Admin.
            /// </para>
            /// </remarks>
            /// <param name="userId">The Steam ID of the user to set the stat on.</param>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than <see cref="Constants.k_cchStatNameMax"/>.</param>
            /// <param name="data">The new value of the stat. This must be an absolute value, it will not increment or decrement for you.</param>
            /// <returns>
            /// This function returns true upon success if all of the following conditions are met; otherwise, false.
            /// <list type="bullet">
            /// <item>The specified stat exists in App Admin on the Steamworks website, and the changes are published.</item>
            /// <item>RequestUserStats has completed and successfully returned its callback for the specified user.</item>
            /// <item>The type passed to this function must match the type listed in the App Admin panel of the Steamworks website.</item>
            /// <item>The stat must be allowed to be set by game server.</item>
            /// </list>
            /// </returns>
            public static bool SetUserStat(CSteamID userId, string statApiName, float data) => SteamGameServerStats.SetUserStat(userId, statApiName, data);
            /// <summary>
            /// Send the changed stats and achievements data to the server for permanent storage for the specified user.
            /// </summary>
            /// <remarks>
            /// <para>
            /// If this fails then nothing is sent to the server. It's advisable to keep trying until the call is successful.
            /// </para>
            /// <para>
            /// If you have stats or achievements that you have saved locally but haven't uploaded with this function when your application process ends then this function will automatically be called.
            /// </para>
            /// <para>
            /// If m_eResult has a result of k_EResultInvalidParam, then one or more stats uploaded has been rejected, either because they broke constraints or were out of date. In this case the server sends back updated values and the stats should be updated locally to keep in sync.
            /// </para>
            /// </remarks>
            /// <param name="userId"></param>
            /// <param name="callback"></param>
            public static void StoreUserStats(CSteamID userId, Action<GSStatsStored_t, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_GSStatsStored_t == null)
                    m_GSStatsStored_t = CallResult<GSStatsStored_t>.Create();

                var handle = SteamGameServerStats.StoreUserStats(userId);
                m_GSStatsStored_t.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Updates an AVGRATE stat with new values for the specified user.
            /// </summary>
            /// <param name="userId">The Steam ID of the user to update the AVGRATE stat for.</param>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than <see cref="Constants.k_cchStatNameMax"/>.</param>
            /// <param name="count">The value accumulation since the last call to this function.</param>
            /// <param name="length">The amount of time in seconds since the last call to this function.</param>
            /// <returns></returns>
            public static bool UpdateUserAvgRateStat(CSteamID userId, string statApiName, float count, double length) => SteamGameServerStats.UpdateUserAvgRateStat(userId, statApiName, count, length);
        }
    }

}
#endif