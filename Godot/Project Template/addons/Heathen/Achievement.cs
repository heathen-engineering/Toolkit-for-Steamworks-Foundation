using System;
using Godot;
using Steamworks;

namespace HeathenEngineering.SteamworksIntegration
{
    public struct Achievement : IEquatable<string>
    {
        public string id;

        public string Name
        {
            get => API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(id, AchievementAttributes.name);
        }

        public string Description
        {
            get => API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(id, AchievementAttributes.desc);
        }

        public bool Hidden
        {
            get => API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(id, AchievementAttributes.hidden) == "1";
        }

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
        public void Unlock(CSteamID user)
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
        public void ClearAchievement(CSteamID user)
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
        public bool GetAchievementStatus(CSteamID user)
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

        public void GetIcon(Action<Image> callback) => API.StatsAndAchievements.Client.GetAchievementIcon(id, callback);

        public void Store() => API.StatsAndAchievements.Client.StoreStats();

        public bool Equals(string other)
        {
            return id.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return id.Equals(obj);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public static bool operator ==(Achievement l, Achievement r) => l.id == r.id;
        public static bool operator ==(string l, Achievement r) => l == r.id;
        public static bool operator ==(Achievement l, string r) => l.id == r;
        public static bool operator !=(Achievement l, Achievement r) => l.id != r.id;
        public static bool operator !=(string l, Achievement r) => l != r.id;
        public static bool operator !=(Achievement l, string r) => l.id != r;

        public static implicit operator string(Achievement c) => c.id;
        public static implicit operator Achievement(string id) => new Achievement { id = id };
    }
}