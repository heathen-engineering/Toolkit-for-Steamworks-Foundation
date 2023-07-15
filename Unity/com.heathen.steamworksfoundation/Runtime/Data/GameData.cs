#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// Represents a Game as defined by <see cref="CGameID"/> in the Steam API
    /// </summary>
    [Serializable]
    public struct GameData : IEquatable<AppId_t>, IEquatable<CGameID>, IEquatable<uint>, IEquatable<ulong>, IEquatable<AppData>, IComparable<AppData>, IComparable<AppId_t>, IComparable<uint>, IComparable<ulong>
    {
        [SerializeField]
        private ulong id;
        /// <summary>
        /// Returns the <see cref="GameData"/> for the current program
        /// </summary>
        public static GameData Me => API.App.Client.Id;
        /// <summary>
        /// Returns the native <see cref="CGameID"/> representing the game
        /// </summary>
        public readonly CGameID GameId => new CGameID(id);
        /// <summary>
        /// Returns the primitive <see cref="ulong"/> representing the game
        /// </summary>
        public readonly ulong Id => GameId.m_GameID;
        /// <summary>
        /// Returns the <see cref="AppData"/> representing the game
        /// </summary>
        public readonly AppData App => GameId.AppID();
        /// <summary>
        /// Returns the Mod flag from the <see cref="CGameID"/> structure for this game
        /// </summary>
        public readonly bool IsMod => GameId.IsMod();
        /// <summary>
        /// Returns the P2P File flag from the <see cref="CGameID"/> structure for this game
        /// </summary>
        public readonly bool IsP2PFile => GameId.IsP2PFile();
        /// <summary>
        /// Returns the Shortcut flag from the <see cref="CGameID"/> structure for this game
        /// </summary>
        public readonly bool IsShortcut => GameId.IsShortcut();
        /// <summary>
        /// Returns the Steam App flag from the <see cref="CGameID"/> structure for this game
        /// </summary>
        public readonly bool IsSteamApp => GameId.IsSteamApp();
        /// <summary>
        /// True if this is a valid Game ID according to Steam API
        /// </summary>
        public readonly bool IsValid => GameId.IsValid();
        /// <summary>
        /// Returns the Mod ID from the <see cref="CGameID"/> structure for this game
        /// </summary>
        public readonly uint ModID => GameId.ModID();
        /// <summary>
        /// Returns true if this <see cref="GameData"/> represents the current program
        /// </summary>
        public readonly bool IsMe => this == Me;
        /// <summary>
        /// Gets the display name for the app this game is related to if known. You must have called <see cref="AppData.LoadNames(Action)"/> before this will be set.
        /// <para>Note that <see cref="SteamworksBehaviour"/> and other tools from Heathen will have already loaded the App names for you during initialization. You usually only need to call this your self when your using a manual form of initialization.</para>
        /// </summary>
        public readonly string Name => App.Name;
        /// <summary>
        /// Returns the <see cref="CGameID.EGameIDType"/> for this game, this is similar to checking the <see cref="IsMod"/>, <see cref="IsShortcut"/>, <see cref="IsP2PFile"/> or <see cref="IsSteamApp"/> flags
        /// </summary>
        public readonly CGameID.EGameIDType Type => GameId.Type();
        /// <summary>
        /// Returns the <see cref="GameData"/> that represents this program
        /// </summary>
        /// <returns>The <see cref="GameData"/> that represents this program</returns>
        public static GameData Get() => Me;
        /// <summary>
        /// Returns the <see cref="GameData"/> given a native <see cref="CGameID"/>
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns>The <see cref="GameData"/> represented by the provided <see cref="CGameID"/></returns>
        public static GameData Get(CGameID gameId) => gameId;
        /// <summary>
        /// Returns the <see cref="GameData"/> given a primitive <see cref="ulong"/> representing its Game ID
        /// </summary>
        /// <param name="gameId">The <see cref="GameData"/> represented by the provided game id</param>
        /// <returns></returns>
        public static GameData Get(ulong gameId) => gameId;
        /// <summary>
        /// Returns the <see cref="GameData"/> given a primitive <see cref="uint"/> representing its App ID
        /// </summary>
        /// <param name="gameId">The <see cref="GameData"/> represented by the provided app id</param>
        /// <returns></returns>
        public static GameData Get(uint appId) => appId;
        /// <summary>
        /// Returns the <see cref="GameData"/> given a native <see cref="AppId_t"/> representing its App ID
        /// </summary>
        /// <param name="gameId">The <see cref="GameData"/> represented by the provided app id</param>
        /// <returns></returns>
        public static GameData Get(AppId_t appId) => appId;
        /// <summary>
        /// Returns the name of the game if known.
        /// <para>This will attempt to load the list of App names from Steam Web API if it has not already been loaded.</para>
        /// </summary>
        /// <param name="name">The name of the game</param>
        /// <returns></returns>
        public readonly bool GetName(out string name) => API.App.Web.GetAppName(App, out name);
        /// <summary>
        /// Requests the system to load the app names and return the name represented by this <see cref="GameData"/>
        /// </summary>
        /// <param name="callback">A delegate of the form (<see cref="string"/> name, <see cref="bool"/> ioError) that will be invoked when completed</param>
        public readonly void GetName(Action<string, bool> callback) => API.App.Web.GetAppName(App, callback);
        /// <summary>
        /// Returns true if the list of App Names have already been loaded
        /// </summary>
        public static bool NamesLoaded => API.App.Web.IsAppsListLoaded;
        /// <summary>
        /// Requests the system to load the list of app names from the Steam Web API
        /// </summary>
        /// <param name="callback">A delegate of the form () that will be invoked when completed</param>
        public static void LoadNames(Action callback) => API.App.Web.LoadAppNames(callback);
        /// <summary>
        /// Opens the Steam Overlay to the store page for the provided app
        /// </summary>
        /// <param name="app">The app to open the store page to</param>
        /// <param name="flag">The <see cref="EOverlayToStoreFlag"/> to open the store with</param>
        public static void OpenSteamStore(AppData app, EOverlayToStoreFlag flag = EOverlayToStoreFlag.k_EOverlayToStoreFlag_None) => SteamFriends.ActivateGameOverlayToStore(app, flag);
        /// <summary>
        /// Opens the Steam Overlay to the store page for the app related to this game
        /// </summary>
        /// <param name="flag">The <see cref="EOverlayToStoreFlag"/> to open the store with</param>
        public static void OpenMySteamStore(EOverlayToStoreFlag flag = EOverlayToStoreFlag.k_EOverlayToStoreFlag_None) => SteamFriends.ActivateGameOverlayToStore(Me, flag);

        #region Boilerplate
        public readonly int CompareTo(AppData other)
        {
            return App.CompareTo(other.AppId);
        }

        public readonly int CompareTo(GameData other)
        {
            return Id.CompareTo(other.Id);
        }

        public readonly int CompareTo(AppId_t other)
        {
            return App.CompareTo(other);
        }

        public readonly int CompareTo(ulong other)
        {
            return GameId.m_GameID.CompareTo(other);
        }

        public readonly int CompareTo(uint other)
        {
            return App.CompareTo(other);
        }

        public readonly override string ToString()
        {
            return GameId.ToString();
        }

        public readonly bool Equals(AppData other)
        {
            return App.Equals(other.AppId);
        }

        public readonly bool Equals(GameData other)
        {
            return Id.Equals(other.Id);
        }

        public readonly bool Equals(AppId_t other)
        {
            return App.Id.Equals(other);
        }

        public readonly bool Equals(uint other)
        {
            return App.Id.Equals(other);
        }

        public readonly bool Equals(CGameID other)
        {
            return GameId.Equals(other.AppID());
        }

        public readonly bool Equals(ulong other)
        {
            return GameId.Equals(new CGameID(other).AppID());
        }

        public readonly override bool Equals(object obj)
        {
            return GameId.m_GameID.Equals(obj);
        }

        public readonly override int GetHashCode()
        {
            return GameId.GetHashCode();
        }

        public static bool operator ==(GameData l, GameData r) => l.GameId.m_GameID == r.GameId.m_GameID;
        public static bool operator ==(AppId_t l, GameData r) => l == r.App;
        public static bool operator ==(GameData l, AppId_t r) => l.App == r;
        public static bool operator !=(GameData l, GameData r) => l.GameId.m_GameID != r.GameId.m_GameID;
        public static bool operator !=(AppId_t l, GameData r) => l != r.App;
        public static bool operator !=(GameData l, AppId_t r) => l.App != r;

        public static implicit operator GameData(CGameID id) => new GameData { id = id.m_GameID };
        public static implicit operator uint(GameData c) => c.App.Id;
        public static implicit operator ulong(GameData c) => c.Id;
        public static implicit operator GameData(ulong id) => new GameData { id = id };
        public static implicit operator GameData(uint id) => new GameData { id = new CGameID(new AppId_t(id)).m_GameID };
        public static implicit operator AppId_t(GameData c) => c.App;
        public static implicit operator GameData(AppId_t id) => new CGameID(id);
        #endregion
    }
}
#endif