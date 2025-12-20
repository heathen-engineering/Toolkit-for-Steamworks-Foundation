using Steamworks;

namespace HeathenEngineering.SteamworksIntegration
{
    public delegate void ApiInitalizationEvent(bool success, string message);
    public delegate void SteamServersDisconnectedEvent(SteamServersDisconnected_t param);
    public delegate void SteamServersConnectedEvent(SteamServersConnected_t param);
    public delegate void SteamServerConnectFailureEvent(SteamServerConnectFailure_t param);
    public delegate void GameConnectedFriendChatMsgEvent(UserData user, string message, EChatEntryType type);
    public delegate void FriendRichPresenceUpdateEvent(FriendRichPresenceUpdate_t data);
    public delegate void PersonaStateChangeEvent(PersonaStateChange_t data);
    public delegate void HeathenEvent();
    public delegate void GameOverlayActivatedEvent(bool isShowing);
    public delegate void GameServerChangeRequestedEvent(GameServerChangeRequested_t data);
    public delegate void GameLobbyJoinRequestedEvent(GameLobbyJoinRequested_t data);
    public delegate void GameRichPresenceJoinRequestedEvent(GameRichPresenceJoinRequested_t data);
    public delegate void UserStatsReceivedEvent(UserStatsReceived_t data);
    public delegate void UserStatsUnloadedEvent(UserStatsUnloaded_t data);
    public delegate void UserStatsStoredEvent(UserStatsStored_t data);
    public delegate void UserAchievementStoredEvent(UserAchievementStored_t data);
}