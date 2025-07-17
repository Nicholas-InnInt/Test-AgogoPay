(function () {
    app.widgets.Widgets_Tenant_MerchantBillsSummary = function () {
        var _tenantDashboardService = abp.services.app.tenantDashboard;
        var _widget;

        var _selectedDateRange = {
            //startDate: moment().add(-7, 'days').startOf('day'),
            startDate: moment().startOf('day'),
            endDate: moment().endOf('day'),
        };

        var merchantBillsSummaryDatePeriod = {
            daily: 1,
            weekly: 2,
        };

        //this.init = function (widgetManager) {
        //    _widget = widgetManager.getWidget();
        //    getMerchantBillsSummary();;
        //};

        //var transformChartData = function (data) {
        //    var labels = [];
        //    var orderInData = {
        //        label: app.localize('OrderIn'),
        //        data: [],
        //        borderColor: '#50CD89',
        //        backgroundColor: '#50CD89',
        //        fill: true
        //    };

        //    var withdrawData = {
        //        label: app.localize('Withdraw'),
        //        data: [],
        //        borderColor: '#F1416C',
        //        backgroundColor: '#F1416C',
        //        fill: true
        //    };

        //    for (var i = 0; i < data.length; i++) {
        //        labels.push(data[i].period);
        //        orderInData.data.push(data[i].orderIn);
        //        withdrawData.data.push(data[i].withdraw);
        //    }

        //    return {
        //        labels: labels,
        //        datasets: [orderInData, withdrawData]
        //    };
        //};

        //var initMerchantBillsSummaryChart = function (merchantBillsSummaryData) {
        //    var MerchantBillsSummaryChart = function () {
        //        var init = function (merchantBillsData) {
        //            const data = transformChartData(merchantBillsData);

        //            const config = {
        //                type: 'line',
        //                data: data,
        //                options: {
        //                    responsive: true,
        //                    plugins: {
        //                        tooltip: {
        //                            mode: 'index'
        //                        },
        //                    },
        //                    interaction: {
        //                        mode: 'nearest',
        //                        axis: 'x',
        //                        intersect: false
        //                    },
                        
        //                    scales: {
        //                        y: {
        //                            stacked: true,
        //                            beginAtZero: true
        //                        }
        //                    }
        //                }
        //            };

        //            if (this.graph) {
        //                this.graph.destroy();
        //            }
   
        //            this.graph = new Chart(document.getElementById('MerchantBillsStatistics'), config);

        //        };
                
        //        var refresh = function (datePeriod) {
        //            _tenantDashboardService
        //                .getMerchantBillsSummary({
        //                    merchantBillsSummaryDatePeriod: datePeriod,
        //                    startDate: _selectedDateRange.startDate,
        //                    endDate: _selectedDateRange.endDate
        //                })
        //                .done(function (result) {
        //                    init(result.merchantBillsSummary);
        //                });
        //        };

        //        var draw = function (data) {
                    
        //            if (!this.graph) {
        //                init(data);
        //            } else {
        //                refresh(data);
        //            }
        //        };

        //        return {
        //            draw: draw,
        //            refresh: refresh,
        //            graph: this.graph
        //        };
        //    };

        //    _widget.find('#MerchantBillsStatistics').show();

        //    var merchantBillsSummary = new MerchantBillsSummaryChart();
        //    merchantBillsSummary.draw(merchantBillsSummaryData);
            
        //    $(_widget).find("input[name='MerchantBillsSummaryDateInterval']").unbind('change');
        //    $(_widget)
        //        .find("input[name='MerchantBillsSummaryDateInterval']")
        //        .change(function () {
        //            $(this).closest('.btn-group').find('.btn').removeClass('active');
        //            $(this).closest('.btn').addClass('active');
        //            merchantBillsSummary.refresh(this.value);
        //        });

        //    _widget.find('#MerchantBillsStatisticsLoading').hide();
        //};

        //var getMerchantBillsSummary = function () {
        //    abp.ui.setBusy(_widget);
        //    _tenantDashboardService
        //        .getMerchantBillsSummary({
        //            merchantBillsSummaryDatePeriod: merchantBillsSummaryDatePeriod.daily,
        //            startDate: _selectedDateRange.startDate,
        //            endDate: _selectedDateRange.endDate
        //        })
        //        .done(function (result) {
        //            initMerchantBillsSummaryChart(result.merchantBillsSummary);
        //        })
        //        .always(function () {
        //            abp.ui.clearBusy(_widget);
        //        });
        //};

        //abp.event.on('app.dashboardFilters.DateRangePicker.OnDateChange', function (dateRange) {
        //    if (!_widget || !dateRange ||
        //        (_selectedDateRange.startDate === dateRange.startDate
        //            && _selectedDateRange.endDate === dateRange.endDate)) {
        //        return;
        //    }
        //    _selectedDateRange.startDate = dateRange.startDate;
        //    _selectedDateRange.endDate = dateRange.endDate;

        //    $(_widget).find('.btn-group .btn:first-child').click();

        //    getMerchantBillsSummary();
        //});
    };
})();
