#if !DISABLESTEAMWORKS && STEAMWORKS_NET
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    [HelpURL("https://kb.heathenengineering.com/assets/steamworks")]
    [DisallowMultipleComponent]
    public class SteamworksCreator : MonoBehaviour
    {
        public bool createOnStart;
        public bool markAsDoNotDestroy;
        public SteamSettings settings;

        private void Start()
        {
            if (createOnStart)
                settings.CreateBehaviour(markAsDoNotDestroy);
        }

        public void CreateIfMissing()
        {
            settings.CreateBehaviour(markAsDoNotDestroy);
        }
    }
}
#endif