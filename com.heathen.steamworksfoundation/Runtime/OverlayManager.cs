#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// Manages the Steam Overlay and exposes its related events and settings.
    /// </summary>
    public class OverlayManager : MonoBehaviour
    {
        [SerializeField]
        private ENotificationPosition notificationPosition = ENotificationPosition.k_EPositionBottomRight;
        [SerializeField]
        private Vector2Int notificationInset = Vector2Int.zero;

        public ENotificationPosition NotificationPosition
        {
            get => API.Overlay.Client.NotificationPosition;
            set => API.Overlay.Client.NotificationPosition = value;
        }
        public Vector2Int NotificationInset
        {
            get => API.Overlay.Client.NotificationInset;
            set => API.Overlay.Client.NotificationInset = value;
        }
        public bool IsShowing => API.Overlay.Client.IsShowing;
        public bool IsEnabled => API.Overlay.Client.IsEnabled;

        public GameOverlayActivatedEvent evtOverlayActivated;
        public GameLobbyJoinRequestedEvent evtGameLobbyJoinRequested;
        public GameServerChangeRequestedEvent evtGameServerChangeRequested;
        public GameRichPresenceJoinRequestedEvent evtRichPresenceJoinRequested;


        private void OnEnable()
        {
            NotificationPosition = notificationPosition;
            NotificationInset = notificationInset;
            API.Overlay.Client.EventGameOverlayActivated.AddListener(evtOverlayActivated.Invoke);
            API.Overlay.Client.EventGameServerChangeRequested.AddListener(evtGameServerChangeRequested.Invoke);
            API.Overlay.Client.EventGameLobbyJoinRequested.AddListener(evtGameLobbyJoinRequested.Invoke);
            API.Overlay.Client.EventGameRichPresenceJoinRequested.AddListener(evtRichPresenceJoinRequested.Invoke);
        }

        private void OnDisable()
        {
            API.Overlay.Client.EventGameOverlayActivated.RemoveListener(evtOverlayActivated.Invoke);
            API.Overlay.Client.EventGameServerChangeRequested.RemoveListener(evtGameServerChangeRequested.Invoke);
            API.Overlay.Client.EventGameLobbyJoinRequested.RemoveListener(evtGameLobbyJoinRequested.Invoke);
            API.Overlay.Client.EventGameRichPresenceJoinRequested.RemoveListener(evtRichPresenceJoinRequested.Invoke);
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (SteamSettings.Initialized)
            {
                if (notificationPosition != API.Overlay.Client.NotificationPosition)
                {
                    notificationPosition = NotificationPosition;
                    Debug.LogWarning("Notification Position cannot be updated from the inspector at runtime.");
                }
                if (notificationInset != API.Overlay.Client.NotificationInset)
                {
                    notificationInset = NotificationInset;
                    Debug.LogWarning("Notification Insert cannot be updated from the inspector at runtime.");
                }
            }
        }
#endif

        /// <summary>
        /// Activates the Steam Overlay to a specific dialog.
        /// </summary>
        /// <param name="dialog">The dialog to open. Valid options are: "friends", "community", "players", "settings", "officialgamegroup", "stats", "achievements".</param>
        public void Open(string dialog) => API.Overlay.Client.Activate(dialog);
        /// <summary>
        /// Activates the Steam Overlay to a specific dialog.
        /// </summary>
        /// <param name="dialog">The dialog to open. Valid options are: "friends", "community", "players", "settings", "officialgamegroup", "stats", "achievements".</param>
        public void Open(OverlayDialog dialog) => API.Overlay.Client.Activate(dialog);
        /// <summary>
        /// Activates the Steam Overlay to open the invite dialog. Invitations sent from this dialog will be for the provided lobby.
        /// </summary>
        /// <param name="lobbyId">The Steam ID of the lobby that selected users will be invited to.</param>
        public void OpenLobbyInvite(CSteamID lobbyId) => API.Overlay.Client.ActivateInviteDialog(lobbyId);
        public void OpenConnectStringInvite(string connectionString) => API.Overlay.Client.ActivateInviteDialog(connectionString);
        public void OpenRemotePlayInvite(CSteamID lobbyId) => API.Overlay.Client.ActivateRemotePlayInviteDialog(lobbyId);
        /// <summary>
        /// Activates the Steam Overlay to the Steam store page for the provided app.
        /// </summary>
        /// <param name="appID">The app ID to show the store page of.</param>
        /// <param name="flag">Flags to modify the behavior when the page opens.</param>
        public void OpenStore(AppId_t appID, EOverlayToStoreFlag flag) => API.Overlay.Client.Activate(appID, flag);
        /// <summary>
        /// Activates Steam Overlay to a specific dialog.
        /// </summary>
        /// <param name="dialog">The dialog to open.</param>
        /// <param name="steamId">The Steam ID of the context to open this dialog to.</param>
        public void OpenUser(string dialog, UserData steamId) => API.Overlay.Client.Activate(dialog, steamId);
        /// <summary>
        /// Activates Steam Overlay to a specific dialog.
        /// </summary>
        /// <param name="dialog">The dialog to open.</param>
        /// <param name="steamId">The Steam ID of the context to open this dialog to.</param>
        public void OpenUser(FriendDialog dialog, UserData steamId) => API.Overlay.Client.Activate(dialog.ToString(), steamId);
        /// <summary>
        /// Activates Steam Overlay web browser directly to the specified URL.
        /// </summary>
        /// <param name="url">The webpage to open. (A fully qualified address with the protocol is required, e.g. "http://www.steampowered.com")</param>
        public void OpenWebPage(string url) => API.Overlay.Client.ActivateWebPage(url);
    }
}
#endif