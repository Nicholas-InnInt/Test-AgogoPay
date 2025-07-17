(function ($) {
    app.modals.CreateOrEditMerchantSettingModal = function () {

        var _merchantSettingsService = abp.services.app.merchantSettings;

        var _modalManager;
        var _$merchantSettingInformationForm = null;

		
		
		

        this.init = function (modalManager) {
            _modalManager = modalManager;

			var modal = _modalManager.getModal();
            modal.find('.date-picker').daterangepicker({
                singleDatePicker: true,
                locale: abp.localization.currentLanguage.name,
                format: 'L'
            });

            

            _$merchantSettingInformationForm = _modalManager.getModal().find('form[name=MerchantSettingInformationsForm]');
            _$merchantSettingInformationForm.validate();
        };

		  

        this.save = function () {
            if (!_$merchantSettingInformationForm.valid()) {
                return;
            }

            

            var merchantSetting = _$merchantSettingInformationForm.serializeFormToObject();
            
            
            
			
			 _modalManager.setBusy(true);
			 _merchantSettingsService.createOrEdit(
				merchantSetting
			 ).done(function () {
               abp.notify.info(app.localize('SavedSuccessfully'));
               _modalManager.close();
               abp.event.trigger('app.createOrEditMerchantSettingModalSaved');
			 }).always(function () {
               _modalManager.setBusy(false);
			});
        };
        
        
    };
})(jQuery);