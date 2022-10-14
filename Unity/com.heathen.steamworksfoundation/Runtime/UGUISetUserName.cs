#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using UnityEngine;
using Steamworks;
using UserAPI = HeathenEngineering.SteamworksIntegration.API.User.Client;
using FriendsAPI = HeathenEngineering.SteamworksIntegration.API.Friends.Client;

namespace HeathenEngineering.SteamworksIntegration.UI
{
    /// <summary>
    /// Applies the name of the indicated user to the attached label
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    public class UGUISetUserName : MonoBehaviour
    {
        private UnityEngine.UI.Text label;
        [SerializeField]
        [Tooltip("Should the component load the local user's name on Start.\nIf false you must call SetName and provide the ID of the user to load")]
        private bool useLocalUser;

        public bool ShowNickname
        {
            get
            {
                return showNickname;
            }
            set
            {
                showNickname = value;
                SetName(currentUser);
            }
        }

        [SerializeField]
        [Tooltip("Should we show the profile name (set by the user this represents) or the nick name (set by the local user for this user)")]
        private bool showNickname;

        public UserData UserData
        {
            get
            {
                return currentUser;
            }
            set
            {
                SetName(value);
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
            label = GetComponent<UnityEngine.UI.Text>();

            if (useLocalUser)
            {
                var user = UserAPI.Id;
                SetName(user);
            }
        }

        private void HandlePersonaStateChange(PersonaStateChange_t arg)
        {
            UserData user = arg.m_ulSteamID;
            if ((FriendsAPI.PersonaChangeHasFlag(arg.m_nChangeFlags, EPersonaChange.k_EPersonaChangeName) || FriendsAPI.PersonaChangeHasFlag(arg.m_nChangeFlags, EPersonaChange.k_EPersonaChangeNickname))
                && user == currentUser)
            {
                if (showNickname)
                    label.text = user.Nickname;
                else
                    label.text = user.Name;
            }
        }

        public void SetName(UserData user)
        {
            if (label == null)
                label = GetComponent<UnityEngine.UI.Text>();

            if (label == null)
                return;

            currentUser = user;

            if (showNickname)
                label.text = user.Nickname;
            else
                label.text = user.Name;
        }

        public void SetName(CSteamID user)
        {
            if (label == null)
                label = GetComponent<UnityEngine.UI.Text>();

            if (label == null)
                return;

            currentUser = user;

            if (showNickname)
                label.text = UserData.Get(user).Nickname;
            else
                label.text = UserData.Get(user).Name;
        }

        public void SetName(ulong user)
        {
            if (label == null)
                label = GetComponent<UnityEngine.UI.Text>();

            if (label == null)
                return;

            currentUser = user;

            if (showNickname)
                label.text = UserData.Get(user).Nickname;
            else
                label.text = UserData.Get(user).Name;
        }
    }
}
#endif