using CefSharp.OffScreen;
using CefSharp;
using Neptune.NsPay.CefTransfer.Common.Classes;
using Neptune.NsPay.CefTransfer.Common.MyEnums;
using System.Text;
using Newtonsoft.Json;

namespace Neptune.NsPay.CefTransfer.Win.Classes
{
    public static class CefHelpers
    {
        public static string? ErrorMessage { get; set; }

        #region TCB

        public static async Task<string?> JsTcbGetInputLoginErrorMessage(this ChromiumWebBrowser browser, int numOfTry) // TCB login input errors (user and password)
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

        public static async Task<string?> JsTcbGetBeneficiaryBankPosition(this ChromiumWebBrowser browser, string bankShortName, int numOfTry)
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

        public static async Task<string?> JsGetTcbMessageFromTransHistory(this ChromiumWebBrowser browser, Lang lang, int numOfTry) // Transaction History details
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

        public static async Task<string?> JsGetTcbTransIdFromTransHistory(this ChromiumWebBrowser browser, Lang lang, int numOfTry) // Transaction History details
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

        public static async Task<string?> JsGetTcbTransactionId(this ChromiumWebBrowser browser, Lang lang, int numOfTry) // Transfer success screen
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
        public static async Task<string?> JsVcbGetInternalTransferBenName(this ChromiumWebBrowser browser, Lang lang, int numOfTry) // Internal Transfer Page 2 Beneficiary Name
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

        public static async Task<string?> JsVcbGetTransactionId(this ChromiumWebBrowser browser, TransferType transType, Lang lang, int numOfTry) // Transfer success screen
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

        public static async Task<string?> JsVcbGetBeneficiaryBankPosition(this ChromiumWebBrowser browser, string bankName, int numOfTry)
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

        public static async Task<string?> JsVcbGetInputTransferBalance(this ChromiumWebBrowser browser, Lang lang, int numOfTry)
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

        public static async Task<string?> JsVcbGetInputTransferErrorMessage(this ChromiumWebBrowser browser, int numOfTry)
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

        #region Common

