#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    [HelpURL("https://kb.heathen.group/assets/steamworks/guides/stats-object")]
    [Serializable]
    public class AvgRateStatObject : StatObject
    {
        /// <summary>
        /// On get this returns the current stored value of the stat.
        /// On set this sets the value on the Steamworks API
        /// </summary>
        public float Value
        {
            get => data.FloatValue();
            set => data.Set(value);
        }

        /// <summary>
        /// Indicates the data type of this stat.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        public override DataType Type { get { return DataType.AvgRate; } }

        public void UpdateAvgRateStat(float value, double length) => data.Set(value, length);
    }
}
#endif