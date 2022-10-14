#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using HeathenEngineering.Events;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// <para>The root of Heathen Engieering's Steamworks system. <see cref="SteamSettings"/> provides access to all core funcitonality including stats, achievements, the friend system and the overlay system.</para>
    /// </summary>
    /// <remarks>
    /// <para>The <see cref="SteamSettings"/> object is the root of Heathen Engineering's Steamworks kit.
    /// <see cref="SteamSettings"/> contains the configuration for the fundamental systems of the Steamworks API and provides access to all core funcitonality.
    /// You can easily access the active <see cref="SteamSettings"/> object any time via <see cref="current"/> a static member that is populated on initalization of the Steamworks API with the settings that are being used to configure it.</para>
    /// <para><see cref="SteamSettings"/> is divided into 2 major areas being <see cref="client"/> and <see cref="server"/>.
    /// The <see cref="client"/> member provides easy access to features and systems relivent for your "client" that is the applicaiton the end user is actually playing e.g. your game.
    /// This would include features such as overlay, friends, clans, stats, achievements, etc.
    /// <see cref="server"/> in contrast deals with tthe configuraiton of Steamworks Game Server features and only comes into play for server builds.
    /// Note that the <see cref="server"/> member and its funcitonality are stripped out of client builds, that is it is only accessable in a server build and in the Unity Editor</para>
    /// </remarks>
    [HelpURL("https://kb.heathenengineering.com/assets/steamworks/objects/steam-settings")]
    [CreateAssetMenu(menuName = "Steamworks/Settings")]
    public class SteamSettings : ScriptableObject
    {
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
            [Serializable]
            public class DisconnectedEvent : UnityEvent<SteamServersDisconnected_t> { }

            [Serializable]
            public class ConnectedEvent : UnityEvent<SteamServersConnected_t> { }

            [Serializable]
            public class FailureEvent : UnityEvent<SteamServerConnectFailure_t> { }

            public bool autoInitialize = false;
            public bool autoLogon = false;

            public uint ip = 0;
            public ushort queryPort = 27016;
            public ushort gamePort = 27015;
            public string serverVersion = "1.0.0.0";
            public ushort spectatorPort = 27017;

            public CSteamID serverId;
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

            [Header("Events")]
            public DisconnectedEvent disconnected = new DisconnectedEvent();
            public ConnectedEvent connected = new ConnectedEvent();
            public FailureEvent failure = new FailureEvent();

            private Callback<SteamServerConnectFailure_t> steamServerConnectFailure;
            private Callback<SteamServersConnected_t> steamServersConnected;
            private Callback<SteamServersDisconnected_t> steamServersDisconnected;

            private void OnSteamServersDisconnected(SteamServersDisconnected_t param)
            {
                LoggedOn = false;
                disconnected.Invoke(param);
            }

            private void OnSteamServersConnected(SteamServersConnected_t param)
            {
                LoggedOn = true;
                connected.Invoke(param);
            }

            private void OnSteamServerConnectFailure(SteamServerConnectFailure_t param)
            {
                LoggedOn = false;
                failure.Invoke(param);
            }

            internal void Init()
            {
                if (current.isDebugging)
                    Debug.Log("Configuring server event system.");

                if (current.isDebugging)
                    Debug.Log("Registering callbacks.");

                RegisterCallbacks();

                EServerMode eMode = EServerMode.eServerModeNoAuthentication;

                if (usingGameServerAuthApi)
                    eMode = EServerMode.eServerModeAuthenticationAndSecure;

                if (current.isDebugging)
                    Debug.Log("Initalizing Steam Game Server API: (" + ip + ", " + gamePort.ToString() + ", " + queryPort.ToString() + ", " + eMode.ToString() + ", " + serverVersion + ")");

                Initialized = Steamworks.GameServer.Init(ip, gamePort, queryPort, eMode, serverVersion);

                if (!Initialized)
                {
                    HasInitalizationError = true;
                    InitalizationErrorMessage = "Steam API failed to initialize!\nOne of the following issues must be true:\n"
                            + "The Steam couldn't determine the App ID of the game. If you're running your server from the executable or debugger directly then you must have a steam_appid.txt in your server directory next to the executable, with your app ID in it and nothing else. Steam will look for this file in the current working directory. If you are running your executable from a different directory you may need to relocate the steam_appid.txt file.\n"
                            + "Ensure that you own a license for the App ID on the currently active Steam account (if login via token). Your game must show up in your Steam library.\n"
                            + "The App ID is not completely set up, i.e. in Release State: Unavailable, or it's missing default packages.";

                    current.evtSteamInitializationError.Invoke(InitalizationErrorMessage);
                    Debug.LogError(InitalizationErrorMessage);
                    return;
                }

                SteamGameServer.SetModDir(gameDirectory);
                SteamGameServer.SetProduct(current.applicationId.m_AppId.ToString());
                SteamGameServer.SetGameDescription(gameDescription);
                SteamGameServer.SetMaxPlayerCount(maxPlayerCount);
                SteamGameServer.SetPasswordProtected(isPasswordProtected);
                SteamGameServer.SetServerName(serverName);
                SteamGameServer.SetBotPlayerCount(botPlayerCount);
                SteamGameServer.SetMapName(mapName);
                SteamGameServer.SetDedicatedServer(isDedicated);

                if (current.isDebugging)
                {
                    Debug.Log("Configuring the SteamGameServer interface:\n\tServer Name: " + serverName +
                        "\n\tDescription: " + gameDescription +
                        "\n\tProduct: " + current.applicationId +
                        "\n\tIs Dedicated Server: " + isDedicated +
                        "\n\tIs Password Protected: " + isPasswordProtected +
                        "\n\tMax Players: " + maxPlayerCount +
                        "\n\tBot Player Count: " + botPlayerCount +
                        "\n\tMod Dir: " + gameDirectory +
                        "\n\tMap Name: " + mapName);
                }

                if (supportSpectators)
                {
                    if (current.isDebugging)
                        Debug.Log("Spectator enabled:\n\tName = " + spectatorServerName + "\n\tSpectator Port = " + spectatorPort.ToString());

                    SteamGameServer.SetSpectatorPort(spectatorPort);
                    SteamGameServer.SetSpectatorServerName(spectatorServerName);
                }
                else if (current.isDebugging)
                    Debug.Log("Spectator Set Up Skipped");

                if (current.isDebugging)
                {
                    Debug.Log("Steam API has been initialized with App ID: " + SteamGameServerUtils.GetAppID());
                }

                if (current.applicationId != SteamGameServerUtils.GetAppID())
                {
#if UNITY_EDITOR
                    Debug.LogWarning("The reported applicaiton ID of " + SteamGameServerUtils.GetAppID().ToString() + " does not match the anticipated ID of " + current.applicationId.ToString() + ". This is most frequently caused when you edit your AppID bu fail to restart Unity, Visual Studio and or any other processes that may have mounted the Steam API under the previous App ID. To correct this please insure your AppID is entered correctly in the SteamSettings object and that you fully restart the Unity Editor, Visual Studio and any other processes that may have connectd to them.");
#else
                    Debug.LogError("The reported AppId is not as expected:\nAppId Reported = " + SteamGameServerUtils.GetAppID().ToString() + "AppId Expected = " + current.applicationId.ToString());
#endif
                }
            }

            /// <summary>
            /// Logs the Steam Game Server interface on to the Steam backend according to the settings provided
            /// </summary>
            public void LogOn()
            {
                if (anonymousServerLogin)
                {
                    if (current.isDebugging)
                        Debug.Log("Logging on with Anonymous");

                    SteamGameServer.LogOnAnonymous();
                }
                else
                {
                    if (current.isDebugging)
                        Debug.Log("Logging on with token");

                    SteamGameServer.LogOn(gameServerToken);
                }

                // We want to actively update the master server with our presence so players can
                // find us via the steam matchmaking/server browser interfaces
                if (usingGameServerAuthApi || enableHeartbeats)
                {
                    if (current.isDebugging)
                        Debug.Log("Enabling server heartbeat.");

                    SteamGameServer.SetAdvertiseServerActive(true);
                }

                Debug.Log("Steamworks Game Server Started.\nWaiting for connection result from Steamworks");
            }

            public void RegisterCallbacks()
            {
                steamServerConnectFailure = Callback<SteamServerConnectFailure_t>.CreateGameServer(OnSteamServerConnectFailure);
                steamServersConnected = Callback<SteamServersConnected_t>.CreateGameServer(OnSteamServersConnected);
                steamServersDisconnected = Callback<SteamServersDisconnected_t>.CreateGameServer(OnSteamServersDisconnected);
            }
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
            internal void Init()
            {
#if !UNITY_EDITOR // || true
                try
                {
                    // If Steamworks is not running or the game wasn't started through Steamworks, SteamAPI_RestartAppIfNecessary starts the
                    // Steamworks client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.
                    if (Steamworks.SteamAPI.RestartAppIfNecessary(current.applicationId))
                    {
                        Application.Quit();
                        return;
                    }
                }
                catch (System.DllNotFoundException e)
                {
                    HasInitalizationError = true;
                    InitalizationErrorMessage = "Steamworks.NET could not load, steam_api.dll/so/dylib. It's likely not in the correct location.";
                    // We catch this exception here, as it will be the first occurence of it.
                    Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, current);
                    current.evtSteamInitializationError.Invoke("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e);
                    Application.Quit();
                    return;
                }
#else
                if (!SteamAPI.IsSteamRunning())
                {
                    HasInitalizationError = true;
                    InitalizationErrorMessage = "Steam Running check returned false, Steam client must be running for the API to intialize.";
                    current.evtSteamInitializationError.Invoke(InitalizationErrorMessage);
                    Debug.LogError("[Steamworks.NET] Steam client must be running for the API to intialize.", current);
                    return;
                }
#endif

                if (current.isDebugging)
                    Debug.Log("Initalizing Steam Client API");
                Initialized = Steamworks.SteamAPI.Init();

                if (!Initialized)
                {
                    HasInitalizationError = true;
                    InitalizationErrorMessage = "The Steam client isn't running. A running Steam client is required to provide implementations of the various Steamworks interfaces.\n"
                            + "The Steam client couldn't determine the App ID of game, this most commonly occures when running the game outside of Steam client.\n"
                            + "Your application is not running under the same OS user context as the Steam client, such as a different user or administration access level.\n"
                            + "Ensure that you own a license for the App ID on the currently active Steam account. Your game must show up in your Steam library.\n"
                            + "Your App ID is not completely set up, i.e. in Release State: Unavailable, or it's missing default packages.";

                    current.evtSteamInitializationError.Invoke(InitalizationErrorMessage);
                    Debug.LogError(InitalizationErrorMessage);
                    return;
                }

                API.StatsAndAchievements.Client.RequestCurrentStats();

                if (current.isDebugging)
                {
                    Debug.Log("Steam API has been initialized with App ID: " + SteamUtils.GetAppID());
                }

                if (current.applicationId != SteamUtils.GetAppID())
                {
#if UNITY_EDITOR
                    Debug.LogWarning("The reported applicaiton ID of " + SteamUtils.GetAppID().ToString() + " does not match the anticipated ID of " + current.applicationId.ToString() + ". This is most frequently caused when you edit your AppID bu fail to restart Unity, Visual Studio and or any other processes that may have mounted the Steam API under the previous App ID. To correct this please insure your AppID is entered correctly in the SteamSettings object and that you fully restart the Unity Editor, Visual Studio and any other processes that may have connectd to them.");
#else
                    Debug.LogError("The reported AppId is not as expected:\nAppId Reported = " + SteamUtils.GetAppID().ToString() + "AppId Expected = " + current.applicationId.ToString());
#endif
                }
            }
        }
        #endregion

        #region Static Access
        public static void Unload()
        {
            Initialized = false;
            HasInitalizationError = false;
            InitalizationErrorMessage = string.Empty;

            API.Friends.Client.UnloadAvatarImages();
        }

        /// <summary>
        /// A reference to the initialized <see cref="SteamSettings"/> object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value gets set when the <see cref="Init"/> method is called which should be done by the <see cref="SteamworksBehaviour"/>. Note that your app should have 1 <see cref="SteamworksBehaviour"/> defined in a scene that is loaded once and is never reloaded, that is you should not put the <see cref="SteamworksBehaviour"/> on a menu scene that will be reloaded multiple times during your games session life as this will break events and other features of the Steamworks API.
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
        public static SteamSettings current;
        public static SteamworksBehaviour behaviour;
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
        ///         Debug.Log("Configred to run as " + SystemSettings.ApplicationId + ", actually running as " + SystemSettings.GetAppId() );
        ///     }
        /// }
        /// </code>
        /// </example>
        public static AppId_t ApplicationId => current.applicationId;

        /// <summary>
        /// Indicates an error with API intializaiton
        /// </summary>
        /// <remarks>
        /// If true than an error occured during the initalization of the Steamworks API and normal funcitonality will not be possible.
        /// </remarks>
        public static bool HasInitalizationError { get; private set; }

        /// <summary>
        /// Initalization error message if any
        /// </summary>
        /// <remarks>
        /// See <see cref="HasInitalizationError"/> to determin if an error occured, if so this message will discribe possible causes.
        /// </remarks>
        public static string InitalizationErrorMessage { get; private set; }

        /// <summary>
        /// Indicates rather or not the Steamworks API is initialized
        /// </summary>
        /// <remarks>
        /// <para>This value gets set to true when <see cref="Init"/> is called by the <see cref="SteamworksClientApiSystem"/>.
        /// Note that if Steamworks API fails to initialize such as if the Steamworks client is not installed, running and logged in with a valid Steamworks user then the call to Init will fail and the <see cref="Initialized"/> value will remain false.</para>
        /// </remarks>
        public static bool Initialized { get; private set; }

        /// <summary>
        /// Static access to the active <see cref="GameClient"/> object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="GameClient"/> object provides easy access to client specifc functions such as the <see cref="Steam.UserData"/> for the local user ... you can access this via.
        /// <code>
        /// SteamSettings.Client.user
        /// </code>
        /// or you can fetch the <see cref="Steam.UserData"/> for any given user via code such as
        /// <code>
        /// SteamSettings.Client.GetUserData(ulong userId)
        /// </code>
        /// For more information please see the documentaiton on the <see cref="GameClient"/> object.
        /// </para>
        /// </remarks>
        public static GameClient Client => current.client;

        /// <summary>
        /// Static access to the active <see cref="GameServer"/> object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="GameServer"/> object provides easy access to Steamworks Game Server configuraiton and server only features.
        /// Note that your server can be a Steamworks Game Server and not have to use the Steamworks Networking transports ... e.g. you can use any transport you like and host anywhere you like.
        /// Being a Steamworks Game Server simply means that your server has initialized the Steamworks API and registered its self against Valve's backend ... in addition if this server has an  IP address of a trusted server as defined in your app configuration on the Steamworks Portal,
        /// then it may perform GS only actions such as setting stats and achievments that are marked as GS only.
        /// </para>
        /// </remarks>
        public static GameServer Server => current.server;

        /// <summary>
        /// The list of achievements registered for this applicaiton.
        /// </summary>
        /// <remarks>
        /// See <see cref="achievements"/> for more information. This field simply access the <see cref="achievements"/> member for the <see cref="current"/> <see cref="SteamworksClientApiSettings"/> object.
        /// </remarks>
        public static List<AchievementObject> Achievements => current.achievements;

        /// <summary>
        /// The list of stats registered for this applicaiton.
        /// </summary>
        /// <remarks>
        /// See <see cref="stats"/> for more information. This field simply access the <see cref="stats"/> member for the <see cref="current"/> <see cref="SteamworksClientApiSettings"/> object.
        /// </remarks>
        public static List<StatObject> Stats => current.stats;
        #endregion

        #region Utility Functions

        /// <summary>
        /// Checks if the Steam API is initialized and if not it will create a new Steamworks Behaviour object configure it with the settings and initialize
        /// </summary>
        /// <remarks>
        /// This should only be used in the rare cases you need to initialize Steam API on demand. In a typical araingment you would defiine the Steamworks Beahviour at developer time in the Unity Editor as part of a scene that is only ever loaded once.
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
        /// This should only be used in the rare cases you need to initialize Steam API on demand. In a typical araingment you would defiine the Steamworks Beahviour at developer time in the Unity Editor as part of a scene that is only ever loaded once.
        /// </remarks>
        /// <param name="doNotDestroy">Optionally mark the created Steamworks Behaviour object as Do Not Destroy On Load</param>
        public static void CreateBeahviour(SteamSettings settings, bool doNotDestroy = false) => settings.CreateBehaviour(doNotDestroy);
        #endregion

        #region Instanced Members
        /// <summary>
        /// The current applicaiton ID
        /// </summary>
        /// <remarks>
        /// <para>It is importnat that this is set to your game's appId.
        /// Note that when working in Unity Editor you need to change this value in the <see cref="SteamworksClientApiSettings"/> object your using but also in the steam_appid.txt file located in the root of your project.
        /// You can read more about the steam_appid.txt file here <a href="https://heathen-engineering.mn.co/posts/steam_appidtxt"/></para>
        /// </remarks>
        public AppId_t applicationId = new AppId_t(0x0);

        /// <summary>
        /// Used in various processes to determin the level of detail to log
        /// </summary>
        public bool isDebugging = false;

        /// <summary>
        /// Contains server side funcitonality and is not available in client builds
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
        /// Contains client side funcitonality
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
        /// The registered stats assoceated with this configuration
        /// </summary>
        /// <remarks>
        /// This collection is used by Valve callbacks to match incoming stat updates and records the value against the approprete stat.
        /// Put more simply if a stat is listed here then the system will update that <see cref="StatObject"/> object with score changes as that information comes in from the Valve backend insuring that these <see cref="StatObject"/> objects are an up to date snap shot of the local user's stat value.
        /// For servers these objects simplify fetching and settting stat values for targeted users but of course dosn't cashe values for a local user since server's have no local user.
        /// </remarks>
        public List<StatObject> stats = new List<StatObject>();

        /// <summary>
        /// The registered achievements assoceated with this configuration
        /// </summary>
        /// <remarks>
        /// This collection is used by Valve callbacks to match incoming stat updates and records the value against the approprete achievement.
        /// Put more simply if a stat is listed here then the system will update that <see cref="SteamAchievementData"/> object with state changes as that information comes in from the Valve backend insuring that these <see cref="AchievementObject"/> objects are an up to date snap shot of the local user's achievement value.
        /// For servers these objects simplify fetching and settting stat values for targeted users but of course dosn't cashe values for a local user since server's have no local user.
        /// </remarks>
        public List<AchievementObject> achievements = new List<AchievementObject>();

        #endregion

        #region Events
        public UnityEvent evtSteamInitialized;

        public UnityStringEvent evtSteamInitializationError;
        #endregion

        #region Internals
        /// <summary>
        /// Initalization logic for the Steam API
        /// </summary>
        public void Init()
        {
            if (Initialized)
            {
                HasInitalizationError = true;
                InitalizationErrorMessage = "Tried to initialize the Steamworks API twice in one session!";
                evtSteamInitializationError.Invoke(InitalizationErrorMessage);
                Debug.LogWarning(InitalizationErrorMessage);
                return;
            }

            if (!Packsize.Test())
            {
                HasInitalizationError = true;
                InitalizationErrorMessage = "Packesize Test returned false, the wrong version of the Steamowrks.NET is being run in this platform.";
                evtSteamInitializationError.Invoke(InitalizationErrorMessage);
                Debug.LogError(InitalizationErrorMessage);
                return;
            }

            if (!DllCheck.Test())
            {
                HasInitalizationError = true;
                InitalizationErrorMessage = "DLL Check Test returned false, one or more of the Steamworks binaries seems to be the wrong version.";
                evtSteamInitializationError.Invoke(InitalizationErrorMessage);
                Debug.LogError(InitalizationErrorMessage);
                return;
            }

            current = this;

#if !UNITY_SERVER
            client.Init();
#else
            server.Init();
#endif
            Application.quitting += Application_quitting;

            if (Initialized)
                evtSteamInitialized.Invoke();
            else
            {
                HasInitalizationError = true;
                InitalizationErrorMessage = "Steam Initalization failed, check the log for more information.";
                evtSteamInitializationError.Invoke(InitalizationErrorMessage);
                Debug.LogError("[Steamworks.NET] Steam Initalization failed, check the log for more information", this);
            }
        }

        private void Application_quitting()
        {
            if (!Initialized)
            {
                return;
            }

#if !UNITY_SERVER
            SteamAPI.Shutdown();
#else
            Steamworks.GameServer.Shutdown();
#endif
            Unload();
        }
        #endregion
    }
}
#endif