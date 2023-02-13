#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System.Collections.Generic;
using UnityEngine;
using Friends = HeathenEngineering.SteamworksIntegration.API.Friends;
using System.Linq;
using System.Collections;

namespace HeathenEngineering.SteamworksIntegration.UI
{
    public class FriendList : MonoBehaviour
    {
        public enum Filter
        {
            All,
            InThisGame,
            InOtherGame,
            InAnyGame,
            NotInThisGame,
            NotInGame,
            AnyOnline,
            AnyOffline,
            Away,
            Buisy,
            Followed
        }

        [SerializeField]
        public bool includeFollowed = false;
        [SerializeField]
        private Filter filter = Filter.All;
        [SerializeField]
        public Transform content;
        [SerializeField]
        public GameObject recordTemplate;

        public Filter ActiveFilter
        {
            get => filter;
            set
            {
                filter = value;
                UpdateDisplay();
            }
        }

        private Dictionary<UserData, GameObject> records = new Dictionary<UserData, GameObject>();

        private void OnEnable()
        {
            Friends.Client.EventPersonaStateChange.AddListener(HandleStateChange);

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
            Friends.Client.EventPersonaStateChange.RemoveListener(HandleStateChange);
            Clear();
        }

        private void DelayUpdate()
        {
            UpdateDisplay();
            API.App.evtSteamInitialized.RemoveListener(DelayUpdate);
        }

        private void HandleStateChange(PersonaStateChange arg0)
        {
            UserData user = arg0.SubjectId;
            if (MatchFilter(user))
                Add(user);
            else
                Remove(user);
        }

        private void Remove(UserData user)
        {
            if (records.ContainsKey(user))
            {
                var target = records[user];
                records.Remove(user);
                Destroy(target.gameObject);
            }
        }

        private void Add(UserData user)
        {
            //Add the user and then resort the display
            if (!records.ContainsKey(user))
            {
                AddNewRecord(user);
                SortRecords();
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
        }

        public void Clear()
        {
            records.Clear();

            if (content.childCount > 0)
            {
                foreach (Transform tran in content)
                {
                    try
                    {
                        Destroy(tran.gameObject);
                    }
                    catch { }
                }
            }
        }

        public void UpdateDisplay()
        {
            Clear();

            List<UserData> filtered = new List<UserData>();
            var friends = new List<UserData>(Friends.Client.GetFriends(EFriendFlags.k_EFriendFlagImmediate));

            if (includeFollowed)
            {
                var followed = new List<UserData>();

                API.Friends.Client.GetFollowed((r) =>
                {
                    if (r != null && r.Length > 0)
                    {
                        var subset = r.Where(p => p.GetEAccountType() == Steamworks.EAccountType.k_EAccountTypeIndividual);
                        if (subset.Count() > 0)
                        {
                            foreach (var id in subset)
                            {
                                if (!friends.Contains(id))
                                    friends.Add(id);

                                followed.Add(id);
                            }
                        }
                    }

                    if (filter == Filter.Followed)
                    {
                        foreach (var user in followed)
                            if (user != UserData.Me && !records.ContainsKey(user))
                                AddNewRecord(user);
                    }
                    else
                    {
                        foreach (var user in friends)
                        {
                            if (user != UserData.Me && MatchFilter(user))
                                filtered.Add(user);
                        }

                        foreach (var user in filtered)
                            if (user != UserData.Me && !records.ContainsKey(user))
                                AddNewRecord(user);
                    }

                    SortRecords();
                });
            }
            else
            {
                foreach (var user in friends)
                {
                    if (user != UserData.Me && MatchFilter(user))
                        AddNewRecord(user);
                }

                SortRecords();
            }
        }

        public bool MatchFilter(UserData friend)
        {
            switch(filter)
            {
                case Filter.All:
                    return true;
                case Filter.AnyOffline:
                    return friend.State == EPersonaState.k_EPersonaStateOffline;
                case Filter.AnyOnline:
                    return friend.State != EPersonaState.k_EPersonaStateOffline;
                case Filter.Away:
                    return friend.State == EPersonaState.k_EPersonaStateAway;
                case Filter.Buisy:
                    return friend.State == EPersonaState.k_EPersonaStateBusy;
                case Filter.InAnyGame:
                    return friend.InGame;
                case Filter.InOtherGame:
                    if (friend.GetGamePlayed(out FriendGameInfo ioGame))
                    {
                        if (!ioGame.Game.IsMe)
                            return true;
                        else
                            return false;
                    }
                    else
                        return false;
                case Filter.InThisGame:
                    if (friend.GetGamePlayed(out FriendGameInfo itGame))
                    {
                        if (itGame.Game.IsMe)
                            return true;
                        else
                            return false;
                    }
                    else
                        return false;
                case Filter.NotInThisGame:
                    if (friend.GetGamePlayed(out FriendGameInfo nitGame))
                    {
                        if (!nitGame.Game.IsMe)
                            return true;
                        else
                            return false;
                    }
                    else if (friend.State != EPersonaState.k_EPersonaStateOffline)
                        return true;
                    else
                        return false;
                case Filter.NotInGame:
                    return !friend.InGame;
                default:
                    return false;
            }
        }
    }
}
#endif