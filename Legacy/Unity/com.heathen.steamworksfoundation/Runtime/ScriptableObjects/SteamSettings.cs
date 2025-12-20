#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
//#define UNITY_SERVER
using HeathenEngineering.Events;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// <para>The root of Heathen's Steamworks system. <see cref="SteamSettings"/> provides access to all core functionality including stats, achievements, the friend system and the overlay system.</para>
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="SteamSettings"/> object is the root of Heathen Engineering's Steamworks kit.
    /// <see cref="SteamSettings"/> contains the configuration for the fundamental systems of the Steamworks API and provides access to all core functionality.
    /// You can easily access the active <see cref="SteamSettings"/> object any time via <see cref="current"/> a static member that is populated on initialization of the Steamworks API with the settings that are being used to configure it.</para>
    /// <para><see cref="SteamSettings"/> is divided into 2 major areas being <see cref="client"/> and <see cref="server"/>.
    /// The <see cref="client"/> member provides easy access to features and systems relevant for your "client" that is the application the end user is actually playing e.g. your game.
    /// This would include features such as overlay, friends, clans, stats, achievements, etc.
    /// <see cref="server"/> in contrast deals with the configuration of Steamworks Game Server features and only comes into play for server builds.
    /// Note that the <see cref="server"/> member and its functionality are stripped out of client builds, that is it is only accessible in a server build and in the Unity Editor</para>
    /// </remarks>
    [HelpURL("https://kb.heathenengineering.com/assets/steamworks/objects/steam-settings")]
    [CreateAssetMenu(menuName = "Steamworks/Settings")]
    public class SteamSettings : ScriptableObject
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RunTimeInit()
        {
            current = null;
            behaviour = null;
        }

        #region Internal Classes
        /// <summary>
        /// Simple wrapper around common Steam related colors such as used on persona status indicators.
        /// </summary>
        public static class Colors
        {
            public static Color SteamBlue = new Color(0.2f, 0.60f, 0.93f, 1f);
            public static Color SteamGreen = new Color(0.2f, 0.42f, 0.2f, 1f);
            public static Color BrightGreen = new Color(0.4f, 0.84f, 0.4f, 1f);
            public static Color HalfAlpha = new Color(1f, 1f, 1f, 0.5f);
            public static Color ErrorRed = new Color(1, 0.5f, 0.5f, 1);
        }

        /// <summary>
        /// configuration settings and features unique to the Server API
        /// </summary>
        /// <remarks>
        /// Note that this is not available in client builds and can only be accessed in server and editor builds.
        /// <para>
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        [Serializable]
        public class GameServer
        {
            public bool LoggedOn { get; private set; }
            public SteamGameServerConfiguration Configuration => new SteamGameServerConfiguration
            {
                autoInitialize = autoInitialize,
                anonymousServerLogin = anonymousServerLogin,
                autoLogon = autoLogon,
                botPlayerCount = botPlayerCount,
                enableHeartbeats = enableHeartbeats,
                gameData = gameData,
                gameDescription = gameDescription,
                gameDirectory = gameDirectory,
                gamePort = gamePort,
                gameServerToken = gameServerToken,
                ip = ip,
                isDedicated = isDedicated,
                isPasswordProtected = isPasswordProtected,
                mapName = mapName,
                maxPlayerCount = maxPlayerCount,
                queryPort = queryPort,
                rulePairs = rulePairs.ToArray(),
                serverName = serverName,
                serverVersion = serverVersion,
                spectatorPort = spectatorPort,
                spectatorServerName = spectatorServerName,
                supportSpectators = supportSpectators,
                usingGameServerAuthApi = usingGameServerAuthApi
            };

            public bool autoInitialize = false;
            public bool autoLogon = false;
            public uint ip = 0;
            public ushort queryPort = 27016;
            public ushort gamePort = 27015;
            public string serverVersion = "1.0.0.0";
            public ushort spectatorPort = 27017;

            public CSteamID ServerId => SteamGameServer.GetSteamID();
            public bool usingGameServerAuthApi = false;
            public bool enableHeartbeats = true;
            public bool supportSpectators = false;
            public string spectatorServerName = "Usually GameDescription + Spectator";
            public bool anonymousServerLogin = false;
            public string gameServerToken = "See https://steamcommunity.com/dev/managegameservers";
            public bool isPasswordProtected = false;
            public string serverName = "My Server Name";
            public string gameDescription = "Usually the name of your game";
            public string gameDirectory = "e.g. its folder name";
            public bool isDedicated = false;
            public int maxPlayerCount = 4;
            public int botPlayerCount = 0;
            public string mapName = "";
            [Tooltip("A delimited string used for Matchmaking Filtering e.g. CoolPeopleOnly,NoWagonsAllowed.\nThe above represents 2 data points matchmaking will then filter accordingly\n... see Heathen Game Server Browser for more informaiton.")]
            public string gameData;
            public List<StringKeyValuePair> rulePairs = new List<StringKeyValuePair>();

            public API.App.Server.DisconnectedEvent EventDisconnected => API.App.Server.eventDisconnected;
            public API.App.Server.ConnectedEvent EventConnected => API.App.Server.eventConnected;
            public API.App.Server.FailureEvent EventFailure => API.App.Server.eventFailure;
        }

        /// <summary>
        /// configuration settings and features unique to the Client API
        /// </summary>
        /// <remarks>
        /// <para>
        /// To wrap your own logic in conditional compilation you can use
        /// </para>
        /// </remarks>
        [Serializable]
        public class GameClient
        {

        }
        #endregion

        #region Static Access
        public static void Unload() => API.App.Unload();

        /// <summary>
        /// A reference to the initialized <see cref="SteamSettings"/> object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value gets set when the <see cref="Initialize"/> method is called which should be done by the <see cref="SteamworksBehaviour"/>. Note that your app should have 1 <see cref="SteamworksBehaviour"/> defined in a scene that is loaded once and is never reloaded, that is you should not put the <see cref="SteamworksBehaviour"/> on a menu scene that will be reloaded multiple times during your games session life as this will break events and other features of the Steamworks API.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class ExampleBehaviour : MonoBehaviour
        /// {
        ///     public void SayMyName()
        ///     {
        ///         Debug.Log("This user's name is " + SystemSettings.Client.user.DisplayName);
        ///     }
        /// }
        /// </code>
        /// </example>
        public static SteamSettings current = null;
        public static SteamworksBehaviour behaviour = null;
        /// <summary>
        /// The AppId_t value configured and initialized for.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the app id value the developer would have typed in to the Unity Editor when setting up the project.
        /// Note that hackers can easily modify this value to cause the Steamworks API to initialize as a different game or can use the steam_appid.txt to force the Steamworks API to register as a different ID.
        /// You can confirm what ID Valve sees this program as running as by calling <see cref="GetAppId"/> you can then compare this fixed value to insure your user is not attempting to manipulate your program.
        /// In addition if you are integrating deeply with the Steamworks API such as using stats, achievements, leaderboards and other features with a configuration specific to your app ID ... this will further insure that if a user manages to initialize as an app other than your App ID ... such as an attempt to pirate your game that these features will break insuring a degraded experance for pirates.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// public class ExampleBehaviour : MonoBehaviour
        /// {
        ///     public void AppIdTests()
        ///     {
        ///         Debug.Log("Configured to run as " + SystemSettings.ApplicationId + ", actually running as " + SystemSettings.GetAppId() );
        ///     }
        /// }
        /// </code>
        /// </example>
        public static AppId_t ApplicationId => current != null ? current.applicationId : default;

        /// <summary>
        /// Indicates an error with API initialization
        /// </summary>
        /// <remarks>
        /// If true than an error occurred during the initialization of the Steamworks API and normal functionality will not be possible.
        /// </remarks>
        public static bool HasInitializationError => API.App.HasInitializationError;

        /// <summary>
        /// Initialization error message if any
        /// </summary>
        /// <remarks>
        /// See <see cref="HasInitializationError"/> to determine if an error occurred, if so this message will describe possible causes.
        /// </remarks>
        public static string InitializationErrorMessage => API.App.InitializationErrorMessage;

        /// <summary>
        /// Indicates rather or not the Steamworks API is initialized
        /// </summary>
        /// <remarks>
        /// <para>This value gets set to true when <see cref="Initialize"/> is called by the <see cref="SteamworksClientApiSystem"/>.
        /// Note that if Steamworks API fails to initialize such as if the Steamworks client is not installed, running and logged in with a valid Steamworks user then the call to Init will fail and the <see cref="Initialized"/> value will remain false.</para>
        /// </remarks>
        public static bool Initialized => API.App.Initialized;

        /// <summary>
        /// Static access to the active <see cref="GameClient"/> object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="GameClient"/> object provides easy access to client specific functions such as the <see cref="Steam.UserData"/> for the local user ... you can access this via.
        /// <code>
        /// SteamSettings.Client.user
        /// </code>
        /// or you can fetch the <see cref="Steam.UserData"/> for any given user via code such as
        /// <code>
        /// SteamSettings.Client.GetUserData(ulong userId)
        /// </code>
        /// For more information please see the documentation on the <see cref="GameClient"/> object.
        /// </para>
        /// </remarks>
        public static GameClient Client => current == null ? null : current.client;

        /// <summary>
        /// Static access to the active <see cref="GameServer"/> object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="GameServer"/> object provides easy access to Steamworks Game Server configuration and server only features.
        /// Note that your server can be a Steamworks Game Server and not have to use the Steamworks Networking transports ... e.g. you can use any transport you like and host anywhere you like.
        /// Being a Steamworks Game Server simply means that your server has initialized the Steamworks API and registered its self against Valve's backend ... in addition if this server has an  IP address of a trusted server as defined in your app configuration on the Steamworks Portal,
        /// then it may perform GS only actions such as setting stats and achievements that are marked as GS only.
        /// </para>
        /// </remarks>
        public static GameServer Server => current == null ? null : current.server;

        /// <summary>
        /// The list of achievements registered for this application.
        /// </summary>
        /// <remarks>
        /// See <see cref="achievements"/> for more information. This field simply access the <see cref="achievements"/> member for the <see cref="current"/> <see cref="SteamworksClientApiSettings"/> object.
        /// </remarks>
        public static List<AchievementObject> Achievements => current == null ? null : current.achievements;

        /// <summary>
        /// The list of stats registered for this application.
        /// </summary>
        /// <remarks>
        /// See <see cref="stats"/> for more information. This field simply access the <see cref="stats"/> member for the <see cref="current"/> <see cref="SteamworksClientApiSettings"/> object.
        /// </remarks>
        public static List<StatObject> Stats => current == null ? null : current.stats;

        #endregion

        #region Utility Functions

        /// <summary>
        /// Checks if the Steam API is initialized and if not it will create a new Steamworks Behaviour object configure it with the settings and initialize
        /// </summary>
        /// <remarks>
        /// This should only be used in the rare cases you need to initialize Steam API on demand. In a typical arraignment you would define the Steamworks Behaviour at developer time in the Unity Editor as part of a scene that is only ever loaded once.
        /// </remarks>
        /// <param name="doNotDestroy">Optionally mark the created Steamworks Behaviour object as Do Not Destroy On Load</param>
        public void CreateBehaviour(bool doNotDestroy = false, Action initializedCallback = null, Action<string> errorCallback = null)
        {
            if (!Initialized)
            {
                var steamGO = new GameObject("Steamworks");
                steamGO.SetActive(false);
                if (doNotDestroy)
                    DontDestroyOnLoad(steamGO);
                var behaviour = steamGO.AddComponent<SteamworksBehaviour>();

                if (initializedCallback != null)
                    behaviour.evtSteamInitialized.AddListener(initializedCallback.Invoke);

                if (errorCallback != null)
                    behaviour.evtSteamInitializationError.AddListener(errorCallback.Invoke);

                behaviour.settings = this;
                steamGO.SetActive(true);
            }
        }

        /// <summary>
        /// Checks if the Steam API is initialized and if not it will create a new Steamworks Behaviour object configure it with the settings and initialize
        /// </summary>
        /// <remarks>
        /// This should only be used in the rare cases you need to initialize Steam API on demand. In a typical arraignment you would define the Steamworks Behaviour at developer time in the Unity Editor as part of a scene that is only ever loaded once.
        /// </remarks>
        /// <param name="doNotDestroy">Optionally mark the created Steamworks Behaviour object as Do Not Destroy On Load</param>
        public static void CreateBehaviour(SteamSettings settings, bool doNotDestroy = false) => settings.CreateBehaviour(doNotDestroy);
        #endregion

        #region Instanced Members
        /// <summary>
        /// The current application ID
        /// </summary>
        /// <remarks>
        /// <para>It is important that this is set to your game's appId.
        /// Note that when working in Unity Editor you need to change this value in the <see cref="SteamworksClientApiSettings"/> object your using but also in the steam_appid.txt file located in the root of your project.
        /// You can read more about the steam_appid.txt file here <a href="https://heathen-engineering.mn.co/posts/steam_appidtxt"/></para>
        /// </remarks>
        public AppId_t applicationId = new AppId_t(0x0);
        public int callbackTick_Milliseconds = 15;

        /// <summary>
        /// Used in various processes to determine the level of detail to log
        /// </summary>
        public bool isDebugging = false;

        /// <summary>
        /// Contains server side functionality and is not available in client builds
        /// </summary>
        /// <remarks>
        /// Note that this is not available in client builds and can only be accessed in server and editor builds.
        /// <para>
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        public GameServer server = new GameServer();

        /// <summary>
        /// Contains client side functionality
        /// </summary>
        /// <remarks>
        /// Note that this is not available in server builds and can only be accessed in client and editor builds.
        /// <para>
        /// To wrap your own logic in conditional compilation you can use
        /// <code>
        /// #if !UNITY_SERVER || UNITY_EDITOR
        /// //You Code Here!
        /// #endif
        /// </code>
        /// </para>
        /// </remarks>
        public GameClient client = new GameClient();

        /// <summary>
        /// The registered stats associated with this configuration
        /// </summary>
        /// <remarks>
        /// This collection is used by Valve callbacks to match incoming stat updates and records the value against the appropriate stat.
        /// Put more simply if a stat is listed here then the system will update that <see cref="StatObject"/> object with score changes as that information comes in from the Valve backend insuring that these <see cref="StatObject"/> objects are an up to date snap shot of the local user's stat value.
        /// For servers these objects simplify fetching and setting stat values for targeted users but of course doesn't cache values for a local user since server's have no local user.
        /// </remarks>
        public List<StatObject> stats = new List<StatObject>();

        /// <summary>
        /// The registered achievements associated with this configuration
        /// </summary>
        /// <remarks>
        /// This collection is used by Valve callbacks to match incoming stat updates and records the value against the appropriate achievement.
        /// Put more simply if a stat is listed here then the system will update that <see cref="SteamAchievementData"/> object with state changes as that information comes in from the Valve backend insuring that these <see cref="AchievementObject"/> objects are an up to date snap shot of the local user's achievement value.
        /// For servers these objects simplify fetching and setting stat values for targeted users but of course doesn't cache values for a local user since server's have no local user.
        /// </remarks>
        public List<AchievementObject> achievements = new List<AchievementObject>();

        #endregion

        #region Internals
        /// <summary>
        /// Initialization logic for the Steam API
        /// </summary>
        public void Initialize()
        {
            if (Initialized)
                return;
            else
            {
                current = this;
                API.App.isDebugging = isDebugging;

#if !UNITY_SERVER
                #region Client
                if(client == null)
                {
                    Debug.LogError("Invalid SteamSettings object detected. the client object is null and will not initialize properly, aborting initialization.");
                    return;
                }

                API.App.Client.Initialize(applicationId);

                #endregion
#else
                #region Server
                API.App.Server.Initialize(applicationId, Server.Configuration);
                #endregion
#endif

            }
        }
        #endregion
    }
}
#endif