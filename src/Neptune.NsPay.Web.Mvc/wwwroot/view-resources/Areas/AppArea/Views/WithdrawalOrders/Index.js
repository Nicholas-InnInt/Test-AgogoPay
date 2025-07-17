(function () {

    $(function () {
        var _$withdrawalOrdersTable = $('#WithdrawalOrdersTable');
        var _withdrawalOrdersService = abp.services.app.withdrawalOrders;
        let _suppressEvents = false;

        $('.select2').select2({
            placeholder: app.localize('Select'),
            theme: 'bootstrap5',
            width: "100%",
            allowClear: true,
            selectionCssClass: 'form-select',
            language: abp.localization.currentCulture.name,
        });

        var clipboard = new ClipboardJS('.btnCopy');
        clipboard.on('success', function (e) {
            abp.notify.success(app.localize('SuccessCopy'));
            e.clearSelection();
        });
        $(".btnCopy").on('click', () => {
            var recoveryCodesText = "";
            // Copy to clipboard
            navigator.clipboard.writeText(recoveryCodesText);
        });

        $(document).keypress(function (e) {
            if (e.which === 13) {
                getWithdrawalOrders();
            }
        });

        var _selectedDateRang = {
            //startDate: moment().startOf('week'),
            //endDate: moment().endOf('day'),
            startDate: moment().startOf('day').add(-1, 'day'),
            endDate: moment().startOf('day').add(+1, 'day')
        };

        var _permissions = {
            create: abp.auth.hasPermission('Pages.WithdrawalOrders.Create'),
            edit: abp.auth.hasPermission('Pages.WithdrawalOrders.Edit'),
            'delete': abp.auth.hasPermission('Pages.WithdrawalOrders.Delete'),
            callback: abp.auth.hasPermission('Pages.WithdrawalOrders.CallBack'),
            view: abp.auth.hasPermission('Pages.WithdrawalOrders.View'),
            cancel: abp.auth.hasPermission('Pages.WithdrawalOrders.Cancel'),
            changedevice: abp.auth.hasPermission('Pages.WithdrawalOrders.ChangeDevice'),
            viewpayoutdetails: abp.auth.hasPermission('Pages.WithdrawalOrders.ViewPayoutDetails'),
            changetopending: abp.auth.hasPermission('Pages.WithdrawalOrders.ChangeToPendingStatus'),
            releasebalance: abp.auth.hasPermission('Pages.WithdrawalOrders.ReleaseBalance')
        };

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/WithdrawalOrders/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/WithdrawalOrders/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditWithdrawalOrderModal'
        });

        var _viewWithdrawalOrderModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/WithdrawalOrders/ViewwithdrawalOrderModal',
            modalClass: 'ViewWithdrawalOrderModal'
        });

        var _editWithdrawalOrderDeviceModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/WithdrawalOrders/EditWithdrawalOrderDeviceModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/WithdrawalOrders/_EditWithdrawalOrderDeviceModal.js',
            modalClass: 'EditWithdrawalOrderDeviceModal'
        });

        var _cancelWithdrawalOrderDeviceModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/WithdrawalOrders/CancelWithdrawalOrderDeviceModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/WithdrawalOrders/_CancelWithdrawalOrderDeviceModal.js',
            modalClass: 'EditWithdrawalOrderDeviceModal'
        });

        var _viewPayoutDetailsModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/WithdrawalOrders/ViewPayoutDetailsModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/WithdrawalOrders/_ViewPayoutDetailsModal.js',
            modalClass: 'ViewPayoutDetailsModal'
        });

        var _viewProofModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/WithdrawalOrders/ViewProofModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/WithdrawalOrders/_ViewProofModal.js',
            modalClass: 'ViewProofModal'
        });

        var _batchCallBackWithdrawalModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/WithdrawalOrders/BatchCallBackWithdrawalModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/WithdrawalOrders/_BatchCallBackWithdrawalModal.js',
            modalClass: 'BatchCallBackWithdrawalModal'
        });

        var $datePicker = $('#OrderCreationTimeRange').daterangepicker(
            $.extend(true, app.createDateRangePickerOptions({
                startDate: moment().startOf('day').add(-1, 'day'),
                endDate: moment().startOf('day').add(+1, 'day'),
            }), _selectedDateRang),
            function (start, end) {
                _selectedDateRang.startDate = start.format('YYYY-MM-DD HH:mm:ss');
                _selectedDateRang.endDate = end.format('YYYY-MM-DD HH:mm:ss');
            }
        );

        var dataTable = _$withdrawalOrdersTable.DataTable({
            paging: true,
            deferRender: true,
            serverSide: true,
            processing: true,
            searching: false,
            ordering: false,
            listAction: {
                ajaxFunction: _withdrawalOrdersService.getAll,
                inputFilter: function () {
                    return {
                        filter: $('#WithdrawalOrdersTableFilter').val(),
                        merchantCodeFilter: $('#MerchantCodeFilterId').val(),
                        orderNoFilter: $('#OrderNoFilterId').val(),
                        orderStatusFilter: $('#OrderStatusFilterId').val(),
                        notifyStatusFilter: $('#NotifyStatusFilterId').val(),
                        benAccountNameFilter: $('#BenAccountNameFilterId').val(),
                        benAccountNoFilter: $('#BenAccountNoFilterId').val(),
                        withdrawalDevicePhoneFilter: $('#WithdrawalDevicePhoneFilterId').val(),
                        withdrawalDeviceBankTypeFilter: $('#WithdrawalDeviceBankTypeFilterId').val(),
                        orderCreationTimeStartDate: _selectedDateRang.startDate,
                        orderCreationTimeEndDate: _selectedDateRang.endDate,
                        utcTimeFilter: $('#UtcTimeFilterId').val(),
                        releaseStatus: $('#ReleaseStatusDDL').val(),
                        minMoneyFilter: $('#MinMoneyFilterId').val(),
                        maxMoneyFilter: $('#MaxMoneyFilterId').val()
                    };
                },
                done: getMerchants()
            },
            columnDefs: [
                {
                    className: 'control responsive',
                    orderable: false,
                    render: function () {
                        return '';
                    },
                    targets: 0
                },
                {
                    targets: 1,
                    data: "null",
                    name: "checkBox",
                    orderable: false,
                    render: function (data, type, full, meta) {
                        //Check if the status is "pending"
                        // WithdrawalOrderStatusEnum wait = 0 , WithdrawalNotifyStatusEnum wait = 0
                        const isPending = (full.withdrawalOrder.orderStatus === 0) && full.withdrawalOrder.notifyStatus == 0 || full.withdrawalOrder.orderStatus != 2;
                        //const isPending = true;
                        return `<input type="checkbox" class="checkbox" 
                       data-id="${full.withdrawalOrder.id}" 
                       data-orderno="${full.withdrawalOrder.orderNo}" 
                       data-name="${full.withdrawalOrder.merchantCode}"
                       data-text="${full.merchantName}" 
                       ${isPending ? '' : 'disabled'}/>`;
                    }
                },
                {
                    width: 120,
                    targets: 2,
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
                                    _viewWithdrawalOrderModal.open({ id: data.record.withdrawalOrder.id });
                                }
                            },
                            {
                                //手动转账
                                text: app.localize('EnforceCallBcakWithdrawal'),
                                iconStyle: 'far fa-edit mr-2',
                                visible: function (data) {
                                    if (data.record.withdrawalOrder.orderStatus != 2 && data.record.withdrawalOrder.notifyStatus == 0 && _permissions.callback && data.record.withdrawalOrder.isShowSuccessCallBack) {
                                        return true;
                                    } else {
                                        return false;
                                    }
                                },
                                action: function (data) {
                                    enforcecallbackOrder(data.record.withdrawalOrder);
                                }
                            },
                            {
                                //重新回调
                                text: app.localize('OrderCallBack'),
                                iconStyle: 'far fa-edit mr-2',
                                visible: function (data) {
                                    if (data.record.withdrawalOrder.orderStatus == 2 && data.record.withdrawalOrder.notifyStatus == 2 && _permissions.callback) {
                                        return true;
                                    } else {
                                        return false;
                                    }
                                },
                                action: function (data) {
                                    callbackOrder(data.record.withdrawalOrder);
                                }
                            },
                            {
                                //回调失败
                                text: app.localize('CallBackCancelOrder'),
                                iconStyle: 'far fa-edit mr-2',
                                visible: function (data) {
                                    if (data.record.withdrawalOrder.orderStatus >= 3 && data.record.withdrawalOrder.notifyStatus != 1 && _permissions.callback) {
                                        return true;
                                    } else {
                                        return false;
                                    }
                                },
                                action: function (data) {
                                    callBackCancelOrder(data.record.withdrawalOrder);
                                }
                            },
                            {
                                //取消订单
                                text: app.localize('CancelOrder'),
                                iconStyle: 'far fa-edit mr-2',
                                visible: function (data) {
                                    if (data.record.withdrawalOrder.orderStatus != 2 && data.record.withdrawalOrder.notifyStatus != 1 && _permissions.cancel) {
                                        return true;
                                    } else {
                                        return false;
                                    }
                                },
                                action: function (data) {
                                    cancelOrder(data.record.withdrawalOrder);
                                }
                            },
                            {
                                //手动转换代付中
                                text: app.localize('ChangeToPending'),
                                iconStyle: 'far fa-edit mr-2',
                                visible: function (data) {
                                    if (data.record.withdrawalOrder.orderStatus == 0 && data.record.withdrawalOrder.notifyStatus == 0 && _permissions.changetopending) {
                                        return true;
                                    } else {
                                        return false;
                                    }
                                },
                                action: function (data) {
                                    changeOrderToPending(data.record.withdrawalOrder);
                                }
                            },
                            {
                                //查看详情
                                text: app.localize('ViewPayoutDetails'),
                                iconStyle: 'far fa-id-card ml-2 mr-2',
                                visible: function (data) {
                                    if (_permissions.viewpayoutdetails) {
                                        return true;
                                    } else {
                                        return false;
                                    }
                                },
                                action: function (data) {
                                    _viewPayoutDetailsModal.open({ id: data.record.withdrawalOrder.id, utcFilter: $("#UtcTimeFilterId").val() });
                                }
                            },
                            {
                                //解冻余额
                                text: app.localize('ReleaseLockedAmount'),
                                iconStyle: 'far fa-id-card ml-2 mr-2',
                                visible: function (data) {
                                    if (_permissions.releasebalance
                                        && (data.record.withdrawalOrder.orderStatus == 2 || data.record.withdrawalOrder.orderStatus == 3)
                                        && data.record.withdrawalOrder.releaseStatus == 1) {
                                        return true;
                                    } else {
                                        return false;
                                    }
                                },
                                action: function (data) {
                                    releaseOrderLockBalance(data.record.withdrawalOrder);
                                }
                            }
                        ]
                    }
                },
                {
                    targets: 3,
                    data: "withdrawalOrder.merchantCode",
                    name: "merchantCode",
                    render: function (data, type, full, meta) {
                        var html = "<span>" + app.localize('MerchantName') + "\uff1a" + full.merchantName + "</span><br/>" +
                            "<span>" + app.localize('MerchantCode') + "\uff1a" + full.withdrawalOrder.merchantCode + "</span><br/>";
                        if (_permissions.view) {
                            if (full.withdrawalDevice != null) {
                                html += "<span>" + app.localize('WithdrawalDevice') + "\uff1a" + full.withdrawalDevice.name + "</span>";
                            }
                        }
                        return html;
                    }
                },
                {
                    targets: 4,
                    data: "withdrawalOrder.withdrawNo",
                    name: "withdrawNo",
                    render: function (data, type, full, meta) {
                        var transactionNo = full.withdrawalOrder.transactionNo;
                        if (transactionNo == null) {
                            transactionNo = "";
                        }
                        return "<span>" + app.localize('OrderNumber') + "\uff1a" + full.withdrawalOrder.orderNo + "</span><br/>" +
                            "<span>" + app.localize('TransactionNo') + "\uff1a" + transactionNo + "</span><br/>" +
                            "<span>" + app.localize('CreationTime') + "\uff1a" + moment(full.withdrawalOrder.creationTime).format('YYYY-MM-DD HH:mm:ss') + "</span><br/>" +
                            "<span>" + app.localize('TransactionTime') + "\uff1a" + moment(full.withdrawalOrder.transactionTime).format('YYYY-MM-DD HH:mm:ss') + "</span>";
                    }
                },
                {
                    targets: 5,
                    data: "withdrawalOrder.benAccountName",
                    name: "benAccountName",
                    render: function (data, type, full, meta) {
                        return "<span>" + app.localize('BenAccountNo') + "\uff1a" + full.withdrawalOrder.benAccountNo + "</span><i class='icon-xl far fa-copy btnCopy' data-clipboard-action='copy' data-clipboard-text='" + full.withdrawalOrder.benAccountNo + "'></i><br/>" +
                            "<span>" + app.localize('BenAccountName') + "\uff1a" + full.withdrawalOrder.benAccountName + "</span><i class='icon-xl far fa-copy btnCopy' data-clipboard-action='copy' data-clipboard-text='" + full.withdrawalOrder.benAccountName + "'></i><br/>" +
                            "<span>" + app.localize('BenBankName') + "\uff1a" + full.withdrawalOrder.benBankName + "</span><i class='icon-xl far fa-copy btnCopy' data-clipboard-action='copy' data-clipboard-text='" + full.withdrawalOrder.benBankName + "'></i>";
                    }
                },
                {
                    targets: 6,
                    data: "withdrawalOrder.orderMoney",
                    name: "orderMoney",
                    render: function (data, type, full, meta) {
                        return "<span>" + app.localize('OrderMoney') + "\uff1a" + full.withdrawalOrder.orderMoney + "</span><i class='icon-xl far fa-copy btnCopy' data-clipboard-action='copy' data-clipboard-text='" + full.withdrawalOrder.orderMoney.replace('₫', '').replace('.', '') + "'></i><br/>" +
                            "<span>" + app.localize('FeeMoney') + "\uff1a" + full.withdrawalOrder.feeMoney + "</span>";
                    }
                },
                {
                    width: 200,
                    targets: 7,
                    data: "withdrawalOrder.notifyStatus",
                    name: "notifyStatus",
                    render: function (data, type, full, meta) {
                        var status = {
                            0: {
                                'class': ' badge-light-primary'
                            },
                            1: {
                                'class': ' badge-light-info'
                            },
                            2: {
                                'class': ' badge-light-success'
                            },
                            3: {
                                'class': ' badge-light-danger'
                            },
                            4: {
                                'class': ' badge-light-danger'
                            },
                            5: {

                                'class': ' badge-light-danger'
                            },
                            6: {
                                'class': ' badge-light-danger'
                            },
                            7: {

                                'class': ' badge-light-danger'
                            },
                            8: {

                                'class': ' badge-light-danger'
                            },
                            9: {

                                'class': ' badge-warning'
                            },
                        };

                        var scoreStatus = {
                            0: {
                                'class': ' badge-light-primary'
                            },
                            1: {
                                'class': ' badge-light-success'
                            },
                            2: {

                                'class': ' badge-light-danger'
                            },
                        };

                        var releaseStatus = {
                            0: {
                                'class': ' badge-light-primary'
                            },
                            2: {
                                'class': ' badge-light-success'
                            },
                            1: {

                                'class': ' badge-light-warning'
                            },
                        };

                        var html = "";
                        if (_permissions.view) {
                            var maxLength = 30; // Define max length for truncation
                            var remark = full.withdrawalOrder.remark;
                            if (remark == "手机未回调") {
                                if (full.withdrawalOrder.orderStatus == 3) {
                                    remark = app.localize('WithdrawNullRemark');
                                } else {
                                    remark = app.localize('WithdrawWaitPhoneRemark');
                                }
                            }
                            if (full.withdrawalOrder.orderStatus >= 3 && full.withdrawalOrder.orderStatus != 9) {
                                if (remark == "") {
                                    remark = app.localize('WithdrawNullRemark');
                                }
                            }
                            if (full.withdrawalOrder.orderStatus == 8) {
                                remark = app.localize('WithdrawWaitPhoneRemark');
                            }


                            if (full.withdrawalOrder.orderStatus != 2) {

                                var finalRemark = remark ? remark : "";

                                if (remark && remark.length > maxLength) {
                                    var shortName = remark.substring(0, maxLength) + '...';

                                    finalRemark = "<span class='short-name'>" + shortName + "</span>" +
                                        "<span class='full-name' style='display:none;'>" + remark + "</span>" +
                                        " <a href='#' class='see-more'>" + app.localize('SeeMore') + "</a>";
                                }

                                html = "<br/>" + finalRemark
                            }

                            if (full.withdrawalOrder.isManualPayout) {
                                var finalRemark = remark ? remark : "";

                                if (remark && remark.length > maxLength) {
                                    var shortName = remark.substring(0, maxLength) + '...';

                                    finalRemark = "<span class='short-name'>" + shortName + "</span>" +
                                        "<span class='full-name' style='display:none;'>" + remark + "</span>" +
                                        " <a href='#' class='see-more'>" + app.localize('SeeMore') + "</a>";
                                }

                                html = "<br/>" + finalRemark
                            }
                        }


                        return "<span>" + app.localize('OrderStatus') + "\uff1a" + '<span class="badge label-lg font-weight-bold' + status[full.withdrawalOrder.orderStatus].class + ' label-inline">' + app.localize('Enum_WithdrawalOrderStatusEnum_' + full.withdrawalOrder.orderStatus) + '</span>' + "</span><br/>" +
                            "<span>" + app.localize('ScoreStatus') + "\uff1a" + '<span class="badge label-lg font-weight-bold' + scoreStatus[full.withdrawalOrder.notifyStatus].class + ' label-inline">' + app.localize('Enum_WithdrawalNotifyStatusEnum_' + full.withdrawalOrder.notifyStatus) + '</span>' + "</span><br/>" +
                            "<span>" + app.localize('ReleaseAmountStatus') + "\uff1a" + '<span class="badge label-lg font-weight-bold ' + releaseStatus[full.withdrawalOrder.releaseStatus].class + ' label-inline">' + app.localize("Enum_WithdrawalReleaseStatusEnum_" + full.withdrawalOrder.releaseStatus) + '</span>' + "</span><br/>" +
                            "<span>" + app.localize('TransactionProof') + "\uff1a" + (full.withdrawalOrder.haveProof ? "<span class='btnproof badge label-lg font-weight-bold badge-light-primary label-inline cursor-pointer'  data-toggle='tooltip' title='" + app.localize("ViewTransactionProof") + "'  data-id='" + full.withdrawalOrder.id + "'><i class='fa fa-file-invoice'></i></span>" : "") + "</span>"
                            + html;
                    }
                }
            ],
            drawCallback: function (settings) {
                $("#OrderTotal").text(settings.rawServerResponse.totalCount);
                $("#OrderMoneyTotal").text(settings.rawServerResponse.orderMoneyTotal);
                $("#FeeMoneyTotal").text(settings.rawServerResponse.feeMoneyTotal);
                $('.checkbox').off('change').on('change', handleCheckboxChange);
            }
        });

        _$withdrawalOrdersTable.on('click', '.btnproof', function () {
            var id = $(this).data('id');
            _viewProofModal.open({ id: id });
        });

        _$withdrawalOrdersTable.on('click', '.see-more', function (e) {
            e.preventDefault();
            var parent = $(this).closest('td'); // Get the parent <td> element (where both the short and full name are located)
            var shortName = parent.find('.short-name');
            var fullName = parent.find('.full-name');

            if (shortName.is(':visible')) {
                shortName.hide();
                fullName.show();
                $(this).text(app.localize('SeeLess'));
            } else {
                shortName.show();
                fullName.hide();
                $(this).text(app.localize('SeeMore'));
            }
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

        $('#CreateNewWithdrawalOrderButton').click(function () {
            _createOrEditModal.open();
        });

        $('#ExportToExcelButton').click(function () {
            _withdrawalOrdersService
                .getWithdrawalOrdersToExcel({
                    filter: $('#WithdrawalOrdersTableFilter').val(),
                    merchantCodeFilter: $('#MerchantCodeFilterId').val(),
                    orderNoFilter: $('#OrderNoFilterId').val(),
                    orderStatusFilter: $('#OrderStatusFilterId').val(),
                    notifyStatusFilter: $('#NotifyStatusFilterId').val(),
                    benAccountNameFilter: $('#BenAccountNameFilterId').val(),
                    benAccountNoFilter: $('#BenAccountNoFilterId').val(),
                    withdrawalDevicePhoneFilter: $('#WithdrawalDevicePhoneFilterId').val(),
                    withdrawalDeviceBankTypeFilter: $('#WithdrawalDeviceBankTypeFilterId').val(),
                    orderCreationTimeStartDate: _selectedDateRang.startDate,
                    orderCreationTimeEndDate: _selectedDateRang.endDate,
                    utcTimeFilter: $('#UtcTimeFilterId').val(),
                    minMoneyFilter: $('#MinMoneyFilterId').val(),
                    maxMoneyFilter: $('#MaxMoneyFilterId').val(),
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

        abp.event.on('app.createOrEditWithdrawalOrderModalSaved', function () {
            getWithdrawalOrders();
        });
        abp.event.on('app.editWithdrawalOrderDeviceModalSaved', function () {
            getWithdrawalOrders();
        });

        $('#GetWithdrawalOrdersButton').click(function (e) {
            e.preventDefault();
            getWithdrawalOrders();
        });

        $('.reload-on-change').change(function (e) {
            if (_suppressEvents) return;
            getWithdrawalOrders();
        });

        $('.reload-on-keyup').keyup(function (e) {
            getWithdrawalOrders();
        });

        $('#btn-reset-filters').click(function (e) {

            _suppressEvents = true;

            $('.reload-on-change,.reload-on-keyup,#MyEntsTableFilter').val('');

            _selectedDateRang.startDate = moment().startOf('day').add(-1, 'day');
            _selectedDateRang.endDate = moment().startOf('day').add(1, 'day');

            $datePicker.data('daterangepicker').setStartDate(moment().startOf('day').add(-1, 'day'));
            $datePicker.data('daterangepicker').setEndDate(moment().startOf('day').add(1, 'day'));


            $('select.reload-on-change').each(function (index, element) {

                var firstSelectionValue = $(element).find("option:first").val();

                if ($(element).hasClass("select2")) {
                    $(element).val('-1').trigger('change');
                }
                else {
                    $(element).val(firstSelectionValue);// drop  downlist select first option
                }

            });

            _suppressEvents = false;
            getWithdrawalOrders();

        });

        $('#withdrawalOrderChangeDevice').click(function () {
            const selectedItems = GetSelectedWithdrawlOrder();

            if (selectedItems.length === 0) {
                abp.message.error(app.localize('ErrAtleastOneWithdrawsOrder'));
                return;
            }

            // Get unique merchant codes
            const uniqueMerchantCodes = [...new Set(selectedItems.map(item => item.merchantCode))];

            // Check if there are multiple different merchant codes
            if (uniqueMerchantCodes.length > 1) {
                abp.message.warn(app.localize('DifferentMerchantCodesWarning'));
                return;
            }

            // Open modal if validation passes
            _editWithdrawalOrderDeviceModal.open({
                input: selectedItems
            });
        });

        $('#withdrawalOrderCancelDevice').click(function () {
            const selectedItems = GetSelectedWithdrawlOrder();

            if (selectedItems.length > 0) {
                _cancelWithdrawalOrderDeviceModal.open({
                    input: selectedItems
                });
            } else {
                abp.message.error(app.localize('ErrAtleastOneWithdrawsOrder'));
            }
        });

        $('#batchCallBackWithdrawal').click(function () {
            const selectedItems = GetSelectedWithdrawlOrder();

            if (selectedItems.length > 0) {
                _batchCallBackWithdrawalModal.open({
                    input: selectedItems
                });
            } else {
                abp.message.error(app.localize('ErrAtleastOneWithdrawsOrder'));
            }
        });

        function getMerchants() {
            _withdrawalOrdersService.getOrderMerchants().done(function (result) {
                $("#MerchantCodeFilterId").empty();
                $("#MerchantCodeFilterId").append("<option selected value=''>" + app.localize('All') + "</option>");
                for (var i = 0; i < result.length; i++) {
                    $("#MerchantCodeFilterId").append("<option value='" + result[i].merchantCode + "'>" + result[i].merchantName + "[" + result[i].merchantCode + "]" + "</option>");
                }
            });
        }

        function getWithdrawalOrders() {
            var startTime = performance.now(); // Start timer before reload

            dataTable.ajax.reload(function () {
                var endTime = performance.now(); // End timer after reload

                // Calculate the time taken for ajax.reload()
                var timeTaken = (endTime - startTime).toFixed(2);
                console.log("ajax.reload() took " + timeTaken + " milliseconds.");
            });
        }

        function enforcecallbackOrder(withdrawalOrder) {
            _withdrawalOrdersService.getOrderMerchantBalance(withdrawalOrder.merchantCode).done(function (result) {
                var merchantBalance = result;
                var alertText = "";

                if (merchantBalance < withdrawalOrder.orderMoneyDec) {
                    alertText = ("<p class='text-danger'>" + app.localize("confirmEnforceSuccess") + "</p>");
                }

                abp.message.confirm(
                    '',
                    alertText.length > 0 ? alertText : app.localize('AreYouSure'),
                    function (isConfirmed) {
                        if (isConfirmed) {
                            _withdrawalOrdersService.enforceCallBcak({
                                id: withdrawalOrder.id
                            }).done(function (result) {
                                getWithdrawalOrders(true);
                                if (result) {
                                    abp.notify.success(app.localize('SuccessCallBack'));
                                } else {
                                    abp.notify.error(app.localize('CallBackCancelOrder'));
                                }
                            });
                        }
                    }
                );
            });
        }

        function callBackCancelOrder(withdrawalOrder) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _withdrawalOrdersService.callBackCancelOrder({
                            id: withdrawalOrder.id
                        }).done(function () {
                            getWithdrawalOrders(true);
                            abp.notify.success(app.localize('SuccessCallBack'));
                        });
                    }
                }
            );
        }

        function changeOrderToPending(withdrawalOrder) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _withdrawalOrdersService.changeOrderStatusToPending({
                            id: withdrawalOrder.id
                        }).done(function () {
                            getWithdrawalOrders(true);
                            abp.notify.success(app.localize('SuccessCallBack'));
                        });
                    }
                }
            );
        }

        function releaseOrderLockBalance(withdrawalOrder) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _withdrawalOrdersService.releaseLockAmount({
                            id: withdrawalOrder.id
                        }).done(function () {
                            getWithdrawalOrders(true);
                            abp.notify.success(app.localize('SuccessCallBack'));
                        });
                    }
                }
            );
        }

        function callbackOrder(withdrawalOrder) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _withdrawalOrdersService.callBcak({
                            id: withdrawalOrder.id
                        }).done(function () {
                            getWithdrawalOrders(true);
                            abp.notify.success(app.localize('SuccessCallBack'));
                        });
                    }
                }
            );
        }

        function cancelOrder(withdrawalOrder) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _withdrawalOrdersService.cancelOrder({
                            id: withdrawalOrder.id
                        }).done(function () {
                            getWithdrawalOrders(true);
                            abp.notify.success(app.localize('SuccessCancelOrder'));
                        });
                    }
                }
            );
        }

        function handleCheckboxChange() {
            const selectedItems = GetSelectedWithdrawlOrder();

            if (selectedItems.length === 0) {
                $('#withdrawalOrderChangeDevice').prop('disabled', true);
                $('#withdrawalOrderCancelDevice').prop('disabled', true);
                $('#batchCallBackWithdrawal').prop('disabled', true);
            } else {
                $('#withdrawalOrderChangeDevice').prop('disabled', false); // Enable button if at least one item is selected
                $('#withdrawalOrderCancelDevice').prop('disabled', false);
                $('#batchCallBackWithdrawal').prop('disabled', false);
            }
        }

        function GetSelectedWithdrawlOrder() {
            const selectedItems = [];

            $('#WithdrawalOrdersTable .checkbox:checked').each(function () {
                selectedItems.push({
                    withdrawId: $(this).data('id'),
                    orderNo: $(this).data('orderno'),
                    merchantCode: $(this).data('name'),
                    merchantName: $(this).data('text')
                });
            });

            return selectedItems;
        }
    });
})();