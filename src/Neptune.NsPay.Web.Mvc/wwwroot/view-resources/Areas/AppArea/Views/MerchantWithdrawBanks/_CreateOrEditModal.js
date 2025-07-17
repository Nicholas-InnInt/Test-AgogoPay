(function ($) {
    app.modals.CreateOrEditMerchantWithdrawBankModal = function () {

        var _merchantWithdrawBanksService = abp.services.app.merchantWithdrawBanks;

        var _modalManager;
        var _$merchantWithdrawBankInformationForm = null;

		
		
		

        this.init = function (modalManager) {
            _modalManager = modalManager;

			var modal = _modalManager.getModal();
            modal.find('.date-picker').daterangepicker({
                singleDatePicker: true,
                locale: abp.localization.currentLanguage.name,
                format: 'L'
            });

            

            _$merchantWithdrawBankInformationForm = _modalManager.getModal().find('form[name=MerchantWithdrawBankInformationsForm]');
            _$merchantWithdrawBankInformationForm.validate();
        };

		  

        this.save = function () {
            if (!_$merchantWithdrawBankInformationForm.valid()) {
                return;
            }

            

            var merchantWithdrawBank = _$merchantWithdrawBankInformationForm.serializeFormToObject();
            
            
            
			
			 _modalManager.setBusy(true);
			 _merchantWithdrawBanksService.createOrEdit(
				merchantWithdrawBank
             ).done(function (result) {
                 if (result === true) {
                     abp.notify.info(app.localize('SavedSuccessfully'));
                 }
                 else {
                     abp.notify.error(app.localize('SavedFailed'));
                 }
               _modalManager.close();
               abp.event.trigger('app.createOrEditMerchantWithdrawBankModalSaved');
			 }).always(function () {
               _modalManager.setBusy(false);
			});
        };
        
        
    };
})(jQuery);