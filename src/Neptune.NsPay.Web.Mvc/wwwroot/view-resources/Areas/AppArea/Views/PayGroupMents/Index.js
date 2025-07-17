(function () {
    $(function () {

        var _$payGroupMentsTable = $('#PayGroupMentsTable');
        var _payGroupMentsService = abp.services.app.payGroupMents;
		
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
            getPayGroupMents();
        })
        .on('cancel.daterangepicker', function (ev, picker) {
            $(this).val("");
            $selectedDate.startDate = null;
            getPayGroupMents();
        });

        $('.endDate').daterangepicker({
            autoUpdateInput: false,
            singleDatePicker: true,
            locale: abp.localization.currentLanguage.name,
            format: 'L',
        })
        .on("apply.daterangepicker", (ev, picker) => {
            $selectedDate.endDate = picker.startDate;
            getPayGroupMents();
        })
        .on('cancel.daterangepicker', function (ev, picker) {
            $(this).val("");
            $selectedDate.endDate = null;
            getPayGroupMents();
        });

        var _permissions = {
            create: abp.auth.hasPermission('Pages.PayGroupMents.Create'),
            edit: abp.auth.hasPermission('Pages.PayGroupMents.Edit'),
            'delete': abp.auth.hasPermission('Pages.PayGroupMents.Delete')
        };

         var _createOrEditModal = new app.ModalManager({
                    viewUrl: abp.appPath + 'AppArea/PayGroupMents/CreateOrEditModal',
                    scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/PayGroupMents/_CreateOrEditModal.js',
                    modalClass: 'CreateOrEditPayGroupMentModal'
                });
                   

		 var _viewPayGroupMentModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/PayGroupMents/ViewpayGroupMentModal',
            modalClass: 'ViewPayGroupMentModal'
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

        var dataTable = _$payGroupMentsTable.DataTable({
            paging: true,
            serverSide: true,
            processing: true,
            listAction: {
                ajaxFunction: _payGroupMentsService.getAll,
                inputFilter: function () {
                    return {
					filter: $('#PayGroupMentsTableFilter').val(),
					minGroupIdFilter: $('#MinGroupIdFilterId').val(),
					maxGroupIdFilter: $('#MaxGroupIdFilterId').val(),
					minPayMentIdFilter: $('#MinPayMentIdFilterId').val(),
					maxPayMentIdFilter: $('#MaxPayMentIdFilterId').val(),
					statusFilter: $('#StatusFilterId').val()
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
                                    _viewPayGroupMentModal.open({ id: data.record.payGroupMent.id });
                                }
                        },
						{
                            text: app.localize('Edit'),
                            visible: function () {
                                return _permissions.edit;
                            },
                            action: function (data) {
                            _createOrEditModal.open({ id: data.record.payGroupMent.id });                                
                            }
                        }, 
						{
                            text: app.localize('Delete'),
                            visible: function () {
                                return _permissions.delete;
                            },
                            action: function (data) {
                                deletePayGroupMent(data.record.payGroupMent);
                            }
                        }]
                    }
                },
					{
						targets: 2,
						 data: "payGroupMent.groupId",
						 name: "groupId"   
					},
					{
						targets: 3,
						 data: "payGroupMent.payMentId",
						 name: "payMentId"   
					},
					{
						targets: 4,
						 data: "payGroupMent.status",
						 name: "status"  ,
						render: function (status) {
							if (status) {
								return '<div class="text-center"><i class="fa fa-check text-success" title="True"></i></div>';
							}
							return '<div class="text-center"><i class="fa fa-times-circle" title="False"></i></div>';
					}
			 
					}
            ]
        });

        function getPayGroupMents() {
            dataTable.ajax.reload();
        }

        function deletePayGroupMent(payGroupMent) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _payGroupMentsService.delete({
                            id: payGroupMent.id
                        }).done(function () {
                            getPayGroupMents(true);
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

        $('#CreateNewPayGroupMentButton').click(function () {
            _createOrEditModal.open();
        });        

		

        abp.event.on('app.createOrEditPayGroupMentModalSaved', function () {
            getPayGroupMents();
        });

		$('#GetPayGroupMentsButton').click(function (e) {
            e.preventDefault();
            getPayGroupMents();
        });

		$(document).keypress(function(e) {
		  if(e.which === 13) {
			getPayGroupMents();
		  }
		});

        $('.reload-on-change').change(function(e) {
			getPayGroupMents();
		});

        $('.reload-on-keyup').keyup(function(e) {
			getPayGroupMents();
		});

        $('#btn-reset-filters').click(function (e) {
            $('.reload-on-change,.reload-on-keyup,#MyEntsTableFilter').val('');
            getPayGroupMents();
        });
		
		
		

    });
})();
