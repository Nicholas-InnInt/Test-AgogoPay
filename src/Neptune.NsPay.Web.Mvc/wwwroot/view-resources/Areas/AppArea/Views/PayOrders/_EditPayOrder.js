(function ($) {
    app.modals.EditPayOrderModel = function () {

        var _payOrdersService = abp.services.app.payOrders;
        var _modalManager;
        var _$editPayorderForm = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _$editPayorderForm = _modalManager.getModal().find('form[name=EditPayorderForm]');
            _$editPayorderForm.validate();
        };

        this.save = function () {
            const batchNotifyItem = [];
            $('input[name="OrderId"]').each(function () {
                const value = $(this).val().trim();  // Trim spaces to avoid any accidental whitespaces.
                if (value) {  // Only push non-empty values
                    batchNotifyItem.push(value);
                }
            });
            _modalManager.setBusy(true);

            _payOrdersService.enforceCallBcakByBatch(batchNotifyItem).done(function (response) {
                if (response.code==200) {
                    abp.notify.info(app.localize('SavedSuccessfully'));
                    _modalManager.close();
                    abp.event.trigger('app.editPayOrderSaved');
                } else {
                    abp.message.error(app.localize('SavedFailed'));
                }
            }).fail(function () {
                abp.message.error(app.localize('SavedFailed'));
            }).always(function () {
                    _modalManager.setBusy(false);
            });

        };
    };
})(jQuery);