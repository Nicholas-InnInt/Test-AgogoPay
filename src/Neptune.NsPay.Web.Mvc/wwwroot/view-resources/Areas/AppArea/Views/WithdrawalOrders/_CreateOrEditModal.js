(function ($) {
    app.modals.CreateOrEditWithdrawalOrderModal = function () {

        var _withdrawalOrdersService = abp.services.app.withdrawalOrders;

        var _modalManager;
        var _$withdrawalOrderInformationForm = null;

		
		
		

        this.init = function (modalManager) {
            _modalManager = modalManager;

			var modal = _modalManager.getModal();
            modal.find('.date-picker').daterangepicker({
                singleDatePicker: true,
                locale: abp.localization.currentLanguage.name,
                format: 'L'
            });

            

            _$withdrawalOrderInformationForm = _modalManager.getModal().find('form[name=WithdrawalOrderInformationsForm]');
            _$withdrawalOrderInformationForm.validate();
        };

		  

        this.save = function () {
            if (!_$withdrawalOrderInformationForm.valid()) {
                return;
            }

            

            var withdrawalOrder = _$withdrawalOrderInformationForm.serializeFormToObject();
            
            
            
			
			 _modalManager.setBusy(true);
			 _withdrawalOrdersService.createOrEdit(
				withdrawalOrder
			 ).done(function () {
               abp.notify.info(app.localize('SavedSuccessfully'));
               _modalManager.close();
               abp.event.trigger('app.createOrEditWithdrawalOrderModalSaved');
			 }).always(function () {
               _modalManager.setBusy(false);
			});
        };
        
        
    };
})(jQuery);