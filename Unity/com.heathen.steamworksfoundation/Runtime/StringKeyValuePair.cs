#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
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