(function () {
    $(function () {

        var _$payOrderDepositsTable = $('#PayOrderDepositsTable');
        var _payOrderDepositsService = abp.services.app.payOrderDeposits;
        let _suppressEvents = false;

        $('.select2').select2({
            placeholder: app.localize('Select'),
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

        var _selectedDateRang = {
            startDate: moment().startOf('day').add(-1, 'day'),
            endDate: moment().startOf('day').add(2, 'day'),

        };


      var Mindatatime = $("#MinTransactionTimeFilter").daterangepicker({
            timePicker: true,
            timePicker24Hour: true,
            //timePickerSeconds: true,
            singleDatePicker: true,
            autoApply: true,
            startDate: moment().startOf('day').add(-1, 'day'),
            locale: {
                format: "L LT"
            }
        }, function (start, end, label) {
            _selectedDateRang.startDate = start.format('YYYY-MM-DD HH:mm:ss');
        });

        var Maxdatatime = $("#MaxTransactionTimeFilter").daterangepicker({
            timePicker: true,
            timePicker24Hour: true,
            //timePickerSeconds: true,
            singleDatePicker: true,
            autoApply: true,
            startDate: moment().startOf('day').add(2, 'day'),
            locale: {
                format: "L LT"
            }
        }, function (start, end, label) {
            _selectedDateRang.endDate = start.format('YYYY-MM-DD HH:mm:ss');
        });

        $('.date-picker').on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY'));
        });

        var _permissions = {
            edit: abp.auth.hasPermission('Pages.PayOrderDeposits.Edit')
        };


        var _associatedOrderModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/PayOrderDeposits/AssociatedOrderModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/PayOrderDeposits/_AssociatedOrderModal.js',
            modalClass: 'AssociatedOrderModal'
        });

        var _rejectOrderModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/PayOrderDeposits/RejectOrderModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/PayOrderDeposits/_RejectOrderModal.js',
            modalClass: 'RejectOrderModal'
        });
		

        var getDateFilter = function (element) {
            if ($selectedDate.startDate == null) {
                return null;
            }
            return $selectedDate.startDate.format("YYYY-MM-DDT00:00:00Z"); 
        }


        var dataTable = _$payOrderDepositsTable.DataTable({
            paging: true,
            serverSide: true,
            processing: true,
            deferRender: true,
            searching: false,
            ordering: false, 
            scrollX: true,
            scrollCollapse: true,
            select: {
                style: 'multi',
                selector: 'td:first-child input[type="checkbox"]',
                className: 'row-selected'
            },
            listAction: {
                ajaxFunction: _payOrderDepositsService.getAll,
                inputFilter: function () {
                    return {
                        filter: $('#PayOrderDepositsTableFilter').val(),
                        merchantCodeFilter: $('#MerchantCodeFilterId').val(),
                        refNoFilter: $('#RefNoFilterId').val(),
                        orderNoFilter: $('#OrderNoFilterId').val(),
                        orderMarkFilter: $('#OrderMarkFilterId').val(),
                        userMemberFilter: $('#UserMemberFilterId').val(),
                        accountNoFilter: $('#AccountNoFilterId').val(),
                        userNameFilter: $('#UserNameFilterId').val(),
                        orderPayTypeFilter: $('#OrderPayTypeFilterId').val() == null ? '-1' : $('#OrderPayTypeFilterId').val(),
                        depositOrderStatusFilter: $('#DepositOrderStatusFilterId').val() == null ? '-1' : $('#DepositOrderStatusFilterId').val(),
                        bankOrderStatusFilter: $('#BankOrderStatusFilterId').val(),
                        minTransactionTimeFilter: _selectedDateRang.startDate,
                        maxTransactionTimeFilter: _selectedDateRang.endDate,
                        minMoneyFilter: $('#MinMoneyFilterId').val(),
                        maxMoneyFilter: $('#MaxMoneyFilterId').val(),
                        utcTimeFilter: $('#UtcTimeFilterId').val(),
                    };
                },
                done: getMerchants()
            },
            columnDefs: [
                {
                    targets: 0,
                    //className: 'control responsive',
                    orderable: false,
                    render: function (data, type, full, meta) {
                        //return '';
                        if (full.payOrderDeposit.orderId == "" && full.type == 1) {
                            return `
                            <div class="form-check form-check-sm form-check-custom form-check-solid">
                                <input class="form-check-input" type="checkbox" value="${full.payOrderDeposit.id}" />
                            </div>`;
                        } else {
                            return '';
                        }
                    }
                },
                {
                    width: 100,
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
                                iconStyle: 'far fa-eye mr-2',
                                action: function (data) {
                                    _viewPayOrderDepositModal.open({ id: data.record.payOrderDeposit.id });
                                }
                            },
                            {
                                text: app.localize('AssociatedDepositOrder'),
                                iconStyle: 'far fa-edit mr-2',
                                visible: function (data) {
                                    if (_permissions.edit && data.record.payOrderDeposit.orderId == "" && data.record.type == 1) {
                                        return true;
                                    } else {
                                        return false;
                                    }
                                },
                                action: function (data) {
                                    _associatedOrderModal.open({ id: data.record.payOrderDeposit.id, payType: data.record.payOrderDeposit.payType });
                                }
                            },
                            {
                                text: app.localize('RejectDepositOrder'),
                                iconStyle: 'far fa-edit mr-2',
                                visible: function (data) {
                                    if (_permissions.edit && data.record.payOrderDeposit.orderId == "" && data.record.type == 1) {
                                        return true;
                                    } else {
                                        return false;
                                    }
                                },
                                action: function (data) {
                                    _rejectOrderModal.open({ id: data.record.payOrderDeposit.id, payType: data.record.payOrderDeposit.payType });
                                }
                            }]
                    }
                },
                {
                    targets: 2,
                    width: 200,
                    orderable: false,
                    data: "payOrderDeposit.accountNo",
                    name: "accountNo",
                    render: function (data, type, full, meta) {
                        return "<span>" + app.localize('BankAccountNo') + "\uff1a" + full.payOrderDeposit.accountNo + "</span><br/>" +
                            "<span>" + app.localize('BankUserName') + "\uff1a" + full.payOrderDeposit.userName + "</span><br/>" +
                            "<span>" + app.localize('BankPayType') + "\uff1a" + app.localize('Enum_PayMentTypeEnum_' + full.payOrderDeposit.payType) + "</span><br/>" +
                            "<span>" + app.localize('BankPayMentName') + "\uff1a" + full.payOrderDeposit.payMentName + "</span><br/>";
                    }
                },
                {
                    targets: 3,
                    orderable: false,
                    data: "payOrderDeposit.creditAmount",
                    name: "creditAmount",
                    render: function (data, type, full, meta) {
                        var html = "<span class='badge label-lg font-weight-bold badge-light-primary label-inline'>" + app.localize('BankOrderStatus') + "\uff1a" + app.localize('BankOrderCredit') + "</span><br/>" +
                            "<span>" + app.localize('CreditAmount') + "\uff1a" + full.payOrderDeposit.creditAmount + "</span><br/>";
                        if (full.payOrderDeposit.type == "DBIT") {
                            html = "<span class='badge label-lg font-weight-bold badge-light-danger label-inline'>" + app.localize('BankOrderStatus') + "\uff1a" + app.localize('BankOrderDebit') + "</span><br/>" +
                                "<span class='badge label-lg font-weight-bold badge-light-danger label-inline'>" + app.localize('DebitAmount') + "\uff1a" + full.payOrderDeposit.debitAmount + "</span><br/>" +
                                "<span class='badge label-lg font-weight-bold badge-light-danger label-inline'>" + app.localize('AvailableBalance') + "\uff1a" + full.payOrderDeposit.availableBalance + "</span><br/>" +
                                "<span class='badge label-lg font-weight-bold badge-light-danger label-inline'>" + app.localize('RefNo') + "\uff1a" + full.payOrderDeposit.refNo + "</span><br/>";
                            return html;
                        }
                        return html +
                            "<span>" + app.localize('AvailableBalance') + "\uff1a" + full.payOrderDeposit.availableBalance + "</span><br/>" +
                            "<span>" + app.localize('RefNo') + "\uff1a" + full.payOrderDeposit.refNo + "</span><br/>";
                    }
                },
                {
                    targets: 4,
                    width: 300,
                    orderable: false,
                    data: "payOrderDeposit.benAccountName",
                    name: "benAccountName",
                    render: function (data, type, full, meta) {
                        var html = "<span class='badge-inline'>" + app.localize('DebitAcctName') + "\uff1a<br/>" + full.payOrderDeposit.debitAcctName + "</span><br/>" +
                            "<span class='badge-inline'>" + app.localize('DebitAcctNo') + "\uff1a <br/>" + full.payOrderDeposit.debitAcctNo + "</span><br/>" +
                            "<span class='badge-inline'>" + app.localize('DebitBank') + "\uff1a <br/>" + full.payOrderDeposit.debitBank + "</span><br/>";
                        if (full.payOrderDeposit.type == "DBIT") {
                            html = "<span class='badge-inline'>" + app.localize('CreditAcctName') + "\uff1a <br/>" + full.payOrderDeposit.creditAcctName + "</span><br/>" +
                                "<span class='badge-inline'>" + app.localize('CreditAcctNo') + "\uff1a <br/>" + full.payOrderDeposit.creditAcctNo + "</span><br/>" +
                                "<span class='badge-inline'>" + app.localize('CreditBank') + "\uff1a <br/>" + full.payOrderDeposit.creditBank + "</span><br/>";
                        }
                        return html;
                    }
                },
                {
                    targets: 5,
                    orderable: false,
                    data: "payOrderDeposit.orderId",
                    name: "orderId",
                    width: 200,
                    render: function (data, type, full, meta) {
                        var userHtml = "";
                        if (full.payOrderDeposit.operateUser != null) {
                            userHtml = "<br/><span>" + app.localize('OperateUser') + "\uff1a" + '<span class="badge label-lg font-weight-bold badge-light-info label-inline">' + full.payOrderDeposit.operateUser + '</span>' + "</span>";
                        }
                        if (full.payOrder) {
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
                            //var userNo = '<br/><span>' + app.localize('UserMember') + "\uff1a" + full.payOrder.userNo + '</span>';
                            return "<span>" + app.localize('OrderTransactionTime') + "\uff1a <br/>" + moment(full.payOrder.creationTime).format('YYYY-MM-DD HH:mm:ss') + "</span><br/>" +
                                "<span>" + app.localize('OrderNumber') + "\uff1a" + full.payOrder.orderNumber + "</span><br/>" +
                                "<span>" + app.localize('OrderStatus') + "\uff1a" + '<span class="badge label-lg font-weight-bold' + status[full.payOrder.orderStatus].class + ' label-inline">' + app.localize('Enum_PayOrderOrderStatusEnum_' + full.payOrder.orderStatus) + '</span>' + "</span><br/>" +
                                "<span>" + app.localize('ScoreStatus') + "\uff1a" + '<span class="badge label-lg font-weight-bold' + scoreStatus[full.payOrder.scoreStatus].class + ' label-inline">' + app.localize('Enum_PayOrderScoreStatusEnum_' + full.payOrder.scoreStatus) + '</span>' + "</span>" + userHtml;
                        } else {
                            if (full.payOrderDeposit.orderId == "-1") {
                                return "<span>" + app.localize('OrderStatus') + "\uff1a" + '<span class="badge label-lg font-weight-bold badge-light-danger label-inline">' + app.localize('RejectOrder') + '</span>' + "</span><br/>" +
                                    "<span>" + app.localize('RejectDepositMsg') + "\uff1a" + full.payOrderDeposit.rejectRemark + "</span>" + userHtml +
                                    "<br/><span>" + app.localize('OperateTime') + "\uff1a" + full.payOrderDeposit.operateTime + "</span>";
                            } else {
                                return "";
                            }
                        }
                    }
                },
                {
                    targets: 6,
                    orderable: false,
                    width: 300,
                    data: "payOrderDeposit.description",
                    name: "description",
                    render: function (data, type, full, meta) {
                        // Define the maximum length for truncation
                        var maxLength = 50;
                        var description = full.payOrderDeposit.description || "";

                        // Format the create time and transaction time
                        var createTime = moment(full.payOrderDeposit.creationTime).format('YYYY-MM-DD HH:mm:ss');
                        var transactionTime = moment(full.payOrderDeposit.transactionTime).format('YYYY-MM-DD HH:mm:ss');

                        var besideDescContent = "";

                        if (full.payOrderDeposit.payType == 3 || full.payOrderDeposit.payType == 4) {
                            besideDescContent = "<span>" + app.localize('BankCreateTime') + "\uff1a" + createTime + "</span><br/>";
                        } else {
                            besideDescContent = "<span>" + app.localize('BankCreateTime') + "\uff1a" + createTime + "</span><br/>" +
                                "<span>" + app.localize('BankTransactionTime') + "\uff1a" + transactionTime + "</span><br/>";
                        }
                            

                        // Truncate the description if it exceeds the max length
                        if (description.length > maxLength) {
                            var shortDescription = description.substring(0, maxLength) + '...';
                            return besideDescContent +
                                "<span>" + app.localize('BankDescription') + "\uff1a" + shortDescription +
                                "</span><span class='full-description' style='display:none;'>" + description + "</span>" +
                                "<a href='#' class='see-more'>" + app.localize('SeeMore') +"</a><br/>";
                        } else {
                            return besideDescContent +
                                "<span>" + app.localize('BankDescription') + "\uff1a" + description + "</span><br/>";
                        }
                    }
                }

            ]
        });

        $('#select-all').on('click', function () {
            if (this.checked) {
                dataTable.rows().select();
            } else {
                dataTable.rows().deselect();
            }
        });

        //批量拒绝
        $("#BulkRejection").click(function () {
            var ids = new Array();
            const container = document.querySelector('#PayOrderDepositsTable');
            const allCheckboxes = container.querySelectorAll('tbody [type="checkbox"]');

            allCheckboxes.forEach(c => {
                if (c.checked) {
                    ids.push($(c).val());
                }
            });
            if (ids.length == 0) {
                abp.message.error(app.localize('NoData'));
                return;
            }
            var data = JSON.stringify(ids);
            var $button = $(this);
            // 禁用按钮
            $button.prop('disabled', true);
            _payOrderDepositsService.bulkRejectOrder({
                orderIds: data
            }).done(function () {
                $button.prop('disabled', false);
                abp.notify.info(app.localize('SavedSuccessfully'));
            });
        })


        //重新匹配
        $("#ResetMatch").click(function () {
            var ids = new Array();
            const container = document.querySelector('#PayOrderDepositsTable');
            const allCheckboxes = container.querySelectorAll('tbody [type="checkbox"]');

            allCheckboxes.forEach(c => {
                if (c.checked) {
                    ids.push($(c).val());
                }
            });
            if (ids.length == 0) {
                abp.message.error(app.localize('NoData'));
                return;
            }
            var data = JSON.stringify(ids);
            var $button = $(this);
            // 禁用按钮
            $button.prop('disabled', true);
            _payOrderDepositsService.bulkMatchOrder({
                orderIds: data
            }).done(function () {
                $button.prop('disabled', false);
                abp.notify.info(app.localize('SavedSuccessfully'));
            });
        })


        function getPayOrderDeposits() {
            var startTime = performance.now(); // Start timer before reload

            dataTable.ajax.reload(function () {
                var endTime = performance.now(); // End timer after reload

                // Calculate the time taken for ajax.reload()
                var timeTaken = (endTime - startTime).toFixed(2);
                console.log("ajax.reload() took " + timeTaken + " milliseconds.");
            });
        }

        function getMerchants() {
            _payOrderDepositsService.getOrderMerchants({
            }).done(function (result) {
                $("#MerchantCodeFilterId").empty();
                $("#MerchantCodeFilterId").append("<option selected value=''>" + app.localize('All') + "</option>");
                for (var i = 0; i < result.length; i++) {
                    $("#MerchantCodeFilterId").append("<option value='" + result[i].merchantCode + "'>" + result[i].merchantName + "[" + result[i].merchantCode + "]" + "</option>");
                }
            });
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
        $('#ExportToExcelButton').click(function () {
            _payOrderDepositsService
                .getPayOrderDepositsToExcel({
                    filter: $('#PayOrderDepositsTableFilter').val(),
                    merchantCodeFilter: $('#MerchantCodeFilterId').val(),
                    refNoFilter: $('#RefNoFilterId').val(),
                    orderNoFilter: $('#OrderNoFilterId').val(),
                    orderMarkFilter: $('#OrderMarkFilterId').val(),
                    userMemberFilter: $('#UserMemberFilterId').val(),
                    accountNoFilter: $('#AccountNoFilterId').val(),
                    userNameFilter: $('#UserNameFilterId').val(),
                    orderPayTypeFilter: $('#OrderPayTypeFilterId').val(),
                    depositOrderStatusFilter: $('#DepositOrderStatusFilterId').val(),
                    bankOrderStatusFilter: $('#BankOrderStatusFilterId').val(),
                    minTransactionTimeFilter: _selectedDateRang.startDate,
                    maxTransactionTimeFilter: _selectedDateRang.endDate,
                    minMoneyFilter: $('#MinMoneyFilterId').val(),
                    maxMoneyFilter: $('#MaxMoneyFilterId').val(),
                    utcTimeFilter: $('#UtcTimeFilterId').val(),
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

        abp.event.on('app.createOrEditPayOrderDepositModalSaved', function () {
            getPayOrderDeposits();
        });

		$('#GetPayOrderDepositsButton').click(function (e) {
            e.preventDefault();
            getPayOrderDeposits();
        });

		$(document).keypress(function(e) {
		  if(e.which === 13) {
			getPayOrderDeposits();
		  }
		});

        $('.reload-on-change').change(function(e) {
            if (_suppressEvents) return;
                getPayOrderDeposits();
            
		});

        $('.reload-on-keyup').keyup(function(e) {
			getPayOrderDeposits();
		});

        $('#btn-reset-filters').click(function (e) {
            _suppressEvents = true; 
            $('.reload-on-change,.reload-on-keyup,#MyEntsTableFilter').val('');
         
            _selectedDateRang.startDate = moment().startOf('day').add(-1, 'day');
            _selectedDateRang.endDate = moment().startOf('day').add(2, 'day');
                
            Mindatatime.data('daterangepicker').setStartDate(moment().startOf('day').add(-1, 'day'));
            Maxdatatime.data('daterangepicker').setStartDate(moment().startOf('day').add(2, 'day'));
          

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
           
            getPayOrderDeposits();
            
        });

        $(document).on('click', '.see-more', function (e) {
            e.preventDefault(); // Prevent the default link behavior
            var parent = $(this).closest('td'); // Find the parent <td> element
            var shortDescription = parent.find('.short-description'); // Find the truncated description
            var fullDescription = parent.find('.full-description'); // Find the full description

            if (fullDescription.is(':visible')) {
                // Hide full description, show short description and change link text to 'See More'
                shortDescription.show();
                fullDescription.hide();
                $(this).text(app.localize('SeeMore'));
            } else {
                // Show full description, hide short description and change link text to 'Show Less'
                shortDescription.hide();
                fullDescription.show();
                $(this).text(app.localize('SeeLess'));
            }
        });


        var _viewPayOrderDepositModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/PayOrderDeposits/ViewPayOrderDepositModal',
            modalClass: 'ViewPayOrderDepositsModal'
        });
		
    });
})();
