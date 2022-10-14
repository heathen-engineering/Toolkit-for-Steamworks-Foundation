using Steamworks;
using System;

namespace HeathenEngineering.SteamworksIntegration.API
{
    public static class Overlay
    {
        public static class Client
        {
            public static void Initialize()
            {
                if (m_GameOverlayActivated_t == null)
                    m_GameOverlayActivated_t = Callback<GameOverlayActivated_t>.Create(OverllayActivated);

                if (m_GameServerChangeRequested_t == null)
                    m_GameServerChangeRequested_t = Callback<GameServerChangeRequested_t>.Create(ServerChangeRequested);

                if (m_GameLobbyJoinRequested_t == null)
                    m_GameLobbyJoinRequested_t = Callback<GameLobbyJoinRequested_t>.Create(LobbyJoinRequested);

                if (m_GameRichPresenceJoinRequested_t == null)
                    m_GameRichPresenceJoinRequested_t = Callback<GameRichPresenceJoinRequested_t>.Create(RichPresenceJoinRequest);
            }

            private static void RichPresenceJoinRequest(GameRichPresenceJoinRequested_t param)
            {
                EventGameRichPresenceJoinRequest?.Invoke(param);
            }

            private static void LobbyJoinRequested(GameLobbyJoinRequested_t param)
            {
                EventGameLobbyJoinRequested?.Invoke(param);
            }

            private static void ServerChangeRequested(GameServerChangeRequested_t param)
            {
                EventGameServerChangeRequested?.Invoke(param);
            }

            private static void OverllayActivated(GameOverlayActivated_t responce)
            {
                isShowing = responce.m_bActive == 1;
                EventGameOverlayActivated?.Invoke(isShowing);
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
            public static Int2 NotificationInset
            {
                get => notificationInset;
                set
                {
                    notificationInset = value;
                    SteamUtils.SetOverlayNotificationInset(value.x, value.y);
                }
            }

            private static bool isShowing = false;
            private static ENotificationPosition notificationPosition = ENotificationPosition.k_EPositionBottomRight;
            private static Int2 notificationInset = new Int2 { x = 0, y = 0 };

            public static event GameOverlayActivatedEvent EventGameOverlayActivated;
            public static event GameServerChangeRequestedEvent EventGameServerChangeRequested;
            public static event GameLobbyJoinRequestedEvent EventGameLobbyJoinRequested;
            public static event GameRichPresenceJoinRequestedEvent EventGameRichPresenceJoinRequest;

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