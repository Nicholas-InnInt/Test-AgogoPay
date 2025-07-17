(function () {
    $(function () {

        var _statisticalBankReportsAppService = abp.services.app.statisticalBankReports;

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

        function GetTbody(data) {
            var html = '<tr>' +
                '<td class="ps-0"><span class="text-gray-800 fw-bold d-block fs-6">' + data.name + '</span><br/><span class="text-gray-800 fw-bold d-block fs-6">' + data.cardNumber + '</span></td>' +
                '<td class="ps-0"><span class="text-gray-800 fw-bold d-block fs-6">' + data.orderCount + '</span><br/><span class="text-gray-800 fw-bold d-block fs-6">' + data.orderMoney + '</span></td>' +
                '<td class="ps-0"><span class="text-gray-800 fw-bold d-block fs-6">' + data.depositcCRDTCount + '</span></td>' +
                '<td class="ps-0"><span class="text-gray-800 fw-bold d-block fs-6">' + data.depositcCRDTSuccessCount + '</span></td>' +
                '<td class="ps-0"><span class="text-gray-800 fw-bold d-block fs-6">' + data.depositcCRDTSuccessMoney + '</span></td>' +
                '<td class="ps-0"><span class="text-gray-800 fw-bold d-block fs-6">' + data.depositcCRDTAssociatedCount + '</span></td>' +
                '<td class="ps-0"><span class="text-gray-800 fw-bold d-block fs-6">' + data.depositcCRDTAssociatedMoney + '</span></td>' +
                '<td class="ps-0"><span class="text-gray-800 fw-bold d-block fs-6">' + data.depositcCRDTRejectCount + '</span></td>' +
                '<td class="ps-0"><span class="text-gray-800 fw-bold d-block fs-6">' + data.depositcCRDTRejectMoney + '</span></td>' +
                '<td class="ps-0"><span class="text-gray-800 fw-bold d-block fs-6">' + data.depositcDBITMoney + '</span></td>' +
                '</tr>';
            return html;
        }


        $("#GetSearchButton").click(function () {
            $("#GetSearchButton").disabled = true;
            _statisticalBankReportsAppService.getStatisticalBankReport({
                orderCreationTimeStartDate: _selectedDateRang.startDate,
                orderCreationTimeEndDate: _selectedDateRang.endDate,
                cardNumberFilter: $('#CardNumberFilter').val(),
            }).done(function (data) {
                for (const [key, value] of Object.entries(data)) {
                    $(`#${key}body`).empty();
                    $.each(value, function (index, obj) {
                        var html = GetTbody(obj)
                        $(`#${key}body`).append(html);
                    });
                }
            }).always(function () {
                $("#GetSearchButton").disabled = false;
            });
        })

        $("#GetSearchButton").click();
    });
})();