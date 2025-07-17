(function ($) {
  $.validator.setDefaults({
    errorElement: 'div',
    errorClass: 'invalid-feedback',
    focusInvalid: false,
    submitOnKeyPress: true,
    ignore: ':hidden',
      highlight: function (element) {
          var targetInputElemt = $(element).closest('.form-group').find('input:eq(0),select:eq(0)');

          if(targetInputElemt.hasClass("select2") && $(element).closest('.form-group').find('span.select2-container').length>0) {
              $(element).closest('.form-group').find('span.select2-container .form-select').addClass('is-invalid');
          }
          else {
              targetInputElemt.addClass('is-invalid')
          }
    },

      unhighlight: function (element) {

          var targetInputElemt = $(element).closest('.form-group').find('input:eq(0),select:eq(0)');

          if (targetInputElemt.hasClass("select2") && $(element).closest('.form-group').find('span.select2-container').length > 0) {
              $(element).closest('.form-group').find('span.select2-container .form-select').removeClass('is-invalid');
          }
          else {
              targetInputElemt.removeClass('is-invalid')
          }
      /*$(element).closest('.form-group').find('input:eq(0)').removeClass('is-invalid');*/
    },

      errorPlacement: function (error, element) {

      if (element.hasClass("select2") && $(element).closest('.form-group').find('span.select2-container').length > 0)
      {
          error.insertAfter($(element).closest('.form-group').find('span.select2-container'));
      }
      else if (element.closest('.input-icon').length === 1) {
        error.insertAfter(element.closest('.input-icon'));
      }
      else {
        error.insertAfter(element);
      }
    },

    success: function (label) {
      label.closest('.form-group').removeClass('has-danger');
      label.remove();
    },

    submitHandler: function (form) {
      $(form).find('.alert-danger').hide();
    },
  });

  $.validator.addMethod(
    'email',
      function (value, element) {
          return /^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test(value);
    },
      app.localize("Email_Validation_Error")
    );
    $.validator.addMethod(
        'emailv1',
        function (value, element) {

            if (value.length < 1) {
                return true;
            }
            else {
                return /^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$/.test(value);
            }
        },
        app.localize("Email_Validation_Error")
    );

    $.validator.addMethod(
        'notDefaultSelect',
        function (value, element) {
            var elementDefaultValue = $(element).attr('defaultValue');
            if (elementDefaultValue == "null") {
                return value != null;
            } else {
                return (elementDefaultValue != null && elementDefaultValue != value) || (elementDefaultValue == null && value != "-1"); // Check if the value is not equal to -1
            }

        },
        app.localize("Error_SelectOption_Default")
    );

    $.validator.addMethod(
        'text',
        function (value, element) {
            return new RegExp($(element).attr('pattern')).test(value);
        },
        function (params, element) {
            // Use the localized message and replace {0} with the actual regex pattern
           // const pattern = $(element).attr('pattern');
            return app.localize("Pattern_Validation_Error") // $.validator.format($.validator.messages.regex, pattern);
        }
    );
    $.validator.addMethod("atLeastOneSelected", function (value, element) {
        // Check if at least one item is selected in the multi-select dropdown
        return $(element).val() && $(element).val().length > 0;
    }, app.localize("AtleastOneSelected_Validation_Error"));
    $.validator.addMethod("checkboxGroup", function (value, element) {
        // Check if at least one item is selected in the multi-select dropdown

        var groupClass = $(element).attr("groupclass");

        if (!groupClass) {
            groupClass = "checkboxGroup"
        }

        return $("input." + groupClass +":checked").length > 0
    }, app.localize("AtleastOneSelected_Validation_Error"));
})(jQuery);
