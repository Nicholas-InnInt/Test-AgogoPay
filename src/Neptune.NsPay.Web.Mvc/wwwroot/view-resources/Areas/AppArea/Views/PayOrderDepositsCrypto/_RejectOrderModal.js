(function($) {
    app.modals.RejectOrderModal = function() {
        var _payOrdersService = abp.services.app.payOrderDeposits;
        var _modalManager;
        var _$rejectOrderForm = null;
        this.init = function(modalManager) {
            _modalManager = modalManager;

            _$rejectOrderForm = _modalManager.getModal().find('form[name=RejectOrderForm]');
            _$rejectOrderForm.validate();
        };
        this.save = function() {
            if (!_$rejectOrderForm.valid()) {
                return;
            }
            var rejectOrder = _$rejectOrderForm.serializeFormToObject();
            _modalManager.setBusy(true);
            _payOrdersService.rejectOrder(
                rejectOrder
            ).done(function() {
                abp.notify.info(app.localize('SavedSuccessfully'));
                _modalManager.close();
                abp.event.trigger('app.RejectOrderModalSaved');
            }).always(function() {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);