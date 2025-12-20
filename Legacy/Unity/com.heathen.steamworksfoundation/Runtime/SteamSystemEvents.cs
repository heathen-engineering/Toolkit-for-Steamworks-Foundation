#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using HeathenEngineering.Events;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    [Obsolete("Replaced by Steamworks Event Triggers")]
    public class SteamSystemEvents : MonoBehaviour
    {
        [Header("Events")]
        public UnityEvent evtSteamInitialized = new UnityEvent();
        /// <summary>
        /// An event raised when an error has occred while intializing the Steamworks API
        /// </summary>
        public UnityStringEvent evtSteamInitializationError = new UnityStringEvent();
        private void Awake()
        {
            API.App.evtSteamInitialized.AddListener(evtSteamInitialized.Invoke);
            API.App.evtSteamInitializationError.AddListener(evtSteamInitializationError.Invoke);
        }

        private void OnDestroy()
        {
            API.App.evtSteamInitialized.RemoveListener(evtSteamInitialized.Invoke);
            API.App.evtSteamInitializationError.RemoveListener(evtSteamInitializationError.Invoke);
        }
    }
}
#endif