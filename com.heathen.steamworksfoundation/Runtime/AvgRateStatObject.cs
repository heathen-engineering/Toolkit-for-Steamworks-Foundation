#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using System;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    [HelpURL("https://kb.heathenengineering.com/assets/steamworks/stats-object")]
    [Serializable]
    public class AvgRateStatObject : StatObject
    {
        /// <summary>
        /// On get this returns the current stored value of the stat.
        /// On set this sets the value on the Steamworks API
        /// </summary>
        public float Value
        {
            get
            {
                if (API.StatsAndAchievements.Client.GetStat(statName, out float data))
                    return data;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Indicates the data type of this stat.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        public override DataType Type { get { return DataType.AvgRate; } }

        /// <summary>
        /// Returns the value of this stat as a float.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <returns></returns>
        public override float GetFloatValue()
        {
            return Value;
        }

        /// <summary>
        /// Returns the value of this stat as an int.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <returns></returns>
        public override int GetIntValue()
        {
            return (int)Value;
        }

        /// <summary>
        /// Sets the value of this stat on the Steamworks API.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <param name="value">The value to set on the API</param>
        public override void SetFloatStat(float value)
        {
            throw new NotImplementedException("Average Rate Stats must use UpdateAvgRateStat in order to update there values.");
        }

        /// <summary>
        /// Sets the value of this stat on the Steamworks API.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <param name="value">The value to set on the API</param>
        public override void SetIntStat(int value)
        {
            throw new NotImplementedException("Average Rate Stats must use UpdateAvgRateStat in order to update there values.");
        }

        public void UpdateAvgRateStat(float value, double length) => SteamUserStats.UpdateAvgRateStat(statName, value, length);

        /// <summary>
        /// This stores all stats to the Valve backend servers it is not possible to store only 1 stat at a time
        /// Note that this will cause a callback from Steamworks which will cause the stats to update
        /// </summary>
        public override void StoreStats()
        {
            SteamUserStats.StoreStats();
        }
    }
}
#endif