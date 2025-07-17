(function () {
    $(function () {

        var _$merchantSettingsTable = $('#MerchantSettingsTable');
        var _merchantSettingsService = abp.services.app.merchantSettings;
		
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
            getMerchantSettings();
        })
        .on('cancel.daterangepicker', function (ev, picker) {
            $(this).val("");
            $selectedDate.startDate = null;
            getMerchantSettings();
        });

        $('.endDate').daterangepicker({
            autoUpdateInput: false,
            singleDatePicker: true,
            locale: abp.localization.currentLanguage.name,
            format: 'L',
        })
        .on("apply.daterangepicker", (ev, picker) => {
            $selectedDate.endDate = picker.startDate;
            getMerchantSettings();
        })
        .on('cancel.daterangepicker', function (ev, picker) {
            $(this).val("");
            $selectedDate.endDate = null;
            getMerchantSettings();
        });

        var _permissions = {
            create: abp.auth.hasPermission('Pages.MerchantSettings.Create'),
            edit: abp.auth.hasPermission('Pages.MerchantSettings.Edit'),
            'delete': abp.auth.hasPermission('Pages.MerchantSettings.Delete')
        };

         var _createOrEditModal = new app.ModalManager({
                    viewUrl: abp.appPath + 'AppArea/MerchantSettings/CreateOrEditModal',
                    scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/MerchantSettings/_CreateOrEditModal.js',
                    modalClass: 'CreateOrEditMerchantSettingModal'
                });
                   

		 var _viewMerchantSettingModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/MerchantSettings/ViewmerchantSettingModal',
            modalClass: 'ViewMerchantSettingModal'
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

        var dataTable = _$merchantSettingsTable.DataTable({
            paging: true,
            serverSide: true,
            processing: true,
            responsive: true,
            listAction: {
                ajaxFunction: _merchantSettingsService.getAll,
                inputFilter: function () {
                    return {
					filter: $('#MerchantSettingsTableFilter').val(),
					merchantCodeFilter: $('#MerchantCodeFilterId').val(),
					minMerchantIdFilter: $('#MinMerchantIdFilterId').val(),
					maxMerchantIdFilter: $('#MaxMerchantIdFilterId').val(),
					nsPayTitleFilter: $('#NsPayTitleFilterId').val(),
					logoUrlFilter: $('#LogoUrlFilterId').val(),
					loginIpAddressFilter: $('#LoginIpAddressFilterId').val(),
					bankNotifyFilter: $('#BankNotifyFilterId').val(),
					bankNotifyTextFilter: $('#BankNotifyTextFilterId').val(),
					telegramNotifyBotIdFilter: $('#TelegramNotifyBotIdFilterId').val(),
					telegramNotifyChatIdFilter: $('#TelegramNotifyChatIdFilterId').val(),
					openRiskWithdrawalFilter: $('#OpenRiskWithdrawalFilterId').val(),
					platformUrlFilter: $('#PlatformUrlFilterId').val(),
					platformUserNameFilter: $('#PlatformUserNameFilterId').val(),
					platformPassWordFilter: $('#PlatformPassWordFilterId').val(),
					minPlatformLimitMoneyFilter: $('#MinPlatformLimitMoneyFilterId').val(),
					maxPlatformLimitMoneyFilter: $('#MaxPlatformLimitMoneyFilterId').val()
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
                                    _viewMerchantSettingModal.open({ id: data.record.merchantSetting.id });
                                }
                        },
						//{
      //                      text: app.localize('Edit'),
      //                      visible: function () {
      //                          return _permissions.edit;
      //                      },
      //                      action: function (data) {
      //                      _createOrEditModal.open({ id: data.record.merchantSetting.id });                                
      //                      }
      //                  }, 
						{
                            text: app.localize('Delete'),
                            visible: function () {
                                return _permissions.delete;
                            },
                            action: function (data) {
                                deleteMerchantSetting(data.record.merchantSetting);
                            }
                        }]
                    }
                },
					{
						targets: 2,
						 data: "merchantSetting.merchantCode",
						 name: "merchantCode"   
					},
					{
						targets: 3,
						 data: "merchantSetting.merchantId",
						 name: "merchantId"   
					},
					{
						targets: 4,
						 data: "merchantSetting.nsPayTitle",
						 name: "nsPayTitle"   
					},
					{
						targets: 5,
						 data: "merchantSetting.logoUrl",
						 name: "logoUrl"   
					},
					{
						targets: 6,
						 data: "merchantSetting.loginIpAddress",
						 name: "loginIpAddress"   
					},
					{
						targets: 7,
						 data: "merchantSetting.bankNotify",
						 name: "bankNotify"   
					},
					{
						targets: 8,
						 data: "merchantSetting.bankNotifyText",
						 name: "bankNotifyText"   
					},
					{
						targets: 9,
						 data: "merchantSetting.telegramNotifyBotId",
						 name: "telegramNotifyBotId"   
					},
					{
						targets: 10,
						 data: "merchantSetting.telegramNotifyChatId",
						 name: "telegramNotifyChatId"   
					},
					{
						targets: 11,
						 data: "merchantSetting.openRiskWithdrawal",
						 name: "openRiskWithdrawal"  ,
						render: function (openRiskWithdrawal) {
							if (openRiskWithdrawal) {
								return '<div class="text-center"><i class="fa fa-check text-success" title="True"></i></div>';
							}
							return '<div class="text-center"><i class="fa fa-times-circle" title="False"></i></div>';
					}
			 
					},
					{
						targets: 12,
						 data: "merchantSetting.platformUrl",
						 name: "platformUrl"   
					},
					{
						targets: 13,
						 data: "merchantSetting.platformUserName",
						 name: "platformUserName"   
					},
					{
						targets: 14,
						 data: "merchantSetting.platformPassWord",
						 name: "platformPassWord"   
					},
					{
						targets: 15,
						 data: "merchantSetting.platformLimitMoney",
						 name: "platformLimitMoney"   
					}
            ]
        });

        function getMerchantSettings() {
            dataTable.ajax.reload();
        }

        function deleteMerchantSetting(merchantSetting) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _merchantSettingsService.delete({
                            id: merchantSetting.id
                        }).done(function () {
                            getMerchantSettings(true);
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

        $('#CreateNewMerchantSettingButton').click(function () {
            _createOrEditModal.open();
        });        

		

        abp.event.on('app.createOrEditMerchantSettingModalSaved', function () {
            getMerchantSettings();
        });

		$('#GetMerchantSettingsButton').click(function (e) {
            e.preventDefault();
            getMerchantSettings();
        });

		$(document).keypress(function(e) {
		  if(e.which === 13) {
			getMerchantSettings();
		  }
		});

        $('.reload-on-change').change(function(e) {
			getMerchantSettings();
		});

        $('.reload-on-keyup').keyup(function(e) {
			getMerchantSettings();
		});

        $('#btn-reset-filters').click(function (e) {
            $('.reload-on-change,.reload-on-keyup,#MyEntsTableFilter').val('');
            $('select.reload-on-change').each(function (index, element) {

                var firstSelectionValue = $(element).find("option:first").val();

                if ($(element).hasClass("select2")) {
                    $(element).select2('val', "-1");
                }
                else {
                    $(element).val(firstSelectionValue);// drop  downlist select first option
                }

            });
            
            getMerchantSettings();
        });
		
		
		

    });
})();
