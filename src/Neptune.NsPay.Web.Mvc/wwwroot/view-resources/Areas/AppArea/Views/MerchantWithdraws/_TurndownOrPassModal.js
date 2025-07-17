(function($) {
    app.modals.TurndownOrPassMerchantWithdrawModal = function() {
        var _merchantWithdrawsService = abp.services.app.merchantWithdraws;
        var _modalManager;
        var _$merchantWithdrawInformationForm = null;
        this.init = function(modalManager) {
            _modalManager = modalManager;
            _$merchantWithdrawInformationForm = _modalManager.getModal().find('form[name=WithdrawTurndownOrPassForm]');
            _$merchantWithdrawInformationForm.validate();
        };
        this.save = function() {
            if (!_$merchantWithdrawInformationForm.valid()) {
                return;
            }
            var merchantWithdraw = _$merchantWithdrawInformationForm.serializeFormToObject();
            _modalManager.setBusy(true);
            _merchantWithdrawsService.auditTurndownOrPass(
                merchantWithdraw
            ).done(function() {
                abp.notify.info(app.localize('SavedSuccessfully'));
                _modalManager.close();
                abp.event.trigger('app.turndownOrPassMerchantWithdrawModalSaved');
            }).always(function() {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);