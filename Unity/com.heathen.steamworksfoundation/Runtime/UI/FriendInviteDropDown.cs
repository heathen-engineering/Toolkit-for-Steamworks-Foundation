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
    public class FriendInviteDropDown : MonoBehaviour
    {
        public struct FilterOptions
        {
            public bool inThisGame;
            public bool inOtherGame;
            public bool busy;
            public bool away;
            public bool snooze;
        }

        public TMP_InputField inputField;
        public Button dropdownButton;
        public Button inviteButton;
        public RectTransform panel;
        public Transform content;
        public GameObject template;
        public FilterOptions filter = new FilterOptions
        {
            inThisGame = true,
            inOtherGame = true,
            busy = false,
            away = true,
            snooze = true,
        };
        [Header("Events")]
        public UserDataEvent Invited = new UserDataEvent();

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
                            var fButton = go.GetComponent<FriendInviteButton>();
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
                        var fButton = go.GetComponent<FriendInviteButton>();
                        fButton.SetFriend(friend);
                        fButton.Click.AddListener(FriendButtonClicked);
                        displayMembers.Add(go);
                    }
                }
            }

            panel.gameObject.SetActive(true);
        }

        public void FriendButtonClicked(UserAndPointerData data)
        {
            inputField.text = data.user.FriendId.ToString();
            panel.gameObject.SetActive(false);
        }
    }
}
#endif