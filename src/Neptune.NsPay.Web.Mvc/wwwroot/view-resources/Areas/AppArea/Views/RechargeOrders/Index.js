(function () {
    $(function () {

        var _rechargeOrdersAppService = abp.services.app.rechargeOrders;
        var _$createRechargeOrderForm = $("#createRechargeOrderForm");
        getMerchants();

        $('.select2').select2({
            placeholder: app.localize("PleaseSelectMerchant"),
            theme: 'bootstrap5',
            width: "100%",
            selectionCssClass: 'form-select',
            language: abp.localization.currentCulture.name,
        });

        _$createRechargeOrderForm.validate({
            rules: {
                computeRate: {
                    notDefaultSelect: true // Ensure the value is not -1
                },
                merchantCode: {
                    notDefaultSelect: true // Ensure the value is not -1
                },
            }
        });

        $('#MerchantCode,#ComputeRateFilterId').on('change', function () {
            $(this).valid();  // Trigger validation for the select element on change
        });
       
        $("#SaveRechargeOrders").click(function () {
            $("#SaveRechargeOrders").prop("disabled", true);

            if (!_$createRechargeOrderForm.valid()) {
                return;
            }

          

            var rechargeOrder = _$createRechargeOrderForm.serializeFormToObject();

            if (rechargeOrder) {
                
                if (rechargeOrder.orderMoney === null || rechargeOrder.orderMoney.trim().length === 0 || rechargeOrder.orderMoney.trim()=="0") {
                    var errormsg = app.localize("OrderMoney") + " " + app.localize("OrderMoneyError");
                        abp.message.error(errormsg);
                    return;
                }
            }

           
            _rechargeOrdersAppService.createRecharge(rechargeOrder).done(function () {
                abp.message.info(app.localize('SavedSuccessfully'));
                resetInput();
            }).always(function () {
                $("#SaveRechargeOrders").disabled = false;
            });
        })


        function resetInput() {
            _$createRechargeOrderForm.find('input').val('');
            _$createRechargeOrderForm.find('select').val(null);
            _$createRechargeOrderForm.find('#MerchantCode').select2("destroy");
            _$createRechargeOrderForm.find('#MerchantCode').select2({
                placeholder: app.localize("PleaseSelectMerchant"),
                theme: 'bootstrap5',
                width: "100%",
                selectionCssClass: 'form-select',
                language: abp.localization.currentCulture.name,
            });
        }


   
        function getMerchants() {
            _rechargeOrdersAppService.getMerchants({
            }).done(function (result) {
                for (var i = 0; i < result.length; i++) {
                    $("#MerchantCode").append("<option value='" + result[i].merchantCode + "'>" + result[i].merchantName + "[" + result[i].merchantCode + "]" + "</option>");
                }
                $("#MerchantCode").val('');
                $("#MerchantCode").select2('val', "");
            });
        }

    });
})();