#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;

namespace HeathenEngineering.SteamworksIntegration
{
    [Serializable]
    public struct GameData : IEquatable<AppId_t>, IEquatable<CGameID>, IEquatable<uint>, IEquatable<ulong>, IEquatable<AppData>, IComparable<AppData>, IComparable<AppId_t>, IComparable<uint>, IComparable<ulong>
    {
        public static GameData Me => API.App.Client.Id;
        public CGameID gameId;
        public ulong Id => gameId.m_GameID;
        public AppData App => gameId.AppID();
        public bool IsMod => gameId.IsMod();
        public bool IsP2PFile => gameId.IsP2PFile();
        public bool IsShortcut => gameId.IsShortcut();
        public bool IsSteamApp => gameId.IsSteamApp();
        public bool IsValid => gameId.IsValid();
        public uint ModID => gameId.ModID();
        public bool IsMe => this == Me;
        public string Name => App.Name;
        public CGameID.EGameIDType Type => gameId.Type();

        public static GameData Get() => Me;
        public static GameData Get(CGameID gameId) => gameId;
        public static GameData Get(ulong gameId) => gameId;
        public static GameData Get(uint appId) => appId;
        public static GameData Get(AppId_t appId) => appId;

        public bool GetName(out string name) => API.App.Web.GetAppName(App, out name);

        public void GetName(Action<string, bool> callback) => API.App.Web.GetAppName(App, callback);

        public static bool NamesLoaded => API.App.Web.IsAppsListLoaded;

        public static void LoadNames(Action callback) => API.App.Web.LoadAppNames(callback);

        public static void OpenSteamStore(AppData app, EOverlayToStoreFlag flag = EOverlayToStoreFlag.k_EOverlayToStoreFlag_None) => SteamFriends.ActivateGameOverlayToStore(app, flag);
        public static void OpenMySteamStore(EOverlayToStoreFlag flag = EOverlayToStoreFlag.k_EOverlayToStoreFlag_None) => SteamFriends.ActivateGameOverlayToStore(Me, flag);

        #region Boilerplate
        public int CompareTo(AppData other)
        {
            return App.CompareTo(other.appId);
        }

        public int CompareTo(GameData other)
        {
            return Id.CompareTo(other.Id);
        }

        public int CompareTo(AppId_t other)
        {
            return App.CompareTo(other);
        }

        public int CompareTo(ulong other)
        {
            return gameId.m_GameID.CompareTo(other);
        }

        public int CompareTo(uint other)
        {
            return App.CompareTo(other);
        }

        public override string ToString()
        {
            return gameId.ToString();
        }

        public bool Equals(AppData other)
        {
            return App.Equals(other.appId);
        }

        public bool Equals(GameData other)
        {
            return Id.Equals(other.Id);
        }

        public bool Equals(AppId_t other)
        {
            return App.Id.Equals(other);
        }

        public bool Equals(uint other)
        {
            return App.Id.Equals(other);
        }

        public bool Equals(CGameID other)
        {
            return gameId.Equals(other.AppID());
        }

        public bool Equals(ulong other)
        {
            return gameId.Equals(new CGameID(other).AppID());
        }

        public override bool Equals(object obj)
        {
            return gameId.m_GameID.Equals(obj);
        }

        public override int GetHashCode()
        {
            return gameId.GetHashCode();
        }

        public static bool operator ==(GameData l, GameData r) => l.gameId.m_GameID == r.gameId.m_GameID;
        public static bool operator ==(AppId_t l, GameData r) => l == r.App;
        public static bool operator ==(GameData l, AppId_t r) => l.App == r;
        public static bool operator !=(GameData l, GameData r) => l.gameId.m_GameID != r.gameId.m_GameID;
        public static bool operator !=(AppId_t l, GameData r) => l != r.App;
        public static bool operator !=(GameData l, AppId_t r) => l.App != r;

        public static implicit operator GameData(CGameID id) => new GameData { gameId = id };
        public static implicit operator uint(GameData c) => c.App.Id;
        public static implicit operator ulong(GameData c) => c.Id;
        public static implicit operator GameData(ulong id) => new GameData { gameId = new CGameID(id) };
        public static implicit operator GameData(uint id) => new GameData { gameId = new CGameID(new AppId_t(id)) };
        public static implicit operator AppId_t(GameData c) => c.App;
        public static implicit operator GameData(AppId_t id) => new CGameID(id);
        #endregion
    }
}
#endif