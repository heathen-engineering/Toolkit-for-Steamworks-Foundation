#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using System.Collections;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration.UI
{
    [RequireComponent(typeof(TMPro.TMP_InputField))]
    [HelpURL("https://kb.heathen.group/assets/steamworks/unity-engine/ui-components/set-user-id-input-field")]
    public class SetUserIdInputField : MonoBehaviour, IUserProfile
    {
        private TMPro.TMP_InputField label;
        [SerializeField]
        [Tooltip("Should the component load the local user's name on Start.\nIf false you must call SetName and provide the ID of the user to load")]
        private bool useLocalUser;
        [SerializeField]
        private bool asHex = false;
        /// <summary>
        /// Should the value display as a simple string or as a Hex value
        /// </summary>
        public bool AsHex
        {
            get => asHex;
            set
            {
                asHex = value;
                Apply(currentUser);
            }
        }
        /// <summary>
        /// The <see cref="UserData"/> for the user being displayed
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

        private void Start()
        {
            label = GetComponent<TMPro.TMP_InputField>();
            StartCoroutine(DelayUpdate());
        }

        private IEnumerator DelayUpdate()
        {
            yield return new WaitUntil(() => API.App.Initialized);
            if (useLocalUser)
            {
                var user = API.User.Client.Id;
                Apply(user);
            }
        }

        /// <summary>
        /// Set the user to display the ID for
        /// </summary>
        /// <param name="user">The user to be applied</param>
        public void Apply(UserData user)
        {
            if (label == null)
                label = GetComponent<TMPro.TMP_InputField>();

            if (label == null)
                return;

            currentUser = user;

            if (asHex)
                label.text = user.FriendId.ToString("X");
            else
                label.text = user.FriendId.ToString();
        }
    }
}
#endif