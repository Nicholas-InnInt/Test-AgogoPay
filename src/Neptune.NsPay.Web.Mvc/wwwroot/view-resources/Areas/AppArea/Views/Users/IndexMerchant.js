
(function ($) {
    app.modals.CreateOrEditUserModal = function () {
        var _merchantUserService = abp.services.app.merchantUser;

        var _modalManager;
        var _$userInformationForm = null;
        var _passwordComplexityHelper = new app.PasswordComplexityHelper();
        var _organizationTree;

        var changeProfilePictureModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/Profile/ChangePictureModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/Profile/_ChangePictureModal.js',
            modalClass: 'ChangeProfilePictureModal',
        });

        function _findAssignedRoleNames() {
            var assignedRoleNames = [];

            _modalManager
                .getModal()
                .find('.user-role-checkbox-list input[type=checkbox]')
                .each(function () {
                    if ($(this).is(':checked') && !$(this).is(':disabled')) {
                        assignedRoleNames.push($(this).attr('name'));
                    }
                });

            return assignedRoleNames;
        }

        function _findAssignedMerchantNames() {
            var assignedMerchantNames = [];

            _modalManager
                .getModal()
                .find('.user-merchant-checkbox-list input[type=checkbox]')
                .each(function () {
                    if ($(this).is(':checked') && !$(this).is(':disabled')) {
                        assignedMerchantNames.push($(this).attr('name'));
                    }
                });

            return assignedMerchantNames;
        }

        this.init = function (modalManager) {
            _modalManager = modalManager;
            KTPasswordMeter.createInstances();

            _organizationTree = new OrganizationTree();

            _organizationTree.init(_modalManager.getModal().find('.organization-tree'), {
                cascadeSelectEnabled: false,
            });

            _$userInformationForm = _modalManager.getModal().find('form[name=UserInformationsForm]');
            _$userInformationForm.validate({
                errorPlacement: function (error, element) {
                    const parentEl = element.parent();
                    const el = parentEl.hasClass('position-relative') ? parentEl.parent() : parentEl;
                    error.appendTo(el);
                }
            });

            var passwordInputs = _modalManager.getModal().find('input[name=Password],input[name=PasswordRepeat]');
            var passwordInputGroups = passwordInputs.closest('.user-password');

            _passwordComplexityHelper.setPasswordComplexityRules(passwordInputs, window.passwordComplexitySetting);

            $('#EditUser_SetRandomPassword').change(function () {
                if ($(this).is(':checked')) {
                    passwordInputGroups.slideUp('fast');
                    if (!_modalManager.getArgs().id) {
                        passwordInputs.removeAttr('required');
                    }
                } else {
                    passwordInputGroups.slideDown('fast');
                    if (!_modalManager.getArgs().id) {
                        passwordInputs.attr('required', 'required');
                    }
                }
            });

            _modalManager
                .getModal()
                .find('.user-role-checkbox-list input[type=checkbox]')
                .change(function () {
                    $('#assigned-role-count').text(_findAssignedRoleNames().length);
                });

            _modalManager
                .getModal()
                .find('.user-merchant-checkbox-list input[type=checkbox]')
                .change(function () {
                    $('#assigned-merchant-count').text(_findAssignedMerchantNames().length);
                });

            _organizationTree.onChange(function () {
                $('#assigned-organization-unit-count').text(_organizationTree.getSelectedOrganizations().length);
            });

            _modalManager.getModal().find('[data-bs-toggle=tooltip]').tooltip();

            _modalManager
                .getModal()
                .find('#changeProfilePicture')
                .click(function () {
                    changeProfilePictureModal.open({ userId: _modalManager.getModal().find('input[name=Id]').val() });
                });

            changeProfilePictureModal.onClose(function () {
                _modalManager.getModal().find('.user-edit-dialog-profile-image').attr('src', abp.appPath + "Profile/GetProfilePictureByUser?userId=" + _modalManager.getModal().find('input[name=Id]').val())
            });
        };

        this.save = function () {
            if (!_$userInformationForm.valid()) {
                return;
            }

            var assignedRoleNames = _findAssignedRoleNames();
            var assignedMerchantNames = _findAssignedMerchantNames();
            var user = _$userInformationForm.serializeFormToObject();

            if (user.SetRandomPassword) {
                user.Password = null;
            }

            _modalManager.setBusy(true);
            _merchantUserService
                .createOrUpdateMerchantUser({
                    user: user,
                    assignedRoleNames: assignedRoleNames,
                    assignedMerchantNames: assignedMerchantNames,
                    sendActivationEmail: user.SendActivationEmail,
                    SetRandomPassword: user.SetRandomPassword,
                    organizationUnits: _organizationTree.getSelectedOrganizationIds(),
                })
                .done(function () {
                    abp.notify.info(app.localize('SavedSuccessfully'));
                    _modalManager.close();
                    abp.event.trigger('app.createOrEditUserModalSaved');
                })
                .always(function () {
                    _modalManager.setBusy(false);
                });
        };
    };
})(jQuery);

