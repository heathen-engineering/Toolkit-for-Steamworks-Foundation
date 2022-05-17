#if HE_SYSCORE && STEAMWORKS_NET
using System;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;

namespace HeathenEngineering.SteamworksIntegration
{
    [Serializable]
    public class UGCCommunityItem
    {
        public string Title => itemDetails.m_rgchTitle;
        public string Description => itemDetails.m_rgchDescription;
        public AppId_t ConsumerApp => itemDetails.m_nConsumerAppID;
        public PublishedFileId_t FileId => itemDetails.m_nPublishedFileId;
        public UserData Owner => new CSteamID(itemDetails.m_ulSteamIDOwner);
        public DateTime TimeCreated => new DateTime(1970, 1, 1).AddSeconds(itemDetails.m_rtimeCreated);
        public DateTime TimeUpdated => new DateTime(1970, 1, 1).AddSeconds(itemDetails.m_rtimeUpdated);
        public uint UpVotes => itemDetails.m_unVotesUp;
        public uint DownVotes => itemDetails.m_unVotesDown;
        public float VoteScore => itemDetails.m_flScore;
        public bool IsBanned => itemDetails.m_bBanned;
        public bool IsTagsTruncated => itemDetails.m_bTagsTruncated;
        public bool IsSubscribed => API.UserGeneratedContent.ItemStateHasFlag(StateFlags, EItemState.k_EItemStateSubscribed);
        public bool IsNeedsUpdate => API.UserGeneratedContent.ItemStateHasFlag(StateFlags, EItemState.k_EItemStateNeedsUpdate);
        public bool IsInstalled => API.UserGeneratedContent.ItemStateHasFlag(StateFlags, EItemState.k_EItemStateInstalled);
        public bool IsDownloading => API.UserGeneratedContent.ItemStateHasFlag(StateFlags, EItemState.k_EItemStateDownloading);
        public bool IsDownloadPending => API.UserGeneratedContent.ItemStateHasFlag(StateFlags, EItemState.k_EItemStateDownloadPending);
        public float DownloadCompletion
        {
            get
            {
                API.UserGeneratedContent.Client.GetItemDownloadInfo(FileId, out float value);
                return value;
            }
        }
        public int FileSize => itemDetails.m_nFileSize;
        public string FolderPath
        {
            get
            {
                API.UserGeneratedContent.Client.GetItemInstallInfo(FileId, out ulong _, out string path, out DateTime _);
                return path;
            }
        }
        public EItemState StateFlags => (EItemState)SteamUGC.GetItemState(itemDetails.m_nPublishedFileId);
        public ERemoteStoragePublishedFileVisibility Visibility => itemDetails.m_eVisibility;
        public string[] Tags => itemDetails.m_rgchTags?.Split(',');
        public Texture2D previewImage;
        public string previewImageLocation;
        public SteamUGCDetails_t SourceItemDetails => itemDetails;
        private SteamUGCDetails_t itemDetails;
        public string metadata;
        public StringKeyValuePair[] keyValueTags;

        public UnityEvent previewImageUpdated = new UnityEvent();

        public CallResult<RemoteStorageDownloadUGCResult_t> m_RemoteStorageDownloadUGCResult;

        public UGCCommunityItem(SteamUGCDetails_t itemDetails)
        {
            this.itemDetails = itemDetails;

            if (itemDetails.m_eFileType != EWorkshopFileType.k_EWorkshopFileTypeCommunity)
            {
                Debug.LogWarning("HeathenWorkshopReadItem is designed to display File Type = Community Item, this item is not a community item and may not load correctly.");
            }

            m_RemoteStorageDownloadUGCResult = CallResult<RemoteStorageDownloadUGCResult_t>.Create(HandleUGCDownload);

            if (itemDetails.m_nPreviewFileSize > 0)
            {
                var previewCall = SteamRemoteStorage.UGCDownload(itemDetails.m_hPreviewFile, 1);
                m_RemoteStorageDownloadUGCResult.Set(previewCall, HandleUGCDownloadPreviewFile);
            }
            else
            {
                Debug.LogWarning("Item [" + Title + "] has no preview file!");
            }
        }

        public void DownloadPreviewImage()
        {
            if (previewImage == null)
            {
                if (itemDetails.m_nPreviewFileSize > 0)
                {
                    var previewCall = SteamRemoteStorage.UGCDownload(itemDetails.m_hPreviewFile, 1);
                    m_RemoteStorageDownloadUGCResult.Set(previewCall, HandleUGCDownloadPreviewFile);
                }
                else
                {
                    Debug.LogWarning("Item [" + Title + "] has no preview file!");
                }
            }
        }

        /// <summary>
        /// Request delete of this item
        /// </summary>
        /// <param name="callback"></param>
        public void DeleteItem(Action<DeleteItemResult_t, bool> callback) => API.UserGeneratedContent.Client.DeleteItem(FileId, callback);
        /// <summary>
        /// Request download of this item
        /// </summary>
        /// <param name="highPriority"></param>
        /// <returns></returns>
        public bool DownloadItem(bool highPriority) => API.UserGeneratedContent.Client.DownloadItem(FileId, highPriority);
        /// <summary>
        /// Subscribe to the item
        /// </summary>
        /// <param name="callback"></param>
        public void Subscribe(Action<RemoteStorageSubscribePublishedFileResult_t, bool> callback) => API.UserGeneratedContent.Client.SubscribeItem(FileId, callback);
        /// <summary>
        /// Unsubscribe to the item
        /// </summary>
        /// <param name="callback"></param>
        public void Unsubscribe(Action<RemoteStorageUnsubscribePublishedFileResult_t, bool> callback) => API.UserGeneratedContent.Client.UnsubscribeItem(FileId, callback);
        /// <summary>
        /// Set the user's vote for this item
        /// </summary>
        /// <param name="voteUp"></param>
        /// <param name="callback"></param>
        public void SetVote(bool voteUp, Action<SetUserItemVoteResult_t, bool> callback) => API.UserGeneratedContent.Client.SetUserItemVote(FileId, voteUp, callback);

        /// <summary>
        /// Generic handler useful for testing and debugging
        /// </summary>
        /// <param name="param"></param>
        /// <param name="bIOFailure"></param>
        private void HandleUGCDownload(RemoteStorageDownloadUGCResult_t param, bool bIOFailure)
        {
            if (!bIOFailure)
            {
                Debug.LogError("UGC Download generic handler loaded without failure.");
            }
            else
            {
                Debug.LogError("UGC Download request failed.");
            }
        }

        private void HandleUGCDownloadPreviewFile(RemoteStorageDownloadUGCResult_t param, bool bIOFailure)
        { 
            if (!bIOFailure)
            {
                if (param.m_eResult == EResult.k_EResultOK)
                {
                    byte[] imageBuffer = new byte[param.m_nSizeInBytes];
                    var count = SteamRemoteStorage.UGCRead(param.m_hFile, imageBuffer, param.m_nSizeInBytes, 0, EUGCReadAction.k_EUGCRead_ContinueReadingUntilFinished);
                    //Initialize the image, the LoadImage call will resize as required
                    previewImage = new Texture2D(2, 2);
                    previewImage.LoadImage(imageBuffer);
                    previewImageLocation = param.m_pchFileName;
                    previewImageUpdated.Invoke();

                }
                else
                {
                    Debug.LogError("UGC Download: unexpected result state: " + param.m_eResult.ToString() + "\nImage will not be loaded.");
                }
            }
            else
            {
                Debug.LogError("UGC Download request failed.");
            }
        }

        ~UGCCommunityItem()
        {
            GameObject.Destroy(previewImage);
        }
    }
}
#endif