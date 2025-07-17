(function ($) {
    app.modals.ViewPayoutDetailsModal = function () {

       // var _withdrawalOrderService = abp.services.app.withdrawalOrders;
        var _modalManager;
        var clipboardModal;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            var modal = _modalManager.getModal();

            clipboardModal = new ClipboardJS("#" + modal.attr("id") + ' .btnCopyModal' ,{
                container: modal[0]
            });
            clipboardModal.on('success', function (e) {
                abp.notify.success(app.localize('SuccessCopy'));
                e.clearSelection();
            });

            modal.on('hidden.bs.modal', function () {
                if (clipboardModal) {
                    clipboardModal.destroy();  // Destroy if necessary (for example, in case you are dynamically creating elements inside the modal)
                    clipboardModal = null; // Reset the clipboard instance
                }
            });
        };
    };
})(jQuery);