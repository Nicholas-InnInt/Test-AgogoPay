(function ($) {
    app.modals.CreateOrEditAbpUserMerchantModal = function () {

        var _abpUserMerchantsService = abp.services.app.abpUserMerchants;

        var _modalManager;
        var _$abpUserMerchantInformationForm = null;

		
		
		

        this.init = function (modalManager) {
            _modalManager = modalManager;

			var modal = _modalManager.getModal();
            modal.find('.date-picker').daterangepicker({
                singleDatePicker: true,
                locale: abp.localization.currentLanguage.name,
                format: 'L'
            });

            

            _$abpUserMerchantInformationForm = _modalManager.getModal().find('form[name=AbpUserMerchantInformationsForm]');
            _$abpUserMerchantInformationForm.validate();
        };

		  

        this.save = function () {
            if (!_$abpUserMerchantInformationForm.valid()) {
                return;
            }

            

            var abpUserMerchant = _$abpUserMerchantInformationForm.serializeFormToObject();
            
            
            
			
			 _modalManager.setBusy(true);
			 _abpUserMerchantsService.createOrEdit(
				abpUserMerchant
			 ).done(function () {
               abp.notify.info(app.localize('SavedSuccessfully'));
               _modalManager.close();
               abp.event.trigger('app.createOrEditAbpUserMerchantModalSaved');
			 }).always(function () {
               _modalManager.setBusy(false);
			});
        };
        
        
    };
})(jQuery);