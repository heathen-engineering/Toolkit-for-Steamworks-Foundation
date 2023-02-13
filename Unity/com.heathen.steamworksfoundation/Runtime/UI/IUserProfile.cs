#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET


namespace HeathenEngineering.SteamworksIntegration.UI
{
    public interface IUserProfile
    {
        UserData UserData
        {
            get;
            set;
        }

        void Apply(UserData user);
    }
}
#endif