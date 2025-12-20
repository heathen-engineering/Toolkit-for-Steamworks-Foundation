#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// A <see cref="ScriptableObject"/> containing the definition of a Steamworks Stat.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that this object simply contains the definition of a stat that has been created in the Steamworks API.
    /// for more information please see <a href="https://partner.steamgames.com/doc/features/achievements">https://partner.steamgames.com/doc/features/achievements</a>
    /// </para>
    /// </remarks>
    [HelpURL("https://kb.heathenengineering.com/assets/steamworks/stats-object")]
    [Serializable]
    public abstract class StatObject : ScriptableObject
    {
        /// <summary>
        /// The name of the stat as it appears in the Steamworks Portal
        /// </summary>
        [HideInInspector]
        public StatData data;
        /// <summary>
        /// Indicates the data type of this stat.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        public abstract DataType Type { get; }
        /// <summary>
        /// Returns the value of this stat as an int.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <returns></returns>
        public int GetIntValue() => data.IntValue();
        /// <summary>
        /// Returns the value of this stat as a float.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <returns></returns>
        public float GetFloatValue() => data.FloatValue();
        /// <summary>
        /// Asynchronously downloads stats and achievements for the specified user from the server.
        /// </summary>
        /// <remarks>
        /// To keep from using too much memory, an least recently used cache (LRU) is maintained and other user's stats will occasionally be unloaded. When this happens a UserStatsUnloaded_t callback is sent. After receiving this callback the user's stats will be unavailable until this function is called again.
        /// </remarks>
        /// <param name="userId"></param>
        /// <param name="callback"></param>
        public void RequestUserStats(UserData user, Action<UserStatsReceived, bool> callback) => data.RequestUserStats(user, callback);   
        /// <summary>
        /// Read the value of this state for a specific user
        /// <para>
        /// You must have called <see cref="RequestUserStats(UserData, Action{UserStatsReceived_t, bool})"/> first
        /// </para>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool GetValue(UserData user, out int value) => data.GetValue(user, out value);
        /// <summary>
        /// Read the value of this state for a specific user
        /// <para>
        /// You must have called <see cref="RequestUserStats(UserData, Action{UserStatsReceived_t, bool})"/> first
        /// </para>
        /// </summary>
        /// <param name="user"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool GetValue(UserData user, out float value) => data.GetValue(user, out value);
        /// <summary>
        /// Sets the value of this stat on the Steamworks API.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <param name="value">The value to set on the API</param>
        public void SetIntStat(int value) => data.Set(value);
        /// <summary>
        /// Sets the value of this stat on the Steamworks API.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <param name="value">The value to set on the API</param>
        public void SetFloatStat(float value) => data.Set(value);
        /// <summary>
        /// Adds the provided value to the existing value of the stat
        /// </summary>
        /// <param name="value">The value to add to the current value</param>
        public void AddFloatStat(float value)
        {
            SetFloatStat(GetFloatValue() + value);
        }
        /// <summary>
        /// Adds the provided value to the existing value of the stat
        /// </summary>
        /// <param name="value">The value to add to the current value</param>
        public void AddIntStat(int value)
        {
            SetIntStat(GetIntValue() + value);
        }
        /// <summary>
        /// This stores all stats to the Valve backend servers it is not possible to store only 1 stat at a time
        /// Note that this will cause a callback from Steamworks which will cause the stats to update
        /// </summary>
        public void StoreStats() => data.Store();
        /// <summary>
        /// The available type of stat data used in the Steamworks API
        /// </summary>
        public enum DataType
        {
            Int,
            Float,
            AvgRate
        }

#if UNITY_SERVER || UNITY_EDITOR
        [Obsolete("Use ServerGetUserIntStat instead.")]
        public int GetUserIntStat(UserData user) => ServerGetUserIntStat(user);
        /// <summary>
        /// Get the int value of this stat for the <paramref name="user"/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only available on server builds
        /// </para>
        /// </remarks>
        /// <param name="user"></param>
        /// <returns></returns>
        public int ServerGetUserIntStat(UserData user)
        {
            int buffer;
            API.StatsAndAchievements.Server.GetUserStat(user, data, out buffer);
            return buffer;
        }
        [Obsolete("User ServerGetUserFloatStat instead.")]
        public float GetUserFloatStat(UserData user) => ServerGetUserFloatStat(user);
        /// <summary>
        /// Get the float value of this stat for the <paramref name="user"/>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only available on server builds
        /// </para>
        /// </remarks>
        /// <param name="user"></param>
        /// <returns></returns>
        public float ServerGetUserFloatStat(UserData user)
        {
            float buffer;
            API.StatsAndAchievements.Server.GetUserStat(user, data, out buffer);
            return buffer;
        }
        [Obsolete("Use ServerSetUserIntStat instead")]
        public void SetUserIntStat(UserData user, int value) => ServerSetUserIntStat(user, value);
        /// <summary>
        /// Sets a integer value for the user on this stat
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only available on server builds
        /// </para>
        /// </remarks>
        /// <param name="user"></param>
        /// <param name="value"></param>
        public void ServerSetUserIntStat(UserData user, int value)
        {
            API.StatsAndAchievements.Server.SetUserStat(user, data, value);
        }
        [Obsolete("Use ServerSetUserFloatStat instead")]
        public void SetUserFloatStat(UserData user, float value) => ServerSetUserFloatStat(user, value);
        /// <summary>
        /// Sets a float value for the user on this stat
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only available on server builds
        /// </para>
        /// </remarks>
        /// <param name="user"></param>
        /// <param name="value"></param>
        public void ServerSetUserFloatStat(UserData user, float value)
        {
            API.StatsAndAchievements.Server.SetUserStat(user, data, value);
        }
        [Obsolete("Use ServerUpdateUserAvgRateStat instead")]
        public void UpdateUserAvgRateStat(UserData user, float countThisSession, double sessionLength) => ServerUpdateUserAvgRateStat(user, countThisSession, sessionLength);   
        /// <summary>
        /// Updates the users average rate for this stat
        /// </summary>
        /// <remarks>
        /// <para>
        /// Only available on server builds
        /// </para>
        /// </remarks>
        /// <param name="user"></param>
        /// <param name="countThisSession"></param>
        /// <param name="sessionLength"></param>
        public void ServerUpdateUserAvgRateStat(UserData user, float countThisSession, double sessionLength)
        {
            API.StatsAndAchievements.Server.UpdateUserAvgRateStat(user, data, countThisSession, sessionLength);
        }
#endif
    }
}
#endif
