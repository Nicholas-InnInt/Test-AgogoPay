(function ($) {
    app.modals.CreateOrEditPayOrderModal = function () {

        var _recipientBankAccountsService = abp.services.app.recipientBankAccounts;
        var _modalManager;
        var _$recipientBankAccountsForm = null;
        var $merchantDropdown = $("#MerchantId");
        var $bankDropdown = $("#BankName");

        function getMerchants() {
            $merchantDropdown.empty();
            $merchantDropdown.append("<option selected value=''>" + app.localize('SelectMerchant') + "</option>");

            _recipientBankAccountsService.getOrderMerchants({}).done(function (result) {
                if (result.length === 0) {
                    return;
                }

                for (var i = 0; i < result.length; i++) {
                    $merchantDropdown.append(
                        "<option value='" + result[i].merchantId + "'>" +
                        result[i].merchantName + " [" + result[i].merchantCode + "]" +
                        "</option>"
                    );
                }

                var selectedMerchantId = $merchantDropdown.data("selected-merchant");


                if (selectedMerchantId) {
                    $merchantDropdown.val(selectedMerchantId).trigger('change');
                }
            });
        }

        function getBanks() {
            $bankDropdown.empty();
            $bankDropdown.append("<option selected value=''>" + app.localize('SelectBank') + "</option>");

            _recipientBankAccountsService.getBankNames({}).done(function (result) {
                if (result.length === 0) {
                    return;
                }

                for (var i = 0; i < result.length; i++) {
                    $bankDropdown.append(
                        "<option value='" + result[i].bankName + "'>" +
                        result[i].bankShortName + " [" + result[i].bankCode + "]" +
                        "</option>"
                    );
                }

                var selectedBankName = $bankDropdown.data("selected-bankname");

                if (selectedBankName) {
                    $bankDropdown.val(selectedBankName).trigger('change');
                }
            });
        }

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _$recipientBankAccountsForm = _modalManager.getModal().find('form[name=RecipientBankAccountsForm]');
            _$recipientBankAccountsForm.validate();

            getMerchants(); 
            getBanks()
        };

        this.save = function () {
            if (!_$recipientBankAccountsForm.valid()) {
                return;
            }

            var recipient = _$recipientBankAccountsForm.serializeFormToObject();
            recipient.merchantId = $("#MerchantId").val();
            recipient.bankName = $("#BankName").val();

            _modalManager.setBusy(true);

            _recipientBankAccountsService.createOrEdit(recipient)
                .done(function (result) {
                    if (result.success) {
                        abp.notify.success(app.localize('SavedSuccessfully'));
                        _modalManager.close();
                        abp.event.trigger('app.createOrEditRecipientModalSaved');
                    } else {
                        if (result.duplicates && result.duplicates.length > 0) {
                            let duplicateList = $("#duplicateRecordsList");
                            duplicateList.empty(); // Clear old data

                            result.duplicates.forEach(function (detail) {
                                duplicateList.append("<li>" + detail + "</li>");
                            });

                            _modalManager.close();
                            $("#duplicateRecordsModal").modal("show");
                        } else {
                            abp.message.warn(result.message || "A duplicate record was found.");
                        }
                    }
                })
                .fail(function (error) {
                    console.error("Error:", error);
                    abp.message.error("An error occurred.");
                })
                .always(function () {
                    _modalManager.setBusy(false);
                });
        };



    };
})(jQuery);
