#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using System;
using System.IO;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// Represents a Steam Game Server configuration.
    /// This can be easily read or write to disk as a simple text file in JSON format.
    /// </summary>
    [Serializable]
    public struct SteamGameServerConfiguration
    {
        public static SteamGameServerConfiguration Default
        {
            get
            {
                return new SteamGameServerConfiguration
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
                    serverName = $"Must Not Be Empty | Must be Less than {Steamworks.Constants.k_cbMaxGameServerName}",
                    gameDescription = $"Must Not Be Empty | Must be Less than {Steamworks.Constants.k_cbMaxGameServerGameDescription}",
                    gameDirectory = $"Must Not Be Empty | Must be Less than {Steamworks.Constants.k_cbMaxGameServerGameDir}",
                    isDedicated = false,
                    maxPlayerCount = 4,
                    botPlayerCount = 0,
                    mapName = string.Empty,
                    gameData = string.Empty,
                    rulePairs = null,
                };
            }
        }

        /// <summary>
        /// Should the system automatically initialize the Steam Game Server APIs for server builds
        /// </summary>
        public bool autoInitialize;
        /// <summary>
        /// Should the system automatically log on to the Steam Game Server end point when the Steam Game Server has finished initialization.
        /// Logon of the Steam Game Server is required for the server to be issued its ID and for the server to appear in the Steam Game Server Browser.
        /// </summary>
        public bool autoLogon;
        /// <summary>
        /// The IP address of the server packed into a <see cref="uint"/> value such that each octave occupies 4 bits of the 32 bit uint value.
        /// You can read and write to the <see cref="IpAddress"/> value as a string and the system will parse and load that for you
        /// </summary>
        public uint ip;
        /// <summary>
        /// The primary game port typically 27015
        /// </summary>
        public ushort gamePort;
        /// <summary>
        /// The query port typically 27016
        /// </summary>
        public ushort queryPort;
        /// <summary>
        /// The spectator port if any typically 27017
        /// </summary>
        public ushort spectatorPort;
        /// <summary>
        /// The version string is usually in the form x.x.x.x, and is used by the master server to detect when the server is out of date. (Only servers with the latest version will be listed.)
        /// </summary>
        public string serverVersion;
        /// <summary>
        /// Sets the server mode to use Game Server Auth API
        /// </summary>
        public bool usingGameServerAuthApi;
        /// <summary>
        /// Enable server heartbeats, this is what allows the Steam Game Server Browser to be updated with ping, user count and to simply list on browser
        /// </summary>
        public bool enableHeartbeats;
        /// <summary>
        /// Should the system support spectators
        /// </summary>
        public bool supportSpectators;
        /// <summary>
        /// The name to display for the Spector server, this must be populated and must not be longer than <see cref="Steamworks.Constants.k_cbMaxGameServerMapName"/>.
        /// No it is not a typo, Valve documentation says it must be shorter than the max Game Server Map Name value
        /// </summary>
        public string spectatorServerName;
        /// <summary>
        /// Should the server logon anonymous, if not then a token is required
        /// </summary>
        public bool anonymousServerLogin;
        /// <summary>
        /// The game server token to be used if we are not logging on anonymous
        /// </summary>
        public string gameServerToken;
        /// <summary>
        /// Indicate if the server requires a password, handling the password is up to you this is just a flag for users to see in the browser
        /// </summary>
        public bool isPasswordProtected;
        /// <summary>
        /// The name of the server as it will be displayed, this must be populated and must not be longer than <see cref="Steamworks.Constants.k_cbMaxGameServerName"/>
        /// </summary>
        public string serverName;
        /// <summary>
        /// The description of the server as it will be displayed, this must be populated and must not be longer than <see cref="Steamworks.Constants.k_cbMaxGameServerGameDescription"/>
        /// </summary>
        public string gameDescription;
        /// <summary>
        /// This should be the same directory game where gets installed into. Just the folder name, not the whole path. e.g. "Spacewar".
        /// This must be populated and must not be longer than <see cref="Steamworks.Constants.k_cbMaxGameServerGameDir"/>
        /// </summary>
        public string gameDirectory;
        /// <summary>
        /// Is this a dedicated server, again this is a flag for the UI only, its up to you to host your server your self
        /// </summary>
        public bool isDedicated;
        /// <summary>
        /// The max player count this server will accept, this will be displayed in the browser as [authenticated Users] / [max player count] such as 3/4 indicating 3 authenticated users out of a max of 4
        /// </summary>
        public int maxPlayerCount;
        /// <summary>
        /// The max number of bots this server will host
        /// </summary>
        public int botPlayerCount;
        /// <summary>
        /// The map name to set, must not be null and cannot be longer than <see cref="Steamworks.Constants.k_cbMaxGameServerMapName"/>
        /// </summary>
        public string mapName;
        /// <summary>
        /// Sets a string defining the "gamedata" for this server, this is optional, but if set it allows users to filter in the matchmaking/server-browser interfaces based on the value.
        /// This is usually formatted as a comma or semicolon separated list.
        /// Don't set this unless it actually changes, its only uploaded to the master once; when acknowledged.
        /// If set this must not be longer than <see cref="Steamworks.Constants.k_cbMaxGameServerGameData"/>
        /// </summary>
        public string gameData;
        public StringKeyValuePair[] rulePairs;

        /// <summary>
        /// Will return true if the configuration is structurally sound.
        /// This will print log messages for each structural issue found.
        /// This is an estimate of structural validity based on the "required" field and max field lengths documented by Valve.
        /// There may be undocumented requirements or other reasons your configuration fails.
        /// </summary>
        /// <returns></returns>
        public readonly bool DebugValidate()
        {
            bool valid = true;

            if (string.IsNullOrEmpty(gameServerToken) && !anonymousServerLogin)
            {
                Debug.LogError($"Non-anonymous login requires a game server token, no token was found.");
                valid = false;
            }
            if (string.IsNullOrEmpty(serverName))
            {
                Debug.LogError($"Server Name must be populated.");
                valid = false;
            }
            if (serverName.Length > Steamworks.Constants.k_cbMaxGameServerName)
            {
                Debug.LogError($"Server Name {Steamworks.Constants.k_cbMaxGameServerName} char or less.");
                valid = false;
            }
            if (string.IsNullOrEmpty(spectatorServerName) && supportSpectators)
            {
                Debug.LogError($"If Support Spectators is true then you must provide a Spectator Server Name.");
                valid = false;
            }
            if ((spectatorPort == 0 || spectatorPort == ushort.MaxValue) && supportSpectators)
            {
                Debug.LogError($"If Support Spectators is true then you must provide a valid Spectator Port value.");
                valid = false;
            }
            if(supportSpectators && spectatorServerName.Length > Steamworks.Constants.k_cbMaxGameServerMapName)
            {
                Debug.LogError($"The Spectators Server Name must be {Steamworks.Constants.k_cbMaxGameServerMapName} char or less.");
                valid = false;
            }
            if(string.IsNullOrEmpty(gameDescription))
            {
                Debug.LogError($"You must provide a Game Description.");
                valid = false;
            }
            if (gameDescription.Length > Steamworks.Constants.k_cbMaxGameServerGameDescription)
            {
                Debug.LogError($"Game Description must be {Steamworks.Constants.k_cbMaxGameServerGameDescription} char or less.");
                valid = false;
            }
            if (string.IsNullOrEmpty(gameDirectory))
            {
                Debug.LogError($"You must provide a Game Directory.");
                valid = false;
            }
            if (gameDirectory.Length > Steamworks.Constants.k_cbMaxGameServerGameDir)
            {
                Debug.LogError($"Game Directory must be {Steamworks.Constants.k_cbMaxGameServerGameDir} char or less.");
                valid = false;
            }

            return valid;
        }

        /// <summary>
        /// Read the <see cref="ip"/> as a human friendly string in traditional format such as 0.0.0.0
        /// </summary>
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

        /// <summary>
        /// Get the currently applied configuration
        /// </summary>
        /// <returns></returns>
        public static SteamGameServerConfiguration Get() => API.App.Server.Configuration;

        /// <summary>
        /// Read server configuration from a file on disk
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static bool Get(FileInfo configFile, out SteamGameServerConfiguration config)
        {
            try
            {
                if (configFile.Exists)
                {
                    config = JsonUtility.FromJson<SteamGameServerConfiguration>(File.ReadAllText(configFile.FullName));
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

        /// <summary>
        /// Read server configuration from a file on disk
        /// </summary>
        /// <param name="configFile"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static bool Get(string configFile, out SteamGameServerConfiguration config)
        {
            try
            {
                if (File.Exists(configFile))
                {
                    config = JsonUtility.FromJson<SteamGameServerConfiguration>(File.ReadAllText(configFile));
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

        /// <summary>
        /// Read server configuration from byte[] representing the string serialized configuraiton
        /// </summary>
        /// <param name="serializedData"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static bool Get(byte[] serializedData, out SteamGameServerConfiguration config)
        {
            try
            {
                if (serializedData != null && serializedData.Length > 0)
                {
                    config = JsonUtility.FromJson<SteamGameServerConfiguration>(System.Text.Encoding.UTF8.GetString(serializedData));
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

        /// <summary>
        /// Returns the JSON formatted serialized form of the configuration, this can be written to disk for later reading
        /// </summary>
        /// <returns></returns>
        public override string ToString() => JsonUtility.ToJson(this);

        /// <summary>
        /// Get the bytes of the serialized configuration for writing to disk
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes() => System.Text.Encoding.UTF8.GetBytes(ToString());

        /// <summary>
        /// Save this configuraiton to disk
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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