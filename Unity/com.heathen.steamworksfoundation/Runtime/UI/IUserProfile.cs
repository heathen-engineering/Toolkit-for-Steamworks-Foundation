#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET


namespace HeathenEngineering.SteamworksIntegration.UI
{
    /// <summary>
    /// A simple interface which can be applied to any visual representation of a User such as avatar images, names or even complex UI elements that display many aspects of user data. Several examples of this interface are present in the uGUI Tools package.
    /// </summary>
    public interface IUserProfile
    {
        /// <summary>
        /// Provides a standard approach to reading and setting the UserData object applied to this object. Typically the set portion of this field would simply call the Apply method.
        /// </summary>
        UserData UserData
        {
            get;
            set;
        }
        /// <summary>
        /// A method which can be used to apply a given UserData object.
        /// </summary>
        /// <param name="user">The user to be applied to the profile</param>
        void Apply(UserData user);
    }
}
#endif