#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;

namespace HeathenEngineering.SteamworksIntegration
{
    public struct WorkshopItemPreviewFile
    {
        public string source;
        /// <summary>
        /// YouTubeVideo and Sketchfab are not currently supported
        /// </summary>
        public EItemPreviewType type;
    }
}
#endif