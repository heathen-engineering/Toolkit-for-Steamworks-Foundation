#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using System;
using System.IO;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    [Serializable]
    public struct SteamGameServerConfiguraiton
    {
        public static SteamGameServerConfiguraiton Default
        {
            get
            {
                return new SteamGameServerConfiguraiton
                {
                    autoInitialize = true,
                    autoLogon = true,
                    ip = 0,
                    gamePort = 27015,
                    queryPort = 27016,
                    spectatorPort = 27017,
                    serverVersion = "1.0.0.0",
                    usingGameServerAuthApi = false,
                    enableHeartbeats = true,
                    supportSpectators = false,
                    spectatorServerName = string.Empty,
                    anonymousServerLogin = true,
                    gameServerToken = string.Empty,
                    isPasswordProtected = false,
                    serverName = string.Empty,
                    gameDescription = string.Empty,
                    gameDirectory = string.Empty,
                    isDedicated = false,
                    maxPlayerCount = 4,
                    botPlayerCount = 0,
                    mapName = string.Empty,
                    gameData = string.Empty,
                    rulePairs = null,
                };
            }
        }

        public bool autoInitialize;
        public bool autoLogon;
        public uint ip;
        public ushort gamePort;
        public ushort queryPort;
        public ushort spectatorPort;
        public string serverVersion;
        public bool usingGameServerAuthApi;
        public bool enableHeartbeats;
        public bool supportSpectators;
        public string spectatorServerName;
        public bool anonymousServerLogin;
        public string gameServerToken;
        public bool isPasswordProtected;
        public string serverName;
        public string gameDescription;
        public string gameDirectory;
        public bool isDedicated;
        public int maxPlayerCount;
        public int botPlayerCount;
        public string mapName;
        public string gameData;
        public StringKeyValuePair[] rulePairs;

        public string IpAddress
        {
            set
            {
                ip = API.Utilities.IPStringToUint(value);
            }
            get
            {
                return API.Utilities.IPUintToString(ip);
            }
        }

        public static SteamGameServerConfiguraiton Get() => API.App.Server.Configuration;

        public static bool Get(FileInfo configFile, out SteamGameServerConfiguraiton config)
        {
            try
            {
                if (configFile.Exists)
                {
                    config = JsonUtility.FromJson<SteamGameServerConfiguraiton>(File.ReadAllText(configFile.FullName));
                    return true;
                }
                else
                {
                    config = Default;
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                config = Default;
                return false;
            }
        }

        public static bool Get(string configFile, out SteamGameServerConfiguraiton config)
        {
            try
            {
                if (File.Exists(configFile))
                {
                    config = JsonUtility.FromJson<SteamGameServerConfiguraiton>(File.ReadAllText(configFile));
                    return true;
                }
                else
                {
                    config = Default;
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                config = Default;
                return false;
            }
        }

        public static bool Get(byte[] serializedData, out SteamGameServerConfiguraiton config)
        {
            try
            {
                if (serializedData != null && serializedData.Length > 0)
                {
                    config = JsonUtility.FromJson<SteamGameServerConfiguraiton>(System.Text.Encoding.UTF8.GetString(serializedData));
                    return true;
                }
                else
                {
                    config = Default;
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                config = Default;
                return false;
            }
        }

        public override string ToString() => JsonUtility.ToJson(this);

        public byte[] ToBytes() => System.Text.Encoding.UTF8.GetBytes(ToString());

        public bool SaveToDisk(string path)
        {
            try
            {
                File.WriteAllText(path, ToString());
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }
    }
}
#endif