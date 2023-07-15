#if UNITY_EDITOR && !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using System;
using UnityEngine;

namespace OnlyInEditor
{
    [Obsolete("This class is onyl for use in Editor and will not compile for builds")]
    public class Heathen : MonoBehaviour
    {
        public void BecomeSponsor()
        {
            Application.OpenURL("https://kb.heathenengineering.com/company/concepts/become-a-sponsor");
        }

        public void JoinDiscord()
        {
            Application.OpenURL("https://discord.gg/6X3xrRc");
        }

        public void KnowledgeBase()
        {
            Application.OpenURL("https://kb.heathenengineering.com/company/introduction");
        }

        public void OpenWebPage(string url)
        {
            Application.OpenURL(url);
        }
    }
}
#endif