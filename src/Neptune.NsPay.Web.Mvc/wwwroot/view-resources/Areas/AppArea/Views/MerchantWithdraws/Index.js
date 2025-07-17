(function () {
    $(function () {

        var _$merchantWithdrawsTable = $('#MerchantWithdrawsTable');
        var _merchantWithdrawsService = abp.services.app.merchantWithdraws;
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

        var $selectedDate = {
            startDate: null,
            endDate: null,
        }

        var _selectedDateRang = {
            startDate: moment().startOf('day').add(-1, 'day'),
            endDate: moment().startOf('day').add(2, 'day'),

        };

       var _mindatetime= $("#MinReviewTimeFilterId").daterangepicker({
            timePicker: true,
            timePicker24Hour: true,
            //timePickerSeconds: true,
            singleDatePicker: true,
            autoApply: true,
            startDate: moment().startOf('day').add(-1, 'day'),
            locale: {
                format: "YYYY-MM-DD HH:mm:ss"
            }
        }, function (start, end, label) {
            _selectedDateRang.startDate = start.format('YYYY-MM-DD HH:mm:ss');
        });

        var _maxdatetime = $("#MaxReviewTimeFilterId").daterangepicker({
            timePicker: true,
            timePicker24Hour: true,
            //timePickerSeconds: true,
            singleDatePicker: true,
            autoApply: true,
            startDate: moment().startOf('day').add(2, 'day'),
            locale: {
                format: "YYYY-MM-DD HH:mm:ss"
            }
        }, function (start, end, label) {
            _selectedDateRang.endDate = start.format('YYYY-MM-DD HH:mm:ss');
        });

        $('.date-picker').on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('YYYY-MM-DD HH:mm:ss'));
        });

        var _permissions = {
            create: abp.auth.hasPermission('Pages.MerchantWithdraws.Create'),
            edit: abp.auth.hasPermission('Pages.MerchantWithdraws.Edit'),
            auditpass: abp.auth.hasPermission('Pages.MerchantWithdraws.AuditPass'),
            auditturndown: abp.auth.hasPermission('Pages.MerchantWithdraws.AuditTurndown'),
        };

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/MerchantWithdraws/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/MerchantWithdraws/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditMerchantWithdrawModal'
        });

        var _turndownOrPassModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/MerchantWithdraws/TurndownOrPassModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/MerchantWithdraws/_TurndownOrPassModal.js',
            modalClass: 'TurndownOrPassMerchantWithdrawModal'
        });


        var _viewMerchantWithdrawModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/MerchantWithdraws/ViewmerchantWithdrawModal',
            modalClass: 'ViewMerchantWithdrawModal'
        });




        var getDateFilter = function (element) {
            if ($selectedDate.startDate == null) {
                return null;
            }
            return $selectedDate.startDate.format("YYYY-MM-DDT00:00:00Z");
        }

        var getMaxDateFilter = function (element) {
            if ($selectedDate.endDate == null) {
                return null;
            }
            return $selectedDate.endDate.format("YYYY-MM-DDT23:59:59Z");
        }

        var dataTable = _$merchantWithdrawsTable.DataTable({
            paging: true,
            serverSide: true,
            processing: true,
            listAction: {
                ajaxFunction: _merchantWithdrawsService.getAll,
                inputFilter: function () {
                    return {
                        filter: $('#MerchantWithdrawsTableFilter').val(),
                        merchantCodeFilter: $('#MerchantCodeFilterId').val(),
                        withDrawNoFilter: $('#WithDrawNoFilterId').val(),
                        bankNameFilter: $('#BankNameFilterId').val(),
                        receivCardFilter: $('#ReceivCardFilterId').val(),
                        receivNameFilter: $('#ReceivNameFilterId').val(),
                        statusFilter: $('#StatusFilterId').val(),
                        //minReviewTimeFilter: getDateFilter($('#MinReviewTimeFilterId')),
                        //maxReviewTimeFilter: getMaxDateFilter($('#MaxReviewTimeFilterId'))
                        minReviewTimeFilter: _selectedDateRang.startDate,
                        maxReviewTimeFilter: _selectedDateRang.endDate
                    };
                }
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
                    width: 120,
                    targets: 1,
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
                                action: function (data) {
                                    _viewMerchantWithdrawModal.open({ id: data.record.merchantWithdraw.id });
                                }
                            },
                            {
                                text: app.localize('AuditPass'),
                                iconStyle: 'far fa-edit mr-2',
                                visible: function (data) {
                                    if (data.record.merchantWithdraw.status == 1 && _permissions.auditpass) {
                                        return true;
                                    } else {
                                        return false;
                                    }
                                },
                                action: function (data) {
                                    passMerchantWithdraw(data.record.merchantWithdraw);
                                }
                            },
                            {
                                text: app.localize('AuditTurndown'),
                                iconStyle: 'far fa-edit mr-2',
                                visible: function (data) {
                                    if (data.record.merchantWithdraw.status == 1 && _permissions.auditturndown) {
                                        return true;
                                    } else {
                                        return false;
                                    }
                                },
                                action: function (data) {
                                    _turndownOrPassModal.open({
                                        id: data.record.merchantWithdraw.id
                                    });
                                }
                            }
                        ]
                    }
                },
                {
                    targets: 2,
                    data: "merchantWithdraw.merchantCode",
                    name: "merchantCode",
                    render: function (data, type, full, meta) {
                        return "<span>" + app.localize('MerchantName') + "\uff1a" + full.merchantName + "</span><br/>" +
                            "<span>" + app.localize('MerchantCode') + "\uff1a" + full.merchantWithdraw.merchantCode + "</span><br/>";
                    }
                },
                {
                    targets: 3,
                    data: "merchantWithdraw.withDrawNo",
                    name: "withDrawNo",
                    render: function (data, type, full, meta) {
                        return "<span>" + app.localize('WithDrawNo') + "\uff1a" + full.merchantWithdraw.withDrawNo + "</span><br/>" +
                            "<span>" + app.localize('CreationTime') + "\uff1a" + moment(full.merchantWithdraw.creationTime).format('YYYY-MM-DD HH:mm:ss') + "</span>";

                    }
                },
                {
                    targets: 4,
                    data: "merchantWithdraw.money",
                    name: "money",
                    render: function (data, type, full, meta) {
                        var status = {
                            1: {
                                'title': app.localize('StatusPending'),
                                'class': ' badge-light-primary'
                            },
                            2: {
                                'title': app.localize('StatusSuccess'),
                                'class': ' badge-light-success'
                            },
                            3: {
                                'title': app.localize('StatusDelivered'),
                                'class': ' badge-light-danger'
                            }
                        };
                        return "<span class='text-gray-900 fw-bold d-block fs-7'>" + app.localize('Money') + "\uff1a" + full.merchantWithdraw.money + "</span><i class='icon-xl far fa-copy btnCopy' data-clipboard-action='copy' data-clipboard-text='" + full.merchantWithdraw.money.replace('₫', '').replace('.', '').replace('.', '') + "'></i><br/>" +
                            "<span>" + app.localize('Status') + "\uff1a" + '<span class="badge label-lg font-weight-bold' + status[full.merchantWithdraw.status].class + ' label-inline">' + app.localize('Enum_MerchantWithdrawStatusEnum_' + full.merchantWithdraw.status) + '</span>' + "</span>";

                    }
                },
                {
                    targets: 5,
                    data: "merchantWithdraw.bankName",
                    name: "bankName",
                    render: function (data, type, full, meta) {
                        return "<span>" + app.localize('WithdrawBankName') + ":" + full.merchantWithdraw.bankName + "</span><i class='icon-xl far fa-copy btnCopy' data-clipboard-action='copy' data-clipboard-text='" + full.merchantWithdraw.bankName + "'></i><br/>" +
                            "<span>" + app.localize('WithdrawReceivCard') + ":" + full.merchantWithdraw.receivCard + "</span><i class='icon-xl far fa-copy btnCopy' data-clipboard-action='copy' data-clipboard-text='" + full.merchantWithdraw.receivCard + "'></i><br/>" +
                            "<span>" + app.localize('WithdrawReceivName') + ":" + full.merchantWithdraw.receivName + "</span><i class='icon-xl far fa-copy btnCopy' data-clipboard-action='copy' data-clipboard-text='" + full.merchantWithdraw.receivName + "'></i>";
                    }
                },
            ]
        });

        function getMerchantWithdraws() {
            dataTable.ajax.reload();
        }

        function passMerchantWithdraw(merchantWithdraw) {
            abp.message.confirm(
                '',
                app.localize('AreYouSurePass'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _merchantWithdrawsService.auditPass({
                            id: merchantWithdraw.id
                        }).done(function () {
                            getMerchantWithdraws(true);
                            abp.notify.success(app.localize('SavedSuccessfully'));
                        });
                    }
                }
            );
        }

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

        $('#CreateNewMerchantWithdrawButton').click(function () {
            _createOrEditModal.open();
        });

        $('#ExportToExcelButton').click(function () {
            _merchantWithdrawsService
                .getMerchantWithdrawsToExcel({
                    filter: $('#MerchantWithdrawsTableFilter').val(),
                    merchantCodeFilter: $('#MerchantCodeFilterId').val(),
                    withDrawNoFilter: $('#WithDrawNoFilterId').val(),
                    bankNameFilter: $('#BankNameFilterId').val(),
                    receivCardFilter: $('#ReceivCardFilterId').val(),
                    receivNameFilter: $('#ReceivNameFilterId').val(),
                    statusFilter: $('#StatusFilterId').val(),
                    //minReviewTimeFilter: getDateFilter($('#MinReviewTimeFilterId')),
                    //maxReviewTimeFilter: getMaxDateFilter($('#MaxReviewTimeFilterId'))
                    minReviewTimeFilter: _selectedDateRang.startDate,
                    maxReviewTimeFilter: _selectedDateRang.endDate
                })
                .done(function (result) {
                    app.downloadTempFile(result);
                });
        });

        abp.event.on('app.createOrEditMerchantWithdrawModalSaved', function () {
            getMerchantWithdraws();
        });

        $('#GetMerchantWithdrawsButton').click(function (e) {
            e.preventDefault();
            getMerchantWithdraws();
        });

        $(document).keypress(function (e) {
            if (e.which === 13) {
                getMerchantWithdraws();
            }
        });

        $('.reload-on-change').change(function (e) {
            getMerchantWithdraws();
        });

        $('.reload-on-keyup').keyup(function (e) {
            getMerchantWithdraws();
        });

        $('#btn-reset-filters').click(function (e) {

            $('.reload-on-change,.reload-on-keyup,#MyEntsTableFilter').val('');

            _selectedDateRang.startDate = moment().startOf('day').add(-1, 'day');
            _selectedDateRang.endDate = moment().startOf('day').add(2, 'day');

            _mindatetime.data('daterangepicker').setStartDate(moment().startOf('day').add(-1, 'day'));
            _maxdatetime.data('daterangepicker').setStartDate(moment().startOf('day').add(2, 'day'));
            $('select.reload-on-change').each(function (index, element) {

                var firstSelectionValue = $(element).find("option:first").val();

                if ($(element).hasClass("select2")) {
                    $(element).val('-1').trigger('change');
                }
                else {
                    $(element).val(firstSelectionValue);// drop  downlist select first option
                }

            });
            getMerchantWithdraws();
        });




    });
})();
