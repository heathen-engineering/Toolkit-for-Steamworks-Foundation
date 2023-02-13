#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using UnityEngine;
using UnityEngine.Events;
using Steamworks;
using UserAPI = HeathenEngineering.SteamworksIntegration.API.User.Client;
using FriendsAPI = HeathenEngineering.SteamworksIntegration.API.Friends.Client;
using System;

namespace HeathenEngineering.SteamworksIntegration.UI
{
    /// <summary>
    /// Applies the avatar of the indicated user to the attached RawImage
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.RawImage))]
    public class SetUserAvatar : MonoBehaviour
    {
        private UnityEngine.UI.RawImage image;
        [SerializeField]
        [Tooltip("Should the component load the local user's avatar on Start.\nIf false you must call LoadAvatar and provide the ID of the user to load")]
        private bool useLocalUser;
        public UnityEvent evtLoaded;

        public UserData UserData
        {
            get
            {
                return currentUser;
            }
            set
            {
                LoadAvatar(value);
            }
        }

        private UserData currentUser;

        private void OnEnable()
        {
            FriendsAPI.EventPersonaStateChange.AddListener(HandlePersonaStateChange);
        }

        private void OnDisable()
        {
            FriendsAPI.EventPersonaStateChange.RemoveListener(HandlePersonaStateChange);
        }
         
        private void Start()
        {
            image = GetComponent<UnityEngine.UI.RawImage>();

            if (useLocalUser)
            {
                if (API.App.Initialized)
                {
                    LoadAvatar(UserData.Me);
                }
                else
                    API.App.evtSteamInitialized.AddListener(HandleSteamInitalized);
            }
        }

        private void HandleSteamInitalized()
        {
            if (useLocalUser)
                LoadAvatar(UserData.Me);

            API.App.evtSteamInitialized.RemoveListener(HandleSteamInitalized);
        }

        private void HandlePersonaStateChange(PersonaStateChange arg)
        {
            if(FriendsAPI.PersonaChangeHasFlag(arg.Flags, EPersonaChange.k_EPersonaChangeAvatar)
                && arg.SubjectId == currentUser)
            {
                UserData user = arg.SubjectId;
                if(user == currentUser)
                {
                    user.LoadAvatar((t) =>
                    {
                        image.texture = t;
                        evtLoaded?.Invoke();
                    });
                }
            }
        }

        public void LoadAvatar(UserData user) => user.LoadAvatar((r) =>
        {
            if(image == null)
                image = GetComponent<UnityEngine.UI.RawImage>();

            if (image == null)
                return;

            currentUser = user;

            image.texture = r;
            evtLoaded?.Invoke();
        });

        public void LoadAvatar(CSteamID user) => UserData.Get(user).LoadAvatar((r) =>
        {
            if (image == null)
                image = GetComponent<UnityEngine.UI.RawImage>();

            if (image == null)
                return;

            currentUser = user;
            image.texture = r;
            evtLoaded?.Invoke();
        });

        public void LoadAvatar(ulong user) => UserData.Get(user).LoadAvatar((r) =>
        {
            if (image == null)
                image = GetComponent<UnityEngine.UI.RawImage>();

            if (image == null)
                return;

            currentUser = user;
            image.texture = r;
            evtLoaded?.Invoke();
        });
    }
}
#endif