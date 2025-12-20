#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;
using System.Linq;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// Represents a Steam Achievement 
    /// </summary>
    [Serializable]
    public struct AchievementData : IEquatable<AchievementData>, IEquatable<string>, IComparable<AchievementData>, IComparable<string>
    {
        /// <summary>
        /// Returns the name of the achievement as seen by this user, this will depend on the user's language and the language configuration of the achievement
        /// </summary>
        public readonly string Name => API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(id, AchievementAttributes.name);
        /// <summary>
        /// Returns the description of the achievement as see by this user, this will depend on the user's language and the language configuration of the achievement
        /// </summary>
        public readonly string Description => API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(id, AchievementAttributes.desc);
        /// <summary>
        /// Returns the Is Hidden value of this achievement as seen by this user
        /// </summary>
        public readonly bool Hidden => API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(id, AchievementAttributes.hidden) == "1";

        [NonSerialized]
        private AchievementObject _so;
        /// <summary>
        /// The ScriptableObject representation of this Achievement if one exists and is registered to the active <see cref="SteamSettings"/> object
        /// </summary>
        public AchievementObject ScriptableObject
        {
            get
            {
                if (SteamSettings.current == null)
                    return null;

                if (_so == null)
                {
                    var nId = this;
                    _so = SteamSettings.Achievements.FirstOrDefault(p => p.Id == nId);
                }

                return _so;
            }
            set
            {
                _so = value;
                id = value.Id;
            }
        }
        /// <summary>
        /// The unique ID of the achievement, this is the API Name that has been set in the Steamworks portal.
        /// </summary>
        public readonly string ApiName => id;
        /// <summary>
        /// The API Name as it appears in the Steamworks portal.
        /// </summary>
        [SerializeField]
        private string id;

        /// <summary>
        /// Indicates that this achievement has been unlocked by this user.
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        public readonly bool IsAchieved
        {
            get
            {
                if (API.StatsAndAchievements.Client.GetAchievement(id, out bool status))
                    return status;
                else
                    return false;
            }
            set
            {
                if (value)
                    API.StatsAndAchievements.Client.SetAchievement(id);
                else
                    API.StatsAndAchievements.Client.ClearAchievement(id);
            }
        }
        /// <summary>
        /// Indicates the time the achievement was unlocked if at all
        /// </summary>
        public readonly DateTime? UnlockTime
        {
            get
            {
                if (API.StatsAndAchievements.Client.GetAchievement(id, out _, out DateTime time))
                    return time;
                else
                    return null;
            }
        }

        /// <summary>
        /// The percentage of users who have unlocked this achievement
        /// </summary>
        public readonly float GlobalPercent
        {
            get
            {
                API.StatsAndAchievements.Client.GetAchievementAchievedPercent(id, out var percent);
                return percent;
            }
        }

        /// <summary>
        /// <para>Unlocks the achievement.</para>
        /// </summary>
        public readonly void Unlock() => IsAchieved = true;

        /// <summary>
        /// <para>Resets the unlock status of an achievement.</para>
        /// </summary>
        public readonly void ClearAchievement() => IsAchieved = false;

        /// <summary>
        /// Unlock the achievement for the <paramref name="user"/>
        /// </summary>
        /// <remarks>
        /// Only available on server builds
        /// </remarks>
        /// <param name="user"></param>
        public readonly void Unlock(UserData user)
        {
            API.StatsAndAchievements.Server.SetUserAchievement(user, id);
        }

        /// <summary>
        /// Clears the achievement for the <paramref name="user"/>
        /// </summary>
        /// <remarks>
        /// Only available on server builds
        /// </remarks>
        /// <param name="user"></param>
        public readonly void ClearAchievement(UserData user)
        {
            API.StatsAndAchievements.Server.ClearUserAchievement(user, id);
        }

        /// <summary>
        /// Gets the achievement status for the <paramref name="user"/>
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public readonly bool GetAchievementStatus(UserData user)
        {
            bool achieved;
            API.StatsAndAchievements.Client.GetAchievement(user, id, out achieved);
            return achieved;
        }

        /// <summary>
        /// Get the unlock state and time for this achievement for a specific user.
        /// </summary>
        /// <param name="user">The user to check</param>
        /// <returns>(<see cref="bool"/> unlocked, <see cref="DataTime"/> unlockTime) indicating the state and time of the achievement for the indicated user if known.</returns>
        public readonly (bool unlocked, DateTime unlockTime) GetAchievementAndUnlockTime(UserData user)
        {
            API.StatsAndAchievements.Client.GetAchievement(user, id, out bool unlocked, out DateTime time);
            return (unlocked, time);
        }
        /// <summary>
        /// Gets the icon for this achievement as seen by the logged in user, this will return either the locked or unlocked icon depending on the state of the achievement for this user
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="Texture2D"/> result) that is invoked when the process completes</param>
        public readonly void GetIcon(Action<Texture2D> callback) => API.StatsAndAchievements.Client.GetAchievementIcon(id, callback);
        /// <summary>
        /// Request Steam client store the current state of all stats and achievements
        /// </summary>
        public readonly void Store() => API.StatsAndAchievements.Client.StoreStats();
        /// <summary>
        /// Get the achievement given the API name provided
        /// </summary>
        /// <param name="apiName">The API name of the achievement as entered into Steam Developer Portal</param>
        /// <returns>The AchievementData object represented by this name</returns>
        public static AchievementData Get(string apiName) => apiName;

        /// <summary>
        /// This will create a ScriptableObject based on this achievement ... in general you should not need this
        /// The AchievementData struct has all the same features of the ScriptableObject but is much lighter weight and
        /// more suitable for creation at runtime.
        /// </summary>
        /// <returns>Creates a new ScriptableObject of type <see cref="AchievementObject"/> and returns it</returns>
        public readonly AchievementObject CreateScriptableObject()
        {
            var newObject = UnityEngine.ScriptableObject.CreateInstance<AchievementObject>();
            newObject.Id = this;
            return newObject;
        }

        /// <summary>
        /// This will create a ScriptableObject ... in general you should not need this
        /// The AchievementData struct has all the same features of the ScriptableObject but is much lighter weight and
        /// more suitable for creation at runtime.
        /// </summary>
        /// <returns></returns>
        public static AchievementObject CreateScriptableObject(string apiName)
        {
            var newObject = UnityEngine.ScriptableObject.CreateInstance<AchievementObject>();
            newObject.Id = apiName;
            return newObject;
        }

        #region Boilerplate
        /// <summary>
        /// Returns the API Name of the achievement
        /// </summary>
        /// <returns></returns>
        public override readonly string ToString()
        {
            return id;
        }
        public readonly bool Equals(string other)
        {
            return id.Equals(other);
        }

        public readonly bool Equals(AchievementData other)
        {
            return id.Equals(other.id);
        }

        public override readonly bool Equals(object obj)
        {
            return id.Equals(obj);
        }

        public override readonly int GetHashCode()
        {
            return id.GetHashCode();
        }

        public readonly int CompareTo(AchievementData other)
        {
            return id.CompareTo(other.id);
        }

        public readonly int CompareTo(string other)
        {
            return id.CompareTo(other);
        }

        public static bool operator ==(AchievementData l, AchievementData r) => l.id == r.id;
        public static bool operator ==(string l, AchievementData r) => l == r.id;
        public static bool operator ==(AchievementData l, string r) => l.id == r;
        public static bool operator !=(AchievementData l, AchievementData r) => l.id != r.id;
        public static bool operator !=(string l, AchievementData r) => l != r.id;
        public static bool operator !=(AchievementData l, string r) => l.id != r;

        public static implicit operator string(AchievementData c) => c.id;
        public static implicit operator AchievementData(string id) => new AchievementData { id = id };
        #endregion
    }
}
#endif