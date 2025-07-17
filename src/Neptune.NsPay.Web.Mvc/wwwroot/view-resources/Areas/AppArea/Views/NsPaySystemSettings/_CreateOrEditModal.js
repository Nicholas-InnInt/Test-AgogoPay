(function ($) {
    app.modals.CreateOrEditNsPaySystemSettingModal = function () {

        var _nsPaySystemSettingsService = abp.services.app.nsPaySystemSettings;

        var _modalManager;
        var _$nsPaySystemSettingInformationForm = null;

        $('.select2').select2({
            placeholder: app.localize('Select'),
            theme: 'bootstrap5',
            width:"100%",
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

            

            _$nsPaySystemSettingInformationForm = _modalManager.getModal().find('form[name=NsPaySystemSettingInformationsForm]');
            _$nsPaySystemSettingInformationForm.validate();
        };

		  

        this.save = function () {
            if (!_$nsPaySystemSettingInformationForm.valid()) {
                return;
            }

            

            var nsPaySystemSetting = _$nsPaySystemSettingInformationForm.serializeFormToObject();
            
            
            
			
			 _modalManager.setBusy(true);
			 _nsPaySystemSettingsService.createOrEdit(
				nsPaySystemSetting
			 ).done(function () {
               abp.notify.info(app.localize('SavedSuccessfully'));
               _modalManager.close();
               abp.event.trigger('app.createOrEditNsPaySystemSettingModalSaved');
			 }).always(function () {
               _modalManager.setBusy(false);
			});
        };
        
        
    };
})(jQuery);