#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKS_NET
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration.Editors
{
    static class SteamAppIdWriter
    {
        /// <summary>
        /// Creates the steam_appid.txt file if its missing
        /// </summary>
        [InitializeOnLoadMethod]
        public static void CreateAppIdTextFileIfMissing()
        {
            var appIdPath = new DirectoryInfo(Application.dataPath).Parent.FullName + "/steam_appid.txt";
            if (!File.Exists(appIdPath))
                File.WriteAllText(appIdPath, "480");
        }
    }
}
#endif