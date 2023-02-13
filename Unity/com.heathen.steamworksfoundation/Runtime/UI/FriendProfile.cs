﻿#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Friends = HeathenEngineering.SteamworksIntegration.API.Friends;

namespace HeathenEngineering.SteamworksIntegration.UI
{
    public class FriendProfile : MonoBehaviour, IUserProfile
    {
        [Serializable]
        public struct TextField
        {
            public TMPro.TextMeshProUGUI label;
            [Header("Colors")]
            public bool useStatusColors;
            public Color inThisGame;
            public Color inOtherGame;
            public Color isOnlineActive;
            public Color isOnlineInactive;
            public Color isOffline;

            public void SetValue(string value, bool inGame, bool inThisGame, EPersonaState state)
            {
                if (label != null)
                {
                    label.text = value;

                    if (useStatusColors)
                    {
                        if (inGame)
                        {
                            if (inThisGame)
                                label.color = this.inThisGame;
                            else
                                label.color = inOtherGame;
                        }
                        else
                        {
                            switch (state)
                            {
                                case EPersonaState.k_EPersonaStateOffline:
                                    label.color = isOffline;
                                    break;
                                case EPersonaState.k_EPersonaStateAway:
                                case EPersonaState.k_EPersonaStateBusy:
                                case EPersonaState.k_EPersonaStateInvisible:
                                case EPersonaState.k_EPersonaStateSnooze:
                                    label.color = isOnlineInactive;
                                    break;
                                default:
                                    label.color = isOnlineActive;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        [Serializable]
        public struct InputField
        {
            public TMPro.TMP_InputField label;
            [Header("Colors")]
            public bool useStatusColors;
            public Color inThisGame;
            public Color inOtherGame;
            public Color isOnlineActive;
            public Color isOnlineInactive;
            public Color isOffline;

            public void SetValue(string value, bool inGame, bool inThisGame, EPersonaState state)
            {
                if (label != null)
                {
                    label.text = value;

                    if (useStatusColors)
                    {
                        if (inGame)
                        {
                            if (inThisGame)
                                label.textComponent.color = this.inThisGame;
                            else
                                label.textComponent.color = inOtherGame;
                        }
                        else
                        {
                            switch (state)
                            {
                                case EPersonaState.k_EPersonaStateOffline:
                                    label.textComponent.color = isOffline;
                                    break;
                                case EPersonaState.k_EPersonaStateAway:
                                case EPersonaState.k_EPersonaStateBusy:
                                case EPersonaState.k_EPersonaStateInvisible:
                                case EPersonaState.k_EPersonaStateSnooze:
                                    label.textComponent.color = isOnlineInactive;
                                    break;
                                default:
                                    label.textComponent.color = isOnlineActive;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        [Serializable]
        public struct ImageField
        {
            public Image image;
            [Header("Colors")]
            public bool useStatusColors;
            public Color inThisGame;
            public Color inOtherGame;
            public Color isOnlineActive;
            public Color isOnlineInactive;
            public Color isOffline;

            public void SetValue(bool inGame, bool inThisGame, EPersonaState state)
            {
                if (image != null)
                {
                    if (useStatusColors)
                    {
                        if (inGame)
                        {
                            if (inThisGame)
                                image.color = this.inThisGame;
                            else
                                image.color = inOtherGame;
                        }
                        else
                        {
                            switch (state)
                            {
                                case EPersonaState.k_EPersonaStateOffline:
                                    image.color = isOffline;
                                    break;
                                case EPersonaState.k_EPersonaStateAway:
                                case EPersonaState.k_EPersonaStateBusy:
                                case EPersonaState.k_EPersonaStateInvisible:
                                case EPersonaState.k_EPersonaStateSnooze:
                                    image.color = isOnlineInactive;
                                    break;
                                default:
                                    image.color = isOnlineActive;
                                    break;
                            }
                        }
                    }
                }
            }
        }

        [Serializable]
        public struct MessageOptions
        {
            public string playingThis;
            public string playingOther;
            public bool nameOtherGame;
            public string inactive;
            public string active;
            public string offline;

            public string ToString(EPersonaState state, bool isPlaying, string gameName)
            {
                if (!string.IsNullOrEmpty(gameName))
                {
                    if (nameOtherGame)
                        return playingOther + gameName;
                    else
                        return gameName;
                }
                else if (isPlaying)
                    return playingThis;

                switch (state)
                {
                    case EPersonaState.k_EPersonaStateOffline:
                        return offline;
                    case EPersonaState.k_EPersonaStateAway:
                    case EPersonaState.k_EPersonaStateBusy:
                    case EPersonaState.k_EPersonaStateInvisible:
                    case EPersonaState.k_EPersonaStateSnooze:
                        return inactive;
                    default:
                        return active;
                }
            }
        }

        [SerializeField]
        [Tooltip("Should the component load the local user's avatar on Start.\nIf false you must call LoadAvatar and provide the ID of the user to load")]
        private bool useLocalUser;
        [Tooltip("If false then the display name field will get the Nickname if available and Friend name if not. If true then the display name will always be the Friend name and the nickname field will be used for nick if available.")]
        public bool appendNickname;
        public MessageOptions messageOptions = new MessageOptions
        {
            active = "Online",
            inactive = "Away",
            playingThis = "Playing",
            offline = "Offline",
            nameOtherGame = true,
            playingOther = "",
        };

        [Header("UI Elements")]
        [SerializeField]
        private RawImage avatar;
        [SerializeField]
        private TextField displayName = new TextField()
        {
            useStatusColors = true,
            inThisGame = new Color(0.8862f, 0.9960f, 0.7568f, 1),
            inOtherGame = new Color(0.5686f, 0.7607f, 0.3411f, 1),
            isOnlineActive = new Color(0.4117f, 0.7803f, 0.9254f, 1),
            isOnlineInactive = new Color(0.4117f, 0.7803f, 0.9254f, 1),
            isOffline = new Color(0.887f, 0.887f, 0.887f, 1)
        };
        [SerializeField]
        private TextField nickname = new TextField()
        {
            useStatusColors = false,
            inThisGame = new Color(0.5686f, 0.7607f, 0.3411f, 1),
            inOtherGame = new Color(0.5686f, 0.7607f, 0.3411f, 1),
            isOnlineActive = new Color(0.4117f, 0.7803f, 0.9254f, 1),
            isOnlineInactive = new Color(0.4117f, 0.7803f, 0.9254f, 1),
            isOffline = new Color(0.887f, 0.887f, 0.887f, 1)
        };
        [SerializeField]
        private TextField statusLabel = new TextField()
        {
            useStatusColors = true,
            inThisGame = new Color(0.5686f, 0.7607f, 0.3411f, 1),
            inOtherGame = new Color(0.5686f, 0.7607f, 0.3411f, 1),
            isOnlineActive = new Color(0.4117f, 0.7803f, 0.9254f, 1),
            isOnlineInactive = new Color(0.4117f, 0.7803f, 0.9254f, 1),
            isOffline = new Color(0.887f, 0.887f, 0.887f, 1)
        };
        [SerializeField]
        private ImageField statusImage = new ImageField()
        {
            useStatusColors = true,
            inThisGame = new Color(0.5686f, 0.7607f, 0.3411f, 1),
            inOtherGame = new Color(0.5686f, 0.7607f, 0.3411f, 1),
            isOnlineActive = new Color(0.4117f, 0.7803f, 0.9254f, 1),
            isOnlineInactive = new Color(0.4117f, 0.7803f, 0.9254f, 1),
            isOffline = new Color(0.887f, 0.887f, 0.887f, 1)
        };
        [SerializeField]
        private InputField friendId = new InputField()
        {
            useStatusColors = false,
            inThisGame = new Color(0.5686f, 0.7607f, 0.3411f, 1),
            inOtherGame = new Color(0.5686f, 0.7607f, 0.3411f, 1),
            isOnlineActive = new Color(0.4117f, 0.7803f, 0.9254f, 1),
            isOnlineInactive = new Color(0.4117f, 0.7803f, 0.9254f, 1),
            isOffline = new Color(0.887f, 0.887f, 0.887f, 1)
        };
        [SerializeField]
        private TextField level = new TextField()
        {
            useStatusColors = false,
            inThisGame = new Color(0.5686f, 0.7607f, 0.3411f, 1),
            inOtherGame = new Color(0.5686f, 0.7607f, 0.3411f, 1),
            isOnlineActive = new Color(0.4117f, 0.7803f, 0.9254f, 1),
            isOnlineInactive = new Color(0.4117f, 0.7803f, 0.9254f, 1),
            isOffline = new Color(0.887f, 0.887f, 0.887f, 1)
        };
        [SerializeField]
        private ImageField panel = new ImageField()
        {
            useStatusColors = true,
            inThisGame = new Color(0.5686f, 0.7607f, 0.3411f, 1),
            inOtherGame = new Color(0.5686f, 0.7607f, 0.3411f, 1),
            isOnlineActive = new Color(0.4117f, 0.7803f, 0.9254f, 1),
            isOnlineInactive = new Color(0.4117f, 0.7803f, 0.9254f, 1),
            isOffline = new Color(0.887f, 0.887f, 0.887f, 1)
        };

        [Header("Events")]
        public UnityEvent evtLoaded;

        public UserData UserData
        {
            get
            {
                return currentUser;
            }
            set
            {
                Apply(value);
            }
        }

        private UserData currentUser;

        private void OnEnable()
        {
            Friends.Client.EventPersonaStateChange.AddListener(HandlePersonaStateChange);
        }

        private void OnDisable()
        {
            Friends.Client.EventPersonaStateChange.RemoveListener(HandlePersonaStateChange);
        }

        private void Start()
        {
            if (API.App.Initialized)
            {
                if (useLocalUser)
                {
                    var user = API.User.Client.Id;
                    Apply(user);
                }
            }
            else
            {
                API.App.evtSteamInitialized.AddListener(DelayUpdate);
            }
        }

        private void DelayUpdate()
        {
            if (useLocalUser)
            {
                var user = API.User.Client.Id;
                Apply(user);
            }

            API.App.evtSteamInitialized.RemoveListener(DelayUpdate);
        }

        private void HandlePersonaStateChange(PersonaStateChange arg)
        {
            if (arg.SubjectId == currentUser)
            {
                UpdateUserData();
            }
        }

        public void Apply(UserData user)
        {
            currentUser = user;
            if (!currentUser.RequestInformation())
                UpdateUserData();
        }

        private void UpdateUserData()
        {
            var inGame = currentUser.GetGamePlayed(out FriendGameInfo gameInfo);
            var inThisGame = inGame && gameInfo.Game.App == API.App.Client.Id;
            var state = currentUser.State;

            if (!appendNickname)
            {
                if (nickname.label != null && nickname.label.gameObject.activeSelf)
                    nickname.label.gameObject.SetActive(false);

                displayName.SetValue(currentUser.Nickname, inGame, inThisGame, state);
            }
            else
            {
                var nick = currentUser.Nickname;
                var friend = currentUser.Name;
                if (friend != nick)
                {
                    if (nickname.label != null && !nickname.label.gameObject.activeSelf)
                        nickname.label.gameObject.SetActive(true);

                    displayName.SetValue(friend, inGame, inThisGame, state);
                    nickname.SetValue(nick, inGame, inThisGame, state);
                }
                else
                {
                    if (nickname.label != null && nickname.label.gameObject.activeSelf)
                        nickname.label.gameObject.SetActive(false);

                    displayName.SetValue(friend, inGame, inThisGame, state);
                }
            }

            friendId.SetValue(currentUser.FriendId.ToString(), inGame, inThisGame, state);
            var levelValue = currentUser.Level;
            if(levelValue == 0)
                level.SetValue("??", inGame, inThisGame, state);
            else
                level.SetValue(levelValue.ToString(), inGame, inThisGame, state);
            panel.SetValue(inGame, inThisGame, state);

            if (avatar != null)
            {
                currentUser.LoadAvatar((r) =>
                {
                    avatar.texture = r;
                    evtLoaded?.Invoke();
                });
            }

            if (!inThisGame)
            {
                if (!inGame)
                {
                    statusLabel.SetValue(messageOptions.ToString(state, inGame, string.Empty), inGame, inThisGame, state);
                    statusImage.SetValue(inGame, inThisGame, state);
                }
                else
                {
                    API.App.Web.GetAppName(gameInfo.Game.App, (r, e) =>
                    {
                        if (!e)
                        {
                            statusLabel.SetValue(messageOptions.ToString(state, inGame, r), inGame, inThisGame, state);
                            statusImage.SetValue(inGame, inThisGame, state);
                        }
                        else
                        {
                            statusLabel.SetValue(messageOptions.ToString(state, inGame, "Unknown"), inGame, inThisGame, state);
                            statusImage.SetValue(inGame, inThisGame, state);
                        }
                    });
                }
            }
            else
            {
                statusLabel.SetValue(messageOptions.ToString(state, inGame, string.Empty), inGame, inThisGame, state);
                statusImage.SetValue(inGame, inThisGame, state);
            }


        }
    }
}
#endif