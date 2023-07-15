#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Friends = HeathenEngineering.SteamworksIntegration.API.Friends;

namespace HeathenEngineering.SteamworksIntegration.UI
{
    /// <summary>
    /// Represents a group of friends such as those that are online, offline, etc.
    /// This is used by the <see cref="FriendGroupsDisplay"/>
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/friend-group")]
    public class FriendGroup : MonoBehaviour
    {
        [SerializeField]
        private TMPro.TextMeshProUGUI label;
        [SerializeField]
        private TMPro.TextMeshProUGUI counter;
        [SerializeField]
        private Toggle toggle;
        [SerializeField]
        private GameObject recordTemplate;
        [SerializeField]
        private Transform content;
        private enum GroupType
        {
            None,
            Online,
            Offline,
            InGame,
            OtherGame,
        }
        private Dictionary<UserData, GameObject> records = new Dictionary<UserData, GameObject>();
        private GroupType type = GroupType.None;

        private void OnEnable()
        {
            Friends.Client.EventPersonaStateChange.AddListener(HandleStateChange);
        }

        private void OnDisable()
        {
            Friends.Client.EventPersonaStateChange.RemoveListener(HandleStateChange);
        }

        private void HandleStateChange(PersonaStateChange arg0)
        {
            UserData user = arg0.SubjectId;
            if (!user.IsMe)
            {
                switch (type)
                {
                    case GroupType.Online:
                        if (user.State == EPersonaState.k_EPersonaStateOffline
                            || user.State == EPersonaState.k_EPersonaStateInvisible)
                            Remove(user);
                        else
                            Add(user);
                        break;
                    case GroupType.Offline:
                        if (user.State != EPersonaState.k_EPersonaStateOffline
                            && user.State != EPersonaState.k_EPersonaStateInvisible)
                            Remove(user);
                        else
                            Add(user);
                        break;
                    case GroupType.InGame:
                        if (user.GetGamePlayed(out FriendGameInfo thisGameCheck) && thisGameCheck.Game.App == SteamUtils.GetAppID())
                            Add(user);
                        else
                            Remove(user);
                        break;
                    case GroupType.OtherGame:
                        if (user.GetGamePlayed(out FriendGameInfo otherGameCheck) && otherGameCheck.Game.App != SteamUtils.GetAppID())
                            Add(user);
                        else
                            Remove(user);
                        break;
                    default:
                        //We dont do anything with type == none
                        break;
                }
            }
        }

        private void Remove(UserData user)
        {
            if (records.ContainsKey(user))
            {
                var target = records[user];
                records.Remove(user);
                Destroy(target);
                counter.text = "(" + records.Count.ToString() + ")";
            }
        }

        private void Add(UserData user)
        {
            //Add the user and then resort the display
            if (!records.ContainsKey(user))
            {
                AddNewRecord(user);
                SortRecords();
                counter.text = "(" + records.Count.ToString() + ")";
            }
            else
                records[user].GetComponent<IUserProfile>().UserData = user;
        }

        private void AddNewRecord(UserData user)
        {
            var go = Instantiate(recordTemplate, content);
            var comp = go.GetComponent<IUserProfile>();
            comp.UserData = user;
            records.Add(user, go);
        }

        private void SortRecords()
        {
            var keys = records.Keys.ToList();
            keys.Sort((a, b) => { return a.Nickname.CompareTo(b.Nickname); });

            foreach (var key in keys)
            {
                records[key].transform.SetAsLastSibling();
            }

            counter.text = "(" + records.Count.ToString() + ")";
        }

        /// <summary>
        /// Initializes the group display with a given collection of users
        /// </summary>
        /// <param name="name">The name of the group</param>
        /// <param name="users">The list of users to display</param>
        /// <param name="expanded">Should the group start expanded</param>
        public void InitializeCustom(string name, List<UserData> users, bool expanded)
        {
            label.text = name;
            toggle.isOn = expanded;
            type = GroupType.None;

            foreach (var user in users)
                if (!records.ContainsKey(user))
                    AddNewRecord(user);

            SortRecords();
        }
        /// <summary>
        /// Initializes the group display for online members
        /// </summary>
        /// <param name="name">The name of the group</param>
        /// <param name="users">The list of users to display</param>
        /// <param name="expanded">Should the group start expanded</param>
        public void InitializeOnline(string name, List<UserData> users, bool expanded)
        {
            label.text = name;
            toggle.isOn = expanded;
            type = GroupType.Online;

            foreach (var user in users)
                if (!records.ContainsKey(user))
                    AddNewRecord(user);

            SortRecords();
        }
        /// <summary>
        /// Initializes the group display
        /// </summary>
        /// <param name="name">The name of the group</param>
        /// <param name="users">The list of users to display</param>
        /// <param name="expanded">Should the group start expanded</param>
        public void InitializeOffline(string name, List<UserData> users, bool expanded)
        {
            label.text = name;
            toggle.isOn = expanded;
            type = GroupType.Offline;

            foreach (var user in users)
                if (!records.ContainsKey(user))
                    AddNewRecord(user);

            SortRecords();
        }
        /// <summary>
        /// Initializes the group display
        /// </summary>
        /// <param name="name">The name of the group</param>
        /// <param name="users">The list of users to display</param>
        /// <param name="expanded">Should the group start expanded</param>
        public void InitializeInGame(string name, List<UserData> users, bool expanded)
        {
            label.text = name;
            toggle.isOn = expanded;
            type = GroupType.InGame;

            foreach (var user in users)
                if (!records.ContainsKey(user))
                    AddNewRecord(user);

            SortRecords();
        }
        /// <summary>
        /// Initializes the group display
        /// </summary>
        /// <param name="name">The name of the group</param>
        /// <param name="users">The list of users to display</param>
        /// <param name="expanded">Should the group start expanded</param>
        public void InitializeInOther(string name, List<UserData> users, bool expanded)
        {
            label.text = name;
            toggle.isOn = expanded;
            type = GroupType.OtherGame;

            foreach (var user in users)
                if (!records.ContainsKey(user))
                    AddNewRecord(user);

            SortRecords();
        }
    }
}
#endif