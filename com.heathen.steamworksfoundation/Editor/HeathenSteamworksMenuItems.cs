using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace HeathenEngineering.SteamworksFoundation.Editors
{
    public class HeathenSteamworksMenuItems
    {
        [InitializeOnLoadMethod]
        public static void InitOnLoadMethod()
        {
            StartCoroutine(InitalizeAndLoad());
        }

        [MenuItem("Help/Heathen/Steamworks/Update (Package Manager)", priority = 0)]
        public static void InstallFoundationMenuItem()
        {
            if (!SessionState.GetBool("SysCoreInstall", false))
            {
                StartCoroutine(InstallFoundation());
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

        [MenuItem("Help/Heathen/Steamworks/Source Code (GitHub)", priority = 3)]
        public static void SourceCode()
        {
            Application.OpenURL("https://github.com/heathen-engineering/SteamworksFoundation");
        }

        [MenuItem("Help/Heathen/Steamworks/Documentation", priority = 4)]
        public static void Documentation()
        {
            Application.OpenURL("https://kb.heathenengineering.com/assets/steamworks");
        }

        [MenuItem("Help/Heathen/Steamworks/Support", priority = 5)]
        public static void Support()
        {
            Application.OpenURL("https://discord.gg/RMGtDXV");
        }

        private static IEnumerator InitalizeAndLoad()
        {
            yield return null;

            if (!SessionState.GetBool("SteamFoundationDepCheck", false))
            {
                SessionState.SetBool("SteamFoundationDepCheck", true);
#if !HE_SYSCORE && !STEAMWORKS_NET
                if (EditorUtility.DisplayDialog("Heathen Installer", "System Core and Steamworks.NET do not appear to be installed. Both of these assets are requirements for Steamworks Foundation to work properly. Would you like to install both dependencies now?", "Install", "No"))
                {
                    yield return null;
                    AddRequest steamProc = null;
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

                    if (sysProc.Status == StatusCode.Failure)
                        Debug.LogError("PackageManager's System Core install failed, Error Message: " + sysProc.Error.message);
                    else if (sysProc.Status == StatusCode.Success)
                        Debug.Log("System Core " + sysProc.Result.version + " installation complete");

                    if (steamProc.Status == StatusCode.Failure)
                        Debug.LogError("PackageManager Steamworks.NET install failed, Error Message: " + steamProc.Error.message);
                    else if (steamProc.Status == StatusCode.Success)
                        Debug.Log("Steamworks.NET " + steamProc.Result.version + " installation complete");

                    SessionState.SetBool("SysCoreInstall", false);
                    SessionState.SetBool("SteamInstall", false);
                }
#elif !HE_SYSCORE || true
                if (EditorUtility.DisplayDialog("Heathen Installer", "System Core do not appear to be installed. System Core is a requirement for Steamworks Foundation to work properly. Would you like to install this dependencies now?", "Install", "No"))
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
#elif !STEAMWORKS_NET
                if (EditorUtility.DisplayDialog("Heathen Installer", "Steamworks.NET do not appear to be installed. Steamworks.NET is a requirement for Steamworks Foundation to work properly. Would you like to install this dependencies now?", "Install", "No"))
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

#if HE_SYSCORE && STEAMWORKS_NET
            string currentDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            HashSet<string> defines = new HashSet<string>(currentDefines.Split(';'))
            {
                "HE_STEAMFOUNDATION"
            };

            string newDefines = string.Join(";", defines);
            if (newDefines != currentDefines)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newDefines);
            }
#endif
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

        private static IEnumerator InstallFoundation()
        {
            yield return null;
            AddRequest steamProc = null;

            if (!SessionState.GetBool("SteamFoundation", false))
            {
                SessionState.SetBool("SteamFoundation", true);
                steamProc = Client.Add("https://github.com/heathen-engineering/SteamworksFoundation.git?path=/com.heathen.steamworksfoundation");
            }

            if (steamProc.Status == StatusCode.Failure)
                Debug.LogError("PackageManager Steamworks Foundation install failed, Error Message: " + steamProc.Error.message);
            else if (steamProc.Status == StatusCode.Success)
                Debug.Log("Steamworks Foundation " + steamProc.Result.version + " installation complete");
            else
            {
                Debug.Log("Installing Steamworks Foundation ...");
                while (steamProc.Status == StatusCode.InProgress)
                {
                    yield return null;
                }
            }

            if (steamProc.Status == StatusCode.Failure)
                Debug.LogError("PackageManager Steamworks Foundation install failed, Error Message: " + steamProc.Error.message);
            else if (steamProc.Status == StatusCode.Success)
                Debug.Log("Steamworks Foundation " + steamProc.Result.version + " installation complete");

            SessionState.SetBool("SteamFoundation", false);
        }

        private static List<IEnumerator> cooroutines;

        private static void StartCoroutine(IEnumerator handle)
        {
            if (cooroutines == null)
            {
                EditorApplication.update -= EditorUpdate;
                EditorApplication.update += EditorUpdate;
                cooroutines = new List<IEnumerator>();
            }

            cooroutines.Add(handle);
        }


        private static void EditorUpdate()
        {
            List<IEnumerator> done = new List<IEnumerator>();

            if (cooroutines != null)
            {
                foreach (var e in cooroutines)
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
                cooroutines.Remove(d);
        }
    }
}
