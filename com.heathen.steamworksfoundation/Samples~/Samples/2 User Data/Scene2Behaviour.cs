#if HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH) && !DISABLESTEAMWORKS 
using UnityEngine;

namespace HeathenEngineering.DEMO
{
    /// <summary>
    /// This is for demonstration purposes only
    /// </summary>
    [System.Obsolete("This script is for demonstration purposes ONLY")]
    public class Scene2Behaviour : MonoBehaviour
    {
        public void OpenKnowledgeBaseUserData()
        {
            Application.OpenURL("https://kb.heathenengineering.com/assets/steamworks");
        }
    }
}
#endif