(function () {
    $(function () {

        var _$merchantDashboardTable = $('#MerchantDashboardTable');
        var _merchantDashboardService = abp.services.app.merchantDashboard;

        $('.select2').select2({
            placeholder: app.localize('Select'),
            theme: 'bootstrap5',
            width: "100%",
            allowClear: true,
            selectionCssClass: 'form-select',
            language: abp.localization.currentCulture.name,
        });

        var _startSettings = {
            startDate: moment().startOf('day'),
            endDate: moment().endOf('day')          
        };

        var _selectedDateRang = {
            startDate: _startSettings.startDate,
            endDate: _startSettings.endDate,
        };
   

        var _permissions = {
            view: abp.auth.hasPermission('Pages.MerchantDashboard.View')
        };


        $('#OrderCreationTimeRange').daterangepicker(
            $.extend(true, app.createDateRangePickerOptions()),
            function (start, end) {
                _selectedDateRang.startDate = start;
                _selectedDateRang.endDate = end;
            }
        );
		
        var dataTable = _$merchantDashboardTable.DataTable({
            info: true,
            paging: true,
            serverSide: true,
            processing: true,
            order: [[2, 'desc']], 
            listAction: {
                ajaxFunction: _merchantDashboardService.getAll,
                inputFilter: function () {
                    var selectedMerchantCode = $('#MerchantCodeFilterId').val();

                    if (selectedMerchantCode.length > 0)
                    {
                        selectedMerchantCode = selectedMerchantCode.join(',')
                    }

                    var orderColumn = dataTable.order()[0][0];  // column index
                    var orderDirection = dataTable.order()[0][1];  // 'asc' or 'desc'

                    var columnNames = [
                        "null",         
                        "merchantCode",   
                        "totalMerchantFund", 
                        "totalMerchantFee",  
                        "totalMerchantBillCashIn",
                        "totalMerchantBillWithdraw", 
                        "totalWithdrawalOrder"
                    ];

                    var orderColumnName = orderColumn !== null ? columnNames[orderColumn] : null;

                    return {
                        merchantCodeFilter: selectedMerchantCode,
                        orderCreationTimeStartDate: _selectedDateRang.startDate,
                        orderCreationTimeEndDate: _selectedDateRang.endDate,
                        orderColumn: orderColumnName,
                        orderDirection: orderDirection
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
                    data: "merchantDashboard.merchantCode",
                    orderable: true,
                    render: function (data, type, full, meta) {
                        var html = "<span>" + app.localize('MerchantName') + "\uff1a" + full.merchantName + "</span><br/>" +
                            "<span>" + app.localize('MerchantCode') + "\uff1a" + full.merchantCode + "</span><br/>";
                        //if (_permissions.view) {
                        //    html += "<span>" + app.localize('WithdrawalDevice') + "\uff1a" + full.withdrawalDevice.name + "</span>";
                        //}
                        return html;
                    }
                },
                {
                    targets: 2,
                    data: "merchantDashboard.totalMerchantFund",
                    orderable: true,
                    render: function (data, type, full, meta) {
                        return "<span>"  + full.totalMerchantFund + "</span>";
                    }
                },
                {
                    targets: 3,
                    data: "merchantDashboard.totalFrozenBalance",
                    orderable: true,
                    render: function (data, type, full, meta) {
                        return "<span>" + full.totalFrozenBalance + "</span>";
                    }
                },
                {
                    targets: 4,
                    data: "merchantDashboard.totalMerchantFee",
                    orderable: true,
                    render: function (data, type, full, meta) {
                        return  "<span>" + app.localize('CurrentMerchantFee') + "\uff1a" + full.currentMerchantFee + " </span>";
                    }
                },
                {
                    targets: 5,
                    data: "merchantDashboard.totalPayOrderCashIn",
                    orderable: true,
                    render: function (data, type, full, meta) {
                        var html = "<span>" + app.localize('CurrentMerchantBillCashIn') + "\uff1a" + full.currentPayOrderCashIn + " </span><br/>";
                        full.currentPayOrderCashInByTypes.forEach(function (item) {
                            html += "<span>" + app.localize('Enum_MerchantDashboardPayChannelEnum_' + item.paymentChannel) + "\uff1a" + item.cashInByType + " </span><br/>";
                        });
                        return html;
                    }
                },
                {
                    targets: 6,
                    data: "merchantDashboard.totalMerchantBillWithdraw",
                    orderable: true,
                    render: function (data, type, full, meta) {
                        return    "<span>" + app.localize('CurrentMerchantBillWithdraw') + "\uff1a" + full.currentMerchantWithdraw + " </span>";
                    }
                },
                {
                    targets: 7,
                    data: "merchantDashboard.totalWithdrawalOrder",
                    orderable: true,
                    render: function (data, type, full, meta) {
                        return "<span>" + app.localize('CurrentWithdrawalOrder') + "\uff1a" + full.currentWithdrawalOrder + " </span>";
                    }
                }
            ],
            drawCallback: function (settings) {
                $("#totalMerchantCount").text(settings.rawServerResponse.totalMerchantCount);
                $("#totalMerchantFund").text(settings.rawServerResponse.totalMerchantFund);
                $("#totalFrozenBalance").text(settings.rawServerResponse.totalFrozenBalance);
                $("#totalMerchantFee").text(settings.rawServerResponse.totalMerchantFee);
                $("#totalPayOrderCashInCount").text(settings.rawServerResponse.totalPayOrderCashInCount);
                $("#totalPayOrderCashIn").text(settings.rawServerResponse.totalPayOrderCashIn);
                $("#totalMerchantBillWithdrawCount").text(settings.rawServerResponse.totalMerchantBillWithdrawCount);
                $("#totalMerchantBillWithdraw").text(settings.rawServerResponse.totalMerchantBillWithdraw);
                $("#totalWithdrawalOrderCount").text(settings.rawServerResponse.totalWithdrawalOrderCount);
                $("#totalWithdrawalOrder").text(settings.rawServerResponse.totalWithdrawalOrder);
            }
        });

        function getMerchantDashboard() {
            dataTable.ajax.reload();
        }
        function getMerchants() {
            _merchantDashboardService.getOrderMerchants({
            }).done(function (result) {
                $("#MerchantCodeFilterId").empty();
                for (var i = 0; i < result.length; i++) {
                    $("#MerchantCodeFilterId").append("<option value='" + result[i].merchantCode + "'>" + result[i].merchantName + "[" + result[i].merchantCode + "]" + "</option>");
                }
            });
        }

		$('#GetMerchantDashboardButton').click(function (e) {
            e.preventDefault();
            var selectedValues = $('#MerchantCodeFilterId').val();
            getMerchantDashboard();
        });

		$(document).keypress(function(e) {
		  if(e.which === 13) {
              getMerchantDashboard();
		  }
		});

        $('.reload-on-change').change(function(e) {
            getMerchantDashboard();
		});

        $('.reload-on-keyup').keyup(function(e) {
            getMerchantDashboard();
		});

    });
})();
