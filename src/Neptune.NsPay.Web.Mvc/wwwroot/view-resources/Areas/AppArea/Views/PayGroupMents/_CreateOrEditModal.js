(function ($) {
    app.modals.CreateOrEditPayGroupMentModal = function () {

        var _payGroupMentsService = abp.services.app.payGroupMents;

        var _modalManager;
        var _$payGroupMentInformationForm = null;

		
		
		

        this.init = function (modalManager) {
            _modalManager = modalManager;

            var payGroupId = _modalManager.getModal().find("#PayGroupMent_Group").val();
            var payGroupText = _modalManager.getModal().find("#PayGroupMent_GroupName").val();
            if (payGroupId == "") {
                abp.message.warn(app.localize('PleaseSelectTemplate'));
                return;
            }

            if (payGroupId) {
                $("#PayGroupMent_Group").val(payGroupId);
                $("#PayGroupMent_GroupName").val(payGroupText);

                $('#TextPayMentSelectionCombobox').select2({
                    theme: 'bootstrap5',
                    selectionCssClass: 'form-select',
                    language: abp.localization.currentCulture.name,
                    width: '100%',
                    multiple: true
                });
            }

            

            _$payGroupMentInformationForm = _modalManager.getModal().find('form[name=PayGroupMentInformationsForm]');
            _$payGroupMentInformationForm.validate({
                rules: {
                    "payMentIds[]": {
                        atLeastOneSelected: true // Custom rule for your multi-select dropdown
                    }
                },
                messages: {
                    "payMentIds[]": {
                        atLeastOneSelected: app.localize('AtleastOneSelected_Validation_Error')
                    }
                }
            });
            _modalManager.getModal().find('#TextPayMentSelectionCombobox').on('change', function () {
                $(this).valid();  // Trigger validation for the select element on change
            });
        };

		  

        this.save = function () {
            if (!_$payGroupMentInformationForm.valid()) {
                return;
            }

            

            var payGroupMent = _$payGroupMentInformationForm.serializeFormToObject();
            
            
            
			
			 _modalManager.setBusy(true);
			 _payGroupMentsService.createOrEdit(
				payGroupMent
			 ).done(function () {
               abp.notify.info(app.localize('SavedSuccessfully'));
               _modalManager.close();
               abp.event.trigger('app.createOrEditPayGroupMentModalSaved');
			 }).always(function () {
               _modalManager.setBusy(false);
			});
        };
        
        
    };
})(jQuery);