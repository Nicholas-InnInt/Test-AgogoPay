using Neptune.NsPay.BankInfo;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.Utils;
using QRCoder;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Neptune.NsPay.VietQR
{
    public class QRPay
    {
        public bool isValid = true;
        public string version;
        public string initMethod;
        public Provider provider;
        public Merchant merchant;
        public Consumer consumer;
        public string category;
        public string currency;
        public string amount;
        public string tipAndFeeType;
        public string tipAndFeeAmount;
        public string tipAndFeePercent;
        public string nation;
        public string city;
        public string zipCode;
        public AdditionalData additionalData;
        public string crc;
        public Dictionary<string, string> EVMCo;
        public Dictionary<string, string> unreserved;

        public QRPay(string content = "")
        {
            provider = new Provider();
            consumer = new Consumer();
            merchant = new Merchant();
            additionalData = new AdditionalData();
            parse(content ?? "");
        }

        // 定义银行名称到 BankKey 的映射

        private bool invalid()
        {
            this.isValid = false;
            return this.isValid;
        }

        public static bool VerifyCRC(string content)
        {
            string checkContent = content.Substring(0, content.Length - 4);
            string crcCode = content.Substring(content.Length - 4).ToUpper();

            string genCrcCode = QRPay.GenCRCCode(checkContent);
            return crcCode == genCrcCode;
        }

        public static string GenCRCCode(string content)
        {
            int crcCode = CRC16.crc16ccitt(content);
            return $"0000{crcCode:X}".Substring(Math.Max(0, $"0000{crcCode:X}".Length - 4));
        }

        private static (string id, int length, string value, string nextValue) SliceContent(string content)
        {
            string id = content.Substring(0, 2);
            int length = int.Parse(content.Substring(2, 2));
            string value = content.Substring(4, length);
            string nextValue = content.Substring(4 + length);
            return (id, length, value, nextValue);
        }

        private void ParseProviderInfo(string content)
        {
            var (id, length, value, nextValue) = QRPay.SliceContent(content);
            switch (id)
            {
                case ProviderFieldID.GUID:
                    this.provider.guid = value;
                    break;

                case ProviderFieldID.DATA:
                    if (this.provider.guid == QRProviderGUID.VNPAY)
                    {
                        this.provider.name = QRProvider.VNPAY;
                        this.merchant.id = value;
                    }
                    else if (this.provider.guid == QRProviderGUID.VIETQR)
                    {
                        this.provider.name = QRProvider.VIETQR;
                        this.ParseVietQRConsumer(value);
                    }
                    break;

                case ProviderFieldID.SERVICE:
                    this.provider.service = value;
                    break;

                default:
                    break;
            }
            if (nextValue.Length > 4)
                this.ParseProviderInfo(nextValue);
        }

        private void ParseVietQRConsumer(string content)
        {
            var (id, length, value, nextValue) = QRPay.SliceContent(content);
            switch (id)
            {
                case VietQRConsumerFieldID.BANK_BIN:
                    this.consumer.bankBin = value;
                    break;

                case VietQRConsumerFieldID.BANK_NUMBER:
                    this.consumer.bankNumber = value;
                    break;

                default:
                    break;
            }
            if (nextValue.Length > 4)
                this.ParseVietQRConsumer(nextValue);
        }

        private void ParseAdditionalData(string content)
        {
            var (id, length, value, nextValue) = QRPay.SliceContent(content);
            switch (id)
            {
                case AdditionalDataID.PURPOSE_OF_TRANSACTION:
                    this.additionalData.purpose = value;
                    break;

                case AdditionalDataID.BILL_NUMBER:
                    this.additionalData.billNumber = value;
                    break;

                case AdditionalDataID.MOBILE_NUMBER:
                    this.additionalData.mobileNumber = value;
                    break;

                case AdditionalDataID.REFERENCE_LABEL:
                    this.additionalData.reference = value;
                    break;

                case AdditionalDataID.STORE_LABEL:
                    this.additionalData.store = value;
                    break;

                case AdditionalDataID.TERMINAL_LABEL:
                    this.additionalData.terminal = value;
                    break;

                default:
                    break;
            }
            if (nextValue.Length > 4)
                this.ParseAdditionalData(nextValue);
        }

        private bool ParseRootContent(string content)
        {
            var (id, length, value, nextValue) = QRPay.SliceContent(content);
            if (value.Length != length)
                return invalid();
            switch (id)
            {
                case FieldID.VERSION:
                    this.version = value;
                    break;

                case FieldID.INIT_METHOD:
                    this.initMethod = value;
                    break;

                case FieldID.VIETQR:
                case FieldID.VNPAYQR:
                    this.provider.fieldId = id;
                    this.ParseProviderInfo(value);
                    break;

                case FieldID.CATEGORY:
                    this.category = value;
                    break;

                case FieldID.CURRENCY:
                    this.currency = value;
                    break;

                case FieldID.AMOUNT:
                    this.amount = value;
                    break;

                case FieldID.TIP_AND_FEE_TYPE:
                    this.tipAndFeeType = value;
                    break;

                case FieldID.TIP_AND_FEE_AMOUNT:
                    this.tipAndFeeAmount = value;
                    break;

                case FieldID.TIP_AND_FEE_PERCENT:
                    this.tipAndFeePercent = value;
                    break;

                case FieldID.NATION:
                    this.nation = value;
                    break;

                case FieldID.MERCHANT_NAME:
                    this.merchant.name = value;
                    break;

                case FieldID.CITY:
                    this.city = value;
                    break;

                case FieldID.ZIP_CODE:
                    this.zipCode = value;
                    break;

                case FieldID.ADDITIONAL_DATA:
                    this.ParseAdditionalData(value);
                    break;

                case FieldID.CRC:
                    this.crc = value;
                    break;

                default:
                    var idNum = int.Parse(id);
                    if (idNum >= 65 && idNum <= 79)
                    {
                        if (this.EVMCo == null)
                            this.EVMCo = new Dictionary<string, string>();
                        this.EVMCo[id] = value;
                    }
                    else if (idNum >= 80 && idNum <= 99)
                    {
                        if (this.unreserved == null)
                            this.unreserved = new Dictionary<string, string>();
                        this.unreserved[id] = value;
                    }
                    break;
            }
            if (nextValue.Length > 4)
                this.ParseRootContent(nextValue);
            return true;
        }

        private bool parse(string content)
        {
            if (content.Length < 4) return invalid();
            // verify CRC
            var crcValid = QRPay.VerifyCRC(content);
            if (!crcValid) return this.invalid();
            // parse content
            return this.ParseRootContent(content);
        }

        private static string GenFieldData(string id = null, string value = null)
        {
            string fieldId = id ?? "";
            string fieldValue = value ?? "";
            int idLen = fieldId.Length;
            if (idLen != 2 || fieldValue.Length <= 0)
            {
                return "";
            }
            string length = fieldValue.Length.ToString().PadLeft(2, '0');
            return $"{fieldId}{length}{fieldValue}";
        }

        public static QRPay InitVietQR(string bankBin, string bankNumber, string amount = null, string purpose = null, string service = null)
        {
            var qr = new QRPay();
            qr.initMethod = amount != null ? "12" : "11";
            qr.provider.fieldId = FieldID.VIETQR;
            qr.provider.guid = QRProviderGUID.VIETQR;
            qr.provider.name = QRProvider.VIETQR;
            qr.provider.service = service ?? VietQRService.BY_ACCOUNT_NUMBER;
            qr.consumer.bankBin = bankBin;
            qr.consumer.bankNumber = bankNumber;
            qr.amount = amount;
            qr.additionalData.purpose = purpose;
            return qr;
        }

        public static QRPay InitVNPayQR(string merchantId, string merchantName, string store, string terminal, string amount = null, string purpose = null, string billNumber = null, string mobileNumber = null, string loyaltyNumber = null, string reference = null, string customerLabel = null)
        {
            var qr = new QRPay();
            qr.merchant.id = merchantId;
            qr.merchant.name = merchantName;
            qr.provider.fieldId = FieldID.VNPAYQR;
            qr.provider.guid = QRProviderGUID.VNPAY;
            qr.provider.name = QRProvider.VNPAY;
            qr.amount = amount;
            qr.additionalData.purpose = purpose;
            qr.additionalData.billNumber = billNumber;
            qr.additionalData.mobileNumber = mobileNumber;
            qr.additionalData.store = store;
            qr.additionalData.terminal = terminal;
            qr.additionalData.loyaltyNumber = loyaltyNumber;
            qr.additionalData.reference = reference;
            qr.additionalData.customerLabel = customerLabel;
            return qr;
        }

        public string Build()
        {
            string version = QRPay.GenFieldData(FieldID.VERSION, this.version ?? "01");
            string initMethod = QRPay.GenFieldData(FieldID.INIT_METHOD, this.initMethod ?? "11");

            string guid = QRPay.GenFieldData(ProviderFieldID.GUID, this.provider.guid);

            string providerDataContent = "";
            if (this.provider.guid == QRProviderGUID.VIETQR)
            {
                string bankBin = QRPay.GenFieldData(VietQRConsumerFieldID.BANK_BIN, this.consumer.bankBin);
                string bankNumber = QRPay.GenFieldData(VietQRConsumerFieldID.BANK_NUMBER, this.consumer.bankNumber);
                providerDataContent = bankBin + bankNumber;
            }
            else if (this.provider.guid == QRProviderGUID.VNPAY)
            {
                providerDataContent = this.merchant.id ?? "";
            }
            string provider = QRPay.GenFieldData(ProviderFieldID.DATA, providerDataContent);
            string service = QRPay.GenFieldData(ProviderFieldID.SERVICE, this.provider.service);
            string providerData = QRPay.GenFieldData(this.provider.fieldId, guid + provider + service);

            string category = QRPay.GenFieldData(FieldID.CATEGORY, this.category);
            string currency = QRPay.GenFieldData(FieldID.CURRENCY, this.currency ?? "704");
            string amountStr = QRPay.GenFieldData(FieldID.AMOUNT, this.amount);
            string tipAndFeeType = QRPay.GenFieldData(FieldID.TIP_AND_FEE_TYPE, this.tipAndFeeType);
            string tipAndFeeAmount = QRPay.GenFieldData(FieldID.TIP_AND_FEE_AMOUNT, this.tipAndFeeAmount);
            string tipAndFeePercent = QRPay.GenFieldData(FieldID.TIP_AND_FEE_PERCENT, this.tipAndFeePercent);
            string nation = QRPay.GenFieldData(FieldID.NATION, this.nation ?? "VN");
            string merchantName = QRPay.GenFieldData(FieldID.MERCHANT_NAME, this.merchant.name);
            string city = QRPay.GenFieldData(FieldID.CITY, this.city);
            string zipCode = QRPay.GenFieldData(FieldID.ZIP_CODE, this.zipCode);

            string buildNumber = QRPay.GenFieldData(AdditionalDataID.BILL_NUMBER, this.additionalData.billNumber);
            string mobileNumber = QRPay.GenFieldData(AdditionalDataID.MOBILE_NUMBER, this.additionalData.mobileNumber);
            string storeLabel = QRPay.GenFieldData(AdditionalDataID.STORE_LABEL, this.additionalData.store);
            string loyaltyNumber = QRPay.GenFieldData(AdditionalDataID.LOYALTY_NUMBER, this.additionalData.loyaltyNumber);
            string reference = QRPay.GenFieldData(AdditionalDataID.REFERENCE_LABEL, this.additionalData.reference);
            string customerLabel = QRPay.GenFieldData(AdditionalDataID.CUSTOMER_LABEL, this.additionalData.customerLabel);
            string terminal = QRPay.GenFieldData(AdditionalDataID.TERMINAL_LABEL, this.additionalData.terminal);
            string purpose = QRPay.GenFieldData(AdditionalDataID.PURPOSE_OF_TRANSACTION, this.additionalData.purpose);
            string dataRequest = QRPay.GenFieldData(AdditionalDataID.ADDITIONAL_CONSUMER_DATA_REQUEST, this.additionalData.dataRequest);

            string additionalDataContent = buildNumber + mobileNumber + storeLabel + loyaltyNumber + reference + customerLabel + terminal + purpose + dataRequest;
            string additionalData = QRPay.GenFieldData(FieldID.ADDITIONAL_DATA, additionalDataContent);

            // For EVMCo
            string EVMCoContent = string.Join("",
                (this.EVMCo ?? new Dictionary<string, string>()).Keys
                .OrderBy(key => key)
                .Select(key =>
                {
                    this.EVMCo.TryGetValue(key, out string value);
                    return QRPay.GenFieldData(key, value);
                }));

            // For unreserved
            string unreservedContent = string.Join("",
                (this.unreserved ?? new Dictionary<string, string>()).Keys
                .OrderBy(key => key)
                .Select(key =>
                {
                    this.unreserved.TryGetValue(key, out string value);
                    return QRPay.GenFieldData(key, value);
                }));

            string content = $"{version}{initMethod}{providerData}{category}{currency}{amountStr}{tipAndFeeType}{tipAndFeeAmount}{tipAndFeePercent}{nation}{merchantName}{city}{zipCode}{additionalData}{EVMCoContent}{unreservedContent}{FieldID.CRC}04";
            string crc = QRPay.GenCRCCode(content);
            return content + crc;
        }

        public void SetUnreservedField(string id, string value)
        {
            if (this.unreserved == null) this.unreserved = new Dictionary<string, string>();
            this.unreserved[id] = value;
        }

        public static string Findbank(PayMentTypeEnum bankcode)
        {
            return bankcode switch
            {
                PayMentTypeEnum.VietcomBank => BankKey.VIETCOMBANK,
                PayMentTypeEnum.VietinBank => BankKey.VIETINBANK,
                PayMentTypeEnum.BusinessVtbBank => BankKey.VIETINBANK,
                PayMentTypeEnum.TechcomBank => BankKey.TECHCOMBANK,
                PayMentTypeEnum.BusinessTcbBank => BankKey.TECHCOMBANK,
                PayMentTypeEnum.MBBank => BankKey.MBBANK,
                PayMentTypeEnum.BusinessMbBank => BankKey.MBBANK,
                PayMentTypeEnum.ACBBank => BankKey.ACB,
                PayMentTypeEnum.BidvBank => BankKey.BIDV,
                PayMentTypeEnum.PVcomBank => BankKey.PVCOM_BANK,
                PayMentTypeEnum.MsbBank => BankKey.MSB,
                PayMentTypeEnum.SeaBank => BankKey.SEA_BANK,
                PayMentTypeEnum.BvBank => BankKey.BANVIET,
                PayMentTypeEnum.NamaBank => BankKey.NAM_A_BANK,
                PayMentTypeEnum.TPBank => BankKey.TPBANK,
                PayMentTypeEnum.VPBBank => BankKey.VPBANK,
                PayMentTypeEnum.OCBBank => BankKey.OCB,
                PayMentTypeEnum.EXIMBank => BankKey.EXIMBANK,
                PayMentTypeEnum.NCBBank => BankKey.NCB,
                PayMentTypeEnum.HDBank => BankKey.HDBANK,
                PayMentTypeEnum.LPBank => BankKey.LIENVIETPOST_BANK,
                PayMentTypeEnum.PGBank => BankKey.PGBANK,
                PayMentTypeEnum.VietBank => BankKey.VIET_BANK,
                PayMentTypeEnum.BacaBank => BankKey.BAC_A_BANK,
                _ => ""
            };
        }
    }

    public class WithdrawalOrderBankMapper
    {
        private static readonly Dictionary<string, string> defaultDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "ABBANK", BankKey.ABBANK },
            { "ABB", BankKey.ABBANK },
            { "ACBBANK", BankKey.ACB },
            { "ACB", BankKey.ACB },
            { "AGRIBANK", BankKey.AGRIBANK },
            { "AGRIB", BankKey.AGRIBANK },
            { "BACABANK", BankKey.BAC_A_BANK },
            { "BAOVIETBANK", BankKey.BAOVIET_BANK },
            { "BIDV", BankKey.BIDV },
            { "BIDVBANK", BankKey.BIDV },
            { "BVBANK", BankKey.BANVIET },
            { "CAKE", BankKey.CAKE },
            { "COOPBANK", BankKey.COOP_BANK },
            { "DONGABANK", BankKey.DONG_A_BANK },
            { "DBS", BankKey.DBS_BANK },
            { "EXIMBANK", BankKey.EXIMBANK },
            { "GPBANK", BankKey.GPBANK },
            { "HDBANK", BankKey.HDBANK },
            { "HONGLEONGBANK", BankKey.HONGLEONG_BANK },
            { "HSBC", BankKey.HSBC },
            { "IBK", BankKey.IBK_HN },
            { "IVB", BankKey.INDOVINA_BANK },
            { "KBank", BankKey.KASIKORN_BANK },
            { "KIENLONGBANK", BankKey.KIENLONG_BANK },
            { "KOOKMI", BankKey.KOOKMIN_BANK_HN },
            { "LPBANK", BankKey.LIENVIETPOST_BANK },
            { "LIENVIETBANK", BankKey.LIENVIETPOST_BANK },
            { "LIENVIET", BankKey.LIENVIETPOST_BANK },
            { "MBBANK", BankKey.MBBANK },
            { "MB", BankKey.MBBANK },
            { "MBB", BankKey.MBBANK },
            { "MSBBANK", BankKey.MSB },
            { "MSB", BankKey.MSB },
            { "NAMABANK", BankKey.NAM_A_BANK },
            { "NAMA", BankKey.NAM_A_BANK },
            { "NAMABA", BankKey.NAM_A_BANK },
            { "CIMBBANK", BankKey.CIMB },
            { "CBBANK", BankKey.CBBANK },
            { "NCB", BankKey.NCB },
            { "OCBBANK", BankKey.OCB },
            { "OCB", BankKey.OCB },
            { "OCEANBANK", BankKey.OCEANBANK },
            { "PGBANK", BankKey.PGBANK },
            { "PVCOMBANK", BankKey.PVCOM_BANK },
            { "SACOMBANK", BankKey.SACOMBANK },
            { "SAIGONBANK", BankKey.SAIGONBANK },
            { "SCB", BankKey.SCB },
            { "SCBVL", BankKey.STANDARD_CHARTERED_BANK },
            { "SEABANK", BankKey.SEA_BANK },
            { "SHBBANK", BankKey.SHB },
            { "SHB", BankKey.SHB },
            { "SHBVN", BankKey.SHB },
            { "SHINHANBANKVN", BankKey.SHINHAN_BANK },
            { "TECHCOMBANK", BankKey.TECHCOMBANK },
            { "TIMOBANK", BankKey.TIMO },
            { "TPBANK", BankKey.TPBANK },
            { "TP", BankKey.TPBANK },
            { "UOB", BankKey.UNITED_OVERSEAS_BANK },
            { "UBANK", BankKey.UBANK },
            { "VCCB", BankKey.BANVIET },
            { "VIBBANK", BankKey.VIB },
            { "VIB", BankKey.VIB },
            { "VIETBANK", BankKey.VIET_BANK },
            { "VIETCOMBANK", BankKey.VIETCOMBANK },
            { "VIETCAPITALBANK", BankKey.BANVIET },
            { "VIETINBANK", BankKey.VIETINBANK },
            { "VPBANK", BankKey.VPBANK },
            { "VP", BankKey.VPBANK },
            { "VIETABANK", BankKey.VIET_A_BANK },
            { "VRB(VIETNGA)", BankKey.VRB },
            { "WOORI", BankKey.WOORI_BANK },
            { "WOORIBANK", BankKey.WOORI_BANK }
        };

        private static ConcurrentDictionary<string, string> BankNameMapping = new ConcurrentDictionary<string, string>(defaultDict);

        public static void findAllBin()
        {
            var BANKbIN = new Dictionary<string, string>();
            foreach (var bankKey in BankNameMapping.Values.Distinct())
            {
                if (!BANKbIN.ContainsKey(bankKey))
                {
                    BANKbIN.Add(bankKey, BankApp.BanksObject[bankKey].bin);
                }
            }
        }

        public static void UpdateMappingData(Dictionary<string, string> newData)
        {
            foreach (var bankdict in newData)
            {
                if (!BankNameMapping.TryAdd(bankdict.Key, bankdict.Value))
                {
                    BankNameMapping[bankdict.Key] = bankdict.Value;
                }
            }
        }

        public static string FindBankByName(string bankName) // for withdrawal order use
        {
            if (string.IsNullOrWhiteSpace(bankName))
            {
                return ""; // 如果输入为空，返回默认值
            }

            // 清理输入的银行名称
            var cleanedBankName = bankName.ToUpperInvariant().Replace(" ", "").Trim();

            // 尝试从映射中查找对应的 BankKey
            if (BankNameMapping.TryGetValue(cleanedBankName, out var bankKey))
            {
                return bankKey;
            }

            // 如果没有匹配，返回 UNKNOWN
            return "";
        }
    }

    #region CRC16

    public static class CRC16
    {
        private static readonly int[] TABLE = new int[] {
            0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7, 0x8108, 0x9129, 0xA14A, 0xB16B,
            0xC18C, 0xD1AD, 0xE1CE, 0xF1EF, 0x1231, 0x0210, 0x3273, 0x2252, 0x52B5, 0x4294, 0x72F7, 0x62D6,
            0x9339, 0x8318, 0xB37B, 0xA35A, 0xD3BD, 0xC39C, 0xF3FF, 0xE3DE, 0x2462, 0x3443, 0x0420, 0x1401,
            0x64E6, 0x74C7, 0x44A4, 0x5485, 0xA56A, 0xB54B, 0x8528, 0x9509, 0xE5EE, 0xF5CF, 0xC5AC, 0xD58D,
            0x3653, 0x2672, 0x1611, 0x0630, 0x76D7, 0x66F6, 0x5695, 0x46B4, 0xB75B, 0xA77A, 0x9719, 0x8738,
            0xF7DF, 0xE7FE, 0xD79D, 0xC7BC, 0x48C4, 0x58E5, 0x6886, 0x78A7, 0x0840, 0x1861, 0x2802, 0x3823,
            0xC9CC, 0xD9ED, 0xE98E, 0xF9AF, 0x8948, 0x9969, 0xA90A, 0xB92B, 0x5AF5, 0x4AD4, 0x7AB7, 0x6A96,
            0x1A71, 0x0A50, 0x3A33, 0x2A12, 0xDBFD, 0xCBDC, 0xFBBF, 0xEB9E, 0x9B79, 0x8B58, 0xBB3B, 0xAB1A,
            0x6CA6, 0x7C87, 0x4CE4, 0x5CC5, 0x2C22, 0x3C03, 0x0C60, 0x1C41, 0xEDAE, 0xFD8F, 0xCDEC, 0xDDCD,
            0xAD2A, 0xBD0B, 0x8D68, 0x9D49, 0x7E97, 0x6EB6, 0x5ED5, 0x4EF4, 0x3E13, 0x2E32, 0x1E51, 0x0E70,
            0xFF9F, 0xEFBE, 0xDFDD, 0xCFFC, 0xBF1B, 0xAF3A, 0x9F59, 0x8F78, 0x9188, 0x81A9, 0xB1CA, 0xA1EB,
            0xD10C, 0xC12D, 0xF14E, 0xE16F, 0x1080, 0x00A1, 0x30C2, 0x20E3, 0x5004, 0x4025, 0x7046, 0x6067,
            0x83B9, 0x9398, 0xA3FB, 0xB3DA, 0xC33D, 0xD31C, 0xE37F, 0xF35E, 0x02B1, 0x1290, 0x22F3, 0x32D2,
            0x4235, 0x5214, 0x6277, 0x7256, 0xB5EA, 0xA5CB, 0x95A8, 0x8589, 0xF56E, 0xE54F, 0xD52C, 0xC50D,
            0x34E2, 0x24C3, 0x14A0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405, 0xA7DB, 0xB7FA, 0x8799, 0x97B8,
            0xE75F, 0xF77E, 0xC71D, 0xD73C, 0x26D3, 0x36F2, 0x0691, 0x16B0, 0x6657, 0x7676, 0x4615, 0x5634,
            0xD94C, 0xC96D, 0xF90E, 0xE92F, 0x99C8, 0x89E9, 0xB98A, 0xA9AB, 0x5844, 0x4865, 0x7806, 0x6827,
            0x18C0, 0x08E1, 0x3882, 0x28A3, 0xCB7D, 0xDB5C, 0xEB3F, 0xFB1E, 0x8BF9, 0x9BD8, 0xABBB, 0xBB9A,
            0x4A75, 0x5A54, 0x6A37, 0x7A16, 0x0AF1, 0x1AD0, 0x2AB3, 0x3A92, 0xFD2E, 0xED0F, 0xDD6C, 0xCD4D,
            0xBDAA, 0xAD8B, 0x9DE8, 0x8DC9, 0x7C26, 0x6C07, 0x5C64, 0x4C45, 0x3CA2, 0x2C83, 0x1CE0, 0x0CC1,
            0xEF1F, 0xFF3E, 0xCF5D, 0xDF7C, 0xAF9B, 0xBFBA, 0x8FD9, 0x9FF8, 0x6E17, 0x7E36, 0x4E55, 0x5E74,
            0x2E93, 0x3EB2, 0x0ED1, 0x1EF0
        };

        public static int crc16ccitt(string content)
        {
            byte[] current = content.StringToUint8Array();
            int crc = 0xFFFF;
            for (int index = 0; index < current.Length; index++)
            {
                crc = (TABLE[((crc >> 8) ^ current[index]) & 0xFF] ^ (crc << 8)) & 0xFFFF;
            }
            return crc;
        }
    }

    #endregion CRC16

    #region QRCodeHelper

    public class QrCodeHelper
    {
        public static string GetQrCodeAsBase64(string content, string color = "#000000")
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.H);
            Base64QRCode qrCode = new Base64QRCode(qrCodeData);
            string qrCodeImageAsBase64 = qrCode.GetGraphic(20, Color.Black, Color.White, true);
            return qrCodeImageAsBase64;
        }

        public static string GetQrCodeAsBase64New(string content, string color = "#000000")
        {
            // only for bank which is ECCLevel scanner
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.L, true, false, QRCodeGenerator.EciMode.Default, 6);
            Base64QRCode qrCode = new Base64QRCode(qrCodeData);
            string qrCodeImageAsBase64 = qrCode.GetGraphic(20, Color.Black, Color.White, true);
            return qrCodeImageAsBase64;
        }
    }

    #endregion QRCodeHelper
}