#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using UnityEngine;
using System.Collections.Generic;
using HeathenEngineering.SteamworksIntegration.API;

namespace HeathenEngineering.SteamworksIntegration.Editors
{
    public class SteamContentBuilderConfig : ScriptableObject
    {
#if UNITY_EDITOR
        [HideInInspector]
        public string username;
        [HideInInspector]
        public bool rememberPassword;
        [HideInInspector]
        public string password;
        [HideInInspector]
        public uint targetApp;
        [HideInInspector]
        public int lastDepot;
        public List<Depot> depots;
#endif
    }
}
#endif