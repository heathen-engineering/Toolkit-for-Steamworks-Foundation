#if HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH) && !DISABLESTEAMWORKS 
using UnityEngine;

namespace HeathenEngineering.DEMO
{
    /// <summary>
    /// This is for demonstration purposes only
    /// </summary>
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class MarketingButtons : MonoBehaviour
    {
        public string[] urls;

        public void LeaveAReview()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/integration/steamworks-v2-complete-190316#reviews");
        }

        public void JoinDiscord()
        {
            Application.OpenURL("https://discord.gg/6X3xrRc");
        }

        public void UXComplete()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/utilities/ux-v2-complete-201905");
        }

        public void uGUIExtras()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/2d/gui/ux-v2-ugui-extras-202542");
        }

        public void PhysKit()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/physics/physkit-complete-122368");
        }

        public void Anibone()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/physics/physkit-anibone-173686");
        }

        public void Containers()
        {
            Application.OpenURL("https://assetstore.unity.com/packages/3d/props/heathen-containers-volume-1-150587");
        }

        public void URLButtons(int index)
        {
            Application.OpenURL(urls[index]);
        }
    }
}
#endif
