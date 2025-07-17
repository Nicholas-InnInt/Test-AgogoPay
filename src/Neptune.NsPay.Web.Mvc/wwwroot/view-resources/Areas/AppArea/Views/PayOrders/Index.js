(function () {
    $(function () {
        var _$payOrdersTable = $('#PayOrdersTable');
        var _payOrdersService = abp.services.app.payOrders;
        let _suppressEvents = false;

        $('.select2').select2({
            placeholder: 'Select',
            theme: 'bootstrap5',
            width: "100%",
            allowClear: true,
            selectionCssClass: 'form-select',
            language: abp.localization.currentCulture.name,
        });

        var $selectedDate = {
            startDate: null,
            endDate: null,
        }

        $('.date-picker').on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY'));
        });

        $('.startDate').daterangepicker({
            autoUpdateInput: false,
            singleDatePicker: true,
            locale: abp.localization.currentLanguage.name,
            format: 'L',
        })
            .on("apply.daterangepicker", (ev, picker) => {
                $selectedDate.startDate = picker.startDate;
                getPayOrders();
            })
            .on('cancel.daterangepicker', function (ev, picker) {
                $(this).val("");
                $selectedDate.startDate = null;
                getPayOrders();
            });

        $('.endDate').daterangepicker({
            autoUpdateInput: false,
            singleDatePicker: true,
            locale: abp.localization.currentLanguage.name,
            format: 'L',
        })
            .on("apply.daterangepicker", (ev, picker) => {
                $selectedDate.endDate = picker.startDate;
                getPayOrders();
            })
            .on('cancel.daterangepicker', function (ev, picker) {
                $(this).val("");
                $selectedDate.endDate = null;
                getPayOrders();
            });

        const _permissions = {
            create: abp.auth.hasPermission('Pages.PayOrders.Create'),
            edit: abp.auth.hasPermission('Pages.PayOrders.Edit'),
            'delete': abp.auth.hasPermission('Pages.PayOrders.Delete'),
            callback: abp.auth.hasPermission('Pages.PayOrders.CallBcak'),
            enforcecallbcak: abp.auth.hasPermission('Pages.PayOrders.EnforceCallBcak'),
            immediatecallBcak: abp.auth.hasPermission('Pages.PayOrders.ImmediateCallBcak'),
            addMerchantBill: abp.auth.hasPermission('Pages.PayOrders.AddMerchantBill'),
            view: abp.auth.hasPermission('Pages.PayOrders.View'),
            viewBankInfo: abp.auth.hasPermission('Pages.PayOrders.View.BankInfo'),
        };

        var _selectedDateRang = {
            startDate: moment().startOf('day').add(-2, 'day'),
            endDate: moment().startOf('day').add(+1, 'day')
        };

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/PayOrders/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/PayOrders/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditPayOrderModal'
        });

        var _viewPayOrderModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/PayOrders/ViewpayOrderModal',
            modalClass: 'ViewPayOrderModal'
        });

        var _editPayOrderCallback = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/PayOrders/EditPayOrderModel',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/PayOrders/_EditPayOrder.js',
            modalClass: 'EditPayOrderModel'
        });

        var dataTable = _$payOrdersTable.DataTable({
            paging: true,
            serverSide: true,
            processing: true,
            deferRender: true,
            searching: false,
            ordering: false,
            listAction: {
                ajaxFunction: _payOrdersService.getAll,
                inputFilter: function () {
                    return {
                        filter: $('#PayOrdersTableFilter').val(),
                        merchantCodeFilter: $('#MerchantCodeFilterId').val(),
                        orderNoFilter: $('#OrderNoFilterId').val(),
                        orderMarkFilter: $('#OrderMarkFilterId').val(),
                        orderPayTypeFilter: $('#OrderPayTypeFilterId').val(),
                        orderTypeFilter: $('#OrderTypeFilterId').val(),
                        orderStatusFilter: $('#OrderStatusFilterId').val(),
                        scoreStatusFilter: $('#ScoreStatusFilterId').val(),
                        orderBankFilter: $('#OrderBankFilterId').val(),
                        orderCreationTimeStartDate: _selectedDateRang.startDate,
                        orderCreationTimeEndDate: _selectedDateRang.endDate,
                        utcTimeFilter: $('#UtcTimeFilterId').val(),
                        minOrderMoneyFilter: $("#MinOrderMoneyFilterId").val(),
                        maxOrderMoneyFilter: $("#MaxOrderMoneyFilterId").val(),
                        cardNumberFilter: $("#CardNumberFilterId").val()
                    };
                },
                done: getMerchants()
            },
            columnDefs: [
                {
                    targets: 0,
                    width: 10,
                    className: 'control responsive',
                    orderable: false,
                    render: function () {
                        return '';
                    },
                },
                {
                    targets: 1,
                    width: 10,
                    data: "null",
                    name: "checkBox",
                    orderable: false,
                    render: function (data, type, full, meta) {

                        const isPending = (
                            (full.payOrder.orderStatus === 4 ||
                                full.payOrder.orderStatus === 1 ||
                                full.payOrder.orderStatus === 3) &&
                            full.payOrder.scoreStatus === 0 &&
                            _permissions.enforcecallbcak
                        );
                        return (isPending ? (`<input type="checkbox" class="checkbox form-check-input" 
                       data-id="${full.payOrder.id}"  data-orderno="${full.payOrder.orderNumber}"/>`) : "");
                    }
                },
                {
                    targets: 2,
                    width: 10,
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    rowAction: {
                        cssClass: 'btn btn-brand dropdown-toggle',
                        text: '<i class="fa fa-cog"></i> ' + app.localize('Actions') + ' <span class="caret"></span>',
                        items: [
                            {
                                text: app.localize('View'),
                                iconStyle: 'far fa-eye mr-2',
                                action: function (data) {
                                    _viewPayOrderModal.open({ id: data.record.payOrder.id });
                                }
                            },
                            {
                                text: app.localize('OrderCallBack'),
                                iconStyle: 'far fa-edit mr-2',
                                visible: function (data) {
                                    if (data.record.payOrder.orderStatus == 5 && data.record.payOrder.scoreStatus == 2 && _permissions.callback) {
                                        return true;
                                    } else {
                                        return false;
                                    }
                                },
                                action: function (data) {
                                    callbackpayOrder(data.record.payOrder);
                                }
                            },
                            {
                                text: app.localize('EnforceCallBcak'),
                                iconStyle: 'far fa-edit mr-2',
                                visible: function (data) {
                                    if ((data.record.payOrder.orderStatus == 4 || data.record.payOrder.orderStatus == 1 || data.record.payOrder.orderStatus == 3) && data.record.payOrder.scoreStatus == 0 && _permissions.enforcecallbcak) {
                                        return true;
                                    } else {
                                        return false;
                                    }
                                },
                                action: function (data) {
                                    enforcecallbackpayOrder(data.record.payOrder);
                                },
                            }
                        ]
                    }
                },
                {
                    width: 130,
                    targets: 3,
                    orderable: false,
                    data: "payOrder.merchantCode",
                    name: "merchantCode",
                    render: function (data, type, full, meta) {
                        return "<span>" + app.localize('MerchantName') + "\uff1a" + full.merchantName + "</span><br/>" +
                            "<span>" + app.localize('MerchantCode') + "\uff1a" + full.payOrder.merchantCode + "</span><br/>";
                    }
                },
                {
                    width: 200,
                    targets: 4,
                    orderable: false,
                    data: "payOrder.orderNumber",
                    name: "orderNumber",
                    render: function (data, type, full, meta) {
                        var transactionTime = full.payOrder.transactionTime;
                        var transactionNo = full.payOrder.transactionNo;

                        if (transactionNo == null) {
                            transactionNo = "";
                        }

                        var tranHtml = "";
                        var tranNoHtml = "";
                        if (_permissions.view) {
                            tranNoHtml = "<span>" + app.localize('TransactionNo') + "\uff1a" + transactionNo + "</span><br/>";
                            tranHtml = "<span>" + app.localize('CallBackTime') + "\uff1a" + moment(full.payOrder.transactionTime).format('YYYY-MM-DD HH:mm:ss') + "</span><br/>";
                        }
                        return "<span>" + app.localize('OrderNumber') + "\uff1a" + full.payOrder.orderNumber + "</span><br/>" + tranNoHtml +
                            "<span>" + app.localize('CreationTime') + "\uff1a" + moment(full.payOrder.creationTime).format('YYYY-MM-DD HH:mm:ss') + "</span> <br/>" + tranHtml;
                    }
                },
                {
                    targets: 5,
                    orderable: false,
                    width: 120,
                    data: "payOrder.orderMoney",
                    name: "orderMoney",
                    render: function (data, type, full, meta) {
                        return "<span>" + app.localize('OrderMoney') + "\uff1a" + full.payOrder.orderMoney + "</span><br/>" +
                            "<span>" + app.localize('FeeMoney') + "\uff1a" + full.payOrder.feeMoney + "</span>";
                    }
                },
                {
                    targets: 6,
                    orderable: false,
                    width: 110,
                    data: "payOrder.orderStatus",
                    name: "orderStatus",
                    render: function (data, type, full, meta) {
                        var status = {
                            1: {
                                'title': app.localize('OrderStatusPending'),
                                'class': ' badge-light-primary'
                            },
                            2: {
                                'title': app.localize('OrderStatusPaid'),
                                'class': ' badge-light-info'
                            },
                            3: {
                                'title': app.localize('OrderStatusDelivered'),
                                'class': ' badge-light-danger'
                            },
                            4: {
                                'title': app.localize('OrderStatusTimeOut'),
                                'class': ' badge-light-danger'
                            },
                            5: {
                                'title': app.localize('OrderStatusSuccess'),
                                'class': ' badge-light-success'
                            }
                        };

                        var scoreStatus = {
                            0: {
                                'title': app.localize('ScoreStatusNoScore'),
                                'class': ' badge-light-primary'
                            },
                            1: {
                                'title': app.localize('ScoreStatusSuccess'),
                                'class': ' badge-light-success'
                            },
                            2: {
                                'title': app.localize('ScoreStatusFail'),
                                'class': ' badge-light-danger'
                            },
                        };

                        return "<span>" + app.localize('OrderStatus') + "\uff1a" + '<span class="badge label-lg font-weight-bold' + status[full.payOrder.orderStatus].class + ' label-inline">' + app.localize('Enum_PayOrderOrderStatusEnum_' + full.payOrder.orderStatus) + '</span>' + "</span><br/>" +
                            "<span>" + app.localize('ScoreStatus') + "\uff1a" + '<span class="badge label-lg font-weight-bold' + scoreStatus[full.payOrder.scoreStatus].class + ' label-inline">' + app.localize('Enum_PayOrderScoreStatusEnum_' + full.payOrder.scoreStatus) + '</span>' + "</span>";
                    }
                },
                {
                    targets: 7,
                    width: 90,
                    orderable: false,
                    data: "payOrder.payTypeStr",
                    name: "payTypeStr",
                    render: function (data, type, full, meta) {
                        var paymentChannel = {
                            "0": {
                                'title': app.localize('NoPay'),
                                'class': ''
                            },
                            "1": {
                                'title': app.localize('ScratchCards'),
                                'class': ''
                            },
                            "2": {
                                'title': app.localize('MoMo'),
                                'class': ''
                            },
                            "3": {
                                'title': app.localize('Enum_PayMentTypeEnum_ScanBank'),
                                'class': ''
                            },
                            "4": {
                                'title': app.localize('Enum_PayMentTypeEnum_OnlineBank'),
                                'class': ''
                            },
                            "5": {
                                'title': app.localize('Enum_PayMentTypeEnum_DirectBank'),
                                'class': ''
                            },
                            "6": {
                                'title': app.localize('Enum_PayMentTypeEnum_SixEightPay'),
                                'class': ''
                            },
                            "7": {
                                'title': app.localize('Enum_PayMentTypeEnum_FengYangPay'),
                                'class': ''
                            },
                            "8": {
                                'title': app.localize('Enum_PayMentTypeEnum_1001'),
                                'class': ''
                            },
                            "9": {
                                'title': app.localize('Enum_PayMentTypeEnum_1002'),
                                'class': ''
                            },
                        };
                        var payType = {
                            "NoPay": {
                                'title': app.localize('NoPay'),
                                'class': ''
                            },
                            "ScratchCards": {
                                'title': app.localize('ScratchCards'),
                                'class': ''
                            },
                            "MoMoPay": {
                                'title': app.localize('MoMo'),
                                'class': ''
                            },
                            "MBBank": {
                                'title': app.localize('MBBank'),
                                'class': ''
                            },
                            "VietinBank": {
                                'title': app.localize('VietinBank'),
                                'class': ''
                            },
                            "TechcomBank": {
                                'title': app.localize('TechcomBank'),
                                'class': ''
                            },
                            "VietcomBank": {
                                'title': app.localize('VietcomBank'),
                                'class': ''
                            },
                            "BidvBank": {
                                'title': app.localize('BidvBank'),
                                'class': ''
                            },
                            "ACBBank": {
                                'title': app.localize('ACBBank'),
                                'class': ''
                            },
                            "PVcomBank": {
                                'title': app.localize('PVcomBank'),
                                'class': ''
                            },
                            "BusinessMbBank": {
                                'title': app.localize('BusinessMbBank'),
                                'class': ''
                            },
                            "BusinessTcbBank": {
                                'title': app.localize('BusinessTcbBank'),
                                'class': ''
                            },
                            "BusinessVtbBank": {
                                'title': app.localize('BusinessVtbBank'),
                            },
                            "ZaloPay": {
                                'title': app.localize('ZaloPay'),
                                'class': ''
                            },
                            "ViettelPay": {
                                'title': app.localize('ViettelPay'),
                                'class': ''
                            },
                            "MsbBank": {
                                'title': app.localize('MsbBank'),
                                'class': ''
                            },
                            "SeaBank": {
                                'title': app.localize('SeaBank'),
                                'class': ''
                            },
                            "BvBank": {
                                'title': app.localize('BvBank'),
                                'class': ''
                            },
                            "NamaBank": {
                                'title': app.localize('NamaBank'),
                                'class': ''
                            },
                            "TPBank": {
                                'title': app.localize('TPBank'),
                                'class': ''
                            },
                            "VPBBank": {
                                'title': app.localize('VPBBank'),
                                'class': ''
                            },
                            "OCBBank": {
                                'title': app.localize('OCBBank'),
                                'class': ''
                            },
                            "EXIMBank": {
                                'title': app.localize('EXIMBank'),
                                'class': ''
                            },
                            "NCBBank": {
                                'title': app.localize('NCBBank'),
                                'class': ''
                            },
                            "HDBank": {
                                'title': app.localize('HDBBank'),
                                'class': ''
                            },
                            "LPBank": {
                                'title': app.localize('LPBBank'),
                                'class': ''
                            },
                            "PGBank": {
                                'title': app.localize('PGBBank'),
                                'class': ''
                            },
                            "VietBank": {
                                'title': app.localize('VIETBBank'),
                                'class': ''
                            },
                            "BacaBank": {
                                'title': app.localize('BACABBank'),
                                'class': ''
                            },
                            "USDT_TRC20": {
                                'title': app.localize('Enum_PayMentTypeEnum_1001'),
                                'class': ''
                            },
                            "USDT_ERC20": {
                                'title': app.localize('Enum_PayMentTypeEnum_1002'),
                                'class': ''
                            },
                        };
                        return '<span class="font-weight-bold">' + paymentChannel[full.payOrder.paymentChannel]?.title + '</span><br/>' +
                            '<span class="font-weight-bold">' + payType[full.payOrder.payTypeStr]?.title + '</span>';
                    }
                },
                {
                    targets: 8,
                    orderable: false,
                    width: 200,
                    data: "payOrder.orderMark",
                    name: "orderMark",
                    visible: _permissions.viewBankInfo,
                    render: function (data, type, full, meta) {
                        let html = "";

                        if (_permissions.viewBankInfo && full.payMent != null) {
                            const phone = full.payMent.phone ?? "";
                            const cardNumber = full.payMent.cardNumber ?? "";
                            const fullname = full.payMent.fullName ?? "";

                            html += `<div>${app.localize('Phone')}: ${phone}</div>`;
                            html += `<div>${app.localize('CardNumber')}: ${cardNumber}</div>`;
                            html += `<div>${app.localize('NameSurname')}: ${fullname}</div>`;
                        }

                        return html;
                    }
                },
                {
                    targets: 9,
                    orderable: false,
                    width: 200,
                    data: "payOrder.orderMark",
                    name: "orderMark",
                    render: function (data, type, full, meta) {
                        let html = `<div>${app.localize('OrderMark')}: ${full.payOrder.orderMark}</div>`;

                        if (full.payMent != null) {
                            const phone = full.payMent.phone ?? "";
                            const cardNumber = full.payMent.cardNumber ?? "";

                            if (full.payMent.companyType == 1) {//公司
                                if (full.payOrder.payTypeStr == "ScratchCards") {
                                    html += `<div>Seri: ${full.payOrder.scCode}</div>` + `<div>Code: ${full.payOrder.scSeri}</div>`;
                                    if (full.payOrder.orderStatus == 3 || full.payOrder.orderStatus == 4) {
                                        var errormsg = full.payOrder.errorMsg;
                                        if (errormsg != null) {
                                            html += `<div data-bs-toggle="tooltip" title="${errormsg}">${app.localize('PayOrderErrorMsg')}: errormsg</div>`;
                                        }
                                    }
                                }
                            }

                            return html;
                        }

                        if (full.payOrder.orderMark == "TopUp") {
                            html += `<div>${app.localize('Remark')}: ${full.payOrder.remark}</div>`;
                        }

                        return html;
                    }
                },
            ],
            drawCallback: function (settings) {
                $("#OrderTotal").text(settings.rawServerResponse.totalCount);
                $("#OrderMoneyTotal").text(settings.rawServerResponse.orderMoneyTotal);
                $("#FeeMoneyTotal").text(settings.rawServerResponse.feeMoneyTotal);
                $('.checkbox').off('change').on('change', handleCheckboxChange);
                $('#select-all-chk').off('change').on('change', handleSelectAllCheckboxChanged);
            }
        });

        function getMerchants() {
            _payOrdersService.getOrderMerchants().done(function (result) {
                $("#MerchantCodeFilterId").empty();
                $("#MerchantCodeFilterId").append("<option selected value=''>" + app.localize('All') + "</option>");
                for (var i = 0; i < result.length; i++) {
                    $("#MerchantCodeFilterId").append("<option value='" + result[i].merchantCode + "'>" + result[i].merchantName + "[" + result[i].merchantCode + "]" + "</option>");
                }
            });
        }

        function getPayOrders() {
            dataTable.ajax.reload();
        }

        function callbackpayOrder(payOrder) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _payOrdersService.callBcak({
                            id: payOrder.id
                        }).done(function () {
                            getPayOrders(true);
                            abp.notify.success(app.localize('SuccessCallBack'));
                        });
                    }
                }
            );
        }

        function enforcecallbackpayOrder(payOrder) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _payOrdersService.enforceCallBcak({
                            id: payOrder.id
                        }).done(function (data) {
                            getPayOrders(true);
                            if (data.code == 200) {
                                abp.notify.success(app.localize('SuccessCallBack'));
                            } else {
                                abp.notify.success(data.message);
                            }
                        });
                    }
                }
            );
        }

        var datePickerRange = $('#OrderCreationTimeRange').daterangepicker(
            $.extend(true, app.createDateRangePickerOptions({
                startDate: moment().startOf('day').add(-1, 'day'),
                endDate: moment().startOf('day').add(+1, 'day'),
            }), _selectedDateRang),
            function (start, end) {
                _selectedDateRang.startDate = start.format('YYYY-MM-DD HH:mm:ss');
                _selectedDateRang.endDate = end.format('YYYY-MM-DD HH:mm:ss');
            }
        );

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

        $('#CreateNewPayOrderButton').click(function () {
            _createOrEditModal.open();
        });

        $('#ExportToExcelButton').click(function () {
            const selectedItems = [];
            $('.checkbox:checked').each(function () {
                selectedItems.push({
                    payOrderId: $(this).data('id'),
                });
            });

            const payOrderIds = selectedItems.map(item => item.payOrderId);

            _payOrdersService
                .getPayOrdersToExcel({
                    filter: $('#PayOrdersTableFilter').val(),
                    merchantCodeFilter: $('#MerchantCodeFilterId').val(),
                    orderNoFilter: $('#OrderNoFilterId').val(),
                    orderMarkFilter: $('#OrderMarkFilterId').val(),
                    orderPayTypeFilter: $('#OrderPayTypeFilterId').val(),
                    orderTypeFilter: $('#OrderTypeFilterId').val(),
                    orderStatusFilter: $('#OrderStatusFilterId').val(),
                    scoreStatusFilter: $('#ScoreStatusFilterId').val(),
                    orderCreationTimeStartDate: _selectedDateRang.startDate,
                    orderCreationTimeEndDate: _selectedDateRang.endDate,
                    utcTimeFilter: $('#UtcTimeFilterId').val(),
                    minOrderMoneyFilter: $("#MinOrderMoneyFilterId").val(),
                    maxOrderMoneyFilter: $("#MaxOrderMoneyFilterId").val(),
                    cardNumberFilter: $("#CardNumberFilterId").val(),
                    listOfPayOrderID: payOrderIds
                }, $('#PayOrdersTable').DataTable().page.info().recordsTotal)
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

        abp.event.on('app.createOrEditPayOrderModalSaved', function () {
            getPayOrders();
        });

        $('#GetPayOrdersButton').click(function (e) {
            e.preventDefault();
            getPayOrders();
        });

        $(document).keypress(function (e) {
            if (e.which === 13) {
                getPayOrders();
            }
        });

        $('.reload-on-change').change(function (e) {
            if (_suppressEvents) return;
            getPayOrders();
        });

        $('.reload-on-keyup').keyup(function (e) {
            getPayOrders();
        });

        $('#btn-reset-filters').click(function (e) {
            _suppressEvents = true;

            $('.reload-on-change,.reload-on-keyup,#MyEntsTableFilter').val('');

            _selectedDateRang.startDate = moment().startOf('day').add(-2, 'day');
            _selectedDateRang.endDate = moment().startOf('day').add(1, 'day');

            datePickerRange.data('daterangepicker').setStartDate(moment().startOf('day').add(-2, 'day'));
            datePickerRange.data('daterangepicker').setEndDate(moment().startOf('day').add(1, 'day'));

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

            getPayOrders();
        });

        $('#batchNotify').click(function () {
            const batchNotifyItem = [];
            $('.checkbox:checked').each(function () {
                batchNotifyItem.push({
                    payOrderID: $(this).data('id'),
                    payOrderNumber: $(this).data('orderno'),
                });
            });

            if (batchNotifyItem.length < 1) {
                abp.message.error(app.localize('ErrAtleastOnePayOrder'));
            }
            else {
                _editPayOrderCallback.open({ input: batchNotifyItem });
            }
        });

        function handleSelectAllCheckboxChanged() {
            var isChecked = $(this).prop('checked');
            var lastIndex = $('.checkbox').length - 1;

            $('.checkbox').each(function (index, item) {
                $(item).prop('checked', isChecked);
                if (index == lastIndex) {
                    $(item).trigger('change');
                }
            });
        }

        function handleCheckboxChange() {
            const selectedItems = [];
            $('.checkbox:checked').each(function () {
                selectedItems.push({
                    payOrderId: $(this).data('id'),
                });
            });

            $('#batchNotify').prop('disabled', selectedItems.length === 0); // Enable button if at least one item is selected
        }

        abp.event.on('app.editPayOrderSaved', function () {
            getPayOrders();
        });
    });
})();