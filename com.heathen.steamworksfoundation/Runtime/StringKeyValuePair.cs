#if HE_SYSCORE && STEAMWORKS_NET
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