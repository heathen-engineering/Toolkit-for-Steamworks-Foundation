#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using UnityEngine;
using UnityEngine.UI;

namespace HeathenEngineering.DEMO
{
    [RequireComponent(typeof(Button))]
    public class OpenUrlButtonClick : MonoBehaviour
    {
        [SerializeField]
        private string url;

        private void Start()
        {   
            if (TryGetComponent<Button>(out var btn))
                btn.onClick.AddListener(HandleButtonClick);
        }

        private void OnDestroy()
        {
            if (TryGetComponent<Button>(out var btn))
                btn.onClick.RemoveListener(HandleButtonClick);
        }

        private void HandleButtonClick()
        {
            if (!string.IsNullOrEmpty(url))
                Application.OpenURL(url);
        }
    }
}
#endif