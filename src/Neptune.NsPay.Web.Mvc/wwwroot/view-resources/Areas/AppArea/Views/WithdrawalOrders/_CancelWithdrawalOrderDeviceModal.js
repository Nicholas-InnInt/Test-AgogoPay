(function ($) {
    app.modals.EditWithdrawalOrderDeviceModal = function () {

        var _withdrawalOrderService = abp.services.app.withdrawalOrders;

        var _modalManager;
        var _$editWithdrawalDeviceForm = null;

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



            _$editWithdrawalDeviceForm = _modalManager.getModal().find('form[name=EditWithdrawalDeviceForm]');

            _$editWithdrawalDeviceForm.validate();
        };



        this.save = function () {
            if (!_$editWithdrawalDeviceForm.valid()) {
                return;
            }

            var withdrawalDevice = _$editWithdrawalDeviceForm.serializeFormToObject();

            withdrawalDevice.OrderNos = [];
            withdrawalDevice.WithdrawalIds = [];

            // Collect hidden inputs for OrderNos
            $('input[name^="OrderNos"]').each(function () {
                withdrawalDevice.OrderNos.push($(this).val());
            });

            // Collect hidden inputs for WithdrawalIds
            $('input[name^="WithdrawalIds"]').each(function () {
                withdrawalDevice.WithdrawalIds.push($(this).val());
            });
            console.log(withdrawalDevice);
            _modalManager.setBusy(true);
            _withdrawalOrderService.updateCancelWithdrawalOrderDevice(
                withdrawalDevice
            ).done(function (response) {
                if (response === true) {
                    abp.notify.info(app.localize('SavedSuccessfully'));
                    _modalManager.close();
                    abp.event.trigger('app.editWithdrawalOrderDeviceModalSaved');
                } else {
                    abp.message.error(app.localize('SavedFailed'));
                }
            })
                .fail(function () {
                    abp.message.error(app.localize('SavedFailed'));
                })
                .always(function () {
                    _modalManager.setBusy(false);
                });
        };

    };
})(jQuery);