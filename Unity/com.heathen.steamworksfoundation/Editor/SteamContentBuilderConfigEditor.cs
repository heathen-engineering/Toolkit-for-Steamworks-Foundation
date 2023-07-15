#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using UnityEngine;
using UnityEditor;
using System.IO;

namespace HeathenEngineering.SteamworksIntegration.Editors
{
    [CustomEditor(typeof(SteamContentBuilderConfig))]
    public class SteamContentBuilderConfigEditor : Editor
    {
        private static readonly string cipher = "simpleCipher";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var serializedObject = new SerializedObject(target);
            var config = target as SteamContentBuilderConfig;

            var sdkLocation = EditorPrefs.GetString("Steam SDK Path");
            GUILayout.Label("Content Builder Folder Path");
            var sdkP = EditorGUILayout.TextField(sdkLocation);
            if (GUILayout.Button("Download Steamworks SDK"))
                Application.OpenURL("https://partner.steamgames.com/downloads/steamworks_sdk.zip");

            if (sdkP != sdkLocation)
            {
                EditorPrefs.SetString("Steam SDK Path", sdkP);
                sdkLocation = sdkP;
            }

            if (!File.Exists($"{sdkLocation}\\builder\\steamcmd.exe"))
            {
                EditorGUILayout.HelpBox("Unable to locate SteamCmd.exe\nPlease specify the folder path of the sdk\\tools\\ContentBuilder", MessageType.Error);
            }

            GUILayout.Label("User");
            if (string.IsNullOrEmpty(config.username))
            {
                var user = EditorGUILayout.TextField("");

                if (!string.IsNullOrEmpty(user))
                {
                    config.username = SimpleCipher.Encrypt(user, cipher);
                    EditorUtility.SetDirty(config);
                }
            }
            else
            {
                var user = SimpleCipher.Decrypt(config.username, cipher);
                var nUser = EditorGUILayout.TextField(user);

                if(user != nUser)
                {
                    config.username = SimpleCipher.Encrypt(nUser, cipher);
                    EditorUtility.SetDirty(config);
                }
            }

            GUILayout.Label("Password");
            if (config.rememberPassword)
            {
                var tP = SessionState.GetString("Steam Password", string.Empty);
                if (!string.IsNullOrEmpty(tP))
                {
                    config.password = SimpleCipher.Encrypt(tP, cipher);
                    SessionState.SetString("Steam Password", string.Empty);
                }

                if (string.IsNullOrEmpty(config.password))
                {
                    var password = EditorGUILayout.PasswordField("");

                    if (!string.IsNullOrEmpty(password))
                    {
                        config.password = SimpleCipher.Encrypt(password, cipher);
                        EditorUtility.SetDirty(config);
                    }
                }
                else
                {
                    var password = SimpleCipher.Decrypt(config.password, cipher);
                    var nPassword = EditorGUILayout.PasswordField(password);

                    if (password != nPassword)
                    {
                        config.password = SimpleCipher.Encrypt(nPassword, cipher);
                        EditorUtility.SetDirty(config);
                    }
                }
            }
            else
            {
                if(!string.IsNullOrEmpty(config.password)) 
                {
                    SessionState.SetString("Steam Password", SimpleCipher.Decrypt(config.password, cipher));
                }

                config.password = string.Empty;
                EditorUtility.SetDirty(config);
                var password = SessionState.GetString("Steam Password", string.Empty);
                password = EditorGUILayout.PasswordField(password);
                SessionState.SetString("Steam Password", password);
            }
            var toggle = serializedObject.FindProperty(nameof(SteamContentBuilderConfig.rememberPassword));
            EditorGUILayout.PropertyField(toggle, new GUIContent("Remember Password"));

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            var appId = serializedObject.FindProperty(nameof(SteamContentBuilderConfig.targetApp));
            EditorGUILayout.PropertyField(appId, new GUIContent("Application ID"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif