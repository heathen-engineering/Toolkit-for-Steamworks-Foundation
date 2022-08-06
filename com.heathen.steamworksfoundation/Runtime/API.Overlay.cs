#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration.API
{
    public static class Overlay
    {
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                eventGameOverlayActivated = new GameOverlayActivatedEvent();
                eventGameServerChangeRequested = new GameServerChangeRequestedEvent();
                eventGameLobbyJoinRequested = new GameLobbyJoinRequestedEvent();
                eventGameRichPresenceJoinRequest = new GameRichPresenceJoinRequestedEvent();
                m_GameOverlayActivated_t = null;
                m_GameServerChangeRequested_t = null;
                m_GameLobbyJoinRequested_t = null;
                m_GameRichPresenceJoinRequested_t = null;
            }

            public static bool IsEnabled => SteamUtils.IsOverlayEnabled();
            public static bool IsShowing => isShowing;
            public static ENotificationPosition NotificationPosition
            {
                get => notificationPosition;
                set
                {
                    notificationPosition = value;
                    SteamUtils.SetOverlayNotificationPosition(notificationPosition);
                }
            }
            public static Vector2Int NotificationInset
            {
                get => notificationInset;
                set
                {
                    notificationInset = value;
                    SteamUtils.SetOverlayNotificationInset(value.x, value.y);
                }
            }

            /// <summary>
            /// Posted when the Steam Overlay activates or deactivates. The game can use this to be pause or resume single player games.
            /// </summary>
            public static GameOverlayActivatedEvent EventGameOverlayActivated
            {
                get
                {
                    if (m_GameOverlayActivated_t == null)
                        m_GameOverlayActivated_t = Callback<GameOverlayActivated_t>.Create((r) =>
                       {
                           isShowing = r.m_bActive == 1;
                           eventGameOverlayActivated.Invoke(isShowing);
                       });

                    return eventGameOverlayActivated;
                }
            }
            /// <summary>
            /// Called when the user tries to join a different game server from their friends list. The game client should attempt to connect to specified server when this is received.
            /// </summary>
            public static GameServerChangeRequestedEvent EventGameServerChangeRequested
            {
                get
                {
                    if (m_GameServerChangeRequested_t == null)
                        m_GameServerChangeRequested_t = Callback<GameServerChangeRequested_t>.Create(eventGameServerChangeRequested.Invoke);

                    return eventGameServerChangeRequested;
                }
            }
            /// <summary>
            /// Called when the user tries to join a lobby from their friends list or from an invite. The game client should attempt to connect to specified lobby when this is received. If the game isn't running yet then the game will be automatically launched with the command line parameter +connect_lobby <64-bit lobby Steam ID> instead.
            /// </summary>
            public static GameLobbyJoinRequestedEvent EventGameLobbyJoinRequested
            {
                get
                {
                    if (m_GameLobbyJoinRequested_t == null)
                        m_GameLobbyJoinRequested_t = Callback<GameLobbyJoinRequested_t>.Create(eventGameLobbyJoinRequested.Invoke);

                    return eventGameLobbyJoinRequested;
                }
            }
            /// <summary>
            /// Invoked when the SteamAPI resonds with a GameRichPresenceJoinRequested_t
            /// </summary>
            public static GameRichPresenceJoinRequestedEvent EventGameRichPresenceJoinRequested
            {
                get
                {
                    if (m_GameRichPresenceJoinRequested_t == null)
                        m_GameRichPresenceJoinRequested_t = Callback<GameRichPresenceJoinRequested_t>.Create(eventGameRichPresenceJoinRequest.Invoke);

                    return eventGameRichPresenceJoinRequest;
                }
            }

            private static bool isShowing = false;
            private static ENotificationPosition notificationPosition = ENotificationPosition.k_EPositionBottomRight;
            private static Vector2Int notificationInset = Vector2Int.zero;

            private static GameOverlayActivatedEvent eventGameOverlayActivated = new GameOverlayActivatedEvent();
            private static GameServerChangeRequestedEvent eventGameServerChangeRequested = new GameServerChangeRequestedEvent();
            private static GameLobbyJoinRequestedEvent eventGameLobbyJoinRequested = new GameLobbyJoinRequestedEvent();
            private static GameRichPresenceJoinRequestedEvent eventGameRichPresenceJoinRequest = new GameRichPresenceJoinRequestedEvent();

            private static Callback<GameOverlayActivated_t> m_GameOverlayActivated_t;
            private static Callback<GameServerChangeRequested_t> m_GameServerChangeRequested_t;
            private static Callback<GameLobbyJoinRequested_t> m_GameLobbyJoinRequested_t;
            private static Callback<GameRichPresenceJoinRequested_t> m_GameRichPresenceJoinRequested_t;

            /// <summary>
            /// Activates the Steam Overlay to a specific dialog.
            /// </summary>
            /// <param name="dialog">The dialog to open. Valid options are: "friends", "community", "players", "settings", "officialgamegroup", "stats", "achievements".</param>
            public static void Activate(string dialog) => SteamFriends.ActivateGameOverlay(dialog);
            /// <summary>
            /// Activates the Steam Overlay to a specific dialog.
            /// </summary>
            /// <param name="dialog">The dialog to open. Valid options are: "friends", "community", "players", "settings", "officialgamegroup", "stats", "achievements".</param>
            public static void Activate(OverlayDialog dialog) => SteamFriends.ActivateGameOverlay(dialog.ToString());
            /// <summary>
            /// Activates the Steam Overlay to open the invite dialog. Invitations sent from this dialog will be for the provided lobby.
            /// </summary>
            /// <param name="lobbyId">The Steam ID of the lobby that selected users will be invited to.</param>
            public static void ActivateInviteDialog(CSteamID lobbyId) => SteamFriends.ActivateGameOverlayInviteDialog(lobbyId);
            public static void ActivateInviteDialog(string connectionString) => SteamFriends.ActivateGameOverlayInviteDialogConnectString(connectionString);
            public static void ActivateRemotePlayInviteDialog(CSteamID lobbyId) => SteamFriends.ActivateGameOverlayRemotePlayTogetherInviteDialog(lobbyId);
            /// <summary>
            /// Activates the Steam Overlay to the Steam store page for the provided app.
            /// </summary>
            /// <param name="appID">The app ID to show the store page of.</param>
            /// <param name="flag">Flags to modify the behavior when the page opens.</param>
            public static void Activate(AppId_t appID, EOverlayToStoreFlag flag) => SteamFriends.ActivateGameOverlayToStore(appID, flag);
            /// <summary>
            /// Activates Steam Overlay to a specific dialog.
            /// </summary>
            /// <param name="dialog">The dialog to open.</param>
            /// <param name="steamId">The Steam ID of the context to open this dialog to.</param>
            public static void Activate(string dialog, CSteamID steamId) => SteamFriends.ActivateGameOverlayToUser(dialog, steamId);
            /// <summary>
            /// Activates Steam Overlay to a specific dialog.
            /// </summary>
            /// <param name="dialog">The dialog to open.</param>
            /// <param name="steamId">The Steam ID of the context to open this dialog to.</param>
            public static void Activate(FriendDialog dialog, CSteamID steamId) => SteamFriends.ActivateGameOverlayToUser(dialog.ToString(), steamId);
            /// <summary>
            /// Activates Steam Overlay web browser directly to the specified URL.
            /// </summary>
            /// <param name="url">The webpage to open. (A fully qualified address with the protocol is required, e.g. "http://www.steampowered.com")</param>
            public static void ActivateWebPage(string url) => SteamFriends.ActivateGameOverlayToWebPage(url);
        }
    }
}
#endif