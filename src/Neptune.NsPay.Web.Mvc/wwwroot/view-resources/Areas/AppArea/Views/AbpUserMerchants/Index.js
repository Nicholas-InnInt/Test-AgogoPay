(function () {
    $(function () {

        var _$abpUserMerchantsTable = $('#AbpUserMerchantsTable');
        var _abpUserMerchantsService = abp.services.app.abpUserMerchants;
		
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
            getAbpUserMerchants();
        })
        .on('cancel.daterangepicker', function (ev, picker) {
            $(this).val("");
            $selectedDate.startDate = null;
            getAbpUserMerchants();
        });

        $('.endDate').daterangepicker({
            autoUpdateInput: false,
            singleDatePicker: true,
            locale: abp.localization.currentLanguage.name,
            format: 'L',
        })
        .on("apply.daterangepicker", (ev, picker) => {
            $selectedDate.endDate = picker.startDate;
            getAbpUserMerchants();
        })
        .on('cancel.daterangepicker', function (ev, picker) {
            $(this).val("");
            $selectedDate.endDate = null;
            getAbpUserMerchants();
        });

        var _permissions = {
            create: abp.auth.hasPermission('Pages.AbpUserMerchants.Create'),
            edit: abp.auth.hasPermission('Pages.AbpUserMerchants.Edit'),
            'delete': abp.auth.hasPermission('Pages.AbpUserMerchants.Delete')
        };

         var _createOrEditModal = new app.ModalManager({
                    viewUrl: abp.appPath + 'AppArea/AbpUserMerchants/CreateOrEditModal',
                    scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/AbpUserMerchants/_CreateOrEditModal.js',
                    modalClass: 'CreateOrEditAbpUserMerchantModal'
                });
                   

		 var _viewAbpUserMerchantModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/AbpUserMerchants/ViewabpUserMerchantModal',
            modalClass: 'ViewAbpUserMerchantModal'
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

        var dataTable = _$abpUserMerchantsTable.DataTable({
            paging: true,
            serverSide: true,
            processing: true,
            listAction: {
                ajaxFunction: _abpUserMerchantsService.getAll,
                inputFilter: function () {
                    return {
					filter: $('#AbpUserMerchantsTableFilter').val()
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
                                    _viewAbpUserMerchantModal.open({ id: data.record.abpUserMerchant.id });
                                }
                        },
						{
                            text: app.localize('Edit'),
                            visible: function () {
                                return _permissions.edit;
                            },
                            action: function (data) {
                            _createOrEditModal.open({ id: data.record.abpUserMerchant.id });                                
                            }
                        }, 
						{
                            text: app.localize('Delete'),
                            visible: function () {
                                return _permissions.delete;
                            },
                            action: function (data) {
                                deleteAbpUserMerchant(data.record.abpUserMerchant);
                            }
                        }]
                    }
                }
            ]
        });

        function getAbpUserMerchants() {
            dataTable.ajax.reload();
        }

        function deleteAbpUserMerchant(abpUserMerchant) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _abpUserMerchantsService.delete({
                            id: abpUserMerchant.id
                        }).done(function () {
                            getAbpUserMerchants(true);
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

        $('#CreateNewAbpUserMerchantButton').click(function () {
            _createOrEditModal.open();
        });        

		

        abp.event.on('app.createOrEditAbpUserMerchantModalSaved', function () {
            getAbpUserMerchants();
        });

		$('#GetAbpUserMerchantsButton').click(function (e) {
            e.preventDefault();
            getAbpUserMerchants();
        });

		$(document).keypress(function(e) {
		  if(e.which === 13) {
			getAbpUserMerchants();
		  }
		});

        $('.reload-on-change').change(function(e) {
			getAbpUserMerchants();
		});

        $('.reload-on-keyup').keyup(function(e) {
			getAbpUserMerchants();
		});

        $('#btn-reset-filters').click(function (e) {
            $('.reload-on-change,.reload-on-keyup,#MyEntsTableFilter').val('');
            getAbpUserMerchants();
        });
		
		
		

    });
})();
