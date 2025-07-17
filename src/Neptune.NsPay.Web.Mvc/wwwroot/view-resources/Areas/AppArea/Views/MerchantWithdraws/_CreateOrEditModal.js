(function ($) {
    app.modals.CreateOrEditMerchantWithdrawModal = function () {

        var _merchantWithdrawsService = abp.services.app.merchantWithdraws;

        var _modalManager;
        var _$merchantWithdrawInformationForm = null;

        $('.select2').select2({
            placeholder: app.localize('Select'),
            theme: 'bootstrap5',
            width: "100%",
            selectionCssClass: 'form-select',
            minimumResultsForSearch: Infinity,
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

            

            _$merchantWithdrawInformationForm = _modalManager.getModal().find('form[name=MerchantWithdrawInformationsForm]');
            _$merchantWithdrawInformationForm.validate();
        };

		  

        this.save = function () {
            if (!_$merchantWithdrawInformationForm.valid()) {
                return;
            }

            

            var merchantWithdraw = _$merchantWithdrawInformationForm.serializeFormToObject();
            
            
            
			
			 _modalManager.setBusy(true);
			 _merchantWithdrawsService.createOrEdit(
				merchantWithdraw
			 ).done(function () {
               abp.notify.info(app.localize('SavedSuccessfully'));
               _modalManager.close();
               abp.event.trigger('app.createOrEditMerchantWithdrawModalSaved');
			 }).always(function () {
               _modalManager.setBusy(false);
			});
        };
        
        
    };
})(jQuery);