        public static async Task<bool> JsClickElementAtIndex(this ChromiumWebBrowser browser, string element, int index, int numOfTry)
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
                ErrorMessage = $"Error JsClickElement, {element}, {iTry}, {ex.Message}";
            }
            return false;
        }

        public static async Task<string?> JsGetElementCount(this ChromiumWebBrowser browser, string element, int numOfTry)
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

        public static async Task<bool> JsElementDisabled(this ChromiumWebBrowser browser, string element, int numOfTry)
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

        public static async Task<string?> JsGetElementPosition(this ChromiumWebBrowser browser, string element, int numOfTry)
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

        public static async Task<bool> MouseClickAndSetCursorActive(this ChromiumWebBrowser browser, string element, int numOfTry)
        {
            var clicked = false;
            var iTry = 0;
            while (iTry < numOfTry)
            {
                clicked = await browser.JsIsActiveElement(element, 1);
                if (!clicked)
                {
                    await browser.MouseClickEvent(element, 1);
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

                iTry++;
            }
            return clicked;
        }

        public static async Task<bool> InputToElement(this ChromiumWebBrowser browser, string value, string element, int numOfTry)
        {
            var result = false;
            var clicked = false;
            var iTry = 0;
            while (iTry < numOfTry)
            {
                clicked = await browser.JsIsActiveElement(element, 1);
                if (!clicked)
                {
                    await browser.MouseClickEvent(element, 1);
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

                iTry++;
            }

            if (clicked)
            {
                result = await browser.SendInputKeyEvent(value, element, numOfTry);
            }

            return result;
        }

        public static bool MouseClickJsPosition(this ChromiumWebBrowser browser, string jsPosition, int numOfTry, int x = 5, int y = 5)
        {
            var iTry = 0;
            try
            {
                if (jsPosition != null)
                {
                    var jsonPosition = JsonConvert.DeserializeObject<dynamic>(jsPosition);
                    var xPosition = (int)jsonPosition?["x"] + x;  // add +5 pixel to the click position
                    var yPosition = (int)jsonPosition?["y"] + y;
                    var browserHost = browser.GetBrowser().GetHost();
                    browserHost.SetFocus(true);

                    browserHost.SendMouseMoveEvent(xPosition, yPosition, false, CefEventFlags.None);
                    Thread.Sleep(50);
                    browserHost.SendMouseClickEvent(xPosition, yPosition, MouseButtonType.Left,
                        false, 1, CefEventFlags.None);
                    Thread.Sleep(50);
                    browserHost.SendMouseClickEvent(xPosition, yPosition, MouseButtonType.Left,
                        true, 1, CefEventFlags.None);
                    Thread.Sleep(50);

                    return true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error MouseClickJsPosition, {jsPosition}, {iTry}, {ex.Message}";
            }
            return false;
        }

        public static async Task<bool> MouseClickEvent(this ChromiumWebBrowser browser, string element, int numOfTry, int x = 5, int y = 5)
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
                    var browserHost = browser.GetBrowser().GetHost();
                    browserHost.SetFocus(true);

                    browserHost.SendMouseMoveEvent(xPosition, yPosition, false, CefEventFlags.None);
                    Thread.Sleep(50);
                    browserHost.SendMouseClickEvent(xPosition, yPosition, MouseButtonType.Left,
                        false, 1, CefEventFlags.None);
                    Thread.Sleep(50);
                    browserHost.SendMouseClickEvent(xPosition, yPosition, MouseButtonType.Left,
                        true, 1, CefEventFlags.None);
                    Thread.Sleep(50);

                    return true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error MouseClickEvent, {element}, {iTry}, {ex.Message}";
            }
            return false;
        }

        private static async Task<bool> SendInputKeyEvent(this ChromiumWebBrowser browser, string value, string element, int numOfTry)
        {
            var entered = string.Empty;
            var inputValue = string.Empty;
            var success = false;

            var browserHost = browser.GetBrowser().GetHost();
            browserHost.SetFocus(true);

            foreach (var c in value)
            {
                entered += c;

                success = false;
                var iTry = 0;
                while (!success && iTry < numOfTry)
                {
                    browserHost.SendKeyEvent(new KeyEvent { WindowsKeyCode = c, Type = KeyEventType.Char });
                    await Task.Delay(50);

                    inputValue = await browser.JsGetInputValue(element, numOfTry);
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

            return success;
        }


        /// <summary>
        /// Check either one elements exists and return the element exists
        /// </summary>
        /// <param name="numOfTry"></param>
        /// <param name="elements"></param>
        /// <returns></returns>
        public static async Task<string> JsEitherElementExists(this ChromiumWebBrowser browser, int numOfTry, params string[] elements)
        {
            var check = false;
            var result = string.Empty;
            foreach (var element in elements)
            {
                check = await browser.JsElementExists(element, numOfTry);

                if (check)
                {
                    result = element;
                    break;
                }
            }

            return result;
        }

        public static async Task<bool> WriteHtmlToFile(this ChromiumWebBrowser browser, string filePath, string fileName)
        {
            try
            {
                var htmlSource = await browser.GetSourceAsync();
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

        public static async Task<string?> JsGetElementInnerText(this ChromiumWebBrowser browser, string element, int numOfTry)
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

        public static async Task<string?> JsGetElementInnerHtml(this ChromiumWebBrowser browser, string element, int numOfTry)
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

        public static async Task<string?> JsGetInputValue(this ChromiumWebBrowser browser, string element, int numOfTry)
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

        public static async Task<bool> JsIsActiveElement(this ChromiumWebBrowser browser, string element, int numOfTry)
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

        //public static async Task<bool> JsSetSelectValue(this ChromiumWebBrowser browser, string element, string value, int numOfTry)
        //{
        //    var js = new StringBuilder();
        //    js.Append(@"
        //        function SetSelectValue(element, value) {

        //            //const simulateClick = (item) => {
        //            //    item.dispatchEvent(new PointerEvent('pointerdown', {bubbles: true}));
        //            //    item.dispatchEvent(new MouseEvent('mousedown', {bubbles: true}));
        //            //    item.dispatchEvent(new PointerEvent('pointerup', {bubbles: true}));
        //            //    item.dispatchEvent(new MouseEvent('mouseup', {bubbles: true}));
        //            //    item.dispatchEvent(new MouseEvent('mouseout', {bubbles: true}));
        //            //    item.dispatchEvent(new MouseEvent('click', {bubbles: true}));
        //            //    item.dispatchEvent(new Event('change', {bubbles: true}));
        //            //}

        //            var ele = document.querySelector(element);
        //            if (ele) {

        //                //ele.value = value
        //                //simulateClick(ele)

        //                var onPointerDown = document.createEvent(""CustomEvent"");
        //                onPointerDown.initCustomEvent(""pointerdown"", true, false);
        //                var onMouseDown = document.createEvent(""CustomEvent"");
        //                onMouseDown.initCustomEvent(""mousedown"", true, false);
        //                var onPointerUp = document.createEvent(""CustomEvent"");
        //                onPointerUp.initCustomEvent(""pointerup"", true, false);
        //                var onMouseUp = document.createEvent(""CustomEvent"");
        //                onMouseUp.initCustomEvent(""mouseup"", true, false);
        //                var onMouseOut = document.createEvent(""CustomEvent"");
        //                onMouseOut.initCustomEvent(""mouseout"", true, false);
        //                var onClick = document.createEvent(""CustomEvent"");
        //                onClick.initCustomEvent(""click"", true, false);
        //                var onChange = document.createEvent(""CustomEvent"");
        //                onChange.initCustomEvent(""change"", true, false);

        //                ele.focus;
        //                setTimeout(function () {
        //                    var ev = new Event(""pointerdown"", onPointerDown);
        //                    ele.dispatchEvent(ev);
        //                }, 0);
        //                setTimeout(function () {
        //                    var ev = new Event(""mousedown"", onMouseDown);
        //                    ele.dispatchEvent(ev);
        //                }, 0);
        //                setTimeout(function () {
        //                    var ev = new Event(""pointerup"", onPointerUp);
        //                    ele.dispatchEvent(ev);
        //                }, 0);
        //                setTimeout(function () {
        //                    var ev = new Event(""mouseup"", onMouseUp);
        //                    ele.dispatchEvent(ev);
        //                }, 0);
        //                setTimeout(function () {
        //                    var ev = new Event(""mouseout"", onMouseOut);
        //                    ele.dispatchEvent(ev);
        //                }, 0);
        //                setTimeout(function () {
        //                    var ev = new Event(""click"", onClick);
        //                    ele.dispatchEvent(ev);
        //                }, 0);
        //                onClick.bubbles = true;
        //                ele.value = value;
        //                setTimeout(function () {
        //                    var ev = new Event(""change"", onChange);
        //                    ele.dispatchEvent(ev);
        //                }, 0);
        //            }
        //        }
        //    ");
        //    js.AppendFormat("\r\n SetSelectValue('{0}', '{1}');", element, value);

        //    var iTry = 0;
        //    var result = false;
        //    try
        //    {
        //        while (!result && iTry < numOfTry)
        //        {
        //            result = await browser.JsExecuteScriptVoid(js.ToString());

        //            if (!result)
        //            {
        //                await WaitOneSecondsAsync(iTry, numOfTry);
        //            }

        //            iTry++;
        //        }
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMessage = $"Error SetSelectValue, {element}, {value}, {iTry}, {ex.Message}";
        //    }
        //    return false;
        //}

        public static async Task<bool> JsElementExists(this ChromiumWebBrowser browser, string element, int numOfTry)
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
                ErrorMessage = $"Error JsElementExists, {element}, {iTry}, {ex.Message}";
            }

            return false;
        }

        public static async Task<bool> JsElementToParentNodeExists(this ChromiumWebBrowser browser, string element, int parentNodeLevel, int numOfTry)
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

        public static async Task<bool> JsClickElement(this ChromiumWebBrowser browser, string element, int numOfTry)
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
                ErrorMessage = $"Error JsClickElement, {element}, {iTry}, {ex.Message}";
            }
            return false;
        }

        public static async Task<bool> JsClickElementToParentNode(this ChromiumWebBrowser browser, string element, int parentNodeLevel, int numOfTry)
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

        public static async Task<bool> JsInputElementValue(this ChromiumWebBrowser browser, string element, string value, int numOfTry)
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

        public static async Task<bool> JsExecuteScriptVoid(this ChromiumWebBrowser browser, string script)
        {
            if (browser.CanExecuteJavascriptInMainFrame)
            {
                var result = await browser.EvaluateScriptAsync(script);
                if (result.Success)
                {
                    return (bool)result.Success;
                }
                else
                {
                    throw new Exception($"Error execute Js Script, {result.Message}");
                }
            }
            else
            {
                throw new Exception("Js Script not executed.");
            }
        }

        public static async Task<bool> JsExecuteScriptBoolean(this ChromiumWebBrowser browser, string script)
        {
            if (browser.CanExecuteJavascriptInMainFrame)
            {
                var result = await browser.EvaluateScriptAsync(script);
                if (result.Success)
                {
                    return (bool)result.Result;
                }
                else
                {
                    throw new Exception($"Error execute Js Script, {result.Message}");
                }
            }
            else
            {
                throw new Exception("Js Script not executed.");
            }
        }

        public static async Task<string> JsExecuteScriptString(this ChromiumWebBrowser browser, string script)
        {
            if (browser.CanExecuteJavascriptInMainFrame)
            {
                var result = await browser.EvaluateScriptAsync(script);
                if (result.Success)
                {
                    return (string)result.Result;
                }
                else
                {
                    throw new Exception($"Error execute Js Script, {result.Message}");
                }
            }
            else
            {
                throw new Exception("Js Script not executed.");
            }
        }

        public static async Task WaitOneSecondsAsync(int currentTry, int totalTry)
        {
            // No need wait 1 seconds for last try
            var isLastTry = currentTry + 1 == totalTry;
            if (!isLastTry)
                await Task.Delay(1000);
        }
        #endregion
    }
}
