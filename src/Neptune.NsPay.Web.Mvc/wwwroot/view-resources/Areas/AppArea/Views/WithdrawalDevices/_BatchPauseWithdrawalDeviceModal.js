(function ($) {
    app.modals.BatchPauseWithdrawalDeviceModal = function () {
        var _modalManager;
        var _$withdrawalDeviceInformationForm = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            var modal = _modalManager.getModal();
            modal.on('shown.bs.modal', function () {
                // Reinitialize Select2 after modal is shown

                modal.find('.date-picker').daterangepicker({
                    singleDatePicker: true,
                    locale: abp.localization.currentLanguage.name,
                    format: 'L'
                });
                modal.find('.select2').select2({
                    placeholder: app.localize('Select'),
                    theme: 'bootstrap5',
                    width: "100%",
                    selectionCssClass: 'form-select',
                    language: abp.localization.currentCulture.name,
                    dropdownParent: modal
                });
            });

            _$withdrawalDeviceInformationForm = _modalManager.getModal().find('form[name=BatchPauseWithdrawalDeviceForm]');
        };

        this.save = function () {
            if (!_$withdrawalDeviceInformationForm.valid()) {
                return;
            }

            var batchPauseWithdrawalDevice = _$withdrawalDeviceInformationForm.serializeFormToObject();

            _modalManager.setBusy(true);

            abp.message.confirm(
                `${app.localize('Pause')} ${app.localize('Enum_WithdrawalDevicesBankTypeEnum_' + batchPauseWithdrawalDevice.pauseBank)} ${app.localize('WithdrawalDevices')}`,
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        abp.services.app.withdrawalDevices
                            .pauseBank(batchPauseWithdrawalDevice.pauseBank)
                            .done(function () {
                                abp.notify.success(app.localize('SavedSuccessfully'));
                            })
                            .fail(function () {
                                abp.notify.error("Failed to validate bank details.");
                            })
                            .always(function () {
                                $("#GetWithdrawalDevicesButton").click()
                                _modalManager.setBusy(false);
                                _modalManager.close();
                            });
                    }
                    else {
                        _modalManager.setBusy(false);
                    }
                }
            );
        };
    };
})(jQuery);