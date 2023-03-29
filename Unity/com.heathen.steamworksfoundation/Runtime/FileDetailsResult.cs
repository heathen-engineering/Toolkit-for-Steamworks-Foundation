#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET
using Steamworks;

namespace HeathenEngineering.SteamworksIntegration
{
    public struct FileDetailsResult
    {
        public FileDetailsResult_t data;
        public EResult Result => data.m_eResult;
        public ulong FileSize => data.m_ulFileSize;
        public byte[] SHA1Hash => data.m_FileSHA;
        public uint Flags => data.m_unFlags;

        public static implicit operator FileDetailsResult(FileDetailsResult_t native) => new FileDetailsResult { data = native };
        public static implicit operator FileDetailsResult_t(FileDetailsResult heathen) => heathen.data;
    }
}
#endif