(function ($) {
    app.modals.CreateOrEditPayMentModal = function () {
        const _payMentsService = abp.services.app.payMents;

        let _modalManager;
        let _$payMentInformationForm = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            const modal = _modalManager.getModal();
            modal.find('.date-picker').daterangepicker({
                singleDatePicker: true,
                locale: abp.localization.currentLanguage.name,
                format: 'L'
            });

            $('.select2').select2({
                placeholder: app.localize('PayMentsTypePlaceholder'),
                theme: 'bootstrap5',
                width: "100%",
                selectionCssClass: 'form-select',
                minimumResultsForSearch: Infinity,
                language: abp.localization.currentCulture.name,
                dropdownParent: modal
            });

            _$payMentInformationForm = _modalManager.getModal().find('form[name=PayMentInformationsForm]');
            _$payMentInformationForm.validate({
                rules: {
                    type: {
                        notDefaultSelect: true // Ensure the value is not -1
                    },
                    mail:
                    {
                        email: false,
                        emailv1: true
                    }
                }
            });
        };

        this.save = function () {
            if (!_$payMentInformationForm.valid()) return;

            const payMent = _$payMentInformationForm.serializeFormToObject();

            _modalManager.setBusy(true);
            _payMentsService.createOrEdit(payMent)
                .done(function () {
                    abp.notify.info(app.localize('SavedSuccessfully'));
                    _modalManager.close();
                    abp.event.trigger('app.createOrEditPayMentModalSaved');
                })
                .always(function () {
                    _modalManager.setBusy(false);
                });
        };
    };
})(jQuery);