#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using HeathenEngineering.Events;
using Steamworks;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace HeathenEngineering.SteamworksIntegration
{

    /// <summary>
    /// <para>This replaces the SteamManager concept from classic Steamworks.NET</para>
    /// 
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="SteamworksBehaviour"/> uses <see cref="SteamSettings"/> to initialize the Steam API and handles callbacks for the system.
    /// For the convenance of users using a singleton model <see cref="SteamSettings.current"/> can be used to access the active settings object.
    /// <see cref="SteamSettings.Client"/> is a static member containing all client scoped objects.
    /// <see cref="SteamSettings.Server"/> is a static member containing all server scoped objects.
    /// </para>
    /// <para>
    /// The <see cref="SteamworksBehaviour"/> is the core compoenent to the Heathen Steamworks kit and replaces the funcitonality present in Steamworks.NET's SteamManager.
    /// The primary funciton of the manager is to operate the update loop required by the Steamworks API and to handle and direct callbacks from the Steamworks API.</para>
    /// <para>It is strongly advised that you never unload or reload the <see cref="SteamworksBehaviour"/> for example you should not place the <see cref="SteamworksBehaviour"/> in your title scene because that scene will be unloaded and reloaded multiple times.
    /// Even if you mark the object as Do Not Destroy on Load, on reload of the title or similar scene Unity will create a second <see cref="SteamworksBehaviour"/> creating issues with the memory it manages.</para>
    /// <para>The recomended approch is place your <see cref="SteamworksBehaviour"/> and any other "manager" in a bootstrap scene that loads first and is never reloaded through the life of the game.
    /// This will help insure that through the life of your users play session 1 and exsactly 1 <see cref="SteamworksBehaviour"/> is created and never destroyed.
    /// While there are other approches you could take to insure this using a simple bootstrap scene is typically the simplest. For more information on this subject see <a href="https://heathen-engineering.mn.co/posts/scenes-management-quick-start"/>.
    /// This article referes to another tool available from Heathen Engineering however the concepts within apply rather or not your using that tool.</para>
    /// </remarks>
    [HelpURL("https://kb.heathenengineering.com/assets/steamworks")]
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

        private void OnDestroy()
        {
            settings.evtSteamInitialized.RemoveListener(evtSteamInitialized.Invoke);
            settings.evtSteamInitializationError.RemoveListener(evtSteamInitializationError.Invoke);
        }
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
            if (settings == null)
            {
                Debug.LogError("Attempted to enable SteamworksBehaviour with no SteamSettings provided. This is not supported!");
                abortedInitalization = true;
                enabled = false;                
                return;
            }

            if(SteamSettings.Initialized)
            {
                if (settings.isDebugging || Application.isEditor)
                    Debug.LogWarning("Detected duplicate initalization!\nInitalization will be aborted, this is most offten caused when you have defined the Steamworks Behaviour object in a scene that is loaded multiple times. " +
                        "This is an inefficent model that causes duplicate initalization of all objects in that scene even if they are declared as Do Not Destroy on load. " +
                        "It is recomended to either use a 'Bootstrap' scene for objects sinsative to this such as Steamworks Beahviour, Event System, etc. or to create such objects on demand from script as opposed to defining them as part of the scene. " +
                        "You can create the Steamworks Behaviour object on demand with a single line call via 'SteamSettings.CreateBehaviour(...)'.");
                abortedInitalization = true;
                enabled = false;
                return;
            }

            settings.evtSteamInitialized.AddListener(evtSteamInitialized.Invoke);
            settings.evtSteamInitializationError.AddListener(evtSteamInitializationError.Invoke);

            SteamSettings.behaviour = this;

#if !UNITY_SERVER
            if (settings.isDebugging)
                Debug.Log("Client Startup Detected!");
            settings.Init();
            
            if(!SteamSettings.Initialized)
            {
                enabled = false;
                Debug.LogWarning("Failed to initialize the Steam API please check the logs for details.");
                return;
            }

            if (m_SteamAPIWarningMessageHook == null)
            {
                // Set up our callback to recieve warning messages from Steamworks.
                // You must launch with "-debug_steamapi" in the launch args to recieve warnings.
                m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
                SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
            }
#endif
#if UNITY_SERVER //|| UNITY_EDITOR
            if (settings.isDebugging)
                Debug.Log("Game Server Startup Detected!");
            if (settings.server.autoInitialize)
                InitializeGameServer();
#endif
        }

        private void OnDisable()
        {
            if (abortedInitalization)
                return;

#if UNITY_SERVER //|| UNITY_EDITOR
            Debug.Log("Logging off the Steam Game Server");

            if (settings.server.usingGameServerAuthApi)
                SteamGameServer.SetAdvertiseServerActive(false);

            //Remove the settings event listeners
            settings.server.disconnected.RemoveListener(LogDisconnect);
            settings.server.connected.RemoveListener(LogConnect);
            settings.server.failure.RemoveListener(LogFailure);

            //Log the server off of Steam
            SteamGameServer.LogOff();
            Debug.Log("Steam Game Server has been logged off");
#endif
        }

        private void Update()
        {
            if (!SteamSettings.Initialized)
            {
                return;
            }

#if !UNITY_SERVER
            Steamworks.SteamAPI.RunCallbacks();
#else
            Steamworks.GameServer.RunCallbacks();
#endif
        }

        /// <summary>
        /// Checks if the Steam API is initialized and if not it will create a new Steamworks Behaviour object configure it with the settings and initialize
        /// </summary>
        /// <remarks>
        /// This should only be used in the rare cases you need to initialize Steam API on demand. In a typical araingment you would defiine the Steamworks Beahviour at developer time in the Unity Editor as part of a scene that is only ever loaded once.
        /// </remarks>
        /// <param name="doNotDestroy">Optionally mark the created Steamworks Behaviour object as Do Not Destroy On Load</param>
        public static void CreateIfMissing(SteamSettings settings, bool doNotDestroy = false) => settings.CreateBehaviour(doNotDestroy);

        #region Server Only Logic
        /// <summary>
        /// Initializes the Steam Game Server API.
        /// </summary>
        /// <remarks>
        /// This is called automatically if <see cref="SteamSettings.GameServer.autoInitialize"/> is set to true on the <see cref="SteamSettings.server"/> member
        /// e.g. check the toggles on your Steam Settings object.
        /// </remarks>
        public void InitializeGameServer()
        {
            if (SteamSettings.Initialized)
            {
                Debug.LogError("Tried to initialize the Steamworks API twice in one session!");
                return;
            }

            if (settings.isDebugging)
                Debug.Log("Adding Steam Game Server connection listeners.");
            settings.server.connected.AddListener(OnSteamServersConnected);

            //If debugging
            if (settings.isDebugging)
            {
                Debug.Log("Adding Steam Game Server debug listeners");
                settings.server.disconnected.AddListener(LogDisconnect);
                settings.server.connected.AddListener(LogConnect);
                settings.server.failure.AddListener(LogFailure);
            }

            SteamSettings.behaviour = this;
            settings.Init();

            if (SteamSettings.Initialized && settings.server.autoLogon)
                settings.server.LogOn();
        }

        private void OnSteamServersConnected(SteamServersConnected_t pLogonSuccess)
        {
            settings.server.serverId = SteamGameServer.GetSteamID();

            if (settings.isDebugging)
                Debug.Log("Game Server connected to Steamworks successfully!\n\tMod Directory = " + settings.server.gameDirectory + "\n\tApplicaiton ID = " + settings.applicationId.m_AppId.ToString() + "\n\tServer ID = " + settings.server.serverId.m_SteamID.ToString() + "\n\tServer Name = " + settings.server.serverName + "\n\tGame Description = " + settings.server.gameDescription + "\n\tMax Player Count = " + settings.server.maxPlayerCount.ToString());

            // Tell Steamworks about our server details
            SendUpdatedServerDetailsToSteam();
        }

        private void SendUpdatedServerDetailsToSteam()
        {
            if (settings.server.rulePairs != null && settings.server.rulePairs.Count > 0)
            {
                var pairString = "Set the following rules:\n";

                foreach (var pair in settings.server.rulePairs)
                {
                    SteamGameServer.SetKeyValue(pair.key, pair.value);
                    pairString += "\n\t[" + pair.key + "] = [" + pair.value + "]";
                }

                if (settings.isDebugging)
                    Debug.Log(pairString);
            }
        }

        private void LogFailure(SteamServerConnectFailure_t arg0)
        {
            Debug.LogError("Steamworks.GameServer.LogOn reported connection Failure: " + arg0.m_eResult.ToString());
        }

        private void LogConnect(SteamServersConnected_t arg0)
        {
            Debug.LogError("Steamworks.GameServer.LogOn reported connection Ready");
        }

        private void LogDisconnect(SteamServersDisconnected_t arg0)
        {
            Debug.LogError("Steamworks.GameServer reported connection Closed: " + arg0.m_eResult.ToString());
        }
        #endregion
    }
}
#endif