(function () {
    $(function () {

        var _$recipientBAcc = $('#RecipientBAccTable');
        var _recipientBankAccountsService = abp.services.app.recipientBankAccounts;

        var _permissions = {
            edit: abp.auth.hasPermission('Pages.RecipientBankAccounts.Edit'),
            'delete': abp.auth.hasPermission('Pages.RecipientBankAccounts.Delete')
        };

        var dataTable = _$recipientBAcc.DataTable({
            paging: true,
            serverSide: true,
            processing: true,
            ordering: false,
            listAction: {
                ajaxFunction: _recipientBankAccountsService.getAll,
                inputFilter: function () {
                    return {
                        filter: $('#RecipientTableFilter').val(),
                        holderName: $('#NameFilterId').val(),
                        accountNumber: $('#AccountNumberId').val(),
                        bankCode: $('#BankCodeId').val()
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
                    data: "actions",
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    rowAction: {
                        cssClass: 'btn btn-brand dropdown-toggle',
                        text: '<i class="fa fa-cog"></i> ' + app.localize('Actions') + ' <span class="caret"></span>',
                        items: [
                            {
                                text: app.localize('Edit'),
                                visible: function () {
                                    if (_permissions.edit) {
                                        return true;
                                    } else {
                                        return false;
                                    }
                                },
                                action: function (data) {
                                    _createOrEditModal.open({ id: data.record.recipientBankAccounts.id });
                                }
                            },
                            {
                                text: app.localize('Delete'),
                                visible: function () {
                                    return _permissions.delete;
                                },
                                action: function (data) {
                                    deleteRecipient(data.record.recipientBankAccounts);
                                }
                            }
                        ]
                    }
                },
                {
                    targets: 2,
                    orderable: false,
                    data: "recipientBankAccounts.merchantCode",
                    name: "merchantCode"
                },
                {
                    targets: 3,
                    orderable: false,
                    data: "recipientBankAccounts.holderName",
                    name: "holderName"
                },
                {
                    targets: 4,
                    orderable: false,
                    data: "recipientBankAccounts.accountNumber",
                    name: "accountNumber"
                },
                {
                    targets: 5,
                    orderable: false,
                    data: "recipientBankAccounts.bankCode",
                    name: "bankCode"
                },
                {
                    targets: 6,
                    orderable: false,
                    data: "recipientBankAccounts.bankKey",
                    name: "bankKey"
                },
                {
                    targets: 7,
                    orderable: false,
                    data: "recipientBankAccounts.createdBy",
                    name: "createdBy"
                }
             
            ]
        });


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

        $('#GetRecipientButton').click(function (e) {
            e.preventDefault();
            getRecipients();
        });

        function getRecipients() {
            dataTable.ajax.reload();
        }


        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/RecipientBankAccounts/CreateOrEditRecipientBankAccountModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/RecipientBankAccounts/_CreateOrEditModal.js',
            modalClass: 'CreateOrEditPayOrderModal'
        });


        abp.event.on('app.createOrEditRecipientModalSaved', function () {
            getRecipients();
        });


        function deleteRecipient(recipientBankAccounts) {
            abp.message.confirm(
                '',
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        console.log('Test' +recipientBankAccounts.id)
                        _recipientBankAccountsService.delete(recipientBankAccounts.id.toString()  
                        ).done(function () {
                            getRecipients();
                            abp.notify.success(app.localize('SuccessfullyDeleted'));
                        });
                    }
                }
            );
        }

        $('#CreateNewRecipientBankAccountButton').click(function () {
            _createOrEditModal.open();
        });

        $('#btn-reset-filters').click(function (e) {
            $('.reload-on-change,.reload-on-keyup,#MyEntsTableFilter').val('');
            getRecipients();

        });

        $('#ImportCSV1').click(function (event) {
            event.preventDefault(); // Prevent form submission

            const fileInput = document.getElementById('ImportCSV');

            if (!fileInput.files.length) {
                abp.message.warn(app.localize('FileSelect'));
                return;
            }

            const file = fileInput.files[0];
            const reader = new FileReader();

            reader.onload = function (event) {
                const base64String = btoa(String.fromCharCode(...new Uint8Array(event.target.result)));

                const input = {
                    fileBytes: base64String,
                    fileName: file.name
                };

                console.log("Sending input:", input);

                _recipientBankAccountsService.importRecipeintBankAccountExcel(input)
                    .done(function (result) {
                        if (result.success) {
                            abp.notify.success(result.message);
                            getRecipients();
                        } else {
                            if (result.duplicates && result.duplicates.length > 0) {
                                let duplicateList = $("#duplicateRecordsList");
                                duplicateList.empty(); 

                                result.duplicates.forEach(function (record) {
                                    duplicateList.append("<li>" + record + "</li>");
                                });

                                $("#duplicateRecordsModal").modal("show");
                                getRecipients();
                            } else {
                                abp.message.error(result.message);
                                getRecipients();
                            }
                        }
                    })
                    .fail(function (error) {
                        abp.message.error(app.localize('ErrorImport'));
                    })
                    .always(function () {
                        _modal_manager.setBusy(false);
                        getRecipients();
                    });
            };

            reader.readAsArrayBuffer(file);
        });


        document.getElementById('ImportCSV').addEventListener('change', function () {
            let fileName = this.files.length > 0 ? this.files[0].name : "No file chosen";
            document.getElementById('fileName').value = fileName;
        });

    });
})();
