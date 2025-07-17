(function () {
    $(function () {

        var _$merchantRatesTable = $('#MerchantRatesTable');
        var _merchantRatesService = abp.services.app.merchantRates;
		
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
            getMerchantRates();
        })
        .on('cancel.daterangepicker', function (ev, picker) {
            $(this).val("");
            $selectedDate.startDate = null;
            getMerchantRates();
        });

        $('.endDate').daterangepicker({
            autoUpdateInput: false,
            singleDatePicker: true,
            locale: abp.localization.currentLanguage.name,
            format: 'L',
        })
        .on("apply.daterangepicker", (ev, picker) => {
            $selectedDate.endDate = picker.startDate;
            getMerchantRates();
        })
        .on('cancel.daterangepicker', function (ev, picker) {
            $(this).val("");
            $selectedDate.endDate = null;
            getMerchantRates();
        });

        var _permissions = {
            create: abp.auth.hasPermission('Pages.MerchantRates.Create'),
            edit: abp.auth.hasPermission('Pages.MerchantRates.Edit'),
            'delete': abp.auth.hasPermission('Pages.MerchantRates.Delete')
        };

         var _createOrEditModal = new app.ModalManager({
                    viewUrl: abp.appPath + 'AppArea/MerchantRates/CreateOrEditModal',
                    scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/MerchantRates/_CreateOrEditModal.js',
                    modalClass: 'CreateOrEditMerchantRateModal'
                });
                   

		 var _viewMerchantRateModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/MerchantRates/ViewmerchantRateModal',
            modalClass: 'ViewMerchantRateModal'
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

        var dataTable = _$merchantRatesTable.DataTable({
            paging: true,
            serverSide: true,
            processing: true,
            listAction: {
                ajaxFunction: _merchantRatesService.getAll,
                inputFilter: function () {
                    return {
					filter: $('#MerchantRatesTableFilter').val(),
					merchantCodeFilter: $('#MerchantCodeFilterId').val(),
					minMerchantIdFilter: $('#MinMerchantIdFilterId').val(),
					maxMerchantIdFilter: $('#MaxMerchantIdFilterId').val(),
					minScanBankRateFilter: $('#MinScanBankRateFilterId').val(),
					maxScanBankRateFilter: $('#MaxScanBankRateFilterId').val(),
					minScratchCardRateFilter: $('#MinScratchCardRateFilterId').val(),
					maxScratchCardRateFilter: $('#MaxScratchCardRateFilterId').val(),
					minMoMoRateFilter: $('#MinMoMoRateFilterId').val(),
					maxMoMoRateFilter: $('#MaxMoMoRateFilterId').val()
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
                                    _viewMerchantRateModal.open({ id: data.record.merchantRate.id });
                                }
                        },
						{
                            text: app.localize('Edit'),
                            visible: function () {
                                return _permissions.edit;
                            },
                            action: function (data) {
                            _createOrEditModal.open({ id: data.record.merchantRate.id });                                
                            }
                        }, 
						{
                            text: app.localize('Delete'),
                            visible: function () {
                                return _permissions.delete;
                            },
                            action: function (data) {
                                deleteMerchantRate(data.record.merchantRate);
                            }
                        }]
                    }
                },
					{
						targets: 2,
						 data: "merchantRate.merchantCode",
						 name: "merchantCode"   
					},
					{
						targets: 3,
						 data: "merchantRate.merchantId",
						 name: "merchantId"   
					},
					{
						targets: 4,
						 data: "merchantRate.scanBankRate",
						 name: "scanBankRate"   
					},
					{
						targets: 5,
						 data: "merchantRate.scratchCardRate",
						 name: "scratchCardRate"   
					},
					{
						targets: 6,
						 data: "merchantRate.moMoRate",
						 name: "moMoRate"   
					}
            ]
        });

        function getMerchantRates() {
            dataTable.ajax.reload();
        }

        function deleteMerchantRate(merchantRate) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _merchantRatesService.delete({
                            id: merchantRate.id
                        }).done(function () {
                            getMerchantRates(true);
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

        $('#CreateNewMerchantRateButton').click(function () {
            _createOrEditModal.open();
        });        

		

        abp.event.on('app.createOrEditMerchantRateModalSaved', function () {
            getMerchantRates();
        });

		$('#GetMerchantRatesButton').click(function (e) {
            e.preventDefault();
            getMerchantRates();
        });

		$(document).keypress(function(e) {
		  if(e.which === 13) {
			getMerchantRates();
		  }
		});

        $('.reload-on-change').change(function(e) {
			getMerchantRates();
		});

        $('.reload-on-keyup').keyup(function(e) {
			getMerchantRates();
		});

        $('#btn-reset-filters').click(function (e) {
            $('.reload-on-change,.reload-on-keyup,#MyEntsTableFilter').val('');
            getMerchantRates();
        });
		
		
		

    });
})();
