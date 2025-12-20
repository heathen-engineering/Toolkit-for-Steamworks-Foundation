#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using HeathenEngineering.Events;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    [HelpURL("https://kb.heathen.group/assets/steamworks/for-unity-game-engine/components/steamworks-behaviour")]
    [DisallowMultipleComponent]
    public class SteamworksBehaviour : MonoBehaviour
    {
        #region Editor Exposed Values
        /// <summary>
        /// Reference to the <see cref="SteamSettings"/> object containing the configuration to be used for initialization of the Steamworks API
        /// </summary>
        public SteamSettings settings;
        /// <summary>
        /// An event raised when the Steam API has completed initialization and is ready for use
        /// </summary>
        [Header("Events")]
        public UnityEvent evtSteamInitialized = new UnityEvent();
        /// <summary>
        /// An event raised when an error has occurred while initializing the Steamworks API
        /// </summary>
        public UnityStringEvent evtSteamInitializationError = new UnityStringEvent();
        #endregion

        private void OnEnable()
        {
            API.App.evtSteamInitialized.AddListener(HandleInitialization);
            API.App.evtSteamInitializationError.AddListener(HandleInitializationError);

            if (SteamSettings.behaviour == null)
                SteamSettings.behaviour = this;

            settings.Initialize();
        }

        private void OnDestroy()
        {
            API.App.evtSteamInitialized.RemoveListener(HandleInitialization);
            API.App.evtSteamInitializationError.RemoveListener(HandleInitializationError);
        }

        /// <summary>
        /// Checks if the Steam API is initialized and if not it will create a new Steamworks Behaviour object configure it with the settings and initialize
        /// </summary>
        /// <remarks>
        /// This should only be used in the rare cases you need to initialize Steam API on demand. In a typical arraignment you would define the Steamworks Behaviour at developer time in the Unity Editor as part of a scene that is only ever loaded once.
        /// </remarks>
        /// <param name="doNotDestroy">Optionally mark the created Steamworks Behaviour object as Do Not Destroy On Load</param>
        public static void CreateIfMissing(SteamSettings settings, bool doNotDestroy = false) => settings.CreateBehaviour(doNotDestroy);

        private void HandleInitializationError(string message)
        {
            Debug.LogError(message);

            evtSteamInitializationError.Invoke(message);
        }

        private void HandleInitialization()
        {
            evtSteamInitialized.Invoke();
        }
    }
}
#endif