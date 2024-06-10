#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Friends = HeathenEngineering.SteamworksIntegration.API.Friends;

namespace HeathenEngineering.SteamworksIntegration.UI
{
    /// <summary>
    /// Friend profile is a simple implementation of the <see cref="IUserProfile"/> interface and is used in the prefab examples for <see cref="FriendList"/> and other controls. In most cases you will want to create your own "Friend Profile" UI script and can use the included FriendProfile as an example to get started with.
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/friend-profile")]
    public class FriendProfile : MonoBehaviour, IUserProfile
    {
        /// <summary>
        /// Represents a configurable text field in the friend profile
        /// </summary>
        [Serializable]
        public struct TextField
        {
            /// <summary>
            /// The label to display the text fields value
            /// </summary>
            [Tooltip("The label to display the fields value with")]
            public TMPro.TextMeshProUGUI label;
            /// <summary>
            /// should the system use specific colors for each status type
            /// </summary>
            [Header("Colors")]
            [Tooltip("Should the system use specific colors for each status type")]
            public bool useStatusColors;
            /// <summary>
            /// The color to use when the status of the player is playing a game and the game being played is this game.
            /// Only used when <see cref="useStatusColors"/> is <see cref="true"/>
            /// </summary>
            [Tooltip("The color to use when the status of the player is playing a game and the game being played is this game.\nOnly used when the Use Status Colors field is true.")]
            public Color inThisGame;
            /// <summary>
            /// The color to use when the status of the player is playing a game and the game being played is not this game.
            /// Only used when <see cref="useStatusColors"/> is <see cref="true"/>
            /// </summary>
            [Tooltip("The color to use when the status of the player is playing a game and the game being played is *NOT* this game.\nOnly used when the Use Status Colors field is true.")]
            public Color inOtherGame;
            /// <summary>
            /// The color to use when the status of the player is online and active.
            /// Only used when <see cref="useStatusColors"/> is <see cref="true"/>
            /// </summary>
            [Tooltip("The color to use when the status of the player is online and active.\nOnly used when the Use Status Colors field is true.")]
            public Color isOnlineActive;
            /// <summary>
            /// The color to use when the status of the player is online and inactive.
            /// Only used when <see cref="useStatusColors"/> is <see cref="true"/>
            /// </summary>
            [Tooltip("The color to use when the status of the player is online and inactive.\nOnly used when the Use Status Colors field is true.")]
            public Color isOnlineInactive;
            /// <summary>
            /// The color to use when the status of the player is offline.
            /// Only used when <see cref="useStatusColors"/> is <see cref="true"/>
            /// </summary>
            [Tooltip("The color to use when the status of the player is offline.\nOnly used when the Use Status Colors field is true.")]
            public Color isOffline;
            /// <summary>
            /// Sets the value of the field and its status
            /// </summary>
            /// <param name="value">The value to apply to the label</param>
            /// <param name="inGame">Is the player in game</param>
            /// <param name="inThisGame">If the player is in game, are they in this game</param>
            /// <param name="state">The persona state value for this player</param>
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

        /// <summary>
        /// Represents a configurable input field in the friend profile
        /// </summary>
        [Serializable]
        public struct InputField
        {
            /// <summary>
            /// The input field
            /// </summary>
            public TMPro.TMP_InputField label;
            /// <summary>
            /// should the system use specific colors for each status type
            /// </summary>
            [Header("Colors")]
            [Tooltip("Should the system use specific colors for each status type")]
            public bool useStatusColors;
            /// <summary>
            /// The color to use when the status of the player is playing a game and the game being played is this game.
            /// Only used when <see cref="useStatusColors"/> is <see cref="true"/>
            /// </summary>
            [Tooltip("The color to use when the status of the player is playing a game and the game being played is this game.\nOnly used when the Use Status Colors field is true.")]
            public Color inThisGame;
            /// <summary>
            /// The color to use when the status of the player is playing a game and the game being played is not this game.
            /// Only used when <see cref="useStatusColors"/> is <see cref="true"/>
            /// </summary>
            [Tooltip("The color to use when the status of the player is playing a game and the game being played is *NOT* this game.\nOnly used when the Use Status Colors field is true.")]
            public Color inOtherGame;
            /// <summary>
            /// The color to use when the status of the player is online and active.
            /// Only used when <see cref="useStatusColors"/> is <see cref="true"/>
            /// </summary>
            [Tooltip("The color to use when the status of the player is online and active.\nOnly used when the Use Status Colors field is true.")]
            public Color isOnlineActive;
            /// <summary>
            /// The color to use when the status of the player is online and inactive.
            /// Only used when <see cref="useStatusColors"/> is <see cref="true"/>
            /// </summary>
            [Tooltip("The color to use when the status of the player is online and inactive.\nOnly used when the Use Status Colors field is true.")]
            public Color isOnlineInactive;
            /// <summary>
            /// The color to use when the status of the player is offline.
            /// Only used when <see cref="useStatusColors"/> is <see cref="true"/>
            /// </summary>
            [Tooltip("The color to use when the status of the player is offline.\nOnly used when the Use Status Colors field is true.")]
            public Color isOffline;
            /// <summary>
            /// Sets the value of the field and its status
            /// </summary>
            /// <param name="value">The value to apply to the label</param>
            /// <param name="inGame">Is the player in game</param>
            /// <param name="inThisGame">If the player is in game, are they in this game</param>
            /// <param name="state">The persona state value for this player</param>
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

