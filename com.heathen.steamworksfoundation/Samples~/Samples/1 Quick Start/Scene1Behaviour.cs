#if HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH) && !DISABLESTEAMWORKS 
using UnityEngine;

namespace HeathenEngineering.DEMO
{
    /// <summary>
    /// This is for demonstration purposes only
    /// </summary>
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class Scene1Behaviour : MonoBehaviour
    {
        public void OpenKnowledgeBaseQuickStart()
        {
            Application.OpenURL("https://kb.heathenengineering.com/assets/steamworks/quick-start-guide");
        }
    }
}
#endif