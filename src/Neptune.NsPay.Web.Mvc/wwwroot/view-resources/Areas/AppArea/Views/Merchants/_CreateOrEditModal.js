(function ($) {
    app.modals.CreateOrEditMerchantModal = function () {
        const _merchantsService = abp.services.app.merchants;

        let _modalManager;
        let _$merchantInformationForm = null;

        $('.select2').select2({
            placeholder: app.localize('Select'),
            theme: 'bootstrap5',
            width: "100%",
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


            _$merchantInformationForm = _modalManager.getModal().find('form[name=MerchantInformationsForm]');

            $.validator.addMethod(
                "vietnamesePhone",
                function (value, element) {
                    var phoneRegex = /^(0?)(3[2-9]|5[25689]|7[06-9]|8[1-689]|9[0-46-9])([0-9]{7})$/;

                    return this.optional(element) || phoneRegex.test(value);
                },
                app.localize("InvalidPhoneNumber", app.localize("Phone"))
            );

            _$merchantInformationForm.validate({
                rules: {
                    PayGroupId: {
                        notDefaultSelect: true
                    },
                    phone: {
                        required: true,
                        vietnamesePhone: true
                    }
                }
            });

            $('#PayGroupId').on('change', function () {
                $(this).valid();
            });

            $('#Merchant_Phone').on("input", function () {
                // Replace any character that is not a digit with an empty string
                $(this).val($(this).val().replace(/[^0-9]/g, ''));
            });
        };

        this.save = function () {
            if (!_$merchantInformationForm.valid()) {
                return;
            }

            var merchant = _$merchantInformationForm.serializeFormToObject();
            if (merchant) {
                if (merchant.ScanBankRate === null || merchant.ScanBankRate.trim().length === 0) {
                    merchant.ScanBankRate = 0;
                }

                if (merchant.ScratchCardRate === null || merchant.ScratchCardRate.trim().length === 0) {
                    merchant.ScratchCardRate = 0;
                }

                if (merchant.MoMoRate === null || merchant.MoMoRate.trim().length === 0) {
                    merchant.MoMoRate = 0;
                }

                if (merchant.ZaloRate === null || merchant.ZaloRate.trim().length === 0) {
                    merchant.ZaloRate = 0;
                }
            }

            _modalManager.setBusy(true);

            _merchantsService.createOrEdit(merchant)
                .done(function () {
                    abp.notify.info(app.localize('SavedSuccessfully'));
                    _modalManager.close();
                    abp.event.trigger('app.createOrEditMerchantModalSaved');
                })
                .always(function () {
                    _modalManager.setBusy(false);
                });
        };
    };
})(jQuery);