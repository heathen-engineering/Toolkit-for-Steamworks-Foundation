#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// Represents a Steam Stat 
    /// </summary>
    [Serializable]
    public struct StatData : IEquatable<StatData>, IEquatable<string>, IComparable<StatData>, IComparable<string>
    {
        /// <summary>
        /// The API Name as it appears in the Steamworks portal.
        /// </summary>
        [SerializeField]
        private string id;
        /// <summary>
        /// The float value of the stat
        /// </summary>
        /// <returns></returns>
        public readonly float FloatValue()
        {
            API.StatsAndAchievements.Client.GetStat(id, out float value);
            return value;
        }
        /// <summary>
        /// The int value of the stat
        /// </summary>
        /// <returns></returns>
        public readonly int IntValue()
        {
            API.StatsAndAchievements.Client.GetStat(id, out int value);
            return value;
        }
        /// <summary>
        /// Asynchronously downloads stats and achievements for the specified user from the server.
        /// </summary>
        /// <remarks>
        /// To keep from using too much memory, an least recently used cache (LRU) is maintained and other user's stats will occasionally be unloaded. When this happens a UserStatsUnloaded_t callback is sent. After receiving this callback the user's stats will be unavailable until this function is called again.
        /// </remarks>
        /// <param name="user">The user to get stats for</param>
        /// <param name="callback">A delegate of the form (<see cref="UserStatsReceived"/> results, <see cref="bool"/> ioError) that is invoked when the process is completed</param>
        public readonly void RequestUserStats(UserData user, Action<UserStatsReceived, bool> callback) => API.StatsAndAchievements.Client.RequestUserStats(user, callback);
        /// <summary>
        /// Get the value of the stat for the given user, this assumes <see cref="RequestUserStats(UserData, Action{UserStatsReceived, bool})"/> has already been called
        /// </summary>
        /// <param name="user">The user to find the value for</param>
        /// <param name="value">The value</param>
        /// <returns>True if the request was accepted</returns>
        public readonly bool GetValue(UserData user, out int value) => API.StatsAndAchievements.Client.GetStat(user, this, out value);
        /// <summary>
        /// Get the value of the stat for the given user, this assumes <see cref="RequestUserStats(UserData, Action{UserStatsReceived, bool})"/> has already been called
        /// </summary>
        /// <param name="user">The user to find the value for</param>
        /// <param name="value">The value</param>
        /// <returns>True if the request was accepted</returns>
        public readonly bool GetValue(UserData user, out float value) => API.StatsAndAchievements.Client.GetStat(user, this, out value);
        /// <summary>
        /// Set the value of the stat
        /// <para>This sets the value in the local cash, and can be called as frequently as you like. When ready call <see cref="Store"/>, store should only be called periodically and is rate limited by Valve</para>
        /// </summary>
        /// <param name="value">The value to set</param>
        public readonly void Set(float value) => API.StatsAndAchievements.Client.SetStat(id, value);
        /// <summary>
        /// Set the value of the stat
        /// <para>This sets the value in the local cash, and can be called as frequently as you like. When ready call <see cref="Store"/>, store should only be called periodically and is rate limited by Valve</para>
        /// </summary>
        /// <param name="value">The value to set</param>
        public readonly void Set(int value) => API.StatsAndAchievements.Client.SetStat(id, value);
        /// <summary>
        /// Set the value of the stat
        /// <para>This sets the value in the local cash, and can be called as frequently as you like. When ready call <see cref="Store"/>, store should only be called periodically and is rate limited by Valve</para>
        /// </summary>
        /// <param name="value">The value to set</param>
        public readonly void Set(float value, double length) => API.StatsAndAchievements.Client.UpdateAvgRateStat(id, value, length);
        /// <summary>
        /// Store the value set to the Steam backend
        /// </summary>
        public readonly void Store() => API.StatsAndAchievements.Client.StoreStats();
        
        #region Boilerplate
        public readonly bool Equals(string other)
        {
            return id.Equals(other);
        }

        public readonly bool Equals(StatData other)
        {
            return id.Equals(other.id);
        }

        public readonly override bool Equals(object obj)
        {
            return id.Equals(obj);
        }

        public readonly override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public readonly int CompareTo(StatData other)
        {
            return id.CompareTo(other.id);
        }

        public readonly int CompareTo(string other)
        {
            return id.CompareTo(other);
        }

        public static bool operator ==(StatData l, StatData r) => l.id == r.id;
        public static bool operator ==(string l, StatData r) => l == r.id;
        public static bool operator ==(StatData l, string r) => l.id == r;
        public static bool operator !=(StatData l, StatData r) => l.id != r.id;
        public static bool operator !=(string l, StatData r) => l != r.id;
        public static bool operator !=(StatData l, string r) => l.id != r;

        public static implicit operator string(StatData c) => c.id;
        public static implicit operator StatData(string id) => new StatData { id = id };
        #endregion
    }
}
#endif