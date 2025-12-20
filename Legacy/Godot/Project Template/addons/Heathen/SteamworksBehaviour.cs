using Godot;
using System;
using Steamworks;
using HeathenEngineering.SteamworksIntegration;

[Tool]
public class SteamworksBehaviour : Node
{
    public bool Initalized { get; private set; } = false;
    public bool HasInitalizationError { get; private set; } = false;
    public string InitalizationErrorMessage { get; private set; } = string.Empty;
    public bool GameServerLoggedOn { get; private set; } = false;

    public event ApiInitalizationEvent evtSteamInitialized;

    private Callback<SteamServerConnectFailure_t> steamServerConnectFailure;
    private Callback<SteamServersConnected_t> steamServersConnected;
    private Callback<SteamServersDisconnected_t> steamServersDisconnected;

    public override void _Ready()
    {
        if (!Initalized)
        {
            /*********************************************************
             * Update this with your App ID
             * Dont forget to also update the steam_appid.txt file
             ********************************************************/
            AppId_t applicationId = new AppId_t(480);

#if GODOT_SERVER
#else
            ClientInitalize(applicationId);
#endif
        }
    }

    public override void _Process(float delta)
    {
        if (!Initalized)
            return;

#if GODOT_SERVER
        GameServer.RunCallbacks();
#else
        SteamAPI.RunCallbacks();
#endif
    }

    public override void _ExitTree()
    {
#if GODOT_SERVER
        GameServer.Shutdown();
#else
        SteamAPI.Shutdown();
#endif
    }

    public void RegisterCallbacks()
    {
        steamServerConnectFailure = Callback<SteamServerConnectFailure_t>.CreateGameServer(OnSteamServerConnectFailure);
        steamServersConnected = Callback<SteamServersConnected_t>.CreateGameServer(OnSteamServersConnected);
        steamServersDisconnected = Callback<SteamServersDisconnected_t>.CreateGameServer(OnSteamServersDisconnected);
    }

    private void ClientInitalize(AppId_t applicationId)
    {
        if (!Packsize.Test())
        {
            HasInitalizationError = true;
            InitalizationErrorMessage = "Packesize Test returned false, the wrong version of the Steamowrks.NET is being run in this platform.";
            evtSteamInitialized?.Invoke(false, InitalizationErrorMessage);
            GD.PrintErr(InitalizationErrorMessage);
            return;
        }

        if (!DllCheck.Test())
        {
            HasInitalizationError = true;
            InitalizationErrorMessage = "DLL Check Test returned false, one or more of the Steamworks binaries seems to be the wrong version.";
            evtSteamInitialized?.Invoke(false, InitalizationErrorMessage);
            GD.PrintErr(InitalizationErrorMessage);
            return;
        }

        if (!Engine.EditorHint)
        {
            try
            {
                // If Steamworks is not running or the game wasn't started through Steamworks, SteamAPI_RestartAppIfNecessary starts the
                // Steamworks client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.
                if (SteamAPI.RestartAppIfNecessary(applicationId))
                {
                    GetTree().Quit();
                    return;
                }
            }
            catch (System.DllNotFoundException e)
            {
                HasInitalizationError = true;
                InitalizationErrorMessage = "Steamworks.NET could not load, steam_api.dll/so/dylib. It's likely not in the correct location.";
                // We catch this exception here, as it will be the first occurence of it.
                GD.PrintErr("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e);
                evtSteamInitialized?.Invoke(false, "[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e);
                GetTree().Quit();
                return;
            }
        }
        else
        {
            if (!SteamAPI.IsSteamRunning())
            {
                HasInitalizationError = true;
                InitalizationErrorMessage = "Steam Running check returned false, Steam client must be running for the API to intialize.";
                evtSteamInitialized?.Invoke(false, InitalizationErrorMessage);
                GD.PrintErr("[Steamworks.NET] Steam client must be running for the API to intialize.");
                return;
            }
        }

        try
        {
            if (SteamAPI.Init())
            {
                GD.Print("Steamworks initialization success!");

                HeathenEngineering.SteamworksIntegration.API.Overlay.Client.Initialize();
                HeathenEngineering.SteamworksIntegration.API.Friends.Client.Initialize();

                Initalized = true;
                evtSteamInitialized?.Invoke(true, string.Empty);
            }
            else
            {
                evtSteamInitialized?.Invoke(false, "Steam API initalization requrest returned false.");
            }
        }
        catch (Exception e)
        {
            GD.PrintErr("Steamworks initialization threw an exception :O");
            GD.PrintErr(e);
            evtSteamInitialized?.Invoke(false, $"Steamworks initialization threw an exception : {e.Message}");
        }
    }

    private void OnSteamServersDisconnected(SteamServersDisconnected_t param)
    {
        GameServerLoggedOn = false;
        EmitSignal(nameof(SteamServersDisconnectedEvent), param);
    }

    private void OnSteamServersConnected(SteamServersConnected_t param)
    {
        GameServerLoggedOn = true;
        EmitSignal(nameof(SteamServersConnectedEvent), param);
    }

    private void OnSteamServerConnectFailure(SteamServerConnectFailure_t param)
    {
        GameServerLoggedOn = false;
        EmitSignal(nameof(SteamServerConnectFailureEvent), param);
    }
}