#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
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
    public class IntStatObject : StatObject
    {
        /// <summary>
        /// On get this returns the current stored value of the stat.
        /// On set this sets the value on the Steamworks API
        /// </summary>
        public int Value
        {
            get
            {
                if (API.StatsAndAchievements.Client.GetStat(statName, out int data))
                    return data;
                else
                    return 0;
            }
            set
            {
                SetIntStat(value);
            }
        }

        /// <summary>
        /// Indicates the data type of this stat.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        public override DataType Type { get { return DataType.Int; } }

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
            return Value;
        }

        /// <summary>
        /// Sets the value of this stat on the Steamworks API.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <param name="value">The value to set on the API</param>
        public override void SetFloatStat(float value) => SteamUserStats.SetStat(statName, value);

        /// <summary>
        /// Sets the value of this stat on the Steamworks API.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        /// <param name="value">The value to set on the API</param>
        public override void SetIntStat(int value) => SteamUserStats.SetStat(statName, value);

        public void AddStat(int value) => Value += value;

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