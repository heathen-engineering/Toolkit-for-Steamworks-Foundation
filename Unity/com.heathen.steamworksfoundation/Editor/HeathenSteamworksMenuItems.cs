#if !DISABLESTEAMWORKS
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration.Editors
{
    public class HeathenSteamworksMenuItems
    {
        [InitializeOnLoadMethod]
        public static void CheckForSteamworksInstall()
        {
            StartCoroutine(ValidateAndInstall());
        }

        private static IEnumerator ValidateAndInstall()
        {
            yield return null;
#if !HE_SYSCORE
            if (EditorUtility.DisplayDialog("Heathen Installer", "System Core appears to be missing and is required for Steamworks Complete to work properly. Should we install System Core now?", "Install", "No"))
            {
                yield return null;
                AddRequest sysProc = null;

                if (!SessionState.GetBool("SysCoreInstall", false))
                {
                    SessionState.SetBool("SysCoreInstall", true);
                    sysProc = Client.Add("https://github.com/heathen-engineering/SystemCore.git?path=/com.heathen.systemcore");
                }

                if (sysProc.Status == StatusCode.Failure)
                    Debug.LogError("PackageManager's System Core install failed, Error Message: " + sysProc.Error.message);
                else if (sysProc.Status == StatusCode.Success)
                    Debug.Log("System Core " + sysProc.Result.version + " installation complete");
                else
                {
                    Debug.Log("Installing System Core ...");
                    while (sysProc.Status == StatusCode.InProgress)
                    {
                        yield return null;
                    }
                }

                if (sysProc.Status == StatusCode.Failure)
                    Debug.LogError("PackageManager's System Core install failed, Error Message: " + sysProc.Error.message);
                else if (sysProc.Status == StatusCode.Success)
                    Debug.Log("System Core " + sysProc.Result.version + " installation complete");

                SessionState.SetBool("SysCoreInstall", false);
            }
#elif !STEAMWORKSNET
            if (EditorUtility.DisplayDialog("Heathen Installer", "No Steam API found, you can install Steamworks.NET, we can install Steamworks.NET for you.", "Install Steamworks.NET", "I'll do it my self"))
            {
                yield return null;
                AddRequest steamProc = null;

                if (!SessionState.GetBool("SteamInstall", false))
                {
                    SessionState.SetBool("SteamInstall", true);
                    steamProc = Client.Add("https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net");
                }

                if (steamProc.Status == StatusCode.Failure)
                    Debug.LogError("PackageManager Steamworks.NET install failed, Error Message: " + steamProc.Error.message);
                else if (steamProc.Status == StatusCode.Success)
                    Debug.Log("Steamworks.NET " + steamProc.Result.version + " installation complete");
                else
                {
                    Debug.Log("Installing Steamworks.NET ...");
                    while (steamProc.Status == StatusCode.InProgress)
                    {
                        yield return null;
                    }
                }

                if (steamProc.Status == StatusCode.Failure)
                    Debug.LogError("PackageManager Steamworks.NET install failed, Error Message: " + steamProc.Error.message);
                else if (steamProc.Status == StatusCode.Success)
                    Debug.Log("Steamworks.NET " + steamProc.Result.version + " installation complete");

                SessionState.SetBool("SteamInstall", false);
            }
#endif
        }

        [MenuItem("Help/Heathen/Steamworks/Update All Requirements (Package Manager)", priority = 0)]
        public static void InstallRequirements()
        {
            if (!SessionState.GetBool("SysCoreInstall", false)
                && !SessionState.GetBool("SteamInstall", false))
            {
                StartCoroutine(InstallAll());
            }
        }

        [MenuItem("Help/Heathen/Steamworks/Update System Core (Package Manager)", priority = 1)]
        public static void InstallSysCoreMenuItem()
        {
            if (!SessionState.GetBool("SysCoreInstall", false))
            {
                StartCoroutine(InstallSystemCore());
            }
        }

        [MenuItem("Help/Heathen/Steamworks/Update Steamworks.NET (Package Manager)", priority = 2)]
        public static void InstallSteamworksMenuItem()
        {
            if (!SessionState.GetBool("SteamInstall", false))
            {
                StartCoroutine(InstallSteamworks());
            }
        }

        [MenuItem("Help/Heathen/Steamworks/Documentation", priority = 3)]
        public static void Documentation()
        {
            Application.OpenURL("https://kb.heathenengineering.com/assets/steamworks");
        }

        [MenuItem("Help/Heathen/Steamworks/Support", priority = 4)]
        public static void Support()
        {
            Application.OpenURL("https://discord.gg/RMGtDXV");
        }

        private static IEnumerator InstallAll()
        {
            yield return null;
            AddRequest sysProc = null;

            if (!SessionState.GetBool("SysCoreInstall", false))
            {
                SessionState.SetBool("SysCoreInstall", true);
                sysProc = Client.Add("https://github.com/heathen-engineering/SystemCore.git?path=/com.heathen.systemcore");
            }

            if (sysProc.Status == StatusCode.Failure)
                Debug.LogError("PackageManager's System Core install failed, Error Message: " + sysProc.Error.message);
            else if (sysProc.Status == StatusCode.Success)
                Debug.Log("System Core " + sysProc.Result.version + " installation complete");
            else
            {
                Debug.Log("Installing System Core ...");
                while (sysProc.Status == StatusCode.InProgress)
                {
                    yield return null;
                }
            }

            if (sysProc.Status == StatusCode.Failure)
                Debug.LogError("PackageManager's System Core install failed, Error Message: " + sysProc.Error.message);
            else if (sysProc.Status == StatusCode.Success)
                Debug.Log("System Core " + sysProc.Result.version + " installation complete");

            SessionState.SetBool("SysCoreInstall", false);

            yield return null;
            AddRequest steamProc = null;

            if (!SessionState.GetBool("SteamInstall", false))
            {
                SessionState.SetBool("SteamInstall", true);
                steamProc = Client.Add("https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net");
            }

            if (steamProc.Status == StatusCode.Failure)
                Debug.LogError("PackageManager Steamworks.NET install failed, Error Message: " + steamProc.Error.message);
            else if (steamProc.Status == StatusCode.Success)
                Debug.Log("Steamworks.NET " + steamProc.Result.version + " installation complete");
            else
            {
                Debug.Log("Installing Steamworks.NET ...");
                while (steamProc.Status == StatusCode.InProgress)
                {
                    yield return null;
                }
            }

            if (steamProc.Status == StatusCode.Failure)
                Debug.LogError("PackageManager Steamworks.NET install failed, Error Message: " + steamProc.Error.message);
            else if (steamProc.Status == StatusCode.Success)
                Debug.Log("Steamworks.NET " + steamProc.Result.version + " installation complete");

            SessionState.SetBool("SteamInstall", false);
        }

        private static IEnumerator InstallSystemCore()
        {
            yield return null;
            AddRequest sysProc = null;

            if (!SessionState.GetBool("SysCoreInstall", false))
            {
                SessionState.SetBool("SysCoreInstall", true);
                sysProc = Client.Add("https://github.com/heathen-engineering/SystemCore.git?path=/com.heathen.systemcore");
            }

            if (sysProc.Status == StatusCode.Failure)
                Debug.LogError("PackageManager's System Core install failed, Error Message: " + sysProc.Error.message);
            else if (sysProc.Status == StatusCode.Success)
                Debug.Log("System Core " + sysProc.Result.version + " installation complete");
            else
            {
                Debug.Log("Installing System Core ...");
                while (sysProc.Status == StatusCode.InProgress)
                {
                    yield return null;
                }
            }

            if (sysProc.Status == StatusCode.Failure)
                Debug.LogError("PackageManager's System Core install failed, Error Message: " + sysProc.Error.message);
            else if (sysProc.Status == StatusCode.Success)
                Debug.Log("System Core " + sysProc.Result.version + " installation complete");

            SessionState.SetBool("SysCoreInstall", false);
        }

        private static IEnumerator InstallSteamworks()
        {
            yield return null;
            AddRequest steamProc = null;

            if (!SessionState.GetBool("SteamInstall", false))
            {
                SessionState.SetBool("SteamInstall", true);
                steamProc = Client.Add("https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net");
            }

            if (steamProc.Status == StatusCode.Failure)
                Debug.LogError("PackageManager Steamworks.NET install failed, Error Message: " + steamProc.Error.message);
            else if (steamProc.Status == StatusCode.Success)
                Debug.Log("Steamworks.NET " + steamProc.Result.version + " installation complete");
            else
            {
                Debug.Log("Installing Steamworks.NET ...");
                while (steamProc.Status == StatusCode.InProgress)
                {
                    yield return null;
                }
            }

            if (steamProc.Status == StatusCode.Failure)
                Debug.LogError("PackageManager Steamworks.NET install failed, Error Message: " + steamProc.Error.message);
            else if (steamProc.Status == StatusCode.Success)
                Debug.Log("Steamworks.NET " + steamProc.Result.version + " installation complete");

            SessionState.SetBool("SteamInstall", false);
        }

        private static List<IEnumerator> coroutines;

        private static void StartCoroutine(IEnumerator handle)
        {
            if (coroutines == null)
            {
                EditorApplication.update -= EditorUpdate;
                EditorApplication.update += EditorUpdate;
                coroutines = new List<IEnumerator>();
            }

            coroutines.Add(handle);
        }

        private static void EditorUpdate()
        {
            List<IEnumerator> done = new List<IEnumerator>();

            if (coroutines != null)
            {
                foreach (var e in coroutines)
                {
                    if (!e.MoveNext())
                        done.Add(e);
                    else
                    {
                        if (e.Current != null)
                            Debug.Log(e.Current.ToString());
                    }
                }
            }

            foreach (var d in done)
                coroutines.Remove(d);
        }
    }
}
#endif
