#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using System;

namespace HeathenEngineering.SteamworksIntegration
{

    [Serializable]
    public struct StringKeyValuePair
    {
        public string key;
        public string value;
    }
}
#endif