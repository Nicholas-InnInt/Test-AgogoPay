(function ($) {
    app.modals.CreateOrEditMerchantFundModal = function () {

        var _merchantFundsService = abp.services.app.merchantFunds;

        var _modalManager;
        var _$merchantFundInformationForm = null;

		
		
		

        this.init = function (modalManager) {
            _modalManager = modalManager;

			var modal = _modalManager.getModal();
            modal.find('.date-picker').daterangepicker({
                singleDatePicker: true,
                locale: abp.localization.currentLanguage.name,
                format: 'L'
            });

            

            _$merchantFundInformationForm = _modalManager.getModal().find('form[name=MerchantFundInformationsForm]');
            _$merchantFundInformationForm.validate();
        };

		  

        this.save = function () {
            if (!_$merchantFundInformationForm.valid()) {
                return;
            }

            

            var merchantFund = _$merchantFundInformationForm.serializeFormToObject();
            
            
            
			
			 _modalManager.setBusy(true);
			 _merchantFundsService.createOrEdit(
				merchantFund
			 ).done(function () {
               abp.notify.info(app.localize('SavedSuccessfully'));
               _modalManager.close();
               abp.event.trigger('app.createOrEditMerchantFundModalSaved');
			 }).always(function () {
               _modalManager.setBusy(false);
			});
        };
        
        
    };
})(jQuery);