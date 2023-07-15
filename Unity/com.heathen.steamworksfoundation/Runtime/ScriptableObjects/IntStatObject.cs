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
    [HelpURL("https://kb.heathen.group/assets/steamworks/guides/stats-object")]
    [Serializable]
    public class IntStatObject : StatObject
    {
        /// <summary>
        /// On get this returns the current stored value of the stat.
        /// On set this sets the value on the Steamworks API
        /// </summary>
        public int Value
        {
            get => data.IntValue();
            set => data.Set(value);
        }

        /// <summary>
        /// Indicates the data type of this stat.
        /// This is used when working with the generic <see cref="StatObject"/> reference.
        /// </summary>
        public override DataType Type { get { return DataType.Int; } }
    }
}
#endif