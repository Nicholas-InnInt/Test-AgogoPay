using DotNetBrowser.Browser;
using DotNetBrowser.Input.Mouse.Events;
using DotNetBrowser.Input.Mouse;
using Neptune.NsPay.CefTransfer.Common.Classes;
using Neptune.NsPay.CefTransfer.Common.MyEnums;
using Neptune.NsPay.Transfer.Win.Models;
using Newtonsoft.Json;
using System.Text;
using DotNetBrowser.Input.Keyboard.Events;
using DotNetBrowser.Input.Keyboard;
using System.Diagnostics;
using DotNetBrowser.Dom;

namespace Neptune.NsPay.Transfer.Win.Classes
{
    public static class MyJsHelpers
    {
        public static string? ErrorMessage { get; set; }
        private static int ScrollingHeight { get; set; } = 0;
        private static int ScrollDownUpHeight { get; } = 30;

        #region ACB
        public static async Task<string?> JsGetAcbBalance(this IBrowser browser, string accountNumber, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function GetBalance(accountNumber) {
                    var result = null
                    var trList = document.querySelectorAll(""table tr[class*='table-style']"")
                    for (let i = 0; i < trList.length; i++) {
                        var tdList = trList[i].querySelectorAll(""td[class*='table-style']"")
                        if (tdList.length > 3) {
                            // 1st td is Account Number
                            // 3rd td is Balance
                            var accNum = tdList[0].querySelector(""a[class*='acc_bold']"")
                            if (accNum) {
                                var accNumText = accNum.innerText
                                if (accNumText == accountNumber) {
                                    var balance = tdList[2].querySelector(""span[class*='text_bold']"");
                                    if (balance) {
                                        balanceText = balance.innerText
                                        if (balanceText.length > 0) {
                                            result = balanceText
                                            break
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return result
                }
            ");
            js.AppendFormat("\r\n GetBalance({0})", accountNumber);
            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsGetAcbBalance, {accountNumber}, {iTry}, {ex.Message}";
            }
            return null;
        }

        public static async Task<string?> JsGetAcbConfirmTransferBenName(this IBrowser browser, Lang lang, int numOfTry)
        {
            var listId = ".main .content-holder form[name=\"confirm_smlibt_bn\"] .table-form tr td table tr";
            var fieldName = AcbText.BeneficiaryName.AcbTranslate(lang);

            var js = new StringBuilder();
            js.Append(@"
                function GetBenName(listId, fieldName) {
                    const TranslateVnToEn = (vn) => {
                        var result = vn;
                        result = result.replace(/[áàảãạăắằẳẵặâấầẩẫậ]/g, 'a');
                        result = result.replace(/[éèẻẽẹêếềểễệ]/g, 'e');
                        result = result.replace(/[íìỉĩị]/g, 'i');
                        result = result.replace(/[óòỏõọôốồổỗộơớờởỡợ]/g, 'o');
                        result = result.replace(/[úùủũụưứừửữự]/g, 'u');
                        result = result.replace(/[ýỳỷỹỵ]/g, 'y');
                        result = result.replace(/[đ]/g, 'd');
                        result = result.replace(/[ÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬ]/g, 'A');
                        result = result.replace(/[ÉÈẺẼẸÊẾỀỂỄỆ]/g, 'E');
                        result = result.replace(/[ÍÌỈĨỊ]/g, 'I');
                        result = result.replace(/[ÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢ]/g, 'O');
                        result = result.replace(/[ÚÙỦŨỤƯỨỪỬỮỰ]/g, 'U');
                        result = result.replace(/[ÝỲỶỸỴ]/g, 'Y');
                        result = result.replace(/[Đ]/g, 'D');
                        return result;
                    }
                    var result = null
                    var list = document.querySelectorAll(listId);
                    if (list) {
                        for (var i = 0; i < list.length; i++) {
                            var tableRow = list[i].querySelectorAll('td')
                            if (tableRow.length == 2) {
                                var colName = tableRow[0].innerText.toLowerCase()
                                var colValue = tableRow[1]
                                if (TranslateVnToEn(colName.toLowerCase()) == TranslateVnToEn(fieldName.toLowerCase())) {
                                    result = colValue.innerText
                                    break
                                }
                            }
                        }
                    }
                    return result;
                }
            ");
            js.AppendFormat("\r\n GetBenName('{0}', '{1}')", listId, fieldName);
            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsGetAcbConfirmTransferBenName, {iTry}, {ex.Message}";
            }
            return null;
        }

        public static async Task<string?> JsAcbGetAuthMethodPostnIntTransPage(this IBrowser browser, int numOfTry)
        {
            // list = .react-select-container
            // with child id is input #authorizationTypeCode

            var js = new StringBuilder();
            js.Append(@"
                function JsAcbGetAuthMethodPostnIntTransPage() {
                    var result = null;
                    var list = document.querySelectorAll('.react-select-container');
                    if (list.length) {
                        for (let i = 0; i < list.length; i++) {
                            var ctn = list[i].querySelector('#authorizationTypeCode')
                            if (ctn) {
                                var pos = list[i].getBoundingClientRect()
                                result = JSON.stringify({ x: pos.left, y: pos.top })
                                break
                            }                            
                        }
                    }
                    return result;
                }
            ");
            js.AppendFormat("\r\n JsAcbGetAuthMethodPostnIntTransPage()");

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsAcbGetAuthMethodPostnIntTransPage, {iTry}, {ex.Message}";
            }

            return null;
        }

        public static async Task<string?> JsAcbGetAuthSafekeyOtpPostnIntTransPage(this IBrowser browser, string authMethod, int numOfTry)
        {
            // list = .react-select-container
            // with child id is input #authorizationTypeCode
            // container with child option .react-select__option innerText is authMethod

            // Static password + Advance OTP SafeKey | Mật khẩu tĩnh + OTP SafeKey nâng cao
            // Static password + OTP SMS | Mật khẩu tĩnh + OTP SMS

            var js = new StringBuilder();
            js.Append(@"
                function JsAcbGetAuthSafekeyOtpPostnIntTransPage(authMethod) {
                    const TranslateVnToEn = (vn) => {
                            var result = vn;
                            result = result.replace(/[áàảãạăắằẳẵặâấầẩẫậ]/g, 'a');
                            result = result.replace(/[éèẻẽẹêếềểễệ]/g, 'e');
                            result = result.replace(/[íìỉĩị]/g, 'i');
                            result = result.replace(/[óòỏõọôốồổỗộơớờởỡợ]/g, 'o');
                            result = result.replace(/[úùủũụưứừửữự]/g, 'u');
                            result = result.replace(/[ýỳỷỹỵ]/g, 'y');
                            result = result.replace(/[đ]/g, 'd');
                            result = result.replace(/[ÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬ]/g, 'A');
                            result = result.replace(/[ÉÈẺẼẸÊẾỀỂỄỆ]/g, 'E');
                            result = result.replace(/[ÍÌỈĨỊ]/g, 'I');
                            result = result.replace(/[ÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢ]/g, 'O');
                            result = result.replace(/[ÚÙỦŨỤƯỨỪỬỮỰ]/g, 'U');
                            result = result.replace(/[ÝỲỶỸỴ]/g, 'Y');
                            result = result.replace(/[Đ]/g, 'D');
                            return result;
                    }
                    var result = null;
                    var list = document.querySelectorAll('.react-select-container');
                    if (list.length) {
                        for (let i = 0; i < list.length; i++) {
                            var ctn = list[i].querySelector('#authorizationTypeCode')
                            if (ctn) {
                                var opt = list[i].querySelectorAll('.react-select__option');
                                if (opt.length) { 
                                    for (let j = 0; j < opt.length; j++) { 
                                        var auth = opt[j].innerText;
                                        if (TranslateVnToEn(auth.trim().toLowerCase()) == TranslateVnToEn(authMethod.trim().toLowerCase())) { 
                                            var pos = opt[j].getBoundingClientRect()
                                            result = JSON.stringify({ x: pos.left, y: pos.top })
                                            break
                                        }
                                    }
                                }
                            }                            
                        }
                    }
                    return result;
                }
            ");
            js.AppendFormat("\r\n JsAcbGetAuthSafekeyOtpPostnIntTransPage('{0}')", authMethod);

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsAcbGetAuthSafekeyOtpPostnIntTransPage, {iTry}, {ex.Message}";
            }

            return null;
        }

        public static async Task<string?> JsAcbGetErrMsgIntTransPage(this IBrowser browser, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function JsAcbGetErrMsgIntTransPage() {
                    var result = null;
                    var list = document.querySelectorAll('.p-error');
                    if (list.length) {
                        for (let i = 0; i < list.length; i++) {
                            if (result == null) result = list[i].innerText
                            else result += ', ' + list[i].innerText                         
                        }
                    }
                    return result;
                }
            ");
            js.AppendFormat("\r\n JsAcbGetErrMsgIntTransPage()");

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsAcbGetErrMsgIntTransPage, {iTry}, {ex.Message}";
            }

            return null;
        }

        #endregion

        #region SEA
        public static async Task<string?> JsSeaGetErrMsgAtTransPage(this IBrowser browser, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function JsSeaGetErrMsgAtTransPage() {
                    var result = null;
                    var list = document.querySelectorAll('form#ftForm .invalid-feedback');
                    if (list.length) {
                        for (let i = 0; i < list.length; i++) {
                            if (result == null) result = list[i].innerText
                            else result += ', ' + list[i].innerText                         
                        }
                    }
                    return result;
                }
            ");
            js.AppendFormat("\r\n JsSeaGetErrMsgAtTransPage()");

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsSeaGetErrMsgAtTransPage, {iTry}, {ex.Message}";
            }

            return null;
        }

        #endregion

        #region MSB
        public static async Task<string?> JsMsbGetErrMsgAtTransPage(this IBrowser browser, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function JsMsbGetErrMsgAtTransPage() {
                    var result = null;
                    var list = document.querySelectorAll('form .formErrorContent');
                    if (list.length) {
                        for (let i = 0; i < list.length; i++) {
                            if (result == null) result = list[i].innerText
                            else result += ', ' + list[i].innerText                         
                        }
                    }
                    return result;
                }
            ");
            js.AppendFormat("\r\n JsMsbGetErrMsgAtTransPage()");

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsMsbGetErrMsgAtTransPage, {iTry}, {ex.Message}";
            }

            return null;
        }

        #endregion

        #region TCB

        public static async Task<string?> JsTcbGetInputLoginErrorMessage(this IBrowser browser, int numOfTry) // TCB login input errors (user and password)
        {
            // .form-input-helper--error

            var js = new StringBuilder();
            js.Append(@"
                function GetMessage() {
                    var messages = '';
                    var listEle = document.querySelectorAll('.form-input-helper--error');
                    if (listEle.length) {
                        listEle.forEach((ele) => {
                            messages += ele.innerText
                        });
                    }
                    return messages
                }
            ");
            js.AppendFormat("\r\n GetMessage()");

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsTcbGetInputLoginErrorMessage, {iTry}, {ex.Message}";
            }
            return null;
        }

        public static async Task<string?> JsTcbGetBeneficiaryBankPosition(this IBrowser browser, string bankShortName, int numOfTry)
        {
            // list = .bank-list .bank-item
            // foreach get innerText match bankShortName .bank-item__title

            var js = new StringBuilder();
            js.Append(@"
                function JsTcbGetBeneficiaryBankPosition(bankShortName) {
                    var result = null;
                    var list = document.querySelectorAll('.bank-list .bank-item');
                    if (list) {
                        for (let i = 0; i < list.length; i++) {
                            var eleBankShortName = list[i].querySelector('.bank-item__title')
                            if (eleBankShortName) {
                                var bShortName = eleBankShortName.innerText;
                                if (bShortName.trim().toLowerCase() == bankShortName.toLowerCase()) {
                                    var pos = list[i].getBoundingClientRect()
                                    result = JSON.stringify({ x: pos.left, y: pos.top })
                                    break
                                }
                            }                            
                        }
                    }
                    return result;
                }
            ");
            js.AppendFormat("\r\n JsTcbGetBeneficiaryBankPosition('{0}')", bankShortName);

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsTcbGetBeneficiaryBankPosition, {bankShortName}, {iTry}, {ex.Message}";
            }

            return null;
        }

        public static async Task<string?> JsGetTcbMessageFromTransHistory(this IBrowser browser, Lang lang, int numOfTry) // Transaction History details
        {
            var listId = ".transaction-history-template__wrapper .d-flex.flex-column";
            var msgId = ".text-neutrals-black";
            var fieldName = TcbText.Message.TcbTranslate(lang);

            var js = new StringBuilder();
            js.Append(@"
                function GetMessageFromTransHistory(listId, msgId, fieldName) {
                    const TranslateVnToEn = (vn) => {
                            var result = vn;
                            result = result.replace(/[áàảãạăắằẳẵặâấầẩẫậ]/g, 'a');
                            result = result.replace(/[éèẻẽẹêếềểễệ]/g, 'e');
                            result = result.replace(/[íìỉĩị]/g, 'i');
                            result = result.replace(/[óòỏõọôốồổỗộơớờởỡợ]/g, 'o');
                            result = result.replace(/[úùủũụưứừửữự]/g, 'u');
                            result = result.replace(/[ýỳỷỹỵ]/g, 'y');
                            result = result.replace(/[đ]/g, 'd');
                            result = result.replace(/[ÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬ]/g, 'A');
                            result = result.replace(/[ÉÈẺẼẸÊẾỀỂỄỆ]/g, 'E');
                            result = result.replace(/[ÍÌỈĨỊ]/g, 'I');
                            result = result.replace(/[ÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢ]/g, 'O');
                            result = result.replace(/[ÚÙỦŨỤƯỨỪỬỮỰ]/g, 'U');
                            result = result.replace(/[ÝỲỶỸỴ]/g, 'Y');
                            result = result.replace(/[Đ]/g, 'D');
                            return result;
                    }
                    var result = null
                    var list = document.querySelectorAll(listId);
                    if (list) {
                        list.forEach((ele) => {
                            var eleText = TranslateVnToEn(ele.innerText.toLowerCase())
                            if (eleText.startsWith(TranslateVnToEn(fieldName.toLowerCase()))) {
                                var msg = ele.querySelector(msgId);
                                if (msg) {
                                    result = msg.innerText
                                }
                            }
                        });
                    }
                    return result;
                }
            ");
            js.AppendFormat("\r\n GetMessageFromTransHistory('{0}', '{1}', '{2}')", listId, msgId, fieldName);

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsGetTcbMessageFromTransHistory, {iTry}, {ex.Message}";
            }
            return null;
        }

        public static async Task<string?> JsGetTcbTransIdFromTransHistory(this IBrowser browser, Lang lang, int numOfTry) // Transaction History details
        {
            var listId = ".transaction-history-template__wrapper .d-flex";
            var msgId = ".ml-1";
            var fieldName = TcbText.TransactionId.TcbTranslate(lang);

            var js = new StringBuilder();
            js.Append(@"
                function GetTransIdFromTransHistory(listId, msgId, fieldName) {
                    const TranslateVnToEn = (vn) => {
                        var result = vn;
                        result = result.replace(/[áàảãạăắằẳẵặâấầẩẫậ]/g, 'a');
                        result = result.replace(/[éèẻẽẹêếềểễệ]/g, 'e');
                        result = result.replace(/[íìỉĩị]/g, 'i');
                        result = result.replace(/[óòỏõọôốồổỗộơớờởỡợ]/g, 'o');
                        result = result.replace(/[úùủũụưứừửữự]/g, 'u');
                        result = result.replace(/[ýỳỷỹỵ]/g, 'y');
                        result = result.replace(/[đ]/g, 'd');
                        result = result.replace(/[ÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬ]/g, 'A');
                        result = result.replace(/[ÉÈẺẼẸÊẾỀỂỄỆ]/g, 'E');
                        result = result.replace(/[ÍÌỈĨỊ]/g, 'I');
                        result = result.replace(/[ÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢ]/g, 'O');
                        result = result.replace(/[ÚÙỦŨỤƯỨỪỬỮỰ]/g, 'U');
                        result = result.replace(/[ÝỲỶỸỴ]/g, 'Y');
                        result = result.replace(/[Đ]/g, 'D');
                        return result;
                    }
                    var result = null
                    var list = document.querySelectorAll(listId);
                    if (list) {
                        list.forEach((ele) => {
                            var eleText = TranslateVnToEn(ele.innerText.toLowerCase())
                            if (eleText.startsWith(TranslateVnToEn(fieldName.toLowerCase()))) {
                                var msg = ele.querySelector(msgId);
                                if (msg) {
                                    result = msg.innerText
                                }
                            }
                        });
                    }
                    return result;
                }
            ");
            js.AppendFormat("\r\n GetTransIdFromTransHistory('{0}', '{1}', '{2}')", listId, msgId, fieldName);

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsGetTcbTransIdFromTransHistory, {iTry}, {ex.Message}";
            }
            return null;
        }

        public static async Task<string?> JsGetTcbTransactionId(this IBrowser browser, Lang lang, int numOfTry) // Transfer success screen
        {
            var fieldName = TcbText.TransactionId.TcbTranslate(lang);
            var js = new StringBuilder();
            js.Append(@"
                function GetTcbTransactionId(fieldName) {
                    const TranslateVnToEn = (vn) => {
                        var result = vn;
                        result = result.replace(/[áàảãạăắằẳẵặâấầẩẫậ]/g, 'a');
                        result = result.replace(/[éèẻẽẹêếềểễệ]/g, 'e');
                        result = result.replace(/[íìỉĩị]/g, 'i');
                        result = result.replace(/[óòỏõọôốồổỗộơớờởỡợ]/g, 'o');
                        result = result.replace(/[úùủũụưứừửữự]/g, 'u');
                        result = result.replace(/[ýỳỷỹỵ]/g, 'y');
                        result = result.replace(/[đ]/g, 'd');
                        result = result.replace(/[ÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬ]/g, 'A');
                        result = result.replace(/[ÉÈẺẼẸÊẾỀỂỄỆ]/g, 'E');
                        result = result.replace(/[ÍÌỈĨỊ]/g, 'I');
                        result = result.replace(/[ÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢ]/g, 'O');
                        result = result.replace(/[ÚÙỦŨỤƯỨỪỬỮỰ]/g, 'U');
                        result = result.replace(/[ÝỲỶỸỴ]/g, 'Y');
                        result = result.replace(/[Đ]/g, 'D');
                        return result;
                    }
                    var result = null;
                    var tcbSuccessFields = document.querySelectorAll('techcom-transfer-successful .successful__content .successful__leading');
                    if (tcbSuccessFields) {
                        tcbSuccessFields.forEach((field) => {
                            if (TranslateVnToEn(field.innerText.toLowerCase()) == TranslateVnToEn(fieldName.toLowerCase())) {
                                var fieldParent = field.parentNode.closest('div');
                                if (fieldParent) {
                                    var fieldValue = fieldParent.querySelector('.successful__title');
                                    if (fieldValue) {
                                        result = fieldValue.innerText
                                    }
                                }
                            }
                        });
                    }
                    return result;
                }
            ");
            js.AppendFormat("\r\n GetTcbTransactionId('{0}')", fieldName);

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsGetTcbTransactionId, {iTry}, {ex.Message}";
            }
            return null;
        }

        #endregion

        #region VCB
        public static async Task<string?> JsVcbGetInternalTransferBenName(this IBrowser browser, Lang lang, int numOfTry) // Internal Transfer Page 2 Beneficiary Name
        {
            var fieldName = VcbText.BeneficiaryName.VcbTranslate(lang);
            var js = new StringBuilder();
            js.Append(@"
                function GetVcbBeneficiaryName(fieldName) {
                    const TranslateVnToEn = (vn) => {
                        var result = vn;
                        result = result.replace(/[áàảãạăắằẳẵặâấầẩẫậ]/g, 'a');
                        result = result.replace(/[éèẻẽẹêếềểễệ]/g, 'e');
                        result = result.replace(/[íìỉĩị]/g, 'i');
                        result = result.replace(/[óòỏõọôốồổỗộơớờởỡợ]/g, 'o');
                        result = result.replace(/[úùủũụưứừửữự]/g, 'u');
                        result = result.replace(/[ýỳỷỹỵ]/g, 'y');
                        result = result.replace(/[đ]/g, 'd');
                        result = result.replace(/[ÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬ]/g, 'A');
                        result = result.replace(/[ÉÈẺẼẸÊẾỀỂỄỆ]/g, 'E');
                        result = result.replace(/[ÍÌỈĨỊ]/g, 'I');
                        result = result.replace(/[ÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢ]/g, 'O');
                        result = result.replace(/[ÚÙỦŨỤƯỨỪỬỮỰ]/g, 'U');
                        result = result.replace(/[ÝỲỶỸỴ]/g, 'Y');
                        result = result.replace(/[Đ]/g, 'D');
                        return result;
                    }
                    var result = null;
                    var list = document.querySelectorAll('form[id=""Step2""] .list-info-item .table');
                    if (list) {
                        for (let i = 0; i < list.length; i++) {
                            var fldName = list[i].querySelector('.list-info-txt-sub');
                            if (fldName) {
                                if (TranslateVnToEn(fldName.innerText.trim().toLowerCase()) == TranslateVnToEn(fieldName.toLowerCase())) {
                                    var fldValue = list[i].querySelector('.list-info-txt-main');
                                    if (fldValue) {
                                        result = fldValue.innerText.trim()
                                    }
                                    break
                                }
                            }
                        }
                    }
                    return result;
                }
            ");
            js.AppendFormat("\r\n GetVcbBeneficiaryName('{0}')", fieldName);

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsGetVcbTransactionId, {iTry}, {ex.Message}";
            }
            return null;
        }

        public static async Task<string?> JsVcbGetTransactionId(this IBrowser browser, TransferType transType, Lang lang, int numOfTry) // Transfer success screen
        {
            var fieldName = VcbText.TransactionId.VcbTranslate(lang);
            var element = transType == TransferType.Internal ? "form[id=\"Step4\"] .list-info-item .table" : "div[id=\"Step4\"] .list-info-item .table";
            var js = new StringBuilder();
            js.Append(@"
                function GetVcbTransactionId(fieldName, element) {
                    const TranslateVnToEn = (vn) => {
                        var result = vn;
                        result = result.replace(/[áàảãạăắằẳẵặâấầẩẫậ]/g, 'a');
                        result = result.replace(/[éèẻẽẹêếềểễệ]/g, 'e');
                        result = result.replace(/[íìỉĩị]/g, 'i');
                        result = result.replace(/[óòỏõọôốồổỗộơớờởỡợ]/g, 'o');
                        result = result.replace(/[úùủũụưứừửữự]/g, 'u');
                        result = result.replace(/[ýỳỷỹỵ]/g, 'y');
                        result = result.replace(/[đ]/g, 'd');
                        result = result.replace(/[ÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬ]/g, 'A');
                        result = result.replace(/[ÉÈẺẼẸÊẾỀỂỄỆ]/g, 'E');
                        result = result.replace(/[ÍÌỈĨỊ]/g, 'I');
                        result = result.replace(/[ÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢ]/g, 'O');
                        result = result.replace(/[ÚÙỦŨỤƯỨỪỬỮỰ]/g, 'U');
                        result = result.replace(/[ÝỲỶỸỴ]/g, 'Y');
                        result = result.replace(/[Đ]/g, 'D');
                        return result;
                    }
                    var result = null;
                    var list = document.querySelectorAll(element);
                    if (list) {
                        for (let i = 0; i < list.length; i++) {
                            var fldName = list[i].querySelector('.list-info-txt-sub');
                            if (fldName) {
                                if (TranslateVnToEn(fldName.innerText.trim().toLowerCase()) == TranslateVnToEn(fieldName.toLowerCase())) {
                                    var fldValue = list[i].querySelector('.list-info-txt-main');
                                    if (fldValue) {
                                        result = fldValue.innerText.trim()
                                    }
                                    break
                                }
                            }
                        }
                    }
                    return result;
                }
            ");
            js.AppendFormat("\r\n GetVcbTransactionId('{0}', '{1}')", fieldName, element);

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsGetVcbTransactionId, {iTry}, {ex.Message}";
            }
            return null;
        }

        public static async Task<string?> JsVcbGetBeneficiaryBankPosition(this IBrowser browser, string bankName, int numOfTry)
        {
            // list = ul.select2-results__options
            // foreach get innerText match bankName

            var js = new StringBuilder();
            js.Append(@"
                function JsVcbGetBeneficiaryBankPosition(bankName) {
                    var result = null;
                    var list = document.querySelectorAll('ul.select2-results__options li');
                    if (list) {
                        for (let i = 0; i < list.length; i++) {
                            var eleBankName = list[i].querySelector('.note')
                            if (eleBankName) {
                                var bName = eleBankName.innerText;
                                if (bName.trim().toLowerCase() == bankName.toLowerCase()) {
                                    var pos = list[i].getBoundingClientRect()
                                    result = JSON.stringify({ x: pos.left, y: pos.top })
                                    break
                                }
                            }                            
                        }
                    }
                    return result;
                }
            ");
            js.AppendFormat("\r\n JsVcbGetBeneficiaryBankPosition('{0}')", bankName);

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsVcbGetBeneficiaryBankPosition, {bankName}, {iTry}, {ex.Message}";
            }

            return null;
        }

        public static async Task<string?> JsVcbGetInputTransferBalance(this IBrowser browser, Lang lang, int numOfTry)
        {
            // form[id='Step1'] .box .form-group label.input-label : text = 'Available balance'
            var fieldName = VcbText.AvailableBalance.VcbTranslate(lang);
            var js = new StringBuilder();
            js.Append(@"
                function GetBalance(fieldName) {
                    const TranslateVnToEn = (vn) => {
                        var result = vn;
                        result = result.replace(/[áàảãạăắằẳẵặâấầẩẫậ]/g, 'a');
                        result = result.replace(/[éèẻẽẹêếềểễệ]/g, 'e');
                        result = result.replace(/[íìỉĩị]/g, 'i');
                        result = result.replace(/[óòỏõọôốồổỗộơớờởỡợ]/g, 'o');
                        result = result.replace(/[úùủũụưứừửữự]/g, 'u');
                        result = result.replace(/[ýỳỷỹỵ]/g, 'y');
                        result = result.replace(/[đ]/g, 'd');
                        result = result.replace(/[ÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬ]/g, 'A');
                        result = result.replace(/[ÉÈẺẼẸÊẾỀỂỄỆ]/g, 'E');
                        result = result.replace(/[ÍÌỈĨỊ]/g, 'I');
                        result = result.replace(/[ÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢ]/g, 'O');
                        result = result.replace(/[ÚÙỦŨỤƯỨỪỬỮỰ]/g, 'U');
                        result = result.replace(/[ÝỲỶỸỴ]/g, 'Y');
                        result = result.replace(/[Đ]/g, 'D');
                        return result;
                    }
                    var balance = null;
                    var list = document.querySelectorAll('form[id=""Step1""] .box .form-group label.input-label');
                    if (list.length) {
                        list.forEach((ele) => {
                            if (TranslateVnToEn(ele.innerText.trim().toLowerCase()) == TranslateVnToEn(fieldName.toLowerCase())) {
                                var par = ele.parentNode;
                                var bal = par.querySelector('.input-group');
                                if (bal) {
                                    balance = bal.innerText
                                }
                            }
                        });
                    }
                    return balance
                }
            ");
            js.AppendFormat("\r\n GetBalance('{0}')", fieldName);

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsVcbGetInputTransferErrorMessage, {iTry}, {ex.Message}";
            }
            return null;
        }

        public static async Task<string?> JsVcbGetInputTransferErrorMessage(this IBrowser browser, int numOfTry)
        {
            // .parsley-errors-list

            var js = new StringBuilder();
            js.Append(@"
                function GetMessage() {
                    var messages = '';
                    var listEle = document.querySelectorAll('.parsley-errors-list');
                    if (listEle.length) {
                        listEle.forEach((ele) => {
                            messages += ele.innerText
                        });
                    }
                    return messages
                }
            ");
            js.AppendFormat("\r\n GetMessage()");

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsVcbGetInputTransferErrorMessage, {iTry}, {ex.Message}";
            }
            return null;
        }

        #endregion

        #region VTB
        public static async Task<string?> JsVtbGetBalanceAtTransferPage(this IBrowser browser, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function GetBalance() {
                    var result = null
                    var balance = document.querySelector(""#app .payment-layout__body .select-number--inner-item"")
                    if (balance) {
                        var bCol = balance.querySelectorAll(""span"");
                        if (bCol.length == 2) {
                            var balanceText = bCol[1].innerText
                            result = balanceText
                        }
                    }

                    return result
                }
            ");
            js.AppendFormat("\r\n GetBalance()");
            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsVtbGetBalanceAtTransferPage, {iTry}, {ex.Message}";
            }
            return null;
        }

        public static async Task<string?> JsVtbGetBeneficiaryBankPosition(this IBrowser browser, string bankName, int numOfTry)
        {
            // list = .bank-list .bank-item
            // foreach get innerText match bankShortName .bank-item__title

            var js = new StringBuilder();
            js.Append(@"
                function JsVtbGetBeneficiaryBankPosition(bankName) {
                    const TranslateVnToEn = (vn) => {
                            var result = vn;
                            result = result.replace(/[áàảãạăắằẳẵặâấầẩẫậ]/g, 'a');
                            result = result.replace(/[éèẻẽẹêếềểễệ]/g, 'e');
                            result = result.replace(/[íìỉĩị]/g, 'i');
                            result = result.replace(/[óòỏõọôốồổỗộơớờởỡợ]/g, 'o');
                            result = result.replace(/[úùủũụưứừửữự]/g, 'u');
                            result = result.replace(/[ýỳỷỹỵ]/g, 'y');
                            result = result.replace(/[đ]/g, 'd');
                            result = result.replace(/[ÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬ]/g, 'A');
                            result = result.replace(/[ÉÈẺẼẸÊẾỀỂỄỆ]/g, 'E');
                            result = result.replace(/[ÍÌỈĨỊ]/g, 'I');
                            result = result.replace(/[ÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢ]/g, 'O');
                            result = result.replace(/[ÚÙỦŨỤƯỨỪỬỮỰ]/g, 'U');
                            result = result.replace(/[ÝỲỶỸỴ]/g, 'Y');
                            result = result.replace(/[Đ]/g, 'D');
                            return result;
                    }
                    var result = null;
                    var list = document.querySelectorAll('.wrap-input-select .vt-select__menu .vt-select__menu-list .vt-select__option');
                    if (list.length) {
                        for (let i = 0; i < list.length; i++) {
                            var bName = list[i].innerText;
                            if (TranslateVnToEn(bName.trim().toLowerCase()) == TranslateVnToEn(bankName.toLowerCase())) {
                                var pos = list[i].getBoundingClientRect()
                                result = JSON.stringify({ x: pos.left, y: pos.top })
                                break
                            }
                        }
                    }
                    return result;
                }
            ");
            js.AppendFormat("\r\n JsVtbGetBeneficiaryBankPosition('{0}')", bankName);

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsVtbGetBeneficiaryBankPosition, {bankName}, {iTry}, {ex.Message}";
            }

            return null;
        }

        public static async Task<string?> JsVtbGetBeneficiaryNameAtTransferPage(this IBrowser browser, TransferType transType, Lang lang, int numOfTry)
        {
            // #app .transfer-external .payment-layout__body .pl-input-group__row
            // .pl-input-group__col
            var listId = transType == TransferType.Internal ?
                "#app .transfer-internal .payment-layout__body .pl-input-group__row" :
                "#app .transfer-external .payment-layout__body .pl-input-group__row";
            var fieldName = VtbText.BeneficiaryName.VtbTranslate(lang);

            var js = new StringBuilder();
            js.Append(@"
                function GetBenName(listId, fieldName) {
                    const TranslateVnToEn = (vn) => {
                        var result = vn;
                        result = result.replace(/[áàảãạăắằẳẵặâấầẩẫậ]/g, 'a');
                        result = result.replace(/[éèẻẽẹêếềểễệ]/g, 'e');
                        result = result.replace(/[íìỉĩị]/g, 'i');
                        result = result.replace(/[óòỏõọôốồổỗộơớờởỡợ]/g, 'o');
                        result = result.replace(/[úùủũụưứừửữự]/g, 'u');
                        result = result.replace(/[ýỳỷỹỵ]/g, 'y');
                        result = result.replace(/[đ]/g, 'd');
                        result = result.replace(/[ÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬ]/g, 'A');
                        result = result.replace(/[ÉÈẺẼẸÊẾỀỂỄỆ]/g, 'E');
                        result = result.replace(/[ÍÌỈĨỊ]/g, 'I');
                        result = result.replace(/[ÓÒỎÕỌÔỐỒỔỖỘƠỚỜỞỠỢ]/g, 'O');
                        result = result.replace(/[ÚÙỦŨỤƯỨỪỬỮỰ]/g, 'U');
                        result = result.replace(/[ÝỲỶỸỴ]/g, 'Y');
                        result = result.replace(/[Đ]/g, 'D');
                        return result;
                    }
                    var result = null
                    var list = document.querySelectorAll(listId);
                    if (list.length) {
                        for (var i = 0; i < list.length; i++) {
                            var tableRow = list[i].querySelectorAll('.pl-input-group__col')
                            if (tableRow.length == 2) {
                                var colName = tableRow[0].innerText
                                var colValue = tableRow[1].innerText
                                if (TranslateVnToEn(colName.toLowerCase()) == TranslateVnToEn(fieldName.toLowerCase())) {
                                    result = colValue
                                    break
                                }
                            }
                        }
                    }
                    return result;
                }
            ");
            js.AppendFormat("\r\n GetBenName('{0}', '{1}')", listId, fieldName);
            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsVtbGetBeneficiaryNameAtTransferPage, {iTry}, {ex.Message}";
            }
            return null;
        }


        #endregion

        #region Common

        private static async Task<string> GetScrollElement(this IBrowser browser, Bank bank)
        {
            var scrollElement = string.Empty;
            switch (bank)
            {
                case Bank.VtbBank:
                    if (browser.Url.ToLower().StartsWith("https://ipay.vietinbank.vn/login"))
                    {
                        var modalId = ".modal.modal-show";
                        var modalExists = await browser.JsElementExists(modalId, 1);
                        if (modalExists.BoolResult)
                        {
                            scrollElement = modalId;
                            break;
                        }
                        scrollElement = "#app .app-container .wrap-layout";
                    }
                    else
                        scrollElement = "body";
                    break;
            }
            return scrollElement;
        }

        public static async Task<bool> JsHasSelected(this IBrowser browser, string element, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function HasSelected(element) {
                    var ele = document.querySelector(element);
                    if (ele) {
                        return ele.selectedIndex > -1
                    }
                    return false
                }
            ");
            js.AppendFormat("\r\n HasSelected('{0}')", element);

            var iTry = 0;
            var result = false;
            try
            {
                while (!result && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptBoolean(js.ToString());

                    if (!result)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsHasSelected, {element}, {iTry}, {ex.Message}";
            }
            return false;
        }

        public static async Task<string?> JsGetSelectText(this IBrowser browser, string element, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function JsGetSelectText(element) {
                    var ele = document.querySelector(element);
                    if (ele) {
                        return ele.options[ele.selectedIndex].text
                    }
                    return null
                }
            ");
            js.AppendFormat("\r\n JsGetSelectText('{0}');", element);

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsGetSelectText, {element}, {iTry}, {ex.Message}";
            }
            return null;
        }

        public static async Task<string?> JsGetElementDisplay(this IBrowser browser, string element, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function GetElementDisplay(element) {
                    var ele = document.querySelector(element);
                    if (ele) {
                        return ele.style.display
                    }
                    return null
                }
            ");
            js.AppendFormat("\r\n GetElementDisplay('{0}');", element);

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsGetElementDisplay, {element}, {iTry}, {ex.Message}";
            }
            return null;
        }

        public static async Task<string?> JsGetElementOpacity(this IBrowser browser, string element, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function JsGetElementOpacity(element) {
                    var ele = document.querySelector(element);
                    if (ele) {
                        return ele.style.opacity
                    }
                    return null
                }
            ");
            js.AppendFormat("\r\n JsGetElementOpacity('{0}');", element);

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsGetElementOpacity, {element}, {iTry}, {ex.Message}";
            }
            return null;
        }

