(function () {
    $(function () {

        var _$payMentsTable = $('#PayMentsTable');
        var _payMentsService = abp.services.app.payMents;
        let _suppressEvents = false;



        $('.select2').select2({
            placeholder: app.localize('Select'),
            theme: 'bootstrap5',
            width: "100%",
            allowClear: true,
            selectionCssClass: 'form-select',
            language: abp.localization.currentCulture.name,
        });

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
                getPayMents();
            })
            .on('cancel.daterangepicker', function (ev, picker) {
                $(this).val("");
                $selectedDate.startDate = null;
                getPayMents();
            });

        $('.endDate').daterangepicker({
            autoUpdateInput: false,
            singleDatePicker: true,
            locale: abp.localization.currentLanguage.name,
            format: 'L',
        })
            .on("apply.daterangepicker", (ev, picker) => {
                $selectedDate.endDate = picker.startDate;
                getPayMents();
            })
            .on('cancel.daterangepicker', function (ev, picker) {
                $(this).val("");
                $selectedDate.endDate = null;
                getPayMents();
            });

        var _permissions = {
            create: abp.auth.hasPermission('Pages.PayMents.Create'),
            edit: abp.auth.hasPermission('Pages.PayMents.Edit'),
            editStatus: abp.auth.hasPermission('Pages.PayMents.EditStatus'),
            childEdit: abp.auth.hasPermission('Pages.PayMents.ChildEdit'),
            openJob: abp.auth.hasPermission('Pages.PayMents.OpenJob'),
            'delete': abp.auth.hasPermission('Pages.PayMents.Delete'),
            getHistory: abp.auth.hasPermission('Pages.PayMents.GetHistory'),
        };

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/PayMents/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/PayMents/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditPayMentModal'
        });

        var _editPayMentModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/PayMents/EditPayMentModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/PayMents/_EditPayMentModal.js',
            modalClass: 'CreateOrEditPayMentModal'
        });

        var _viewPayMentModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/PayMents/ViewpayMentModal',
            modalClass: 'ViewPayMentModal'
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

        var dataTable = _$payMentsTable.DataTable({
            paging: true,
            serverSide: true,
            processing: true,
            listAction: {
                ajaxFunction: _payMentsService.getAll,
                inputFilter: function () {
                    return {
                        filter: $('#PayMentsTableFilter').val(),
                        nameFilter: $('#NameFilterId').val(),
                        typeFilter: $('#TypeFilterId').val(),
                        phoneFilter: $('#PhoneFilterId').val(),
                        showStatusFilter: $('#ShowStatusFilterId').val(),
                        useMoMoFilter: $('#UseMoMoFilterId').val(),
                        payMentStatusFilter: $('#PayMentStatusFilterId').val(),
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
                            //    text: app.localize('View'),
                            //    action: function (data) {
                            //        _viewPayMentModal.open({ id: data.record.payMent.id });
                            //    }
                            //},
                            {
                                text: app.localize('Edit'),
                                visible: function () {
                                    return _permissions.edit;
                                },
                                action: function (data) {
                                    _createOrEditModal.open({ id: data.record.payMent.id });
                                }
                            },
                            {
                                text: app.localize('Edit'),
                                visible: function () {
                                    return _permissions.childEdit;
                                },
                                action: function (data) {
                                    _editPayMentModal.open({ id: data.record.payMent.id });
                                }
                            },
                            {
                                text: app.localize('OpenJob'),
                                visible: function () {
                                    return _permissions.openJob;
                                },
                                action: function (data) {
                                    openJobPayMent(data.record.payMent);
                                }
                            },
                            {
                                text: function (data) {
                                    if (data.payMent.status == 0) {
                                        return app.localize('HidePayMent')
                                    } else {
                                        return app.localize('ShowPayMent')
                                    }
                                },
                                visible: function () {
                                    return _permissions.editStatus;
                                },
                                action: function (data) {
                                    hidePayMent(data.record.payMent);
                                }
                            },
                            {
                                text: app.localize('Delete'),
                                visible: function () {
                                    return _permissions.delete;
                                },
                                action: function (data) {
                                    deletePayMent(data.record.payMent);
                                }
                            },
                            //{
                            //    text: app.localize('GetHistory'),
                            //    visible: function (data) {
                            //        var visible = _permissions.getHistory;

                            //        if (visible) {
                            //            var bankList = [1, 3, 4, 5, 6, 7, 12, 13, 14, 15]
                            //            if (!bankList.includes(data.record.payMent.type))
                            //            {
                            //                visible = false;
                            //            }
                            //        }

                            //        return visible;
                            //    },
                            //    action: function (data) {
                            //        GetPayMentHistory(data.record.payMent);
                            //    }
                            //},
                        ]
                    }
                },
                {
                    targets: 2,
                    data: "payMent.name",
                    name: "name",
                    render: function (data, type, full, meta) {
                        return "<span>" + app.localize('PayMentName') + ":" + full.payMent.name + "</span><br/>" +
                            "<span>" + app.localize('PayMentType') + ":" + app.localize('Enum_PayMentTypeEnum_' + full.payMent.type) + "</span><br/>";
                    }
                },
                {
                    targets: 3,
                    data: "payMent.phone",
                    name: "phone",
                    render: function (data, type, full, meta) {
                        const cryptoType = [1001, 1002];
                        if (cryptoType.some(x => x == full.payMent.type)) {
                            return `<span>${app.localize('CryptoWalletAddress')}: ${full.payMent.cryptoWalletAddress ?? 'N/A'}</span>`;
                        }

                        return "<span>" + app.localize('LoginAccount') + ":" + full.payMent.phone + "</span><br/>" + //db column is phone
                            "<span>" + app.localize('CardNumber') + ":" + full.payMent.cardNumber + "</span><br/>" +
                            "<span>" + app.localize('FullName') + ":" + full.payMent.fullName + "</span><br/>";
                    }
                },
                {
                    targets: 4,
                    data: "payMent.minMoney",
                    name: "minMoney",
                    render: function (data, type, full, meta) {
                        return "<span>" + app.localize('MinMoney') + ":" + full.payMent.minMoney + "</span><br/>" +
                            "<span>" + app.localize('MaxMoney') + ":" + full.payMent.maxMoney + "</span><br/>";
                    }
                },
                {
                    targets: 5,
                    data: "payMent.limitMoney",
                    name: "limitMoney",
                    render: function (data, type, full, meta) {
                        return "<span>" + app.localize('LimitMoney') + ":" + full.payMent.limitMoney + "</span><br/>" +
                            "<span>" + app.localize('BalanceLimitMoney') + ":" + full.payMent.balanceLimitMoney + "</span><br/>";
                    }
                },
                {
                    targets: 6,
                    data: "payMent.status",
                    name: "status",
                    render: function (data, type, full, meta) {
                        var paymentHtml = "";
                        if (full.payMent.payMentStatus) {
                            paymentHtml = "<span class='badge label-lg font-weight-bold  badge-light-success'>" + app.localize('PayMentStatusOpen') + "</span>";
                        } else {
                            paymentHtml = "<span class='badge label-lg font-weight-bold  badge-light-danger'>" + app.localize('PayMentStatusClose') + "</span>";
                        }

                        var statusHtml = "";
                        if (full.payMent.status == 0) {
                            statusHtml = "<span class='badge label-lg font-weight-bold  badge-light-success'>"+app.localize('ShowPayMent') + "</span>";
                        } else {
                            statusHtml = "<span class='badge label-lg font-weight-bold  badge-light-danger'>" + app.localize('HidePayMent') + "</span>";
                        }

                        var usemomoHtml = "";
                        if (full.payMent.useMoMo) {
                            usemomoHtml = "<span class='badge label-lg font-weight-bold  badge-light-success'>" + app.localize('UseMoMoOpen') + "</span>";
                        } else {
                            usemomoHtml = "<span class='badge label-lg font-weight-bold  badge-light-danger'>" + app.localize('UseMoMoClose') + "</span>";
                        }
                        return "<span>" + app.localize('PayMentStatus') + ":" + paymentHtml + "</span><br/>" +
                            "<span>" + app.localize('ShowStatus') + ":" + statusHtml + "</span><br/>" +
                            "<span>" + app.localize('UseMoMo') + ":" + usemomoHtml + "</span><br/>";
                    }
                }
            ]
        });

        function getPayMents() {
            dataTable.ajax.reload();
        }

        function openJobPayMent(payMent) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _payMentsService.openJob({
                            id: payMent.id
                        }).done(function () {
                            getPayMents(true);
                            abp.notify.success(app.localize('SavedSuccessfully'));
                        });
                    }
                }
            );
        }

        function hidePayMent(payMent) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _payMentsService.hide({
                            id: payMent.id
                        }).done(function () {
                            getPayMents(true);
                            abp.notify.success(app.localize('SavedSuccessfully'));
                        });
                    }
                }
            );
        }

        function deletePayMent(payMent) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _payMentsService.delete({
                            id: payMent.id
                        }).done(function () {
                            getPayMents(true);
                            abp.notify.success(app.localize('SuccessfullyDeleted'));
                        });
                    }
                }
            );
        }

        function GetPayMentHistory(payMent) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _payMentsService.getHistory({
                            id: payMent.id
                        }).done(function () {
                            abp.notify.success(app.localize('GetHistorySuccessfully'));
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

        $('#CreateNewPayMentButton').click(function () {
            _createOrEditModal.open();
        });



        abp.event.on('app.createOrEditPayMentModalSaved', function () {
            getPayMents();
        });

        $('#GetPayMentsButton').click(function (e) {
            e.preventDefault();
            getPayMents();
        });

        $(document).keypress(function (e) {
            if (e.which === 13) {
                getPayMents();
            }
        });

        $('.reload-on-change').change(function (e) {
            if (_suppressEvents) return;
            getPayMents();
        });

        $('.reload-on-keyup').keyup(function (e) {
            getPayMents();
        });

        $('#btn-reset-filters').click(function (e) {
            _suppressEvents = true;
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

            getPayMents();
            
        });
    });
})();