(function () {
    $(function () {
        var _$usersTable = $('#UsersTable');
        var _merchantUserService = abp.services.app.merchantUser;
        var _$numberOfFilteredPermission = $('#NumberOfFilteredPermission');
        var _dynamicEntityPropertyManager = new DynamicEntityPropertyManager();

        var _selectedPermissionNames = [];

        var _$permissionFilterModal = app.modals.PermissionTreeModal.create({
            disableCascade: true,
            onSelectionDone: function (filteredPermissions) {
                _selectedPermissionNames = filteredPermissions;
                var filteredPermissionCount = filteredPermissions.length;

                _$numberOfFilteredPermission.text(filteredPermissionCount);
                abp.notify.success(app.localize('XCountPermissionFiltered', filteredPermissionCount));

                getUsers();
            },
        });

        var _permissions = {
            create: abp.auth.hasPermission('Pages.Merchant.Users.Create'),
            edit: abp.auth.hasPermission('Pages.Merchant.Users.Edit'),
            unlock: abp.auth.hasPermission('Pages.Merchant.Users.Unlock'),
            delete: abp.auth.hasPermission('Pages.Merchant.Users.Delete'),
        };

        var _createOrEditModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/Users/CreateOrEditModalMerchant',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/Users/_CreateOrEditModalMerchant.js',
            modalClass: 'CreateOrEditUserModal',
        });

        var _userPermissionsModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/Users/PermissionsModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/Users/_PermissionsModal.js',
            modalClass: 'UserPermissionsModal',
        });

        var _excelExportModal = new app.ModalManager({
            viewUrl: abp.appPath + 'AppArea/Users/ExcelExportModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/Users/_ExcelExportModal.js',
            modalClass: 'ExcelExportModal'
        });

        var dataTable = _$usersTable.DataTable({
            paging: true,
            serverSide: true,
            processing: true,
            listAction: {
                ajaxFunction: _merchantUserService.getMerchantUsers,
                inputFilter: function () {
                    return {
                        filter: $('#UsersTableFilter').val(),
                        permissions: _selectedPermissionNames,
                        role: $('#RoleSelectionCombo').val(),
                        onlyLockedUsers: $('#UsersTable_OnlyLockedUsers').is(':checked'),
                    };
                },
            },
            drawCallback: function () {
                $('[data-bs-toggle="tooltip"]').tooltip();
            },
            columnDefs: [
                {
                    className: 'dtr-control responsive',
                    orderable: false,
                    render: function () {
                        return '';
                    },
                    targets: 0,
                },
                {
                    targets: 1,
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    rowAction: {
                        text:
                            '<i class="fa fa-cog"></i> <span class="d-none d-md-inline-block d-lg-inline-block d-xl-inline-block">' +
                            app.localize('Actions') +
                            '</span> <span class="caret"></span>',
                        items: [
                            {
                                text: app.localize('LoginAsThisUser'),
                                visible: function (data) {
                                    return _permissions.impersonation && data.record.id !== abp.session.userId;
                                },
                                action: function (data) {
                                    abp.ajax({
                                        url: abp.appPath + 'Account/ImpersonateUser',
                                        data: JSON.stringify({
                                            tenantId: abp.session.tenantId,
                                            userId: data.record.id,
                                        }),
                                    });
                                },
                            },
                            {
                                text: app.localize('Edit'),
                                visible: function () {
                                    return _permissions.edit;
                                },
                                action: function (data) {
                                    _createOrEditModal.open({ id: data.record.id });
                                },
                            },
                            {
                                text: app.localize('Unlock'),
                                visible: function (data) {
                                    return _permissions.unlock && data.record.lockoutEndDateUtc;
                                },
                                action: function (data) {
                                    _merchantUserService
                                        .unlockUser({
                                            id: data.record.id,
                                        })
                                        .done(function () {
                                            abp.notify.success(app.localize('UnlockedTheUser', data.record.userName));
                                            dataTable.ajax.reload()
                                        });
                                },
                            },                     
                            {
                                text: app.localize('Delete'),
                                visible: function () {
                                    return _permissions.delete;
                                },
                                action: function (data) {
                                    deleteUser(data.record);
                                },
                            },
                        ],
                    },
                },
                {
                    targets: 2,
                    data: 'userName',
                    render: function (userName, type, row, meta) {
                        var $container = $('<div/>');
                        var $userName = $('<span/>');
                        var lockedIcon = '<i class="fas fa-lock ms-2"></i>';
                        var profilePicture =
                            abp.appPath + 'Profile/GetProfilePictureByUser?userId=' + row.id + '&profilePictureId=' + row.profilePictureId;

                        if (profilePicture) {
                            var $link = $('<a/>').attr('href', profilePicture).attr('target', '_blank');
                            var $img = $('<img/>').addClass('img-circle').attr('src', profilePicture);

                            $link.append($img);
                            $container.append($link);
                        }

                        $container.addClass('hide-overflown');

                        if (userName.length > 9) {
                            $userName.attr("data-bs-toggle", "tooltip")
                            $userName.attr("title", userName)
                        }

                        $userName.append(userName);

                        if (row.lockoutEndDateUtc) {
                            if (moment.utc(row.lockoutEndDateUtc) > moment.utc()) {
                                $userName.append(lockedIcon);
                            }
                        }

                        $container.append($userName);
                        return $container[0].outerHTML;
                    },
                    width: 150
                },
                {
                    targets: 3,
                    data: 'name',
                },
                {
                    targets: 4,
                    data: 'surname',
                },
                {
                    targets: 5,
                    data: 'roles',
                    orderable: false,
                    render: function (roles) {
                        var roleNames = '';
                        for (var j = 0; j < roles.length; j++) {
                            if (roleNames.length) {
                                roleNames = roleNames + ', ';
                            }

                            roleNames = roleNames + roles[j].roleName;
                        }

                        return roleNames;
                    },
                },
                {
                    targets: 6,
                    data: 'emailAddress',
                },
                {
                    targets: 7,
                    data: 'isActive',
                    render: function (isActive) {
                        var $span = $('<span/>').addClass('label');
                        if (isActive) {
                            $span.addClass('badge badge-success').text(app.localize('Yes'));
                        } else {
                            $span.addClass('badge badge-dark').text(app.localize('No'));
                        }
                        return $span[0].outerHTML;
                    },
                },
                {
                    targets: 8,
                    data: 'creationTime',
                    render: function (creationTime) {
                        return moment(creationTime).format('L');
                    },
                },
            ],
        });

        function getUsers() {
            dataTable.ajax.reload();
        }

        function deleteUser(user) {
            if (user.userName === app.consts.userManagement.defaultAdminUserName) {
                abp.message.warn(app.localize('{0}UserCannotBeDeleted', app.consts.userManagement.defaultAdminUserName));
                return;
            }

            abp.message.confirm(
                app.localize('UserDeleteWarningMessage', user.userName),
                app.localize('AreYouSure'),
                function (isConfirmed) {
                    if (isConfirmed) {
                        _merchantUserService
                            .deleteMerchantUser({
                                id: user.id,
                            })
                            .done(function () {
                                getUsers(true);
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

        $('#CreateNewUserButton').click(function () {
            _createOrEditModal.open();
        });

        var getSortingFromDatatable = function () {
            if (dataTable.ajax.params().order.length > 0) {
                var columnIndex = dataTable.ajax.params().order[0].column;
                var dir = dataTable.ajax.params().order[0].dir;
                var columnName = dataTable.ajax.params().columns[columnIndex].data;

                return columnName + ' ' + dir;
            } else {
                return '';
            }
        };

        $('#ExportUsersToExcelButton').click(function (e) {
            _excelExportModal.open({
                filter: $('#UsersTableFilter').val(),
                permissions: _selectedPermissionNames,
                role: $('#RoleSelectionCombo').val(),
                onlyLockedUsers: $('#UsersTable_OnlyLockedUsers').is(':checked'),
                sorting: getSortingFromDatatable(),
            });
        });

        $('#GetUsersButton, #RefreshUserListButton').click(function (e) {
            e.preventDefault();
            getUsers();
        });

        $('#UsersTableFilter').on('keydown', function (e) {
            if (e.keyCode !== 13) {
                return;
            }

            e.preventDefault();
            getUsers();
        });

        abp.event.on('app.createOrEditUserModalSaved', function () {
            getUsers();
        });

        $('#UsersTableFilter').focus();

        $('#ImportUsersFromExcelButton')
            .fileupload({
                url: abp.appPath + 'AppArea/Users/ImportFromExcel',
                dataType: 'json',
                maxFileSize: 1048576 * 100,
                dropZone: $('#UsersTable'),
                done: function (e, response) {
                    var jsonResult = response.result;
                    if (jsonResult.success) {
                        abp.notify.info(app.localize('ImportUsersProcessStart'));
                    } else {
                        abp.notify.warn(app.localize('ImportUsersUploadFailed'));
                    }
                },
            })
            .prop('disabled', !$.support.fileInput)
            .parent()
            .addClass($.support.fileInput ? undefined : 'disabled');

        $('#FilterByPermissionsButton').click(function () {
            _$permissionFilterModal.open({ grantedPermissionNames: _selectedPermissionNames });
        });
    });
})();
