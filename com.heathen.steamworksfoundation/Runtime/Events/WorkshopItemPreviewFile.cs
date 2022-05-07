#if HE_SYSCORE && STEAMWORKS_NET
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