(function ($) {
    app.modals.CreateOrEditWithdrawalDeviceModal = function () {

        var _withdrawalDevicesService = abp.services.app.withdrawalDevices;

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
           

            

            _$withdrawalDeviceInformationForm = _modalManager.getModal().find('form[name=WithdrawalDeviceInformationsForm]');
            _$withdrawalDeviceInformationForm.validate({
                rules: {
                    bankType: {
                        notDefaultSelect: true // Ensure the value is not -1
                    },
                    merchantCode: {
                        notDefaultSelect: true // Ensure the value is not -1
                    },
                }
            });

            $('#WithdrawalDevice_BankType,#WithdrawalDevice_MerchantCode').on('change', function () {
                $(this).valid();  // Trigger validation for the select element on change
            });
        };

		  

        this.save = function () {
            if (!_$withdrawalDeviceInformationForm.valid()) {
                return;
            }

            

            var withdrawalDevice = _$withdrawalDeviceInformationForm.serializeFormToObject();
            
            
            
			
			 _modalManager.setBusy(true);
            abp.services.app.withdrawalDevices.checkDuplicate(withdrawalDevice)
                .done(function (validationMessage) {
                    if (validationMessage === "Success") {
                        abp.services.app.withdrawalDevices.createOrEdit(withdrawalDevice)
                            .done(function () {
                                if (withdrawalDevice.Id == null) {
                                    abp.notify.info(app.localize('SavedSuccessfully'));
                                } else {
                                    abp.notify.info(app.localize('SavedSuccessfully'));
                                }
                                _modalManager.close();
                                abp.event.trigger('app.createOrEditWithdrawalDeviceModalSaved');
                            })
                            .fail(function () {
                                abp.notify.error(app.localize('AnErrorOccurred'));
                            })
                            .always(function () {
                                _modalManager.setBusy(false);
                            });
                    } else {
                        abp.notify.warn(app.localize(validationMessage));
                        _modalManager.setBusy(false); 
                    }
                })
                .fail(function () {
                    // Handle errors during validation
                    abp.notify.error("Failed to validate bank details.");
                    _modalManager.setBusy(false);
                });
        };
        
        
    };
})(jQuery);