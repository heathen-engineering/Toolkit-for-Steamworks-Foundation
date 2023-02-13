#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using HeathenEngineering.Events;
using Steamworks;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace HeathenEngineering.SteamworksIntegration
{
    [HelpURL("https://kb.heathen.group/assets/steamworks/for-unity-game-engine/components/steamworks-behaviour")]
    [DisallowMultipleComponent]
    public class SteamworksBehaviour : MonoBehaviour
    {
        #region Editor Exposed Values
        /// <summary>
        /// Reference to the <see cref="SteamworksClientApiSettings"/> object containing the configuration to be used for intialization of the Steamworks API
        /// </summary>
        public SteamSettings settings;
        [Header("Events")]
        public UnityEvent evtSteamInitialized = new UnityEvent();
        /// <summary>
        /// An event raised when an error has occred while intializing the Steamworks API
        /// </summary>
        public UnityStringEvent evtSteamInitializationError = new UnityStringEvent();
        #endregion

        private bool abortedInitalization = false;

        private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
        [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
        private static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
        {
            Debug.LogWarning(pchDebugText);
        }

        private void OnEnable()
        {
            API.App.evtSteamInitialized.AddListener(HandleInitalization);
            API.App.evtSteamInitializationError.AddListener(HandleInitalizationError);

            if(SteamSettings.behaviour == null)
                SteamSettings.behaviour = this;

            settings.Initialize();
        }

        private void OnDestroy()
        {
            API.App.evtSteamInitialized.RemoveListener(HandleInitalization);
            API.App.evtSteamInitializationError.RemoveListener(HandleInitalizationError);

            if (SteamSettings.behaviour == this)
                SteamSettings.behaviour = null;
        }

        /// <summary>
        /// Checks if the Steam API is initialized and if not it will create a new Steamworks Behaviour object configure it with the settings and initialize
        /// </summary>
        /// <remarks>
        /// This should only be used in the rare cases you need to initialize Steam API on demand. In a typical araingment you would defiine the Steamworks Beahviour at developer time in the Unity Editor as part of a scene that is only ever loaded once.
        /// </remarks>
        /// <param name="doNotDestroy">Optionally mark the created Steamworks Behaviour object as Do Not Destroy On Load</param>
        public static void CreateIfMissing(SteamSettings settings, bool doNotDestroy = false) => settings.CreateBehaviour(doNotDestroy);

        private void HandleInitalizationError(string message)
        {
            Debug.LogError(message);

            evtSteamInitializationError.Invoke(message);
        }

        private void HandleInitalization()
        {
            evtSteamInitialized.Invoke();
        }
    }
}
#endif