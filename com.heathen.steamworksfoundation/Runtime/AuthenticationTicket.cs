﻿#if HE_SYSCORE && STEAMWORKS_NET
using Steamworks;
using System;

namespace HeathenEngineering.SteamworksIntegration
{
    /// <summary>
    /// Represents a ticekt such as is generated by a user and sent to start an authentication session.
    /// </summary>
    [Serializable]
    public class AuthenticationTicket
    {
        #region Depricated
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Use IsClientTicket")]
        public bool isClientTicket => IsClientTicket;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Use Handle")]
        public HAuthTicket handle => Handle;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Use Data")]
        public byte[] data => Data;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Use Verified")]
        public bool verified => Verified;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Use CreatedOn")]
        public uint createdOn => CreatedOn;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Use Result")]
        public EResult result => Result;
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("Use Callback")]
        public Action<AuthenticationTicket, bool> callback => Callback;
        #endregion

        /// <summary>
        /// Indicates that this session is being managed by a client or server
        /// </summary>
        public bool IsClientTicket { get; private set; } = true;
        /// <summary>
        /// The authentication handle assoceated with this ticket
        /// </summary>
        public HAuthTicket Handle { get; private set; }
        /// <summary>
        /// The ticket data of this ticket ... this is what should be sent to servers for processing
        /// </summary>
        public byte[] Data { get; private set; }
        /// <summary>
        /// Has this ticket been verified, this gets set to true when the Get Authentication Session responce comes back from the Steamworks backend.
        /// </summary>
        public bool Verified { get; private set; }
        /// <summary>
        /// The Steamworks date time this ticket was created
        /// </summary>
        public uint CreatedOn { get; private set; }

        public EResult Result { get; private set; }
        public Action<AuthenticationTicket, bool> Callback { get; private set; }

        public AuthenticationTicket(Action<AuthenticationTicket, bool> callback, bool isClient = true)
        {
            Callback = callback;
            IsClientTicket = isClient;
            var array = new byte[1024];
            uint m_pcbTicket;
            Handle = SteamUser.GetAuthSessionTicket(array, 1024, out m_pcbTicket);
            CreatedOn = SteamUtils.GetServerRealTime();
            Array.Resize(ref array, (int)m_pcbTicket);
            Data = array;
        }

        public void Authenticate(GetAuthSessionTicketResponse_t responce)
        {
            if (Handle != default(HAuthTicket) && Handle != HAuthTicket.Invalid
                    && responce.m_eResult == EResult.k_EResultOK)
            {
                Result = responce.m_eResult;
                Verified = true;
                Callback?.Invoke(this, false);
            }
            else
            {
                Result = responce.m_eResult;
                Callback?.Invoke(this, true);
            }
        }

        /// <summary>
        /// The age of this ticket from the current server realtime
        /// </summary>
        public TimeSpan Age
        {
            get { return new TimeSpan(0, 0, (int)(SteamUtils.GetServerRealTime() - CreatedOn)); }
        }

        /// <summary>
        /// Cancels the ticekt
        /// </summary>
        public void Cancel()
        {
            if (IsClientTicket)
                SteamUser.CancelAuthTicket(Handle);
            else
                SteamGameServer.CancelAuthTicket(Handle);
        }
    }

}
#endif