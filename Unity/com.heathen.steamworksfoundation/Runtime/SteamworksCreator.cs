#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using HeathenEngineering.Events;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    [HelpURL("https://kb.heathenengineering.com/assets/steamworks")]
    [DisallowMultipleComponent]
    public class SteamworksCreator : MonoBehaviour
    {
        public bool createOnStart;
        public bool markAsDoNotDestroy;
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

            if (SteamSettings.behaviour != null)
            {
                SteamSettings.behaviour.evtSteamInitialized.RemoveListener(evtSteamInitialized.Invoke);
                SteamSettings.behaviour.evtSteamInitializationError.RemoveListener(evtSteamInitializationError.Invoke);
            }
        }

        private void Start()
        {
            if (createOnStart)
                settings.CreateBehaviour(markAsDoNotDestroy, evtSteamInitialized.Invoke, evtSteamInitializationError.Invoke);
        }

        public void CreateIfMissing()
        {
            settings.CreateBehaviour(markAsDoNotDestroy, evtSteamInitialized.Invoke, evtSteamInitializationError.Invoke);
        }
    }
}
#endif