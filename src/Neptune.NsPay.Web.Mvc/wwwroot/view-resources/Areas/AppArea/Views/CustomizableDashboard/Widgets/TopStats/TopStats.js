(function () {
    app.widgets.Widgets_Tenant_TopStats = function () {
        var _tenantDashboardService = abp.services.app.tenantDashboard;
        var _widget;
        var _widgetManager;
        var _selectedDateRange = {
            //startDate: moment().add(-7, 'days').startOf('day'),
            startDate: moment().startOf('day'),
            endDate: moment().endOf('day'),
        };
        this.init = function (widgetManager) {
            _widgetManager = widgetManager;
            _widget = widgetManager.getWidget();
            _widgetManager.runDelayed(getTopStatsData);
        };

        var getTopStatsData = function () {
            _tenantDashboardService.getTopStats(_selectedDateRange.startDate, _selectedDateRange.endDate).done(function (result) {
                _widget.find('#totalMerchantFund').text(result.totalMerchantFund);
                _widget.find('#totalMerchantCount').text(result.totalMerchantCount);
                _widget.find('#totalPayOrderFee').text(result.totalPayOrderFee);
                _widget.find('#totalPayOrderMoney').text(result.totalPayOrderMoney);
                _widget.find('#totalTransferMoney').text(result.totalTransferMoney);
                _widget.find('#totalTransferCount').text(result.totalTransferCount);
                _widget.find('#totalMerchantWithdraw').text(result.totalMerchantWithdraw);
                _widget.find('#totalMerchantWithdrawCount').text(result.totalMerchantWithdrawCount);
                _widget.find('.counterup').counterUp();
            });
        };

        abp.event.on('app.dashboardFilters.DateRangePicker.OnDateChange', function (dateRange) {
            if (!_widget || !dateRange ||
                (_selectedDateRange.startDate === dateRange.startDate
                    && _selectedDateRange.endDate === dateRange.endDate)) {
              return;
            }
            _selectedDateRange.startDate = dateRange.startDate;
            _selectedDateRange.endDate = dateRange.endDate;
            _widgetManager.runDelayed(getTopStatsData);
      });
  };
})();