        /// <summary>
        /// Represents a configurable image field in the friend profile
        /// </summary>
        [Serializable]
        public struct ImageField
        {
            public Image image;
            /// <summary>
            /// should the system use specific colors for each status type
            /// </summary>
            [Header("Colors")]
            [Tooltip("Should the system use specific colors for each status type")]
            public bool useStatusColors;
            /// <summary>
            /// The color to use when the status of the player is playing a game and the game being played is this game.
            /// Only used when <see cref="useStatusColors"/> is <see cref="true"/>
            /// </summary>
            [Tooltip("The color to use when the status of the player is playing a game and the game being played is this game.\nOnly used when the Use Status Colors field is true.")]
            public Color inThisGame;
            /// <summary>
            /// The color to use when the status of the player is playing a game and the game being played is not this game.
            /// Only used when <see cref="useStatusColors"/> is <see cref="true"/>
            /// </summary>
            [Tooltip("The color to use when the status of the player is playing a game and the game being played is *NOT* this game.\nOnly used when the Use Status Colors field is true.")]
            public Color inOtherGame;
            /// <summary>
            /// The color to use when the status of the player is online and active.
            /// Only used when <see cref="useStatusColors"/> is <see cref="true"/>
            /// </summary>
            [Tooltip("The color to use when the status of the player is online and active.\nOnly used when the Use Status Colors field is true.")]
            public Color isOnlineActive;
            /// <summary>
            /// The color to use when the status of the player is online and inactive.
            /// Only used when <see cref="useStatusColors"/> is <see cref="true"/>
            /// </summary>
            [Tooltip("The color to use when the status of the player is online and inactive.\nOnly used when the Use Status Colors field is true.")]
            public Color isOnlineInactive;
            /// <summary>
            /// The color to use when the status of the player is offline.
            /// Only used when <see cref="useStatusColors"/> is <see cref="true"/>
            /// </summary>
            [Tooltip("The color to use when the status of the player is offline.\nOnly used when the Use Status Colors field is true.")]
            public Color isOffline;
            /// <summary>
            /// Sets the value of the field and its status
            /// </summary>
            /// <param name="value">The value to apply to the label</param>
            /// <param name="inGame">Is the player in game</param>
            /// <param name="inThisGame">If the player is in game, are they in this game</param>
            /// <param name="state">The persona state value for this player</param>
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
        /// <summary>
        /// Configuration settings for the message to display for each of the valid states
        /// </summary>
        [Serializable]
        public struct MessageOptions
        {
            /// <summary>
            /// The message to be displayed when the subject is playing this game
            /// </summary>
            [Tooltip("V")]
            public string playingThis;
            /// <summary>
            /// The message to be displayed when the subject is *NOT* playing this game
            /// </summary>
            [Tooltip("The message to be displayed when the subject is *NOT* playing this game. This is used if Name Other Game is false")]
            public string playingOther;
            /// <summary>
            /// If the player is playing some other game should the system name that game?
            /// </summary>
            [Tooltip("If true then the message will be the name of the game the player is playing if known, if false then the Playing Other message will be used.")]
            public bool nameOtherGame;
            /// <summary>
            /// The message to display when the subject is inactive
            /// </summary>
            [Tooltip("The message to display when the subject is inactive")]
            public string inactive;
            /// <summary>
            /// The message to display when the subject is active
            /// </summary>
            [Tooltip("The message to display when the subject is active")]
            public string active;
            /// <summary>
            /// The message to display when the subject is offline
            /// </summary>
            [Tooltip("The message to display when the subject is offline")]
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
        /// <summary>
        /// If false then the display name field will get the Nickname if available and Friend name if not. If true then the display name will always be the Friend name and the nickname field will be used for nick if available.
        /// </summary>
        [Tooltip("If false then the display name field will get the Nickname if available and Friend name if not. If true then the display name will always be the Friend name and the nickname field will be used for nick if available.")]
        public bool appendNickname;
        /// <summary>
        /// The message values to use for each of the possible status options
        /// </summary>
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
        /// <summary>
        /// An event that will be raised when the avatar image is freshly loaded
        /// </summary>
        [Header("Events")]
        public UnityEvent evtLoaded;
        /// <summary>
        /// The <see cref="UserData"/> of the currently displayed user if any
        /// </summary>
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
            if (!currentUser.IsValid)
                return;

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
            if (levelValue == 0)
            {
                level.SetValue("??", inGame, inThisGame, state);
                //Try again in 1 second, this will keep retrying untill it gets the level
                Invoke(nameof(UpdateUserData), 1);
            }
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