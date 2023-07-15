#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// Used in the <see cref="API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(string, AchievementAttributes)"/> method to read a specific attribute
    /// </summary>
    public enum AchievementAttributes
    {
        /// <summary>
        /// Get the name of the achievement
        /// </summary>
        name,
        /// <summary>
        /// Get the description of the achievement
        /// </summary>
        desc,
        /// <summary>
        /// Return a value that indicates if the achievement is hidden from users
        /// </summary>
        hidden,
    }
}
#endif