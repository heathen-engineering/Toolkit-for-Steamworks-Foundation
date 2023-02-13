#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using System;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    [Serializable]
    public struct StatData : IEquatable<StatData>, IEquatable<string>, IComparable<StatData>, IComparable<string>
    {
        /// <summary>
        /// The API Name as it appears in the Steamworks portal.
        /// </summary>
        [SerializeField]
        private string id;

        public float FloatValue()
        {
            API.StatsAndAchievements.Client.GetStat(id, out float value);
            return value;
        }

        public int IntValue()
        {
            API.StatsAndAchievements.Client.GetStat(id, out int value);
            return value;
        }

        public void Set(float value) => API.StatsAndAchievements.Client.SetStat(id, value);
        public void Set(int value) => API.StatsAndAchievements.Client.SetStat(id, value);
        public void Set(float value, double length) => API.StatsAndAchievements.Client.UpdateAvgRateStat(id, value, length);

        public static void Store() => API.StatsAndAchievements.Client.StoreStats();

        #region Boilerplate
        public bool Equals(string other)
        {
            return id.Equals(other);
        }

        public bool Equals(StatData other)
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

        public int CompareTo(StatData other)
        {
            return id.CompareTo(other.id);
        }

        public int CompareTo(string other)
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