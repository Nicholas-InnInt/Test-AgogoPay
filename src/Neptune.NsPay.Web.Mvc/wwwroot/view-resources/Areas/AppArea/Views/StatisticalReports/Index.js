(function () {
    $(function () {

        var _statisticalReportsAppService = abp.services.app.statisticalReports;

        getMerchants();

        $('.select2').select2({
            placeholder: app.localize('Select'),
            theme: 'bootstrap5',
            width: "100%",
            allowClear: true,
            selectionCssClass: 'form-select',
            language: abp.localization.currentCulture.name,
        });

        var _selectedDateRang = {
            startDate: moment().startOf('day').add(-1, 'day'),
            endDate: moment().startOf('day').add(+1, 'day')
        };

        $('#OrderCreationTimeRange').daterangepicker(
            $.extend(true, app.createDateRangePickerOptions({
                startDate: moment().startOf('day').add(-1, 'day'),
                endDate: moment().startOf('day').add(+1, 'day'),
                locale: {
                    format: 'YYYY-MM-DD HH:mm:ss',
                    timeZone: 'Asia/Ho_Chi_Minh' // 设置时区
                }
            }), _selectedDateRang),
            function (start, end) {
                _selectedDateRang.startDate = start.format('YYYY-MM-DD HH:mm:ss');
                _selectedDateRang.endDate = end.format('YYYY-MM-DD HH:mm:ss');
            }
        );


        $("#GetSearchButton").click(function () {
            $("#GetSearchButton").disabled = true;
            _statisticalReportsAppService.getStatisticalReport({
                orderCreationTimeStartDate: _selectedDateRang.startDate,
                orderCreationTimeEndDate: _selectedDateRang.endDate,
                merchantCodeFilter: $('#MerchantCode').val(),
            }).done(function (data) {
                $("#PayOrderSumCount").text(data.payOrderSumCount);
                $("#PayOrderSuccessCount").text(data.payOrderSuccessCount);
                $("#PayOrderBankFeeMoney").text(data.payOrderBankFeeMoney);
                $("#PayOrderScFeeMoney").text(data.payOrderScFeeMoney);
                $("#PayOrderSuccessBankMoney").text(data.payOrderSuccessBankMoney);
                $("#PayOrderSuccessScMoney").text(data.payOrderSuccessScMoney);
                $("#PayOrderSuccessBankRate").text(data.payOrderSuccessBankRate);
                $("#PayOrderSuccessScRate").text(data.payOrderSuccessScRate);

                $("#WithdrawOrderSumCount").text(data.withdrawOrderSumCount);
                $("#WithdrawOrderSuccessCount").text(data.withdrawOrderSuccessCount);
                $("#WithdrawOrderFeeMoney").text(data.withdrawOrderFeeMoney);
                $("#WithdrawOrderSuccessMoney").text(data.withdrawOrderSuccessMoney);
                $("#WithdrawOrderSuccessRate").text(data.withdrawOrderSuccessRate);

                $("#MerchantSumBalance").text(data.merchantSumBalance);
                $("#MerchantFrozenBalance").text(data.merchantFrozenBalance);
                $("#MerchantWithdrawMoney").text(data.merchantWithdrawMoney);
            }).always(function () {
                $("#GetSearchButton").disabled = false;
            });
        })

        $("#GetSearchButton").click();

        function getMerchants() {
            _statisticalReportsAppService.getMerchants({
            }).done(function (result) {
                $("#MerchantCode").empty();
                $("#MerchantCode").append("<option selected value=''>" + app.localize('All') + "</option>");
                for (var i = 0; i < result.length; i++) {
                    $("#MerchantCode").append("<option value='" + result[i].merchantCode + "'>" + result[i].merchantName + "[" + result[i].merchantCode + "]" + "</option>");
                }
            });
        }

    });
})();