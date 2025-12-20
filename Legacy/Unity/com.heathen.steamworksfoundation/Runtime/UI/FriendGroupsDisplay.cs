#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using Friends = HeathenEngineering.SteamworksIntegration.API.Friends;
using App = HeathenEngineering.SteamworksIntegration.API.App;
using System.Collections;

namespace HeathenEngineering.SteamworksIntegration.UI
{
    /// <summary>
    /// This control component is focused on emulation of Steam's own Friend List. It will read for and sort the local player's friends into the same list structure see in Steam Client's Friend List i.e. Playing, Online, Offline, any custom groups the player may have, etc.
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/friend-groups-display")]
    public class FriendGroupsDisplay : MonoBehaviour
    {
        [SerializeField]
        private Transform inGameCollection;
        [SerializeField]
        private Transform inOtherGameCollection;
        [SerializeField]
        private Transform groupedCollection;
        [SerializeField]
        private Transform onlineCollection;
        [SerializeField]
        private Transform offlineCollection;
        [SerializeField]
        private GameObject groupPrefab;

        private void OnEnable()
        {
            if (API.App.Initialized)
            {
                UpdateDisplay();
            }
            else
            {
                API.App.evtSteamInitialized.AddListener(DelayUpdate);
            }
        }

        private void OnDisable()
        {
            Clear();
        }

        private void DelayUpdate()
        {
            UpdateDisplay();
            API.App.evtSteamInitialized.RemoveListener(DelayUpdate);
        }

        /// <summary>
        /// Clear the display
        /// </summary>
        public void Clear()
        {
            if (inGameCollection != null
                && inGameCollection.childCount > 0)
            {
                foreach (Transform tran in inGameCollection)
                {
                    Destroy(tran.gameObject);
                }
            }

            if (groupedCollection != null
                && groupedCollection.childCount > 0)
            {
                foreach (Transform tran in groupedCollection)
                {
                    Destroy(tran.gameObject);
                }
            }

            if (onlineCollection != null
                && onlineCollection.childCount > 0)
            {
                foreach (Transform tran in onlineCollection)
                {
                    Destroy(tran.gameObject);
                }
            }

            if (offlineCollection != null
                && offlineCollection.childCount > 0)
            {
                foreach (Transform tran in offlineCollection)
                {
                    Destroy(tran.gameObject);
                }
            }

            if (inOtherGameCollection != null
                && inOtherGameCollection.childCount > 0)
            {
                foreach (Transform tran in inOtherGameCollection)
                {
                    Destroy(tran.gameObject);
                }
            }
        }
        /// <summary>
        /// Update the display
        /// </summary>
        public void UpdateDisplay()
        {
            Clear();

            List<UserData> online = new List<UserData>();
            List<UserData> inGame = new List<UserData>();
            List<UserData> inOtherGame = new List<UserData>();
            List<UserData> offline = new List<UserData>();
            Dictionary<string, List<UserData>> customGroups = new Dictionary<string, List<UserData>>();

            var friends = Friends.Client.GetFriends(EFriendFlags.k_EFriendFlagImmediate);
            var groups = Friends.Client.GetFriendsGroups();

            foreach (var group in groups)
            {
                var groupName = Friends.Client.GetFriendsGroupName(group);
                if (!customGroups.ContainsKey(groupName))
                    customGroups.Add(groupName, new List<UserData>());

                var list = customGroups[groupName];

                foreach (var user in Friends.Client.GetFriendsGroupMembersList(group))
                {
                    if (user != UserData.Me && !list.Contains(user))
                        list.Add(user);
                }
            }

            foreach (var user in friends)
            {
                if (user == UserData.Me)
                    continue;

                if (user.GetGamePlayed(out FriendGameInfo gameInfo))
                {
                    online.Add(user);
                    if (gameInfo.Game.IsMe)
                    {
                        //In this game
                        inGame.Add(user);
                    }
                    else
                    {
                        //In other game
                        inOtherGame.Add(user);
                    }
                }
                else if (user.State != EPersonaState.k_EPersonaStateOffline
                    && user.State != EPersonaState.k_EPersonaStateInvisible)
                {
                    //On line in some form
                    online.Add(user);
                }
                else
                {
                    //Off line or hidden
                    offline.Add(user);
                }
            }

            if (onlineCollection != null)
            {
                onlineCollection.gameObject.SetActive(true);
                var onlineGo = Instantiate(groupPrefab, onlineCollection);
                var onlineComp = onlineGo.GetComponent<FriendGroup>();
                onlineComp.InitializeOnline("Online", online, true);
            }

            if (offlineCollection != null)
            {
                var offlineGo = Instantiate(groupPrefab, offlineCollection);
                var offlineComp = offlineGo.GetComponent<FriendGroup>();
                offlineComp.InitializeOffline("Offline", offline, false);
            }

            if (inGameCollection != null)
            {
                var inGameGo = Instantiate(groupPrefab, inGameCollection);
                var inGameComp = inGameGo.GetComponent<FriendGroup>();
                inGameComp.InitializeInGame("In Game", inGame, true);
            }

            if (inOtherGameCollection != null)
            {
                var otherGO = Instantiate(groupPrefab, inOtherGameCollection);
                var otherComp = otherGO.GetComponent<FriendGroup>();
                otherComp.InitializeInOther("Other Games", inOtherGame, true);
            }

            if (customGroups.Count > 0)
            {
                foreach (var kvp in customGroups)
                {
                    var kvpGO = Instantiate(groupPrefab, groupedCollection);
                    var kvpComp = kvpGO.GetComponent<FriendGroup>();
                    kvpComp.InitializeCustom(kvp.Key, kvp.Value, true);
                }
            }
            else
            {
                groupedCollection.gameObject.SetActive(false);
            }
        }
    }
}
#endif