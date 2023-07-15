#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using UnityEditor;

namespace HeathenEngineering.SteamworksIntegration.Editors
{
    [System.Serializable]
    public class Depot
    {
        public string name;
        public uint id;
        public BuildTarget target;
        public string extension;
    }
}
#endif