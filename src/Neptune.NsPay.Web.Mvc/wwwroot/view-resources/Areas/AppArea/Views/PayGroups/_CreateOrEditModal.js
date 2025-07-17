(function ($) {
    app.modals.CreateOrEditPayGroupModal = function () {

        var _payGroupsService = abp.services.app.payGroups;

        var _modalManager;
        var _$payGroupInformationForm = null;

		
		
		

        this.init = function (modalManager) {
            _modalManager = modalManager;

			var modal = _modalManager.getModal();
            modal.find('.date-picker').daterangepicker({
                singleDatePicker: true,
                locale: abp.localization.currentLanguage.name,
                format: 'L'
            });

            

            _$payGroupInformationForm = _modalManager.getModal().find('form[name=PayGroupInformationsForm]');
            _$payGroupInformationForm.validate();
        };

		  

        this.save = function () {
            if (!_$payGroupInformationForm.valid()) {
                return;
            }

            

            var payGroup = _$payGroupInformationForm.serializeFormToObject();
            
            
            
			
			 _modalManager.setBusy(true);
			 _payGroupsService.createOrEdit(
				payGroup
			 ).done(function () {
               abp.notify.info(app.localize('SavedSuccessfully'));
               _modalManager.close();
               abp.event.trigger('app.createOrEditPayGroupModalSaved');
			 }).always(function () {
               _modalManager.setBusy(false);
			});
        };
        
        
    };
})(jQuery);