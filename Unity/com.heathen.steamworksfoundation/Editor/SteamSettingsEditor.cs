#if !DISABLESTEAMWORKS && HE_SYSCORE && (STEAMWORKSNET || FACEPUNCH)
using Steamworks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration.Editors
{
    [CustomEditor(typeof(SteamSettings))]
    public class SteamSettingsEditor : Editor
    {
        public Texture2D dropBoxTexture;

        private SteamSettings settings;
        private bool sgsFoldout = false;
        bool needRefresh = false;
        private static bool achievements = false;
        private static bool inventoryFoldout = false;
        private static bool inputFoldout = false;
        private static bool inputActionFoldout = false;
        private static bool inputActionSetFoldout = false;
        private static bool inputActionSetLayerFoldout = false;
        private static bool itemsFoldout = false;
        private static bool bundlesFoldout = false;
        private static bool generatorFoldout = false;
        private static bool playtimegeneratorFoldout = false;
        private static bool taggeneratorFoldout = false;
        private static bool dlcFoldout = false;
        private static bool artifactFoldout = false;
        private static bool statsFoldout = false;
        private static bool leaderboardFoldout = false;

        public override void OnInspectorGUI()
        {
            settings = target as SteamSettings;

            if (needRefresh)
            {
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings), ImportAssetOptions.ForceUpdate);
                needRefresh = false;
            }

            ValidationChecks();
            DrawCommonSettings();
            DrawServerSettings();

        }

        private void ValidationChecks()
        {
            if (settings.server == null)
            {
                settings.server = new SteamSettings.GameServer();
                EditorUtility.SetDirty(settings);
            }

            if (settings.client == null)
            {
                settings.client = new SteamSettings.GameClient();
                EditorUtility.SetDirty(settings);
            }
        }

        private void DrawCommonSettings()
        {
            EditorGUILayout.BeginHorizontal();
            var debug = GUILayout.Toggle(settings.isDebugging, "Enable Debug Messages", EditorStyles.toolbarButton);
            if(settings.isDebugging != debug)
            {
                Undo.RecordObject(target, "editor");
                settings.isDebugging = debug;
                UnityEditor.EditorUtility.SetDirty(target);
            }
            EditorGUILayout.EndHorizontal();

            var id = EditorGUILayout.TextField("Application Id", settings.applicationId.m_AppId.ToString());
            uint buffer = 0;
            if (uint.TryParse(id, out buffer))
            {
                if (buffer != settings.applicationId.m_AppId)
                {
                    var appIdPath = new DirectoryInfo(Application.dataPath).Parent.FullName + "/steam_appid.txt";
                    File.WriteAllText(appIdPath, buffer.ToString());
                    Undo.RecordObject(target, "editor");
                    settings.applicationId = new AppId_t(buffer);
                    EditorUtility.SetDirty(target);
                    Debug.LogWarning("When updating the App ID we also update the steam_appid.txt for you. You must restart Unity and Visual Studio for this change to take full effect as seen from the Steamworks Client.");
                }
            }

            artifactFoldout = EditorGUILayout.Foldout(artifactFoldout, "Artifacts");

            if (artifactFoldout)
            {
                int il = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;

                DrawInputArea();
                DrawStatsList();
                DrawLeaderboardList();
                DrawAchievementList();
                DrawDLCList();
                DrawInventoryArea();

                EditorGUI.indentLevel = il;
            }
        }

        private void DrawServerSettings()
        {
            sgsFoldout = EditorGUILayout.Foldout(sgsFoldout, "Steam Game Server Configuraiton");

            if (sgsFoldout)
            {
                EditorGUILayout.Space();
                DrawServerToggleSettings();
                EditorGUILayout.Space();
                DrawConnectionSettings();
                EditorGUILayout.Space();
                DrawServerGeneralSettings();
            }
        }

        private void DrawServerGeneralSettings()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("General", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            if (!settings.server.anonymousServerLogin)
            {
                EditorGUILayout.HelpBox("If anonymous server login is not enabled then you must provide a game server token.", MessageType.Info);

                var token = EditorGUILayout.TextField("Token", settings.server.gameServerToken);

                if (token != settings.server.gameServerToken)
                {
                    Undo.RecordObject(target, "editor");
                    settings.server.gameServerToken = token;
                }
            }

            var serverName = EditorGUILayout.TextField("Server Name", settings.server.serverName);

            if (serverName != settings.server.serverName)
            {
                Undo.RecordObject(target, "editor");
                settings.server.serverName = serverName;
            }

            if (settings.server.supportSpectators)
            {
                serverName = EditorGUILayout.TextField("Spectator Name", settings.server.spectatorServerName);

                if (serverName != settings.server.spectatorServerName)
                {
                    Undo.RecordObject(target, "editor");
                    settings.server.spectatorServerName = serverName;
                }
            }

            serverName = EditorGUILayout.TextField("Description", settings.server.gameDescription);

            if (serverName != settings.server.gameDescription)
            {
                Undo.RecordObject(target, "editor");
                settings.server.gameDescription = serverName;
            }

            serverName = EditorGUILayout.TextField("Directory", settings.server.gameDirectory);

            if (serverName != settings.server.gameDirectory)
            {
                Undo.RecordObject(target, "editor");
                settings.server.gameDirectory = serverName;
            }

            serverName = EditorGUILayout.TextField("Map Name", settings.server.mapName);

            if (serverName != settings.server.mapName)
            {
                Undo.RecordObject(target, "editor");
                settings.server.mapName = serverName;
            }

            serverName = EditorGUILayout.TextField("Game Metadata", settings.server.gameData);

            if (serverName != settings.server.gameData)
            {
                Undo.RecordObject(target, "editor");
                settings.server.gameData = serverName;
            }

            var count = EditorGUILayout.TextField("Max Player Count", settings.server.maxPlayerCount.ToString());
            int buffer;
            if (int.TryParse(count, out buffer) && buffer != settings.server.maxPlayerCount)
            {
                Undo.RecordObject(target, "editor");
                settings.server.maxPlayerCount = buffer;
            }

            count = EditorGUILayout.TextField("Bot Player Count", settings.server.botPlayerCount.ToString());

            if (int.TryParse(count, out buffer) && buffer != settings.server.botPlayerCount)
            {
                Undo.RecordObject(target, "editor");
                settings.server.botPlayerCount = buffer;
            }
        }

        private void DrawConnectionSettings()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Connection", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            var address = API.Utilities.IPUintToString(settings.server.ip);
            var nAddress = EditorGUILayout.TextField("IP Address", address);
            
            if(address != nAddress)
            {
                try
                {
                    var nip = API.Utilities.IPStringToUint(nAddress);
                    Undo.RecordObject(target, "editor");
                    settings.server.ip = nip;
                }
                catch { }
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Ports ");
            EditorGUILayout.EndHorizontal();

            var port = EditorGUILayout.TextField(new GUIContent("Game", "The port that clients will connect to for gameplay.  You will usually open up your own socket bound to this port."), settings.server.gamePort.ToString());
            ushort nPort;

            if (ushort.TryParse(port, out nPort) && nPort != settings.server.gamePort)
            {
                Undo.RecordObject(target, "editor");
                settings.server.gamePort = nPort;
            }

            port = EditorGUILayout.TextField(new GUIContent("Query", "The port that will manage server browser related duties and info pings from clients.\nIf you pass MASTERSERVERUPDATERPORT_USEGAMESOCKETSHARE (65535) for QueryPort, then it will use 'GameSocketShare' mode, which means that the game is responsible for sending and receiving UDP packets for the master server updater. See references to GameSocketShare in isteamgameserver.hn"), settings.server.queryPort.ToString());
            
            if(ushort.TryParse(port, out nPort) && nPort != settings.server.queryPort)
            {
                Undo.RecordObject(target, "editor");
                settings.server.queryPort = nPort;
            }

            port = EditorGUILayout.TextField("Spectator", settings.server.spectatorPort.ToString());

            if (ushort.TryParse(port, out nPort) && nPort != settings.server.spectatorPort)
            {
                Undo.RecordObject(target, "editor");
                settings.server.spectatorPort = nPort;
            }
        }

        private void DrawServerToggleSettings()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Features", EditorStyles.whiteLabel, GUILayout.Width(250));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            var autoInt = GUILayout.Toggle(settings.server.autoInitialize, (settings.server.autoInitialize ? "Disable" : "Enable") + " Auto-Initialize", EditorStyles.toolbarButton);
            var autoLog = GUILayout.Toggle(settings.server.autoLogon, (settings.server.autoLogon ? "Disable" : "Enable") + " Auto-Logon", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var heart = GUILayout.Toggle(settings.server.enableHeartbeats, (settings.server.enableHeartbeats ? "Disable" : "Enable") + " Server Heartbeat", EditorStyles.toolbarButton);
            var anon = GUILayout.Toggle(settings.server.anonymousServerLogin, (settings.server.anonymousServerLogin ? "Disable" : "Enable") + " Anonymous Server Login", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();            
            var gsAuth = GUILayout.Toggle(settings.server.usingGameServerAuthApi, (settings.server.usingGameServerAuthApi ? "Disable" : "Enable") + " Game Server Auth API", EditorStyles.toolbarButton);
            var pass = GUILayout.Toggle(settings.server.isPasswordProtected, (settings.server.isPasswordProtected ? "Disable" : "Enable") + " Password Protected", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            var dedicated = GUILayout.Toggle(settings.server.isDedicated, (settings.server.isDedicated ? "Disable" : "Enable") + " Dedicated Server", EditorStyles.toolbarButton);
            var spec = GUILayout.Toggle(settings.server.supportSpectators, (settings.server.supportSpectators ? "Disable" : "Enable") + " Spectator Support", EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            //var mirror = GUILayout.Toggle(settings.server.enableMirror, (settings.server.enableMirror ? "Disable" : "Enable") + " Mirror Support", EditorStyles.toolbarButton);

            if (autoInt != settings.server.autoInitialize)
            {
                Undo.RecordObject(target, "editor");
                settings.server.autoInitialize = autoInt;
                EditorUtility.SetDirty(target);
            }

            if (heart != settings.server.enableHeartbeats)
            {
                Undo.RecordObject(target, "editor");
                settings.server.enableHeartbeats = heart;
                EditorUtility.SetDirty(target);
            }

            if (spec != settings.server.supportSpectators)
            {
                Undo.RecordObject(target, "editor");
                settings.server.supportSpectators = spec;
                EditorUtility.SetDirty(target);
            }

            if (anon != settings.server.anonymousServerLogin)
            {
                Undo.RecordObject(target, "editor");
                settings.server.anonymousServerLogin = anon;
                EditorUtility.SetDirty(target);
            }

            if (gsAuth != settings.server.usingGameServerAuthApi)
            {
                Undo.RecordObject(target, "editor");
                settings.server.usingGameServerAuthApi = gsAuth;
                EditorUtility.SetDirty(target);
            }

            if (pass != settings.server.isPasswordProtected)
            {
                Undo.RecordObject(target, "editor");
                settings.server.isPasswordProtected = pass;
                EditorUtility.SetDirty(target);
            }

            if (dedicated != settings.server.isDedicated)
            {
                Undo.RecordObject(target, "editor");
                settings.server.isDedicated = dedicated;
                EditorUtility.SetDirty(target);
            }

            if (autoLog != settings.server.autoLogon)
            {
                Undo.RecordObject(target, "editor");
                settings.server.autoLogon = autoLog;
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawStatsList()
        {

            statsFoldout = EditorGUILayout.Foldout(statsFoldout, "Stats: " + settings.stats.Count);

            if (statsFoldout)
            {
                int mil = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;

                var color = GUI.contentColor;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                GUI.contentColor = SteamSettings.Colors.BrightGreen;
                if (GUILayout.Button("+ Int", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    GUI.FocusControl(null);

                    IntStatObject nStat = ScriptableObject.CreateInstance<IntStatObject>();
                    nStat.name = "[Stat Int] New Int Stat";
                    nStat.statName = "New Int Stat";
                    AssetDatabase.AddObjectToAsset(nStat, settings);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                    settings.stats.Add(nStat);
                    EditorUtility.SetDirty(target);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                    EditorGUIUtility.PingObject(nStat);
                }
                if (GUILayout.Button("+ Float", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    GUI.FocusControl(null);

                    FloatStatObject nStat = ScriptableObject.CreateInstance<FloatStatObject>();
                    nStat.name = "[Stat Float] New Float Stat";
                    nStat.statName = "New Float Stat";
                    AssetDatabase.AddObjectToAsset(nStat, settings);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                    settings.stats.Add(nStat);
                    EditorUtility.SetDirty(target);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                    EditorGUIUtility.PingObject(nStat);
                }
                if (GUILayout.Button("+ Avg Rate", EditorStyles.toolbarButton, GUILayout.Width(75)))
                {
                    GUI.FocusControl(null);

                    AvgRateStatObject nStat = ScriptableObject.CreateInstance<AvgRateStatObject>();
                    nStat.name = "[Stat AvgRate] New Average Rate Stat";
                    nStat.statName = "New Average Rate Stat";
                    AssetDatabase.AddObjectToAsset(nStat, settings);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));

                    settings.stats.Add(nStat);
                    EditorUtility.SetDirty(target);
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                    EditorGUIUtility.PingObject(nStat);
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                int il = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;

                settings.stats.RemoveAll(p => p == null);

                for (int i = 0; i < settings.stats.Count; i++)
                {
                    var target = settings.stats[i];
                    if (target == null)
                        continue;

                    Color sC = GUI.backgroundColor;

                    GUI.backgroundColor = sC;
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
                    {
                        GUI.FocusControl(null);
                        EditorGUIUtility.PingObject(target);
                    }

                    var newName = EditorGUILayout.TextField(target.statName);
                    if (!string.IsNullOrEmpty(newName) && newName != target.statName)
                    {
                        Undo.RecordObject(target, "name change");
                        target.statName = newName;
                        switch (target.Type)
                        {
                            case StatObject.DataType.Int:
                                target.name = "[Stat Int] " + newName;
                                break;
                            case StatObject.DataType.Float:
                                target.name = "[Stat Float] " + newName;
                                break;
                            case StatObject.DataType.AvgRate:
                                target.name = "[Stat AvgRate] " + newName;
                                break;
                        }
                        EditorUtility.SetDirty(target);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                        EditorGUIUtility.PingObject(target);
                    }


                    var terminate = false;
                    GUI.contentColor = SteamSettings.Colors.ErrorRed;
                    if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                    {
                        GUI.FocusControl(null);
                        if (AssetDatabase.GetAssetPath(settings.stats[i]) == AssetDatabase.GetAssetPath(settings))
                        {
                            AssetDatabase.RemoveObjectFromAsset(settings.stats[i]);
                            needRefresh = true;
                        }

                        settings.stats.RemoveAt(i);
                        terminate = true;
                    }
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();

                    if (terminate)
                    {
                        EditorUtility.SetDirty(this.target);
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(this.target));
                        EditorGUIUtility.PingObject(this.target);
                        break;
                    }
                }
                EditorGUI.indentLevel = il;

                EditorGUI.indentLevel = mil;
            }
        }

        private void DrawAchievementList()
        {
            settings.achievements.RemoveAll(p => p == null);
            achievements = EditorGUILayout.Foldout(achievements, "Achievements: " + settings.achievements.Count);

            if (achievements)
            {
                var color = GUI.contentColor;
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                GUI.contentColor = color;
                if (GUILayout.Button("Import", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    try
                    {
                        GUI.FocusControl(null);

                        var names = API.StatsAndAchievements.Client.GetAchievementNames();

                        List<AchievementObject> toRemove = new List<AchievementObject>();
                        for (int i = 0; i < settings.achievements.Count; i++)
                        {
                            var achievement = settings.achievements[i];
                            if (!names.Contains(achievement.Id))
                            {
                                toRemove.Add(achievement);
                            }
                        }

                        while (toRemove.Count > 0)
                        {
                            var target = toRemove[0];
                            toRemove.Remove(target);
                            GUI.FocusControl(null);
                            if (AssetDatabase.GetAssetPath(target) == AssetDatabase.GetAssetPath(settings))
                            {
                                AssetDatabase.RemoveObjectFromAsset(target);
                                needRefresh = true;
                            }
                            settings.achievements.Remove(target);
                            EditorUtility.SetDirty(target);
                            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(settings));
                            EditorGUIUtility.PingObject(target);
                        }

                        for (int i = 0; i < names.Length; i++)
                        {
                            var achName = names[i];

                            var achObj = settings.achievements.FirstOrDefault(p => p.Id == achName);

                            bool created = false;
                            if (achObj == null)
                            {
                                achObj = ScriptableObject.CreateInstance<AchievementObject>();
                                created = true;
                            }

                            achObj.name = "[Ach] " + achName;
                            achObj.Id = achName;
                            achObj.Name = API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(achName, AchievementAttributes.name);
                            achObj.Description = API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(achName, AchievementAttributes.desc);
                            achObj.Hidden = API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(achName, AchievementAttributes.hidden) == "1";

                            if (created)
                            {
                                UnityEditor.AssetDatabase.AddObjectToAsset(achObj, SteamSettings.current);
                                settings.achievements.Add(achObj);
                            }

                            UnityEditor.EditorUtility.SetDirty(achObj);
                        }

                        UnityEditor.EditorUtility.SetDirty(target);
                        UnityEditor.EditorUtility.SetDirty(SteamSettings.current);
                        UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(SteamSettings.current));
                        UnityEditor.EditorUtility.SetDirty(target);

                        //settings.achievements.Add(nStat);
                        EditorUtility.SetDirty(target);
                        EditorGUIUtility.PingObject(target);
                    }
                    catch
                    {
                        Debug.LogWarning("Achievements can only be imported while the simulation is running, press play and try again to import.");
                    }
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                int il = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;

                for (int i = 0; i < settings.achievements.Count; i++)
                {
                    var target = settings.achievements[i];

                    if (target == null)
                        continue;

                    Color sC = GUI.backgroundColor;

                    GUI.backgroundColor = sC;
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("P", EditorStyles.toolbarButton, GUILayout.Width(20)))
                    {
                        GUI.FocusControl(null);
                        EditorGUIUtility.PingObject(target);
                    }

                    EditorGUILayout.LabelField(target.Id);
                    if (UnityEngine.Application.isPlaying && SteamSettings.Initialized)
                        EditorGUILayout.LabelField(target.IsAchieved ? "Unlocked" : "Locked");

                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel = il;
            }
        }

        private void DrawLeaderboardList()
        {
            leaderboardFoldout = EditorGUILayout.Foldout(leaderboardFoldout, "Leaderboards ");

            if (leaderboardFoldout)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.HelpBox("Steam Leaderboards are not supported in Foundaiton, you can upgrade to Steamworks Complete by becoming a GitHub Sponsor for $10.\n\nSponsors get instant access to all Heathen assets and the Heathen Standard Licnese.\n\nCancel your subscription at any time and keep what you have installed and the license to go with it.", MessageType.Info);

                if (GUILayout.Button("Become a GitHub Sponsor", EditorStyles.toolbarButton))
                {
                    Application.OpenURL("https://github.com/sponsors/heathen-engineering/sponsorships?tier_id=140443&preview=true");
                }
                if (GUILayout.Button("Learn More", EditorStyles.toolbarButton))
                {
                    Application.OpenURL("https://kb.heathenengineering.com/company/concepts/become-a-sponsor");
                }
                if (GUILayout.Button("Ask a Question", EditorStyles.toolbarButton))
                {
                    Application.OpenURL("https://discord.gg/6X3xrRc");
                }

                EditorGUI.indentLevel--;
            }            
        }

        private void DrawDLCList()
        {
            dlcFoldout = EditorGUILayout.Foldout(dlcFoldout, "Downloadable Content ");

            if (dlcFoldout)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.HelpBox("Steam DLC is not supported in Foundaiton, you can upgrade to Steamworks Complete by becoming a GitHub Sponsor for $10.\n\nSponsors get instant access to all Heathen assets and the Heathen Standard Licnese.\n\nCancel your subscription at any time and keep what you have installed and the license to go with it.", MessageType.Info);

                if (GUILayout.Button("Become a GitHub Sponsor", EditorStyles.toolbarButton))
                {
                    Application.OpenURL("https://github.com/sponsors/heathen-engineering/sponsorships?tier_id=140443&preview=true");
                }
                if (GUILayout.Button("Learn More", EditorStyles.toolbarButton))
                {
                    Application.OpenURL("https://kb.heathenengineering.com/company/concepts/become-a-sponsor");
                }
                if (GUILayout.Button("Ask a Question", EditorStyles.toolbarButton))
                {
                    Application.OpenURL("https://discord.gg/6X3xrRc");
                }

                EditorGUI.indentLevel--;
            }
        }

        private void DrawInventoryArea()
        {
            inventoryFoldout = EditorGUILayout.Foldout(inventoryFoldout, "Inventory ");

            if (inventoryFoldout)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.HelpBox("Steam Inventory is not supported in Foundaiton, you can upgrade to Steamworks Complete by becoming a GitHub Sponsor for $10.\n\nSponsors get instant access to all Heathen assets and the Heathen Standard Licnese.\n\nCancel your subscription at any time and keep what you have installed and the license to go with it.", MessageType.Info);

                if (GUILayout.Button("Become a GitHub Sponsor", EditorStyles.toolbarButton))
                {
                    Application.OpenURL("https://github.com/sponsors/heathen-engineering/sponsorships?tier_id=140443&preview=true");
                }
                if (GUILayout.Button("Learn More", EditorStyles.toolbarButton))
                {
                    Application.OpenURL("https://kb.heathenengineering.com/company/concepts/become-a-sponsor");
                }
                if (GUILayout.Button("Ask a Question", EditorStyles.toolbarButton))
                {
                    Application.OpenURL("https://discord.gg/6X3xrRc");
                }

                EditorGUI.indentLevel--;
            }
        }

        private void DrawInputArea()
        {
            //inputFoldout
            inputFoldout = EditorGUILayout.Foldout(inputFoldout, "Input");

            if(inputFoldout)
            {

                EditorGUI.indentLevel++;

                EditorGUILayout.HelpBox("Steam Input is not supported in Foundaiton, you can upgrade to Steamworks Complete by becoming a GitHub Sponsor for $10.\n\nSponsors get instant access to all Heathen assets and the Heathen Standard Licnese.\n Cancel your subscription at any time and keep what you have installed and the license to go with it.", MessageType.Info);

                if (GUILayout.Button("Become a GitHub Sponsor", EditorStyles.toolbarButton))
                {
                    Application.OpenURL("https://github.com/sponsors/heathen-engineering/sponsorships?tier_id=140443&preview=true");
                }
                if (GUILayout.Button("Learn More", EditorStyles.toolbarButton))
                {
                    Application.OpenURL("https://kb.heathenengineering.com/company/concepts/become-a-sponsor");
                }
                if (GUILayout.Button("Ask a Question", EditorStyles.toolbarButton))
                {
                    Application.OpenURL("https://discord.gg/6X3xrRc");
                }

                EditorGUI.indentLevel--;
            }
        }
    }
}
#endif