#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration.UI
{
    [RequireComponent(typeof(TMPro.TMP_InputField))]
    public class FriendIdField : MonoBehaviour, IUserProfile
    {
        private TMPro.TMP_InputField label;
        [SerializeField]
        [Tooltip("Should the component load the local user's name on Start.\nIf false you must call SetName and provide the ID of the user to load")]
        private bool useLocalUser;

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

        private void Start()
        {
            label = GetComponent<TMPro.TMP_InputField>();

            if (useLocalUser)
            {
                if (API.App.Initialized)
                    Apply(UserData.Me);
                else
                    API.App.evtSteamInitialized.AddListener(HandleSteamInitalized);
            }
        }

        private void HandleSteamInitalized()
        {
            if (useLocalUser)
                Apply(UserData.Me);

            API.App.evtSteamInitialized.RemoveListener(HandleSteamInitalized);
        }

        public void Apply(UserData user)
        {
            if (label == null)
                label = GetComponent<TMPro.TMP_InputField>();

            if (label == null)
                return;

            currentUser = user;

            label.text = user.FriendId.ToString();
        }
    }
}
#endif