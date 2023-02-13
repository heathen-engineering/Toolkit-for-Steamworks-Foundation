#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    public class SteamGameServerEvents : MonoBehaviour
    {
        /// <summary>
        /// An event raised when by Steamworks debugging on disconnected.
        /// This is only avilable in server builds.
        /// </summary>
        public API.App.Server.DisconnectedEvent evtDisconnected;
        /// <summary>
        /// An event raised by Steamworks debugging on connected.
        /// This is only avilable in server builds.
        /// </summary>
        public API.App.Server.ConnectedEvent evtConnected;
        /// <summary>
        /// An event raised by Steamworks debugging on failure.
        /// This is only avilable in server builds.
        /// </summary>
        public API.App.Server.FailureEvent evtFailure;

        private void Awake()
        {
            API.App.Server.eventDisconnected.AddListener(evtDisconnected.Invoke);
            API.App.Server.eventConnected.AddListener(evtConnected.Invoke);
            API.App.Server.eventFailure.AddListener(evtFailure.Invoke);
        }

        private void OnDestroy()
        {
            API.App.Server.eventDisconnected.RemoveListener(evtDisconnected.Invoke);
            API.App.Server.eventConnected.RemoveListener(evtConnected.Invoke);
            API.App.Server.eventFailure.RemoveListener(evtFailure.Invoke);
        }
    }
}
#endif