#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;
using System.Linq;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    [Serializable]
    public struct AchievementData : IEquatable<AchievementData>, IEquatable<string>, IComparable<AchievementData>, IComparable<string>
    {
        public string Name => API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(id, AchievementAttributes.name);

        public string Description => API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(id, AchievementAttributes.desc);

        public bool Hidden => API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(id, AchievementAttributes.hidden) == "1";

        [NonSerialized]
        private AchievementObject _so;
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
        /// The API Name as it appears in the Steamworks portal.
        /// </summary>
        [SerializeField]
        private string id;

        /// <summary>
        /// Indicates that this achievment has been unlocked by this user.
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        public bool IsAchieved
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
        public DateTime? UnlockTime
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
        public float GlobalPercent
        {
            get
            {
                API.StatsAndAchievements.Client.GetAchievementAchievedPercent(id, out var percent);
                return percent;
            }
        }

        /// <summary>
        /// <para>Unlocks the achievement.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#SetAchievement</a>
        /// </summary>
        public void Unlock() => IsAchieved = true;

        /// <summary>
        /// <para>Resets the unlock status of an achievmeent.</para>
        /// <a href="https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement">https://partner.steamgames.com/doc/api/ISteamUserStats#ClearAchievement</a>
        /// </summary>
        public void ClearAchievement() => IsAchieved = false;

        

        /// <summary>
        /// Unlock the achievement for the <paramref name="user"/>
        /// </summary>
        /// <remarks>
        /// Only available on server builds
        /// </remarks>
        /// <param name="user"></param>
        public void Unlock(UserData user)
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
        public void ClearAchievement(UserData user)
        {
            API.StatsAndAchievements.Server.ClearUserAchievement(user, id);
        }

        /// <summary>
        /// Gets the achievement status for the <paramref name="user"/>
        /// </summary>
        /// <remarks>
        /// Only available on server builds
        /// </remarks>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool GetAchievementStatus(UserData user)
        {
            bool achieved;
            API.StatsAndAchievements.Client.GetAchievement(user, id, out achieved);
            return achieved;
        }

        public (bool unlocked, DateTime unlockTime) GetAchievementAndUnlockTime(UserData user)
        {
            API.StatsAndAchievements.Client.GetAchievement(user, id, out bool unlocked, out DateTime time);
            return (unlocked, time);
        }

        public void GetIcon(Action<Texture2D> callback) => API.StatsAndAchievements.Client.GetAchievementIcon(id, callback);

        public void Store() => API.StatsAndAchievements.Client.StoreStats();

        /// <summary>
        /// This will create a ScriptableObject based on this leaderbaord ... in general you should not need this
        /// The AchievementData struct has all the same features of the ScriptableObject but is much lighterweight and
        /// more suitable for creation at runtime.
        /// </summary>
        /// <returns></returns>
        public AchievementObject CreateScriptableObject()
        {
            var newObject = UnityEngine.ScriptableObject.CreateInstance<AchievementObject>();
            newObject.Id = this;
            return newObject;
        }

        /// <summary>
        /// This will create a ScriptableObject based on this leaderbaord ... in general you should not need this
        /// The AchievementData struct has all the same features of the ScriptableObject but is much lighterweight and
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
        public override string ToString()
        {
            return id;
        }

        public bool Equals(string other)
        {
            return id.Equals(other);
        }

        public bool Equals(AchievementData other)
        {
            return id.Equals(other.id);
        }

        public override bool Equals(object obj)
        {
            return id.Equals(obj);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public int CompareTo(AchievementData other)
        {
            return id.CompareTo(other.id);
        }

        public int CompareTo(string other)
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