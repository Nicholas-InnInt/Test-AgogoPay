(function () {
    $(function () {

        var _$nsPaySystemSettingsTable = $('#NsPaySystemSettingsTable');
        var _nsPaySystemSettingsService = abp.services.app.nsPaySystemSettings;
		
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
            getNsPaySystemSettings();
        })
        .on('cancel.daterangepicker', function (ev, picker) {
            $(this).val("");
            $selectedDate.startDate = null;
            getNsPaySystemSettings();
        });

        $('.endDate').daterangepicker({
            autoUpdateInput: false,
            singleDatePicker: true,
            locale: abp.localization.currentLanguage.name,
            format: 'L',
        })
        .on("apply.daterangepicker", (ev, picker) => {
            $selectedDate.endDate = picker.startDate;
            getNsPaySystemSettings();
        })
        .on('cancel.daterangepicker', function (ev, picker) {
            $(this).val("");
            $selectedDate.endDate = null;
            getNsPaySystemSettings();
        });

        var _permissions = {
            create: abp.auth.hasPermission('Pages.NsPaySystemSettings.Create'),
            edit: abp.auth.hasPermission('Pages.NsPaySystemSettings.Edit'),
            'delete': abp.auth.hasPermission('Pages.NsPaySystemSettings.Delete')
        };

         var _createOrEditModal = new app.ModalManager({
                    viewUrl: abp.appPath + 'AppArea/NsPaySystemSettings/CreateOrEditModal',
                    scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/NsPaySystemSettings/_CreateOrEditModal.js',
                    modalClass: 'CreateOrEditNsPaySystemSettingModal'
                });
                   

		 var _viewNsPaySystemSettingModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/NsPaySystemSettings/ViewnsPaySystemSettingModal',
            modalClass: 'ViewNsPaySystemSettingModal'
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

        var dataTable = _$nsPaySystemSettingsTable.DataTable({
            paging: true,
            serverSide: true,
            processing: true,
            listAction: {
                ajaxFunction: _nsPaySystemSettingsService.getAll,
                inputFilter: function () {
                    return {
					filter: $('#NsPaySystemSettingsTableFilter').val(),
					keyFilter: $('#KeyFilterId').val(),
					valueFilter: $('#ValueFilterId').val()
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
						//{
      //                          text: app.localize('View'),
      //                          action: function (data) {
      //                              _viewNsPaySystemSettingModal.open({ id: data.record.nsPaySystemSetting.id });
      //                          }
      //                  },
						{
                            text: app.localize('Edit'),
                            visible: function () {
                                return _permissions.edit;
                            },
                            action: function (data) {
                            _createOrEditModal.open({ id: data.record.nsPaySystemSetting.id });                                
                            }
                        }, 
						{
                            text: app.localize('Delete'),
                            visible: function () {
                                return _permissions.delete;
                            },
                            action: function (data) {
                                deleteNsPaySystemSetting(data.record.nsPaySystemSetting);
                            }
                        }]
                    }
                },
					{
						targets: 2,
						 data: "nsPaySystemSetting.key",
						 name: "key"   
					},
					{
						targets: 3,
						 data: "nsPaySystemSetting.value",
						 name: "value"   
					}
            ]
        });

        function getNsPaySystemSettings() {
            dataTable.ajax.reload();
        }

        function deleteNsPaySystemSetting(nsPaySystemSetting) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _nsPaySystemSettingsService.delete({
                            id: nsPaySystemSetting.id
                        }).done(function () {
                            getNsPaySystemSettings(true);
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

        $('#CreateNewNsPaySystemSettingButton').click(function () {
            _createOrEditModal.open();
        });        

		

        abp.event.on('app.createOrEditNsPaySystemSettingModalSaved', function () {
            getNsPaySystemSettings();
        });

		$('#GetNsPaySystemSettingsButton').click(function (e) {
            e.preventDefault();
            getNsPaySystemSettings();
        });

		$(document).keypress(function(e) {
		  if(e.which === 13) {
			getNsPaySystemSettings();
		  }
		});

        $('.reload-on-change').change(function(e) {
			getNsPaySystemSettings();
		});

        $('.reload-on-keyup').keyup(function(e) {
			getNsPaySystemSettings();
		});

        $('#btn-reset-filters').click(function (e) {
            $('.reload-on-change,.reload-on-keyup,#MyEntsTableFilter').val('');
            getNsPaySystemSettings();
        });
		
		
		

    });
})();
