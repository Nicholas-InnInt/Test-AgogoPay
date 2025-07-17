(function ($) {
    app.modals.CreateOrEditPayOrderModal = function () {

        var _payOrdersService = abp.services.app.payOrders;

        var _modalManager;
        var _$payOrderInformationForm = null;

		
		
		

        this.init = function (modalManager) {
            _modalManager = modalManager;

			var modal = _modalManager.getModal();
            modal.find('.date-picker').daterangepicker({
                singleDatePicker: true,
                locale: abp.localization.currentLanguage.name,
                format: 'L'
            });

            

            _$payOrderInformationForm = _modalManager.getModal().find('form[name=PayOrderInformationsForm]');
            _$payOrderInformationForm.validate();
        };

		  

        this.save = function () {
            if (!_$payOrderInformationForm.valid()) {
                return;
            }

            

            var payOrder = _$payOrderInformationForm.serializeFormToObject();
            
            
            
			
			 _modalManager.setBusy(true);
			 _payOrdersService.createOrEdit(
				payOrder
			 ).done(function () {
               abp.notify.info(app.localize('SavedSuccessfully'));
               _modalManager.close();
               abp.event.trigger('app.createOrEditPayOrderModalSaved');
			 }).always(function () {
               _modalManager.setBusy(false);
			});
        };
        
        
    };
})(jQuery);