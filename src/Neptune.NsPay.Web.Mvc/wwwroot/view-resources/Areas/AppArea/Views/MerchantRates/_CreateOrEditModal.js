(function ($) {
    app.modals.CreateOrEditMerchantRateModal = function () {

        var _merchantRatesService = abp.services.app.merchantRates;

        var _modalManager;
        var _$merchantRateInformationForm = null;

		
		
		

        this.init = function (modalManager) {
            _modalManager = modalManager;

			var modal = _modalManager.getModal();
            modal.find('.date-picker').daterangepicker({
                singleDatePicker: true,
                locale: abp.localization.currentLanguage.name,
                format: 'L'
            });

            

            _$merchantRateInformationForm = _modalManager.getModal().find('form[name=MerchantRateInformationsForm]');
            _$merchantRateInformationForm.validate();
        };

		  

        this.save = function () {
            if (!_$merchantRateInformationForm.valid()) {
                return;
            }

            

            var merchantRate = _$merchantRateInformationForm.serializeFormToObject();
            
            
            
			
			 _modalManager.setBusy(true);
			 _merchantRatesService.createOrEdit(
				merchantRate
			 ).done(function () {
               abp.notify.info(app.localize('SavedSuccessfully'));
               _modalManager.close();
               abp.event.trigger('app.createOrEditMerchantRateModalSaved');
			 }).always(function () {
               _modalManager.setBusy(false);
			});
        };
        
        
    };
})(jQuery);