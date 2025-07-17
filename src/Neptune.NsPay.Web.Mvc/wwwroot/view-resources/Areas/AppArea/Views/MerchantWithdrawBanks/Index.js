(function () {
    $(function () {

        var _$merchantWithdrawBanksTable = $('#MerchantWithdrawBanksTable');
        var _merchantWithdrawBanksService = abp.services.app.merchantWithdrawBanks;

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
                getMerchantWithdrawBanks();
            })
            .on('cancel.daterangepicker', function (ev, picker) {
                $(this).val("");
                $selectedDate.startDate = null;
                getMerchantWithdrawBanks();
            });

        $('.endDate').daterangepicker({
            autoUpdateInput: false,
            singleDatePicker: true,
            locale: abp.localization.currentLanguage.name,
            format: 'L',
        })
            .on("apply.daterangepicker", (ev, picker) => {
                $selectedDate.endDate = picker.startDate;
                getMerchantWithdrawBanks();
            })
            .on('cancel.daterangepicker', function (ev, picker) {
                $(this).val("");
                $selectedDate.endDate = null;
                getMerchantWithdrawBanks();
            });

        var _permissions = {
            create: abp.auth.hasPermission('Pages.MerchantWithdrawBanks.Create'),
            edit: abp.auth.hasPermission('Pages.MerchantWithdrawBanks.Edit'),
            'delete': abp.auth.hasPermission('Pages.MerchantWithdrawBanks.Delete')
        };

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/MerchantWithdrawBanks/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/MerchantWithdrawBanks/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditMerchantWithdrawBankModal'
        });


        var _viewMerchantWithdrawBankModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/MerchantWithdrawBanks/ViewmerchantWithdrawBankModal',
            modalClass: 'ViewMerchantWithdrawBankModal'
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

        var dataTable = _$merchantWithdrawBanksTable.DataTable({
            paging: true,
            serverSide: true,
            processing: true,
            listAction: {
                ajaxFunction: _merchantWithdrawBanksService.getAll,
                inputFilter: function () {
                    return {
                        filter: $('#MerchantWithdrawBanksTableFilter').val(),
                        bankNameFilter: $('#BankNameFilterId').val(),
                        receivCardFilter: $('#ReceivCardFilterId').val(),
                        receivNameFilter: $('#ReceivNameFilterId').val()
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
                                    _viewMerchantWithdrawBankModal.open({ id: data.record.merchantWithdrawBank.id });
                                }
                            },
                            {
                                text: app.localize('Edit'),
                                visible: function () {
                                    return _permissions.edit;
                                },
                                action: function (data) {
                                    _createOrEditModal.open({ id: data.record.merchantWithdrawBank.id });
                                }
                            },
                            {
                                text: function (data) {
                                    if (data.merchantWithdrawBank.status) {
                                        return app.localize('closestatus')
                                    } else {
                                        return app.localize('openstatus')
                                    }
                                },
                                iconStyle: 'far fa-edit mr-2',
                                visible: function () {
                                    return _permissions.edit;
                                },
                                action: function (data) {
                                    statusMerchantWithdrawBank(data.record.merchantWithdrawBank);
                                }
                            },
                            {
                                text: app.localize('Delete'),
                                visible: function () {
                                    return _permissions.delete;
                                },
                                action: function (data) {
                                    deleteMerchantWithdrawBank(data.record.merchantWithdrawBank);
                                }
                            }]
                    }
                },
                {
                    targets: 2,
                    data: "merchantWithdrawBank.bankName",
                    name: "bankName",
                    render: function (data, type, full, meta) {
                        var maxLength = 20; // Define max length for truncation
                        var name = full.merchantWithdrawBank ? full.merchantWithdrawBank.bankName : "";

                        if (!name) {
                            return name;
                        }

                        // If name length exceeds maxLength, show truncated version
                        if (name.length > maxLength) {
                            var shortName = name.substring(0, maxLength) + '...';
                            return "<span class='short-name'>" + shortName + "</span>" +
                                "<span class='full-name' style='display:none;'>" + name + "</span>" +
                                " <a href='#' class='see-more' onclick='toggleName(this)'>" + app.localize('SeeMore') +"</a>";
                        } else {
                            return name;
                        }
                    }
                },
                {
                    targets: 3,
                    data: "merchantWithdrawBank.receivCard",
                    name: "receivCard",
                    render: function (data, type, full, meta) {
                        var maxLength = 20; 
                        var card = full.merchantWithdrawBank ? full.merchantWithdrawBank.receivCard : "";

                        if (!card) {
                            return card;
                        }
                        if (card.length > maxLength) {
                            var shortCard = card.substring(0, maxLength) + '...';
                            return "<span class='short-name'>" + shortCard + "</span>" +
                                "<span class='full-name' style='display:none;'>" + card + "</span>" +
                                " <a href='#' class='see-more' onclick='toggleName(this)'>" + app.localize('SeeMore') +"</a>";
                        } else {
                            return card;
                        }
                    }
                },
                {
                    targets: 4,
                    data: "merchantWithdrawBank.receivName",
                    name: "receivName",
                    render: function (data, type, full, meta) {
                        var maxLength = 20; 
                        var name = full.merchantWithdrawBank ? full.merchantWithdrawBank.receivName : "";
                        if (!name) {
                            return name;
                        }
                        if (name.length > maxLength) {
                            var shortName = name.substring(0, maxLength) + '...';
                            return "<span class='short-name'>" + shortName + "</span>" +
                                "<span class='full-name' style='display:none;'>" + name + "</span>" +
                                " <a href='#' class='see-more' onclick='toggleName(this)'>" + app.localize('SeeMore') +"</a>";
                        } else {
                            return name;
                        }
                    }
                }
,
                {
                    targets: 5,
                    data: "merchantWithdrawBank.status",
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

        function getMerchantWithdrawBanks() {
            dataTable.ajax.reload();
        }

        function statusMerchantWithdrawBank(merchantWithdrawBank) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _merchantWithdrawBanksService.status({
                            id: merchantWithdrawBank.id
                        }).done(function () {
                            getMerchantWithdrawBanks(true);
                            abp.notify.success(app.localize('SavedSuccessfully'));
                        });
                    }
                }
            );
        }

        function deleteMerchantWithdrawBank(merchantWithdrawBank) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _merchantWithdrawBanksService.delete({
                            id: merchantWithdrawBank.id
                        }).done(function () {
                            getMerchantWithdrawBanks(true);
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

        $('#CreateNewMerchantWithdrawBankButton').click(function () {
            _createOrEditModal.open();
        });



        abp.event.on('app.createOrEditMerchantWithdrawBankModalSaved', function () {
            getMerchantWithdrawBanks();
        });

        $('#GetMerchantWithdrawBanksButton').click(function (e) {
            e.preventDefault();
            getMerchantWithdrawBanks();
        });

        $(document).keypress(function (e) {
            if (e.which === 13) {
                getMerchantWithdrawBanks();
            }
        });

        $('.reload-on-change').change(function (e) {
            getMerchantWithdrawBanks();
        });

        $('.reload-on-keyup').keyup(function (e) {
            getMerchantWithdrawBanks();
        });

        $('#btn-reset-filters').click(function (e) {
            $('.reload-on-change,.reload-on-keyup,#MyEntsTableFilter').val('');
            getMerchantWithdrawBanks();
        });


        $(document).on('click', '.see-more', function (e) {
            e.preventDefault();
            var parent = $(this).closest('td'); // Get the parent <td> element (where both the short and full name are located)
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