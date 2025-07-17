(function ($) {
    app.modals.AssociatedOrderModal = function () {
        var _payOrdersService = abp.services.app.payOrderDeposits;
        var _modalManager;
        var _$associatedOrderForm = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$associatedOrderForm = _modalManager.getModal().find('form[name=AssociatedOrderForm]');
            _$associatedOrderForm.validate();
        };

        this.save = function () {
            if (!_$associatedOrderForm.valid()) {
                return;
            }
            var associatedOrder = _$associatedOrderForm.serializeFormToObject();
            _modalManager.setBusy(true);
            _payOrdersService.associatedCryptoOrder(associatedOrder)
                .done(function () {
                    abp.notify.info(app.localize('SavedSuccessfully'));
                    _modalManager.close();
                    abp.event.trigger('app.associatedOrderModalSaved');
                }).always(function () {
                    _modalManager.setBusy(false);
                });
        };
    };
})(jQuery);