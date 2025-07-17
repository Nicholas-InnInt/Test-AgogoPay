(function ($) {
    app.modals.CreateOrEditPayMentModal = function () {

        var _payMentsService = abp.services.app.payMents;

        var _modalManager;
        var _$payMentInformationForm = null;





        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$payMentInformationForm = _modalManager.getModal().find('form[name=PayMentInformationsForm]');
            _$payMentInformationForm.validate();
        };



        this.save = function () {
            if (!_$payMentInformationForm.valid()) {
                return;
            }



            var payMent = _$payMentInformationForm.serializeFormToObject();




            _modalManager.setBusy(true);
            _payMentsService.editPayMent(
                payMent
            ).done(function () {
                abp.notify.info(app.localize('SavedSuccessfully'));
                _modalManager.close();
                abp.event.trigger('app.createOrEditPayMentModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };


    };
})(jQuery);