#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET

using HeathenEngineering.SteamworksIntegration;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeathenEngineering.DEMO
{
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class UsingUnityEvents : MonoBehaviour
    {
        public void HandleOverlayActived(bool arg0)
        {
            Debug.Log(nameof(HandleOverlayActived) + " called");
        }

        public void HandleGameLobbyJoinRequested(CSteamID lobby, UserData user)
        {
            Debug.Log(nameof(HandleGameLobbyJoinRequested) + " called");
        }

        public void HandleGameServerChangeRequested(string connection, string password)
        {
            Debug.Log(nameof(HandleGameServerChangeRequested) + " called");
        }

        public void HandleRichPresenceJoinRequested(UserData user, string connection)
        {
            Debug.Log(nameof(HandleRichPresenceJoinRequested) + " called");
        }
    }
}
#endif