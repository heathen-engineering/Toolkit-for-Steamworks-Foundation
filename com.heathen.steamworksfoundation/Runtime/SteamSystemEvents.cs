#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using HeathenEngineering.Events;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    public class SteamSystemEvents : MonoBehaviour
    {
        public SteamSettings settings;
        [Header("Events")]
        public UnityEvent evtSteamInitialized = new UnityEvent();
        /// <summary>
        /// An event raised when an error has occred while intializing the Steamworks API
        /// </summary>
        public UnityStringEvent evtSteamInitializationError = new UnityStringEvent();
        private void Awake()
        {
            settings.evtSteamInitialized.AddListener(evtSteamInitialized.Invoke);
            settings.evtSteamInitializationError.AddListener(evtSteamInitializationError.Invoke);
        }

        private void OnDestroy()
        {
            settings.evtSteamInitialized.RemoveListener(evtSteamInitialized.Invoke);
            settings.evtSteamInitializationError.RemoveListener(evtSteamInitializationError.Invoke);
        }
    }
}
#endif