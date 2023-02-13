#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;
using System.IO;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    [Serializable]
    public struct DlcData : IEquatable<AppId_t>, IEquatable<uint>, IEquatable<AppData>, IComparable<AppData>, IComparable<AppId_t>, IComparable<uint>
    {
        [SerializeField]
        private uint id;
        public AppId_t AppId => new AppId_t(id);

        [SerializeField]
        private bool _available;
        public bool Available
        {
            get
            {
                if (!_available)
                {
                    var count = SteamApps.GetDLCCount();
                    for (int i = 0; i < count; i++)
                    {
                        if (SteamApps.BGetDLCDataByIndex(i, out var pAppID, out var pAvailable, out var pName, 512))
                        {
                            if (pAppID.m_AppId == id)
                            {
                                _available = pAvailable;
                                _name = pName;
                                break;
                            }
                        }
                    }
                }

                return _available;
            }

            private set
            {
                _available = value;
            }
        }

        [SerializeField]
        private string _name;
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    var count = SteamApps.GetDLCCount();
                    for (int i = 0; i < count; i++)
                    {
                        if (SteamApps.BGetDLCDataByIndex(i, out var pAppID, out var pAvailable, out var pName, 512))
                        {
                            if (pAppID == this)
                            {
                                _available = pAvailable;
                                _name = pName;
                                break;
                            }
                        }
                    }
                }

                return _name;
            }

            private set
            {
                _name = value;
            }
        }
        public bool IsSubscribed => SteamApps.BIsSubscribedApp(this);
        public bool IsInstalled => SteamApps.BIsDlcInstalled(this);
        public DirectoryInfo InstallDirectory
        {
            get
            {
                if (SteamApps.GetAppInstallDir(this, out var path, 2048) > 0)
                {
                    return new DirectoryInfo(path.Trim());
                }
                else
                {
                    return default;
                }
            }
        }
        public float DownloadProgress
        {
            get
            {
                var IsDownloading = SteamApps.GetDlcDownloadProgress(this, out ulong current, out ulong total);
                if (IsDownloading)
                {
                    return Convert.ToSingle(current / (double)total);
                }
                else
                    return 0f;
            }
        }
        public DateTime EarliestPurchaseTime
        {
            get
            {
                var val = SteamApps.GetEarliestPurchaseUnixTime(this);
                var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds(val);
                return dateTime;
            }
        }
        public void Install()
        {
            SteamApps.InstallDLC(this);
        }
        public void Uninstall()
        {
            SteamApps.UninstallDLC(this);
        }
        public void OpenStore(EOverlayToStoreFlag flag = EOverlayToStoreFlag.k_EOverlayToStoreFlag_None) => SteamFriends.ActivateGameOverlayToStore(this, flag);

        public DlcData(AppId_t id, bool available, string name)
        {
            this.id = id.m_AppId;
            _available = available;
            _name = name;
        }

        public static DlcData Get(uint appId) => appId;
        public static DlcData Get(AppId_t appId) => appId;
        public static DlcData Get(AppData appData) => appData.appId;

        #region Boilerplate
        public int CompareTo(AppData other)
        {
            return id.CompareTo(other.appId);
        }

        public int CompareTo(AppId_t other)
        {
            return id.CompareTo(other);
        }

        public int CompareTo(uint other)
        {
            return id.CompareTo(other);
        }

        public bool Equals(uint other)
        {
            return id.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return id.Equals(obj);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode() + _name.GetHashCode() + _available.GetHashCode();
        }

        public bool Equals(AppId_t other)
        {
            return id.Equals(other);
        }

        public bool Equals(AppData other)
        {
            return id.Equals(other.appId);
        }

        public static bool operator ==(DlcData l, DlcData r) => l.id == r.id;
        public static bool operator ==(DlcData l, AppData r) => l.id == r.Id;
        public static bool operator ==(DlcData l, AppId_t r) => l.id == r.m_AppId;
        public static bool operator ==(AppId_t l, DlcData r) => l.m_AppId == r.id;
        public static bool operator !=(DlcData l, DlcData r) => l.id != r.id;
        public static bool operator !=(DlcData l, AppData r) => l.id != r.appId.m_AppId;
        public static bool operator !=(DlcData l, AppId_t r) => l.id != r.m_AppId;
        public static bool operator !=(AppId_t l, DlcData r) => l.m_AppId != r.id;

        public static implicit operator uint(DlcData c) => c.id;
        public static implicit operator DlcData(uint id) => new DlcData { id = id };
        public static implicit operator AppId_t(DlcData c) => c.AppId;
        public static implicit operator DlcData(AppId_t id) => new DlcData { id = id.m_AppId };
        public static implicit operator DlcData(AppData id) => new DlcData { id = id.Id };
        public static implicit operator AppData(DlcData id) => new AppData { Id = id.id };
        #endregion
    }
}
#endif