        public static async Task<JsResultModel> JsReloadAsync(this IBrowser browser, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function ReloadPage() {
                    location.reload()
                }
            ");
            js.AppendFormat("\r\n ReloadPage()");

            var iTry = 0;
            var result = false;
            var result2 = new JsResultModel();
            try
            {
                while (!result && iTry < numOfTry)
                {
                    //var w = browser.RegisterWait();
                    result = await browser.JsExecuteScriptVoid(js.ToString());
                    //await w.WaitAsync();

                    if (!result)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                result2.IsSuccess = true;
                result2.BoolResult = result;
                return result2;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsReloadAsync, {iTry}, {ex.Message}";
                result2.ErrorMessage = $"Error JsReloadAsync, {iTry}, {ex.Message}";
            }
            return result2;
        }

        public static async Task<bool> JsScrollToElement(this IBrowser browser, Bank bank, string element, int headerOffset, int numOfTry)
        {
            var scrollElement = await browser.GetScrollElement(bank);

            var js = new StringBuilder();
            if (string.IsNullOrEmpty(scrollElement))
            {
                js.Append(@"
                    function ScrollTo(element, headerOffset) {
                        var ele = document.querySelector(element);
                        if (ele) {
                            var elePost = ele.getBoundingClientRect().top
                            var offsetPosition = elePost + window.pageYOffset - headerOffset;
                        
                            window.scrollTo({
                                top: offsetPosition,
                                behavior: ""smooth""
                            })
                        }
                    }
                ");
                js.AppendFormat("\r\n ScrollTo('{0}', {1})", element, headerOffset);
            }
            else
            {
                js.Append(@"
                    function ScrollTo(element, headerOffset, scrollElement) {
                        var ele = document.querySelector(element)
                        var sEle = document.querySelector(scrollElement)
                        if (ele) {
                            var elePost = ele.getBoundingClientRect().top
		                    if (sEle) {
			                    var offsetPosition = elePost + sEle.scrollTop - headerOffset
			
			                    sEle.scrollTo({
				                    top: offsetPosition,
				                    behavior: ""smooth""
			                    })
		                    }
                        }
                    }
                ");
                js.AppendFormat("\r\n ScrollTo('{0}', {1}, '{2}')", element, headerOffset, scrollElement);
            }

            var iTry = 0;
            var result = false;
            try
            {
                browser.Focus();

                while (!result && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptVoid(js.ToString());

                    if (!result)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }

                if (result) // success scrolled, wait 3 second until the element available on viewport
                {
                    await Task.Delay(1000);

                    var vTry = 0;
                    var vTryMax = 3;
                    while (true)
                    {
                        vTry++;

                        var visible = await browser.JsIsElementVisibleInViewport(element, 1);
                        if (visible)
                            break;
                        else if (vTry >= vTryMax)
                        {
                            result = false;
                            break;
                        }
                        else
                            await Task.Delay(1000);
                    }
                }

                Debug.WriteLine($"JsScrollToElement result: {result}");
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsScrollToElement, {element}, {iTry}, {ex.Message}";
                Debug.WriteLine(ErrorMessage);
            }
            return false;
        }

        public static async Task<bool> JsScrollToJsPosition(this IBrowser browser, Bank bank, string jsPosition, int numOfTry)
        {
            var jsonPosition = JsonConvert.DeserializeObject<dynamic>(jsPosition);
            var yPosition = (int)jsonPosition?["y"];

            var scrollElement = await browser.GetScrollElement(bank);

            var js = new StringBuilder();
            if (string.IsNullOrEmpty(scrollElement))
            {
                js.Append(@"
                    function ScrollTo(yPosition) {
                        window.scrollTo({
                            top: yPosition + window.pageYOffset,
                            behavior: ""smooth""
                        })
                    }
                ");
                js.AppendFormat("\r\n ScrollTo({0})", yPosition);
            }
            else // Not tested
            {
                js.Append(@"
                    function ScrollTo(yPosition, scrollElement) {
                        var sEle = document.querySelector(scrollElement)
		                if (sEle) {
			                sEle.scrollTo({
				                top: yPosition,
				                behavior: ""smooth""
			                })
		                }
                    }
                ");
                js.AppendFormat("\r\n ScrollTo({0}, '{1}')", yPosition, scrollElement);
            }

            var iTry = 0;
            var result = false;
            try
            {
                browser.Focus();

                while (!result && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptVoid(js.ToString());

                    if (!result)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsScrollToJsPosition, {jsPosition}, {iTry}, {ex.Message}";
            }
            return false;
        }

        public static async Task<bool> JsScrollToY(this IBrowser browser, Bank bank, int yPosition, int numOfTry)
        {
            var scrollElement = await browser.GetScrollElement(bank);

            var js = new StringBuilder();
            if (string.IsNullOrEmpty(scrollElement))
            {
                js.Append(@"
                    function ScrollTo(yPosition) {
                        window.scrollTo({
                            top: yPosition,
                            behavior: ""smooth""
                        })
                    }
                ");
                js.AppendFormat("\r\n ScrollTo({0})", yPosition);
            }
            else
            {
                js.Append(@"
                    function ScrollTo(yPosition, scrollElement) {
                        var sEle = document.querySelector(scrollElement)
		                if (sEle) {
			                sEle.scrollTo({
				                top: yPosition,
				                behavior: ""smooth""
			                })
		                }
                    }
                ");
                js.AppendFormat("\r\n ScrollTo({0}, '{1}')", yPosition, scrollElement);
            }

            var iTry = 0;
            var result = false;
            try
            {
                browser.Focus();

                while (!result && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptVoid(js.ToString());

                    if (!result)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsScrollToY, {yPosition}, {iTry}, {ex.Message}";
            }
            return false;
        }

        public static async Task<bool> JsClickElementAtIndex(this IBrowser browser, string element, int index, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function ClickElement(element, index) {
                    var list = document.querySelectorAll(element);
                    if (list) {
                        var i = 0
                        list.forEach(ele => {
                            if (index == i){
                                ele.click()
                            }
                            i++;
                        });
                    }
                }
            ");
            js.AppendFormat("\r\n ClickElement('{0}', {1})", element, index);

            var iTry = 0;
            var result = false;
            try
            {
                while (!result && iTry < numOfTry)
                {
                    //var w = browser.RegisterWait();
                    result = await browser.JsExecuteScriptVoid(js.ToString());
                    //await w.WaitAsync();

                    if (!result)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsClickElement, {element}, {iTry}, {ex.Message}";
            }
            return false;
        }

        public static async Task<string?> JsGetElementCount(this IBrowser browser, string element, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function GetCount(element) {
                    var ele = document.querySelectorAll(element);
                    if (ele) {
                        return ele.length.toString()
                    }
                    return '0'
                }
            ");
            js.AppendFormat("\r\n GetCount('{0}')", element);

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsGetElementCount, {element}, {iTry}, {ex.Message}";
            }
            return null;
        }

        public static async Task<bool> JsElementDisabled(this IBrowser browser, string element, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function ElementDisabled(element) {
                    var ele = document.querySelector(element);
                    if (ele) {
                        return ele.disabled
                    }
                    return true
                }
            ");
            js.AppendFormat("\r\n ElementDisabled('{0}')", element);

            var iTry = 0;
            var result = false;
            try
            {
                while (!result && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptBoolean(js.ToString());

                    if (!result)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsElementDisabled, {element}, {iTry}, {ex.Message}";
            }
            return false;
        }

        public static async Task<string?> JsGetElementPosition(this IBrowser browser, string element, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function ElementPosition(element) {
                    var ele = document.querySelector(element);
                    if (ele) {
                        var pos = ele.getBoundingClientRect()
                        return JSON.stringify({ x: pos.left, y: pos.top })
                    }
                    return null
                }
            ");
            js.AppendFormat("\r\n ElementPosition('{0}')", element);

            var iTry = 0;
            string? jsPosition = null;
            try
            {
                while (jsPosition == null && iTry < numOfTry)
                {
                    jsPosition = await browser.JsExecuteScriptString(js.ToString());

                    if (jsPosition == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return jsPosition;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsGetElementPosition, {element}, {iTry}, {ex.Message}";
            }
            return null;
        }

        public static async Task<bool> MouseClickAndSetCursorActiveJsScroll(this IBrowser browser, Bank bank, string element, int headerOffset, int numOfTry, int dpi)
        {
            var clicked = false;
            var iTry = 0;
            while (iTry < numOfTry)
            {
                var scrollTo = await browser.JsScrollToElement(bank, element, headerOffset, 1);

                clicked = await browser.JsIsActiveElement(element, 1);
                if (!clicked)
                {
                    await browser.MouseClickEventJsScroll(bank, element, headerOffset, 1, dpi);
                    clicked = await browser.JsIsActiveElement(element, 1);
                }
                else
                {
                    break;
                }

                if (!clicked)
                {
                    await WaitOneSecondsAsync(iTry, numOfTry);
                }
                else break;

                iTry++;
            }
            return clicked;
        }

        public static async Task<bool> InputToElementJsScroll(this IBrowser browser, Bank bank, string element, string value, int headerOffset, int numOfTry, int dpi, bool compareComma = true, bool sendTabKey = false)
        {
            var result = false;
            var clicked = false;
            var iTry = 0;
            while (iTry < numOfTry)
            {
                var scrollTo = await browser.JsScrollToElement(bank, element, headerOffset, 1);

                clicked = await browser.JsIsActiveElement(element, 1);
                if (!clicked)
                {
                    await browser.MouseClickEventJsScroll(bank, element, headerOffset, 1, dpi);
                    clicked = await browser.JsIsActiveElement(element, 1);
                }
                else
                {
                    break;
                }

                if (!clicked)
                {
                    await WaitOneSecondsAsync(iTry, numOfTry);
                }
                else break;

                iTry++;
            }

            if (clicked)
            {
                result = await browser.SendInputKeyEvent(value, element, numOfTry, compareComma, sendTabKey);
            }

            return result;
        }

        public static async Task<bool> InputToElement(this IBrowser browser, string element, string value, int headerOffset, int numOfTry, int dpi, bool compareComma = true, bool sendTabKey = false)
        {
            var result = false;
            var clicked = false;
            var iTry = 0;
            while (iTry < numOfTry)
            {
                var scrollTo = await browser.MouseWheelEvent(element, headerOffset, 2, dpi);

                clicked = await browser.JsIsActiveElement(element, 1);
                if (!clicked)
                {
                    await browser.MouseClickEvent(element, headerOffset, 1, dpi);
                    clicked = await browser.JsIsActiveElement(element, 1);
                }
                else
                {
                    break;
                }

                if (!clicked)
                {
                    await WaitOneSecondsAsync(iTry, numOfTry);
                }
                else break;

                iTry++;
            }

            if (clicked)
            {
                result = await browser.SendInputKeyEvent(value, element, numOfTry, compareComma, sendTabKey);
            }

            return result;
        }

        public static bool MouseClickJsPosition(this IBrowser browser, string jsPosition, int numOfTry, int dpi, int x = 5, int y = 5)
        {
            var iTry = 0;
            try
            {
                if (jsPosition != null)
                {
                    var jsonPosition = JsonConvert.DeserializeObject<dynamic>(jsPosition);
                    var xPosition = (int)jsonPosition?["x"] + x;  // add +5 pixel to the click position
                    var yPosition = (int)jsonPosition?["y"] + y;

                    IMouse mouse = browser.Mouse;
                    MouseButton mouseButton = MouseButton.Left;
                    DotNetBrowser.Geometry.Point location = new DotNetBrowser.Geometry.Point(xPosition, yPosition).ToDpiPoint(dpi);

                    MouseMovedEventArgs moveEvent = new MouseMovedEventArgs
                    {
                        Location = location,
                    };
                    MousePressedEventArgs pressEvent = new MousePressedEventArgs
                    {
                        Location = location,
                        Button = mouseButton,
                        ClickCount = 1
                    };
                    MouseReleasedEventArgs releaseEvent = new MouseReleasedEventArgs
                    {
                        Button = mouseButton,
                        ClickCount = 1,
                        Location = location
                    };

#if DEBUG
                    IDocument document = browser.MainFrame.Document;
                    document.DocumentElement.Events.Click += (sender, e) =>
                    {
                        // Mouse click event has been received.
                        var mouseEvent = (DotNetBrowser.Dom.Events.IMouseEvent)e.Event;
                        Debug.WriteLine($"MouseClicked - ClientLocation: {mouseEvent.ClientLocation} OffsetLocation: {mouseEvent.OffsetLocation} ScreenLocation: {mouseEvent.ScreenLocation}");
                    };
#endif
                    mouse.Moved.Raise(moveEvent);
                    Thread.Sleep(200);
                    mouse.Pressed.Raise(pressEvent);
                    Thread.Sleep(200);
                    mouse.Released.Raise(releaseEvent);
                    Thread.Sleep(200);

                    return true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error MouseClickJsPosition, {jsPosition}, {iTry}, {ex.Message}";
            }
            return false;
        }

        //public static async Task<bool> MouseWheelToScrollY(this IBrowser browser, Bank bank, int scrollY, int headerOffset, int numOfTry, int x = 1, int y = 1)
        //{
        //    var iTry = 0;
        //    try
        //    {
        //        while (iTry < numOfTry) // Tries
        //        {
        //            // Scroll down/up to calculate the measurement
        //            int downOrUp = -1;
        //            int initY;
        //            int afterY;
        //            int diff;

        //            while (true) // Evaluate Wheel
        //            {
        //                // Get the init position
        //                var initPos = browser.JsGetWindowScrollYPosition(bank);

        //                // Mouse move to (1,1) then scroll down 30
        //                browser.Focus();
        //                IMouse mouse = browser.Mouse;
        //                DotNetBrowser.Geometry.Point location = new DotNetBrowser.Geometry.Point(x, y);
        //                MouseMovedEventArgs moveEvent = new MouseMovedEventArgs
        //                {
        //                    Location = location,
        //                };
        //                MouseWheelMovedEventArgs wheelEvent_1 = new MouseWheelMovedEventArgs
        //                {
        //                    DeltaX = 0,
        //                    DeltaY = 30 * downOrUp,
        //                    Location = location,
        //                    LocationOnScreen = location,
        //                    //Modifiers = new KeyModifiers(),
        //                    ScrollType = MouseScrollType.UnitScroll,
        //                };
        //                MouseWheelMovedEventArgs wheelEvent_2 = new MouseWheelMovedEventArgs
        //                {
        //                    DeltaX = 0,
        //                    DeltaY = 30 * downOrUp * -1,
        //                    Location = location,
        //                    LocationOnScreen = location,
        //                    //Modifiers = new KeyModifiers(),
        //                    ScrollType = MouseScrollType.UnitScroll,
        //                };

        //                mouse.Moved.Raise(moveEvent);
        //                Thread.Sleep(200);
        //                mouse.WheelMoved.Raise(wheelEvent_1);
        //                Thread.Sleep(500);

        //                // Get the position after scroll
        //                var afterPos = browser.JsGetWindowScrollYPosition(bank);

        //                // Get the vertical diff and find the ratio of scroll
        //                initY = (int)initPos - headerOffset;
        //                afterY = (int)afterPos - headerOffset;

        //                diff = initY - afterY;

        //                if (downOrUp == 1) break;

        //                if (diff == 0)
        //                    downOrUp = 1;
        //                else break;
        //            }

        //            if (diff != 0) // Wheel
        //            {
        //                float actualDeltaY = ((float)scrollY / (float)diff * ((float)30 * (float)downOrUp));

        //                browser.Focus();
        //                IMouse mouse = browser.Mouse;
        //                DotNetBrowser.Geometry.Point location = new DotNetBrowser.Geometry.Point(x, y);
        //                MouseMovedEventArgs moveEvent = new MouseMovedEventArgs
        //                {
        //                    Location = location,
        //                };
        //                MouseWheelMovedEventArgs wheelEvent = new MouseWheelMovedEventArgs
        //                {
        //                    DeltaX = 0,
        //                    DeltaY = actualDeltaY,
        //                    Location = location,
        //                    LocationOnScreen = location,
        //                    //Modifiers = new KeyModifiers(),
        //                    ScrollType = MouseScrollType.UnitScroll,
        //                };

        //                mouse.Moved.Raise(moveEvent);
        //                Thread.Sleep(200);
        //                mouse.WheelMoved.Raise(wheelEvent);
        //                Thread.Sleep(500);

        //                var resultPos = browser.JsGetWindowScrollYPosition(bank);
        //                if ((int)resultPos == scrollY) return true;
        //            }

        //            iTry++;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMessage = $"Error MouseWheelToScrollY, {scrollY}, {iTry}, {ex.Message}";
        //    }
        //    return false;
        //}

        public static async Task<bool> MouseWheelEvent(this IBrowser browser, string element, int headerOffset, int numOfTry, int dpi, int x = 1, int y = 1)
        {
            var iTry = 0;
            try
            {
                while (iTry < numOfTry) // Tries
                {
                    // Scroll down/up to calculate the measurement
                    int downOrUp = -1;
                    int initY;
                    int afterY;
                    int diff;

                    while (true) // Evaluate Wheel
                    {
                        // Get the init position
                        var initPos = await browser.JsGetElementPosition(element, 1);

                        // Mouse move to (1,1) then scroll down 30
                        browser.Focus();
                        IMouse mouse = browser.Mouse;
                        DotNetBrowser.Geometry.Point location = new DotNetBrowser.Geometry.Point(x, y).ToDpiPoint(dpi);
                        MouseMovedEventArgs moveEvent = new MouseMovedEventArgs
                        {
                            Location = location,
                        };
                        MouseWheelMovedEventArgs wheelEvent_1 = new MouseWheelMovedEventArgs
                        {
                            DeltaX = 0,
                            DeltaY = 30 * downOrUp,
                            Location = location,
                            LocationOnScreen = location,
                            //Modifiers = new KeyModifiers(),
                            ScrollType = MouseScrollType.UnitScroll,
                        };
                        MouseWheelMovedEventArgs wheelEvent_2 = new MouseWheelMovedEventArgs
                        {
                            DeltaX = 0,
                            DeltaY = 30 * downOrUp * -1,
                            Location = location,
                            LocationOnScreen = location,
                            //Modifiers = new KeyModifiers(),
                            ScrollType = MouseScrollType.UnitScroll,
                        };

                        mouse.Moved.Raise(moveEvent);
                        Thread.Sleep(200);
                        mouse.WheelMoved.Raise(wheelEvent_1);
                        Thread.Sleep(500);

                        // Get the position after scroll
                        var afterPos = await browser.JsGetElementPosition(element, 1);

                        // Get the vertical diff and find the ratio of scroll
                        initY = JsonConvert.DeserializeObject<dynamic>(initPos)?["y"] - headerOffset;
                        afterY = JsonConvert.DeserializeObject<dynamic>(afterPos)?["y"] - headerOffset;

                        //// Revert the scroll
                        //mouse.Moved.Raise(moveEvent);
                        //Thread.Sleep(200);
                        //mouse.WheelMoved.Raise(wheelEvent_2);
                        //Thread.Sleep(500);

                        if (downOrUp == 1)
                        {
                            var eleHeight = (await browser.JsGetElementOffsetHeight(element, 1)).StrToInt();
                            initY -= eleHeight;
                            afterY -= eleHeight;
                        }

                        diff = initY - afterY;

                        if (downOrUp == 1) break;

                        if (diff == 0)
                            downOrUp = 1;
                        else break;
                    }

                    if (diff != 0) // Wheel
                    {
                        float actualDeltaY = ((float)afterY / (float)diff * ((float)30 * (float)downOrUp));

                        browser.Focus();
                        IMouse mouse = browser.Mouse;
                        DotNetBrowser.Geometry.Point location = new DotNetBrowser.Geometry.Point(x, y).ToDpiPoint(dpi);
                        MouseMovedEventArgs moveEvent = new MouseMovedEventArgs
                        {
                            Location = location,
                        };
                        MouseWheelMovedEventArgs wheelEvent = new MouseWheelMovedEventArgs
                        {
                            DeltaX = 0,
                            DeltaY = actualDeltaY,
                            Location = location,
                            LocationOnScreen = location,
                            //Modifiers = new KeyModifiers(),
                            ScrollType = MouseScrollType.UnitScroll,
                        };

                        mouse.Moved.Raise(moveEvent);
                        Thread.Sleep(200);
                        mouse.WheelMoved.Raise(wheelEvent);
                        Thread.Sleep(500);

                        var visible = await browser.JsIsElementVisibleInViewport(element, 1);
                        if (visible) return true;
                    }

                    iTry++;
                }

            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error MouseWheelEvent, {element}, {iTry}, {ex.Message}";
            }
            return false;
        }

        public static async Task<bool> MouseClickEvent(this IBrowser browser, string element, int headerOffset, int numOfTry, int dpi, bool scroll = false, int x = 5, int y = 5)
        {
            var js = new StringBuilder();
            js.Append(@"
                function ElementPosition(element) {
                    var ele = document.querySelector(element);
                    if (ele) {
                        var pos = ele.getBoundingClientRect()
                        return JSON.stringify({ x: pos.left, y: pos.top })
                    }
                    return null
                }
            ");
            js.AppendFormat("\r\n ElementPosition('{0}')", element);

            var iTry = 0;
            try
            {
                string? jsPosition = null;
                while (jsPosition == null && iTry < numOfTry)
                {
                    if (scroll)
                    {
                        var scrollTo = await browser.MouseWheelEvent(element, headerOffset, 1, dpi);
                    }

                    jsPosition = await browser.JsExecuteScriptString(js.ToString());

                    if (jsPosition == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }

                if (jsPosition != null)
                {
                    var jsonPosition = JsonConvert.DeserializeObject<dynamic>(jsPosition);
                    var xPosition = (int)jsonPosition?["x"] + x;  // add +5 pixel to the click position
                    var yPosition = (int)jsonPosition?["y"] + y;

                    browser.Focus();
                    IMouse mouse = browser.Mouse;
                    MouseButton mouseButton = MouseButton.Left;
                    DotNetBrowser.Geometry.Point location = new DotNetBrowser.Geometry.Point(xPosition, yPosition).ToDpiPoint(dpi);
                    MouseMovedEventArgs moveEvent = new MouseMovedEventArgs
                    {
                        Location = location,
                    };
                    MousePressedEventArgs pressEvent = new MousePressedEventArgs
                    {
                        Location = location,
                        Button = mouseButton,
                        ClickCount = 1
                    };
                    MouseReleasedEventArgs releaseEvent = new MouseReleasedEventArgs
                    {
                        Button = mouseButton,
                        ClickCount = 1,
                        Location = location
                    };

#if DEBUG
                    IDocument document = browser.MainFrame.Document;
                    document.DocumentElement.Events.Click += (sender, e) =>
                    {
                        // Mouse click event has been received.
                        var mouseEvent = (DotNetBrowser.Dom.Events.IMouseEvent)e.Event;
                        Debug.WriteLine($"MouseClicked - ClientLocation: {mouseEvent.ClientLocation} OffsetLocation: {mouseEvent.OffsetLocation} ScreenLocation: {mouseEvent.ScreenLocation}");
                    };
#endif
                    mouse.Moved.Raise(moveEvent);
                    Thread.Sleep(200);
                    mouse.Pressed.Raise(pressEvent);
                    Thread.Sleep(200);
                    mouse.Released.Raise(releaseEvent);
                    Thread.Sleep(200);

                    return true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error MouseClickEvent, {element}, {iTry}, {ex.Message}";
            }
            return false;
        }


        public static async Task<bool> MouseClickEventJsScroll(this IBrowser browser, Bank bank, string element, int headerOffset, int numOfTry, int dpi, int x = 5, int y = 5)
        {
            var js = new StringBuilder();
            js.Append(@"
                function ElementPosition(element) {
                    var ele = document.querySelector(element);
                    if (ele) {
                        var pos = ele.getBoundingClientRect()
                        return JSON.stringify({ x: pos.left, y: pos.top })
                    }
                    return null
                }
            ");
            js.AppendFormat("\r\n ElementPosition('{0}')", element);

            var iTry = 0;
            try
            {
                string? jsPosition = null;
                while (jsPosition == null && iTry < numOfTry)
                {
                    var scrollTo = await browser.JsScrollToElement(bank, element, headerOffset, 1);

                    jsPosition = await browser.JsExecuteScriptString(js.ToString());
                    Debug.WriteLine($"MouseClickEventJsScroll jsPosition: {jsPosition}");

                    if (jsPosition == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }

                if (jsPosition != null)
                {
                    var jsonPosition = JsonConvert.DeserializeObject<dynamic>(jsPosition);
                    var xPosition = (int)jsonPosition?["x"] + x;  // add +5 pixel to the click position
                    var yPosition = (int)jsonPosition?["y"] + y;

                    IMouse mouse = browser.Mouse;
                    MouseButton mouseButton = MouseButton.Left;
                    DotNetBrowser.Geometry.Point location = new DotNetBrowser.Geometry.Point(xPosition, yPosition).ToDpiPoint(dpi);

                    Debug.WriteLine($"Mouse click x: {xPosition}");
                    Debug.WriteLine($"Mouse click y: {yPosition}");
                    Debug.WriteLine($"Mouse click location: {location}");

                    MouseMovedEventArgs moveEvent = new MouseMovedEventArgs
                    {
                        Location = location,
                    };
                    MousePressedEventArgs pressEvent = new MousePressedEventArgs
                    {
                        Location = location,
                        Button = mouseButton,
                        ClickCount = 1
                    };
                    MouseReleasedEventArgs releaseEvent = new MouseReleasedEventArgs
                    {
                        Button = mouseButton,
                        ClickCount = 1,
                        Location = location
                    };

                    //var w = browser.RegisterWait();

#if DEBUG
                    IDocument document = browser.MainFrame.Document;
                    document.DocumentElement.Events.Click += (sender, e) =>
                    {
                        // Mouse click event has been received.
                        var mouseEvent = (DotNetBrowser.Dom.Events.IMouseEvent)e.Event;
                        Debug.WriteLine($"MouseClicked - ClientLocation: {mouseEvent.ClientLocation} OffsetLocation: {mouseEvent.OffsetLocation} ScreenLocation: {mouseEvent.ScreenLocation}");
                    };
#endif
                    mouse.Moved.Raise(moveEvent);
                    Thread.Sleep(200);
                    mouse.Pressed.Raise(pressEvent);
                    Thread.Sleep(200);
                    mouse.Released.Raise(releaseEvent);
                    Thread.Sleep(200);

                    //await w.WaitAsync();

                    return true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error MouseClickEventJsScroll, {element}, {iTry}, {ex.Message}";
                Debug.WriteLine(ErrorMessage);
            }
            return false;
        }


        private static async Task<bool> SendInputKeyEvent(this IBrowser browser, string value, string element, int numOfTry, bool compareComma = true, bool sendTabKey = false)
        {
            var entered = string.Empty;
            var inputValue = string.Empty;
            var success = false;

            //var w = browser.RegisterWait();

            var keyboard = browser.Keyboard;
            foreach (var c in value)
            {
                entered += c;

                success = false;
                var iTry = 0;
                while (!success && iTry < numOfTry)
                {
                    var keyCode = c.ToMyKeyCode();
                    keyboard.SendKeyboardKey(KeyCode.Oem1, c.ToString(), null);
                    await Task.Delay(50);

                    inputValue = await browser.JsGetInputValue(element, numOfTry);

#if DEBUG
                    Debug.WriteLine($"inputValue <{inputValue}>, entered <{entered}>");
#endif
                    if (!compareComma && inputValue != null)
                        inputValue = inputValue.Replace(",", "").Replace(".","");

                    success = entered == inputValue;
                    if (!success)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }

                if (!success)
                    break;
            }

            if (sendTabKey)
            {
                await Task.Delay(1000);
                keyboard.SendKeyboardKey(KeyCode.Tab, "", null);
            }

            //await w.WaitAsync();

            return success;
        }


        /// <summary>
        /// Check either one elements exists and return the element exists
        /// </summary>
        /// <param name="numOfTry"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static async Task<string> JsEitherElementExists(this IBrowser browser, int numOfTry, params string[] elements)
        {
            JsResultModel check;
            //var check = false;
            var result = string.Empty;
            foreach (var element in elements)
            {
                check = await browser.JsElementExists(element, numOfTry);

                if (check.BoolResult)
                {
                    result = element;
                    break;
                }
            }

            return result;
        }

        public static async Task<bool> WriteHtmlToFile(this IBrowser browser, string filePath, string fileName)
        {
            try
            {
                var htmlSource = browser.MainFrame.Html;
                var fileFullPath = Path.Combine(filePath, string.Format(fileName + ".txt", DateTime.Now.ToString("yyyyMMddHHmmss.fff")));
                File.WriteAllText(fileFullPath, htmlSource);
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"WriteHtmlToFile failed, {fileName}, {ex.Message}";
            }
            return false;
        }

        public static async Task<string?> JsGetElementOffsetHeight(this IBrowser browser, string element, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function GetHeight(element) {
                    var ele = document.querySelector(element);
                    if (ele) {
                        return ele.offsetHeight.toString()
                    }
                    return null
                }
            ");
            js.AppendFormat("\r\n GetHeight('{0}');", element);

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsGetElementOffsetHeight, {element}, {iTry}, {ex.Message}";
            }
            return null;
        }

        public static async Task<string?> JsGetElementInnerText(this IBrowser browser, string element, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function GetInnerText(element) {
                    var ele = document.querySelector(element);
                    if (ele) {
                        return ele.innerText
                    }
                    return null
                }
            ");
            js.AppendFormat("\r\n GetInnerText('{0}');", element);

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsGetElementInnerText, {element}, {iTry}, {ex.Message}";
            }
            return null;
        }

        public static async Task<string?> JsGetElementInnerHtml(this IBrowser browser, string element, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function GetInnerHtml(element) {
                    var ele = document.querySelector(element);
                    if (ele) {
                        return ele.innerHTML
                    }
                    return null
                }
            ");
            js.AppendFormat("\r\n GetInnerHtml('{0}')", element);

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsGetElementInnerHtml, {element}, {iTry}, {ex.Message}";
            }
            return null;
        }

        public static async Task<string?> JsGetInputValue(this IBrowser browser, string element, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function GetInputValue(element) {
                    var ele = document.querySelector(element);
                    if (ele) {
                        return ele.value
                    }
                    return null
                }
            ");
            js.AppendFormat("\r\n GetInputValue('{0}');", element);

            var iTry = 0;
            string? result = null;
            try
            {
                while (result == null && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptString(js.ToString());

                    if (result == null)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsGetInputValue, {element}, {iTry}, {ex.Message}";
            }
            return null;
        }

        public static async Task<bool> JsIsElementVisibleInViewport(this IBrowser browser, string element, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function IsElementVisibleInViewport(element) {
                    const elementIsVisibleInViewport = (el, partiallyVisible = false) => {
                        const { top, left, bottom, right } = el.getBoundingClientRect()
                        const { innerHeight, innerWidth } = window
                        return partiallyVisible
                            ? ((top > 0 && top < innerHeight) ||
                                (bottom > 0 && bottom < innerHeight)) &&
                            ((left > 0 && left < innerWidth) || (right > 0 && right < innerWidth))
                            : top >= 0 && left >= 0 && bottom <= innerHeight && right <= innerWidth
                    }

                    var ele = document.querySelector(element)
                    return elementIsVisibleInViewport(ele);
                }
            ");
            js.AppendFormat("\r\n IsElementVisibleInViewport('{0}')", element);

            var iTry = 0;
            var result = false;
            try
            {
                while (!result && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptBoolean(js.ToString());

                    if (!result)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsIsElementVisibleInViewport, {element}, {iTry}, {ex.Message}";
            }
            return false;
        }

        public static async Task<bool> JsIsActiveElement(this IBrowser browser, string element, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function IsActiveElement(element) {
                    var ele = document.querySelector(element);
                    if (ele && ele == document.activeElement) {
                        return true
                    }
                    return false
                }
            ");
            js.AppendFormat("\r\n IsActiveElement('{0}')", element);

            var iTry = 0;
            var result = false;
            try
            {
                while (!result && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptBoolean(js.ToString());

                    if (!result)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error IsActiveElement, {element}, {iTry}, {ex.Message}";
            }
            return false;
        }

        public static async Task<bool> JsSetSelectValue(this IBrowser browser, string element, string value, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function SetSelectValue(element, value) {

                    //const simulateClick = (item) => {
                    //    item.dispatchEvent(new PointerEvent('pointerdown', {bubbles: true}));
                    //    item.dispatchEvent(new MouseEvent('mousedown', {bubbles: true}));
                    //    item.dispatchEvent(new PointerEvent('pointerup', {bubbles: true}));
                    //    item.dispatchEvent(new MouseEvent('mouseup', {bubbles: true}));
                    //    item.dispatchEvent(new MouseEvent('mouseout', {bubbles: true}));
                    //    item.dispatchEvent(new MouseEvent('click', {bubbles: true}));
                    //    item.dispatchEvent(new Event('change', {bubbles: true}));
                    //}

                    var ele = document.querySelector(element);
                    if (ele) {

                        //ele.value = value
                        //simulateClick(ele)

                        var onPointerDown = document.createEvent(""CustomEvent"");
                        onPointerDown.initCustomEvent(""pointerdown"", true, false);
                        var onMouseDown = document.createEvent(""CustomEvent"");
                        onMouseDown.initCustomEvent(""mousedown"", true, false);
                        var onPointerUp = document.createEvent(""CustomEvent"");
                        onPointerUp.initCustomEvent(""pointerup"", true, false);
                        var onMouseUp = document.createEvent(""CustomEvent"");
                        onMouseUp.initCustomEvent(""mouseup"", true, false);
                        var onMouseOut = document.createEvent(""CustomEvent"");
                        onMouseOut.initCustomEvent(""mouseout"", true, false);
                        var onClick = document.createEvent(""CustomEvent"");
                        onClick.initCustomEvent(""click"", true, false);
                        var onChange = document.createEvent(""CustomEvent"");
                        onChange.initCustomEvent(""change"", true, false);

                        ele.focus;
                        setTimeout(function () {
                            var ev = new Event(""pointerdown"", onPointerDown);
                            ele.dispatchEvent(ev);
                        }, 0);
                        setTimeout(function () {
                            var ev = new Event(""mousedown"", onMouseDown);
                            ele.dispatchEvent(ev);
                        }, 0);
                        setTimeout(function () {
                            var ev = new Event(""pointerup"", onPointerUp);
                            ele.dispatchEvent(ev);
                        }, 0);
                        setTimeout(function () {
                            var ev = new Event(""mouseup"", onMouseUp);
                            ele.dispatchEvent(ev);
                        }, 0);
                        setTimeout(function () {
                            var ev = new Event(""mouseout"", onMouseOut);
                            ele.dispatchEvent(ev);
                        }, 0);
                        setTimeout(function () {
                            var ev = new Event(""click"", onClick);
                            ele.dispatchEvent(ev);
                        }, 0);
                        onClick.bubbles = true;
                        ele.value = value;
                        setTimeout(function () {
                            var ev = new Event(""change"", onChange);
                            ele.dispatchEvent(ev);
                        }, 0);
                    }
                }
            ");
            js.AppendFormat("\r\n SetSelectValue('{0}', '{1}');", element, value);

            var iTry = 0;
            var result = false;
            try
            {
                while (!result && iTry < numOfTry)
                {
                    //var w = browser.RegisterWait();
                    result = await browser.JsExecuteScriptVoid(js.ToString());
                    //await w.WaitAsync();

                    if (!result)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error SetSelectValue, {element}, {value}, {iTry}, {ex.Message}";
            }
            return false;
        }

        public static async Task<bool> JsSetCheckbox(this IBrowser browser, string element, bool isChecked, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function SetCheckbox(element, isChecked) {
                    var ele = document.querySelector(element);
                    if (ele) {
                        var onPointerDown = document.createEvent(""CustomEvent"");
                        onPointerDown.initCustomEvent(""pointerdown"", true, false);
                        var onMouseDown = document.createEvent(""CustomEvent"");
                        onMouseDown.initCustomEvent(""mousedown"", true, false);
                        var onPointerUp = document.createEvent(""CustomEvent"");
                        onPointerUp.initCustomEvent(""pointerup"", true, false);
                        var onMouseUp = document.createEvent(""CustomEvent"");
                        onMouseUp.initCustomEvent(""mouseup"", true, false);
                        var onMouseOut = document.createEvent(""CustomEvent"");
                        onMouseOut.initCustomEvent(""mouseout"", true, false);
                        var onClick = document.createEvent(""CustomEvent"");
                        onClick.initCustomEvent(""click"", true, false);
                        var onChange = document.createEvent(""CustomEvent"");
                        onChange.initCustomEvent(""change"", true, false);

                        ele.focus;
                        setTimeout(function () {
                            var ev = new Event(""pointerdown"", onPointerDown);
                            ele.dispatchEvent(ev);
                        }, 0);
                        setTimeout(function () {
                            var ev = new Event(""mousedown"", onMouseDown);
                            ele.dispatchEvent(ev);
                        }, 0);
                        setTimeout(function () {
                            var ev = new Event(""pointerup"", onPointerUp);
                            ele.dispatchEvent(ev);
                        }, 0);
                        setTimeout(function () {
                            var ev = new Event(""mouseup"", onMouseUp);
                            ele.dispatchEvent(ev);
                        }, 0);
                        setTimeout(function () {
                            var ev = new Event(""mouseout"", onMouseOut);
                            ele.dispatchEvent(ev);
                        }, 0);
                        setTimeout(function () {
                            var ev = new Event(""click"", onClick);
                            ele.dispatchEvent(ev);
                        }, 0);
                        onClick.bubbles = true;
                        ele.checked = isChecked;
                        setTimeout(function () {
                            var ev = new Event(""change"", onChange);
                            ele.dispatchEvent(ev);
                        }, 0);
                    }
                }
            ");
            js.AppendFormat("\r\n SetCheckbox('{0}', {1});", element, isChecked ? "true" : "false");

            var iTry = 0;
            var result = false;
            try
            {
                while (!result && iTry < numOfTry)
                {
                    //var w = browser.RegisterWait();
                    result = await browser.JsExecuteScriptVoid(js.ToString());
                    //await w.WaitAsync();

                    if (!result)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsSetCheckbox, {element}, {isChecked}, {iTry}, {ex.Message}";
            }
            return false;
        }

        public static async Task<bool> JsIsInputChecked(this IBrowser browser, string element, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function ElementChecked(element) {
                    var ele = document.querySelector(element);
                    if (ele) {
                        return ele.checked
                    }
                    return false
                }
            ");
            js.AppendFormat("\r\n ElementChecked('{0}')", element);

            var iTry = 0;
            var result = false;
            try
            {
                while (!result && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptBoolean(js.ToString());

                    if (!result)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsIsInputChecked, {element}, {iTry}, {ex.Message}";
            }

            return false;
        }

        public static async Task<JsResultModel> JsElementExists(this IBrowser browser, string element, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function ElementExists(element) {
                    var ele = document.querySelector(element);
                    if (ele) {
                        return true
                    }
                    return false
                }
            ");
            js.AppendFormat("\r\n ElementExists('{0}')", element);

            var iTry = 0;
            var result = false;
            var result2 = new JsResultModel();
            try
            {
                while (!result && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptBoolean(js.ToString());

                    if (!result)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                result2.BoolResult = result;
                result2.IsSuccess = true;
                return result2;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsElementExists, {element}, {iTry}, {ex.Message}";
                result2.ErrorMessage = $"Error JsElementExists, {element}, {iTry}, {ex.Message}";
            }

            result2.BoolResult = false;
            return result2;
        }

        public static async Task<bool> JsElementToParentNodeExists(this IBrowser browser, string element, int parentNodeLevel, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function ElementExists(element, parNodeLevel) {
                    var ele = document.querySelector(element);
                    if (ele) {
                        var par = ele;
                        for (let i = 0; i < parNodeLevel; i++) {
                            par = par.parentNode;
                        }
                        if (par) {
                            return true
                        }
                        return false
                    }
                    return false
                }
                ElementExists()
            ");
            js.AppendFormat("\r\n ElementExists('{0}', {1})", element, parentNodeLevel);

            var iTry = 0;
            var result = false;
            try
            {
                while (!result && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptBoolean(js.ToString());

                    if (!result)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }

                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsElementToParentNodeExists, {element}, {parentNodeLevel}, {iTry}, {ex.Message}";
            }

            return false;
        }

        public static async Task<bool> JsClickElement(this IBrowser browser, string element, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function ClickElement(element) {
                    var ele = document.querySelector(element);
                    if (ele) {
                        ele.click()
                    }
                }
            ");
            js.AppendFormat("\r\n ClickElement('{0}')", element);

            var iTry = 0;
            var result = false;
            try
            {
                while (!result && iTry < numOfTry)
                {
                    //var w = browser.RegisterWait();
                    result = await browser.JsExecuteScriptVoid(js.ToString());
                    //if (hasLoading) await w.WaitAsync();

                    if (!result)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsClickElement, {element}, {iTry}, {ex.Message}";
            }
            return false;
        }

        public static async Task<bool> JsClickElementToParentNode(this IBrowser browser, string element, int parentNodeLevel, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function ClickElement(element, parNodeLevel) {
                    var ele = document.querySelector(element);
                    if (ele) {
                        var par = ele;
                        for (let i = 0; i < parNodeLevel; i++) {
                            par = par.parentNode;
                        }
                        if (par) {
                            par.click()
                        }
                    }
                }
            ");
            js.AppendFormat("\r\n ClickElement('{0}', {1})", element, parentNodeLevel);

            var iTry = 0;
            var result = false;
            try
            {
                while (!result && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptVoid(js.ToString());

                    if (!result)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsClickElementToParentNode, {element}, {parentNodeLevel}, {iTry}, {ex.Message}";
            }
            return false;
        }

        public static async Task<bool> JsInputElementValue(this IBrowser browser, string element, string value, int numOfTry)
        {
            var js = new StringBuilder();
            js.Append(@"
                function InputValue(value, element) {
                    if (document.querySelector(element)) {
                        const ele = document.querySelector(element);
                        var key;
                        var onKeyDown = document.createEvent(""CustomEvent"");
                        onKeyDown.initCustomEvent(""keydown"", true, false);
                        var onKeyPress = document.createEvent(""CustomEvent"");
                        onKeyPress.initCustomEvent(""keypress"", true, false);
                        var onInput = document.createEvent(""CustomEvent"");
                        onInput.initCustomEvent(""input"", true, false);
                        var onKeyUp = document.createEvent(""CustomEvent"");
                        onKeyUp.initCustomEvent(""keyup"", true, false);

                        key = value.charCodeAt(i);
                        onInput.bubbles = true;
                        ele.focus;
                        setTimeout(function () {
                            var eKeyDown = new Event(""keydown"", onKeyDown);
                            ele.dispatchEvent(eKeyDown);
                        }, 0);
                        setTimeout(function () {
                            var eKeyPress = new Event(""keypress"", onKeyPress);
                            ele.dispatchEvent(eKeyPress);
                        }, 0);
                        setTimeout(function () {
                            var eInput = new Event(""input"", onInput);
                            ele.dispatchEvent(eInput);
                        }, 0);
                        ele.value = '';
                        setTimeout(function () {
                            var eKeyUp = new Event(""keyup"", onKeyUp);
                            ele.dispatchEvent(eKeyUp);
                        }, 0);

                        for (var i = 0; i < value.length; ++i) {
                            key = value.charCodeAt(i);
                            onInput.bubbles = true;
                            ele.focus;
                            setTimeout(function () {
                                var eKeyDown = new Event(""keydown"", onKeyDown);
                                ele.dispatchEvent(eKeyDown);
                            }, 0);
                            setTimeout(function () {
                                var eKeyPress = new Event(""keypress"", onKeyPress);
                                ele.dispatchEvent(eKeyPress);
                            }, 0);
                            setTimeout(function () {
                                var eInput = new Event(""input"", onInput);
                                ele.dispatchEvent(eInput);
                            }, 0);
                            ele.value = ele.value + value.charAt(i);
                            setTimeout(function () {
                                var eKeyUp = new Event(""keyup"", onKeyUp);
                                ele.dispatchEvent(eKeyUp);
                            }, 0);
                        }
                    }
                }
            "
            );
            js.AppendFormat("\r\n InputValue('{0}', '{1}')", value, element);

            var iTry = 0;
            var result = false;
            try
            {
                while (!result && iTry < numOfTry)
                {
                    result = await browser.JsExecuteScriptVoid(js.ToString());

                    if (!result)
                    {
                        await WaitOneSecondsAsync(iTry, numOfTry);
                    }

                    iTry++;
                }
                return result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error JsInputElementValue, {element}, {value}, {iTry}, {ex.Message}";
            }
            return false;
        }

        public static async Task<bool> JsExecuteScriptVoid(this IBrowser browser, string script)
        {
            var result = browser.MainFrame.ExecuteJavaScript<object>(script).Result;
            return result == null;
        }

        public static async Task<bool> JsExecuteScriptBoolean(this IBrowser browser, string script)
        {
            return browser.MainFrame.ExecuteJavaScript<bool>(script).Result;
        }

        public static async Task<string?> JsExecuteScriptString(this IBrowser browser, string script)
        {
            return browser.MainFrame.ExecuteJavaScript<string?>(script).Result;
        }

        public static async Task<Bitmap> Screenshot(this IBrowser browser, Bank bank)
        {
            // Cache the current size
            var cachedSize = browser.Size;
            var cachedScrollY = await browser.JsGetWindowScrollYPosition(bank);

            // Adjust to webpage size and capture screenshot
            double documentWidth = browser.JsGetDocumentWidth();
            double documentHeight = browser.JsGetDocumentHeight();
            browser.Size = new DotNetBrowser.Geometry.Size((uint)documentWidth, (uint)documentHeight);
            await Task.Delay(200);
            var result = browser.TakeImage().ToBitmap();

            // Revert to current size
            browser.Size = cachedSize;
            await Task.Delay(1000);
            await browser.JsScrollToY(bank, (int)cachedScrollY, 1);

            return result;
        }

        public static void SendKeyboardKey(IKeyboard keyboard, KeyCode key, string keyChar,
                                        KeyModifiers modifiers = null)
        {
            modifiers = modifiers ?? new KeyModifiers();
            KeyPressedEventArgs keyDownEventArgs = new KeyPressedEventArgs
            {
                KeyChar = keyChar,
                VirtualKey = key,
                Modifiers = modifiers
            };

            KeyTypedEventArgs keyPressEventArgs = new KeyTypedEventArgs
            {
                KeyChar = keyChar,
                VirtualKey = key,
                Modifiers = modifiers
            };
            KeyReleasedEventArgs keyUpEventArgs = new KeyReleasedEventArgs
            {
                VirtualKey = key,
                Modifiers = modifiers
            };

            keyboard.KeyPressed.Raise(keyDownEventArgs);
            keyboard.KeyTyped.Raise(keyPressEventArgs);
            keyboard.KeyReleased.Raise(keyUpEventArgs);
        }

        public static async Task WaitOneSecondsAsync(int currentTry, int totalTry)
        {
            // No need wait 1 seconds for last try
            var isLastTry = currentTry + 1 == totalTry;
            if (!isLastTry)
                await Task.Delay(1000);
        }

        //public static async Task<ManualResetEvent> RegisterWaitAsync(this IBrowser browser, int maxWait = 15)
        //{
        //    ManualResetEvent waitEvent = new ManualResetEvent(false);

        //    //Subscribe to the navigation event which indicates that the new web page was loaded.
        //    browser.Navigation.FrameDocumentLoadFinished += (o, args) =>
        //    {
        //        if (args.Frame.IsMain)
        //        {
        //            waitEvent.Set();
        //        }
        //    };

        //    //var wait = maxWait;
        //    //var otpWatch = Stopwatch.StartNew();
        //    //while (true)
        //    //{
        //    //    if (otpWatch.Elapsed.TotalSeconds.DoubleToInt() > wait)
        //    //    {
        //    //        waitEvent.Set();
        //    //        break;
        //    //    }
        //    //    await Task.Delay(500);
        //    //}
        //    //otpWatch.Stop();

        //    return waitEvent;
        //}

        //public static async Task WaitAsync(this ManualResetEvent w)
        //{
        //    w.WaitOne();
        //}

        #endregion


        #region DotNetBrowser Js


        public static double JsGetDocumentHeight(this IBrowser browser)
        {
            return browser.MainFrame.ExecuteJavaScript<double>(@"
                function GetPageHeight() {
                    var pageHeight = 0

                    function findHighestNode(nodesList) {
                        for (var i = nodesList.length - 1; i >= 0; i--) {
                            if (nodesList[i].scrollHeight && nodesList[i].clientHeight) {
                                var elHeight = Math.max(nodesList[i].scrollHeight, nodesList[i].clientHeight)
                                pageHeight = Math.max(elHeight, pageHeight)
                            }
                            if (nodesList[i].childNodes.length) findHighestNode(nodesList[i].childNodes)
                        }
                    }

                    findHighestNode(document.documentElement.childNodes)

                    return pageHeight
                }

                GetPageHeight()
            ").Result;
        }

        public static double JsGetDocumentWidth(this IBrowser browser)
        {
            return browser.MainFrame.ExecuteJavaScript<double>(@"
                function GetPageWidth() {
                    var pageWidth = 0

                    function findHighestNode(nodesList) {
                        for (var i = nodesList.length - 1; i >= 0; i--) {
                            if (nodesList[i].scrollWidth && nodesList[i].clientWidth) {
                                var elWidth = Math.max(nodesList[i].scrollWidth, nodesList[i].clientWidth)
                                pageWidth = Math.max(elWidth, pageWidth)
                            }
                            if (nodesList[i].childNodes.length) findHighestNode(nodesList[i].childNodes)
                        }
                    }

                    findHighestNode(document.documentElement.childNodes)

                    return pageWidth
                }

                GetPageWidth()
            ").Result;
        }

        public static async Task<double> JsGetWindowScrollYPosition(this IBrowser browser, Bank bank)
        {
            var scrollElement = await browser.GetScrollElement(bank);

            if (!string.IsNullOrWhiteSpace(scrollElement))
            {
                var js = new StringBuilder();
                js.Append(@"
                    function GetElementScrollTop(element) {
                        var ele = document.querySelector(element);
                        if (ele) {
                            return ele.scrollTop
                        }
                        return 0;
                    }
                ");
                js.AppendFormat("\r\n GetElementScrollTop('{0}')", scrollElement);
                return browser.MainFrame.ExecuteJavaScript<double>(js.ToString()).Result;
            }

            return browser.MainFrame.ExecuteJavaScript<double>(@"
                window.scrollY
            ").Result;
        }
        #endregion
    }
}