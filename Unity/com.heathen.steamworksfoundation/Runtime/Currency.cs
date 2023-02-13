#if !DISABLESTEAMWORKS && HE_SYSCORE && STEAMWORKSNET

namespace HeathenEngineering.SteamworksIntegration
{
    public static class Currency
    {
        public enum Code
        {
            Unknown,
            AED,
            ARS,
            AUD,
            BRL,
            CAD,
            CHF,
            CLP,
            CNY,
            COP,
            CRC,
            EUR,
            GBP,
            HKD,
            ILS,
            IDR,
            INR,
            JPY,
            KRW,
            KWD,
            KZT,
            MXN,
            MYR,
            NOK,
            NZD,
            PEN,
            PHP,
            PLN,
            QAR,
            RUB,
            SAR,
            SGD,
            THB,
            TRY,
            TWD,
            UAH,
            USD,
            UYU,
            VND,
            ZAR,
        }

        public static string GetSymbol(Code code)
        {
            switch (code)
            {
                case Code.Unknown: return string.Empty;
                case Code.AED: return "د.إ";
                case Code.BRL: return "R$";
                case Code.CHF: return "CHF";
                case Code.CNY: return "¥";
                case Code.CRC: return "₡";
                case Code.EUR: return "€";
                case Code.GBP: return "£";
                case Code.ILS: return "₪";
                case Code.IDR: return "Rp";
                case Code.INR: return "₹";
                case Code.JPY: return "¥";
                case Code.KRW: return "₩";
                case Code.KWD: return "د.ك";
                case Code.KZT: return "лв";
                case Code.MYR: return "RM";
                case Code.NOK: return "kr";
                case Code.PEN: return "S/.";
                case Code.PHP: return "₱";
                case Code.PLN: return "zł";
                case Code.QAR: return "﷼";
                case Code.RUB: return "₽";
                case Code.SAR: return "﷼";
                case Code.THB: return "฿";
                case Code.TRY: return "₺";
                case Code.TWD: return "NT$";
                case Code.UAH: return "₴";
                case Code.UYU: return "$U";
                case Code.VND: return "₫";
                case Code.ZAR: return "R";
                default: return "$";
            }
        }
    }
}
#endif