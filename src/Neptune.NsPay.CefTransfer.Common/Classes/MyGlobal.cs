using Neptune.NsPay.CefTransfer.Common.Models;
using Neptune.NsPay.CefTransfer.Common.MyEnums;
using Neptune.NsPay.WithdrawalDevices;
using System.Drawing.Imaging;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Neptune.NsPay.CefTransfer.Common.Classes
{
    public static class MyGlobal
    {
        public static string MilisecondsToTimeTaken(this long ms)
        {
            var result = string.Empty;
            TimeSpan t = TimeSpan.FromMilliseconds(ms);
            if (t.Days > 0 || !string.IsNullOrWhiteSpace(result))
            {
                result += string.Format("{1}{0:D2}d", t.Days, string.IsNullOrWhiteSpace(result) ? "" : ":");
            }
            if (t.Hours > 0 || !string.IsNullOrWhiteSpace(result))
            {
                result += string.Format("{1}{0:D2}h", t.Hours, string.IsNullOrWhiteSpace(result) ? "" : ":");
            }
            if (t.Minutes > 0 || !string.IsNullOrWhiteSpace(result))
            {
                result += string.Format("{1}{0:D2}m", t.Minutes, string.IsNullOrWhiteSpace(result) ? "" : ":");
            }
            if (t.Seconds > 0 || !string.IsNullOrWhiteSpace(result))
            {
                result += string.Format("{1}{0:D2}s", t.Seconds, string.IsNullOrWhiteSpace(result) ? "" : ":");
            }

            return result;
        }

        public static string RemoveDiacritics(this string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string VietnameseToEnglish(this string vnText)
        {
            string result = vnText.RemoveDiacritics();
            result = Regex.Replace(result, "[áàảãạăắằẳẵặâấầẩẫậ]", "a");
            result = Regex.Replace(result, "[éèẻẽẹêếềểễệ]", "e");
            result = Regex.Replace(result, "[íìỉĩị]", "i");
            result = Regex.Replace(result, "[óòỏõọôốồổỗộơớờởỡợ]", "o");
            result = Regex.Replace(result, "[úùủũụưứừửữự]", "u");
            result = Regex.Replace(result, "[ýỳỷỹỵ]", "y");
            result = Regex.Replace(result, "[đ]", "d");
            result = Regex.Replace(result, "[ÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬ]", "A");
            result = Regex.Replace(result, "[ÉÈẺẼẸÊẾỀỂỄỆ]", "E");
            result = Regex.Replace(result, "[ÍÌỈĨỊ]", "I");
            result = Regex.Replace(result, "[ÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢ]", "O");
            result = Regex.Replace(result, "[ÚÙỦŨỤƯỨỪỬỮỰ]", "U");
            result = Regex.Replace(result, "[ÝỲỶỸỴ]", "Y");
            result = Regex.Replace(result, "[Đ]", "D");
            return result;
        }

        public static BankModel GetBank(this Bank bank)
        {
            BankModel result;
            switch (bank)
            {
                case Bank.AcbBank:
                    result = new BankModel()
                    {
                        id = Bank.AcbBank,
                        name = "ACBBank",
                        domain = ".acb.com.vn",
                        url = "https://online.acb.com.vn/",
                        ele_user = "#user-name", // "id.user-name",
                        ele_password = "#password", // "id.password"
                        ele_corp = null,
                    };
                    break;
                case Bank.BidvBank:
                    result = new BankModel()
                    {
                        id = Bank.BidvBank,
                        name = "BIDVBank",
                        domain = ".bidv.com.vn",
                        url = "https://smartbanking.bidv.com.vn/dang-nhap/",
                        ele_user = "input[formcontrolname=\"soDienThoai\"]", // "formcontrolname.soDienThoai",
                        ele_password = "#app_password_matKhau", // "id.app_password_matKhau"
                        ele_corp = null,
                    };
                    break;
                case Bank.MbBank:
                    result = new BankModel()
                    {
                        id = Bank.MbBank,
                        name = "MBBank",
                        domain = ".mbbank.com.vn",
                        url = "https://online.mbbank.com.vn/",
                        ele_user = "#user-id", // "id.user-id",
                        ele_password = "#new-password", // "id.new-password"
                        ele_corp = null,
                    };
                    break;
                case Bank.TcbBank:
                    result = new BankModel()
                    {
                        id = Bank.TcbBank,
                        name = "TCBBank",
                        domain = ".techcombank.com.vn",
                        url = "https://onlinebanking.techcombank.com.vn/#/dashboard/main",
                        ele_user = "#username", // "id.username",
                        ele_password = "#password", // "id.password"
                        ele_corp = null,
                    };
                    break;
                case Bank.PvcomBank:
                    result = new BankModel()
                    {
                        id = Bank.PvcomBank,
                        name = "PVCOMBank",
                        domain = ".pvcombank.com.vn",
                        url = "https://digitalbanking.pvcombank.com.vn/",
                        ele_user = "#username", // "id.username",
                        ele_password = "#password", // "id.password"
                        ele_corp = null,
                    };
                    break;
                case Bank.VcbBank:
                    result = new BankModel()
                    {
                        id = Bank.VcbBank,
                        name = "VCBBank",
                        domain = ".vietcombank.com.vn",
                        url = "https://vcbdigibank.vietcombank.com.vn/login?returnUrl=%2F",
                        ele_user = "#username", // "id.username",
                        ele_password = "#app_password_login", // "id.app_password_login"
                        ele_corp = null,
                    };
                    break;
                case Bank.VtbBank:
                    result = new BankModel()
                    {
                        id = Bank.VtbBank,
                        name = "VTBBank",
                        domain = ".vietinbank.vn",
                        url = "https://ipay.vietinbank.vn/login",
                        ele_user = "input[name=\"userName\"]", // "name.userName",
                        ele_password = "input[name=\"accessCode\"]", // "name.accessCode"
                        ele_corp = null,
                    };
                    break;
                case Bank.MbBiz:
                    result = new BankModel()
                    {
                        id = Bank.MbBiz,
                        name = "BusinessMbBank",
                        domain = ".mbbank.com.vn",
                        url = "https://ebank.mbbank.com.vn/cp/pl/login?logout=1",
                        ele_user = "#user-id",
                        ele_password = "#password",
                        ele_corp = "#corp-id",
                    };
                    break;
                case Bank.SeaBank:
                    result = new BankModel()
                    {
                        id = Bank.SeaBank,
                        name = "SeaBank",
                        domain = ".seanet.vn",
                        url = "https://seanet.vn/canhan",
                        ele_user = "input[name=\"username\"]",
                        ele_password = "input[name=\"new-password\"]",
                        ele_corp = null,
                    };
                    break;
                case Bank.TcbBiz:
                    result = new BankModel()
                    {
                        id = Bank.TcbBiz,
                        name = "BusinessTcbBank",
                        domain = ".techcombank.com.vn",
                        url = "https://business.techcombank.com.vn",
                        ele_user = "#username",
                        ele_password = "#password",
                        ele_corp = null,
                    };
                    break;
                case Bank.MsbBank:
                    result = new BankModel()
                    {
                        id = Bank.MsbBank,
                        name = "MsbBank",
                        domain = ".msb.com.vn",
                        url = "https://ebank.msb.com.vn/IBSRetail/Request",
                        ele_user = "#_userName",
                        ele_password = "#msbPassword",
                        ele_corp = null,
                    };
                    break;

                default:
                    result = new BankModel();
                    break;
            }
            return result;
        }

        public static Bank GetConfigBank(this string bankType)
        {
            var result = Bank.Unknown;
            switch (bankType.Trim().ToLower())
            {
                case "acb":
                    result = Bank.AcbBank;
                    break;
                case "tcb":
                    result = Bank.TcbBank;
                    break;
                case "vcb":
                    result = Bank.VcbBank;
                    break;
                case "vtb":
                    result = Bank.VtbBank;
                    break;
                case "sea":
                    result = Bank.SeaBank;
                    break;
                case "msb":
                    result = Bank.MsbBank;
                    break;
                default: break;
            }
            return result;
        }

        public static Bank GetConfigBank(this WithdrawalDevicesBankTypeEnum bankType)
        {
            var result = Bank.Unknown;
            switch (bankType)
            {
                case WithdrawalDevicesBankTypeEnum.ACB:
                    result = Bank.AcbBank;
                    break;
                case WithdrawalDevicesBankTypeEnum.TCB:
                    result = Bank.TcbBank;
                    break;
                case WithdrawalDevicesBankTypeEnum.VCB:
                    result = Bank.VcbBank;
                    break;
                case WithdrawalDevicesBankTypeEnum.VTB:
                    result = Bank.VtbBank;
                    break;
                case WithdrawalDevicesBankTypeEnum.Sea:
                    result = Bank.SeaBank;
                    break;
                case WithdrawalDevicesBankTypeEnum.MSB:
                    result = Bank.MsbBank;
                    break;
                default: break;
            }
            return result;
        }

        public static string ToBase64(this System.Drawing.Bitmap myBitmap)
        {
            Bitmap bImage = myBitmap;  // Your Bitmap Image
            System.IO.MemoryStream ms = new MemoryStream();
            bImage.Save(ms, ImageFormat.Jpeg);
            byte[] byteImage = ms.ToArray();
            return Convert.ToBase64String(byteImage); // Get Base64
        }

        public static System.Drawing.Bitmap ToBitmap(this byte[] byteArray)
        {
            System.Drawing.Bitmap image;
            var stream = new MemoryStream(byteArray);
            image = new System.Drawing.Bitmap(stream);
            return image;
        }

        //public static Bitmap ToBitmap(this byte[] byteArray)
        //{
        //    using (var ms = new MemoryStream(byteArray))
        //    {
        //        return new Bitmap(ms);
        //    }
        //}

        public static decimal StrToDec(this string decString, decimal def = 0.00M)
        {
            decimal d = def;
            decimal.TryParse(decString.Replace(",", "").Replace(" ", ""), out d);
            return d;
        }

        public static int StrToInt(this string intString, int def = 0)
        {
            int i = def;
            int.TryParse(intString, out i);
            return i;
        }

        public static string ToNumber(this string currencyString, Bank bank = Bank.Unknown)
        {
            Match match = Regex.Match(currencyString, @"[\d,.]+");
            if (match.Success)
            {
                switch (bank)
                {
                    case Bank.AcbBank:
                    case Bank.AcbBiz:
                    case Bank.MbBank:
                    case Bank.MsbBank:
                    case Bank.TcbBank:
                    case Bank.TcbBiz:
                    case Bank.VtbBank:
                        return match.Value.Replace(".", "").Replace(",", "");

                    case Bank.Unknown:
                    default:
                        return match.Value;
                }

            }
            return string.Empty;
        }

        public static int GetIntegerOnly(this string text)
        {
            Match match = Regex.Match(text, @"[\d]+");
            if (match.Success)
            {
                return match.Value.StrToInt();
            }
            return 0;
        }

        public static string GetDigitOnly(this string text)
        {
            Match match = Regex.Match(text, @"[\d]+");
            if (match.Success)
            {
                return match.Value;
            }
            return "";
        }

        public static string To3Decimal(this double value)
        {
            return value.ToString("0.###");
        }

        public static int DoubleToInt(this double value)
        {
            return Convert.ToInt32(value);
        }

        public static CommonPage ToCommonPage(this TransferType transType)
        {
            return transType == TransferType.Internal ? CommonPage.InternalTransfer : CommonPage.ExternalTransfer;
        }

        public static string ToBeneficiaryNameCompare(this string benName)
        {
            return benName.VietnameseToEnglish().ToLower().Trim().Replace(" ", "");
        }

        #region VCB
        public static VcbExtBank ToVcbExtBank(this string bank)
        {
            var result = VcbExtBank.Unknown;
            switch (bank.Trim().ToLower())
            {
                case "agribank": result = VcbExtBank.AGRIBANK; break;
                case "bidv": result = VcbExtBank.BIDV; break;
                case "vietinbank": result = VcbExtBank.VIETINBANK; break;
                case "vpbank": result = VcbExtBank.VPBANK; break;
                case "citibankvietnam": result = VcbExtBank.CITIBANKVIETNAM; break;
                case "cubhcm": result = VcbExtBank.CUBHCM; break;
                case "mafc": result = VcbExtBank.MAFC; break;
                case "svfc": result = VcbExtBank.SVFC; break;
                case "napas": result = VcbExtBank.NAPAS; break;
                case "abbank": result = VcbExtBank.ABBANK; break;
                case "lpbank": result = VcbExtBank.LPBank; break;
                case "bacabank": result = VcbExtBank.BACABANK; break;
                case "cimb": result = VcbExtBank.CIMB; break;
                case "gpbank": result = VcbExtBank.GPBANK; break;
                case "hlbank": result = VcbExtBank.HLBANK; break;
                case "msb": result = VcbExtBank.MSB; break;
                case "kienlongbank": result = VcbExtBank.KIENLONGBANK; break;
                case "vrb": result = VcbExtBank.VRB; break;
                case "namabank": result = VcbExtBank.NAMABANK; break;
                case "dbs": result = VcbExtBank.DBS; break;
                case "kbhn": result = VcbExtBank.KBHN; break;
                case "kbhcm": result = VcbExtBank.KBHCM; break;
                case "pbvn": result = VcbExtBank.PBVN; break;
                case "nonghuyp": result = VcbExtBank.NONGHUYP; break;
                case "ocb": result = VcbExtBank.OCB; break;
                case "ncb": result = VcbExtBank.NCB; break;
                case "vib": result = VcbExtBank.VIB; break;
                case "shinhan": result = VcbExtBank.SHINHAN; break;
                case "saigonbank": result = VcbExtBank.SAIGONBANK; break;
                case "shb": result = VcbExtBank.SHB; break;
                case "sacombank": result = VcbExtBank.SACOMBANK; break;
                case "hsbc": result = VcbExtBank.HSBC; break;
                case "scvn": result = VcbExtBank.SCVN; break;
                case "ivb": result = VcbExtBank.IVB; break;
                case "ubank": result = VcbExtBank.UBANK; break;
                case "uobvn": result = VcbExtBank.UOBVN; break;
                case "vietbank": result = VcbExtBank.VIETBANK; break;
                case "vietabank": result = VcbExtBank.VIETABANK; break;
                case "eximbank": result = VcbExtBank.EXIMBANK; break;
                case "dongabank": result = VcbExtBank.DONGABANK; break;
                case "pvcombank": result = VcbExtBank.PVCOMBANK; break;
                case "bnpparibashn": result = VcbExtBank.BNPPARIBASHN; break;
                case "bnpparibashcm": result = VcbExtBank.BNPPARIBASHCM; break;
                case "bochk": result = VcbExtBank.BOCHK; break;
                case "bvbank": result = VcbExtBank.BVBank; break;
                case "bvb": result = VcbExtBank.BVB; break;
                case "cake": result = VcbExtBank.CAKE; break;
                case "vbsp": result = VcbExtBank.VBSP; break;
                case "ibkbank": result = VcbExtBank.IBKBANK; break;
                case "co_opbank": result = VcbExtBank.CO_OPBANK; break;
                case "kebhanahanoi": result = VcbExtBank.KEBHANAHANOI; break;
                case "kebhanahcm": result = VcbExtBank.KEBHANAHCM; break;
                case "techcombank": case "techcombank - tcb": case "tcb": result = VcbExtBank.TECHCOMBANK; break;
                case "hdbank": result = VcbExtBank.HDBANK; break;
                case "mbbank": case "mb": result = VcbExtBank.MB; break;
                case "scb": result = VcbExtBank.SCB; break;
                case "pgbank": result = VcbExtBank.PGBANK; break;
                case "tpbank": result = VcbExtBank.TPBANK; break;
                case "woori": result = VcbExtBank.WOORI; break;
                case "cbbank": result = VcbExtBank.CBBANK; break;
                case "liobank": result = VcbExtBank.Liobank; break;
                case "bvbanktimo": result = VcbExtBank.BVBankTimo; break;
                case "umee": result = VcbExtBank.UMEE; break;
                case "acb": result = VcbExtBank.ACB; break;
                case "seabank": result = VcbExtBank.SEABANK; break;
                case "oceanbank": result = VcbExtBank.OCEANBANK; break;
                case "bidchn": result = VcbExtBank.BIDCHN; break;
                case "kbank": result = VcbExtBank.KBANK; break;
                case "vnptmoney": result = VcbExtBank.VNPTMONEY; break;
                case "viettelmoney": result = VcbExtBank.VIETTELMONEY; break;
                case "vcb": case "vietcom": result = VcbExtBank.VCB; break; // 999
                default: break;
            }
            return result;
        }

        public static VcbExtBankModel? GetVcbExtBank(this VcbExtBank bank)
        {
            VcbExtBankModel? result = null;

            switch (bank)
            {
                case VcbExtBank.AGRIBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "AGRIBANK", NameVN = "AGRIBANK", NameEN = "Vietnam Bank for Agriculture and Rural Development (AGRIBANK)", }; break;
                case VcbExtBank.BIDV: result = new VcbExtBankModel() { Bank = bank, ShortName = "BIDV", NameVN = "BIDV", NameEN = "Bank for Investment and Development of Vietnam (BIDV)", }; break;
                case VcbExtBank.VIETINBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "VIETINBANK", NameVN = "VIETINBANK", NameEN = "Bank for Industry and Trade (VIETINBANK)", }; break;
                case VcbExtBank.VPBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "VPBANK", NameVN = "VPBANK", NameEN = "Vietnam Prosperity Bank (VPBANK)", }; break;
                case VcbExtBank.CITIBANKVIETNAM: result = new VcbExtBankModel() { Bank = bank, ShortName = "CITIBANK VIETNAM", NameVN = "CITIBANK VIETNAM", NameEN = "CITIBANK VIETNAM", }; break;
                case VcbExtBank.CUBHCM: result = new VcbExtBankModel() { Bank = bank, ShortName = "CUB HCM", NameVN = "CUB HCM", NameEN = "Cathay United Bank – HO CHI MINH BRANCH (CUBHCM)", }; break;
                case VcbExtBank.MAFC: result = new VcbExtBankModel() { Bank = bank, ShortName = "MAFC", NameVN = "MAFC", NameEN = "Mirae Asset Finance Company VN (MAFC)", }; break;
                case VcbExtBank.SVFC: result = new VcbExtBankModel() { Bank = bank, ShortName = "SVFC", NameVN = "SVFC", NameEN = "Shinhan Vietnam Finace company Limited (SVFC)", }; break;
                case VcbExtBank.NAPAS: result = new VcbExtBankModel() { Bank = bank, ShortName = "NAPAS", NameVN = "NAPAS", NameEN = "NAPAS", }; break;
                case VcbExtBank.ABBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "ABBANK", NameVN = "ABBANK", NameEN = "An Binh Bank (ABBANK)", }; break;
                case VcbExtBank.LPBank: result = new VcbExtBankModel() { Bank = bank, ShortName = "LPBank", NameVN = "LPBank", NameEN = "Lien Viet Post Bank (LPBank)", }; break;
                case VcbExtBank.BACABANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "BAC A BANK", NameVN = "BAC A BANK", NameEN = "Bac A Bank (BAC A BANK)", }; break;
                case VcbExtBank.CIMB: result = new VcbExtBankModel() { Bank = bank, ShortName = "CIMB", NameVN = "CIMB", NameEN = "CIMB Bank Vietnam (CIMB)", }; break;
                case VcbExtBank.GPBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "GPBANK", NameVN = "GPBANK", NameEN = "Global Petro Bank (GPBANK)", }; break;
                case VcbExtBank.HLBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "HLBANK", NameVN = "HLBANK", NameEN = "Hong Leong Bank Vietnam Limited (HLBANK)", }; break;
                case VcbExtBank.MSB: result = new VcbExtBankModel() { Bank = bank, ShortName = "MSB", NameVN = "MSB", NameEN = "Maritime Bank (MSB)", }; break;
                case VcbExtBank.KIENLONGBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "KIEN LONG BANK", NameVN = "KIEN LONG BANK", NameEN = "Kien Long Bank (KIEN LONG BANK)", }; break;
                case VcbExtBank.VRB: result = new VcbExtBankModel() { Bank = bank, ShortName = "VRB", NameVN = "VRB", NameEN = "Vietnam - Russia Joint Venture Bank (VRB)", }; break;
                case VcbExtBank.NAMABANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "NAM A BANK", NameVN = "NAM A BANK", NameEN = "Nam A Bank (NAM A BANK)", }; break;
                case VcbExtBank.DBS: result = new VcbExtBankModel() { Bank = bank, ShortName = "DBS", NameVN = "DBS", NameEN = "DBS Bank Ltd - Ho Chi Minh Branch (DBS)", }; break;
                case VcbExtBank.KBHN: result = new VcbExtBankModel() { Bank = bank, ShortName = "KBHN", NameVN = "KBHN", NameEN = "Kookmin Bank - Hanoi Branch (KBHN)", }; break;
                case VcbExtBank.KBHCM: result = new VcbExtBankModel() { Bank = bank, ShortName = "KBHCM", NameVN = "KBHCM", NameEN = "Kookmin Bank - Ho Chi Minh City Branch (KBHCM)", }; break;
                case VcbExtBank.PBVN: result = new VcbExtBankModel() { Bank = bank, ShortName = "PBVN", NameVN = "PBVN", NameEN = "Public Bank Vietnam Limited (PBVN)", }; break;
                case VcbExtBank.NONGHUYP: result = new VcbExtBankModel() { Bank = bank, ShortName = "NONGHUYP", NameVN = "NONGHUYP", NameEN = "Nonghyup Bank - Hanoi Branch (NONGHUYP)", }; break;
                case VcbExtBank.OCB: result = new VcbExtBankModel() { Bank = bank, ShortName = "OCB", NameVN = "OCB", NameEN = "Orient Bank (OCB)", }; break;
                case VcbExtBank.NCB: result = new VcbExtBankModel() { Bank = bank, ShortName = "NCB", NameVN = "NCB", NameEN = "National Citizen Bank (NCB)", }; break;
                case VcbExtBank.VIB: result = new VcbExtBankModel() { Bank = bank, ShortName = "VIB", NameVN = "VIB", NameEN = "Vietnam International Bank (VIB)", }; break;
                case VcbExtBank.SHINHAN: result = new VcbExtBankModel() { Bank = bank, ShortName = "SHINHAN", NameVN = "SHINHAN", NameEN = "SHINHAN Bank Vietnam (SHINHAN)", }; break;
                case VcbExtBank.SAIGONBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "SAIGONBANK", NameVN = "SAIGONBANK", NameEN = "Saigon Bank For Industry and Trade (SAIGONBANK)", }; break;
                case VcbExtBank.SHB: result = new VcbExtBankModel() { Bank = bank, ShortName = "SHB", NameVN = "SHB", NameEN = "Saigon Hanoi Bank (SHB)", }; break;
                case VcbExtBank.SACOMBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "SACOMBANK", NameVN = "SACOMBANK", NameEN = "Sai Gon Thuong Tin Bank (SACOMBANK)", }; break;
                case VcbExtBank.HSBC: result = new VcbExtBankModel() { Bank = bank, ShortName = "HSBC", NameVN = "HSBC", NameEN = "HSBC Bank Vietnam Ltd. (HSBC)", }; break;
                case VcbExtBank.SCVN: result = new VcbExtBankModel() { Bank = bank, ShortName = "SCVN", NameVN = "SCVN", NameEN = "Standard Chartered Vietnam (SCVN)", }; break;
                case VcbExtBank.IVB: result = new VcbExtBankModel() { Bank = bank, ShortName = "IVB", NameVN = "IVB", NameEN = "Indovina Bank Ltd. (IVB)", }; break;
                case VcbExtBank.UBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "UBANK", NameVN = "UBANK", NameEN = "UBANK BY VPBANK (UBANK)", }; break;
                case VcbExtBank.UOBVN: result = new VcbExtBankModel() { Bank = bank, ShortName = "UOB VN", NameVN = "UOB VN", NameEN = "UOB Vietnam bank (UOB VN)", }; break;
                case VcbExtBank.VIETBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "VIETBANK", NameVN = "VIETBANK", NameEN = "Vietnam Thuong Tin Bank (VIETBANK)", }; break;
                case VcbExtBank.VIETABANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "VIET A BANK", NameVN = "VIET A BANK", NameEN = "Viet A Bank (VIET A BANK)", }; break;
                case VcbExtBank.EXIMBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "EXIMBANK", NameVN = "EXIMBANK", NameEN = "Vietnam Export Import Bank (EXIMBANK)", }; break;
                case VcbExtBank.DONGABANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "DONG A BANK", NameVN = "DONG A BANK", NameEN = "Dong A Bank (DONG A BANK)", }; break;
                case VcbExtBank.PVCOMBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "PVCOMBANK", NameVN = "PVCOMBANK", NameEN = "Vietnam Public Bank (PVCOMBANK)", }; break;
                case VcbExtBank.BNPPARIBASHN: result = new VcbExtBankModel() { Bank = bank, ShortName = "BNP PARIBAS HN", NameVN = "BNP PARIBAS HN", NameEN = "BNP Paribas Hanoi Branch", }; break;
                case VcbExtBank.BNPPARIBASHCM: result = new VcbExtBankModel() { Bank = bank, ShortName = "BNP PARIBAS HCM", NameVN = "BNP PARIBAS HCM", NameEN = "BNP Paribas Ho Chi Minh City Branch Vietnam", }; break;
                case VcbExtBank.BOCHK: result = new VcbExtBankModel() { Bank = bank, ShortName = "BOCHK", NameVN = "BOCHK", NameEN = "Bank of China (Hongkong) Limited – Ho Chi Minh Branch", }; break;
                case VcbExtBank.BVBank: result = new VcbExtBankModel() { Bank = bank, ShortName = "BVBank", NameVN = "BVBank", NameEN = "Viet Capital Bank (BVBank)", }; break;
                case VcbExtBank.BVB: result = new VcbExtBankModel() { Bank = bank, ShortName = "BVB", NameVN = "BVB", NameEN = "Bao Viet Bank (BVB)", }; break;
                case VcbExtBank.CAKE: result = new VcbExtBankModel() { Bank = bank, ShortName = "CAKE", NameVN = "CAKE", NameEN = "CAKE BY VPBANK (CAKE)", }; break;
                case VcbExtBank.VBSP: result = new VcbExtBankModel() { Bank = bank, ShortName = "VBSP", NameVN = "VBSP", NameEN = "Vietnam Bank for Social Policies (VBSP)", }; break;
                case VcbExtBank.IBKBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "IBK BANK", NameVN = "IBK BANK", NameEN = "Industrial Bank of Korea (IBK BANK)", }; break;
                case VcbExtBank.CO_OPBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "CO-OPBANK", NameVN = "CO-OPBANK", NameEN = "Co-operative bank of Vietnam (CO-OPBANK)", }; break;
                case VcbExtBank.KEBHANAHANOI: result = new VcbExtBankModel() { Bank = bank, ShortName = "KEB HANA", NameVN = "KEB HANA HANOI", NameEN = "KEB HANA BANK – HANOI BRANCH (KEB HANA)", }; break;
                case VcbExtBank.KEBHANAHCM: result = new VcbExtBankModel() { Bank = bank, ShortName = "KEB HANA", NameVN = "KEB HANA HCM", NameEN = "KEB HANA BANK –HO CHI MINH CITY BRANCH (KEB HANA)", }; break;
                case VcbExtBank.TECHCOMBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "TECHCOMBANK", NameVN = "TECHCOMBANK", NameEN = "Vietnam Technological and Commercial Bank (TECHCOMBANK)", }; break;
                case VcbExtBank.HDBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "HD BANK", NameVN = "HD BANK", NameEN = "Ho Chi Minh City Development Bank (HD BANK)", }; break;
                case VcbExtBank.MB: result = new VcbExtBankModel() { Bank = bank, ShortName = "MB", NameVN = "MB", NameEN = "Military Bank (MB)", }; break;
                case VcbExtBank.SCB: result = new VcbExtBankModel() { Bank = bank, ShortName = "SCB", NameVN = "SCB", NameEN = "Saigon Bank (SCB)", }; break;
                case VcbExtBank.PGBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "PGBANK", NameVN = "PGBANK", NameEN = "Prosperity and Growth Commercial Joint Stock Bank (PGBANK)", }; break;
                case VcbExtBank.TPBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "TPBANK", NameVN = "TPBANK", NameEN = "Tien Phong Bank (TPBANK)", }; break;
                case VcbExtBank.WOORI: result = new VcbExtBankModel() { Bank = bank, ShortName = "WOORI", NameVN = "WOORI", NameEN = "Woori Bank Vietnam (WOORI)", }; break;
                case VcbExtBank.CBBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "CBBANK", NameVN = "CBBANK", NameEN = "Construction Bank (CBBANK)", }; break;
                case VcbExtBank.Liobank: result = new VcbExtBankModel() { Bank = bank, ShortName = "Liobank", NameVN = "Liobank", NameEN = "Liobank – Orient Commercial Joint Stock Bank (Liobank)", }; break;
                case VcbExtBank.BVBankTimo: result = new VcbExtBankModel() { Bank = bank, ShortName = "BVBank Timo", NameVN = "BVBank Timo", NameEN = "BVBank Timo - Viet Capital Bank", }; break;
                case VcbExtBank.UMEE: result = new VcbExtBankModel() { Bank = bank, ShortName = "UMEE", NameVN = "UMEE", NameEN = "UMEE by Kienlongbank (UMEE)", }; break;
                case VcbExtBank.ACB: result = new VcbExtBankModel() { Bank = bank, ShortName = "ACB", NameVN = "ACB", NameEN = "Asia Commercial Bank (ACB)", }; break;
                case VcbExtBank.SEABANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "SEABANK", NameVN = "SEABANK", NameEN = "Southeast Asia Bank (SEABANK)", }; break;
                case VcbExtBank.OCEANBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "OCEANBANK", NameVN = "OCEANBANK", NameEN = "Ocean Commercial One Member Limited Liability Bank (OCEANBANK)", }; break;
                case VcbExtBank.BIDCHN: result = new VcbExtBankModel() { Bank = bank, ShortName = "BIDC HN", NameVN = "BIDC HN", NameEN = "Bank for investment and development of cambodia PLC – Hanoi Branch (BIDC HN)", }; break;
                case VcbExtBank.KBANK: result = new VcbExtBankModel() { Bank = bank, ShortName = "KBANK", NameVN = "KBANK", NameEN = "KASIKORNBANK public company limited - Ho Chi Minh City Branch (KBANK)", }; break;
                case VcbExtBank.VNPTMONEY: result = new VcbExtBankModel() { Bank = bank, ShortName = "VNPT MONEY", NameVN = "VNPT MONEY", NameEN = "VNPT Money", }; break;
                case VcbExtBank.VIETTELMONEY: result = new VcbExtBankModel() { Bank = bank, ShortName = "VIETTEL MONEY", NameVN = "VIETTEL MONEY", NameEN = "Viettel Money", }; break;

                default: break;
            }
            return result;
        }

        public static string? GetVcbShortName(this VcbExtBank bank)
        {
            return bank.GetVcbExtBank()?.ShortName;
        }

        public static string? GetVcbName(this VcbExtBank bank, Lang lang)
        {
            string? result = null;
            if (lang == Lang.English)
            {
                result = bank.GetVcbExtBank()?.NameEN;
            }
            else if (lang == Lang.Vietnamese)
            {
                result = bank.GetVcbExtBank()?.NameVN;
            }
            return result;
        }

        public static string VcbTranslate(this VcbText txt, Lang lang)
        {
            string result = string.Empty;
            switch (txt)
            {
                case VcbText.AvailableBalance:
                    result = lang == Lang.English ? "Available balance" : "Số dư khả dụng";
                    break;
                case VcbText.BeneficiaryName:
                    result = lang == Lang.English ? "Beneficiary name" : "Tên người thụ hưởng";
                    break;
                case VcbText.TransactionId:
                    result = lang == Lang.English ? "Transaction ID" : "Mã giao dịch";
                    break;

                default: break;
            }
            return result;
        }
        #endregion

        #region VTB
        public static VtbExtBank ToVtbExtBank(this string bank)
        {
            var result = VtbExtBank.Unknown;
            switch (bank.Trim().ToLower())
            {
                case "vietcomb": case "vcb": case "vietcombank": case "vietcom": result = VtbExtBank.Vietcombank; break;
                case "bidv": case "bidvbank": result = VtbExtBank.BIDV; break;
                case "agribank": result = VtbExtBank.Agribank; break;
                case "mbbank": case "mb": result = VtbExtBank.MB; break;
                case "techcomb": case "tcb": case "techcombank": case "techcom": result = VtbExtBank.Techcombank; break;
                case "acb": case "acbbank": result = VtbExtBank.ACB; break;
                case "sbank": case "stb": case "sacombank": result = VtbExtBank.Sacombank; break;
                case "vpbank": case "vpb": result = VtbExtBank.VPBank; break;
                case "vib": case "vibbank": result = VtbExtBank.VIB; break;
                case "tpbank": case "tpb": case "tp": result = VtbExtBank.TPBank; break;
                case "maribank": case "msb": case "msbbank": result = VtbExtBank.MSB; break;
                case "dab": case "dongabank": case "dongabankdab": result = VtbExtBank.DongABank; break;
                case "shb": case "shbbank": result = VtbExtBank.SHB; break;
                case "scb": result = VtbExtBank.SCB; break;
                case "lvpbank": case "lienvietbank": case "lpb": case "lpbank": result = VtbExtBank.LPBank; break;
                case "shinhan": result = VtbExtBank.Shinhan; break;
                case "ocbank": case "ocb": case "ocbbank": result = VtbExtBank.OCB; break;
                case "hdbank": case "hdb": result = VtbExtBank.HDBANK; break;
                case "seabank": case "seab": result = VtbExtBank.SEABANK; break;
                case "abb": case "abbank": result = VtbExtBank.ABBANK; break;
                case "exim": case "eib": case "eximbankeib": case "eximbank": result = VtbExtBank.Eximbank; break;
                case "nabank": case "nama": case "namabank": result = VtbExtBank.NamA; break;
                case "wrbank": case "woo": case "wooribank": result = VtbExtBank.Wooribank; break;
                case "pvbank": case "pvcombank": result = VtbExtBank.PVcomBank; break;
                case "vietcapitalbank": result = VtbExtBank.VietCapitalBank; break;
                case "klbank": case "kienlongbank": case "kienlongbankklb": case "klb": case "kienlong": result = VtbExtBank.KienLong; break;
                case "ncb": result = VtbExtBank.NCB; break;
                case "pgba": case "pgbank": result = VtbExtBank.PGBank; break;
                case "vietbank": result = VtbExtBank.VietBank; break;
                case "cake": result = VtbExtBank.CAKE; break;
                case "bvb": case "baovietbank": case "baovietbankbvb": result = VtbExtBank.BaoVietBank; break;
                case "stanchart": result = VtbExtBank.StanChart; break;
                case "bab": case "bacabank": case "bacabanknasb": result = VtbExtBank.BAB; break;
                case "vietabank": result = VtbExtBank.VietABank; break;
                case "obank": case "oceanbank": case "ocean": result = VtbExtBank.OceanBank; break;
                case "sabank": case "sgb": case "saigonbank": result = VtbExtBank.Saigonbank; break;
                case "hsbcbank": case "hsbc": result = VtbExtBank.HSBC; break;
                case "indovina": result = VtbExtBank.Indovina; break;
                case "cimbvnd": case "cimb": case "cimbbank": result = VtbExtBank.CIMB; break;
                case "co-opbank": result = VtbExtBank.Co_opBank; break;
                case "gpbank": result = VtbExtBank.GPBank; break;
                case "vrb": result = VtbExtBank.VRB; break;
                case "public bank": result = VtbExtBank.PublicBank; break;
                case "cbbank": result = VtbExtBank.CBBank; break;
                case "hlbank": case "hlb": case "hongleongbank": case "hongleong": result = VtbExtBank.HongLeong; break;
                case "ubank": result = VtbExtBank.UBANK; break;
                case "ibkhanoi": result = VtbExtBank.IBKHanoi; break;
                case "dbs": result = VtbExtBank.DBS; break;
                case "ibk hcmc": result = VtbExtBank.IBKHCMC; break;
                case "kookmin hanoi": result = VtbExtBank.KookminHanoi; break;
                case "kookmin hcmc": result = VtbExtBank.KookminHCMC; break;
                case "nonghyup": result = VtbExtBank.Nonghyup; break;
                case "kbank": result = VtbExtBank.KBank; break;
                case "uob": result = VtbExtBank.UOB; break;
                case "cub hcm": result = VtbExtBank.CUBHCM; break;
                case "citibank": result = VtbExtBank.CitiBank; break;
                case "bidc": result = VtbExtBank.BIDC; break;
                case "vikki by hdbank": result = VtbExtBank.VikkibyHDBank; break;
                case "svfc": result = VtbExtBank.SVFC; break;
                case "liobank": result = VtbExtBank.Liobank; break;
                case "timo": result = VtbExtBank.Timo; break;
                case "umee": result = VtbExtBank.UMEE; break;
                case "bnp paribas hcm": result = VtbExtBank.BNPPARIBASHCM; break;
                case "bnp paribas hn": result = VtbExtBank.BNPPARIBASHN; break;
                case "bank of china": result = VtbExtBank.BankofChina; break;
                case "kebhanahcm": result = VtbExtBank.KEBHANAHCM; break;
                case "kebhanahn": result = VtbExtBank.KEBHANAHN; break;
                case "viettelmoney": result = VtbExtBank.ViettelMoney; break;
                case "vnptmoney": result = VtbExtBank.VNPTMoney; break;
                case "napas ha noi": result = VtbExtBank.NapasHaNoi; break;
                case "pvcombank-napas": result = VtbExtBank.PVCombank_NAPAS; break;
                case "mafc": result = VtbExtBank.MAFC; break;
                case "vbsp": result = VtbExtBank.VBSP; break;

                case "icb": case "vtb": case "vietinbank": case "vietin": case "vietinba": result = VtbExtBank.Vtb; break;
                default: break;
            }
            return result;
        }

        public static VtbExtBankModel? GetVtbExtBank(this VtbExtBank bank)
        {
            VtbExtBankModel? result = null;
            switch (bank)
            {
                case VtbExtBank.Vietcombank: result = new VtbExtBankModel() { Bank = bank, ShortName = "Vietcombank", NameVN = "Vietcombank_Ngân hàng Ngoại thương Việt Nam (VCB)", NameEN = "Vietcombank_Bank for Foreign Trade of VN (VCB)", }; break;
                case VtbExtBank.BIDV: result = new VtbExtBankModel() { Bank = bank, ShortName = "BIDV", NameVN = "BIDV_Ngân hàng Đầu tư và Phát triển Việt Nam", NameEN = "BIDV_Bank for Investment and Development of Vietnam", }; break;
                case VtbExtBank.Agribank: result = new VtbExtBankModel() { Bank = bank, ShortName = "Agribank", NameVN = "Agribank_Ngân hàng Nông nghiệp và Phát triển nông thôn Việt Nam", NameEN = "Agribank_Vietnam Bank for Agriculture and Rural Development", }; break;
                case VtbExtBank.MB: result = new VtbExtBankModel() { Bank = bank, ShortName = "MB", NameVN = "MB_Ngân hàng Quân đội", NameEN = "MB_Military Bank", }; break;
                case VtbExtBank.Techcombank: result = new VtbExtBankModel() { Bank = bank, ShortName = "Techcombank", NameVN = "Techcombank_Ngân hàng Kỹ thương Việt Nam (TCB)", NameEN = "Techcombank_Vietnam Technological and Commercial Bank (TCB)", }; break;
                case VtbExtBank.ACB: result = new VtbExtBankModel() { Bank = bank, ShortName = "ACB", NameVN = "ACB_Ngân hàng Á Châu", NameEN = "ACB_Asia Commercial Bank", }; break;
                case VtbExtBank.Sacombank: result = new VtbExtBankModel() { Bank = bank, ShortName = "Sacombank", NameVN = "Sacombank_Ngân hàng Sài Gòn Thương Tín (STB)", NameEN = "Sacombank_Sai Gon Thuong Tin Bank (STB)", }; break;
                case VtbExtBank.VPBank: result = new VtbExtBankModel() { Bank = bank, ShortName = "VPBank", NameVN = "VPBank_Ngân hàng Việt Nam Thịnh vượng", NameEN = "VPBank_Vietnam Prosperity Bank", }; break;
                case VtbExtBank.VIB: result = new VtbExtBankModel() { Bank = bank, ShortName = "VIB", NameVN = "VIB_Ngân hàng Quốc tế Việt Nam", NameEN = "VIB_Vietnam International Bank", }; break;
                case VtbExtBank.TPBank: result = new VtbExtBankModel() { Bank = bank, ShortName = "TPBank", NameVN = "TPBank_Ngân hàng Tiên Phong", NameEN = "TPBank_Tien Phong Bank", }; break;
                case VtbExtBank.MSB: result = new VtbExtBankModel() { Bank = bank, ShortName = "MSB", NameVN = "MSB_Ngân hàng Hàng Hải", NameEN = "MSB_Maritime Bank", }; break;
                case VtbExtBank.DongABank: result = new VtbExtBankModel() { Bank = bank, ShortName = "DongABank", NameVN = "DongABank_Ngân hàng Đông Á (DAB)", NameEN = "DongABank_Dong A Bank (DAB)", }; break;
                case VtbExtBank.SHB: result = new VtbExtBankModel() { Bank = bank, ShortName = "SHB", NameVN = "SHB_Ngân hàng Sài Gòn Hà Nội", NameEN = "SHB_Saigon Hanoi Bank", }; break;
                case VtbExtBank.SCB: result = new VtbExtBankModel() { Bank = bank, ShortName = "SCB", NameVN = "SCB_Ngân hàng Sài Gòn", NameEN = "SCB_Sai Gon Bank", }; break;
                case VtbExtBank.LPBank: result = new VtbExtBankModel() { Bank = bank, ShortName = "LPBank", NameVN = "LPBank_Ngân hàng Bưu điện Liên Việt (LPB)", NameEN = "LPBank_LPBank (LPB)", }; break;
                case VtbExtBank.Shinhan: result = new VtbExtBankModel() { Bank = bank, ShortName = "Shinhan", NameVN = "Shinhan_Ngân hàng Shinhan Việt Nam (SHBVN)", NameEN = "Shinhan_Shinhan Vietnam Bank Limited (SHBVN)", }; break;
                case VtbExtBank.OCB: result = new VtbExtBankModel() { Bank = bank, ShortName = "OCB", NameVN = "OCB_Ngân hàng Phương Đông", NameEN = "OCB_Orient Commercial Joint Stock Bank", }; break;
                case VtbExtBank.HDBANK: result = new VtbExtBankModel() { Bank = bank, ShortName = "HDBANK", NameVN = "HDBANK_Ngân hàng Phát triển TP HCM", NameEN = "HDBANK_Ho Chi Minh City Development Bank", }; break;
                case VtbExtBank.SEABANK: result = new VtbExtBankModel() { Bank = bank, ShortName = "SEABANK", NameVN = "SEABANK_Ngân hàng Đông Nam Á", NameEN = "SEABANK_SouthEast Asia Bank", }; break;
                case VtbExtBank.ABBANK: result = new VtbExtBankModel() { Bank = bank, ShortName = "ABBANK", NameVN = "ABBANK_Ngân hàng An Bình", NameEN = "ABBANK_An Binh Bank", }; break;
                case VtbExtBank.Eximbank: result = new VtbExtBankModel() { Bank = bank, ShortName = "Eximbank", NameVN = "Eximbank_Ngân hàng xuất nhập khẩu Việt Nam", NameEN = "Eximbank_Vietnam Export Import Bank", }; break;
                case VtbExtBank.NamA: result = new VtbExtBankModel() { Bank = bank, ShortName = "NamA", NameVN = "Nam Á_ Ngân hàng Nam Á", NameEN = "NamA_Nam A Bank", }; break;
                case VtbExtBank.Wooribank: result = new VtbExtBankModel() { Bank = bank, ShortName = "Wooribank", NameVN = "Wooribank_Ngân hàng Woori Việt Nam", NameEN = "Wooribank_Woori Bank Vietnam Limited", }; break;
                case VtbExtBank.PVcomBank: result = new VtbExtBankModel() { Bank = bank, ShortName = "PVcomBank", NameVN = "PVcomBank_Ngân hàng Đại chúng Việt Nam", NameEN = "PVcomBank_Vietnam Public Bank", }; break;
                case VtbExtBank.VietCapitalBank: result = new VtbExtBankModel() { Bank = bank, ShortName = "VietCapitalBank", NameVN = "VietCapitalBank_Ngân hàng Bản Việt", NameEN = "VietCapitalBank_Viet Capital Bank", }; break;
                case VtbExtBank.KienLong: result = new VtbExtBankModel() { Bank = bank, ShortName = "Kien Long", NameVN = "Kien Long_Ngân hàng Kiên Long (KLB)", NameEN = "Kien Long_Kien Long Bank (KLB)", }; break;
                case VtbExtBank.NCB: result = new VtbExtBankModel() { Bank = bank, ShortName = "NCB", NameVN = "NCB_Ngân hàng Quốc Dân", NameEN = "NCB_National Citizen Bank", }; break;
                case VtbExtBank.PGBank: result = new VtbExtBankModel() { Bank = bank, ShortName = "PGBank", NameVN = "PGB_NH TMCP Thinh Vuong Phat Trien", NameEN = "PGBank_Petrolimex Group Bank", }; break;
                case VtbExtBank.VietBank: result = new VtbExtBankModel() { Bank = bank, ShortName = "VietBank", NameVN = "VietBank_Ngân hàng Việt Nam Thương Tín", NameEN = "VietBank_Vietnam Thuong Tin Bank", }; break;
                case VtbExtBank.CAKE: result = new VtbExtBankModel() { Bank = bank, ShortName = "CAKE", NameVN = "CAKE_Ngân hàng số Cake by VPBank", NameEN = "CAKE_Cake by VPBank", }; break;
                case VtbExtBank.BaoVietBank: result = new VtbExtBankModel() { Bank = bank, ShortName = "BaoVietBank", NameVN = "BaoVietBank_Ngân hàng Bảo Việt (BVB)", NameEN = "BaoVietBank_Bao Viet Bank (BVB)", }; break;
                case VtbExtBank.StanChart: result = new VtbExtBankModel() { Bank = bank, ShortName = "StanChart", NameVN = "StanChart_Ngân hàng Standard Chartered Việt Nam (SCVN)", NameEN = "StanChart_Standard Chartered Bank Vietnam (SCVN)", }; break;
                case VtbExtBank.BAB: result = new VtbExtBankModel() { Bank = bank, ShortName = "BAB", NameVN = "BAB_Ngân hàng Bắc Á", NameEN = "BAB_ Bac A Bank", }; break;
                case VtbExtBank.VietABank: result = new VtbExtBankModel() { Bank = bank, ShortName = "VietABank", NameVN = "VietABank_Ngân hàng Việt Á", NameEN = "VietABank_Vietnam Asia Bank", }; break;
                case VtbExtBank.OceanBank: result = new VtbExtBankModel() { Bank = bank, ShortName = "OceanBank", NameVN = "OceanBank_Ngân hàng Đại Dương", NameEN = "OceanBank_Ocean Commercial Bank", }; break;
                case VtbExtBank.Saigonbank: result = new VtbExtBankModel() { Bank = bank, ShortName = "Saigonbank", NameVN = "Saigonbank_Ngân hàng Sài Gòn Công Thương (SGB)", NameEN = "Saigonbank_SaiGon Bank For Industry And Trade (SGB)", }; break;
                case VtbExtBank.HSBC: result = new VtbExtBankModel() { Bank = bank, ShortName = "HSBC", NameVN = "HSBC_Ngân hàng HSBC Việt Nam", NameEN = "HSBC_HSBC Viet Nam", }; break;
                case VtbExtBank.Indovina: result = new VtbExtBankModel() { Bank = bank, ShortName = "Indovina", NameVN = "Indovina_Ngân hàng Indovina (IVB)", NameEN = "Indovina_Indovina Bank (IVB)", }; break;
                case VtbExtBank.CIMB: result = new VtbExtBankModel() { Bank = bank, ShortName = "CIMB", NameVN = "CIMB_Ngân hàng CIMB", NameEN = "CIMB_CIMB Bank", }; break;
                case VtbExtBank.Co_opBank: result = new VtbExtBankModel() { Bank = bank, ShortName = "Co-opBank", NameVN = "Co-opBank_Ngân hàng Hợp tác xã Việt Nam", NameEN = "Co-opBank_Co-operative Bank of Viet Nam", }; break;
                case VtbExtBank.GPBank: result = new VtbExtBankModel() { Bank = bank, ShortName = "GPBank", NameVN = "GPBank_Ngân hàng Dầu khí toàn cầu", NameEN = "GPBank_Global Petro Sole Member Limited Commercial Bank", }; break;
                case VtbExtBank.VRB: result = new VtbExtBankModel() { Bank = bank, ShortName = "VRB", NameVN = "VRB_Ngân hàng liên doanh Việt Nga", NameEN = "VRB_Vietnam - Russia Joint Venture Bank", }; break;
                case VtbExtBank.PublicBank: result = new VtbExtBankModel() { Bank = bank, ShortName = "Public Bank", NameVN = "Public Bank_Ngân hàng Public Việt Nam (PBVN)", NameEN = "Public Bank_Public Bank Vietnam Limited (PBVN)", }; break;
                case VtbExtBank.CBBank: result = new VtbExtBankModel() { Bank = bank, ShortName = "CBBank", NameVN = "CBBank_Ngân hàng Xây dựng Việt Nam", NameEN = "CBBank_Vietnam Construction Bank", }; break;
                case VtbExtBank.HongLeong: result = new VtbExtBankModel() { Bank = bank, ShortName = "Hong Leong", NameVN = "Hong Leong_Ngân hàng Hong Leong Viet Nam (HLBVN)", NameEN = "Hong Leong_Hong Leong Bank Vietnam (HLBVN)", }; break;
                case VtbExtBank.UBANK: result = new VtbExtBankModel() { Bank = bank, ShortName = "UBANK", NameVN = "UBANK_Ngân hàng số UBank by VPBank", NameEN = "UBANK_UBank by VPBank", }; break;
                case VtbExtBank.IBKHanoi: result = new VtbExtBankModel() { Bank = bank, ShortName = "IBK Hanoi", NameVN = "IBK_Industrial Bank of Korea - CN Hà Nội", NameEN = "IBK_Industrial Bank of Korea - Hanoi branch", }; break;
                case VtbExtBank.DBS: result = new VtbExtBankModel() { Bank = bank, ShortName = "DBS", NameVN = "DBS_Ngân hàng DBS - CN HCM", NameEN = "DBS_Development Bank of Singapore - HCMC branch", }; break;
                case VtbExtBank.IBKHCMC: result = new VtbExtBankModel() { Bank = bank, ShortName = "IBK HCMC", NameVN = "IBK_Industrial Bank of Korea - CN HCM", NameEN = "IBK_Industrial Bank of Korea - HCMC branch", }; break;
                case VtbExtBank.KookminHanoi: result = new VtbExtBankModel() { Bank = bank, ShortName = "Kookmin Hanoi", NameVN = "Kookmin_Ngân hàng Kookmin - CN Hà Nội", NameEN = "Kookmin_Kookmin Bank - Hanoi branch", }; break;
                case VtbExtBank.KookminHCMC: result = new VtbExtBankModel() { Bank = bank, ShortName = "Kookmin HCMC", NameVN = "Kookmin_Ngân hàng Kookmin Bank - CN HCM", NameEN = "Kookmin_Kookmin Bank - HCMC branch", }; break;
                case VtbExtBank.Nonghyup: result = new VtbExtBankModel() { Bank = bank, ShortName = "Nonghyup", NameVN = "Nonghyup_ Ngân hàng Nonghyup - CN Hà Nội", NameEN = "Nonghyup_Nonghyup Bank - Hanoi branch", }; break;
                case VtbExtBank.KBank: result = new VtbExtBankModel() { Bank = bank, ShortName = "KBank", NameVN = "KBank_Ngân hàng Đại chúng Kasikornbank - CN HCM", NameEN = "KBank_Kasikornbank Commercial Public Bank - HCMC branch", }; break;
                case VtbExtBank.UOB: result = new VtbExtBankModel() { Bank = bank, ShortName = "UOB", NameVN = "UOB_Ngân hàng United Overseas Bank Việt Nam", NameEN = "UOB_United Overseas Bank Viet Nam", }; break;
                case VtbExtBank.CUBHCM: result = new VtbExtBankModel() { Bank = bank, ShortName = "CUB HCM", NameVN = "Cathay United Bank - Chi nhánh Hồ Chí Minh", NameEN = "CUB HCM", }; break;
                case VtbExtBank.CitiBank: result = new VtbExtBankModel() { Bank = bank, ShortName = "CitiBank", NameVN = "CitiBank_Ngan hang CitiBank Viet Nam", NameEN = "CitiBank_CitiBank Viet Nam", }; break;
                case VtbExtBank.BIDC: result = new VtbExtBankModel() { Bank = bank, ShortName = "BIDC", NameVN = "BIDC_NH DT va PT Campuchia - CN Ha Noi", NameEN = "BIDC_Bank for Investment and Development of Cambodia - Hanoi Branch", }; break;
                case VtbExtBank.VikkibyHDBank: result = new VtbExtBankModel() { Bank = bank, ShortName = "Vikki by HDBank", NameVN = "Vikki by HDBank", NameEN = "Vikki by HDBank", }; break;
                case VtbExtBank.SVFC: result = new VtbExtBankModel() { Bank = bank, ShortName = "SVFC", NameVN = "SVFC_Cty TC TNHH MTV Shinhan VN", NameEN = "SVFC_Shinhan Finance", }; break;
                case VtbExtBank.Liobank: result = new VtbExtBankModel() { Bank = bank, ShortName = "Liobank", NameVN = "Liobank_Ngân hàng số Liobank by OCB", NameEN = "Liobank_Liobank by OCB", }; break;
                case VtbExtBank.Timo: result = new VtbExtBankModel() { Bank = bank, ShortName = "Timo", NameVN = "Ngân hàng số Timo_Timo by Ban Viet Bank", NameEN = "Timo_Timo by Ban Viet Bank", }; break;
                case VtbExtBank.UMEE: result = new VtbExtBankModel() { Bank = bank, ShortName = "UMEE", NameVN = "UMEE_Ngân hàng số UMEE by KienLongBank", NameEN = "UMEE_UMEE by KienLongBank", }; break;
                case VtbExtBank.BNPPARIBASHCM: result = new VtbExtBankModel() { Bank = bank, ShortName = "BNP PARIBAS HCM", NameVN = "BNP Paribas - Chi nhánh TP. Hồ Chí Minh", NameEN = "BNP PARIBAS HCM", }; break;
                case VtbExtBank.BNPPARIBASHN: result = new VtbExtBankModel() { Bank = bank, ShortName = "BNP PARIBAS HN", NameVN = "BNP Paribas - Chi nhánh Hà Nội", NameEN = "BNP PARIBAS HN", }; break;
                case VtbExtBank.BankofChina: result = new VtbExtBankModel() { Bank = bank, ShortName = "Bank of China", NameVN = "Bank of China (Hongkong) Limited - CN Ho Chi Minh", NameEN = "Bank of China (Hongkong) Limited - Ho Chi Minh Branch", }; break;
                case VtbExtBank.KEBHANAHCM: result = new VtbExtBankModel() { Bank = bank, ShortName = "KEBHANAHCM", NameVN = "KEBHANAHCM_KEB HANA CN TP HO CHI MINH", NameEN = "KEBHANAHCM_KEB HANA HO CHI MINH Branch", }; break;
                case VtbExtBank.KEBHANAHN: result = new VtbExtBankModel() { Bank = bank, ShortName = "KEBHANAHN", NameVN = "KEBHANAHN_KEB HANA CN Ha Noi", NameEN = "KEBHANAHN_KEB HANA Ha Noi Branch", }; break;
                case VtbExtBank.ViettelMoney: result = new VtbExtBankModel() { Bank = bank, ShortName = "ViettelMoney", NameVN = "ViettelMoney_Viettel Money", NameEN = "ViettelMoney_Viettel Money", }; break;
                case VtbExtBank.VNPTMoney: result = new VtbExtBankModel() { Bank = bank, ShortName = "VNPTMoney", NameVN = "VNPTMoney_VNPT Money", NameEN = "VNPTMoney_VNPT Money", }; break;
                case VtbExtBank.NapasHaNoi: result = new VtbExtBankModel() { Bank = bank, ShortName = "Napas Ha Noi", NameVN = "Napas Hà Nội", NameEN = "Napas Ha Noi", }; break;
                case VtbExtBank.PVCombank_NAPAS: result = new VtbExtBankModel() { Bank = bank, ShortName = "PVCombank-NAPAS", NameVN = "PVCombank-NAPAS", NameEN = "PVCombank-NAPAS", }; break;
                case VtbExtBank.MAFC: result = new VtbExtBankModel() { Bank = bank, ShortName = "MAFC", NameVN = "MAFC_Cty tai chinh TNHH MTV Mirae Asset (Viet Nam)", NameEN = "MAFC_Mirae Asset (Viet Nam)", }; break;
                case VtbExtBank.VBSP: result = new VtbExtBankModel() { Bank = bank, ShortName = "VBSP", NameVN = "VBSP_Ngân hàng Chính sách Xã hội", NameEN = "VBSP_Chinh sach Xa hoi Bank", }; break;

                default: break;
            }
            return result;
        }

        public static string? GetVtbName(this VtbExtBank bank, Lang lang)
        {
            string? result = null;
            if (lang == Lang.English)
            {
                result = bank.GetVtbExtBank()?.NameEN;
            }
            else if (lang == Lang.Vietnamese)
            {
                result = bank.GetVtbExtBank()?.NameVN;
            }
            return result;
        }

        public static string VtbTranslate(this VtbText txt, Lang lang)
        {
            string result = string.Empty;
            switch (txt)
            {
                case VtbText.Hotline:
                    result = lang == Lang.English ? "Hotline" : "Số điện thoại hỗ trợ";
                    break;

                case VtbText.BeneficiaryName:
                    result = lang == Lang.English ? "Beneficiary name" : "Tên người nhận";
                    break;

                default: break;
            }
            return result;
        }
        #endregion

        #region TCB
        public static TcbExtBank ToTcbExtBank(this string bank)
        {
            var result = TcbExtBank.Unknown;
            switch (bank.Trim().ToLower())
            {
                case "chinaabchanoi": result = TcbExtBank._CHINA_ABC_HANOI; break;
                case "abb": case "abbank": result = TcbExtBank.ABBank; break;
                case "acb": case "acbbank": result = TcbExtBank.ACB; break;
                case "agribank": result = TcbExtBank.Agribank; break;
                case "anzvl": case "anzbank": result = TcbExtBank.ANZBank; break;
                case "bab": case "bacabank": case "bacabanknasb": result = TcbExtBank.BacABank_NASB; break;
                case "bangkokbank": result = TcbExtBank.BangkokBank; break;
                //case "bankofchinabochkhcmcbranch": result = TcbExtBank.BankOfChina_BOCHKHCMCBranch; break;
                //case "bankofindiaboi": result = TcbExtBank.BankofIndia_BOI; break;
                //case "banksinopac": result = TcbExtBank.BankSinopac; break;
                case "bvb": case "baovietbank": case "baovietbankbvb": result = TcbExtBank.BaoVietBank_BVB; break;
                case "bidc": result = TcbExtBank.BIDC; break;
                case "bidv": case "bidvbank": result = TcbExtBank.BIDV; break;
                case "bnpphcm": result = TcbExtBank.BNPPHCM; break;
                case "bnpphn": result = TcbExtBank.BNPPHN; break;
                case "bocom": result = TcbExtBank.BoCom; break;
                case "bpceiom": result = TcbExtBank.BPCEIOM; break;
                case "busanbnk": result = TcbExtBank.Busan_BNK; break;
                case "bvbank": result = TcbExtBank.BVBank; break;
                case "bvbanktimo": result = TcbExtBank.BVBankTimo; break;
                case "cake": result = TcbExtBank.CAKE; break;
                case "cathayunited": result = TcbExtBank.CathayUnited; break;
                case "cbbank": result = TcbExtBank.CBBank; break;
                case "ccb": result = TcbExtBank.CCB; break;
                case "cimbvnd": case "cimb": case "cimbbank": result = TcbExtBank.CIMB; break;
                case "citibank": result = TcbExtBank.Citibank; break;
                case "coopbank": result = TcbExtBank.COOPBANK; break;
                case "ctbc": result = TcbExtBank.CTBC; break;
                case "daegubank": result = TcbExtBank.DaeguBank; break;
                case "dbshcm": result = TcbExtBank.DBS_HCM; break;
                case "deutschebank": result = TcbExtBank.DeutscheBank; break;
                case "dab": case "dongabank": case "dongabankdab": result = TcbExtBank.DongABank_DAB; break;
                case "esunbank": result = TcbExtBank.EsunBank; break;
                case "exim": case "eib": case "eximbankeib": case "eximbank": result = TcbExtBank.Eximbank_EIB; break;
                case "firstcommercialbank": result = TcbExtBank.FirstCommercialBank; break;
                case "gpbank": result = TcbExtBank.GPBank; break;
                case "hdbank": case "hdb": result = TcbExtBank.HDBank; break;
                case "hongleonghlbvn": result = TcbExtBank.HONGLEONG_HLBVN; break;
                case "hsbcvietnam": result = TcbExtBank.HSBCVietnam; break;
                case "huananbank": result = TcbExtBank.HuaNanBank; break;
                case "ibkhcm": result = TcbExtBank.IBK_HCM; break;
                case "ibkhn": result = TcbExtBank.IBK_HN; break;
                case "icbchanoi": result = TcbExtBank.ICBCHanoi; break;
                case "indobank": case "ivb": result = TcbExtBank.IVB; break;
                //case "jpmorgan": result = TcbExtBank.J_P_Morgan; break;
                case "kbank": result = TcbExtBank.KBANK; break;
                case "kbhcm": result = TcbExtBank.KBHCM; break;
                case "kbhn": result = TcbExtBank.KBHN; break;
                case "kebhanahcm": result = TcbExtBank.KEBHANAHCM; break;
                case "kebhanahn": result = TcbExtBank.KEBHANAHN; break;
                //case "khobacnhanuockbnn": result = TcbExtBank.KhobacNhanuoc_KBNN; break;
                case "klbank": case "kienlongbank": case "kienlongbankklb": case "klb": case "kienlong": result = TcbExtBank.Kienlongbank_KLB; break;
                case "liobank": result = TcbExtBank.Liobank; break;
                case "lvpbank": case "lienvietbank": case "lpb": case "lpbank": result = TcbExtBank.LPBANK; break;
                case "mafc": result = TcbExtBank.MAFC; break;
                case "maybank": result = TcbExtBank.Maybank; break;
                case "mbbank": case "mb": result = TcbExtBank.MB; break;
                case "megaicbc": result = TcbExtBank.MegaICBC; break;
                case "mhb": result = TcbExtBank.MHB; break;
                case "mizuho": result = TcbExtBank.Mizuho; break;
                case "maribank": case "msb": case "msbbank": result = TcbExtBank.MSB; break;
                case "mufgbank": result = TcbExtBank.MUFGBank; break;
                case "nab": result = TcbExtBank.NAB; break;
                case "ncb": result = TcbExtBank.NCB; break;
                case "nonghuypbank": result = TcbExtBank.NonghuypBank; break;
                case "ocbank": case "ocb": case "ocbbank": result = TcbExtBank.OCB; break;
                case "ocbchcm": result = TcbExtBank.OCBCHCM; break;
                case "obank": case "oceanbank": case "ocean": result = TcbExtBank.OceanBank; break;
                case "pgba": case "pgbank": result = TcbExtBank.PGBANK; break;
                case "publicbankvietnam": result = TcbExtBank.PUBLICBANKVIETNAM; break;
                case "pvbank": case "pvcombank": result = TcbExtBank.PVcomBank; break;
                case "quytdcs": result = TcbExtBank.QuyTDCS; break;
                case "sbank": case "stb": case "sacombank": result = TcbExtBank.Sacombank; break;
                case "sabank": case "sgb": case "saigonbank": result = TcbExtBank.SAIGONBANK; break;
                case "scb": result = TcbExtBank.SCB; break;
                case "scsb": result = TcbExtBank.SCSB; break;
                case "seabank": case "seab": result = TcbExtBank.SeABank; break;
                case "shb": case "shbbank": result = TcbExtBank.SHB; break;
                case "shanbank": case "shinhanbankvn": case "shinhanbankvietnam": case "shbvn": result = TcbExtBank.SHINHANBANKVIETNAM; break;
                case "siamscb": result = TcbExtBank.SIAM_SCB; break;
                case "smbc": result = TcbExtBank.SMBC; break;
                //case "standardcharteredvietnam": result = TcbExtBank.StandardCharteredVietnam; break;
                case "svfc": result = TcbExtBank.SVFC; break;
                case "taipeifubon": result = TcbExtBank.TaipeiFubon; break;
                case "tpbank": case "tpb": case "tp": result = TcbExtBank.TPBank; break;
                case "ubank": result = TcbExtBank.UBANK; break;
                case "umee": result = TcbExtBank.UMEE; break;
                case "uob": result = TcbExtBank.UOB; break;
                case "vbsp": result = TcbExtBank.VBSP; break;
                case "vdb": result = TcbExtBank.VDB; break;
                case "vib": case "vibbank": result = TcbExtBank.VIB; break;
                case "vietabank": result = TcbExtBank.VietABank; break;
                case "vietbank": result = TcbExtBank.Vietbank; break;
                case "vietcomb": case "vcb": case "vietcombank": case "vietcom": result = TcbExtBank.Vietcombank; break;
                //case "viethoanh": result = TcbExtBank.VietHoaNH; break;
                case "vietinba": case "vtb": case "vietinbank": case "vietin": case "icb": result = TcbExtBank.VietinBank; break;
                //case "viettelmoney": result = TcbExtBank.ViettelMoney; break;
                case "vnptmoney": result = TcbExtBank.VNPTMoney; break;
                case "vpbank": case "vpb": result = TcbExtBank.VPBANK; break;
                case "vrb": result = TcbExtBank.VRB; break;
                case "wrbank": case "woo": case "wooribank": result = TcbExtBank.WOORIBANK; break;

                case "techcomb": case "tcb": case "techcombank": case "techcom": result = TcbExtBank.Techcombank_TCB; break;

                default: break;
            }

            return result;
        }

        public static TcbExtBankModel? GetTcbExtBank(this TcbExtBank bank)
        {
            TcbExtBankModel? result = null;

            switch (bank)
            {
                case TcbExtBank._CHINA_ABC_HANOI: result = new TcbExtBankModel() { Bank = bank, ShortName = "(CHINA)ABC - HANOI", NameVN = "Trung Quốc Nông Nghiệp - CN Hà Nội", NameEN = "Agricultural Bank of China Limited - Ha Noi Branch", }; break;
                case TcbExtBank.ABBank: result = new TcbExtBankModel() { Bank = bank, ShortName = "ABBank", NameVN = "Ngân hàng TMCP An Bình", NameEN = "An Binh Commercial Joint Stock Bank", }; break;
                case TcbExtBank.ACB: result = new TcbExtBankModel() { Bank = bank, ShortName = "ACB", NameVN = "Ngân hàng TMCP Á Châu", NameEN = "Asia Commercial Joint Stock Bank", }; break;
                case TcbExtBank.Agribank: result = new TcbExtBankModel() { Bank = bank, ShortName = "Agribank", NameVN = "Ngân hàng Nông nghiệp và Phát triển Nông thôn Việt Nam", NameEN = "Vietnam Bank for Agriculture and Rural Development", }; break;
                case TcbExtBank.ANZBank: result = new TcbExtBankModel() { Bank = bank, ShortName = "ANZ Bank", NameVN = "Ngân hàng TNHH MTV ANZ Việt Nam", NameEN = "ANZ Bank Vietnam Limited", }; break;
                case TcbExtBank.BacABank_NASB: result = new TcbExtBankModel() { Bank = bank, ShortName = "BacABank - NASB", NameVN = "Ngân hàng TMCP Bắc Á", NameEN = "Bac A Commercial Joint Stock Bank", }; break;
                case TcbExtBank.BangkokBank: result = new TcbExtBankModel() { Bank = bank, ShortName = "Bangkok Bank", NameVN = "Ngân Hàng Bangkok Đại Chúng TNHH", NameEN = "Bangkok Bank Public Company Limited", }; break;
                case TcbExtBank.BankOfChina_BOCHKHCMCBranch: result = new TcbExtBankModel() { Bank = bank, ShortName = "Bank Of China - BOCHK HCMC Branch", NameVN = "Ngân hàng Trung Quốc tại Việt Nam - CN TP.HCM", NameEN = "Bank Of China (Hong Kong) Limited - Ho Chi Minh City Branch", }; break;
                case TcbExtBank.BankofIndia_BOI: result = new TcbExtBankModel() { Bank = bank, ShortName = "Bank of India - BOI", NameVN = "Ngân hàng Bank Of India (Việt Nam)", NameEN = "Bank of India (Viet Nam)", }; break;
                case TcbExtBank.BankSinopac: result = new TcbExtBankModel() { Bank = bank, ShortName = "Bank Sinopac", NameVN = "Ngân hàng SinoPac - CN TP.HCM", NameEN = "Bank SinoPac - Ho Chi Minh City Branch", }; break;
                case TcbExtBank.BaoVietBank_BVB: result = new TcbExtBankModel() { Bank = bank, ShortName = "BaoVietBank - BVB", NameVN = "Ngân hàng TMCP Bảo Việt", NameEN = "Bao Viet Joint Stock Commercial Bank", }; break;
                case TcbExtBank.BIDC: result = new TcbExtBankModel() { Bank = bank, ShortName = "BIDC", NameVN = "Ngân hàng Đầu tư và Phát triển Campuchia", NameEN = "Bank for Investment and Development of Cambodia", }; break;
                case TcbExtBank.BIDV: result = new TcbExtBankModel() { Bank = bank, ShortName = "BIDV", NameVN = "Ngân hàng TMCP Đầu tư và Phát triển Việt Nam", NameEN = "Bank for Investment and Development of Vietnam JSC", }; break;
                case TcbExtBank.BNPPHCM: result = new TcbExtBankModel() { Bank = bank, ShortName = "BNPPHCM", NameVN = "Ngân hàng Bnp Paribas – CN TP.HCM", NameEN = "BNP Paribas - Ho Chi Minh City Branch", }; break;
                case TcbExtBank.BNPPHN: result = new TcbExtBankModel() { Bank = bank, ShortName = "BNPPHN", NameVN = "Ngân hàng Bnp Paribas – CN Hà Nội", NameEN = "BNP Paribas - Hanoi Branch", }; break;
                case TcbExtBank.BoCom: result = new TcbExtBankModel() { Bank = bank, ShortName = "BoCom", NameVN = "Ngân hàng Bank of Communications Việt Nam", NameEN = "Bank of Communications Co., Ltd. (Viet Nam)", }; break;
                case TcbExtBank.BPCEIOM: result = new TcbExtBankModel() { Bank = bank, ShortName = "BPCE IOM", NameVN = "Ngân hàng BPCE IOM Việt Nam", NameEN = "BPCE IOM Bank Vietnam", }; break;
                case TcbExtBank.Busan_BNK: result = new TcbExtBankModel() { Bank = bank, ShortName = "Busan - BNK", NameVN = "Ngân hàng BNK Busan", NameEN = "Busan Bank Co., Ltd", }; break;
                case TcbExtBank.BVBank: result = new TcbExtBankModel() { Bank = bank, ShortName = "BVBank", NameVN = "Ngân hàng TMCP Bản Việt", NameEN = "Vietcapital Commercial Joint Stock Bank", }; break;
                case TcbExtBank.BVBankTimo: result = new TcbExtBankModel() { Bank = bank, ShortName = "BVBank Timo", NameVN = "Ngân hàng số Timo by Ban Viet Bank", NameEN = "Timo Digital Bank by Ban Viet Bank", }; break;
                case TcbExtBank.CAKE: result = new TcbExtBankModel() { Bank = bank, ShortName = "CAKE", NameVN = "Ngân hàng TMCP Việt Nam Thịnh Vượng - NH số Cake by VPBank", NameEN = "Vietnam Prosperity Joint Stock Commercial Bank - Cake Digital Bank by VPBank", }; break;
                case TcbExtBank.CathayUnited: result = new TcbExtBankModel() { Bank = bank, ShortName = "Cathay United", NameVN = "Ngân hàng Cathay United", NameEN = "Cathay United Bank", }; break;
                case TcbExtBank.CBBank: result = new TcbExtBankModel() { Bank = bank, ShortName = "CBBank", NameVN = "Ngân hàng Thương mại TNHH MTV Xây dựng Việt Nam", NameEN = "Vietnam Construction Bank", }; break;
                case TcbExtBank.CCB: result = new TcbExtBankModel() { Bank = bank, ShortName = "CCB", NameVN = "Ngân hàng China Construction Bank chi nhánh TP. Hồ Chí Minh", NameEN = "China Construction Bank – Hồ Chí Minh City Branch", }; break;
                case TcbExtBank.CIMB: result = new TcbExtBankModel() { Bank = bank, ShortName = "CIMB", NameVN = "Ngân hàng TNHH MTV CIMB Việt Nam", NameEN = "CIMB Bank (Vietnam) Limited", }; break;
                case TcbExtBank.Citibank: result = new TcbExtBankModel() { Bank = bank, ShortName = "Citibank", NameVN = "Ngân hàng Citibank Việt Nam", NameEN = "Citibank Vietnam", }; break;
                case TcbExtBank.COOPBANK: result = new TcbExtBankModel() { Bank = bank, ShortName = "COOPBANK", NameVN = "Ngân Hàng Hợp tác xã Việt Nam", NameEN = "Co-Operative Bank Of Viet Nam", }; break;
                case TcbExtBank.CTBC: result = new TcbExtBankModel() { Bank = bank, ShortName = "CTBC", NameVN = "Ngân hàng CTBC Việt nam", NameEN = "China Trust Commercial Bank", }; break;
                case TcbExtBank.DaeguBank: result = new TcbExtBankModel() { Bank = bank, ShortName = "Daegu Bank", NameVN = "Ngân hàng Daegu - CN TP.HCM", NameEN = "Daegu Bank – Ho Chi Minh City Branch", }; break;
                case TcbExtBank.DBS_HCM: result = new TcbExtBankModel() { Bank = bank, ShortName = "DBS-HCM", NameVN = "Ngân hàng TNHH MTV Phát triển Singapore", NameEN = "The Development Bank of Singapore Limited", }; break;
                case TcbExtBank.DeutscheBank: result = new TcbExtBankModel() { Bank = bank, ShortName = "Deutsche Bank", NameVN = "Ngân hàng Deutsche Bank AG Việt Nam", NameEN = "Deutsche Bank AG Vietnam", }; break;
                case TcbExtBank.DongABank_DAB: result = new TcbExtBankModel() { Bank = bank, ShortName = "DongA Bank - DAB", NameVN = "Ngân hàng TMCP Đông Á", NameEN = "Dong A Commercial Joint Stock Bank", }; break;
                case TcbExtBank.EsunBank: result = new TcbExtBankModel() { Bank = bank, ShortName = "Esun Bank", NameVN = "Ngân hàng thương mại TNHH E.SUN - CN Đồng Nai", NameEN = "E.SUN Commercial Bank Ltd. – Dong Nai Branch", }; break;
                case TcbExtBank.Eximbank_EIB: result = new TcbExtBankModel() { Bank = bank, ShortName = "Eximbank - EIB", NameVN = "Ngân hàng TMCP Xuất Nhập Khẩu Việt Nam", NameEN = "Vietnam Commercial Joint Stock Export Import Bank", }; break;
                case TcbExtBank.FirstCommercialBank: result = new TcbExtBankModel() { Bank = bank, ShortName = "First Commercial Bank", NameVN = "Ngân hàng First Commercial Bank", NameEN = "First Commercial Bank", }; break;
                case TcbExtBank.GPBank: result = new TcbExtBankModel() { Bank = bank, ShortName = "GPBank", NameVN = "Ngân hàng TMCP Dầu Khí Toàn Cầu", NameEN = "Global Petro Joint Stock Commercial Bank", }; break;
                case TcbExtBank.HDBank: result = new TcbExtBankModel() { Bank = bank, ShortName = "HDBank", NameVN = "Ngân hàng TMCP Phát triển TP. Hồ Chí Minh", NameEN = "Ho Chi Minh City Development Joint Stock Commercial Bank", }; break;
                case TcbExtBank.HONGLEONG_HLBVN: result = new TcbExtBankModel() { Bank = bank, ShortName = "HONG LEONG - HLBVN", NameVN = "Ngân hàng TNHH MTV Hong Leong Việt Nam", NameEN = "Hong Leong Bank Vietnam Limited", }; break;
                case TcbExtBank.HSBCVietnam: result = new TcbExtBankModel() { Bank = bank, ShortName = "HSBC Vietnam", NameVN = "Ngân hàng TNHH một thành viên HSBC (Việt Nam)", NameEN = "HSBC Bank Vietnam Limited", }; break;
                case TcbExtBank.HuaNanBank: result = new TcbExtBankModel() { Bank = bank, ShortName = "HuaNan Bank", NameVN = "Ngân hàng Hua Nan Việt Nam", NameEN = "Hua Nan Commercial Bank, Ltd", }; break;
                case TcbExtBank.IBK_HCM: result = new TcbExtBankModel() { Bank = bank, ShortName = "IBK-HCM", NameVN = "Ngân hàng Công Nghiệp Hàn Quốc - CN TP.HCM", NameEN = "Industrial Bank of Korea - Ho Chi Minh City Branch", }; break;
                case TcbExtBank.IBK_HN: result = new TcbExtBankModel() { Bank = bank, ShortName = "IBK-HN", NameVN = "Ngân hàng Công Nghiệp Hàn Quốc - CN Hà Nội", NameEN = "Industrial Bank of Korea - Hanoi Branch", }; break;
                case TcbExtBank.ICBCHanoi: result = new TcbExtBankModel() { Bank = bank, ShortName = "ICBC Hanoi", NameVN = "Ngân hàng Công Thương Trung Quốc - CN Hà Nội", NameEN = "Industrial and Commercial Bank of China Limited, Hanoi Branch", }; break;
                case TcbExtBank.IVB: result = new TcbExtBankModel() { Bank = bank, ShortName = "IVB", NameVN = "Ngân hàng TNHH Indovina", NameEN = "Indovina Bank Ltd.", }; break;
                case TcbExtBank.J_P_Morgan: result = new TcbExtBankModel() { Bank = bank, ShortName = "J.P. Morgan", NameVN = "Ngân hàng JP Morgan Chase", NameEN = "JP Morgan Chase Bank & Co.", }; break;
                case TcbExtBank.KBANK: result = new TcbExtBankModel() { Bank = bank, ShortName = "KBANK", NameVN = "Ngân hàng Đại chúng TNHH KASIKORNBANK - CN TP.HCM", NameEN = "KASIKORNBANK Public Company Limited – Ho Chi Minh City Branch", }; break;
                case TcbExtBank.KBHCM: result = new TcbExtBankModel() { Bank = bank, ShortName = "KBHCM", NameVN = "Ngân hàng Kookmin – CN TP.HCM", NameEN = "Kookmin Bank – Ho Chi Minh City Branch", }; break;
                case TcbExtBank.KBHN: result = new TcbExtBankModel() { Bank = bank, ShortName = "KBHN", NameVN = "Ngân hàng Kookmin – CN Hà Nội", NameEN = "Kookmin Bank - Hanoi Branch", }; break;
                case TcbExtBank.KEBHANAHCM: result = new TcbExtBankModel() { Bank = bank, ShortName = "KEBHANAHCM", NameVN = "Ngân hàng KEB Hana Bank Hồ Chí Minh", NameEN = "KEB Hana Bank - Ho Chi Minh Branch", }; break;
                case TcbExtBank.KEBHANAHN: result = new TcbExtBankModel() { Bank = bank, ShortName = "KEBHANAHN", NameVN = "Ngân hàng KEB Hana Bank Hà Nội", NameEN = "KEB Hana Bank - Hanoi Branch", }; break;
                case TcbExtBank.KhobacNhanuoc_KBNN: result = new TcbExtBankModel() { Bank = bank, ShortName = "Kho bac Nha nuoc - KBNN", NameVN = "Kho bạc Nhà nước", NameEN = "Vietnam State Treasury", }; break;
                case TcbExtBank.Kienlongbank_KLB: result = new TcbExtBankModel() { Bank = bank, ShortName = "Kienlongbank - KLB", NameVN = "Ngân hàng TMCP Kiên Long", NameEN = "Kien Long Commercial Joint - Stock Bank", }; break;
                case TcbExtBank.Liobank: result = new TcbExtBankModel() { Bank = bank, ShortName = "Liobank", NameVN = "Ngân hàng số Liobank by OCB", NameEN = "Liobank - Digital bank by OCB", }; break;
                case TcbExtBank.LPBANK: result = new TcbExtBankModel() { Bank = bank, ShortName = "LPBANK", NameVN = "Ngân hàng TMCP Bưu điện Liên Việt", NameEN = "LienViet Post Joint Stock Commercial Bank", }; break;
                case TcbExtBank.MAFC: result = new TcbExtBankModel() { Bank = bank, ShortName = "MAFC", NameVN = "Công ty Tài chính TNHH MTV Mirae Asset (Việt Nam) Limited", NameEN = "Mirae Asset Finance Company (Vietnam)", }; break;
                case TcbExtBank.Maybank: result = new TcbExtBankModel() { Bank = bank, ShortName = "Maybank", NameVN = "Ngân hàng Malayan Banking Berhad", NameEN = "Malayan Banking Berhad", }; break;
                case TcbExtBank.MB: result = new TcbExtBankModel() { Bank = bank, ShortName = "MB", NameVN = "Ngân hàng TMCP Quân Đội", NameEN = "Military Commercial Joint Stock Bank", }; break;
                case TcbExtBank.MegaICBC: result = new TcbExtBankModel() { Bank = bank, ShortName = "Mega ICBC", NameVN = "Ngân hàng Mega ICBC - CN TP.HCM", NameEN = "Mega International Commercial Bank Co., Ltd.- Ho Chi Minh City Branch", }; break;
                case TcbExtBank.MHB: result = new TcbExtBankModel() { Bank = bank, ShortName = "MHB", NameVN = "Ngân hàng TMCP Phát triển Nhà Đồng bằng sông Cửu Long", NameEN = "Housing Bank Of Mekong Delta", }; break;
                case TcbExtBank.Mizuho: result = new TcbExtBankModel() { Bank = bank, ShortName = "Mizuho", NameVN = "Ngân hàng Mizuho", NameEN = "Mizuho Bank, LTd", }; break;
                case TcbExtBank.MSB: result = new TcbExtBankModel() { Bank = bank, ShortName = "MSB", NameVN = "Ngân hàng TMCP Hàng Hải Việt Nam", NameEN = "Vietnam Maritime Commercial Join Stock Bank", }; break;
                case TcbExtBank.MUFGBank: result = new TcbExtBankModel() { Bank = bank, ShortName = "MUFG Bank", NameVN = "Ngân hàng MUFG Bank,Ltd", NameEN = "MUFG Bank,Ltd", }; break;
                case TcbExtBank.NAB: result = new TcbExtBankModel() { Bank = bank, ShortName = "NAB", NameVN = "Ngân hàng TMCP Nam Á", NameEN = "Nam A Comercial Join Stock Bank", }; break;
                case TcbExtBank.NCB: result = new TcbExtBankModel() { Bank = bank, ShortName = "NCB", NameVN = "Ngân hàng TMCP Quốc Dân", NameEN = "National Citizen Commercial Joint Stock Bank", }; break;
                case TcbExtBank.NonghuypBank: result = new TcbExtBankModel() { Bank = bank, ShortName = "NonghuypBank", NameVN = "Ngân hàng Nonghyup - CN Hà Nội", NameEN = "Nonghyup Bank - Hanoi Branch", }; break;
                case TcbExtBank.OCB: result = new TcbExtBankModel() { Bank = bank, ShortName = "OCB", NameVN = "Ngân hàng TMCP Phương Đông", NameEN = "Orient Commercial Joint Stock Bank", }; break;
                case TcbExtBank.OCBCHCM: result = new TcbExtBankModel() { Bank = bank, ShortName = "OCBC HCM", NameVN = "Ngân hàng Oversea-Chinese Banking Corporation LTD – CN TP.HCM", NameEN = "Oversea-Chinese Banking Corporation LTD – Ho Chi Minh City Branch", }; break;
                case TcbExtBank.OceanBank: result = new TcbExtBankModel() { Bank = bank, ShortName = "OceanBank", NameVN = "Ngân hàng TMCP Đại Dương", NameEN = "Ocean Commercial One Member Limited Liability Bank", }; break;
                case TcbExtBank.PGBANK: result = new TcbExtBankModel() { Bank = bank, ShortName = "PGBANK", NameVN = "Ngân hàng TMCP Thịnh vượng và Phát triển", NameEN = "Prosperity and Growth Commercial Joint Stock Bank", }; break;
                case TcbExtBank.PUBLICBANKVIETNAM: result = new TcbExtBankModel() { Bank = bank, ShortName = "PUBLIC BANK VIETNAM", NameVN = "Ngân hàng TNHH MTV Public Việt Nam", NameEN = "Public Bank Vietnam Limited", }; break;
                case TcbExtBank.PVcomBank: result = new TcbExtBankModel() { Bank = bank, ShortName = "PVcomBank", NameVN = "Ngân hàng TMCP Đại chúng Việt Nam", NameEN = "Vietnam Public Joint Stock Commercial Bank", }; break;
                case TcbExtBank.QuyTDCS: result = new TcbExtBankModel() { Bank = bank, ShortName = "Quy TDCS", NameVN = "Ngân hàng Quỹ TDCS", NameEN = "Policy Credit Fund Bank", }; break;
                case TcbExtBank.Sacombank: result = new TcbExtBankModel() { Bank = bank, ShortName = "Sacombank", NameVN = "Ngân hàng TMCP Sài Gòn Thương Tín", NameEN = "Sai Gon Thuong Tin Commercial Joint Stock Bank", }; break;
                case TcbExtBank.SAIGONBANK: result = new TcbExtBankModel() { Bank = bank, ShortName = "SAIGONBANK", NameVN = "Ngân hàng TMCP Sài Gòn Công Thương", NameEN = "Sai Gon Joint Stock Commercial Bank", }; break;
                case TcbExtBank.SCB: result = new TcbExtBankModel() { Bank = bank, ShortName = "SCB", NameVN = "Ngân hàng TMCP Sài Gòn", NameEN = "Sai Gon Joint Stock Commercial Bank", }; break;
                case TcbExtBank.SCSB: result = new TcbExtBankModel() { Bank = bank, ShortName = "SCSB", NameVN = "Ngân hàng The Shanghai Commercial & Savings Bank, Ltd.", NameEN = "The Shanghai Commercial & Savings Bank, Ltd.", }; break;
                case TcbExtBank.SeABank: result = new TcbExtBankModel() { Bank = bank, ShortName = "SeABank", NameVN = "Ngân Hàng TMCP Đông Nam Á", NameEN = "Southeast Asia Commercial Joint Stock Bank", }; break;
                case TcbExtBank.SHB: result = new TcbExtBankModel() { Bank = bank, ShortName = "SHB", NameVN = "Ngân hàng TMCP Sài Gòn-Hà Nội", NameEN = "Saigon Hanoi Commercial Joint Stock Bank", }; break;
                case TcbExtBank.SHINHANBANKVIETNAM: result = new TcbExtBankModel() { Bank = bank, ShortName = "SHINHAN BANK VIETNAM", NameVN = "Ngân hàng TNHH MTV Shinhan Việt Nam", NameEN = "Shinhan Bank Vietnam Limited", }; break;
                case TcbExtBank.SIAM_SCB: result = new TcbExtBankModel() { Bank = bank, ShortName = "SIAM - SCB", NameVN = "Ngân hàng The Siam Commercial Bank Public Company Limited - CN TP.HCM", NameEN = "The Siam Commercial Bank Public Company Limited – Ho Chi Minh City Branch", }; break;
                case TcbExtBank.SMBC: result = new TcbExtBankModel() { Bank = bank, ShortName = "SMBC", NameVN = "Ngân hàng Sumitomo Mitsui Banking Corporation", NameEN = "Sumitomo Mitsui Banking Corporation", }; break;
                case TcbExtBank.StandardCharteredVietnam: result = new TcbExtBankModel() { Bank = bank, ShortName = "Standard Chartered Vietnam", NameVN = "Ngân hàng TNHH MTV Standard Chartered (Việt Nam)", NameEN = "Standard Chartered Bank Vietnam Limited", }; break;
                case TcbExtBank.SVFC: result = new TcbExtBankModel() { Bank = bank, ShortName = "SVFC", NameVN = "Công ty Tài chính TNHH MTV Shinhan Việt Nam", NameEN = "Shinhan Finance Vietnam", }; break;
                case TcbExtBank.TaipeiFubon: result = new TcbExtBankModel() { Bank = bank, ShortName = "Taipei Fubon", NameVN = "Ngân hàng Thương Mại Taipei Fubon", NameEN = "Taipei Fubon Commercial Bank Co.,ltd", }; break;
                case TcbExtBank.TPBank: result = new TcbExtBankModel() { Bank = bank, ShortName = "TPBank", NameVN = "Ngân hàng TMCP Tiên Phong", NameEN = "Tien Phong Commercial Joint Stock Bank", }; break;
                case TcbExtBank.UBANK: result = new TcbExtBankModel() { Bank = bank, ShortName = "UBANK", NameVN = "Ngân hàng TMCP Việt Nam Thịnh Vượng - NH số Ubank by VPBank", NameEN = "Vietnam Prosperity Joint Stock Commercial Bank - Ubank Digital Bank by VPBank", }; break;
                case TcbExtBank.UMEE: result = new TcbExtBankModel() { Bank = bank, ShortName = "UMEE", NameVN = "NH số UMEE by Kienlongbank", NameEN = "UMEE Digital Bank by Kienlongbank", }; break;
                case TcbExtBank.UOB: result = new TcbExtBankModel() { Bank = bank, ShortName = "UOB", NameVN = "Ngân hàng TNHH MTV United Overseas Bank (Việt Nam)", NameEN = "United Overseas Bank Limited (Vietnam)", }; break;
                case TcbExtBank.VBSP: result = new TcbExtBankModel() { Bank = bank, ShortName = "VBSP", NameVN = "Ngân hàng Chính sách xã hội", NameEN = "Vietnam Bank for Social Policies", }; break;
                case TcbExtBank.VDB: result = new TcbExtBankModel() { Bank = bank, ShortName = "VDB", NameVN = "Ngân hàng Phát triển Việt Nam", NameEN = "Vietnam Development Bank", }; break;
                case TcbExtBank.VIB: result = new TcbExtBankModel() { Bank = bank, ShortName = "VIB", NameVN = "Ngân hàng TMCP Quốc tế Việt Nam", NameEN = "Vietnam International Commercial Joint Stock Bank", }; break;
                case TcbExtBank.VietABank: result = new TcbExtBankModel() { Bank = bank, ShortName = "VietABank", NameVN = "Ngân hàng TMCP Việt Á", NameEN = "Viet A Commercial Joint Stock Bank", }; break;
                case TcbExtBank.Vietbank: result = new TcbExtBankModel() { Bank = bank, ShortName = "Vietbank", NameVN = "Ngân hàng TMCP Việt Nam Thương Tín", NameEN = "Vietnam Thuong Tin Commercial Joint Stock Bank", }; break;
                case TcbExtBank.Vietcombank: result = new TcbExtBankModel() { Bank = bank, ShortName = "Vietcombank", NameVN = "Ngân hàng TMCP Ngoại thương Việt Nam", NameEN = "Joint Stock Commercial Bank for Foreign Trade of Vietnam", }; break;
                case TcbExtBank.VietHoaNH: result = new TcbExtBankModel() { Bank = bank, ShortName = "Viet Hoa NH", NameVN = "Ngân hàng TMCP Việt Hoa", NameEN = "Viet Hoa Bank", }; break;
                case TcbExtBank.VietinBank: result = new TcbExtBankModel() { Bank = bank, ShortName = "VietinBank", NameVN = "Ngân hàng TMCP Công thương Việt Nam", NameEN = "Vietnam Joint Stock Commercial Bank For Industry And Trade", }; break;
                case TcbExtBank.ViettelMoney: result = new TcbExtBankModel() { Bank = bank, ShortName = "Viettel Money", NameVN = "Viettel Money", NameEN = "Viettel Money", }; break;
                case TcbExtBank.VNPTMoney: result = new TcbExtBankModel() { Bank = bank, ShortName = "VNPT Money", NameVN = "Trung tâm dịch vụ tài chính số VNPT - Chi nhánh tổng công ty truyền thông", NameEN = "VNPT Digital Financial Service Center", }; break;
                case TcbExtBank.VPBANK: result = new TcbExtBankModel() { Bank = bank, ShortName = "VPBANK", NameVN = "Ngân hàng TMCP Việt Nam Thịnh Vượng", NameEN = "Vietnam Prosperity Joint Stock Commercial Bank", }; break;
                case TcbExtBank.VRB: result = new TcbExtBankModel() { Bank = bank, ShortName = "VRB", NameVN = "Ngân hàng Liên doanh Việt - Nga", NameEN = "Vietnam - Russia Joint Venture Bank", }; break;
                case TcbExtBank.WOORIBANK: result = new TcbExtBankModel() { Bank = bank, ShortName = "WOORI BANK VIETNAM", NameVN = "Ngân hàng TNHH MTV Woori Việt Nam", NameEN = "Woori Bank Vietnam Limited", }; break;
                default: break;
            }
            return result;
        }

        public static string? GetTcbShortName(this TcbExtBank bank)
        {
            return bank.GetTcbExtBank()?.ShortName;
        }

        public static string TcbTranslate(this TcbText txt, Lang lang)
        {
            string result = string.Empty;
            switch (txt)
            {
                case TcbText.NotRegistered:
                    result = lang == Lang.English ? "Let's connect" : "Kết nối ngay nào";
                    break;
                case TcbText.Message:
                    result = lang == Lang.English ? "Message" : "Lời nhắn";
                    break;
                case TcbText.TransactionId:
                    result = lang == Lang.English ? "Transaction ID" : "Mã giao dịch";
                    break;
                case TcbText.ServiceUnavailable:
                    result = lang == Lang.English ? "The service is temporarily unavailable. Please try again later." : "Dịch vụ tạm thời gián đoạn. Quý khách vui lòng thử lại sau.";
                    break;

                default: break;
            }
            return result;
        }
        #endregion

        #region ACB
        public static AcbExtBank ToAcbExtBank(this string bank)
        {
            var result = AcbExtBank.Unknown;
            switch (bank.Trim().ToLower())
            {
                case "vietcomb": case "vcb": case "vietcombank": case "vietcom": result = AcbExtBank.VCB_970436; break;
                case "bidv": case "bidvbank": result = AcbExtBank.BIDV_970418; break;
                case "vietbank": result = AcbExtBank.VIETBANK_970433; break;
                case "techcomb": case "tcb": case "techcombank": case "techcom": result = AcbExtBank.TCB_970407; break;
                case "sbank": case "stb": case "sacombank": result = AcbExtBank.STB_970403; break;
                case "vpbank": case "vpb": result = AcbExtBank.VPB_970432; break;
                case "exim": case "eib": case "eximbankeib": case "eximbank": result = AcbExtBank.EIB_970431; break;
                case "abb": case "abbank": result = AcbExtBank.ABB_970425; break;
                case "agribank": result = AcbExtBank.AGRIBANK_970405; break;
                case "bab": case "bacabank": case "bacabanknasb": result = AcbExtBank.BAB_970409; break;
                case "bnpparibashanoi": result = AcbExtBank.BNPParibasHaNoi_963668; break;
                case "bnpparibashochiminh": result = AcbExtBank.BNPParibasHoChiMinh_963666; break;
                case "bvb": case "baovietbank": case "baovietbankbvb": result = AcbExtBank.BVB_970438; break;
                case "bvbank": result = AcbExtBank.BVBank_970454; break;
                case "bvbanktimo": result = AcbExtBank.BVBankTimo_963388; break;
                case "cbb": result = AcbExtBank.CBB_970444; break;
                case "cimbvnd": case "cimb": case "cimbbank": result = AcbExtBank.CIMB_422589; break;
                case "citivnvn": result = AcbExtBank.CITIVNVN_533948; break;
                case "coopbank": result = AcbExtBank.COOPBANK_970446; break;
                case "cathayunitedbank": result = AcbExtBank.CathayUnitedBank_168999; break;
                case "congtytaichinh": result = AcbExtBank.CongtyTaichinh_963368; break;
                case "dbs": result = AcbExtBank.DBS_796500; break;
                case "dob": result = AcbExtBank.DOB_970406; break;
                case "dvc": result = AcbExtBank.DVC_971100; break;
                case "gpb": result = AcbExtBank.GPB_970408; break;
                case "hdbank": case "hdb": result = AcbExtBank.HDB_970437; break;
                case "hlbank": case "hlb": case "hongleongbank": case "hongleong": result = AcbExtBank.HLB_970442; break;
                case "hsbcbank": case "hsbc": result = AcbExtBank.HSBC_458761; break;
                case "ibkhcm": result = AcbExtBank.IBKHCM_970456; break;
                case "ibkhanoi": result = AcbExtBank.IBKHaNoi_970455; break;
                case "vietinba": case "vtb": case "vietinbank": case "vietin": case "icb": result = AcbExtBank.ICB_970415; break;
                case "indobank": case "ivb": result = AcbExtBank.IVB_970434; break;
                case "kbhcm": result = AcbExtBank.KBHCM_970463; break;
                case "kbhn": result = AcbExtBank.KBHN_970462; break;
                case "kebhanahanoi": result = AcbExtBank.KEBHanaHaNoi_970467; break;
                case "kebhanahochiminh": result = AcbExtBank.KEBHanaHoChiMinh_970466; break;
                case "klbank": case "kienlongbank": case "kienlongbankklb": case "klb": case "kienlong": result = AcbExtBank.KLB_970452; break;
                case "ksk": result = AcbExtBank.KSK_668888; break;
                case "lvpbank": case "lienvietbank": case "lpb": case "lpbank": result = AcbExtBank.LPBank_970449; break;
                case "mafc": result = AcbExtBank.MAFC_977777; break;
                case "mbbank": case "mb": result = AcbExtBank.MB_970422; break;
                case "maribank": case "msb": case "msbbank": result = AcbExtBank.MSB_970426; break;
                case "nab": result = AcbExtBank.NAB_970428; break;
                case "ncb": result = AcbExtBank.NCB_970419; break;
                case "nhbank": result = AcbExtBank.NHBank_963688; break;
                case "liobank": result = AcbExtBank.NHTMCP_963369; break;
                case "nonghyup": result = AcbExtBank.NONGHYUP_801011; break;
                case "nganhangdau": result = AcbExtBank.NganhangDau_555666; break;
                case "ocbank": case "ocb": case "ocbbank": result = AcbExtBank.OCB_970448; break;
                case "obank": case "oceanbank": case "ocean": result = AcbExtBank.Oceanbank_970414; break;
                case "pbvn": result = AcbExtBank.PBVN_970439; break;
                case "pgba": case "pgbank": result = AcbExtBank.PGBank_970430; break;
                case "pvcb": result = AcbExtBank.PVCB_970412; break;
                case "pvbank": case "pvcombank": result = AcbExtBank.PVCombank_971122; break;
                case "scbsaigon": result = AcbExtBank.SCBSaiGon_970429; break;
                case "seabank": case "seab": result = AcbExtBank.SEAB_970440; break;
                case "sabank": case "sgb": case "saigonbank": result = AcbExtBank.SGB_970400; break;
                case "shb": case "shbbank": result = AcbExtBank.SHB_970443; break;
                case "shanbank": case "shinhanbankvn": case "shinhanbankvietnam": case "shbvn": result = AcbExtBank.SHBVN_970424; break;
                case "cake": result = AcbExtBank.TMCPCAKE_546034; break;
                case "ubank": result = AcbExtBank.TMCPUbank_546035; break;
                case "scb": result = AcbExtBank.TNHH_970410; break;
                case "tpbank": case "tpb": case "tp": result = AcbExtBank.TPB_970423; break;
                case "uob": result = AcbExtBank.UOB_970458; break;
                case "vabank": case "vab": result = AcbExtBank.VAB_970427; break;
                case "vbsp": result = AcbExtBank.VBSP_999888; break;
                case "vib": case "vibbank": result = AcbExtBank.VIB_970441; break;
                case "vnpt": result = AcbExtBank.VNPT_971011; break;
                case "vrb": result = AcbExtBank.VRB_970421; break;
                case "viettel": result = AcbExtBank.Viettel_971005; break;
                case "vikki": result = AcbExtBank.Vikki_963311; break;
                case "wrbank": case "woo": case "wooribank": result = AcbExtBank.WOO_970457; break;

                case "acb": case "acbbank": result = AcbExtBank.ACB; break;
                default: break;
            }
            return result;
        }

        public static AcbExtBankModel? GetAcbExtBank(this AcbExtBank bank)
        {
            AcbExtBankModel? result = null;
            switch (bank)
            {
                case AcbExtBank.VCB_970436: result = new AcbExtBankModel() { Bank = bank, ShortName = "970436", NameVN = "", NameEN = "VCB - NH TMCP Ngoai Thuong Viet Nam", }; break;
                case AcbExtBank.BIDV_970418: result = new AcbExtBankModel() { Bank = bank, ShortName = "970418", NameVN = "", NameEN = "BIDV - NH TMCP Dau tu va Phat trien Viet Nam", }; break;
                case AcbExtBank.VIETBANK_970433: result = new AcbExtBankModel() { Bank = bank, ShortName = "970433", NameVN = "", NameEN = "VIETBANK - NH TMCP Viet Nam Thuong Tin", }; break;
                case AcbExtBank.TCB_970407: result = new AcbExtBankModel() { Bank = bank, ShortName = "970407", NameVN = "", NameEN = "TCB - NH TMCP Ky thuong Viet Nam", }; break;
                case AcbExtBank.STB_970403: result = new AcbExtBankModel() { Bank = bank, ShortName = "970403", NameVN = "", NameEN = "STB - NH TMCP Sai Gon Thuong Tin", }; break;
                case AcbExtBank.VPB_970432: result = new AcbExtBankModel() { Bank = bank, ShortName = "970432", NameVN = "", NameEN = "VPB - NH TMCP Viet Nam Thinh Vuong", }; break;
                case AcbExtBank.EIB_970431: result = new AcbExtBankModel() { Bank = bank, ShortName = "970431", NameVN = "", NameEN = "EIB - NH TMCP Xuat nhap khau Viet Nam", }; break;
                case AcbExtBank.ABB_970425: result = new AcbExtBankModel() { Bank = bank, ShortName = "970425", NameVN = "", NameEN = "ABB - NH TMCP An Binh", }; break;
                case AcbExtBank.AGRIBANK_970405: result = new AcbExtBankModel() { Bank = bank, ShortName = "970405", NameVN = "", NameEN = "AGRIBANK - NH NN Va PTNT Viet Nam", }; break;
                case AcbExtBank.BAB_970409: result = new AcbExtBankModel() { Bank = bank, ShortName = "970409", NameVN = "", NameEN = "BAB - NH TMCP Bac A", }; break;
                case AcbExtBank.BNPParibasHaNoi_963668: result = new AcbExtBankModel() { Bank = bank, ShortName = "963668", NameVN = "", NameEN = "BNP Paribas - Chi nhanh Ha Noi", }; break;
                case AcbExtBank.BNPParibasHoChiMinh_963666: result = new AcbExtBankModel() { Bank = bank, ShortName = "963666", NameVN = "", NameEN = "BNP Paribas - Chi nhanh TP. Ho Chi Minh", }; break;
                case AcbExtBank.BVB_970438: result = new AcbExtBankModel() { Bank = bank, ShortName = "970438", NameVN = "", NameEN = "BVB - NH TMCP Bao Viet", }; break;
                case AcbExtBank.BVBank_970454: result = new AcbExtBankModel() { Bank = bank, ShortName = "970454", NameVN = "", NameEN = "BVBank - NH TMCP Ban Viet", }; break;
                case AcbExtBank.BVBankTimo_963388: result = new AcbExtBankModel() { Bank = bank, ShortName = "963388", NameVN = "", NameEN = "BVBank Timo - NH TMCP Ban Viet", }; break;
                case AcbExtBank.CBB_970444: result = new AcbExtBankModel() { Bank = bank, ShortName = "970444", NameVN = "", NameEN = "CBB - NH TM TNHH MTV Xay Dung Viet Nam", }; break;
                case AcbExtBank.CIMB_422589: result = new AcbExtBankModel() { Bank = bank, ShortName = "422589", NameVN = "", NameEN = "CIMB - NH TNHH MTV CIMB", }; break;
                case AcbExtBank.CITIVNVN_533948: result = new AcbExtBankModel() { Bank = bank, ShortName = "533948", NameVN = "", NameEN = "CITIVNVN - Citibank Viet Nam", }; break;
                case AcbExtBank.COOPBANK_970446: result = new AcbExtBankModel() { Bank = bank, ShortName = "970446", NameVN = "", NameEN = "COOPBANK - NH Hop tac xa Viet Nam", }; break;
                case AcbExtBank.CathayUnitedBank_168999: result = new AcbExtBankModel() { Bank = bank, ShortName = "168999", NameVN = "", NameEN = "Cathay United Bank – Chi nhanh Ho Chi Minh ", }; break;
                case AcbExtBank.CongtyTaichinh_963368: result = new AcbExtBankModel() { Bank = bank, ShortName = "963368", NameVN = "", NameEN = "Cong ty Tai chinh TNHH MTV Shinhan Viet Nam", }; break;
                case AcbExtBank.DBS_796500: result = new AcbExtBankModel() { Bank = bank, ShortName = "796500", NameVN = "", NameEN = "DBS - NH DBS chi nhanh HCM", }; break;
                case AcbExtBank.DOB_970406: result = new AcbExtBankModel() { Bank = bank, ShortName = "970406", NameVN = "", NameEN = "DOB - NH TMCP Dong A", }; break;
                case AcbExtBank.DVC_971100: result = new AcbExtBankModel() { Bank = bank, ShortName = "971100", NameVN = "", NameEN = "DVC", }; break;
                case AcbExtBank.GPB_970408: result = new AcbExtBankModel() { Bank = bank, ShortName = "970408", NameVN = "", NameEN = "GPB - NH TM TNHH MTV Dau Khi Toan Cau", }; break;
                case AcbExtBank.HDB_970437: result = new AcbExtBankModel() { Bank = bank, ShortName = "970437", NameVN = "", NameEN = "HDB - NH TMCP Phat Trien Thanh Pho Ho Chi Minh", }; break;
                case AcbExtBank.HLB_970442: result = new AcbExtBankModel() { Bank = bank, ShortName = "970442", NameVN = "", NameEN = "HLB - NH TNHH MTV Hongleong Viet Nam", }; break;
                case AcbExtBank.HSBC_458761: result = new AcbExtBankModel() { Bank = bank, ShortName = "458761", NameVN = "", NameEN = "HSBC - Ngan hang TNHH MTV HSBC (Viet Nam)", }; break;
                case AcbExtBank.IBKHCM_970456: result = new AcbExtBankModel() { Bank = bank, ShortName = "970456", NameVN = "", NameEN = "IBK - NH IBK - chi nhanh HCM", }; break;
                case AcbExtBank.IBKHaNoi_970455: result = new AcbExtBankModel() { Bank = bank, ShortName = "970455", NameVN = "", NameEN = "IBK - NH IBK - chi nhanh Ha Noi", }; break;
                case AcbExtBank.ICB_970415: result = new AcbExtBankModel() { Bank = bank, ShortName = "970415", NameVN = "", NameEN = "ICB - NH TMCP Cong Thuong Viet Nam", }; break;
                case AcbExtBank.IVB_970434: result = new AcbExtBankModel() { Bank = bank, ShortName = "970434", NameVN = "", NameEN = "IVB - NH TNHH Indovina", }; break;
                case AcbExtBank.KBHCM_970463: result = new AcbExtBankModel() { Bank = bank, ShortName = "970463", NameVN = "", NameEN = "KBHCM - Kookmin Chi nhanh Thanh pho Ho Chi Minh", }; break;
                case AcbExtBank.KBHN_970462: result = new AcbExtBankModel() { Bank = bank, ShortName = "970462", NameVN = "", NameEN = "KBHN - Kookmin Chi nhanh Ha Noi", }; break;
                case AcbExtBank.KEBHanaHaNoi_970467: result = new AcbExtBankModel() { Bank = bank, ShortName = "970467", NameVN = "", NameEN = "KEB Hana - Chi nhanh Ha Noi", }; break;
                case AcbExtBank.KEBHanaHoChiMinh_970466: result = new AcbExtBankModel() { Bank = bank, ShortName = "970466", NameVN = "", NameEN = "KEB Hana - Chi nhanh Thanh pho Ho Chi Minh", }; break;
                case AcbExtBank.KLB_970452: result = new AcbExtBankModel() { Bank = bank, ShortName = "970452", NameVN = "", NameEN = "KLB - NH TMCP Kien Long", }; break;
                case AcbExtBank.KSK_668888: result = new AcbExtBankModel() { Bank = bank, ShortName = "668888", NameVN = "", NameEN = "KSK - Dai chung TNHH Kasikornbank - Chi nhanh TP. HCM", }; break;
                case AcbExtBank.LPBank_970449: result = new AcbExtBankModel() { Bank = bank, ShortName = "970449", NameVN = "", NameEN = "LPBank - NH TMCP Buu Dien Lien Viet", }; break;
                case AcbExtBank.MAFC_977777: result = new AcbExtBankModel() { Bank = bank, ShortName = "977777", NameVN = "", NameEN = "MAFC - CTY Tai chinh TNHH MTV Mirae Asset (Viet Nam)", }; break;
                case AcbExtBank.MB_970422: result = new AcbExtBankModel() { Bank = bank, ShortName = "970422", NameVN = "", NameEN = "MB - NH TMCP Quan Doi", }; break;
                case AcbExtBank.MSB_970426: result = new AcbExtBankModel() { Bank = bank, ShortName = "970426", NameVN = "", NameEN = "MSB - NH TMCP Hang Hai Viet Nam", }; break;
                case AcbExtBank.NAB_970428: result = new AcbExtBankModel() { Bank = bank, ShortName = "970428", NameVN = "", NameEN = "NAB - NH TMCP Nam A", }; break;
                case AcbExtBank.NCB_970419: result = new AcbExtBankModel() { Bank = bank, ShortName = "970419", NameVN = "", NameEN = "NCB - NH TMCP Quoc Dan", }; break;
                case AcbExtBank.NHBank_963688: result = new AcbExtBankModel() { Bank = bank, ShortName = "963688", NameVN = "", NameEN = "NH Bank of China (Hongkong) Limited – CN Ho Chi Minh", }; break;
                case AcbExtBank.NHTMCP_963369: result = new AcbExtBankModel() { Bank = bank, ShortName = "963369", NameVN = "", NameEN = "NH TMCP Phuong Dong (Liobank)", }; break;
                case AcbExtBank.NONGHYUP_801011: result = new AcbExtBankModel() { Bank = bank, ShortName = "801011", NameVN = "", NameEN = "NONGHYUP - Chi nhanh HN", }; break;
                case AcbExtBank.NganhangDau_555666: result = new AcbExtBankModel() { Bank = bank, ShortName = "555666", NameVN = "", NameEN = "Ngan hang Dau tu va Phat trien Campuchia – Chi nhanh Ha Noi", }; break;
                case AcbExtBank.OCB_970448: result = new AcbExtBankModel() { Bank = bank, ShortName = "970448", NameVN = "", NameEN = "OCB - NH TMCP Phuong Dong", }; break;
                case AcbExtBank.Oceanbank_970414: result = new AcbExtBankModel() { Bank = bank, ShortName = "970414", NameVN = "", NameEN = "Oceanbank - NH TMCP Dai Duong", }; break;
                case AcbExtBank.PBVN_970439: result = new AcbExtBankModel() { Bank = bank, ShortName = "970439", NameVN = "", NameEN = "PBVN - NH TNHH MTV Public Viet Nam", }; break;
                case AcbExtBank.PGBank_970430: result = new AcbExtBankModel() { Bank = bank, ShortName = "970430", NameVN = "", NameEN = "PG Bank - NH TMCP Thinh Vuong va Phat trien", }; break;
                case AcbExtBank.PVCB_970412: result = new AcbExtBankModel() { Bank = bank, ShortName = "970412", NameVN = "", NameEN = "PVCB - NH TMCP Dai Chung Viet Nam", }; break;
                case AcbExtBank.PVCombank_971122: result = new AcbExtBankModel() { Bank = bank, ShortName = "971122", NameVN = "", NameEN = "PVCombank-NAPAS", }; break;
                case AcbExtBank.SCBSaiGon_970429: result = new AcbExtBankModel() { Bank = bank, ShortName = "970429", NameVN = "", NameEN = "SCB - NH TMCP Sai Gon", }; break;
                case AcbExtBank.SEAB_970440: result = new AcbExtBankModel() { Bank = bank, ShortName = "970440", NameVN = "", NameEN = "SEAB - NH TMCP Dong Nam A", }; break;
                case AcbExtBank.SGB_970400: result = new AcbExtBankModel() { Bank = bank, ShortName = "970400", NameVN = "", NameEN = "SGB - NH TMCP Sai Gon Cong Thuong", }; break;
                case AcbExtBank.SHB_970443: result = new AcbExtBankModel() { Bank = bank, ShortName = "970443", NameVN = "", NameEN = "SHB - NH TMCP Sai Gon - Ha Noi", }; break;
                case AcbExtBank.SHBVN_970424: result = new AcbExtBankModel() { Bank = bank, ShortName = "970424", NameVN = "", NameEN = "SHBVN - NH TNHH MTV Shinhan Viet Nam", }; break;
                case AcbExtBank.TMCPCAKE_546034: result = new AcbExtBankModel() { Bank = bank, ShortName = "546034", NameVN = "", NameEN = "TMCP Viet Nam Thinh Vuong - Ngan hang so CAKE by VPBank", }; break;
                case AcbExtBank.TMCPUbank_546035: result = new AcbExtBankModel() { Bank = bank, ShortName = "546035", NameVN = "", NameEN = "TMCP Viet Nam Thinh Vuong - Ngan hang so Ubank by VPBank", }; break;
                case AcbExtBank.TNHH_970410: result = new AcbExtBankModel() { Bank = bank, ShortName = "970410", NameVN = "", NameEN = "TNHH MTV Standard Chartered Bank (Vietnam) Limited", }; break;
                case AcbExtBank.TPB_970423: result = new AcbExtBankModel() { Bank = bank, ShortName = "970423", NameVN = "", NameEN = "TPB - NH TMCP Tien Phong", }; break;
                case AcbExtBank.UOB_970458: result = new AcbExtBankModel() { Bank = bank, ShortName = "970458", NameVN = "", NameEN = "UOB - NH TNHH MTV United Overseas Bank", }; break;
                case AcbExtBank.VAB_970427: result = new AcbExtBankModel() { Bank = bank, ShortName = "970427", NameVN = "", NameEN = "VAB - NH TMCP Viet A", }; break;
                case AcbExtBank.VBSP_999888: result = new AcbExtBankModel() { Bank = bank, ShortName = "999888", NameVN = "", NameEN = "VBSP - Ngan hang Chinh sach Xa hoi", }; break;
                case AcbExtBank.VIB_970441: result = new AcbExtBankModel() { Bank = bank, ShortName = "970441", NameVN = "", NameEN = "VIB - NH TMCP Quoc te Viet Nam", }; break;
                case AcbExtBank.VNPT_971011: result = new AcbExtBankModel() { Bank = bank, ShortName = "971011", NameVN = "", NameEN = "VNPT Money - TT DV TC so VNPT - CN Tong cong ty truyen thong", }; break;
                case AcbExtBank.VRB_970421: result = new AcbExtBankModel() { Bank = bank, ShortName = "970421", NameVN = "", NameEN = "VRB - NH Lien Doanh Viet Nga", }; break;
                case AcbExtBank.Viettel_971005: result = new AcbExtBankModel() { Bank = bank, ShortName = "971005", NameVN = "", NameEN = "Viettel Money - TCT DV so Viettel - CN Tap doan CN VT Quan doi", }; break;
                case AcbExtBank.Vikki_963311: result = new AcbExtBankModel() { Bank = bank, ShortName = "963311", NameVN = "", NameEN = "Vikki by HDBank", }; break;
                case AcbExtBank.WOO_970457: result = new AcbExtBankModel() { Bank = bank, ShortName = "970457", NameVN = "", NameEN = "WOO - NH Wooribank", }; break;

                default: break;
            }
            return result;
        }

        public static string AcbTranslate(this AcbText txt, Lang lang)
        {
            string result = string.Empty;
            switch (txt)
            {
                case AcbText.Language:
                    result = lang == Lang.English ? "English" : "Tiếng việt";
                    break;

                case AcbText.BeneficiaryName:
                    result = lang == Lang.English ? "Beneficiary name" : "Tên đơn vị thụ hưởng";
                    break;

                case AcbText.Balance:
                    result = lang == Lang.English ? "Balance" : "Số dư";
                    break;

                case AcbText.BeneficiaryUnit:
                    result = lang == Lang.English ? "Beneficiary unit" : "Đơn vị thụ hưởng";
                    break;

                case AcbText.AuthMethodSafeKey:
                    result = lang == Lang.English ? "Static password + Advance OTP SafeKey" : "Mật khẩu tĩnh + OTP SafeKey nâng cao";
                    break;

                default: break;
            }
            return result;
        }
        #endregion

        #region SEA
        public static SeaExtBank ToSeaExtBank(this string bank)
        {
            var result = SeaExtBank.Unknown;
            switch (bank.Trim().ToLower())
            {
                case "seabank": case "seab": result = SeaExtBank.SEABANK_317; break;
                case "sbv": result = SeaExtBank.SBV_101; break;
                case "vietinba": case "vtb": case "vietinbank": case "vietin": case "icb": result = SeaExtBank.VIETINBANK_201; break;
                case "bidv": case "bidvbank": result = SeaExtBank.BIDV_202; break;
                case "vietcomb": case "vcb": case "vietcombank": case "vietcom": result = SeaExtBank.Vietcombank_203; break;
                case "agribank": result = SeaExtBank.AGRIBANK_204; break;
                case "vbsp": result = SeaExtBank.VBSP_207; break;
                case "vdb": result = SeaExtBank.VDB_208; break;
                case "maribank": case "msb": case "msbbank": result = SeaExtBank.MSB_302; break;
                case "sbank": case "stb": case "sacombank": result = SeaExtBank.SACOMBANK_303; break;
                case "dab": case "dongabank": case "dongabankdab": result = SeaExtBank.DONGABANK_304; break;
                case "exim": case "eib": case "eximbankeib": case "eximbank": result = SeaExtBank.EXIMBANK_305; break;
                case "nabank": case "nama": case "namabank": result = SeaExtBank.NAMABANK_306; break;
                case "acb": case "acbbank": result = SeaExtBank.ACB_307; break;
                case "sabank": case "sgb": case "saigonbank": result = SeaExtBank.SAIGONBANK_308; break;
                case "vpbank": case "vpb": result = SeaExtBank.VPBANK_309; break;
                case "techcomb": case "tcb": case "techcombank": case "techcom": result = SeaExtBank.TECHCOMBANK_310; break;
                case "mbbank": case "mb": result = SeaExtBank.MB_311; break;
                case "baca": result = SeaExtBank.BACA_313; break;
                case "vib": case "vibbank": result = SeaExtBank.VIB_314; break;
                case "obank": case "oceanbank": case "ocean": result = SeaExtBank.OCEANBANK_319; break;
                case "gpbank": result = SeaExtBank.GPBANK_320; break;
                case "hdbank": case "hdb": result = SeaExtBank.HDBANK_321; break;
                case "abb": case "abbank": result = SeaExtBank.ABBANK_323; break;
                case "viethoa": result = SeaExtBank.VIETHOA_324; break;
                case "vietcapitalbank": result = SeaExtBank.VIETCAPITALBANK_327; break;
                case "ocbank": case "ocb": case "ocbbank": result = SeaExtBank.OCB_333; break;
                case "scb": result = SeaExtBank.SCB_334; break;
                case "nhtmtnhhmtvxaydungvn": result = SeaExtBank.NHTMTNHHMTVXAYDUNGVN_339; break;
                case "pgba": case "pgbank": result = SeaExtBank.PGBANK_341; break;
                case "shb": case "shbbank": result = SeaExtBank.SHB_348; break;
                case "ncb": result = SeaExtBank.NCB_352; break;
                case "klbank": case "kienlongbank": case "kienlongbankklb": case "klb": case "kienlong": result = SeaExtBank.KIENLONGBANK_353; break;
                case "vietabank": result = SeaExtBank.VIETABANK_355; break;
                case "vietbank": result = SeaExtBank.VIETBANK_356; break;
                case "lvpbank": case "lienvietbank": case "lpb": case "lpbank": result = SeaExtBank.LPBANK_357; break;
                case "tpbank": case "tpb": case "tp": result = SeaExtBank.TPBANK_358; break;
                case "bvb": case "baovietbank": case "baovietbankbvb": result = SeaExtBank.BAOVIETBANK_359; break;
                case "pvbank": case "pvcombank": result = SeaExtBank.PVCOMBANK_360; break;
                case "pbvn": result = SeaExtBank.PBVN_501; break;
                case "indobank": case "ivb": result = SeaExtBank.IVB_502; break;
                case "vrb": result = SeaExtBank.VRB_505; break;
                case "thesiamcommercialbankpublic": result = SeaExtBank.THESIAMCOMMERCIALBANKPUBLIC_600; break;
                case "bpceiom": result = SeaExtBank.BPCEIOM_601; break;
                case "anz": result = SeaExtBank.ANZ_602; break;
                case "hlbvn": result = SeaExtBank.HLBVN_603; break;
                case "sc": result = SeaExtBank.SC_604; break;
                case "citibankhanoi": result = SeaExtBank.CITIBANKHANOI_605; break;
                case "scsb": result = SeaExtBank.SCSB_606; break;
                case "fcbhanoi": result = SeaExtBank.FCBHANOI_608; break;
                case "maybankhanoi": result = SeaExtBank.MAYBANKHANOI_609; break;
                case "ccb": result = SeaExtBank.CCB_611; break;
                case "bangkok": result = SeaExtBank.BANGKOK_612; break;
                case "mizuhohanoi": result = SeaExtBank.MIZUHOHANOI_613; break;
                case "bnpparibashcm": result = SeaExtBank.BNPPARIBASHCM_614; break;
                case "bocomm": result = SeaExtBank.BOCOMM_615; break;
                case "nhtnhhmtvshinhanvn": result = SeaExtBank.NHTNHHMTVSHINHANVN_616; break;
                case "hsbcbank": case "hsbc": result = SeaExtBank.HSBC_617; break;
                case "dp": result = SeaExtBank.DP_619; break;
                case "chinabank": result = SeaExtBank.CHINABANK_620; break;
                case "nhmufgbankltd": result = SeaExtBank.NHMUFGBANKLTD_622; break;
                case "mega": result = SeaExtBank.MEGA_623; break;
                case "ocbc": result = SeaExtBank.OCBC_625; break;
                case "kebhanabankcnhanoi": result = SeaExtBank.KEBHANABANKCNHANOI_626; break;
                case "jpm": result = SeaExtBank.JPM_627; break;
                case "ctbc": result = SeaExtBank.CTBC_629; break;
                case "fcbhcm": result = SeaExtBank.FCBHCM_630; break;
                case "kookmin": result = SeaExtBank.KOOKMIN_631; break;
                case "sinopacbank": result = SeaExtBank.SINOPACBANK_632; break;
                case "nhcathay": result = SeaExtBank.NHCATHAY_634; break;
                case "maybank": result = SeaExtBank.MAYBANK_635; break;
                case "smbc": result = SeaExtBank.SMBC_636; break;
                case "bidchn": result = SeaExtBank.BIDCHN_638; break;
                case "mizuhohcm": result = SeaExtBank.MIZUHOHCM_639; break;
                case "hncb": result = SeaExtBank.HNCB_640; break;
                case "ibknhindustrial": result = SeaExtBank.IBKNHINDUSTRIAL_641; break;
                case "fbohanoi": result = SeaExtBank.FBOHANOI_642; break;
                case "bidchcm": result = SeaExtBank.BIDCHCM_648; break;
                case "icbc": result = SeaExtBank.ICBC_649; break;
                case "dbs": result = SeaExtBank.DBS_650; break;
                case "fbohcm": result = SeaExtBank.FBOHCM_651; break;
                case "ibknhcong": result = SeaExtBank.IBKNHCONG_652; break;
                case "nhmufgbankltdcnhanoi": result = SeaExtBank.NHMUFGBANKLTDCNHANOI_653; break;
                case "citibankhcm": result = SeaExtBank.CITIBANKHCM_654; break;
                case "fbobinhduong": result = SeaExtBank.FBOBINHDUONG_655; break;
                case "kebhanabankcntphcm": result = SeaExtBank.KEBHANABANKCNTPHCM_656; break;
                case "bnpparibashanoi": result = SeaExtBank.BNPPARIBASHANOI_657; break;
                case "nhtnhhe.suncndongnai": result = SeaExtBank.NHTNHHE_SUNCNDONGNAI_658; break;
                case "bankofindia": result = SeaExtBank.BANKOFINDIA_659; break;
                case "busan": result = SeaExtBank.BUSAN_660; break;
                case "cimbvnd": case "cimb": case "cimbbank": result = SeaExtBank.CIMB_661; break;
                case "nonghyup": result = SeaExtBank.NONGHYUP_662; break;
                case "woorivn": result = SeaExtBank.WOORIVN_663; break;
                case "nhagriculturalbankofchinalimited": result = SeaExtBank.NHAGRICULTURALBANKOFCHINALIMITED_664; break;
                case "nhtnhhmtvunitedoverseasbankvn": result = SeaExtBank.NHTNHHMTVUNITEDOVERSEASBANKVN_665; break;
                case "nhkookmincnhanoi": result = SeaExtBank.NHKOOKMINCNHANOI_666; break;
                case "nhbangkokdaichungcnhanoi": result = SeaExtBank.NHBANGKOKDAICHUNGCNHANOI_667; break;
                case "nganhangdaegu": result = SeaExtBank.NGANHANGDAEGU_668; break;
                case "kbank": result = SeaExtBank.KBANK_669; break;
                case "kbnn": result = SeaExtBank.KBNN_701; break;
                case "coopbank": result = SeaExtBank.COOPBANK_901; break;
                case "nhvnthinhvuongcake": result = SeaExtBank.NHVNTHINHVUONGCAKE_1001; break;
                case "nhvnthinhvuongubank": result = SeaExtBank.NHVNTHINHVUONGUbank_1002; break;
                case "nhsoumeebykienlongbank": result = SeaExtBank.NHsoUMEEbyKienlongbank_1003; break;
                case "viettelmoney": result = SeaExtBank.VIETTELMONEY_1004; break;
                case "timobybanvietbank": result = SeaExtBank.TimobyBanVietBank_1005; break;
                case "mafc": result = SeaExtBank.MAFC_1006; break;
                case "liobank": result = SeaExtBank.LIOBANK_1007; break;
                case "vnptmoney": result = SeaExtBank.VNPTMoney_1008; break;
                case "mvasmbilemoney": result = SeaExtBank.MVASMbileMoney_1009; break;
                case "svfc": result = SeaExtBank.SVFC_1010; break;
                case "vikkibyhdbank": result = SeaExtBank.VikkibyHDBank_1011; break;
                default: break;
            }
            return result;
        }

        public static SeaExtBankModel? GetSeaExtBank(this SeaExtBank bank)
        {
            SeaExtBankModel? result = null;
            switch (bank)
            {
                case SeaExtBank.SEABANK_317: result = new SeaExtBankModel() { Bank = bank, ShortName = "317", NameVN = "Ngân hàng TMCP Đông Nam Á", NameEN = "SEABANK-NH TMCP DONG NAM A", }; break;
                case SeaExtBank.SBV_101: result = new SeaExtBankModel() { Bank = bank, ShortName = "101", NameVN = "SBV - Ngân hàng Nhà Nước Việt Nam", NameEN = "SBV-NH NHA NUOC VIET NAM", }; break;
                case SeaExtBank.VIETINBANK_201: result = new SeaExtBankModel() { Bank = bank, ShortName = "201", NameVN = "VIETINBANK - Ngân hàng TMCP Công Thương Việt Nam", NameEN = "VIETINBANK-NH TMCP CONG THUONG VN", }; break;
                case SeaExtBank.BIDV_202: result = new SeaExtBankModel() { Bank = bank, ShortName = "202", NameVN = "BIDV - Ngân hàng TMCP Đầu Tư và Phát Triển Việt Nam", NameEN = "BIDV-NH TMCP DT VA PHAT TRIEN VN", }; break;
                case SeaExtBank.Vietcombank_203: result = new SeaExtBankModel() { Bank = bank, ShortName = "203", NameVN = "VIETCOMBANK - Ngân hàng TMCP Ngoại Thương Việt Nam", NameEN = "VCB-NH TMCP NGOAI THUONG VN", }; break;
                case SeaExtBank.AGRIBANK_204: result = new SeaExtBankModel() { Bank = bank, ShortName = "204", NameVN = "AGRIBANK - Ngân hàng Nông Nghiệp và Phát Triển Nông Thôn Việt Nam", NameEN = "AGRIBANK-NH NONG NGHIEP VA PTNT VN", }; break;
                case SeaExtBank.VBSP_207: result = new SeaExtBankModel() { Bank = bank, ShortName = "207", NameVN = "VBSP - Ngân hàng Chính Sách Xã Hội", NameEN = "VBSP-NH CHINH SACH XA HOI", }; break;
                case SeaExtBank.VDB_208: result = new SeaExtBankModel() { Bank = bank, ShortName = "208", NameVN = "VDB - Ngân hàng Phát triển Việt Nam", NameEN = "VDB-NH PHAT TRIEN VIET NAM", }; break;
                case SeaExtBank.MSB_302: result = new SeaExtBankModel() { Bank = bank, ShortName = "302", NameVN = "MSB - Ngân hàngTMCP Hàng Hải Việt Nam", NameEN = "MSB-NH TMCP HANG HAI VN", }; break;
                case SeaExtBank.SACOMBANK_303: result = new SeaExtBankModel() { Bank = bank, ShortName = "303", NameVN = "SACOMBANK - Ngân hàng TMCP Sài Gòn Thương Tín", NameEN = "SACOMBANK-NH TMCP SG THUONG TIN", }; break;
                case SeaExtBank.DONGABANK_304: result = new SeaExtBankModel() { Bank = bank, ShortName = "304", NameVN = "DONGABANK - Ngân hàng TMCP Đông Á", NameEN = "DONGABANK-NH TMCP DONG A", }; break;
                case SeaExtBank.EXIMBANK_305: result = new SeaExtBankModel() { Bank = bank, ShortName = "305", NameVN = "EXIMBANK - Ngân hàng TMCP Xuất Nhập Khẩu Việt Nam", NameEN = "EXIMBANK-NH TMCP XUAT NHAP KHAU VN", }; break;
                case SeaExtBank.NAMABANK_306: result = new SeaExtBankModel() { Bank = bank, ShortName = "306", NameVN = "NAM A BANK - Ngân hàng TMCP Nam Á", NameEN = "NAM A BANK-NH TMCP NAM A", }; break;
                case SeaExtBank.ACB_307: result = new SeaExtBankModel() { Bank = bank, ShortName = "307", NameVN = "ACB - Ngân hàng TMCP Á Châu", NameEN = "ACB-NH TMCP A CHAU", }; break;
                case SeaExtBank.SAIGONBANK_308: result = new SeaExtBankModel() { Bank = bank, ShortName = "308", NameVN = "SAIGONBANK - Ngân hàng TMCP Sài Gòn Công Thương", NameEN = "SAIGONBANK-NH TMCP SG CONG THUONG", }; break;
                case SeaExtBank.VPBANK_309: result = new SeaExtBankModel() { Bank = bank, ShortName = "309", NameVN = "VPBANK - Ngân hàng TMCP Việt Nam Thịnh Vượng", NameEN = "VPBANK-NH VIET NAM THINH VUONG", }; break;
                case SeaExtBank.TECHCOMBANK_310: result = new SeaExtBankModel() { Bank = bank, ShortName = "310", NameVN = "TECHCOMBANK - Ngân hàng TMCP Kỹ Thương Việt Nam", NameEN = "TECHCOMBANK-NH TMCP KY THUONG VN", }; break;
                case SeaExtBank.MB_311: result = new SeaExtBankModel() { Bank = bank, ShortName = "311", NameVN = "MBBANK - Ngân hàng TMCP Quân Đội", NameEN = "MB-NH TMCP QUAN DOI", }; break;
                case SeaExtBank.BACA_313: result = new SeaExtBankModel() { Bank = bank, ShortName = "313", NameVN = "BACABANK - Ngân hàng TMCP Bắc Á", NameEN = "BACA-NH TMCP BAC A NASB", }; break;
                case SeaExtBank.VIB_314: result = new SeaExtBankModel() { Bank = bank, ShortName = "314", NameVN = "VIB - Ngân hàng TMCP Quốc tế Việt Nam", NameEN = "VIB-NH TMCP QUOC TE", }; break;
                case SeaExtBank.OCEANBANK_319: result = new SeaExtBankModel() { Bank = bank, ShortName = "319", NameVN = "OCEANBANK - Ngân hàng TNHH MTV Đại Dương", NameEN = "OCEANBANK-NH TNHH MTV DAI DUONG", }; break;
                case SeaExtBank.GPBANK_320: result = new SeaExtBankModel() { Bank = bank, ShortName = "320", NameVN = "GPBANK - Ngân hàng TMCP Dầu Khí Toàn Cầu", NameEN = "GPBANK-NGAN HANG DAU KHI TOAN CAU", }; break;
                case SeaExtBank.HDBANK_321: result = new SeaExtBankModel() { Bank = bank, ShortName = "321", NameVN = "HDBANK - Ngân Hàng TMCP Phát Triển Nhà Tp.Hồ Chí Minh", NameEN = "HDBANK-NH TMCP PHAT TRIEN TPHCM", }; break;
                case SeaExtBank.ABBANK_323: result = new SeaExtBankModel() { Bank = bank, ShortName = "323", NameVN = "ABBANK - Ngân hàng TMCP An Bình", NameEN = "ABBANK-AN BINH COMMERCIAL J.S BANK", }; break;
                case SeaExtBank.VIETHOA_324: result = new SeaExtBankModel() { Bank = bank, ShortName = "324", NameVN = "VIETHOA - Ngân hàng TMCP Việt Hoa", NameEN = "VIETHOA-NH TMCP VIET HOA", }; break;
                case SeaExtBank.VIETCAPITALBANK_327: result = new SeaExtBankModel() { Bank = bank, ShortName = "327", NameVN = "VIETCAPITALBANK - Ngân hàng TMCP Bản Việt", NameEN = "VIETCAPITALBANK-NH TMCP BAN VIET", }; break;
                case SeaExtBank.OCB_333: result = new SeaExtBankModel() { Bank = bank, ShortName = "333", NameVN = "OCB - Ngân hàng TMCP Phương Đông", NameEN = "OCB-NH TMCP PHUONG DONG", }; break;
                case SeaExtBank.SCB_334: result = new SeaExtBankModel() { Bank = bank, ShortName = "334", NameVN = "SCB - Ngân hàng TMCP Sài Gòn", NameEN = "SCB-NH TMCP SAI GON", }; break;
                case SeaExtBank.NHTMTNHHMTVXAYDUNGVN_339: result = new SeaExtBankModel() { Bank = bank, ShortName = "339", NameVN = "CBBANK - Ngân hàng TM TNHH MTV Xây Dựng Việt Nam", NameEN = "NH TM TNHH MTV XAY DUNG VN", }; break;
                case SeaExtBank.PGBANK_341: result = new SeaExtBankModel() { Bank = bank, ShortName = "341", NameVN = "PGBANK - Ngân hàng TMCP Thịnh vượng và Phát triển", NameEN = "PGBANK-NH TMCP THINH VUONG VA PT", }; break;
                case SeaExtBank.SHB_348: result = new SeaExtBankModel() { Bank = bank, ShortName = "348", NameVN = "SHB - Ngân hàng TMCP Sài Gòn - Hà Nội", NameEN = "SHB-NH TMCP SAI GON-HA NOI", }; break;
                case SeaExtBank.NCB_352: result = new SeaExtBankModel() { Bank = bank, ShortName = "352", NameVN = "NCB - Ngân hàng TMCP Quốc Dân", NameEN = "NCB-NH TMCP QUOC DAN", }; break;
                case SeaExtBank.KIENLONGBANK_353: result = new SeaExtBankModel() { Bank = bank, ShortName = "353", NameVN = "KIENLONGBANK - Ngân hàng TMCP Kiên Long", NameEN = "KIENLONGBANK-NH TMCP KIEN LONG", }; break;
                case SeaExtBank.VIETABANK_355: result = new SeaExtBankModel() { Bank = bank, ShortName = "355", NameVN = "VIETABANK - Ngân hàng TMCP Việt Á", NameEN = "VIETABANK-NH TMCP VIET A", }; break;
                case SeaExtBank.VIETBANK_356: result = new SeaExtBankModel() { Bank = bank, ShortName = "356", NameVN = "VIETBANK - Ngân hàng TMCP Việt Nam Thương Tín", NameEN = "VIETBANK-NH TMCP VN THUONG TIN", }; break;
                case SeaExtBank.LPBANK_357: result = new SeaExtBankModel() { Bank = bank, ShortName = "357", NameVN = "LPBANK - Ngân hàng TMCP Lộc Phát Việt Nam", NameEN = "LPBANK-NH TMCP LOC PHAT VIET NAM", }; break;
                case SeaExtBank.TPBANK_358: result = new SeaExtBankModel() { Bank = bank, ShortName = "358", NameVN = "TPBANK - Ngân hàng TMCP Tiên Phong", NameEN = "TPBANK-NH TMCP TIEN PHONG", }; break;
                case SeaExtBank.BAOVIETBANK_359: result = new SeaExtBankModel() { Bank = bank, ShortName = "359", NameVN = "BAOVIETBANK - Ngân hàng TMCP Bảo Việt", NameEN = "BAOVIETBANK-NH TMCP BAO VIET", }; break;
                case SeaExtBank.PVCOMBANK_360: result = new SeaExtBankModel() { Bank = bank, ShortName = "360", NameVN = "PVCOMBANK - Ngân hàng TMCP Đại Chúng Việt Nam", NameEN = "PVCOMBANK-NH TMCP DAI CHUNG VN", }; break;
                case SeaExtBank.PBVN_501: result = new SeaExtBankModel() { Bank = bank, ShortName = "501", NameVN = "PUBLICBANK - Ngân hàng TNHH MTV Public Việt Nam", NameEN = "PBVN-NH TNHH MTV PUBLIC VIET NAM", }; break;
                case SeaExtBank.IVB_502: result = new SeaExtBankModel() { Bank = bank, ShortName = "502", NameVN = "INDOVINABANK - Ngân hàng TNHH Indovina", NameEN = "IVB-NH TNHH INDOVINA", }; break;
                case SeaExtBank.VRB_505: result = new SeaExtBankModel() { Bank = bank, ShortName = "505", NameVN = "VRB - Ngân hàng Liên doanh Việt - Nga", NameEN = "VRB-NH LIEN DOANH VIET - NGA", }; break;
                case SeaExtBank.THESIAMCOMMERCIALBANKPUBLIC_600: result = new SeaExtBankModel() { Bank = bank, ShortName = "600", NameVN = "SIAMOB - Ngân hàng The Siam Commercial Bank Public Company Limited", NameEN = "THE SIAM COMMERCIAL BANK PUBLIC", }; break;
                case SeaExtBank.BPCEIOM_601: result = new SeaExtBankModel() { Bank = bank, ShortName = "601", NameVN = "BPCE IOM - Ngân hàng BPCE IOM tại Việt Nam", NameEN = "BPCEIOM-NGAN HANG BPCEIOM", }; break;
                case SeaExtBank.ANZ_602: result = new SeaExtBankModel() { Bank = bank, ShortName = "602", NameVN = "ANZ - Ngân hàng TNHH MTV ANZ Việt Nam", NameEN = "ANZ-NH TNHH MTV ANZ VN", }; break;
                case SeaExtBank.HLBVN_603: result = new SeaExtBankModel() { Bank = bank, ShortName = "603", NameVN = "HLBANK - Ngân hàng Hong Leong Bank Việt Nam", NameEN = "HLBVN-NH HONG LEONG VN", }; break;
                case SeaExtBank.SC_604: result = new SeaExtBankModel() { Bank = bank, ShortName = "604", NameVN = "SC - Ngân hàng TNHH MTV Standard Chartered", NameEN = "SC-NH STANDARD CHARTERED BANK", }; break;
                case SeaExtBank.CITIBANKHANOI_605: result = new SeaExtBankModel() { Bank = bank, ShortName = "605", NameVN = "CITIBANK - Ngân hàng Citibank CN Hà Nội", NameEN = "CITIBANK-NH CITI BANK HA NOI", }; break;
                case SeaExtBank.SCSB_606: result = new SeaExtBankModel() { Bank = bank, ShortName = "606", NameVN = "SCSB - Ngân hàng The Shanghai Commercial & Savings Bank, Ltd", NameEN = "SCSB-NH SHANGHAI COMMERCIAL SAVINGS", }; break;
                case SeaExtBank.FCBHANOI_608: result = new SeaExtBankModel() { Bank = bank, ShortName = "608", NameVN = "FCB - Ngân Hàng First Commercial Bank CN Hà Nội", NameEN = "FCB-NH FIRST COMMERCIAL CN HA NOI", }; break;
                case SeaExtBank.MAYBANKHANOI_609: result = new SeaExtBankModel() { Bank = bank, ShortName = "609", NameVN = "MAYBANK - Ngân hàng MALAYAN BANKING BERHAD CN Hà Nội", NameEN = "MAYBANK-NH MAYBANK HA NOI", }; break;
                case SeaExtBank.CCB_611: result = new SeaExtBankModel() { Bank = bank, ShortName = "611", NameVN = "CCB - Ngân hàng China Construction Bank Corporation tại Việt Nam", NameEN = "CCB-NH CHINA CONSTRUCTION", }; break;
                case SeaExtBank.BANGKOK_612: result = new SeaExtBankModel() { Bank = bank, ShortName = "612", NameVN = "BANGKOK - Ngân hàng BANGKOK tại Việt Nam", NameEN = "BANGKOK-NH BANGKOK TAI VN", }; break;
                case SeaExtBank.MIZUHOHANOI_613: result = new SeaExtBankModel() { Bank = bank, ShortName = "613", NameVN = "MIZUHO-NGÂN HÀNG MIZUHO BANK HÀ NỘI", NameEN = "MIZUHO-NGAN HANG MIZUHO BANK HA NOI", }; break;
                case SeaExtBank.BNPPARIBASHCM_614: result = new SeaExtBankModel() { Bank = bank, ShortName = "614", NameVN = "BNP PARIBAS - Ngân hàng BNP PARIBAS CN Hồ Chí Minh", NameEN = "BNP PARIBAS-NH BNP PARIBAS CN HCM", }; break;
                case SeaExtBank.BOCOMM_615: result = new SeaExtBankModel() { Bank = bank, ShortName = "615", NameVN = "BOCOMM - Ngân hàng COMMUNICATIONS tại Việt Nam", NameEN = "BOCOMM-NH COMMUNICATIONS TAI VN", }; break;
                case SeaExtBank.NHTNHHMTVSHINHANVN_616: result = new SeaExtBankModel() { Bank = bank, ShortName = "616", NameVN = "SHBVN - Ngân Hàng TNHH MTV Shinhan Việt Nam", NameEN = "NH TNHH MTV SHINHAN VN", }; break;
                case SeaExtBank.HSBC_617: result = new SeaExtBankModel() { Bank = bank, ShortName = "617", NameVN = "HSBC - Ngân hàng TNHH MTV HSBC Việt Nam", NameEN = "HSBC-NH TNHH MTV HSBC VN", }; break;
                case SeaExtBank.DP_619: result = new SeaExtBankModel() { Bank = bank, ShortName = "619", NameVN = "DP - Ngân hàng Deutsche Bank tại Việt Nam", NameEN = "DP-NH DEUTSCHE BANK TAI VN", }; break;
                case SeaExtBank.CHINABANK_620: result = new SeaExtBankModel() { Bank = bank, ShortName = "620", NameVN = "CHINA BANK - Ngân hàng BANK OF CHINA tại Việt Nam", NameEN = "CHINA BANK-NH CHINA TAI VN", }; break;
                case SeaExtBank.NHMUFGBANKLTD_622: result = new SeaExtBankModel() { Bank = bank, ShortName = "622", NameVN = "MUFG - Ngân hàng MUFG BANK, LTD", NameEN = "NH MUFG BANK LTD", }; break;
                case SeaExtBank.MEGA_623: result = new SeaExtBankModel() { Bank = bank, ShortName = "623", NameVN = "MEGA - Ngân hàng Mega International Commercial Bank", NameEN = "MEGA-NH MEGA ICBC BANK", }; break;
                case SeaExtBank.OCBC_625: result = new SeaExtBankModel() { Bank = bank, ShortName = "625", NameVN = "OCBC - Ngân hàng Oversea-Chinese Banking Corporation LTD", NameEN = "OCBC-NH OVERSEA CHINESE BANKING", }; break;
                case SeaExtBank.KEBHANABANKCNHANOI_626: result = new SeaExtBankModel() { Bank = bank, ShortName = "626", NameVN = "KEB - Ngân hàng KOREA EXCHANGE BANK", NameEN = "KEB HANA BANK CN HA NOI", }; break;
                case SeaExtBank.JPM_627: result = new SeaExtBankModel() { Bank = bank, ShortName = "627", NameVN = "JPM - Ngân hàng JP Morgan Chase, N.A", NameEN = "JPM-NH JPMORGAN CHASE N.A", }; break;
                case SeaExtBank.CTBC_629: result = new SeaExtBankModel() { Bank = bank, ShortName = "629", NameVN = "CTBC - Ngân hàng TNHH CTBC", NameEN = "CTBC-NH TNHH CTBC", }; break;
                case SeaExtBank.FCBHCM_630: result = new SeaExtBankModel() { Bank = bank, ShortName = "630", NameVN = "FCB - Ngân Hàng First Commercial Bank CN Hồ Chí Minh", NameEN = "FCB-NH FIRST COMMERCIAL CN HCM", }; break;
                case SeaExtBank.KOOKMIN_631: result = new SeaExtBankModel() { Bank = bank, ShortName = "631", NameVN = "KOOKMIN - Ngân Hàng Kookmin CN Hồ Chí Minh", NameEN = "KOOKMIN-NH KOOKMIN CN TPHCM", }; break;
                case SeaExtBank.SINOPACBANK_632: result = new SeaExtBankModel() { Bank = bank, ShortName = "632", NameVN = "SINOPACBANK - Ngân hàng SinoPac", NameEN = "SINOPACBANK-NGAN HANG SINOPAC", }; break;
                case SeaExtBank.NHCATHAY_634: result = new SeaExtBankModel() { Bank = bank, ShortName = "634", NameVN = "CATHAY - Ngân hàng Cathay United Bank CN Chu Lai", NameEN = "NH CATHAY", }; break;
                case SeaExtBank.MAYBANK_635: result = new SeaExtBankModel() { Bank = bank, ShortName = "635", NameVN = "MAYBANK - Ngân hàng MALAYAN BANKING BERHAD", NameEN = "MAYBANK-NH MALAYAN BANKING BERHAD", }; break;
                case SeaExtBank.SMBC_636: result = new SeaExtBankModel() { Bank = bank, ShortName = "636", NameVN = "SMBC - Ngân hàng Sumitomo Mitsui Banking Corporation", NameEN = "SMBC-NH SUMITOMO MITSUI CORPORATION", }; break;
                case SeaExtBank.BIDCHN_638: result = new SeaExtBankModel() { Bank = bank, ShortName = "638", NameVN = "BIDC - NH Đầu tư và Phát triển CAMPUCHIA CN Hà Nội", NameEN = "BIDC-NH DT VA PT CAMPUCHIA CN HN", }; break;
                case SeaExtBank.MIZUHOHCM_639: result = new SeaExtBankModel() { Bank = bank, ShortName = "639", NameVN = "MIZUHO-NGÂN HÀNG MIZUHO BANK TP.HCM", NameEN = "MIZUHO-NGAN HANG MIZUHO BANK TP.HCM", }; break;
                case SeaExtBank.HNCB_640: result = new SeaExtBankModel() { Bank = bank, ShortName = "640", NameVN = "HNCB - Ngân hàng Hua Nan Commercial Bank tại Việt Nam", NameEN = "HNCB-NH HUA NAN COMMERCIAL TAI VN", }; break;
                case SeaExtBank.IBKNHINDUSTRIAL_641: result = new SeaExtBankModel() { Bank = bank, ShortName = "641", NameVN = "IBK - Ngân Hàng Industrial Bank Of Korea CN Hồ Chí Minh", NameEN = "IBK-NH INDUSTRIAL BANK OF KOREA", }; break;
                case SeaExtBank.FBOHANOI_642: result = new SeaExtBankModel() { Bank = bank, ShortName = "642", NameVN = "FBO - Ngân hàng Taipei Fubon CN Hà Nội", NameEN = "FBO-NH TAIPEI FUBON CN HA NOI", }; break;
                case SeaExtBank.BIDCHCM_648: result = new SeaExtBankModel() { Bank = bank, ShortName = "648", NameVN = "BIDC - NH Đầu tư và Phát triển CAMPUCHIA CN Hồ Chí Minh", NameEN = "BIDC-NH DT VA PT CAMPUCHIA CN TPHCM", }; break;
                case SeaExtBank.ICBC_649: result = new SeaExtBankModel() { Bank = bank, ShortName = "649", NameVN = "ICBC - Ngân hàng Industrial and Commercial Bank of China", NameEN = "ICBC-NH INDUSTRIAL AND COM OF CHINA", }; break;
                case SeaExtBank.DBS_650: result = new SeaExtBankModel() { Bank = bank, ShortName = "650", NameVN = "DBS - Ngân hàng DBS Bank Ltd", NameEN = "DBS-NH DBS BANK LTD", }; break;
                case SeaExtBank.FBOHCM_651: result = new SeaExtBankModel() { Bank = bank, ShortName = "651", NameVN = "FBO - Ngân hàng Taipei Fubon CN Hồ Chí Minh", NameEN = "FBO-NH TAIPEI FUBON CN HCM", }; break;
                case SeaExtBank.IBKNHCONG_652: result = new SeaExtBankModel() { Bank = bank, ShortName = "652", NameVN = "IBK - Ngân Hàng Industrial Bank Of Korea CN Hà Nội", NameEN = "IBK-NH CONG NGHIEP HAN QUOC", }; break;
                case SeaExtBank.NHMUFGBANKLTDCNHANOI_653: result = new SeaExtBankModel() { Bank = bank, ShortName = "653", NameVN = "MUFG - Ngân hàng MUFG BANK, LTD CN Hà Nội", NameEN = "NH MUFG BANK LTD CN HA NOI", }; break;
                case SeaExtBank.CITIBANKHCM_654: result = new SeaExtBankModel() { Bank = bank, ShortName = "654", NameVN = "CITIBANK - Ngân hàng Citibank CN Hồ Chí Minh", NameEN = "CITIBANK-NH CITIBANK CN TP HCM HSC", }; break;
                case SeaExtBank.FBOBINHDUONG_655: result = new SeaExtBankModel() { Bank = bank, ShortName = "655", NameVN = "FBO - Ngân hàng Taipei Fubon CN Bình Dương", NameEN = "FBO-NH TAIPEI FUBON CN BINH DUONG", }; break;
                case SeaExtBank.KEBHANABANKCNTPHCM_656: result = new SeaExtBankModel() { Bank = bank, ShortName = "656", NameVN = "HANABANK - Ngân hàng KEB Hana CN Hồ Chí Minh", NameEN = "KEB HANA BANK CN TPHCM", }; break;
                case SeaExtBank.BNPPARIBASHANOI_657: result = new SeaExtBankModel() { Bank = bank, ShortName = "657", NameVN = "BNP PARIBAS - Ngân hàng BNP PARIBAS CN Hà Nội", NameEN = "BNP PARIBAS-NH BNP PARIBAS CN HANOI", }; break;
                case SeaExtBank.NHTNHHE_SUNCNDONGNAI_658: result = new SeaExtBankModel() { Bank = bank, ShortName = "658", NameVN = "E.SUN - Ngân hàng TNHH E.SUN", NameEN = "NH TNHH E.SUN CN DONG NAI", }; break;
                case SeaExtBank.BANKOFINDIA_659: result = new SeaExtBankModel() { Bank = bank, ShortName = "659", NameVN = "BANK OF INDIA - Ngân hàng BANK OF INDIA CN Hồ Chí Minh", NameEN = "BANK OF INDIA-NH BANK OF INDIA", }; break;
                case SeaExtBank.BUSAN_660: result = new SeaExtBankModel() { Bank = bank, ShortName = "660", NameVN = "BUSAN - Ngân hàng BUSAN tại Việt Nam", NameEN = "BUSAN-NGAN HANG BUSAN", }; break;
                case SeaExtBank.CIMB_661: result = new SeaExtBankModel() { Bank = bank, ShortName = "661", NameVN = "CIMB - Ngân hàng TNHH MTV CIMB Việt Nam", NameEN = "CIMB-NH TNHH MTV CIMB VIET NAM", }; break;
                case SeaExtBank.NONGHYUP_662: result = new SeaExtBankModel() { Bank = bank, ShortName = "662", NameVN = "NONGHYUP - Ngân hàng Nonghyup", NameEN = "NONGHYUP-NH NONGHYUP", }; break;
                case SeaExtBank.WOORIVN_663: result = new SeaExtBankModel() { Bank = bank, ShortName = "663", NameVN = "WOORI VN - Ngân hàng TNHH MTV Woori Việt Nam", NameEN = "WOORI VN-NH TNHH MTV WOORI VIET NAM", }; break;
                case SeaExtBank.NHAGRICULTURALBANKOFCHINALIMITED_664: result = new SeaExtBankModel() { Bank = bank, ShortName = "664", NameVN = "ABOC - Ngân hàng Agricultural Bank of China Limited", NameEN = "NHAGRICULTURALBANK OFCHINA LIMITED", }; break;
                case SeaExtBank.NHTNHHMTVUNITEDOVERSEASBANKVN_665: result = new SeaExtBankModel() { Bank = bank, ShortName = "665", NameVN = "UOB - Ngân hàng TNHH MTV United Overseas Bank Việt Nam", NameEN = "NH TNHH MTV UNITED OVERSEAS BANK VN", }; break;
                case SeaExtBank.NHKOOKMINCNHANOI_666: result = new SeaExtBankModel() { Bank = bank, ShortName = "666", NameVN = "KOOKMIN - Ngân Hàng Kookmin CN Hà Nội", NameEN = "NH KOOKMIN CN HA NOI", }; break;
                case SeaExtBank.NHBANGKOKDAICHUNGCNHANOI_667: result = new SeaExtBankModel() { Bank = bank, ShortName = "667", NameVN = "BANGKOK BANK - Ngân hàng Bangkok Đại chúng CN Hà Nội", NameEN = "NH BANGKOK DAI CHUNG CN HA NOI", }; break;
                case SeaExtBank.NGANHANGDAEGU_668: result = new SeaExtBankModel() { Bank = bank, ShortName = "668", NameVN = "DAEGU - Ngân hàng Daegu CN Hồ Chí Minh", NameEN = "NGAN HANG DAEGU - CN TP HO CHI MINH", }; break;
                case SeaExtBank.KBANK_669: result = new SeaExtBankModel() { Bank = bank, ShortName = "669", NameVN = "KBANK - Ngân hàng Kasikorn CN Hồ Chí Minh", NameEN = "KBANK- NHDAICHUNGKASIKORNBANK CNHCM", }; break;
                case SeaExtBank.KBNN_701: result = new SeaExtBankModel() { Bank = bank, ShortName = "701", NameVN = "KBNN - Kho Bạc Nhà Nước Tỉnh, Thành phố", NameEN = "KBNN-KHO BAC NHA NUOC TINH, TP", }; break;
                case SeaExtBank.COOPBANK_901: result = new SeaExtBankModel() { Bank = bank, ShortName = "901", NameVN = "COOPBANK - Ngân hàng Hợp Tác Xã Việt Nam", NameEN = "COOPBANK-NH HOP TAC XA VN", }; break;
                case SeaExtBank.NHVNTHINHVUONGCAKE_1001: result = new SeaExtBankModel() { Bank = bank, ShortName = "1001", NameVN = "CAKE - Ngân hàng số Cake by VPBank", NameEN = "NH VN THINH VUONG-CAKE by VPBank", }; break;
                case SeaExtBank.NHVNTHINHVUONGUbank_1002: result = new SeaExtBankModel() { Bank = bank, ShortName = "1002", NameVN = "UBANK - Ngân hàng số UBank by VPBank", NameEN = "NH VN THINH VUONG-Ubank by VPBank", }; break;
                case SeaExtBank.NHsoUMEEbyKienlongbank_1003: result = new SeaExtBankModel() { Bank = bank, ShortName = "1003", NameVN = "UMEE - Ngân hàng số Umee by KienLongBank", NameEN = "NH so UMEE by Kienlongbank", }; break;
                case SeaExtBank.VIETTELMONEY_1004: result = new SeaExtBankModel() { Bank = bank, ShortName = "1004", NameVN = "Viettel Money", NameEN = "VIETTEL MONEY", }; break;
                case SeaExtBank.TimobyBanVietBank_1005: result = new SeaExtBankModel() { Bank = bank, ShortName = "1005", NameVN = "Ngân hàng số Timo – Ngân hàng TMCP Bản Việt", NameEN = "Timo by Ban Viet Bank", }; break;
                case SeaExtBank.MAFC_1006: result = new SeaExtBankModel() { Bank = bank, ShortName = "1006", NameVN = "MAFC-Cty TC TNHH MTV Mirae Asset", NameEN = "MAFC-Cty TC TNHH MTV Mirae Asset", }; break;
                case SeaExtBank.LIOBANK_1007: result = new SeaExtBankModel() { Bank = bank, ShortName = "1007", NameVN = "Liobank - Ngân hàng số Liobank by OCB", NameEN = "LIOBANK-Ngan hang so Liobank", }; break;
                case SeaExtBank.VNPTMoney_1008: result = new SeaExtBankModel() { Bank = bank, ShortName = "1008", NameVN = "VNPT Money", NameEN = "VNPT Money", }; break;
                case SeaExtBank.MVASMbileMoney_1009: result = new SeaExtBankModel() { Bank = bank, ShortName = "1009", NameVN = "Mobifone - MVAS", NameEN = "MVAS Mbile Money", }; break;
                case SeaExtBank.SVFC_1010: result = new SeaExtBankModel() { Bank = bank, ShortName = "1010", NameVN = "SVFC-Cty TC TNHH MTV SHINHAN VN", NameEN = "SVFC-Cty TC TNHH MTV SHINHAN VN", }; break;
                case SeaExtBank.VikkibyHDBank_1011: result = new SeaExtBankModel() { Bank = bank, ShortName = "1011", NameVN = "Vikki by HDBank-kenh so hoa HDbank", NameEN = "Vikki by HDBank-kenh so hoa HDbank", }; break;

                default: break;
            }
            return result;
        }

        public static string? GetSeaShortName(this SeaExtBank bank)
        {
            return bank.GetSeaExtBank()?.ShortName;
        }

        public static string? GetSeaBankName(this SeaExtBank bank, Lang lang)
        {
            string? result = null;
            if (lang == Lang.English)
            {
                result = bank.GetSeaExtBank()?.NameEN;
            }
            else if (lang == Lang.Vietnamese)
            {
                result = bank.GetSeaExtBank()?.NameVN;
            }
            return result;
        }
        #endregion

        #region MSB
        public static MsbExtBank ToMsbExtBank(this string bank)
        {
            var result = MsbExtBank.Unknown;
            switch (bank.Trim().ToLower())
            {
                case "abb": case "abbank": result = MsbExtBank.ABBANK; break;
                case "acb": case "acbbank": result = MsbExtBank.ACB; break;
                case "agribank": result = MsbExtBank.AGRIBANK; break;
                case "bvb": case "baovietbank": case "baovietbankbvb": result = MsbExtBank.BAOVIETBANK; break;
                case "bidc": result = MsbExtBank.BIDC; break;
                case "bidv": case "bidvbank": result = MsbExtBank.BIDV; break;
                case "bnpparibashcm": result = MsbExtBank.BNPPARIBASHCM; break;
                case "bnpparibashn": result = MsbExtBank.BNPPARIBASHN; break;
                case "bochk": result = MsbExtBank.BOCHK; break;
                case "bvbank": result = MsbExtBank.BVBANK; break;
                case "bvbanktimo": result = MsbExtBank.BVBANKTIMO; break;
                case "cathayunitedbank": result = MsbExtBank.CATHAYUNITEDBANK; break;
                case "cbbank": result = MsbExtBank.CBBANK; break;
                case "cimbvnd": case "cimb": case "cimbbank": result = MsbExtBank.CIMB; break;
                case "citibankvietnam": result = MsbExtBank.CITIBANKVIETNAM; break;
                case "coopbank": result = MsbExtBank.COOPBANK; break;
                case "dbs": result = MsbExtBank.DBS; break;
                case "dab": case "dongabank": case "dongabankdab": result = MsbExtBank.DONGABANK; break;
                case "dvcqg": result = MsbExtBank.DVCQG; break;
                case "exim": case "eib": case "eximbankeib": case "eximbank": result = MsbExtBank.EXIMBANK; break;
                case "gpbank": result = MsbExtBank.GPBANK; break;
                case "hdbank": case "hdb": result = MsbExtBank.HDBANK; break;
                case "hlbank": case "hlb": case "hongleongbank": case "hongleong": result = MsbExtBank.HONGLEONGBANK; break;
                case "hsbcbank": case "hsbc": result = MsbExtBank.HSBC; break;
                case "ibk": result = MsbExtBank.IBK; break;
                case "indovinabank": result = MsbExtBank.INDOVINABANK; break;
                case "kasikornbank": result = MsbExtBank.KASIKORNBANK; break;
                case "kebhanahcm": result = MsbExtBank.KEBHANAHCM; break;
                case "kebhanahn": result = MsbExtBank.KEBHANAHN; break;
                case "klbank": case "kienlongbank": case "kienlongbankklb": case "klb": case "kienlong": result = MsbExtBank.KIENLONGBANK; break;
                case "kookminbank": result = MsbExtBank.KOOKMINBANK; break;
                case "kookmin": result = MsbExtBank.KOOKMIN; break;
                case "liobank": result = MsbExtBank.LIOBANK; break;
                case "lvpbank": case "lienvietbank": case "lpb": case "lpbank": result = MsbExtBank.LPBANK; break;
                case "mafc": result = MsbExtBank.MAFC; break;
                case "mbbank": case "mb": result = MsbExtBank.MB; break;
                case "nabank": case "nama": case "namabank": result = MsbExtBank.NAMABANK; break;
                case "nasbank": result = MsbExtBank.NASBANK; break;
                case "ncb": result = MsbExtBank.NCB; break;
                case "nonghyup": result = MsbExtBank.NONGHYUP; break;
                case "ocbank": case "ocb": case "ocbbank": result = MsbExtBank.OCB; break;
                case "obank": case "oceanbank": case "ocean": result = MsbExtBank.OCEANBANK; break;
                case "pgba": case "pgbank": result = MsbExtBank.PGBANK; break;
                case "pvbank": case "pvcombank": result = MsbExtBank.PVCOMBANK; break;
                case "sbank": case "stb": case "sacombank": result = MsbExtBank.SACOMBANK; break;
                case "sabank": case "sgb": case "saigonbank": result = MsbExtBank.SAIGONBANK; break;
                case "scb": result = MsbExtBank.SCB; break;
                case "seabank": case "seab": result = MsbExtBank.SEABANK; break;
                case "shb": case "shbbank": result = MsbExtBank.SHB; break;
                case "shinhanbank": result = MsbExtBank.SHINHANBANK; break;
                case "standardcharteredbank": result = MsbExtBank.STANDARDCHARTEREDBANK; break;
                case "svfc": result = MsbExtBank.SVFC; break;
                case "techcomb": case "tcb": case "techcombank": case "techcom": result = MsbExtBank.TECHCOMBANK; break;
                case "tmcpvietnamthinhvuong": result = MsbExtBank.TMCPVIETNAMTHINHVUONG; break;
                case "tpbank": case "tpb": case "tp": result = MsbExtBank.TPBANK; break;
                case "ubank": result = MsbExtBank.UBANK; break;
                case "umeekienlongbank": result = MsbExtBank.UMEEKIENLONGBANK; break;
                case "uob": result = MsbExtBank.UOB; break;
                case "vbsp": result = MsbExtBank.VBSP; break;
                case "vib": case "vibbank": result = MsbExtBank.VIB; break;
                case "vidbank": result = MsbExtBank.VIDBANK; break;
                case "vietabank": result = MsbExtBank.VIETABANK; break;
                case "vietbank": result = MsbExtBank.VIETBANK; break;
                case "vietcomb": case "vcb": case "vietcombank": case "vietcom": result = MsbExtBank.VIETCOMBANK; break;
                case "vietinba": case "vtb": case "vietinbank": case "vietin": case "icb": result = MsbExtBank.VIETINBANK; break;
                case "viettelmoney": result = MsbExtBank.VIETTELMONEY; break;
                case "vnptmoney": result = MsbExtBank.VNPTMONEY; break;
                case "vpbank": case "vpb": result = MsbExtBank.VPBANK; break;
                case "vrb": result = MsbExtBank.VRB; break;
                case "wrbank": case "woo": case "wooribank": result = MsbExtBank.WOORIBANK; break;

                case "maribank": case "msb": case "msbbank": result = MsbExtBank.MSB; break;

                default: break;
            }
            return result;
        }

        public static MsbExtBankModel? GetMsbExtBank(this MsbExtBank bank)
        {
            MsbExtBankModel? result = null;
            switch (bank)
            {
                case MsbExtBank.ABBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "ABBANK - NH TMCP AN BINH (ABB)", }; break;
                case MsbExtBank.ACB: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "ACB - NH TMCP A CHAU", }; break;
                case MsbExtBank.AGRIBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "AGRIBANK - NH NN - PTNT VIET NAM", }; break;
                case MsbExtBank.BAOVIETBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "BAOVIETBANK - NH TMCP BAO VIET (BVB)", }; break;
                case MsbExtBank.BIDC: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "BIDC - NGAN HANG DAU TU VA PHAT TRIEN CAMPUCHIA - CN HA NOI", }; break;
                case MsbExtBank.BIDV: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "BIDV - NH DAU TU VA PHAT TRIEN VIET NAM", }; break;
                case MsbExtBank.BNPPARIBASHCM: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "BNP PARIBAS HCM", }; break;
                case MsbExtBank.BNPPARIBASHN: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "BNP PARIBAS HN", }; break;
                case MsbExtBank.BOCHK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "BOCHK - NGAN HANG BANK OF CHINA (HONGKONG) LIMITED – CN HO CHI MINH", }; break;
                case MsbExtBank.BVBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "BVBANK - NH TMCP BẢN VIỆT", }; break;
                case MsbExtBank.BVBANKTIMO: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "BVBANK TIMO", }; break;
                case MsbExtBank.CATHAYUNITEDBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "CATHAY UNITED BANK", }; break;
                case MsbExtBank.CBBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "CBBANK - NH TM TNHH MTV XAY DUNG VIET NAM", }; break;
                case MsbExtBank.CIMB: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "CIMB -NGAN HANG TNHH MTV CIMB VIET NAM", }; break;
                case MsbExtBank.CITIBANKVIETNAM: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "CITIBANK VIETNAM", }; break;
                case MsbExtBank.COOPBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "COOPBANK-NGAN HANG HOP TAC (CO-OPBANK)", }; break;
                case MsbExtBank.DBS: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "DBS - DBS BANK LTD - CN TP HO CHI MINH", }; break;
                case MsbExtBank.DONGABANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "DONGABANK -NH TMCP DONG A (EAB)", }; break;
                case MsbExtBank.DVCQG: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "DVCQG-CONG DICH VU CONG QUOC GIA", }; break;
                case MsbExtBank.EXIMBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "EXIMBANK - NH TMCP XUAT NHAP KHAU VIET NAM (EIB)", }; break;
                case MsbExtBank.GPBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "GPBANK - NH TMCP DAU KHI TOAN CAU (GPB)", }; break;
                case MsbExtBank.HDBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "HDBANK - NH TMCP PHAT TRIEN TP.HCM (HDB)", }; break;
                case MsbExtBank.HONGLEONGBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "HONGLEONGBANK - NH TNHH MTV HONG LEONG VIET NAM", }; break;
                case MsbExtBank.HSBC: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "HSBC - NH TNHH MTV HSBC VIET NAM", }; break;
                case MsbExtBank.IBK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "IBK-NH CONG NGHIEP HAN QUOC (IBK)", }; break;
                case MsbExtBank.INDOVINABANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "INDOVINABANK - NH TNHH INDOVINA (IVB)", }; break;
                case MsbExtBank.KASIKORNBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "KASIKORNBANK-NH DAI CHUNG TNHH KASIKORNBANK CN TPHCM", }; break;
                case MsbExtBank.KEBHANAHCM: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "KEBHANAHCM", }; break;
                case MsbExtBank.KEBHANAHN: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "KEBHANAHN", }; break;
                case MsbExtBank.KIENLONGBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "KIENLONGBANK - NH TMCP KIEN LONG", }; break;
                case MsbExtBank.KOOKMINBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "KOOKMIN BANK", }; break;
                case MsbExtBank.KOOKMIN: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "KOOKMIN-NGAN HANG KOOKMIN", }; break;
                case MsbExtBank.LIOBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "LIO BANK", }; break;
                case MsbExtBank.LPBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "LPBANK - NGAN HANG TMCP LOC PHAT VIET NAM", }; break;
                case MsbExtBank.MAFC: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "MAFC-CONG TY TAI CHINH TNHH MTV MIRAE ASSET (VIET NAM)", }; break;
                case MsbExtBank.MB: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "MB - NH TMCP QUAN DOI", }; break;
                case MsbExtBank.NAMABANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "NAMABANK - NHTMCP NAM A (NAB)", }; break;
                case MsbExtBank.NASBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "NASBANK - NH TMCP BAC A HA NOI", }; break;
                case MsbExtBank.NCB: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "NCB - NH TMCP QUOC DAN", }; break;
                case MsbExtBank.NONGHYUP: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "NONGHYUP-NGAN HANG NONGHYUP", }; break;
                case MsbExtBank.OCB: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "OCB - NH TMCP PHUONG DONG", }; break;
                case MsbExtBank.OCEANBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "OCEANBANK - NH TMCP DAI DUONG (OJB)", }; break;
                case MsbExtBank.PGBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "PGBANK - NH TMCP XANG DAU PETROLIMEX (PGB)", }; break;
                case MsbExtBank.PVCOMBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "PVCOMBANK - NH TMCP DAI CHUNG VIET NAM", }; break;
                case MsbExtBank.SACOMBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "SACOMBANK - NH TMCP SAI GON THUONG TIN", }; break;
                case MsbExtBank.SAIGONBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "SAIGONBANK - NH TMCP SAI GON CONG THUONG", }; break;
                case MsbExtBank.SCB: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "SCB - NH TMCP SAI GON", }; break;
                case MsbExtBank.SEABANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "SEABANK - NH TMCP DONG NAM A", }; break;
                case MsbExtBank.SHB: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "SHB - NH TMCP SAI GON HA NOI", }; break;
                case MsbExtBank.SHINHANBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "SHINHANBANK - NH TNHH SHINHAN VIET NAM", }; break;
                case MsbExtBank.STANDARDCHARTEREDBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "STANDARD CHARTERED BANK - NH TNHH MTV STANDARD CHARTERED VIET NAM", }; break;
                case MsbExtBank.SVFC: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "SVFC - CONG TY TAI CHINH TNHH MTV SHINHAN VIET NAM", }; break;
                case MsbExtBank.TECHCOMBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "TECHCOMBANK - NH TMCP KY THUONG VIET NAM (TCB)", }; break;
                case MsbExtBank.TMCPVIETNAMTHINHVUONG: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "TMCP VIET NAM THINH VUONG – NGAN HANG SO CAKE BY VPBANK", }; break;
                case MsbExtBank.TPBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "TPBANK - NH TMCP TIEN PHONG", }; break;
                case MsbExtBank.UBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "UBANK BY VPBANK", }; break;
                case MsbExtBank.UMEEKIENLONGBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "UMEE KIENLONGBANK - NH TMCP KIEN LONG", }; break;
                case MsbExtBank.UOB: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "UOB - NH UNITED OVERSEAS BANK", }; break;
                case MsbExtBank.VBSP: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "VBSP - NH CHINH SACH XA HOI VIET NAM", }; break;
                case MsbExtBank.VIB: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "VIB - NH TMCP QUOC TE VIET NAM", }; break;
                case MsbExtBank.VIDBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "VIDBANK - PUBLIC BANK", }; break;
                case MsbExtBank.VIETABANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "VIETABANK - NH TMCP VIET A", }; break;
                case MsbExtBank.VIETBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "VIETBANK - NH TMCP VIET NAM THUONG TIN", }; break;
                case MsbExtBank.VIETCOMBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "VIETCOMBANK -NH TMCP NGOAI THUONG VIET NAM (VCB)", }; break;
                case MsbExtBank.VIETINBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "VIETINBANK - NH TMCP CONG THUONG VIET NAM", }; break;
                case MsbExtBank.VIETTELMONEY: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "VIETTEL MONEY", }; break;
                case MsbExtBank.VNPTMONEY: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "VNPT MONEY", }; break;
                case MsbExtBank.VPBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "VPBANK - NH TMCP VIET NAM THINH VUONG", }; break;
                case MsbExtBank.VRB: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "VRB- NH LIEN DOANH VIET NGA", }; break;
                case MsbExtBank.WOORIBANK: result = new MsbExtBankModel() { Bank = bank, ShortName = "", NameVN = "", NameEN = "WOORI BANK", }; break;

                default: break;
            }
            return result;
        }

        public static string? GetMsbBankName(this MsbExtBank bank)
        {
            return bank.GetMsbExtBank()?.NameEN;
        }

        #endregion
    }
}
