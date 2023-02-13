#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;

namespace HeathenEngineering.SteamworksIntegration
{
    [Serializable]
    public struct AppData : IEquatable<AppId_t>, IEquatable<CGameID>, IEquatable<uint>, IEquatable<ulong>, IEquatable<AppData>, IComparable<AppData>, IComparable<AppId_t>, IComparable<uint>, IComparable<ulong>
    {
        public static AppData Me => API.App.Client.Id;
        public AppId_t appId;
        public bool IsMe => appId == API.App.Client.Id;
        public uint Id
        {
            get => appId.m_AppId;
            set => appId = new AppId_t(value);
        }
        /// <summary>
        /// Gets the name if loaded, returns Unknown until names have been loaded
        /// </summary>
        /// <remarks>
        /// you can call <see cref="LoadNames(Action)"/> to load the names for all apps.
        /// you can call <see cref="GetName(Action{string, bool})"/> to load the names if needed and then return this apps name when loaded
        /// </remarks>
        public string Name
        {
            get
            {
                API.App.Web.GetAppName(appId, out string value);
                return value;
            }
        }

        public void OpenSteamStore(EOverlayToStoreFlag flag = EOverlayToStoreFlag.k_EOverlayToStoreFlag_None) => SteamFriends.ActivateGameOverlayToStore(this, flag);

        public static AppData Get() => Me;
        public static AppData Get(CGameID gameId) => gameId;
        public static AppData Get(ulong gameId) => gameId;
        public static AppData Get(uint appId) => appId;
        public static AppData Get(AppId_t appId) => appId;
        public static AppData Get(DlcData dlcData) => dlcData;

        public bool GetName(out string name) => API.App.Web.GetAppName(appId, out name);

        public void GetName(Action<string, bool> callback) => API.App.Web.GetAppName(appId, callback);

        public static bool NamesLoaded => API.App.Web.IsAppsListLoaded;

        public static void LoadNames(Action callback) => API.App.Web.LoadAppNames(callback);

        public static void OpenSteamStore(AppData app, EOverlayToStoreFlag flag = EOverlayToStoreFlag.k_EOverlayToStoreFlag_None) => SteamFriends.ActivateGameOverlayToStore(app, flag);
        public static void OpenMySteamStore(EOverlayToStoreFlag flag = EOverlayToStoreFlag.k_EOverlayToStoreFlag_None) => SteamFriends.ActivateGameOverlayToStore(Me, flag);

        #region Boilerplate
        public int CompareTo(AppData other)
        {
            return appId.CompareTo(other.appId);
        }

        public int CompareTo(AppId_t other)
        {
            return appId.CompareTo(other);
        }

        public int CompareTo(ulong other)
        {
            return appId.CompareTo(new CGameID(other).AppID());
        }

        public int CompareTo(uint other)
        {
            return appId.m_AppId.CompareTo(other);
        }

        public override string ToString()
        {
            return appId.ToString();
        }

        public bool Equals(AppData other)
        {
            return appId.Equals(other.appId);
        }

        public bool Equals(AppId_t other)
        {
            return appId.Equals(other);
        }

        public bool Equals(uint other)
        {
            return appId.m_AppId.Equals(other);
        }

        public bool Equals(CGameID other)
        {
            return appId.Equals(other.AppID());
        }

        public bool Equals(ulong other)
        {
            return appId.Equals(new CGameID(other).AppID());
        }

        public override bool Equals(object obj)
        {
            return appId.m_AppId.Equals(obj);
        }

        public override int GetHashCode()
        {
            return appId.GetHashCode();
        }

        public static bool operator ==(AppData l, AppData r) => l.appId == r.appId;
        public static bool operator ==(AppId_t l, AppData r) => l == r.appId;
        public static bool operator ==(AppData l, AppId_t r) => l.appId == r;
        public static bool operator !=(AppData l, AppData r) => l.appId != r.appId;
        public static bool operator !=(AppId_t l, AppData r) => l != r.appId;
        public static bool operator !=(AppData l, AppId_t r) => l.appId != r;

        public static implicit operator AppData(CGameID id) => new AppData { appId = id.AppID() };
        public static implicit operator uint(AppData c) => c.appId.m_AppId;
        public static implicit operator AppData(ulong id) => new AppData { appId = new CGameID(id).AppID() };
        public static implicit operator AppData(uint id) => new AppData { appId = new AppId_t(id) };
        public static implicit operator AppId_t(AppData c) => c.appId;
        public static implicit operator AppData(AppId_t id) => new AppData { appId = id };
        public static implicit operator AppData(GameData id) => id.App;
        #endregion
    }
}
#endif