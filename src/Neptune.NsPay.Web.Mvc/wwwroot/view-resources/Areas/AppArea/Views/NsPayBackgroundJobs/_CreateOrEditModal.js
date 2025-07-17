(function ($) {
    app.modals.CreateOrEditNsPayBackgroundJobModal = function () {

        var _nsPayBackgroundJobsService = abp.services.app.nsPayBackgroundJobs;

        var _modalManager;
        var _$nsPayBackgroundJobInformationForm = null;

		
		
		

        this.init = function (modalManager) {
            _modalManager = modalManager;

			var modal = _modalManager.getModal();
            modal.find('.date-picker').daterangepicker({
                singleDatePicker: true,
                locale: abp.localization.currentLanguage.name,
                format: 'L'
            });

            

            _$nsPayBackgroundJobInformationForm = _modalManager.getModal().find('form[name=NsPayBackgroundJobInformationsForm]');
            _$nsPayBackgroundJobInformationForm.validate();
        };

		  

        this.save = function () {
            if (!_$nsPayBackgroundJobInformationForm.valid()) {
                return;
            }

            

            var nsPayBackgroundJob = _$nsPayBackgroundJobInformationForm.serializeFormToObject();
            
            
            
			
			 _modalManager.setBusy(true);
			 _nsPayBackgroundJobsService.createOrEdit(
				nsPayBackgroundJob
			 ).done(function () {
               abp.notify.info(app.localize('SavedSuccessfully'));
               _modalManager.close();
               abp.event.trigger('app.createOrEditNsPayBackgroundJobModalSaved');
			 }).always(function () {
               _modalManager.setBusy(false);
			});
        };
        
        
    };
})(jQuery);