(function () {
    $(function () {

        var _$nsPayBackgroundJobsTable = $('#NsPayBackgroundJobsTable');
        var _nsPayBackgroundJobsService = abp.services.app.nsPayBackgroundJobs;
		
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
            getNsPayBackgroundJobs();
        })
        .on('cancel.daterangepicker', function (ev, picker) {
            $(this).val("");
            $selectedDate.startDate = null;
            getNsPayBackgroundJobs();
        });

        $('.endDate').daterangepicker({
            autoUpdateInput: false,
            singleDatePicker: true,
            locale: abp.localization.currentLanguage.name,
            format: 'L',
        })
        .on("apply.daterangepicker", (ev, picker) => {
            $selectedDate.endDate = picker.startDate;
            getNsPayBackgroundJobs();
        })
        .on('cancel.daterangepicker', function (ev, picker) {
            $(this).val("");
            $selectedDate.endDate = null;
            getNsPayBackgroundJobs();
        });

        var _permissions = {
            create: abp.auth.hasPermission('Pages.NsPayBackgroundJobs.Create'),
            edit: abp.auth.hasPermission('Pages.NsPayBackgroundJobs.Edit'),
            'delete': abp.auth.hasPermission('Pages.NsPayBackgroundJobs.Delete')
        };

         var _createOrEditModal = new app.ModalManager({
                    viewUrl: abp.appPath + 'AppArea/NsPayBackgroundJobs/CreateOrEditModal',
                    scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/NsPayBackgroundJobs/_CreateOrEditModal.js',
                    modalClass: 'CreateOrEditNsPayBackgroundJobModal'
                });
                   

		 var _viewNsPayBackgroundJobModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/NsPayBackgroundJobs/ViewnsPayBackgroundJobModal',
            modalClass: 'ViewNsPayBackgroundJobModal'
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

        var dataTable = _$nsPayBackgroundJobsTable.DataTable({
            paging: true,
            serverSide: true,
            processing: true,
            listAction: {
                ajaxFunction: _nsPayBackgroundJobsService.getAll,
                inputFilter: function () {
                    return {
					filter: $('#NsPayBackgroundJobsTableFilter').val(),
					nameFilter: $('#NameFilterId').val(),
					cronFilter: $('#CronFilterId').val(),
					apiUrlFilter: $('#ApiUrlFilterId').val(),
					requsetModeFilter: $('#RequsetModeFilterId').val(),
					stateFilter: $('#StateFilterId').val(),
					paramDataFilter: $('#ParamDataFilterId').val(),
					merchantCodeFilter: $('#MerchantCodeFilterId').val(),
					descriptionFilter: $('#DescriptionFilterId').val(),
					remarkFilter: $('#RemarkFilterId').val()
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
                                    _viewNsPayBackgroundJobModal.open({ id: data.record.nsPayBackgroundJob.id });
                                }
                        },
						{
                            text: app.localize('Edit'),
                            visible: function () {
                                return _permissions.edit;
                            },
                            action: function (data) {
                            _createOrEditModal.open({ id: data.record.nsPayBackgroundJob.id });                                
                            }
                            }, 
                            {
                                text: app.localize('Pause'),
                                visible: function () {
                                    return _permissions.edit;
                                },
                                action: function (data) {
                                    pauseNsPayBackgroundJob(data.record.nsPayBackgroundJob);
                                }
                            },
                            {
                                text: app.localize('Restart'),
                                visible: function () {
                                    return _permissions.edit;
                                },
                                action: function (data) {
                                    restartNsPayBackgroundJob(data.record.nsPayBackgroundJob);
                                }
                            },
						{
                            text: app.localize('Delete'),
                            visible: function () {
                                return _permissions.delete;
                            },
                            action: function (data) {
                                deleteNsPayBackgroundJob(data.record.nsPayBackgroundJob);
                            }
                        }]
                    }
                },
					{
						targets: 2,
						 data: "nsPayBackgroundJob.name",
						 name: "name"   
					},
					{
						targets: 3,
						 data: "nsPayBackgroundJob.groupName",
						 name: "groupName"   
					},
					{
						targets: 4,
						 data: "nsPayBackgroundJob.cron",
						 name: "cron"   
					},
					{
						targets: 5,
						 data: "nsPayBackgroundJob.apiUrl",
						 name: "apiUrl"   
					},
					{
						targets: 6,
						 data: "nsPayBackgroundJob.requsetMode",
						 name: "requsetMode"   ,
						render: function (requsetMode) {
							return app.localize('Enum_NsPayBackgroundJobRequsetModeEnum_' + requsetMode);
						}
			
					},
					{
						targets: 7,
						 data: "nsPayBackgroundJob.state",
						 name: "state"   ,
						render: function (state) {
							return app.localize('Enum_NsPayBackgroundJobStateEnum_' + state);
						}
			
					},
					{
						targets: 8,
						 data: "nsPayBackgroundJob.paramData",
						 name: "paramData"   
					},
					{
						targets: 9,
						 data: "nsPayBackgroundJob.merchantCode",
						 name: "merchantCode"   
					},
					{
						targets: 10,
						 data: "nsPayBackgroundJob.description",
						 name: "description"   
					},
					{
						targets: 11,
						 data: "nsPayBackgroundJob.remark",
						 name: "remark"   
					}
            ]
        });

        function getNsPayBackgroundJobs() {
            dataTable.ajax.reload();
        }

        function deleteNsPayBackgroundJob(nsPayBackgroundJob) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _nsPayBackgroundJobsService.delete({
                            id: nsPayBackgroundJob.id
                        }).done(function () {
                            getNsPayBackgroundJobs(true);
                            abp.notify.success(app.localize('SuccessfullyDeleted'));
                        });
                    }
                }
            );
        }

        function pauseNsPayBackgroundJob(nsPayBackgroundJob) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _nsPayBackgroundJobsService.pause({
                            id: nsPayBackgroundJob.id
                        }).done(function () {
                            abp.notify.success(app.localize('SuccessfullyPaused'));
                        });
                    }
                }
            );
        }

        function restartNsPayBackgroundJob(nsPayBackgroundJob) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _nsPayBackgroundJobsService.restart({
                            id: nsPayBackgroundJob.id
                        }).done(function () {
                            abp.notify.success(app.localize('SuccessfullyRestart'));
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

        $('#CreateNewNsPayBackgroundJobButton').click(function () {
            _createOrEditModal.open();
        });        

		

        abp.event.on('app.createOrEditNsPayBackgroundJobModalSaved', function () {
            getNsPayBackgroundJobs();
        });

		$('#GetNsPayBackgroundJobsButton').click(function (e) {
            e.preventDefault();
            getNsPayBackgroundJobs();
        });

		$(document).keypress(function(e) {
		  if(e.which === 13) {
			getNsPayBackgroundJobs();
		  }
		});

        $('.reload-on-change').change(function(e) {
			getNsPayBackgroundJobs();
		});

        $('.reload-on-keyup').keyup(function(e) {
			getNsPayBackgroundJobs();
		});

        $('#btn-reset-filters').click(function (e) {
            $('.reload-on-change,.reload-on-keyup,#MyEntsTableFilter').val('');

            $('select.reload-on-change').each(function (index, element) {

                var firstSelectionValue = $(element).find("option:first").val();

                if ($(element).hasClass("select2")) {
                    $(element).val('-1').trigger('change');

                }
                else {
                    $(element).val(firstSelectionValue);// drop  downlist select first option
                }

            });

            getNsPayBackgroundJobs();
        });
		
		
		

    });
})();
