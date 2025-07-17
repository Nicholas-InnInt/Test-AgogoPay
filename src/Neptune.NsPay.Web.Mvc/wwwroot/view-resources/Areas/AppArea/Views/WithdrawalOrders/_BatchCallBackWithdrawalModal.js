(function ($) {
    app.modals.BatchCallBackWithdrawalModal = function () {
        var _modalManager;
        var _form = null;

        $('.select2').select2({
            placeholder: app.localize('Select'),
            theme: 'bootstrap5',
            width: "100%",
            selectionCssClass: 'form-select',
            language: abp.localization.currentCulture.name,
        });

        this.init = function (modalManager) {
            _modalManager = modalManager;

            var modal = _modalManager.getModal();
            modal.find('.date-picker').daterangepicker({
                singleDatePicker: true,
                locale: abp.localization.currentLanguage.name,
                format: 'L'
            });

            _form = _modalManager.getModal().find('form[name=BatchCallBackWithdrawalForm]');

            _form.validate();
        };

        $("button#batchEnforceCallBack").click(function () {
            const withdrawalIds = getWithdrawalIds();

            _modalManager.setBusy(true);

            abp.services.app.withdrawalOrders
                .batchEnforceCallBcak(withdrawalIds)
                .done(function (response) {
                    popupModal(response);
                })
                .fail(function () {
                    abp.message.error(app.localize('SavedFailed'));
                })
                .always(function () {
                    _modalManager.setBusy(false);
                });
        });

        $("button#batchCallBack").click(function () {
            const withdrawalIds = getWithdrawalIds();

            _modalManager.setBusy(true);

            abp.services.app.withdrawalOrders
                .batchCallBcak(withdrawalIds)
                .done(function (response) {
                    popupModal(response);
                })
                .fail(function () {
                    abp.message.error(app.localize('SavedFailed'));
                })
                .always(function () {
                    _modalManager.setBusy(false);
                });
        });

        $("button#batchCallBackCancel").click(function () {
            const withdrawalIds = getWithdrawalIds();

            _modalManager.setBusy(true);

            abp.services.app.withdrawalOrders
                .batchCallBackCancelOrder(withdrawalIds)
                .done(function (response) {
                    popupModal(response);
                })
                .fail(function () {
                    abp.message.error(app.localize('SavedFailed'));
                })
                .always(function () {
                    _modalManager.setBusy(false);
                });
        });

        $("button.save-button").click(function () {
            $(this).prepend('<i class="fa fa-spin fa-spinner"></i>');
        });

        function getWithdrawalIds() {
            const withdrawalIds = [];
            $('input[name^="WithdrawalId"]').each(function () {
                withdrawalIds.push({ id: $(this).val() });
            });
            return withdrawalIds;
        }

        function popupModal(response) {
            let popupContent = app.localize('SuccessUpdateOrder');
            popupContent += (" : <b>" + response.successOrder.length + "</b><br>");

            response.successOrder.forEach(function (element, index, array) {
                popupContent += ("<pre>" + element + "</pre>");
            });

            popupContent += ("<br><br>");
            popupContent += app.localize('FailedUpdateOrder');
            popupContent += (" : <b>" + response.failedOrder.length + "</b><br>")

            response.failedOrder.forEach(function (element, index, array) {
                popupContent += ("<pre>" + element + "</pre>");
            });

            if (response.isSuccess) {
                abp.message.info(popupContent, "", { isHtml: true }).done(() => $("#GetWithdrawalOrdersButton").click());
            }
            else {
                abp.message.warn(popupContent);
            }

            _modalManager.close();
            abp.event.trigger('app.editWithdrawalOrderDeviceModalSaved');
        }

        this.save = function () { };
    };
})(jQuery);