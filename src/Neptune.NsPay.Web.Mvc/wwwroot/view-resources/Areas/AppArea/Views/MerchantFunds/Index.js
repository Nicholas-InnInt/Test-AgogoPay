(function () {
    $(function () {

        var _$merchantFundsTable = $('#MerchantFundsTable');
        var _merchantFundsService = abp.services.app.merchantFunds;
		
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
            getMerchantFunds();
        })
        .on('cancel.daterangepicker', function (ev, picker) {
            $(this).val("");
            $selectedDate.startDate = null;
            getMerchantFunds();
        });

        $('.endDate').daterangepicker({
            autoUpdateInput: false,
            singleDatePicker: true,
            locale: abp.localization.currentLanguage.name,
            format: 'L',
        })
        .on("apply.daterangepicker", (ev, picker) => {
            $selectedDate.endDate = picker.startDate;
            getMerchantFunds();
        })
        .on('cancel.daterangepicker', function (ev, picker) {
            $(this).val("");
            $selectedDate.endDate = null;
            getMerchantFunds();
        });

        var _permissions = {
            create: abp.auth.hasPermission('Pages.MerchantFunds.Create'),
            edit: abp.auth.hasPermission('Pages.MerchantFunds.Edit'),
            'delete': abp.auth.hasPermission('Pages.MerchantFunds.Delete')
        };

         var _createOrEditModal = new app.ModalManager({
                    viewUrl: abp.appPath + 'AppArea/MerchantFunds/CreateOrEditModal',
                    scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/MerchantFunds/_CreateOrEditModal.js',
                    modalClass: 'CreateOrEditMerchantFundModal'
                });
                   

		 var _viewMerchantFundModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/MerchantFunds/ViewmerchantFundModal',
            modalClass: 'ViewMerchantFundModal'
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

        var dataTable = _$merchantFundsTable.DataTable({
            paging: true,
            serverSide: true,
            processing: true,
            listAction: {
                ajaxFunction: _merchantFundsService.getAll,
                inputFilter: function () {
                    return {
					filter: $('#MerchantFundsTableFilter').val(),
					merchantCodeFilter: $('#MerchantCodeFilterId').val(),
					minMerchantIdFilter: $('#MinMerchantIdFilterId').val(),
					maxMerchantIdFilter: $('#MaxMerchantIdFilterId').val(),
					minDepositAmountFilter: $('#MinDepositAmountFilterId').val(),
					maxDepositAmountFilter: $('#MaxDepositAmountFilterId').val(),
					minWithdrawalAmountFilter: $('#MinWithdrawalAmountFilterId').val(),
					maxWithdrawalAmountFilter: $('#MaxWithdrawalAmountFilterId').val(),
					minRateFeeBalanceFilter: $('#MinRateFeeBalanceFilterId').val(),
					maxRateFeeBalanceFilter: $('#MaxRateFeeBalanceFilterId').val(),
					minBalanceFilter: $('#MinBalanceFilterId').val(),
					maxBalanceFilter: $('#MaxBalanceFilterId').val(),
					minCreationTimeFilter:  getDateFilter($('#MinCreationTimeFilterId')),
					maxCreationTimeFilter:  getMaxDateFilter($('#MaxCreationTimeFilterId')),
					minUpdateTimeFilter:  getDateFilter($('#MinUpdateTimeFilterId')),
					maxUpdateTimeFilter:  getMaxDateFilter($('#MaxUpdateTimeFilterId'))
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
                                    _viewMerchantFundModal.open({ id: data.record.merchantFund.id });
                                }
                        },
						{
                            text: app.localize('Edit'),
                            visible: function () {
                                return _permissions.edit;
                            },
                            action: function (data) {
                            _createOrEditModal.open({ id: data.record.merchantFund.id });                                
                            }
                        }, 
						{
                            text: app.localize('Delete'),
                            visible: function () {
                                return _permissions.delete;
                            },
                            action: function (data) {
                                deleteMerchantFund(data.record.merchantFund);
                            }
                        }]
                    }
                },
					{
						targets: 2,
						 data: "merchantFund.merchantCode",
						 name: "merchantCode"   
					},
					{
						targets: 3,
						 data: "merchantFund.merchantId",
						 name: "merchantId"   
					},
					{
						targets: 4,
						 data: "merchantFund.depositAmount",
						 name: "depositAmount"   
					},
					{
						targets: 5,
						 data: "merchantFund.withdrawalAmount",
						 name: "withdrawalAmount"   
					},
					{
						targets: 6,
						 data: "merchantFund.rateFeeBalance",
						 name: "rateFeeBalance"   
					},
					{
						targets: 7,
						 data: "merchantFund.balance",
						 name: "balance"   
					},
					{
						targets: 8,
						 data: "merchantFund.creationTime",
						 name: "creationTime" ,
					render: function (creationTime) {
						if (creationTime) {
							return moment(creationTime).format('L');
						}
						return "";
					}
			  
					},
					{
						targets: 9,
						 data: "merchantFund.updateTime",
						 name: "updateTime" ,
					render: function (updateTime) {
						if (updateTime) {
							return moment(updateTime).format('L');
						}
						return "";
					}
			  
					}
            ]
        });

        function getMerchantFunds() {
            dataTable.ajax.reload();
        }

        function deleteMerchantFund(merchantFund) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _merchantFundsService.delete({
                            id: merchantFund.id
                        }).done(function () {
                            getMerchantFunds(true);
                            abp.notify.success(app.localize('SuccessfullyDeleted'));
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

        $('#CreateNewMerchantFundButton').click(function () {
            _createOrEditModal.open();
        });        

		

        abp.event.on('app.createOrEditMerchantFundModalSaved', function () {
            getMerchantFunds();
        });

		$('#GetMerchantFundsButton').click(function (e) {
            e.preventDefault();
            getMerchantFunds();
        });

		$(document).keypress(function(e) {
		  if(e.which === 13) {
			getMerchantFunds();
		  }
		});

        $('.reload-on-change').change(function(e) {
			getMerchantFunds();
		});

        $('.reload-on-keyup').keyup(function(e) {
			getMerchantFunds();
		});

        $('#btn-reset-filters').click(function (e) {
            $('.reload-on-change,.reload-on-keyup,#MyEntsTableFilter').val('');
            getMerchantFunds();
        });
		
		
		

    });
})();
