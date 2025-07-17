(function () {
    $(function () {

        var _merchantConfigService = abp.services.app.merchantConfig;

        $('.select2').select2({
            placeholder: app.localize('Select'),
            theme: 'bootstrap5',
            width: "100%",
            allowClear: true,
            selectionCssClass: 'form-select',
            language: abp.localization.currentCulture.name,
        });


        //comment out from index.cshtml
        //$("#SaveMerchantChangeBankIpTcb").click(function () {
        //    _merchantConfigService.changeTcbBankIp().done(function () {
        //        abp.message.info(app.localize('SavedSuccessfully'));
        //    }).always(function () {
        //        $("#SaveMerchantChangeBankIpTcb").disabled = false;
        //    });
        //})

        //$("#SaveMerchantChangeBankIpVcb").click(function () {
        //    _merchantConfigService.changeVcbBankIp().done(function () {
        //        abp.message.info(app.localize('SavedSuccessfully'));
        //    }).always(function () {
        //        $("#SaveMerchantChangeBankIpVcb").disabled = false;
        //    });
        //})

        $("#SaveMerchantInformation").click(function () {
            var _$merchantInformationForm = $('form[name=MerchantInformationForm]');
            if (!_$merchantInformationForm.valid()) {
                return;
            }
            _merchantConfigService.updateMerchantConfigTitle({
                merchantCode: $("#MerchantCode").val(),
                merchantId: $("#MerchantId").val(),
                title: $("#MerchantConfigTitle").val(),
                orderBankRemark: $("#OrderBankRemark").val()
            }).done(function () {
                abp.message.info(app.localize('SavedSuccessfully'));
            }).always(function () {
                $("#SaveMerchantInformation").disabled = false;
            });
        })

        // Real-time validation for MerchantConfigTitle field
        $("#MerchantConfigTitle").on("input", function () {
            const value = $(this).val();
            const hasInvalidCharacters = /[^a-zA-Z0-9]/.test(value); // Regex to check for non-alphanumeric characters

            if (hasInvalidCharacters) {
                // Show error message
                $("#MerchantConfigTitleError").text(app.localize('MerchantTitleSymbolNotAllow')).show();
                $(this).addClass("is-invalid");
                $("#SaveMerchantInformation").prop("disabled", true);
            } else {
                // Hide error message
                $("#MerchantConfigTitleError").hide();
                $(this).removeClass("is-invalid");
                $("#SaveMerchantInformation").prop("disabled", false);
            }
        });

        $("#MerchantConfig_LoginIpAddress").on("input", function () {
            if ($(this).val().trim() !== "") {
                $(this).removeClass("is-invalid");
            } else {
                $(this).addClass("is-invalid");
            }
        });

        $("#SaveMerchantIpAddress").click(function () {
            var form = document.getElementById("MerchantIpForm");

            $("#MerchantConfig_LoginIpAddress").val($.trim($("#MerchantConfig_LoginIpAddress").val()));

            var ipAddressInput = $("#MerchantConfig_LoginIpAddress");
            var ipAddressValue = $.trim(ipAddressInput.val());

            var pattern = /^((25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.){3}(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])(,((25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.){3}(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9]))*$/;

            if (!pattern.test(ipAddressValue)) {
                ipAddressInput.addClass("is-invalid");
                abp.message.warn(app.localize('InvalidIPAddressFormat')); // Replace with localization key if needed
                return;
            }

            if (!form.checkValidity()) {
                $("#MerchantConfig_LoginIpAddress").addClass("is-invalid");
                abp.message.warn(app.localize('InvalidIPAddressFormat')); // Replace this message key with your localization key
            }
            _merchantConfigService.updateMerchantConfigIpAddress({
                merchantCode: $("#MerchantCode").val(),
                merchantId: $("#MerchantId").val(),
                loginIpAddress: $("#MerchantConfig_LoginIpAddress").val()
            }).done(function () {
                abp.message.info(app.localize('SavedSuccessfully'));
            }).always(function () {
                $("#SaveMerchantInformation").disabled = false;
            });
        })
        //textarea need extra handling for required
        $("#MerchantConfig_BankNotifyText").on("input", function () {
            if ($(this).val().trim() !== "") {
                $(this).removeClass("is-invalid");
            } else {
                $(this).addClass("is-invalid");
            }
        });
        $("#SaveMerchantNotify").click(function () {
            var _$merchantNotifyForm = $('form[name=MerchantNotifyForm]');
            if (!_$merchantNotifyForm.valid()) {
                $("#MerchantConfig_BankNotifyText").addClass("is-invalid");
                return;
            }
            var banks = [];
            $("input:checkbox[name='EditableCheckbox']:checked").each(function () {
                var type = $(this).val();
                var temp = {
                    Type: type,
                    IsOpen: true,
                    Money: $("#BankNotify_" + type).val()
                };
                banks.push(temp);
            });
            _merchantConfigService.updateMerchantNotify({
                merchantCode: $("#MerchantCode").val(),
                merchantId: $("#MerchantId").val(),
                telegramNotifyBotId: $("#TelegramNotifyBotId").val(),
                telegramNotifyChatId: $("#TelegramNotifyChatId").val(),
                bankNotifyText: $("#MerchantConfig_BankNotifyText").val(),
                merchantConfigBank: banks
            }).done(function () {
                abp.message.info(app.localize('SavedSuccessfully'));
            }).always(function () {
                $("#SaveMerchantInformation").disabled = false;
            });
        })

        $("#SaveMerchantPlatFromWithdraw").click(function () {
            var _$merchantPlatformForm = $('form[name=MerchantPlatFromWithdrawForm]');
            if (!_$merchantPlatformForm.valid()) {
                return;
            }
            _merchantConfigService.updateMerchantPlatFromWithdraw({
                merchantCode: $("#MerchantCode").val(),
                merchantId: $("#MerchantId").val(),
                OpenRiskWithdrawal: $('#MerchantConfigOpenRiskWithdrawal').is(':checked'),
                PlatformUrl: $("#MerchantConfigPlatformUrl").val(),
                PlatformUserName: $("#MerchantConfigPlatformUserName").val(),
                PlatformPassWord: $("#MerchantConfigPlatformPassWord").val(),
                PlatformLimitMoney: $("#MerchantConfigPlatformLimitMoney").val()
            }).done(function () {
                abp.message.info(app.localize('SavedSuccessfully'));
            }).always(function () {
                $("#SaveMerchantInformation").disabled = false;
            });
        })

        $('.upload-on-change').on("change", function () {
            // trigger nearest form submit
            $(this).closest('form').submit();
        });

        $('#MerchantConfigForm').ajaxForm({
            beforeSubmit: function (formData, jqForm, options) {

                var $fileInput = $('#MerchantConfigForm input[name=MerchantLogoPicture]');
                var files = $fileInput.get()[0].files;

                if (!files.length) {
                    return false;
                }

                var file = files[0];

                //File type check
                var type = '|' + file.type.slice(file.type.lastIndexOf('/') + 1) + '|';
                if ('|jpg|jpeg|png|gif|'.indexOf(type) === -1) {
                    abp.message.warn(app.localize('File_Invalid_Type_Error'));
                    return false;
                }

                //File size check
                if (file.size > 5242880) {
                    //5M
                    abp.message.warn(app.localize('File_SizeLimit_Error'));
                    return false;
                }

                return true;
            },
            success: function (response) {
                if (response.success) {
                    refreshLogo(
                        abp.appPath +
                        'AppArea/MerchantConfig/GetProfilePicture?t=' + new Date().getTime()
                    );
                    abp.notify.info(app.localize('SavedSuccessfully'));
                } else {
                    abp.message.error(response.error.message);
                }
            },
        });

        function refreshLogo(url) {
            $('.brand-light-logo-preview-area img').attr('src', url);
        }

    });
})();