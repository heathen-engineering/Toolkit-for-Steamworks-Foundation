#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;
using System;
using System.IO;
using UnityEngine;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// Represents a Downloadable Content app.
    /// <para>Downloadable Content e.g. DLC are child apps of a parent 'game' app. They must be created in Steam Developer Portal and then can be access in code via their id using this structure</para>
    /// </summary>
    [Serializable]
    public struct DlcData : IEquatable<AppId_t>, IEquatable<uint>, IEquatable<AppData>, IComparable<AppData>, IComparable<AppId_t>, IComparable<uint>
    {
        [SerializeField]
        private uint id;
        /// <summary>
        /// The native <see cref="AppId_t"/> representation of this DLC
        /// </summary>
        public readonly AppId_t AppId => new AppId_t(id);
        /// <summary>
        /// The primitive <see cref="uint"/> representation of this DLC
        /// </summary>
        public readonly uint Id => id;
        /// <summary>
        /// Checks if the DLC in question is available or not
        /// </summary>
        public readonly bool Available
        {
            get
            {
                if (API.App.dlcAppCash.ContainsKey(Id))
                    return API.App.dlcAppCash[Id].available;
                else
                {
                    bool _available = false;
                    var count = SteamApps.GetDLCCount();
                    for (int i = 0; i < count; i++)
                    {
                        if (SteamApps.BGetDLCDataByIndex(i, out var pAppID, out var pAvailable, out var pName, 512))
                        {
                            if (API.App.dlcAppCash.ContainsKey(pAppID.m_AppId))
                                API.App.dlcAppCash[pAppID.m_AppId] = (pName, pAvailable);
                            else
                                API.App.dlcAppCash.Add(pAppID.m_AppId, (pName, pAvailable));

                            if (pAppID.m_AppId == id)
                                _available = pAvailable;
                        }
                    }
                    return _available;
                }
            }
        }
        /// <summary>
        /// Returns the DLC's name as visible to the user
        /// </summary>
        public readonly string Name
        {
            get
            {
                if (API.App.dlcAppCash.ContainsKey(Id))
                    return API.App.dlcAppCash[Id].name;
                else
                {
                    string _name = "None Found";
                    var count = SteamApps.GetDLCCount();
                    for (int i = 0; i < count; i++)
                    {
                        if (SteamApps.BGetDLCDataByIndex(i, out var pAppID, out var pAvailable, out var pName, 512))
                        {
                            if (API.App.dlcAppCash.ContainsKey(pAppID.m_AppId))
                                API.App.dlcAppCash[pAppID.m_AppId] = (pName, pAvailable);
                            else
                                API.App.dlcAppCash.Add(pAppID.m_AppId, (pName, pAvailable));

                            if (pAppID.m_AppId == id)
                                _name = pName;
                        }
                    }
                    return _name;
                }
            }
        }
        /// <summary>
        /// Does the user "own" this DLC.
        /// </summary>
        public readonly bool IsSubscribed => SteamApps.BIsSubscribedApp(this);
        /// <summary>
        /// Is this DLC installed
        /// </summary>
        public readonly bool IsInstalled => SteamApps.BIsDlcInstalled(this);
        /// <summary>
        /// The location where this DLC is installed
        /// </summary>
        public readonly DirectoryInfo InstallDirectory
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
        /// <summary>
        /// The download progress of this DLC if known
        /// </summary>
        public readonly float DownloadProgress
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
        /// <summary>
        /// The first known date of purchase for this DLC
        /// </summary>
        public readonly DateTime EarliestPurchaseTime
        {
            get
            {
                var val = SteamApps.GetEarliestPurchaseUnixTime(this);
                var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddSeconds(val);
                return dateTime;
            }
        }
        /// <summary>
        /// Request that this DLC be installed
        /// </summary>
        public readonly void Install()
        {
            SteamApps.InstallDLC(this);
        }
        /// <summary>
        /// Request that this DLC be uninstalled
        /// </summary>
        public readonly void Uninstall()
        {
            SteamApps.UninstallDLC(this);
        }
        /// <summary>
        /// Open the Steam overlay to the store page for this DLC with the indicated flags
        /// </summary>
        /// <param name="flag">The <see cref="EOverlayToStoreFlag"/> to open the store page with</param>
        public readonly void OpenStore(EOverlayToStoreFlag flag = EOverlayToStoreFlag.k_EOverlayToStoreFlag_None) => SteamFriends.ActivateGameOverlayToStore(this, flag);
        /// <summary>
        /// Construct a new <see cref="DlcData"/> based on an <see cref="AppId_t"/> and known availability and name values.
        /// <para>
        /// This is an internal feature for the editor tools and in general shouldn't be used by developers at runtime.
        /// </para>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="available"></param>
        /// <param name="name"></param>
        public DlcData(AppId_t id, bool available, string name)
        {
            this.id = id.m_AppId;
            if (API.App.dlcAppCash.ContainsKey(id.m_AppId))
                API.App.dlcAppCash[id.m_AppId] = (name, available);
            else
                API.App.dlcAppCash.Add(id.m_AppId, (name, available));
        }
        /// <summary>
        /// Get <see cref="DlcData"/> based on a <see cref="uint"/> representing its App ID
        /// </summary>
        /// <param name="appId">The ID of the App the DLC is known by</param>
        /// <returns></returns>
        public static DlcData Get(uint appId) => appId;
        /// <summary>
        /// Get <see cref="DlcData"/> based on a <see cref="AppId_t"/> representing the Dlc in Steam
        /// </summary>
        /// <param name="appId">The App ID the DLC is known by</param>
        /// <returns></returns>
        public static DlcData Get(AppId_t appId) => appId;
        /// <summary>
        /// Get <see cref="DlcData"/> based on a <see cref="AppData"/> representing its Steam App
        /// </summary>
        /// <param name="appData"></param>
        /// <returns></returns>
        public static DlcData Get(AppData appData) => appData.AppId;

        #region Boilerplate
        public readonly int CompareTo(AppData other)
        {
            return id.CompareTo(other.AppId);
        }

        public readonly int CompareTo(AppId_t other)
        {
            return id.CompareTo(other);
        }

        public readonly int CompareTo(uint other)
        {
            return id.CompareTo(other);
        }

        public readonly bool Equals(uint other)
        {
            return id.Equals(other);
        }

        public readonly override bool Equals(object obj)
        {
            return id.Equals(obj);
        }

        public readonly override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public readonly bool Equals(AppId_t other)
        {
            return id.Equals(other);
        }

        public readonly bool Equals(AppData other)
        {
            return id.Equals(other.AppId);
        }

        public static bool operator ==(DlcData l, DlcData r) => l.id == r.id;
        public static bool operator ==(DlcData l, AppData r) => l.id == r.Id;
        public static bool operator ==(DlcData l, AppId_t r) => l.id == r.m_AppId;
        public static bool operator ==(AppId_t l, DlcData r) => l.m_AppId == r.id;
        public static bool operator !=(DlcData l, DlcData r) => l.id != r.id;
        public static bool operator !=(DlcData l, AppData r) => l.id != r.AppId.m_AppId;
        public static bool operator !=(DlcData l, AppId_t r) => l.id != r.m_AppId;
        public static bool operator !=(AppId_t l, DlcData r) => l.m_AppId != r.id;

        public static implicit operator uint(DlcData c) => c.id;
        public static implicit operator DlcData(uint id) => new DlcData { id = id };
        public static implicit operator AppId_t(DlcData c) => c.AppId;
        public static implicit operator DlcData(AppId_t id) => new DlcData { id = id.m_AppId };
        public static implicit operator DlcData(AppData id) => new DlcData { id = id.Id };
        public static implicit operator AppData(DlcData id) => AppData.Get(id.id);
        #endregion
    }
}
#endif