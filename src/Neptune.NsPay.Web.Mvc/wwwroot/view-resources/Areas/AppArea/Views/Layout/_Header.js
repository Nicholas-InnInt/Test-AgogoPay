﻿(function ($) {
  $(function () {
    // Back to my account

    $('#UserProfileBackToMyAccountButton').click(function (e) {
      e.preventDefault();
      abp.ajax({
        url: abp.appPath + 'Account/BackToImpersonator',
        success: function () {
          if (!app.supportsTenancyNameInUrl) {
            abp.multiTenancy.setTenantIdCookie(abp.session.impersonatorTenantId);
          }
        },
      });
    });

    //My settings

    var mySettingsModal = new app.ModalManager({
      viewUrl: abp.appPath + 'AppArea/Profile/MySettingsModal',
      scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/Profile/_MySettingsModal.js',
      modalClass: 'MySettingsModal',
    });

    $('#UserProfileMySettingsLink').click(function (e) {
      e.preventDefault();
      mySettingsModal.open();
    });

    $('#UserDownloadCollectedDataLink').click(function (e) {
      e.preventDefault();
      abp.services.app.profile.prepareCollectedData().done(function () {
        abp.message.success(app.localize('GdprDataPrepareStartedNotification'));
      });
    });

    // Change password

    var changePasswordModal = new app.ModalManager({
      viewUrl: abp.appPath + 'AppArea/Profile/ChangePasswordModal',
      scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/Profile/_ChangePasswordModal.js',
      modalClass: 'ChangePasswordModal',
    });

    $('#UserProfileChangePasswordLink').click(function (e) {
      e.preventDefault();
      changePasswordModal.open();
    });

    // Change profile picture

    var changeProfilePictureModal = new app.ModalManager({
      viewUrl: abp.appPath + 'AppArea/Profile/ChangePictureModal',
      scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/Profile/_ChangePictureModal.js',
      modalClass: 'ChangeProfilePictureModal',
    });

    $('#UserProfileChangePictureLink').click(function (e) {
      e.preventDefault();
      changeProfilePictureModal.open();
    });

    // Manage linked accounts
    var _userLinkService = abp.services.app.userLink;

    var manageLinkedAccountsModal = new app.ModalManager({
      viewUrl: abp.appPath + 'AppArea/Profile/LinkedAccountsModal',
      scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/Profile/_LinkedAccountsModal.js',
      modalClass: 'LinkedAccountsModal',
    });

    $('.manage-linked-accounts-link').click(function (e) {
      e.preventDefault();
      manageLinkedAccountsModal.open();
    });

    var getRecentlyLinkedUsers = function () {
      _userLinkService.getRecentlyUsedLinkedUsers().done(function (result) {
        loadRecentlyUsedLinkedUsers(result);

        $('.recently-linked-user').click(function (e) {
          e.preventDefault();
          var userId = $(this).attr('data-user-id');
          var tenantId = $(this).attr('data-tenant-id');
          if (userId) {
            switchToUser(userId, tenantId);
          }
        });
      });
    };


    function loadRecentlyUsedLinkedUsers(result) {
      var $ul = $('div#RecentlyUsedLinkedUsers');

      $.each(result.items, function (index, linkedUser) {
        linkedUser.shownUserName = app.getShownLinkedUserName(linkedUser);
      });

        if (!result.items.length > 0) {
            hideRecentlyLinkedUsersUi();
        } else {
            showRecentlyLinkedUsersUi();
        }

      var template = $('#linkedAccountsSubMenuTemplate').html();
      Mustache.parse(template);
      var rendered = Mustache.render(template, result);
      $ul.html(rendered);

      if (!result || !result.items || !result.items.length) {
        $('#RecentlyUsedLinkedUsers').closest('div').hide();
      } else {
        $('#RecentlyUsedLinkedUsers').closest('div').show();
      }
    }

    function hideRecentlyLinkedUsersUi() {
         var items = document.querySelectorAll('.no-recently-linked-account');

         items.forEach(item => {
             item.classList.add('d-none');
         });

         $('#manageLinkedAccounts').click(function (e) {
             e.preventDefault();
             manageLinkedAccountsModal.open();
         });
      }

    function showRecentlyLinkedUsersUi() {
          var items = document.querySelectorAll('.no-recently-linked-account');

          items.forEach(item => {
              item.classList.remove('d-none');
          });

        $('#manageLinkedAccounts').unbind();
      }

    function switchToUser(linkedUserId, linkedTenantId) {
      abp.ajax({
        url: abp.appPath + 'Account/SwitchToLinkedAccount',
        data: JSON.stringify({
          targetUserId: linkedUserId,
          targetTenantId: linkedTenantId,
        }),
        success: function () {
          if (!app.supportsTenancyNameInUrl) {
            abp.multiTenancy.setTenantIdCookie(linkedTenantId);
          }
        },
      });
    }

    manageLinkedAccountsModal.onClose(function () {
      getRecentlyLinkedUsers();
    });

    // Notifications
    var _appUserNotificationHelper = new app.UserNotificationHelper();
    var _cacheService = abp.services.app.caching;
    var _notificationService = abp.services.app.notification;
    
    function shouldUserUpdateApp(){
      _notificationService
          .shouldUserUpdateApp().done(result => {
            if(result) {
            
              abp.message.confirm(
                  null,
                  app.localize('NewVersionAvailableNotification'),
                  function (isConfirmed) {
                    if (isConfirmed) {
                        abp.ajax({
                          url: '/BrowserCacheCleaner/Clear'
                        }).done(result =>{
                          if (result){
                            window.location.reload();
                          }
                        })
                      }
                  }
            );
          }
      })
    }
    
    function bindNotificationEvents() {
      shouldUserUpdateApp();
        
      $('#setAllNotificationsAsReadLink').click(function (e) {
        e.preventDefault();
        e.stopPropagation();

        _appUserNotificationHelper.setAllAsRead(function () {
          loadNotifications();
        });
      });

      $('.set-notification-as-read').click(function (e) {
        e.preventDefault();
        e.stopPropagation();

        var notificationId = $(this).attr('data-notification-id');
        if (notificationId) {
          _appUserNotificationHelper.setAsRead(notificationId, function () {
            loadNotifications();
          });
        }
      });

      $('#openNotificationSettingsModalLink').click(function (e) {
        e.preventDefault();
        e.stopPropagation();

        _appUserNotificationHelper.openSettingsModal();
      });

      $('.user-notification-item-clickable').click(function () {
        var url = $(this).attr('data-url');
        document.location.href = url;
      });
    }

    function loadNotifications() {
      _notificationService
        .getUserNotifications({
          maxResultCount: 3,
        })
        .done(function (result) {
          result.notifications = [];
          result.unreadMessageExists = result.unreadCount > 0;
          $.each(result.items, function (index, item) {
            var formattedItem = _appUserNotificationHelper.format(item);
            result.notifications.push(formattedItem);
          });

          var $li = $('#header_notification_bar');

          var template = $('#headerNotificationBarTemplate').html();
          Mustache.parse(template);
          var rendered = Mustache.render(template, result);

          $li.html(rendered);

          bindNotificationEvents();
        });
    }

    abp.event.on('abp.notifications.received', function (userNotification) {
      _appUserNotificationHelper.show(userNotification);
      loadNotifications();
    });

    abp.event.on('app.notifications.refresh', function () {
      loadNotifications();
    });

    abp.event.on('app.notifications.read', function (userNotificationId) {
      loadNotifications();
    });

    // Chat
    abp.event.on('app.chat.unreadMessageCountChanged', function (messageCount) {
      $('#chatIconUnRead .unread-chat-message-count').text(messageCount);

      if (messageCount) {
        $('#chatIconUnRead').removeClass('d-none');
        $('#chatIcon').addClass('d-none');
      } else {
        $('#chatIconUnRead').addClass('d-none');
        $('#chatIcon').removeClass('d-none');
      }
    });

    // User Delegation
    var userDelegationsModal = new app.ModalManager({
      viewUrl: abp.appPath + 'AppArea/Profile/UserDelegationsModal',
      scriptUrl: abp.appPath + 'view-resources/Areas/AppArea/Views/Profile/_UserDelegationsModal.js',
      modalClass: 'UserDelegationsModal',
    });

    $('#ManageUserDelegations').click(function (e) {
      e.preventDefault();
      userDelegationsModal.open();
    });

    $('#ActiveUserDelegationsCombobox').click(function () {
      var $activeUserDelegationsCombobox = $(this);
      var userDelegationId = parseInt($activeUserDelegationsCombobox.val());
      if (userDelegationId <= 0 || !userDelegationId) {
        return;
      }

      var username = $activeUserDelegationsCombobox.children('option:selected').attr('data-username');

      abp.message.confirm(
        app.localize('SwitchToDelegatedUserWarningMessage', username),
        app.localize('AreYouSure'),
        function (isConfirmed) {
          if (isConfirmed) {
            $activeUserDelegationsCombobox.attr('data-value', userDelegationId);
            abp.ajax({
              url: abp.appPath + 'Account/DelegatedImpersonate',
              data: JSON.stringify({
                userDelegationId: userDelegationId,
              }),
            });
          } else {
            $activeUserDelegationsCombobox.val($activeUserDelegationsCombobox.attr('data-value'));
            return false;
          }
        }
      );
    });

    // Init

    function init() {
      loadNotifications();
      getRecentlyLinkedUsers();
    }

    init();
  });
})(jQuery);
