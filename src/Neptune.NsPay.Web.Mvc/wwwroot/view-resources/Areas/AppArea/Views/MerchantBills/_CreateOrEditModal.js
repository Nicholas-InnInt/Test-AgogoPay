(function ($) {
    app.modals.CreateOrEditMerchantBillModal = function () {

        var _merchantBillsService = abp.services.app.merchantBills;

        var _modalManager;
        var _$merchantBillInformationForm = null;

		
        $('.select2').select2({
            placeholder: app.localize('Select'),
            theme: 'bootstrap5',
            width: "100%",
            allowClear: true,
            selectionCssClass: 'form-select',
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

            

            _$merchantBillInformationForm = _modalManager.getModal().find('form[name=MerchantBillInformationsForm]');
            _$merchantBillInformationForm.validate();
        };

		  

        this.save = function () {
            if (!_$merchantBillInformationForm.valid()) {
                return;
            }

            

            var merchantBill = _$merchantBillInformationForm.serializeFormToObject();
            
            
            
			
			 _modalManager.setBusy(true);
			 _merchantBillsService.createOrEdit(
				merchantBill
			 ).done(function () {
               abp.notify.info(app.localize('SavedSuccessfully'));
               _modalManager.close();
               abp.event.trigger('app.createOrEditMerchantBillModalSaved');
			 }).always(function () {
               _modalManager.setBusy(false);
			});
        };
        
        
    };
})(jQuery);