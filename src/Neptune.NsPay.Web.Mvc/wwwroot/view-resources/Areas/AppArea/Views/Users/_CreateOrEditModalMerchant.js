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
                .find('.user-merchant-checkbox-list input[value=true]')
                .each(function () {
                    assignedMerchantNames.push($(this).attr('name'));
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
                ignore: [], // Do not ignore hidden fields, they will be validated  
                rules: {
                    "chkGroupRoles": {
                        checkboxGroup: true // Custom rule for your multi-select dropdown
                    }
                },
                messages: {
                    "chkGroupRoles": {
                        checkboxGroup: app.localize('AtleastOneSelected_Validation_Error')
                    }
                },
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
                    $('#chkGroupRolesValidator').valid();
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
            console.log(assignedRoleNames);
            var assignedMerchantNames = _findAssignedMerchantNames();
            var user = _$userInformationForm.serializeFormToObject();

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
