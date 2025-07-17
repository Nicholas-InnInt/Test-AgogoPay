(function () {
    $(function () {
        var _$withdrawalDevicesTable = $('#WithdrawalDevicesTable');
        var _withdrawalDevicesService = abp.services.app.withdrawalDevices;
        let _suppressEvents = false;

        var $selectedDate = {
            startDate: null,
            endDate: null,
        }

        $('.select2').select2({
            placeholder: app.localize('All'),
            theme: 'bootstrap5',
            width: "100%",
            allowClear: true,
            selectionCssClass: 'form-select',
            language: abp.localization.currentCulture.name,
        });

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
                getWithdrawalDevices();
            })
            .on('cancel.daterangepicker', function (ev, picker) {
                $(this).val("");
                $selectedDate.startDate = null;
                getWithdrawalDevices();
            });

        $('.endDate').daterangepicker({
            autoUpdateInput: false,
            singleDatePicker: true,
            locale: abp.localization.currentLanguage.name,
            format: 'L',
        })
            .on("apply.daterangepicker", (ev, picker) => {
                $selectedDate.endDate = picker.startDate;
                getWithdrawalDevices();
            })
            .on('cancel.daterangepicker', function (ev, picker) {
                $(this).val("");
                $selectedDate.endDate = null;
                getWithdrawalDevices();
            });

        var _permissions = {
            create: abp.auth.hasPermission('Pages.WithdrawalDevices.Create'),
            edit: abp.auth.hasPermission('Pages.WithdrawalDevices.Edit'),
            childEdit: abp.auth.hasPermission('Pages.WithdrawalDevices.ChildEdit'),
            'delete': abp.auth.hasPermission('Pages.WithdrawalDevices.Delete')
        };

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/WithdrawalDevices/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/WithdrawalDevices/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditWithdrawalDeviceModal'
        });

        var _batchPauseWithdrawalDeviceModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/WithdrawalDevices/BatchPauseWithdrawalDeviceModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/WithdrawalDevices/_BatchPauseWithdrawalDeviceModal.js',
            modalClass: 'BatchPauseWithdrawalDeviceModal'
        });

        var _viewWithdrawalDeviceModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/WithdrawalDevices/ViewwithdrawalDeviceModal',
            modalClass: 'ViewWithdrawalDeviceModal'
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

        var dataTable = _$withdrawalDevicesTable.DataTable({
            paging: true,
            serverSide: true,
            processing: true,
            ordering: false,
            listAction: {
                ajaxFunction: _withdrawalDevicesService.getAll,
                inputFilter: function () {
                    return {
                        filter: $('#WithdrawalDevicesTableFilter').val(),
                        //merchantCodeFilter: $('#MerchantCodeFilterId').val(),
                        nameFilter: $('#NameFilterId').val(),
                        phoneFilter: $('#PhoneFilterId').val(),
                        bankTypeFilter: $('#BankTypeFilterId').val(),
                        withdrawProcessFilter: $('#WithdrawProcessFilterId').val()
                    };
                },
                done: getMerchants()
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
                                    _viewWithdrawalDeviceModal.open({ id: data.record.withdrawalDevice.id });
                                }
                            },
                            {
                                text: app.localize('Edit'),
                                visible: function () {
                                    return _permissions.edit;
                                },
                                action: function (data) {
                                    _createOrEditModal.open({ id: data.record.withdrawalDevice.id });
                                }
                            },
                            //{
                            //    text: function (data) {
                            //        if (data.withdrawalDevice.status) {
                            //            return app.localize('Close');
                            //        } else {
                            //            return app.localize('Open');
                            //        }
                            //    },
                            //    iconStyle: 'far fa-edit mr-2',
                            //    visible: function () {
                            //        return _permissions.childEdit;
                            //    },
                            //    action: function (data) {
                            //        openWithdrawalDevice(data.record.withdrawalDevice);
                            //    }
                            //},
                            {
                                text: function (data) {
                                    if (data.withdrawalDevice.process == 0) {
                                        return app.localize('Enum_WithdrawalDevicesProcessTypeEnum_1')
                                    } else {
                                        return app.localize('Enum_WithdrawalDevicesProcessTypeEnum_0')
                                    }
                                },
                                iconStyle: 'far fa-edit mr-2',
                                visible: function () {
                                    return _permissions.childEdit;
                                },
                                action: function (data) {
                                    stopWithdrawalDevice(data.record.withdrawalDevice);
                                }
                            },
                            {
                                text: app.localize('Delete'),
                                visible: function () {
                                    return _permissions.delete;
                                },
                                action: function (data) {
                                    deleteWithdrawalDevice(data.record.withdrawalDevice);
                                }
                            }]
                    }
                },
                {
                    targets: 2,
                    data: "withdrawalDevice.merchantCode",
                    name: "merchantCode",
                    render: function (data, type, full, meta) {
                        if (full.withdrawalDevice.merchantCode == null || full.withdrawalDevice.merchantCode == "NsPay") {
                            return "<span>" + app.localize('MerchantName') + "\uff1a" + "NsPay</span><br/>" +
                                "<span>" + app.localize('MerchantCode') + "\uff1a" + "NsPay</span><br/>";
                        } else {
                            return "<span>" + app.localize('MerchantName') + "\uff1a" + full.merchantName + "</span><br/>" +
                                "<span>" + app.localize('MerchantCode') + "\uff1a" + full.withdrawalDevice.merchantCode + "</span><br/>";
                        }
                    }
                },
                {
                    targets: 3,
                    data: "withdrawalDevice.name",
                    name: "name",
                    render: function (data, type, full, meta) {
                        var maxLength = 20; // Define max length for truncation
                        var name = full.withdrawalDevice ? full.withdrawalDevice.name : "";

                        if (!name) {
                            return "<span>" + app.localize('Name') + ": N/A</span><br/>";
                        }

                        if (name.length > maxLength) {
                            var shortName = name.substring(0, maxLength) + '...';
                            return "<span>" + app.localize('Name') + ": <span class='short-name'>" + shortName + "</span>" +
                                "<span class='full-name' style='display:none;'>" + name + "</span>" +
                                " <a href='#' class='see-more' onclick='toggleName(this)'>" + app.localize('SeeMore') + "</a></span><br/>" +
                                "<span>" + app.localize('WithdrawBankType') + ": " + app.localize('Enum_WithdrawalDevicesBankTypeEnum_' + full.withdrawalDevice.bankType) + "</span><br/>" +
                                "<span>" + app.localize('Money') + ": " + full.withdrawalDevice.minMoney + "-" + full.withdrawalDevice.maxMoney + "</span>";
                        } else {
                            return "<span>" + app.localize('Name') + ": " + name + "</span><br/>" +
                                "<span>" + app.localize('WithdrawBankType') + ": " + app.localize('Enum_WithdrawalDevicesBankTypeEnum_' + full.withdrawalDevice.bankType) + "</span><br/>" +
                                "<span>" + app.localize('Money') + ": " + full.withdrawalDevice.minMoney + "-" + full.withdrawalDevice.maxMoney + "</span>";
                        }
                    }
                },
                {
                    targets: 4,
                    data: "withdrawalDevice.phone",
                    name: "phone",
                    render: function (data, type, full, meta) {
                        return "<span>" + app.localize('Phone') + "\uff1a" + full.withdrawalDevice.phone + "</span><br/>" +
                            "<span>" + app.localize('WithdrawCardName') + "\uff1a" + full.withdrawalDevice.cardName + "</span><br/>";
                    }
                },
                {
                    targets: 5,
                    data: "withdrawalDevice.status",
                    name: "status",
                    render: function (data, type, full, meta) {
                        var processStatus = {
                            0: {
                                'title': app.localize('ScoreStatusSuccess'),
                                'class': ' badge-light-success'
                            },
                            1: {
                                'title': app.localize('ScoreStatusFail'),
                                'class': ' badge-light-danger'
                            },
                        };
                        //var html = "";
                        //if (full.withdrawalDevice.status) {
                        //    html = "<span>" + app.localize('WithdrawStatus') + "\uff1a<span class='badge label-lg font-weight-bold badge-light-success  label-inline'>" + app.localize('Open') + "</span></span><br/>";
                        //} else {
                        //    html = "<span>" + app.localize('WithdrawStatus') + "\uff1a<span class='badge label-lg font-weight-bold badge-light-danger  label-inline'>" + app.localize('Close') + "</span></span><br/>";
                        //}
                        //return html +
                        var html = "<span>" + app.localize('AvailableBalance') + "\uff1a<span class='badge label-lg font-weight-bold  label-inline'>" + full.withdrawalDevice.balance + "</span></span><br/>";
                        return html + "<span>" + app.localize('WithdrawProcess') + "\uff1a<span class='badge label-lg font-weight-bold" + processStatus[full.withdrawalDevice.process].class + " label-inline'>" + app.localize('Enum_WithdrawalDevicesProcessTypeEnum_' + full.withdrawalDevice.process) + "</span></span>";
                    }
                }
            ]
        });

        function getMerchants() {
            _withdrawalDevicesService.getMerchants({
            }).done(function (result) {
                $("#MerchantCodeFilterId").empty();
                $("#MerchantCodeFilterId").append("<option selected value=''>" + app.localize('All') + "</option>");
                for (var i = 0; i < result.length; i++) {
                    $("#MerchantCodeFilterId").append("<option value='" + result[i].merchantCode + "'>" + result[i].name + "[" + result[i].merchantCode + "]" + "</option>");
                }
            });
        }

        function getWithdrawalDevices() {
            dataTable.ajax.reload();
        }

        function openWithdrawalDevice(withdrawalDevice) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _withdrawalDevicesService.editStatus({
                            id: withdrawalDevice.id
                        }).done(function () {
                            getWithdrawalDevices(true);
                            abp.notify.success(app.localize('SavedSuccessfully'));
                        });
                    }
                }
            );
        }

        function stopWithdrawalDevice(withdrawalDevice) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _withdrawalDevicesService.editProcess({
                            id: withdrawalDevice.id
                        }).done(function () {
                            getWithdrawalDevices(true);
                            abp.notify.success(app.localize('SavedSuccessfully'));
                        });
                    }
                }
            );
        }

        function deleteWithdrawalDevice(withdrawalDevice) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _withdrawalDevicesService.delete({
                            id: withdrawalDevice.id
                        }).done(function () {
                            getWithdrawalDevices(true);
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

        $('#CreateNewWithdrawalDeviceButton').click(function () {
            _createOrEditModal.open();
        });

        $('#BatchPauseWithdrawalDeviceButton').click(function () {
            _batchPauseWithdrawalDeviceModal.open();
        });

        abp.event.on('app.createOrEditWithdrawalDeviceModalSaved', function () {
            getWithdrawalDevices();
        });

        $('#GetWithdrawalDevicesButton').click(function (e) {
            e.preventDefault();
            getWithdrawalDevices();
        });

        $(document).keypress(function (e) {
            if (e.which === 13) {
                getWithdrawalDevices();
            }
        });

        $('.reload-on-change').change(function (e) {
            if (_suppressEvents) return;
            getWithdrawalDevices();
        });

        $('.reload-on-keyup').keyup(function (e) {
            getWithdrawalDevices();
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

            _suppressEvents = false;
            getWithdrawalDevices();
        });

        $(document).on('click', '.see-more', function (e) {
            e.preventDefault();
            var parent = $(this).closest('span');
            var shortName = parent.find('.short-name');
            var fullName = parent.find('.full-name');

            if (shortName.is(':visible')) {
                shortName.hide();
                fullName.show();
                $(this).text(app.localize('SeeLess'));
            } else {
                shortName.show();
                fullName.hide();
                $(this).text(app.localize('SeeMore'));
            }
        });
    });
})();