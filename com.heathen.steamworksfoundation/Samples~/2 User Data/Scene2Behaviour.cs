using UnityEngine;
using HeathenEngineering.SteamworksIntegration;

namespace HeathenEngineering.DEMO
{
    /// <summary>
    /// This is for demonstration purposes only
    /// </summary>
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class Scene2Behaviour : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.UI.RawImage avatarImage;
        [SerializeField]
        private TMPro.TextMeshProUGUI userName;

        private void Start()
        {
            UserData.Me.LoadAvatar(r => avatarImage.texture = r);
            userName.text = UserData.Me.Nickname;
        }

        public void OpenKnowledgeBaseUserData()
        {
            Application.OpenURL("https://kb.heathenengineering.com/assets/steamworks");
        }
    }
}
