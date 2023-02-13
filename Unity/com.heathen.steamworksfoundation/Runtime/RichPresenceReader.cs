#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Friends = HeathenEngineering.SteamworksIntegration.API.Friends;

namespace HeathenEngineering.SteamworksIntegration
{
    public class RichPresenceReader : MonoBehaviour
    {
        [Serializable]
        public class RichPresenceReaderUpdatedEvent : UnityEvent<RichPresenceReader>
        { }

        public AppData App { get; private set; } = AppId_t.Invalid;
        public UserData User { get => currentUser; set => Apply(value); }
        public Dictionary<string, string> Values { get; private set; } = new Dictionary<string, string>();

        public RichPresenceReaderUpdatedEvent evtUpdate;

        private UserData currentUser = CSteamID.Nil;

        private void OnEnable()
        {
            Friends.Client.EventFriendRichPresenceUpdate.AddListener(HandleChange);
        }

        private void OnDisable()
        {
            Friends.Client.EventFriendRichPresenceUpdate.RemoveListener(HandleChange);
        }

        public void Apply(UserData user)
        {
            currentUser = user;

            if(user.GetGamePlayed(out FriendGameInfo gameInfo))
            {
                App = gameInfo.Game;
                Values = Friends.Client.GetFriendRichPresence(user);
                evtUpdate.Invoke(this);
            }
            else
            {
                App = AppId_t.Invalid;
                Values.Clear();
                evtUpdate.Invoke(this);
            }
        }

        private void HandleChange(FriendRichPresenceUpdate param)
        {
            if (param.Friend == currentUser)
            {
                App = param.App;
                Values = Friends.Client.GetFriendRichPresence(param.Friend);
                evtUpdate.Invoke(this);
            }
        }
    }
}
#endif