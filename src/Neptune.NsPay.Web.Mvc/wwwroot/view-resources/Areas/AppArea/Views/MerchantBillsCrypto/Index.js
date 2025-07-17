(function () {
    $(function () {
        const _$merchantBillsTable = $('#MerchantBillsTable');
        const _merchantBillsService = abp.services.app.merchantBills;
        let _suppressEvents = false;

        $('.select2').select2({
            placeholder: app.localize('Select'),
            theme: 'bootstrap5',
            width: "100%",
            allowClear: true,
            selectionCssClass: 'form-select',
            language: abp.localization.currentCulture.name,
        });

        $('.date-picker').on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY'));
        });

        var _selectedDateRang = {
            startDate: moment().startOf('day').add(-1, 'day'),
            endDate: moment().startOf('day').add(+1, 'day')
        };

        var _selectedTransactionTimeDateRang = {
            startDate: moment().startOf('day').add(-1, 'day'),
            endDate: moment().startOf('day').add(+1, 'day')
        };

        const dataTable = _$merchantBillsTable.DataTable({
            paging: true,
            deferRender: true,
            serverSide: true,
            searching: false,
            processing: true,
            ordering: false,
            listAction: {
                ajaxFunction: _merchantBillsService.getAllCrypto,
                inputFilter: function () {
                    return {
                        filter: $('#MerchantBillsTableFilterId').val(),
                        merchantCodeFilter: $('#MerchantCodeFilterId').val(),
                        billNoFilter: $('#BillNoFilterId').val(),
                        billTypeFilter: $('#BillTypeFilterId').val(),
                        utcTimeFilter: $('#UtcTimeFilterId').val(),
                        minCreationTimeFilter: _selectedDateRang.startDate,
                        maxCreationTimeFilter: _selectedDateRang.endDate,
                        minTransactionTimeFilter: _selectedTransactionTimeDateRang.startDate,
                        maxTransactionTimeFilter: _selectedTransactionTimeDateRang.endDate,
                    };
                },
                done: getMerchants()
            },
            columnDefs: [
                {
                    targets: 0,
                    data: "merchantBill.merchantCode",
                    name: "merchantCode",
                    render: function (data, type, full, meta) {
                        return full.merchantName + "[" + full.merchantBill.merchantCode + "]";
                    }
                },
                {
                    targets: 1,
                    data: "merchantBill.billNo",
                    name: "billNo"
                },
                {
                    targets: 2,
                    data: "merchantBill.billType",
                    name: "billType",
                    render: function (billType) {
                        return app.localize('Enum_MerchantBillTypeEnum_' + billType);
                    }
                },
                {
                    targets: 3,
                    data: "merchantBill.money",
                    name: "money"
                },
                {
                    targets: 4,
                    data: "merchantBill.feeMoney",
                    name: "feeMoney"
                },
                {
                    targets: 5,
                    data: "merchantBill.balanceBefore",
                    name: "balanceBefore"
                },
                {
                    targets: 6,
                    data: "merchantBill.balanceAfter",
                    name: "balanceAfter"
                },
                {
                    targets: 7,
                    data: "merchantBill.creationTime",
                    name: "creationTime",
                    render: function (creationTime) {
                        if (creationTime) {
                            return moment(creationTime).format('YYYY-MM-DD HH:mm:ss');
                        }
                        return "";
                    }
                },
                {
                    targets: 8,
                    data: "merchantBill.transactionTime",
                    name: "transactionTime",
                    render: function (transactionTime) {
                        if (transactionTime) {
                            return moment(transactionTime).format('YYYY-MM-DD HH:mm:ss');
                        }
                        return "";
                    }
                }]
        });

        function getMerchants() {
            _merchantBillsService.getOrderMerchants()
                .done(function (result) {
                    $("#MerchantCodeFilterId").empty();
                    $("#MerchantCodeFilterId").append("<option selected value=''>" + app.localize('All') + "</option>");
                    for (var i = 0; i < result.length; i++) {
                        $("#MerchantCodeFilterId").append("<option value='" + result[i].merchantCode + "'>" + result[i].merchantName + "[" + result[i].merchantCode + "]" + "</option>");
                    }
                });
        }

        function getMerchantBills() {
            dataTable.ajax.reload();
        }

        let datePickerRange = $('#OrderCreationTimeRange').daterangepicker(
            $.extend(true, app.createDateRangePickerOptions({
                timePicker: true,
                timePicker24Hour: true,
                timePickerSeconds: true,
                startDate: moment().startOf('day').add(-1, 'day'),
                endDate: moment().startOf('day').add(+2, 'day'),
            }), _selectedDateRang),
            function (start, end) {
                _selectedDateRang.startDate = start.format('YYYY-MM-DD HH:mm:ss');
                _selectedDateRang.endDate = end.format('YYYY-MM-DD HH:mm:ss');
            });

        let dateTransactionPickerRange = $('#OrderTransactionTimeRange').daterangepicker(
            $.extend(true, app.createDateRangePickerOptions({
                timePicker: true,
                timePicker24Hour: true,
                timePickerSeconds: true,
                startDate: moment().startOf('day').add(-1, 'day'),
                endDate: moment().startOf('day').add(+2, 'day'),
            }), _selectedTransactionTimeDateRang),
            function (start, end) {
                _selectedTransactionTimeDateRang.startDate = start.format('YYYY-MM-DD 00:00:00');
                _selectedTransactionTimeDateRang.endDate = end.format('YYYY-MM-DD 23:59:59');
            });

        $('#ShowAdvancedFiltersSpan').click(function () {
            $('#ShowAdvancedFiltersSpan').hide();
            $('#HideAdvancedFiltersSpan').show();
            $('#AdvacedAuditFiltersArea').slideDown();
        });

        $('#HideAdvancedFiltersSpan').click(function () {
            $('#HideAdvancedFiltersSpan').hide();
            $('#ShowAdvancedFiltersSpan').show();
            $('#AdvacedAuditFiltersArea').slideUp();
        });

        $('#ExportToExcelButton').click(function () {
            _merchantBillsService
                .getMerchantBillsToExcel({
                    filter: $('#MerchantBillsTableFilter').val(),
                    merchantCodeFilter: $('#MerchantCodeFilterId').val(),
                    billNoFilter: $('#BillNoFilterId').val(),
                    billTypeFilter: $('#BillTypeFilterId').val(),
                    utcTimeFilter: $('#UtcTimeFilterId').val(),
                    minCreationTimeFilter: _selectedDateRang.startDate,
                    maxCreationTimeFilter: _selectedDateRang.endDate,
                    minTransactionTimeFilter: _selectedTransactionTimeDateRang.startDate,
                    maxTransactionTimeFilter: _selectedTransactionTimeDateRang.endDate,
                })
                .done(function (result) {
                    if (result === "") {
                        abp.notify.info(app.localize('ImportUsersProcessStart'));
                    }
                    else if (result) {
                        var downloadUrl = window.location.origin + result;
                        window.location.href = downloadUrl;
                        abp.notify.info(app.localize('ExportStarted'));
                    } else {
                        abp.notify.error(app.localize('ExportFailed'));
                    }
                })
                .fail(function () {
                    abp.notify.error(app.localize('ExportFailed'));
                });
        });

        abp.event.on('app.createOrEditMerchantBillModalSaved', function () {
            getMerchantBills();
        });

        $('#GetMerchantBillsButton').click(function (e) {
            e.preventDefault();
            getMerchantBills();
        });

        $(document).keypress(function (e) {
            if (e.which === 13) {
                getMerchantBills();
            }
        });

        $('.reload-on-change').change(function (e) {
            if (_suppressEvents) return;
            getMerchantBills();
        });

        $('.reload-on-keyup').keyup(function (e) {
            getMerchantBills();
        });

        $('#btn-reset-filters').click(function (e) {
            _suppressEvents = true;

            $('.reload-on-change,.reload-on-keyup,#MyEntsTableFilter').val('');

            _selectedDateRang.startDate = moment().startOf('day').add(-1, 'day');
            _selectedDateRang.endDate = moment().startOf('day').add(2, 'day');

            datePickerRange.data('daterangepicker').setStartDate(moment().startOf('day').add(-1, 'day'));
            datePickerRange.data('daterangepicker').setEndDate(moment().startOf('day').add(2, 'day'));

            $('select.reload-on-change').each(function (index, element) {
                var firstSelectionValue = $(element).find("option:first").val();

                if ($(element).hasClass("select2")) {
                    $(element).val('').trigger('change');
                }
                else {
                    $(element).val(firstSelectionValue);// drop  downlist select first option
                }
            });

            _suppressEvents = false;

            getMerchantBills();
        });
    });
})();