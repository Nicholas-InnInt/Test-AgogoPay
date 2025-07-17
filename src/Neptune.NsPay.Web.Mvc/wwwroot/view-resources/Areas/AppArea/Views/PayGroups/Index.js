(function () {
    $(function () {

        var _$payGroupsTable = $('#PayGroupsTable');
        var _payGroupsService = abp.services.app.payGroups;

        var _$payGroupMentsTable = $('#PayGroupMentsTable');
        var _payGroupMentsService = abp.services.app.payGroupMents;


        var _permissions = {
            create: abp.auth.hasPermission('Pages.PayGroups.Create'),
            edit: abp.auth.hasPermission('Pages.PayGroups.Edit'),
            'delete': abp.auth.hasPermission('Pages.PayGroups.Delete')
        };

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/PayGroups/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/PayGroups/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditPayGroupModal'
        });


        var _viewPayGroupModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/PayGroups/ViewpayGroupModal',
            modalClass: 'ViewPayGroupModal'
        });

        var _paygroupmentcreateOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/PayGroupMents/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/PayGroupMents/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditPayGroupMentModal'
        });



        var dataTable = _$payGroupsTable.DataTable({
            paging: true,
            serverSide: true,
            processing: true,
            select: true,
            listAction: {
                ajaxFunction: _payGroupsService.getAll,
                inputFilter: function () {
                    return {
                        filter: $('#PayGroupsTableFilter').val(),
                        groupNameFilter: $('#GroupNameFilterId').val(),
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
                                iconStyle: 'far fa-eye mr-2',
                                action: function (data) {
                                    _viewPayGroupModal.open({ id: data.record.payGroup.id });
                                }
                            },
                            {
                                text: app.localize('Edit'),
                                iconStyle: 'far fa-edit mr-2',
                                visible: function () {
                                    return _permissions.edit;
                                },
                                action: function (data) {
                                    _createOrEditModal.open({ id: data.record.payGroup.id });
                                }
                            },
                            {
                                text: app.localize('Delete'),
                                iconStyle: 'far fa-trash-alt mr-2',
                                visible: function () {
                                    return _permissions.delete;
                                },
                                action: function (data) {
                                    deletePayGroup(data.record.payGroup);
                                }
                            }]
                    }
                },
                {
                    targets: 2,
                    data: "payGroup.groupName",
                    name: "groupName"
                },
                {
                    targets: 3,
                    data: "payGroup.bankApi",
                    name: "bankApi"
                },
                {
                    targets: 4,
                    data: "payGroup.vietcomApi",
                    name: "vietcomApi"
                },
                {
                    targets: 5,
                    data: "payGroup.status",
                    name: "status",
                    render: function (status) {
                        if (status) {
                            return '<div class="text-center"><i class="fa fa-check text-success" title="True"></i></div>';
                        }
                        return '<div class="text-center"><i class="fa fa-times-circle" title="False"></i></div>';
                    }

                }
            ]
        });

        function getPayGroups() {
            dataTable.ajax.reload();
        }

        function deletePayGroup(payGroup) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _payGroupsService.delete({
                            id: payGroup.id
                        }).done(function () {
                            getPayGroups(true);
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

        $('#CreateNewPayGroupButton').click(function () {
            _createOrEditModal.open();
        });

        dataTable.on('select', function (e, dt, type, indexes) {
            if (type === 'row') {
                //dataTable[type](indexes)
                //    .nodes()
                //    .to$()
                //    .addClass('color: #000000;');
                var data = dt.row(indexes).data();
                $('#SelectedOuRightTitle').text(data.payGroup.groupName);
                $('#SelectedOuRightId').val(data.payGroup.id);
                getPayGroupMents();
            }
        });
        dataTable.on('deselect', function (e, dt, type, indexes) {
            if (type === 'row') {
                $('#SelectedOuRightTitle').text("");
                $('#SelectedOuRightId').val("0");
                getPayGroupMents();
            }
        });

        dataTable.on('draw', function () {
                $('#SelectedOuRightTitle').text("");
                $('#SelectedOuRightId').val("0");
                getPayGroupMents();
        });

        abp.event.on('app.createOrEditPayGroupModalSaved', function () {
            getPayGroups();
        });

        $('#GetPayGroupsButton').click(function (e) {
            e.preventDefault();
            getPayGroups();
        });

        $(document).keypress(function (e) {
            if (e.which === 13) {
                getPayGroups();
            }
        });
        var payGroupMentsdataTable;
        var bindPayGroupMentsdataTable = function (isClear) {

            //if isClear will reset to empty
            if (isClear) {
                payGroupMentsdataTable.destroy();
                _$payGroupMentsTable.find("tbody").empty();
            }

                payGroupMentsdataTable = _$payGroupMentsTable.DataTable({
                    paging: true,
                    serverSide: true,
                    processing: true,
                    deferLoading: 0, //Not Auto Load
                    listAction: {
                        ajaxFunction: _payGroupMentsService.getAll,
                        inputFilter: function () {
                            return {
                                filter: $('#PayGroupMentsTableFilter').val(),
                                payGroupsIdFilter: $('#SelectedOuRightId').val()
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
                                        text: function (data) { return data.payGroupMent.status?app.localize('DisableAction'):app.localize('EnableAction') } ,
                                        iconStyle: 'far fa-edit mr-2',
                                        visible: function () {
                                            return _permissions.edit;
                                        },
                                        action: function (data) {
                                            enabledPayGroupMent(data.record.payGroupMent);
                                        }
                                    },
                                    {
                                        text: app.localize('Delete'),
                                        iconStyle: 'far fa-trash-alt mr-2',
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
                            data: "payGroupMent.payMentName",
                            name: "payMentName"
                        },
                        {
                            targets: 3,
                            data: "payGroupMent.payMentType",
                            name: "type",
                            render: function (payMentType) {
                                return app.localize('Enum_PayMentTypeEnum_' + payMentType);
                            }
                        },
                        {
                            targets: 4,
                            data: "payGroupMent.status",
                            name: "status",
                            render: function (status) {
                                if (status) {
                                    return '<div class="text-center"><i class="fa fa-check text-success" title="True"></i></div>';
                                }
                                return '<div class="text-center"><i class="fa fa-times-circle" title="False"></i></div>';
                            }

                        }
                    ]
                });
        }

        bindPayGroupMentsdataTable(false);

        function getPayGroupMents() {

            if ($('#SelectedOuRightId').val() == "0") {
                bindPayGroupMentsdataTable(true);
            }
            else {
                payGroupMentsdataTable.ajax.reload();
            }
        }

        function enabledPayGroupMent(payGroupMent) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _payGroupMentsService.enabled({
                            id: payGroupMent.id
                        }).done(function () {
                            getPayGroupMents(true);
                            abp.notify.success(app.localize('SuccessfullyDeleted'));
                        });
                    }
                }
            );
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

        //$('#ShowAdvancedFiltersSpan').click(function () {
        //    $('#ShowAdvancedFiltersSpan').hide();
        //    $('#HideAdvancedFiltersSpan').show();
        //    $('#AdvacedAuditFiltersArea').slideDown();
        //});

        //$('#HideAdvancedFiltersSpan').click(function () {
        //    $('#HideAdvancedFiltersSpan').hide();
        //    $('#ShowAdvancedFiltersSpan').show();
        //    $('#AdvacedAuditFiltersArea').slideUp();
        //});

        $('#CreateNewPayGroupMentButton').click(function () {
            var id = $('#SelectedOuRightId').val();
            var text = $('#SelectedOuRightTitle').text();
            if (id == "0") {
                abp.message.warn(app.localize('PleaseSelectPaymentGroup'));
            } else {
                _paygroupmentcreateOrEditModal.open({ GroupId: id });
            }
        });



        abp.event.on('app.createOrEditPayGroupMentModalSaved', function () {
            getPayGroupMents();
        });

        $('#GetPayGroupMentsButton').click(function (e) {
            e.preventDefault();
            getPayGroupMents();
        });

        $('#btn-reset-filters').click(function (e) {
            $('input.reload-on-change,.reload-on-keyup,#PayGroupsTableFilter').val('');
            $('select.reload-on-change').each(function (index, element) {
                $(element).val($(element).find("option:first").val());// drop  downlist select first option  
            });
            getPayGroups();
        });

    });
})();
