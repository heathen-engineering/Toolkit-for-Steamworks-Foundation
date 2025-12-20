#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HeathenEngineering.SteamworksIntegration.UI
{
    /// <summary>
    /// A simple dropdown like UI control that displays a list of player's that are available to invited to join a lobby
    /// </summary>
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/friend-invite-dropdown")]
    public class FriendInviteDropDown : MonoBehaviour
    {
        /// <summary>
        /// Flags used to filter the results of friends listed in the <see cref="FriendInviteDropDown"/>
        /// </summary>
        public struct FilterOptions
        {
            /// <summary>
            /// Show friends that are playing this game
            /// </summary>
            public bool inThisGame;
            /// <summary>
            /// Show friends that are playing some other game
            /// </summary>
            public bool inOtherGame;
            /// <summary>
            /// Show friends that are listed as busy
            /// </summary>
            public bool busy;
            /// <summary>
            /// Show friends that are listed as away
            /// </summary>
            public bool away;
            /// <summary>
            /// Show friends that are listed as snooze
            /// </summary>
            public bool snooze;
        }

        [SerializeField]
        private TMP_InputField inputField;
        [SerializeField]
        private Button dropdownButton;
        [SerializeField]
        private Button inviteButton;
        [SerializeField]
        private RectTransform panel;
        [SerializeField]
        private Transform content;
        [SerializeField]
        private GameObject template;
        /// <summary>
        /// What sorts of friends should be displayed
        /// </summary>
        public FilterOptions filter = new FilterOptions
        {
            inThisGame = true,
            inOtherGame = true,
            busy = false,
            away = true,
            snooze = true,
        };
        /// <summary>
        /// Invoked when the invite button has been pressed and provides information about the user for whom the button was pressed
        /// </summary>
        [Header("Events")]
        public UserDataEvent Invited = new UserDataEvent();
        /// <summary>
        /// Is the drop down expanded
        /// </summary>
        public bool IsExpanded
        {
            get => panel.gameObject.activeSelf;
            set
            {
                if (value)
                    Show();
                else
                    panel.gameObject.SetActive(false);
            }
        }
        /// <summary>
        /// What text is currently displayed in the field
        /// </summary>
        public string InputText
        {
            get => inputField.text;
            set => inputField.text = value;
        }

        private readonly List<GameObject> displayMembers = new List<GameObject>();
        private RectTransform selfTransform;

        private void Start()
        {
            selfTransform = GetComponent<RectTransform>();
            dropdownButton.onClick.AddListener(InternalHandleDropDownClick);
            inviteButton.onClick.AddListener(InternalInviteButtonClicked);
        }

        private void InternalInviteButtonClicked()
        {
            if(uint.TryParse(inputField.text, out var friendID))
            {
                var user = UserData.Get(friendID);
                if(user.IsValid)
                {
                    Invited.Invoke(user);
                }
            }
        }

        private void Update()
        {
            if (panel.gameObject.activeSelf
                && ((
#if ENABLE_INPUT_SYSTEM   
                Mouse.current.leftButton.wasPressedThisFrame
                && !RectTransformUtility.RectangleContainsScreenPoint(selfTransform, Mouse.current.position.ReadValue())
                && !RectTransformUtility.RectangleContainsScreenPoint(panel, Mouse.current.position.ReadValue())
#else
                Input.GetMouseButtonDown(0)
                && !RectTransformUtility.RectangleContainsScreenPoint(selfTransform, Input.mousePosition)
                && !RectTransformUtility.RectangleContainsScreenPoint(panel, Input.mousePosition)
#endif
                )
                ||
#if ENABLE_INPUT_SYSTEM
                Keyboard.current.escapeKey.wasPressedThisFrame
#else
                Input.GetKeyDown(KeyCode.Escape)
#endif
                ))
            {
                //And if so then we hide the panel and clear the to invite text field
                panel.gameObject.SetActive(false);
            }
        }

        private void InternalHandleDropDownClick()
        {
            if (panel.gameObject.activeSelf)
                panel.gameObject.SetActive(false);
            else
                Show();
        }

        /// <summary>
        /// Expand the drop down and show the list of friends
        /// </summary>
        public void Show()
        {
            foreach(var go in displayMembers)
                Destroy(go);

            var friends = API.Friends.Client.GetFriends(EFriendFlags.k_EFriendFlagImmediate);
            foreach(var friend in friends)
            {
                if(friend != UserData.Me)
                {
                    if (filter.inThisGame || filter.inOtherGame)
                    {
                        if (friend.GetGamePlayed(out var gameInfo))
                        {
                            if (!filter.inThisGame && gameInfo.Game.IsMe)
                                continue;
                            else if (!filter.inOtherGame && !gameInfo.Game.IsMe)
                                continue;

                            var go = Instantiate(template, content);
                            var fButton = go.GetComponent<UserInviteButton>();
                            fButton.SetFriend(friend);
                            fButton.Click.AddListener(FriendButtonClicked);
                            displayMembers.Add(go);
                            continue;
                        }
                    }

                    var state = friend.State;
                    if(state == EPersonaState.k_EPersonaStateOnline
                        || (state == EPersonaState.k_EPersonaStateBusy && filter.busy)
                        || (state == EPersonaState.k_EPersonaStateAway && filter.away)
                        || (state == EPersonaState.k_EPersonaStateSnooze && filter.snooze))
                    {
                        var go = Instantiate(template, content);
                        var fButton = go.GetComponent<UserInviteButton>();
                        fButton.SetFriend(friend);
                        fButton.Click.AddListener(FriendButtonClicked);
                        displayMembers.Add(go);
                    }
                }
            }

            panel.gameObject.SetActive(true);
        }

        /// <summary>
        /// Used internally when the invite button is clicked
        /// </summary>
        /// <param name="data"></param>
        public void FriendButtonClicked(UserAndPointerData data)
        {
            inputField.text = data.user.FriendId.ToString();
            panel.gameObject.SetActive(false);
        }
    }
}
#endif