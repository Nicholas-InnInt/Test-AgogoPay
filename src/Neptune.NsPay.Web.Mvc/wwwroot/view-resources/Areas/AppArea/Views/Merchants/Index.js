(function () {
    $(function () {
        var _$merchantsTable = $('#MerchantsTable');
        var _merchantsService = abp.services.app.merchants;

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
        }).on("apply.daterangepicker", (ev, picker) => {
            $selectedDate.startDate = picker.startDate;
            getMerchants();
        }).on('cancel.daterangepicker', function (ev, picker) {
            $(this).val("");
            $selectedDate.startDate = null;
            getMerchants();
        });

        $('.endDate').daterangepicker({
            autoUpdateInput: false,
            singleDatePicker: true,
            locale: abp.localization.currentLanguage.name,
            format: 'L',
        }).on("apply.daterangepicker", (ev, picker) => {
            $selectedDate.endDate = picker.startDate;
            getMerchants();
        }).on('cancel.daterangepicker', function (ev, picker) {
            $(this).val("");
            $selectedDate.endDate = null;
            getMerchants();
        });

        var _permissions = {
            create: abp.auth.hasPermission('Pages.Merchants.Create'),
            edit: abp.auth.hasPermission('Pages.Merchants.Edit'),
            'delete': abp.auth.hasPermission('Pages.Merchants.Delete'),
            recalBalance: abp.auth.hasPermission('Pages.Merchants.Recal_LockBalance'),
        };

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/Merchants/CreateOrEditModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/Merchants/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditMerchantModal'
        });

        var _viewMerchantModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/Merchants/ViewmerchantModal',
            modalClass: 'ViewMerchantModal'
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

        var dataTable = _$merchantsTable.DataTable({
            paging: true,
            serverSide: true,
            processing: true,
            listAction: {
                ajaxFunction: _merchantsService.getAll,
                inputFilter: function () {
                    return {
                        filter: $('#MerchantsTableFilter').val(),
                        nameFilter: $('#NameFilterId').val(),
                        merchantCodeFilter: $('#MerchantCodeFilterId').val(),
                        platformCodeFilter: $('#PlatformCodeFilterId').val()
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
                        text: `<i class="fa fa-cog"></i> ${app.localize('Actions')} <span class="caret"></span>`,
                        items: [
                            //{
                            //                          text: app.localize('View'),
                            //                          action: function (data) {
                            //                              _viewMerchantModal.open({ id: data.record.merchant.id });
                            //    }
                            //},
                            {
                                text: app.localize('Edit'),
                                visible: function () {
                                    return _permissions.edit;
                                },
                                action: function (data) {
                                    _createOrEditModal.open({ id: data.record.merchant.id });
                                }
                            },
                            {
                                text: app.localize('Delete'),
                                visible: function () {
                                    return _permissions.delete;
                                },
                                action: function (data) {
                                    deleteMerchant(data.record.merchant);
                                }
                            },
                            {
                                text: app.localize('RecalculateLockedBalance'),
                                visible: function () {
                                    return _permissions.recalBalance;
                                },
                                action: function (data) {
                                    resetLockedBalance(data.record.merchant);
                                }
                            }]
                    }
                },
                {
                    targets: 2,
                    data: "merchant.name",
                    name: "name",
                    render: function (data, type, full, meta) {
                        return `<span>${app.localize('MerchantName')}:${full.merchant.name}</span><br/>` +
                            `<span>${app.localize('PlatformCode')}:${full.merchant.platformCode}</span><br/>`;
                    }
                },
                {
                    targets: 3,
                    data: "merchant.merchantCode",
                    name: "merchantCode",
                    render: function (data, type, full, meta) {
                        return `<span>${app.localize('MerchantCode')}:${full.merchant.merchantCode}</span><br/>` +
                            `<span>${app.localize('MerchantSecret')}:${full.merchant.merchantSecret}</span><br/>`;
                    }
                },
                {
                    targets: 4,
                    orderable: false,
                    data: "merchant.rate",
                    name: "rate",
                    render: function (data, type, full, meta) {
                        return `<span>${app.localize('ScanRate')}:${full.merchantRate.scanBankRate}</span><br/>` +
                            `<span>${app.localize('MoMoRate')}:${full.merchantRate.moMoRate}</span><br/>` +
                            `<span>${app.localize('ScratchCardRate')}:${full.merchantRate.scratchCardRate}</span><br/>` +
                            `<span>${app.localize('USDTFixedFees')}:${full.merchantRate.usdtFixedFees}</span><br/>` +
                            `<span>${app.localize('USDTRateFees')}:${full.merchantRate.usdtRateFees}</span><br/>`;
                    }
                },
                {
                    targets: 5,
                    data: "merchant.remark",
                    name: "remark",
                    render: function (data, type, full, meta) {
                        return `<span>${app.localize('Balance')}:${full.merchantFund.balance}</span><br/>` +
                            `<span>${app.localize('LockedBalance')}:${full.merchantFund.frozenBalance}</span><br/>` +
                            `<span>${app.localize('DepositAmount')}:${full.merchantFund.depositAmount}</span><br/>` +
                            `<span>${app.localize('WithdrawalAmount')}:${full.merchantFund.withdrawalAmount}</span><br/>` +
                            `<span>${app.localize('RateFeeBalance')}:${full.merchantFund.rateFeeBalance}</span><br/>`;
                    }
                },
                {
                    targets: 6,
                    data: "merchant.payGroupId",
                    name: "payGroupId",
                    render: function (data, type, full, meta) {
                        return `<span>${app.localize('PayGroupId')}:${full.merchant.payGroupName}</span><br/>` +
                            `<span>${app.localize('CountryType')}:${full.merchant.countryType}</span><br/>`;
                    }
                }
            ]
        });

        function getMerchants() {
            dataTable.ajax.reload();
        }

        function deleteMerchant(merchant) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _merchantsService.delete({
                            id: merchant.id
                        }).done(function () {
                            getMerchants(true);
                            abp.notify.success(app.localize('SuccessfullyDeleted'));
                        });
                    }
                }
            );
        }

        function resetLockedBalance(merchant) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _merchantsService.resetLoackedBalance({
                            id: merchant.id
                        }).done(function (result) {

                            if (result) {
                                getMerchants(true);
                                abp.notify.success(app.localize('SuccessfullyDeleted'));
                            }
                            else {
                                abp.notify.info(app.localize('Updatefailed'));
                            }

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

        $('#CreateNewMerchantButton').click(function () {
            _createOrEditModal.open();
        });



        abp.event.on('app.createOrEditMerchantModalSaved', function () {
            getMerchants();
        });

        $('#GetMerchantsButton').click(function (e) {
            e.preventDefault();
            getMerchants();
        });

        $(document).keypress(function (e) {
            if (e.which === 13) {
                getMerchants();
            }
        });

        $('.reload-on-change').change(function (e) {
            getMerchants();
        });

        $('.reload-on-keyup').keyup(function (e) {
            getMerchants();
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

            getMerchants();
        });




    });
})();