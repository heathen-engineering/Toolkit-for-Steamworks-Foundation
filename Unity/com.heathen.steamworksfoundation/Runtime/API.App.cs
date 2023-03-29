#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
//#define UNITY_SERVER
using HeathenEngineering.Events;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace HeathenEngineering.SteamworksIntegration.API
{
    /// <summary>
    /// Exposes a wide range of information and actions for applications and Downloadable Content (DLC).
    /// </summary>
    /// <remarks>
    /// https://partner.steamgames.com/doc/api/ISteamApps
    /// </remarks>
    public static class App
    {
        #region Global
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RunTimeInit()
        {
            evtSteamInitialized = new UnityEvent();
            evtSteamInitializationError = new UnityStringEvent();
            m_SteamAPIWarningMessageHook = null;

            Server.eventDisconnected = new Server.DisconnectedEvent();
            Server.eventConnected = new Server.ConnectedEvent();
            Server.eventFailure = new Server.FailureEvent();

            Server.steamServerConnectFailure = null;
            Server.steamServersConnected = null;
            Server.steamServersDisconnected = null;

            Server.Configuration = default;

            if (callbackWaitThread != null)
            {
                if (callbackWaitThread.IsBusy)
                {
                    callbackWaitThread.RunWorkerCompleted -= CallbackWaitThread_RunWorkerCompleted;
                    callbackWaitThread.ProgressChanged -= CallbackWaitThread_ProgressChanged;

                    callbackWaitThread.CancelAsync();
                    callbackWaitThread.Dispose();
                }

                callbackWaitThread = null;
            }
        }

        private static void Application_quitting()
        {
            if (!Initialized)
            {
                return;
            }

            if (callbackWaitThread != null)
            {
                if (callbackWaitThread.IsBusy)
                {
                    callbackWaitThread.RunWorkerCompleted -= CallbackWaitThread_RunWorkerCompleted;
                    callbackWaitThread.ProgressChanged -= CallbackWaitThread_ProgressChanged;
                    callbackWaitThread.CancelAsync();
                }

                callbackWaitThread.Dispose();
                callbackWaitThread = null;
            }

#if !UNITY_SERVER
            SteamAPI.Shutdown();
#else
            if (Server.Configuration.usingGameServerAuthApi)
                SteamGameServer.SetAdvertiseServerActive(false);

            SteamGameServer.LogOff();

            Steamworks.GameServer.Shutdown();
#endif
            Unload();
        }

        public static void Unload()
        {
            Initialized = false;
            HasInitalizationError = false;
            InitalizationErrorMessage = string.Empty;

            API.Friends.Client.UnloadAvatarImages();
        }

        public static bool Initialized { get; private set; }
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
        public static int callbackTick_Milliseconds = 15;
        public static bool isDebugging = false;
        public static UnityEvent evtSteamInitialized = new UnityEvent();
        public static UnityStringEvent evtSteamInitializationError = new UnityStringEvent();

        private static BackgroundWorker callbackWaitThread = null;

        private static void CallbackWaitThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
        private static void CallbackWaitThread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
#if !UNITY_SERVER
            Steamworks.SteamAPI.RunCallbacks();
#else
            Steamworks.GameServer.RunCallbacks();
#endif
        }
        #endregion

        public static AppData Id => SteamUtils.GetAppID();

        private static SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
        [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
        private static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
        {
            Debug.LogWarning(pchDebugText);
        }

        public static class Client
        {
            [Serializable]
            public class UnityEventServersDisconnected : UnityEvent<EResult>
            { }

            [Serializable]
            public class UnityEventServersConnectFailure : UnityEvent<SteamServerConnectFailure>
            { }

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                m_FileDetailResult_t = null;
                m_DlcInstalled_t = null;
                m_NewUrlLaunchParameters_t = null;

                eventDlcInstalled = new DlcInstalledEvent();
                eventNewUrlLaunchParameters = new NewUrlLaunchParametersEvent();
                eventServersConnected = new UnityEvent();
                eventServersDisconnected = new UnityEventServersDisconnected();
            }

            public static bool LoggedOn => Initialized && SteamUser.BLoggedOn();

            public static void Initialize(AppData appId) 
            {
                if (Initialized)
                {
                    HasInitalizationError = true;
                    InitalizationErrorMessage = "Tried to initialize the Steamworks API twice in one session, operation aborted!";
                    evtSteamInitializationError.Invoke(InitalizationErrorMessage);
                    Debug.LogWarning(InitalizationErrorMessage);
                }
                else if (!Packsize.Test())
                {
                    HasInitalizationError = true;
                    InitalizationErrorMessage = "Packesize Test returned false, the wrong version of the Steamowrks.NET is being run in this platform.";
                    evtSteamInitializationError.Invoke(InitalizationErrorMessage);
                    Debug.LogError(InitalizationErrorMessage);
                }
                else if (!DllCheck.Test())
                {
                    HasInitalizationError = true;
                    InitalizationErrorMessage = "DLL Check Test returned false, one or more of the Steamworks binaries seems to be the wrong version.";
                    evtSteamInitializationError.Invoke(InitalizationErrorMessage);
                    Debug.LogError(InitalizationErrorMessage);
                }
                else
                {
#if !UNITY_SERVER
#if !UNITY_EDITOR //|| true
                    // If Steamworks is not running or the game wasn't started through Steamworks, SteamAPI_RestartAppIfNecessary starts the
                    // Steamworks client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.
                    if (Steamworks.SteamAPI.RestartAppIfNecessary(appId))
                    {
                        Application.Quit();
                    }
#else
                    if (!SteamAPI.IsSteamRunning())
                    {
                        HasInitalizationError = true;
                        InitalizationErrorMessage = "Steam Running check returned false, Steam client must be running for the API to intialize.";
                        evtSteamInitializationError.Invoke(InitalizationErrorMessage);
                        Debug.LogError("[Steamworks.NET] Steam client must be running for the API to intialize.");
                    }
#endif
                    else
                    {
                        if (isDebugging)
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

                            evtSteamInitializationError.Invoke(InitalizationErrorMessage);
                            Debug.LogError(InitalizationErrorMessage);
                        }
                        else
                        {
                            //Get the local user's data refreshed in the case, Steam claims Steam Level is instant ... it is not
                            API.Friends.Client.RequestUserInformation(UserData.Me, false);

                            Debug.Log($"Local User: {UserData.Me.Name}:{UserData.Me.Level}");

                            if (m_SteamAPIWarningMessageHook == null)
                            {
                                // Set up our callback to recieve warning messages from Steamworks.
                                // You must launch with "-debug_steamapi" in the launch args to recieve warnings.
                                m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
                                SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
                            }

                            if (!SteamUser.BLoggedOn())
                            {
                                Debug.LogWarning("Steam API was able to initalize however the user does not have an acitve logon; no real-time services provided by the Steamworks API will be enabled. The Steam client will automatically be trying to recreate the connection as often as possible. When the connection is restored a API.App.Client.EvenServersConnected event will be posted.");
                            }

                            API.StatsAndAchievements.Client.RequestCurrentStats();

                            if (isDebugging)
                            {
                                Debug.Log("Steam API has been initialized with App ID: " + SteamUtils.GetAppID());
                            }

                            if (appId != SteamUtils.GetAppID())
                            {
#if UNITY_EDITOR
                                Debug.LogWarning($"The reported applicaiton ID of {SteamUtils.GetAppID()} does not match the anticipated ID of {appId}. This is most frequently caused when you edit your AppID but fail to restart Unity, Visual Studio and or any other processes that may have mounted the Steam API under the previous App ID. To correct this please insure your AppID is entered correctly in the SteamSettings object and that you fully restart the Unity Editor, Visual Studio and any other processes that may have connectd to them.");
#else
                            Debug.LogError($"The reported AppId is not as expected:\ntAppId Reported = {SteamUtils.GetAppID()}\n\tAppId Expected = {appId}");
                            Application.Quit();
#endif
                            }
                        }
                    }
#else
                    
#endif
                    Application.quitting += Application_quitting;

                    if (Initialized)
                    {
                        if (callbackWaitThread == null)
                        {
                            callbackWaitThread = new BackgroundWorker();
                            callbackWaitThread.WorkerSupportsCancellation = true;
                            callbackWaitThread.WorkerReportsProgress = true;
                            callbackWaitThread.DoWork += (p, e) =>
                            {
                                while (true)
                                {
                                    Thread.Sleep(callbackTick_Milliseconds);
                                    callbackWaitThread.ReportProgress(1);
                                }
                            };
                            callbackWaitThread.RunWorkerCompleted += CallbackWaitThread_RunWorkerCompleted;
                            callbackWaitThread.ProgressChanged += CallbackWaitThread_ProgressChanged;
                        }

                        callbackWaitThread.RunWorkerAsync();
                        Web.LoadAppNames(null);

                        evtSteamInitialized.Invoke();
                    }
                    else
                    {
                        HasInitalizationError = true;
                        InitalizationErrorMessage = "Steam Initalization failed, check the log for more information.";
                        evtSteamInitializationError.Invoke(InitalizationErrorMessage);
                        Debug.LogError("[Steamworks.NET] Steam Initalization failed, check the log for more information");
                    }
                }
            }

            /// <summary>
            /// Triggered after the current user gains ownership of DLC and that DLC is installed.
            /// </summary>
            public static DlcInstalledEvent EventDlcInstalled
            {
                get
                {
                    if (m_DlcInstalled_t == null)
                        m_DlcInstalled_t = Callback<DlcInstalled_t>.Create(eventDlcInstalled.Invoke);

                    return eventDlcInstalled;
                }
            }

            /// <summary>
            /// Posted after the user executes a steam url with command line or query parameters such as steam://run/<appid>//?param1=value1;param2=value2;param3=value3; while the game is already running. The new params can be queried with GetLaunchCommandLine and GetLaunchQueryParam.
            /// </summary>
            public static NewUrlLaunchParametersEvent EventNewUrlLaunchParameters
            {
                get
                {
                    if (m_NewUrlLaunchParameters_t == null)
                        m_NewUrlLaunchParameters_t = Callback<NewUrlLaunchParameters_t>.Create(eventNewUrlLaunchParameters.Invoke);

                    return eventNewUrlLaunchParameters;
                }
            }

            public static UnityEvent EventServersConnected
            {
                get
                {
                    if (m_SteamServersConnected_t == null)
                        m_SteamServersConnected_t = Callback<SteamServersConnected_t>.Create((connected) =>
                        {
                            eventServersConnected?.Invoke();
                        });

                    return eventServersConnected;
                }
            }

            public static UnityEventServersDisconnected EventServersDisconnected
            {
                get
                {
                    if (m_SteamServersConnected_t == null)
                        m_SteamServersDisconnected_t = Callback<SteamServersDisconnected_t>.Create((connected) =>
                        {
                            eventServersDisconnected?.Invoke(connected.m_eResult);
                        });

                    return eventServersDisconnected;
                }
            }

            public static UnityEventServersConnectFailure EventServersConnectFailure
            {
                get
                {
                    if (m_SteamServerConnectFailure_t == null)
                        m_SteamServerConnectFailure_t = Callback<SteamServerConnectFailure_t>.Create((connected) =>
                        {
                            eventServersConnectFailure?.Invoke(connected);
                        });

                    return eventServersConnectFailure;
                }
            }

            private static DlcInstalledEvent eventDlcInstalled = new DlcInstalledEvent();
            private static NewUrlLaunchParametersEvent eventNewUrlLaunchParameters = new NewUrlLaunchParametersEvent();
            private static UnityEvent eventServersConnected = new UnityEvent();
            private static UnityEventServersDisconnected eventServersDisconnected = new UnityEventServersDisconnected();
            private static UnityEventServersConnectFailure eventServersConnectFailure = new UnityEventServersConnectFailure();

            private static CallResult<FileDetailsResult_t> m_FileDetailResult_t;
            private static Callback<DlcInstalled_t> m_DlcInstalled_t;
            private static Callback<NewUrlLaunchParameters_t> m_NewUrlLaunchParameters_t;
            private static Callback<SteamServerConnectFailure_t> m_SteamServerConnectFailure_t;
            private static Callback<SteamServersConnected_t> m_SteamServersConnected_t;
            private static Callback<SteamServersDisconnected_t> m_SteamServersDisconnected_t;

            /// <summary>
            /// Checks if the active user is subscribed to the current App ID.
            /// </summary>
            /// <remarks>
            /// NOTE: This will always return true if you're using Steam DRM or calling SteamAPI_RestartAppIfNecessary.
            /// </remarks>
            public static bool IsSubscribed => SteamApps.BIsSubscribed();
            /// <summary>
            /// Checks if the active user is accessing the current appID via a temporary Family Shared license owned by another user.
            /// </summary>
            public static bool IsSubscribedFromFamilySharing => SteamApps.BIsSubscribedFromFamilySharing();
            /// <summary>
            /// Checks if the user is subscribed to the current App ID through a free weekend.
            /// </summary>
            public static bool IsSubscribedFromFreeWeekend => SteamApps.BIsSubscribedFromFreeWeekend();
            /// <summary>
            /// Checks if the user has a VAC ban on their account
            /// </summary>
            public static bool IsVACBanned => SteamApps.BIsVACBanned();
            /// <summary>
            /// Gets the Steam ID of the original owner of the current app. If it's different from the current user then it is borrowed.
            /// </summary>
            public static UserData Owner => SteamApps.GetAppOwner();
            /// <summary>
            /// Returns a list of languages supported by the app
            /// </summary>
            public static string[] AvailableLanguages
            {
                get
                {
                    var list = SteamApps.GetAvailableGameLanguages();
                    return list.Split(',');
                }
            }
            /// <summary>
            /// Returns true if a beta branch is being used
            /// </summary>
            public static bool IsBeta => SteamApps.GetCurrentBetaName(out string _, 128);
            /// <summary>
            /// Returns the name of the beta branch being used if any
            /// </summary>
            public static string CurrentBetaName
            {
                get
                {
                    if (SteamApps.GetCurrentBetaName(out string name, 512))
                        return name;
                    else
                        return string.Empty;
                }
            }
            /// <summary>
            /// Gets the current language that the user has set
            /// </summary>
            public static string CurrentGameLanguage => SteamApps.GetCurrentGameLanguage();
            /// <summary>
            /// Returns the metadata for all available DLC
            /// </summary>
            public static DlcData[] Dlc
            {
                get
                {
                    var count = SteamApps.GetDLCCount();
                    if (count > 0)
                    {
                        var result = new DlcData[count];
                        for (int i = 0; i < count; i++)
                        {
                            if (SteamApps.BGetDLCDataByIndex(i, out AppId_t appid, out bool available, out string name, 512))
                            {
                                result[i] = new DlcData(appid, available, name);
                            }
                            else
                            {
                                Debug.LogWarning("Failed to fetch DLC at index [" + i.ToString() + "]");
                            }
                        }
                        return result;
                    }
                    else
                        return new DlcData[0];
                }
            }
            /// <summary>
            /// Checks whether the current App ID is for Cyber Cafes.
            /// </summary>
            public static bool IsCybercafe => SteamApps.BIsCybercafe();
            /// <summary>
            /// Checks if the license owned by the user provides low violence depots.
            /// </summary>
            public static bool IsLowViolence => SteamApps.BIsLowViolence();
            /// <summary>
            /// Gets the App ID of the current process.
            /// </summary>
            public static AppId_t Id => SteamUtils.GetAppID();
            /// <summary>
            /// Gets the buildid of this app, may change at any time based on backend updates to the game.
            /// </summary>
            public static int BuildId => SteamApps.GetAppBuildId();
            /// <summary>
            /// Gets the install folder for a specific AppID.
            /// </summary>
            public static string InstallDirectory
            {
                get
                {
                    SteamApps.GetAppInstallDir(SteamUtils.GetAppID(), out string folder, 2048);
                    return folder;
                }
            }
            /// <summary>
            /// Gets the number of DLC pieces for the current app.
            /// </summary>
            public static int DLCCount => SteamApps.GetDLCCount();
            /// <summary>
            /// Gets the command line if the game was launched via Steam URL, e.g. steam://run/&lt;appid&gt;//&lt;command line&gt;/. This method is preferable to launching with a command line via the operating system, which can be a security risk. In order for rich presence joins to go through this and not be placed on the OS command line, you must enable "Use launch command line" from the Installation &gt; General page on your app.
            /// </summary>
            public static string LaunchCommandLine
            {
                get
                {
                    if (
                SteamApps.GetLaunchCommandLine(out string commandline, 512) > 0)
                        return commandline;
                    else
                        return string.Empty;
                }
            }

            /// <summary>
            /// Checks if a specific app is installed.
            /// </summary>
            /// <remarks>
            /// The app may not actually be owned by the current user, they may have it left over from a free weekend, etc.
            /// This only works for base applications, not Downloadable Content(DLC). Use IsDlcInstalled for DLC instead.
            /// </remarks>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static bool IsAppInstalled(AppData appId) => SteamApps.BIsAppInstalled(appId);
            /// <summary>
            /// Checks if the user owns a specific DLC and if the DLC is installed
            /// </summary>
            /// <param name="appId">The App ID of the DLC to check.</param>
            /// <returns></returns>
            public static bool IsDlcInstalled(AppData appId) => SteamApps.BIsDlcInstalled(appId);
            /// <summary>
            /// Gets the download progress for optional DLC.
            /// </summary>
            /// <param name="appId"></param>
            /// <param name="bytesDownloaded"></param>
            /// <param name="bytesTotal"></param>
            /// <returns></returns>
            public static bool GetDlcDownloadProgress(AppData appId, out ulong bytesDownloaded, out ulong bytesTotal) => SteamApps.GetDlcDownloadProgress(appId, out bytesDownloaded, out bytesTotal);
            /// <summary>
            /// Gets the install directory of the app if any
            /// </summary>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static string GetAppInstallDirectory(AppData appId)
            {
                SteamApps.GetAppInstallDir(appId, out string folder, 2048);
                return folder;
            }
            /// <summary>
            /// Returns the collection of installed depots in mount order
            /// </summary>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static DepotId_t[] InstalledDepots(AppData appId)
            {
                var results = new DepotId_t[256];
                var count = SteamApps.GetInstalledDepots(appId, results, 256);
                Array.Resize(ref results, (int)count);
                return results;
            }
            /// <summary>
            /// Parameter names starting with the character '@' are reserved for internal use and will always return an empty string. Parameter names starting with an underscore '_' are reserved for steam features -- they can be queried by the game, but it is advised that you not param names beginning with an underscore for your own features.
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public static string QueryLaunchParam(string key) => SteamApps.GetLaunchQueryParam(key);
            /// <summary>
            /// Install an optional DLC
            /// </summary>
            /// <param name="appId"></param>
            public static void InstallDLC(AppData appId) => SteamApps.InstallDLC(appId);
            /// <summary>
            /// Uninstall an optional DLC
            /// </summary>
            /// <param name="appId"></param>
            public static void UninstallDLC(AppData appId) => SteamApps.UninstallDLC(appId);
            /// <summary>
            /// Checks if the active user is subscribed to a specified appId.
            /// </summary>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static bool IsSubscribedApp(AppData appId) => SteamApps.BIsSubscribedApp(appId);
            public static bool IsTimedTrial(out uint secondsAllowed, out uint secondsPlayed) => SteamApps.BIsTimedTrial(out secondsAllowed, out secondsPlayed);
            /// <summary>
            /// Gets the current beta branch name if any
            /// </summary>
            /// <param name="name">outputs the name of the current beta branch if any</param>
            /// <returns>True if the user is running from a beta branch</returns>
            public static bool GetCurrentBetaName(out string name) => SteamApps.GetCurrentBetaName(out name, 512);
            /// <summary>
            /// Gets the time of purchase of the specified app
            /// </summary>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static DateTime GetEarliestPurchaseTime(AppData appId)
            {
                var secondsSince1970 = SteamApps.GetEarliestPurchaseUnixTime(appId);
                return new DateTime(1970, 1, 1).AddSeconds(secondsSince1970);
            }
            /// <summary>
            /// Asynchronously retrieves metadata details about a specific file in the depot manifest.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="callback"></param>
            public static void GetFileDetails(string name, Action<FileDetailsResult, bool> callback)
            {
                if (callback == null)
                    return;

                if (m_FileDetailResult_t == null)
                    m_FileDetailResult_t = CallResult<FileDetailsResult_t>.Create();

                var handle = SteamApps.GetFileDetails(name);
                m_FileDetailResult_t.Set(handle, (r,e) => { callback.Invoke(r, e); });
            }
            /// <summary>
            /// If you detect the game is out-of-date (for example, by having the client detect a version mismatch with a server), you can call use MarkContentCorrupt to force a verify, show a message to the user, and then quit.
            /// </summary>
            /// <param name="missingFilesOnly"></param>
            /// <returns></returns>
            public static bool MarkContentCorrupt(bool missingFilesOnly) => SteamApps.MarkContentCorrupt(missingFilesOnly);

        }

        public static class Server
        {
            public static CSteamID ID => SteamGameServer.GetSteamID();
            public static bool LoggedOn { get; private set; }
            public static SteamGameServerConfiguraiton Configuration { get; internal set; }
            [Serializable]
            public class DisconnectedEvent : UnityEvent<SteamServersDisconnected> { }

            [Serializable]
            public class ConnectedEvent : UnityEvent<SteamServersConnected_t> { }

            [Serializable]
            public class FailureEvent : UnityEvent<SteamServerConnectFailure> { }

            public static DisconnectedEvent eventDisconnected = new DisconnectedEvent();
            public static ConnectedEvent eventConnected = new ConnectedEvent();
            public static FailureEvent eventFailure = new FailureEvent();

            internal static Callback<SteamServerConnectFailure_t> steamServerConnectFailure;
            internal static Callback<SteamServersConnected_t> steamServersConnected;
            internal static Callback<SteamServersDisconnected_t> steamServersDisconnected;

            private static void OnSteamServersDisconnected(SteamServersDisconnected_t param)
            {
                LoggedOn = false;
                if (isDebugging)
                    Debug.LogError("Steamworks.GameServer reported connection Closed: " + param.m_eResult.ToString());
                eventDisconnected.Invoke(param);
            }

            private static void OnSteamServersConnected(SteamServersConnected_t param)
            {
                LoggedOn = true;

                if (isDebugging)
                    Debug.Log($"Game Server connected to Steamworks successfully!" +
                        $"\n\tMod Directory = {Configuration.gameDirectory}" +
                        $"\n\tApplicaiton ID = {SteamGameServerUtils.GetAppID()}" +
                        $"\n\tServer ID = {SteamGameServer.GetSteamID()}" +
                        $"\n\tServer Name = {Configuration.serverName}" +
                        $"\n\tGame Description = {Configuration.gameDescription}" +
                        $"\n\tMax Player Count = {Configuration.maxPlayerCount}");

                SendUpdatedServerDetailsToSteam();
                eventConnected.Invoke(param);
            }

            private static void OnSteamServerConnectFailure(SteamServerConnectFailure_t param)
            {
                LoggedOn = false;
                if (isDebugging)
                    Debug.LogError("Steamworks.GameServer.LogOn reported connection Failure: " + param.m_eResult.ToString());
                eventFailure.Invoke(param);
            }

            public static void Initialize(AppData appId, SteamGameServerConfiguraiton serverConfiguration)
            {
                if (Initialized)
                {
                    HasInitalizationError = true;
                    InitalizationErrorMessage = "Tried to initialize the Steamworks API twice in one session, operation aborted!";
                    evtSteamInitializationError.Invoke(InitalizationErrorMessage);
                    Debug.LogWarning(InitalizationErrorMessage);
                }
                else if (!Packsize.Test())
                {
                    HasInitalizationError = true;
                    InitalizationErrorMessage = "Packesize Test returned false, the wrong version of the Steamowrks.NET is being run in this platform.";
                    evtSteamInitializationError.Invoke(InitalizationErrorMessage);
                    Debug.LogError(InitalizationErrorMessage);
                }
                else if (!DllCheck.Test())
                {
                    HasInitalizationError = true;
                    InitalizationErrorMessage = "DLL Check Test returned false, one or more of the Steamworks binaries seems to be the wrong version.";
                    evtSteamInitializationError.Invoke(InitalizationErrorMessage);
                    Debug.LogError(InitalizationErrorMessage);
                }
                else
                {

                    Configuration = serverConfiguration;

                    if (isDebugging)
                        Debug.Log("Registering Steam Game Server callbacks.");

                    RegisterCallbacks();

                    EServerMode eMode = EServerMode.eServerModeNoAuthentication;

                    if (serverConfiguration.usingGameServerAuthApi)
                        eMode = EServerMode.eServerModeAuthenticationAndSecure;

                    if (isDebugging)
                        Debug.Log("Initalizing Steam Game Server API: (" + serverConfiguration.ip + ", " + serverConfiguration.gamePort.ToString() + ", " + serverConfiguration.queryPort.ToString() + ", " + eMode.ToString() + ", " + serverConfiguration.serverVersion + ")");

                    Initialized = Steamworks.GameServer.Init(serverConfiguration.ip, serverConfiguration.gamePort, serverConfiguration.queryPort, eMode, serverConfiguration.serverVersion);

                    if (!Initialized)
                    {
                        HasInitalizationError = true;
                        InitalizationErrorMessage = "Steam API failed to initialize!\nOne of the following issues must be true:\n"
                                + "The Steam couldn't determine the App ID of the game. If you're running your server from the executable or debugger directly then you must have a steam_appid.txt in your server directory next to the executable, with your app ID in it and nothing else. Steam will look for this file in the current working directory. If you are running your executable from a different directory you may need to relocate the steam_appid.txt file.\n"
                                + "Ensure that you own a license for the App ID on the currently active Steam account (if login via token). Your game must show up in your Steam library.\n"
                                + "The App ID is not completely set up, i.e. in Release State: Unavailable, or it's missing default packages.";

                        evtSteamInitializationError.Invoke(InitalizationErrorMessage);
                        Debug.LogError(InitalizationErrorMessage);
                    }
                    else
                    {
                        if(isDebugging)
                        {
                            Debug.Log($"Applying Steam Game Server Settings:" +
                                $"\n\tSetModDir: {serverConfiguration.gameDirectory}" +
                                $"\n\tSetProduct: {appId}" +
                                $"\n\tSetGameDescription: {serverConfiguration.gameDescription}" +
                                $"\n\tSetMaxPlayerCount: {serverConfiguration.maxPlayerCount}" +
                                $"\n\tSetPasswordProtected: {serverConfiguration.isPasswordProtected}" +
                                $"\n\tSetServerName: {serverConfiguration.serverName}" +
                                $"\n\tSetBotPlayerCount: {serverConfiguration.botPlayerCount}" +
                                $"\n\tSetMapName: {serverConfiguration.mapName}" +
                                $"\n\tSetDedicatedServer: {serverConfiguration.isDedicated}");
                        }

                        SteamGameServer.SetModDir(serverConfiguration.gameDirectory);
                        SteamGameServer.SetProduct(appId.ToString());
                        SteamGameServer.SetGameDescription(serverConfiguration.gameDescription);
                        SteamGameServer.SetMaxPlayerCount(serverConfiguration.maxPlayerCount);
                        SteamGameServer.SetPasswordProtected(serverConfiguration.isPasswordProtected);
                        SteamGameServer.SetServerName(serverConfiguration.serverName);
                        SteamGameServer.SetBotPlayerCount(serverConfiguration.botPlayerCount);
                        SteamGameServer.SetMapName(serverConfiguration.mapName);
                        SteamGameServer.SetDedicatedServer(serverConfiguration.isDedicated);

                        if (serverConfiguration.supportSpectators)
                        {
                            if (isDebugging)
                                Debug.Log("Spectator enabled:\n\tName = " + serverConfiguration.spectatorServerName + "\n\tSpectator Port = " + serverConfiguration.spectatorPort.ToString());

                            SteamGameServer.SetSpectatorPort(serverConfiguration.spectatorPort);
                            SteamGameServer.SetSpectatorServerName(serverConfiguration.spectatorServerName);
                        }
                        else if (isDebugging)
                            Debug.Log("Spectator Set Up Skipped");

                        if (isDebugging)
                        {
                            Debug.Log("Steam API has been initialized with App ID: " + SteamGameServerUtils.GetAppID());
                        }

                        if (appId != SteamGameServerUtils.GetAppID())
                        {
#if UNITY_EDITOR
                            Debug.LogWarning($"The reported applicaiton ID of {SteamGameServerUtils.GetAppID()} does not match the anticipated ID of {appId}. This is most frequently caused when you edit your AppID bu fail to restart Unity, Visual Studio and or any other processes that may have mounted the Steam API under the previous App ID. To correct this please insure your AppID is entered correctly in the SteamSettings object and that you fully restart the Unity Editor, Visual Studio and any other processes that may have connectd to them.");
#else
                            Debug.LogError($"The reported AppId is not as expected:\nAppId Reported = {SteamGameServerUtils.GetAppID()}, AppId Expected = {appId}");
#endif
                        }
                    }

                    Application.quitting += Application_quitting;

                    if (Initialized)
                    {
                        if (callbackWaitThread == null)
                        {
                            callbackWaitThread = new BackgroundWorker();
                            callbackWaitThread.WorkerSupportsCancellation = true;
                            callbackWaitThread.WorkerReportsProgress = true;
                            callbackWaitThread.DoWork += (p,e) =>
                            {
                                while (true)
                                {
                                    Thread.Sleep(callbackTick_Milliseconds);
                                    callbackWaitThread.ReportProgress(1);
                                }
                            };
                            callbackWaitThread.RunWorkerCompleted += CallbackWaitThread_RunWorkerCompleted;
                            callbackWaitThread.ProgressChanged += CallbackWaitThread_ProgressChanged;
                        }

                        callbackWaitThread.RunWorkerAsync();

                        evtSteamInitialized.Invoke();

                        if (Configuration.autoLogon)
                            LogOn();
                    }
                    else
                    {
                        HasInitalizationError = true;
                        InitalizationErrorMessage = "Steam Initalization failed, check the log for more information.";
                        evtSteamInitializationError.Invoke(InitalizationErrorMessage);
                        Debug.LogError("[Steamworks.NET] Steam Initalization failed, check the log for more information");
                    }
                }
            }
            public static void LogOn()
            {
                if (Configuration.anonymousServerLogin)
                {
                    if (isDebugging)
                        Debug.Log("Logging on with Anonymous");

                    SteamGameServer.LogOnAnonymous();
                }
                else
                {
                    if (isDebugging)
                        Debug.Log("Logging on with token");

                    SteamGameServer.LogOn(Configuration.gameServerToken);
                }

                // We want to actively update the master server with our presence so players can
                // find us via the steam matchmaking/server browser interfaces
                if (Configuration.usingGameServerAuthApi || Configuration.enableHeartbeats)
                {
                    if (isDebugging)
                        Debug.Log("Enabling server heartbeat.");

                    SteamGameServer.SetAdvertiseServerActive(true);
                }

                Debug.Log("Steamworks Game Server Started.\nWaiting for connection result from Steamworks");
            }

            public static void SendUpdatedServerDetailsToSteam()
            {
                if (Configuration.rulePairs != null && Configuration.rulePairs.Length > 0)
                {
                    var pairString = "Set the following rules:\n";

                    foreach (var pair in Configuration.rulePairs)
                    {
                        SteamGameServer.SetKeyValue(pair.key, pair.value);
                        pairString += "\n\t[" + pair.key + "] = [" + pair.value + "]";
                    }

                    if (isDebugging)
                        Debug.Log(pairString);
                }
            }

            public static void RegisterCallbacks()
            {
                steamServerConnectFailure = Callback<SteamServerConnectFailure_t>.CreateGameServer(OnSteamServerConnectFailure);
                steamServersConnected = Callback<SteamServersConnected_t>.CreateGameServer(OnSteamServersConnected);
                steamServersDisconnected = Callback<SteamServersDisconnected_t>.CreateGameServer(OnSteamServersDisconnected);
            }
        }

        public static class Web
        {
            private static bool appListLoading = false;
            private static bool appsListLoaded = false;
            private static SteamAppsListAPI appsListApi;
            private static BackgroundWorker appListWorker;
            private static BackgroundWorker getNewsForApp;
            private static List<Action> waitingForAppListLoad = new List<Action>();

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                appsListApi = default;
                appsListLoaded = false;

                if (appListWorker != null)
                {
                    if (appListWorker.IsBusy)
                        appListWorker.CancelAsync();

                    appListWorker.Dispose();
                    appListWorker = null;
                }

                if (getNewsForApp != null)
                {
                    if (getNewsForApp.IsBusy)
                        getNewsForApp.CancelAsync();

                    getNewsForApp.Dispose();
                    getNewsForApp = null;
                }

                waitingForAppListLoad = new List<Action>();
            }

            [Serializable]
            private struct SteamAppsListAPI
            {
                [Serializable]
                public struct Model
                {
                    [Serializable]
                    public struct AppData
                    {
                        public ulong appid;
                        public string name;
                    }

                    public AppData[] apps;
                }

                public Model applist;

                public static UnityWebRequest GetRequest()
                {
                    return UnityWebRequest.Get("https://api.steampowered.com/ISteamApps/GetAppList/v2/?");
                }
            }

            [Serializable]
            public struct SteamAppNews
            {
                [Serializable]
                public struct SteamNewsItem
                {
                    public ulong gid;
                    public string title;
                    public string url;
                    public bool is_external_url;
                    public string author;
                    public string contents;
                    public string feedlabel;
                    public long date;
                    public string feedname;
                    public uint feed_type;
                    public uint appid;

                    public DateTime Date => new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(date);
                }

                public uint appid;
                public SteamNewsItem[] newsitems;
                public uint count;
            }

            public static bool IsAppsListLoaded => appsListLoaded;

            /// <summary>
            /// Requests the list of all Steam apps from the Steam Web API.
            /// This must be called before the <see cref="GetAppName(AppId_t, out string)"/> can work.
            /// The <see cref="GetAppName(AppData, Action{string, bool})"/> will call this for you if requried.
            /// </summary>
            /// <param name="callback"></param>
            /// <returns>True if the process was started, false if the process was already running, a new process cannot be started till the previous is completed.</returns>
            public static void LoadAppNames(Action callback)
            {
                if(appsListLoaded)
                {
                    callback?.Invoke();
                    return;
                }

                if (!appListLoading)
                {
                    appListLoading = true;
                    waitingForAppListLoad.Add(callback);

                    var www = SteamAppsListAPI.GetRequest();
                    var co = www.SendWebRequest();
                    co.completed += (arg) =>
                    {
                        if (www.result == UnityWebRequest.Result.Success)
                        {
                            try
                            {
                                string resultContent = www.downloadHandler.text;
                                appsListApi = JsonUtility.FromJson<SteamAppsListAPI>(resultContent);
                                appsListLoaded = true;
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError("Failed to load the Steam App List: Exception = " + ex.Message);
                            }
                        }
                        else
                        {
                            Debug.LogError($"Failed to load the Steam App List: State = {www.result}, Error Message = {www.error}");
                        }

                        foreach (var action in waitingForAppListLoad)
                        {
                            if (action != null)
                                action.Invoke();
                        }
                        appListLoading = false;
                        waitingForAppListLoad.Clear();
                    };
                }
                else
                    waitingForAppListLoad.Add(callback);
            }

            /// <summary>
            /// This calls assumes you have already called <see cref="LoadAppNames(Action)"/> and simply returns the name of the indicated app if known.
            /// </summary>
            /// <param name="appId">The app to read the name for</param>
            /// <param name="name">The name found if any</param>
            /// <returns><see cref="true"/> if the app was found, <see cref="false"/> otherwise</returns>
            public static bool GetAppName(AppData appId, out string name)
            {
                if (appsListApi.applist.apps != null && appsListApi.applist.apps.Length > 0)
                {
                    var app = appsListApi.applist.apps.FirstOrDefault(p => p.appid == appId);
                    if (app.appid == appId)
                    {
                        name = app.name;
                        return true;
                    }
                    else
                    {
                        name = "Unknown";
                        return false;
                    }

                }
                else
                {
                    name = "Unknown";
                    return false;
                }
            }
            /// <summary>
            /// Gets the app name invoking the callback immeaditly if the names are already loaded. If not this will load the names and then invoke the callback when read.
            /// </summary>
            /// <remarks>
            /// Callback signature should be
            /// <code>
            /// void HandleCallback(string name, bool ioFailure);
            /// </code>
            /// The name paramiter is the name found if any and the ioFailure paramiter is true if an error occured or the app was not found.
            /// </remarks>
            /// <param name="appId">The App ID to find the name for</param>
            /// <param name="callback">The callback to invoke when found, the <see cref="string"/> will be the name found if any, the <see cref="bool"/> will be true if an error occured</param>
            public static void GetAppName(AppData appId, Action<string, bool> callback)
            {
                if (!appsListLoaded)
                {
                    LoadAppNames(() =>
                    {
                        if (GetAppName(appId, out string name))
                            callback?.Invoke(name, false);
                        else
                            callback?.Invoke("Unkown", true);
                    });
                }
                else
                {
                    if (GetAppName(appId, out string name))
                        callback?.Invoke(name, false);
                    else
                        callback?.Invoke("Unkown", true);
                }
            }

            /// <summary>
            /// Gets the news entries for the specified app as they are seen in the Steam Community New listing
            /// </summary>
            /// <param name="appId">The app to read the news for</param>
            /// <param name="count">The number of entries to return, if left to 0 the Steam default will return (20 at the time of writing)</param>
            /// <param name="feeds">The comma delimited list of feeds to be read, leave blank or pass in <see cref="string.Empty"/> to return all feeds</param>
            /// <param name="tags">The comma delimited list of tags to be read, leave blank or pass in <see cref="string.Empty"/> to return all tags</param>
            /// <param name="callback">This will be invoked when the call is complete, if null no call will be made. The <see cref="SteamAppNews"/> paramiter is the results found, the <see cref="bool"/> paramiter is true when an error occured i.e. IOFailure</param>
            public static void GetNewsForApp(AppData appId, uint count, string feeds, string tags, Action<SteamAppNews, bool> callback)
            {
                string get = "https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=" + appId.ToString();
                if (count > 0)
                    get += "&count=" + count.ToString();
                if (!string.IsNullOrEmpty(feeds))
                    get += "&feeds=" + feeds.ToString();
                if (!string.IsNullOrEmpty(tags))
                    get += "&tags=" + tags.ToString();

                var www = new UnityWebRequest(get);
                var co = www.SendWebRequest();
                co.completed += (arg) =>
                {
                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        try
                        {
                            string resultContent = www.downloadHandler.text;
                            callback?.Invoke(JsonUtility.FromJson<SteamAppNews>(resultContent), false);

                        }
                        catch (Exception)
                        {
                            callback?.Invoke(default(SteamAppNews), true);
                        }
                    }
                    else
                    {
                        callback?.Invoke(default(SteamAppNews), true);
                    }
                };
            }
        }
    }
}
#endif