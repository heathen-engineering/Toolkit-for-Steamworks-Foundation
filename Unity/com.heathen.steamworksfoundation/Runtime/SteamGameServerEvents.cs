#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    public class SteamGameServerEvents : MonoBehaviour
    {
        public SteamSettings settings;
        /// <summary>
        /// An event raised when by Steamworks debugging on disconnected.
        /// This is only avilable in server builds.
        /// </summary>
        public SteamSettings.GameServer.DisconnectedEvent evtDisconnected;
        /// <summary>
        /// An event raised by Steamworks debugging on connected.
        /// This is only avilable in server builds.
        /// </summary>
        public SteamSettings.GameServer.ConnectedEvent evtConnected;
        /// <summary>
        /// An event raised by Steamworks debugging on failure.
        /// This is only avilable in server builds.
        /// </summary>
        public SteamSettings.GameServer.FailureEvent evtFailure;

        private void Awake()
        {
            settings.server.disconnected.AddListener(evtDisconnected.Invoke);
            settings.server.connected.AddListener(evtConnected.Invoke);
            settings.server.failure.AddListener(evtFailure.Invoke);
        }

        private void OnDestroy()
        {
            settings.server.disconnected.RemoveListener(evtDisconnected.Invoke);
            settings.server.connected.RemoveListener(evtConnected.Invoke);
            settings.server.failure.RemoveListener(evtFailure.Invoke);
        }
    }
}
#endif