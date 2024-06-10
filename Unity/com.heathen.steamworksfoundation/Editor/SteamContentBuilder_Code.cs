#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using UnityEditor;
using UnityEngine;
using System.Text;
using System.IO;

namespace HeathenEngineering.SteamworksIntegration.Editors
{
    public class SteamContentBuilder_Code : EditorWindow
    {
        public static SteamContentBuilder_Code instance;
        public static string sdkLocation;
        private static SteamContentBuilderConfig config;

        private static readonly string cipher = "simpleCipher";

        public Texture2D icon;
        System.Diagnostics.Process steamCMD;
        bool uploading = false;
        string twoFactor;
        bool showTwoFactor = false;
        StringBuilder output = new StringBuilder();
        string processingMessage = "Processing";
        float processingValue = 0f;
        int step = 0;

        private static string AppBuildVDF(string appId, string description, string depot, string platform)
        {
            var sb = new StringBuilder();
            sb.AppendLine("\"appbuild\"");
            sb.AppendLine("{");
            sb.AppendLine($"\t\"appid\"\t\"{appId}\"");
            sb.AppendLine($"\t\"desc\"\t\"{description}\"");
            sb.AppendLine($"\t\"contentroot\"\t\"..\\content\\{Application.productName}\\{platform}\"");
            sb.AppendLine($"\t\"buildoutput\"\t\"..\\output\\{Application.productName}\\{platform}\"");
            sb.AppendLine("\t\"depots\"");
            sb.AppendLine("\t{");
            sb.AppendLine($"\t\t\"{depot}\"");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\t\"FileMapping\"");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\t\"LocalPath\"\t\"*\"");
            sb.AppendLine("\t\t\t\t\"DepotPath\"\t\".\"");
            sb.AppendLine("\t\t\t\t\"recursive\"\t\"1\"");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t\t\"FileExclusion\"\t\"*.pdb\"");
            sb.AppendLine("\t\t}");
            sb.AppendLine("\t}");
            sb.AppendLine("}");
            return sb.ToString();
        }

        [MenuItem("File/Steam Build", priority = 212)]
        [MenuItem("Window/Steamworks/Builder")]
        public static void ShowWindow()
        {
            var version = SessionState.GetString("com.heathen.steamworks-version", "[unknown]");
            instance = GetWindow<SteamContentBuilder_Code>();
            instance.titleContent = new GUIContent($"Builder v{version}", instance.icon);
        }

        private void OnEnable()
        {
            var matches = AssetDatabase.FindAssets("t: SteamContentBuilderConfig");

            if (matches == null
                || matches.Length < 1)
            {
                Debug.Log("No Steam Content Builder Config was found. A new config will be created as Assets/Editor/Content Builder Config.asset");
                config = ScriptableObject.CreateInstance<SteamContentBuilderConfig>();
                if (!AssetDatabase.IsValidFolder("Assets/Editor"))
                    AssetDatabase.CreateFolder("Assets", "Editor");
                AssetDatabase.CreateAsset(config, $"Assets/Editor/Content Builder Config.asset");
                AssetDatabase.SaveAssets();
            }
            else if (matches != null
                && matches.Length > 1)
            {
                var key = matches[0];
                var path = AssetDatabase.GUIDToAssetPath(key);
                Debug.LogWarning("Multiple Steam Content Builder Config files where found. The first entry (" + path + ") will be used, please remove the other config files");
                config = AssetDatabase.LoadAssetAtPath<SteamContentBuilderConfig>(path);
            }
            else
            {
                var path = AssetDatabase.GUIDToAssetPath(matches[0]);
                config = AssetDatabase.LoadAssetAtPath<SteamContentBuilderConfig>(path);
            }

            if (config.targetApp == 0)
            {
                Undo.RecordObject(config, "Steam Content Builder Config App Id");
                config.targetApp = 480;
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssets();
            }
        }

        private void OnGUI()
        {
            sdkLocation = EditorPrefs.GetString("Steam SDK Path");

            if (config == null)
            {
                EditorGUILayout.HelpBox("No Steam Content Builder Configuration Found", MessageType.Error);
                return;
            }

            bool changed = false;
            var userTmp = string.IsNullOrEmpty(config.username) ? string.Empty : SimpleCipher.Decrypt(config.username, cipher);
            var pasTmp = config.rememberPassword ? (string.IsNullOrEmpty(config.password) ? string.Empty : SimpleCipher.Decrypt(config.password, cipher)) : SessionState.GetString("Steam Password", string.Empty);

            if (!File.Exists($"{sdkLocation}\\builder\\steamcmd.exe"))
            {
                EditorGUILayout.HelpBox("Unable to locate SteamCmd.exe\nPlease specify the folder path of the sdk\\tools\\ContentBuilder", MessageType.Error);
                if (GUILayout.Button("Update Configuration"))
                {
                    EditorGUIUtility.PingObject(config);
                    Selection.objects = new Object[1] { config };
                }
            }
            else
            {
                if (GUILayout.Button("View Configuration"))
                {
                    EditorGUIUtility.PingObject(config);
                    Selection.objects = new Object[1] { config };
                }
            }

            GUILayout.Label("User");
            var user = EditorGUILayout.TextField(userTmp);
            GUILayout.Label("Password");
            var password = EditorGUILayout.PasswordField(pasTmp);

            var toggle = EditorGUILayout.Toggle("Remember Password", config.rememberPassword);

            if (toggle != config.rememberPassword)
            {
                changed = true;
                config.rememberPassword = toggle;
            }

            if (user != userTmp
                || password != pasTmp)
            {
                Undo.RecordObject(config, "Steam Content Builder Config User/Pass");
                if (!string.IsNullOrEmpty(user))
                    config.username = SimpleCipher.Encrypt(user, cipher);
                else
                    config.username = string.Empty;

                if (config.rememberPassword)
                {
                    if (!string.IsNullOrEmpty(password))
                        config.password = SimpleCipher.Encrypt(password, cipher);
                    else
                        config.password = string.Empty;
                }
                else
                {
                    config.password = string.Empty;
                    SessionState.SetString("Steam Password", password);
                }

                changed = true;
            }

            GUILayout.Space(50);

            if (config.depots != null
                && config.depots.Count > 0)
            {
                GUILayout.Label("Build Target (Depot)");

                string[] depotsArray = new string[config.depots.Count];
                for (int i = 0; i < depotsArray.Length; i++)
                {
                    depotsArray[i] = $"{config.depots[i].name} ({config.depots[i].id})";
                }

                config.lastDepot = EditorGUILayout.Popup(config.lastDepot, depotsArray);
            }
            else
            {
                if (GUILayout.Button("Add Depot"))
                {
                    EditorGUIUtility.PingObject(config);
                    Selection.objects = new Object[1] { config };
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(config);
            }

            if (showTwoFactor)
            {
                TwoFactorInput();
            }

            if (uploading)
            {
                if ((waitingLoadingAPI && EditorApplication.timeSinceStartup - timeSinceLoadingAPI > 3))
                {
                    processingValue = 0f;
                    showTwoFactor = true;
                }

                if (!showTwoFactor)
                {
                    //Progress
                    var rect = EditorGUILayout.BeginHorizontal();
                    rect.height = 20;
                    EditorGUILayout.Space();
                    EditorGUI.ProgressBar(rect, processingValue, processingMessage);
                    EditorGUILayout.EndHorizontal();
                    Repaint();
                    GUILayout.Space(20);
                }

                if (GUILayout.Button("Cancel"))
                {
                    uploading = false;
                    twoFactor = string.Empty;
                    showTwoFactor = false;
                    processingMessage = "Processing";
                    processingValue = 0f;
                    step = 0;

                    if (steamCMD != null)
                    {
                        steamCMD.Close();
                        steamCMD.Dispose();
                        steamCMD = null;
                    }
                }
            }
            else
            {
                if (config.depots != null
                    && config.depots.Count > 0)
                {
                    var depot = config.depots[config.lastDepot];

                    GUILayout.Label($"Build Target Platform: {BuildPipeline.GetBuildTargetName(depot.target)} ({PlayerSettings.productName}{depot.extension})");
                    bool targetSupported = BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(depot.target), depot.target);
                    if (targetSupported)
                    {
                        int doThis = 0;
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Build & Upload", GUILayout.Width(150)))
                        {
                            doThis = 1;
                        }
                        if (GUILayout.Button("Upload", GUILayout.Width(150)))
                        {
                            doThis = 2;

                        }
                        GUILayout.EndHorizontal();

                        if (doThis == 1)
                        {
                            Build();
                        }
                        else if (doThis == 2)
                        {
                            var selectedDepot = config.depots[config.lastDepot];
                            string platform = "Windows";

                            if (selectedDepot.target is BuildTarget.StandaloneLinux64)
                                platform = "Linux";
                            if (selectedDepot.target is BuildTarget.StandaloneOSX)
                                platform = "Mac";

                            Upload(platform);
                        }
                    }
                    else
                    {
                        GUILayout.Label($"Is {depot.target} not installed?");
                    }
                }
            }
        }

        void TwoFactorInput()
        {
            GUILayout.Label("Two-factor Token");
            twoFactor = EditorGUILayout.TextField(twoFactor, GUILayout.MaxWidth(300));

            if (steamCMD != null
                && !string.IsNullOrEmpty(twoFactor) && twoFactor.Trim().Length == 5)
            {
                steamCMD.StandardInput.WriteLine(twoFactor.Trim());
                steamCMD.StandardInput.Flush();
                showTwoFactor = false;
            }
        }

        private static void DeleteDoNotShip(string path)
        {
            DirectoryInfo directory = new(path);
            if (!directory.Exists)
                return;

            foreach (DirectoryInfo sub in directory.GetDirectories())
            {
                if (sub.Name.ToLower().Contains("donotship")
                    || sub.Name.ToLower().Contains("dontship"))
                    sub.Delete(true);
                else
                    DeleteDoNotShip(sub.FullName);
            }
        }

        public static void DeleteSubFolders(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            if (!directory.Exists)
                return;

            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            {
                DeleteSubFolders(subDirectory.FullName);
                foreach (FileInfo file in subDirectory.GetFiles())
                    file.Delete();

                subDirectory.Delete(true);
            }

            foreach (FileInfo file in directory.GetFiles())
                file.Delete();
        }

        private void Build()
        {
            var selectedDepot = config.depots[config.lastDepot];

            string platform = "Windows";

            if (selectedDepot.target is BuildTarget.StandaloneLinux64)
                platform = "Linux";
            if (selectedDepot.target is BuildTarget.StandaloneOSX)
                platform = "Mac";

            string buildPath = $"{sdkLocation}\\content\\{Application.productName}\\{platform}\\";
            DeleteSubFolders(buildPath);


            var buildReport = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, $"{buildPath}{PlayerSettings.productName}{selectedDepot.extension}", selectedDepot.target, BuildOptions.None);

            if (buildReport.summary.totalErrors == 0)
            {
                DeleteDoNotShip(buildPath);

                Debug.Log("Build done");
                Upload(platform);
            }
            else
            {
                Debug.LogError("Build FAILED");
            }
        }

        private void Upload(string platform)
        {
            uploading = false;
            twoFactor = string.Empty;
            showTwoFactor = false;
            waitingLoadingAPI = false;
            timeSinceLoadingAPI = 0;
            processingMessage = "Processing";
            processingValue = 0f;
            step = 0;

            var user = string.IsNullOrEmpty(config.username) ? string.Empty : SimpleCipher.Decrypt(config.username, cipher);

            var password = config.rememberPassword ? (string.IsNullOrEmpty(config.password) ? string.Empty : SimpleCipher.Decrypt(config.password, cipher)) : SessionState.GetString("Steam Password", string.Empty);

            if (string.IsNullOrEmpty(sdkLocation) || string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
            {
                Debug.LogWarning("Can't upload no password or username set!");
                return;
            }

            steamCMD?.Close();
            steamCMD?.Dispose();
            steamCMD = null;

            Debug.Log("Starting Steam CMD");
            uploading = true;

            string BuildScripts = sdkLocation + "\\scripts\\simple_app_build.vdf";
            var selectedDepot = config.depots[config.lastDepot];



            string content = AppBuildVDF(config.targetApp.ToString(), Application.version, selectedDepot.id.ToString(), platform);
            File.WriteAllText(BuildScripts, content);

            steamCMD = new System.Diagnostics.Process();
            steamCMD.StartInfo.FileName = $"{sdkLocation}\\builder\\steamcmd.exe";
            steamCMD.StartInfo.Arguments = $"+login {user} {password} +run_app_build ..\\scripts\\simple_app_build.vdf +quit";
            steamCMD.StartInfo.RedirectStandardInput = true;
            steamCMD.StartInfo.RedirectStandardOutput = true;
            steamCMD.StartInfo.UseShellExecute = false;
            steamCMD.StartInfo.CreateNoWindow = true;
            steamCMD.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;

            steamCMD.Start();
            steamCMD.OutputDataReceived += SteamCMD_OutputDataReceived;
            steamCMD.Exited += SteamCMD_Exited;
            steamCMD.BeginOutputReadLine();
        }

        bool waitingLoadingAPI = false;
        double timeSinceLoadingAPI = 0;

        private void SteamCMD_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            output.AppendLine(e.Data);

            if (!string.IsNullOrEmpty(e.Data))
                Debug.Log(e.Data);

            if (e.Data.Contains("Steam Console"))
            {
                processingMessage = "Initializing Steam CMD";
                processingValue = 0f;
            }
            else if (e.Data.Contains("Building depot"))
            {
                processingMessage = "Building depot";
                processingValue = 0f;
            }
            else if (e.Data.Contains("Logging in"))
            {
                processingMessage = "logging in";
                processingValue = 0f;
                showTwoFactor = false;
            }
            else if (e.Data.Contains("Loading Steam API...OK"))
            {
                processingMessage = "Loading Steam API";
                processingValue = 0f;
                waitingLoadingAPI = true;
                timeSinceLoadingAPI = EditorApplication.timeSinceStartup;
            }
            else if (e.Data.Contains("Two-factor code:"))
            {
                processingValue = 0f;
                showTwoFactor = true;
            }
            else if (e.Data.Contains("Building file mapping"))
            {
                waitingLoadingAPI = false;
                processingMessage = "Building file mapping";
                processingValue = 0f;
                showTwoFactor = false;
                step = 2;
            }
            else if (step == 2)
            {
                waitingLoadingAPI = false;
                showTwoFactor = false;
                processingMessage = "Building file mapping";
                processingValue = 0f;

                if (e.Data.Contains("Scanning content"))
                    step = 3;
            }
            else if (step == 3)
            {
                waitingLoadingAPI = false;
                showTwoFactor = false;
                processingMessage = "Scanning content";
                if (e.Data.Contains("%)"))
                {
                    var li = e.Data.LastIndexOf("(") + 1;
                    if (int.TryParse(e.Data.Substring(li, e.Data.LastIndexOf("%") - li), out var pert))
                    {
                        processingValue = pert / 200f;
                    }
                }

                if (e.Data.Contains("Uploading content"))
                    step = 4;
            }
            else if (step == 4)
            {
                waitingLoadingAPI = false;
                showTwoFactor = false;
                processingMessage = "Uploading content";
                if (e.Data.Contains("%)"))
                {
                    var li = e.Data.LastIndexOf("(") + 1;
                    if (int.TryParse(e.Data.Substring(li, e.Data.LastIndexOf("%") - li), out var pert))
                    {
                        processingValue = 0.5f + (pert / 200f);
                    }
                }
            }
        }

        private void OnInspectorUpdate()
        {
            if (steamCMD != null)
            {
                if (steamCMD.HasExited)
                {
                    steamCMD.Close();
                    Debug.Log(output.ToString());
                    uploading = false;
                    output.Clear();
                    processingMessage = string.Empty;
                    processingValue = 0;
                    step = 0;
                    showTwoFactor = false;
                    twoFactor = string.Empty;
                    steamCMD.Dispose();
                    steamCMD = null;
                }
            }

            Repaint();
        }

        private void SteamCMD_Exited(object sender, System.EventArgs e)
        {
            Debug.Log("Steam CMD has exited,");
            uploading = false;
        }


    }
}
#endif