#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// Represents the ID of a Steam App and exposes the core tools and features of Steam API with regards to Steam Apps.
    /// </summary>
    [Serializable]
    public struct AppData : IEquatable<AppId_t>, IEquatable<CGameID>, IEquatable<uint>, IEquatable<ulong>, IEquatable<AppData>, IComparable<AppData>, IComparable<AppId_t>, IComparable<uint>, IComparable<ulong>
    {
        [SerializeField]
        private uint id;
        /// <summary>
        /// Return the AppData object representing this program's App
        /// </summary>
        public static AppData Me => API.App.Client.Id;
        /// <summary>
        /// The native <see cref="Steamworks.AppId_t"/> id of the app
        /// </summary>
        public readonly AppId_t AppId => new AppId_t(id);
        /// <summary>
        /// Returns true if this AppData represents the current App being ran
        /// </summary>
        public readonly bool IsMe => AppId == API.App.Client.Id;
        /// <summary>
        /// Returns the primitive <see cref="uint"/> value of the id
        /// </summary>
        public readonly uint Id => AppId.m_AppId;
        /// <summary>
        /// Gets the name if loaded, returns Unknown until names have been loaded
        /// </summary>
        /// <remarks>
        /// you can call <see cref="LoadNames(Action)"/> to load the names for all apps.
        /// you can call <see cref="GetName(Action{string, bool})"/> to load the names if needed and then return this apps name when loaded
        /// </remarks>
        public readonly string Name
        {
            get
            {
                API.App.Web.GetAppName(AppId, out string value);
                return value;
            }
        }
        /// <summary>
        /// Opens the Steam Overlay to the Steam Store page for this app
        /// </summary>
        /// <param name="flag">The <see cref="Steamworks.EOverlayToStoreFlag"/> if any, this will default to <see cref="Steamworks.EOverlayToStoreFlag.k_EOverlayToStoreFlag_None"/></param>
        public readonly void OpenSteamStore(EOverlayToStoreFlag flag = EOverlayToStoreFlag.k_EOverlayToStoreFlag_None) => SteamFriends.ActivateGameOverlayToStore(this, flag);
        /// <summary>
        /// Returns the AppData representing this app, this is the same as <see cref="AppData.Me"/>
        /// </summary>
        /// <returns>The <see cref="AppData"/> representing this program</returns>
        public static AppData Get() => Me;
        /// <summary>
        /// Returns the AppData represented by the input game ID
        /// </summary>
        /// <param name="gameId">The game ID of the app to get the AppData for</param>
        /// <returns>The <see cref="AppData"/> represented by this <see cref="Steamworks.CGameID"/></returns>
        public static AppData Get(CGameID gameId) => gameId;
        /// <summary>
        /// Returns the AppData represented by the primitive <see cref="ulong"/> value representing a game ID
        /// </summary>
        /// <param name="gameId">The <see cref="ulong"/> value of a Game ID to get the AppData for</param>
        /// <returns>The <see cref="AppData"/> represented by the <see cref="ulong"/> value, where the value is a Game Id</returns>
        public static AppData Get(ulong gameId) => gameId;
        /// <summary>
        /// Returns the AppData represented by the primitive <see cref="uint"/> value representing a native <see cref="Steamworks.AppId_t"/>
        /// </summary>
        /// <param name="appId">The native <see cref="Steamworks.AppId_t"/></param> value
        /// <returns>The <see cref="AppData"/> represented by the native <see cref="Steamworks.AppId_t"/></returns>
        public static AppData Get(uint appId) => appId;
        /// <summary>
        /// Returns the AppData represented by the native <see cref="Steamworks.AppId_t"/>
        /// </summary>
        /// <param name="appId">The native <see cref="Steamworks.AppId_t"/></param> value
        /// <returns>The <see cref="AppData"/> represented by the native <see cref="Steamworks.AppId_t"/></returns>
        public static AppData Get(AppId_t appId) => appId;
        /// <summary>
        /// Returns the AppData represented by the <see cref="DlcData"/>
        /// </summary>
        /// <param name="dlcData">The DLC object to get the ID for
        /// <returns>The <see cref="AppData"/> represented by the native <see cref="Steamworks.AppId_t"/></returns>
        public static AppData Get(DlcData dlcData) => dlcData;
        /// <summary>
        /// Gets the name of the app if available
        /// </summary>
        /// <param name="name">The name of the app found</param>
        /// <returns>True if a name was found, false otherwise</returns>
        public readonly bool GetName(out string name) => API.App.Web.GetAppName(AppId, out name);
        /// <summary>
        /// Asynchronous attempt to fetch the app name
        /// </summary>
        /// <param name="callback">A delegate with a signature of (<see cref="string"/> nameFound, <see cref="bool"/> ioError) that will be invoked on completion</param>
        public readonly void GetName(Action<string, bool> callback) => API.App.Web.GetAppName(AppId, callback);
        /// <summary>
        /// True if the system has loaded the set of App names from Steam's web API
        /// </summary>
        public static bool NamesLoaded => API.App.Web.IsAppsListLoaded;
        /// <summary>
        /// Request the system to load the list of App names from Steam's web API
        /// </summary>
        /// <param name="callback">A delegate with a signature of () that will be invoked on completion"/></param>
        public static void LoadNames(Action callback) => API.App.Web.LoadAppNames(callback);
        /// <summary>
        /// Open the Steam store to the indicated app
        /// </summary>
        /// <param name="app">The app whose store should be opened</param>
        /// <param name="flag">The <see cref="EOverlayToStoreFlag"/> to use when opening the store</param>
        public static void OpenSteamStore(AppData app, EOverlayToStoreFlag flag = EOverlayToStoreFlag.k_EOverlayToStoreFlag_None) => SteamFriends.ActivateGameOverlayToStore(app, flag);
        /// <summary>
        /// Open the Steam store to this apps store page with the indicated flags
        /// </summary>
        /// <param name="flag">The <see cref="EOverlayToStoreFlag"/> to be used when opening the store page</param>
        public static void OpenMySteamStore(EOverlayToStoreFlag flag = EOverlayToStoreFlag.k_EOverlayToStoreFlag_None) => SteamFriends.ActivateGameOverlayToStore(Me, flag);

        #region Boilerplate
        public readonly int CompareTo(AppData other)
        {
            return AppId.CompareTo(other.AppId);
        }

        public readonly int CompareTo(AppId_t other)
        {
            return AppId.CompareTo(other);
        }

        public readonly int CompareTo(ulong other)
        {
            return AppId.CompareTo(new CGameID(other).AppID());
        }

        public readonly int CompareTo(uint other)
        {
            return AppId.m_AppId.CompareTo(other);
        }

        public readonly override string ToString()
        {
            return AppId.ToString();
        }

        public readonly bool Equals(AppData other)
        {
            return AppId.Equals(other.AppId);
        }

        public readonly bool Equals(AppId_t other)
        {
            return AppId.Equals(other);
        }

        public readonly bool Equals(uint other)
        {
            return AppId.m_AppId.Equals(other);
        }

        public readonly bool Equals(CGameID other)
        {
            return AppId.Equals(other.AppID());
        }

        public readonly bool Equals(ulong other)
        {
            return AppId.Equals(new CGameID(other).AppID());
        }

        public readonly override bool Equals(object obj)
        {
            return AppId.m_AppId.Equals(obj);
        }

        public readonly override int GetHashCode()
        {
            return AppId.GetHashCode();
        }

        public static bool operator ==(AppData l, AppData r) => l.AppId == r.AppId;
        public static bool operator ==(AppId_t l, AppData r) => l == r.AppId;
        public static bool operator ==(AppData l, AppId_t r) => l.AppId == r;
        public static bool operator !=(AppData l, AppData r) => l.AppId != r.AppId;
        public static bool operator !=(AppId_t l, AppData r) => l != r.AppId;
        public static bool operator !=(AppData l, AppId_t r) => l.AppId != r;

        public static implicit operator AppData(CGameID id) => new AppData { id = id.AppID().m_AppId };
        public static implicit operator uint(AppData c) => c.AppId.m_AppId;
        public static implicit operator AppData(ulong id) => new AppData { id = new CGameID(id).AppID().m_AppId };
        public static implicit operator AppData(uint id) => new AppData { id = id };
        public static implicit operator AppId_t(AppData c) => c.AppId;
        public static implicit operator AppData(AppId_t id) => new AppData { id = id.m_AppId };
        public static implicit operator AppData(GameData id) => new AppData { id = id.App.Id };
        #endregion
    }
}
#endif