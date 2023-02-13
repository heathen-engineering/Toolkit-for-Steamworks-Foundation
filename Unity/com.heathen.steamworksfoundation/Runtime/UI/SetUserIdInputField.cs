#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using System.Collections;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration.UI
{
    [RequireComponent(typeof(TMPro.TMP_InputField))]
    public class SetUserIdInputField : MonoBehaviour, IUserProfile
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