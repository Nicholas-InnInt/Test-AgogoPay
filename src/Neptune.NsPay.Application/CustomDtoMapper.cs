using Neptune.NsPay.WithdrawalOrders.Dtos;
using Neptune.NsPay.WithdrawalOrders;
using Neptune.NsPay.WithdrawalDevices.Dtos;
using Neptune.NsPay.WithdrawalDevices;
using Neptune.NsPay.MerchantWithdraws.Dtos;
using Neptune.NsPay.MerchantWithdraws;
using Neptune.NsPay.MerchantWithdrawBanks.Dtos;
using Neptune.NsPay.MerchantWithdrawBanks;
using Neptune.NsPay.PayOrders.Dtos;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.PayOrderDeposits.Dtos;
using Neptune.NsPay.PayOrderDeposits;
using Neptune.NsPay.NsPayBackgroundJobs.Dtos;
using Neptune.NsPay.NsPayBackgroundJobs;
using Neptune.NsPay.AbpUserMerchants.Dtos;
using Neptune.NsPay.AbpUserMerchants;
using Neptune.NsPay.PayMents.Dtos;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayGroups.Dtos;
using Neptune.NsPay.PayGroups;
using Neptune.NsPay.PayGroupMents.Dtos;
using Neptune.NsPay.PayGroupMents;
using Neptune.NsPay.NsPaySystemSettings.Dtos;
using Neptune.NsPay.NsPaySystemSettings;
using Neptune.NsPay.MerchantSettings.Dtos;
using Neptune.NsPay.MerchantSettings;
using Neptune.NsPay.MerchantBills.Dtos;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.MerchantFunds.Dtos;
using Neptune.NsPay.MerchantFunds;
using Neptune.NsPay.MerchantRates.Dtos;
using Neptune.NsPay.MerchantRates;
using Neptune.NsPay.Merchants.Dtos;
using Neptune.NsPay.Merchants;
using Abp.Application.Editions;
using Abp.Application.Features;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.DynamicEntityProperties;
using Abp.EntityHistory;
using Abp.Extensions;
using Abp.Localization;
using Abp.Notifications;
using Abp.Organizations;
using Abp.UI.Inputs;
using Abp.Webhooks;
using AutoMapper;
using Neptune.NsPay.Auditing.Dto;
using Neptune.NsPay.Authorization.Accounts.Dto;
using Neptune.NsPay.Authorization.Delegation;
using Neptune.NsPay.Authorization.Permissions.Dto;
using Neptune.NsPay.Authorization.Roles;
using Neptune.NsPay.Authorization.Roles.Dto;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Authorization.Users.Delegation.Dto;
using Neptune.NsPay.Authorization.Users.Dto;
using Neptune.NsPay.Authorization.Users.Importing.Dto;
using Neptune.NsPay.Authorization.Users.Profile.Dto;
using Neptune.NsPay.Chat;
using Neptune.NsPay.Chat.Dto;
using Neptune.NsPay.Common.Dto;
using Neptune.NsPay.DynamicEntityProperties.Dto;
using Neptune.NsPay.Editions;
using Neptune.NsPay.Editions.Dto;
using Neptune.NsPay.Friendships;
using Neptune.NsPay.Friendships.Cache;
using Neptune.NsPay.Friendships.Dto;
using Neptune.NsPay.Localization.Dto;
using Neptune.NsPay.MultiTenancy;
using Neptune.NsPay.MultiTenancy.Dto;
using Neptune.NsPay.MultiTenancy.HostDashboard.Dto;
using Neptune.NsPay.MultiTenancy.Payments;
using Neptune.NsPay.MultiTenancy.Payments.Dto;
using Neptune.NsPay.Notifications.Dto;
using Neptune.NsPay.Organizations.Dto;
using Neptune.NsPay.Sessions.Dto;
using Neptune.NsPay.WebHooks.Dto;
using Neptune.NsPay.RedisExtensions.Models;
using Neptune.NsPay.MongoDbExtensions.Models;
using Neptune.NsPay.DataEvent;
using Neptune.NsPay.SignalRClient;
using Neptune.NsPay.RecipientBankAccounts.Dtos;

namespace Neptune.NsPay
{
    internal static class CustomDtoMapper
    {
        public static void CreateMappings(IMapperConfigurationExpression configuration)
        {
            configuration.CreateMap<CreateOrEditWithdrawalOrderDto, WithdrawalOrdersMongoEntity>().ReverseMap();
            configuration.CreateMap<WithdrawalOrderDto, WithdrawalOrdersMongoEntity>().ReverseMap();
            configuration.CreateMap<CreateOrEditWithdrawalDeviceDto, WithdrawalDevice>().ReverseMap();
            configuration.CreateMap<WithdrawalDeviceRedisModel, WithdrawalDevice>().ReverseMap();
            configuration.CreateMap<WithdrawalDeviceDto, WithdrawalDevice>().ReverseMap();
            configuration.CreateMap<TurndownOrPassMerchantWithdrawDto, MerchantWithdraw>().ReverseMap();
            configuration.CreateMap<CreateOrEditMerchantWithdrawDto, MerchantWithdraw>().ReverseMap();
            configuration.CreateMap<MerchantWithdrawDto, MerchantWithdraw>().ReverseMap();
            configuration.CreateMap<CreateOrEditMerchantWithdrawBankDto, MerchantWithdrawBank>().ReverseMap();
            configuration.CreateMap<MerchantWithdrawBankDto, MerchantWithdrawBank>().ReverseMap();
            configuration.CreateMap<CreateOrEditPayOrderDto, PayOrdersMongoEntity>().ReverseMap();
            configuration.CreateMap<PayOrderDto, PayOrdersMongoEntity>().ReverseMap();
            configuration.CreateMap<CreateOrEditPayOrderDepositDto, PayOrderDepositsMongoEntity>().ReverseMap();
            configuration.CreateMap<PayOrderDepositDto, PayOrderDepositsMongoEntity>().ReverseMap();
            configuration.CreateMap<CreateOrEditNsPayBackgroundJobDto, NsPayBackgroundJob>().ReverseMap();
            configuration.CreateMap<NsPayBackgroundJobDto, NsPayBackgroundJob>().ReverseMap();
            configuration.CreateMap<CreateOrEditAbpUserMerchantDto, AbpUserMerchant>().ReverseMap();
            configuration.CreateMap<AbpUserMerchantDto, AbpUserMerchant>().ReverseMap();
            configuration.CreateMap<PayMentRedisModel, PayMent>().ReverseMap();

            configuration.CreateMap<CreateOrEditPayMentDto, PayMent>().ReverseMap();
            configuration.CreateMap<PayMentDto, PayMent>().ReverseMap();
            configuration.CreateMap<CreateOrEditPayGroupDto, PayGroup>().ReverseMap();
            configuration.CreateMap<PayGroupDto, PayGroup>().ReverseMap();
            configuration.CreateMap<CreateOrEditPayGroupMentDto, PayGroupMent>().ReverseMap();
            configuration.CreateMap<PayGroupMentDto, PayGroupMent>().ReverseMap();
            configuration.CreateMap<CreateOrEditNsPaySystemSettingDto, NsPaySystemSetting>().ReverseMap();
            configuration.CreateMap<NsPaySystemSettingDto, NsPaySystemSetting>().ReverseMap();
            configuration.CreateMap<CreateOrEditMerchantSettingDto, MerchantSetting>().ReverseMap();
            configuration.CreateMap<MerchantSettingDto, MerchantSetting>().ReverseMap();
            configuration.CreateMap<CreateOrEditMerchantBillDto, MerchantBillsMongoEntity>().ReverseMap();
            configuration.CreateMap<MerchantBillDto, MerchantBillsMongoEntity>().ReverseMap();
            configuration.CreateMap<CreateOrEditMerchantFundDto, MerchantFund>().ReverseMap();
            configuration.CreateMap<MerchantFundDto, MerchantFund>().ReverseMap();
            configuration.CreateMap<CreateOrEditMerchantRateDto, MerchantRate>().ReverseMap();
            configuration.CreateMap<MerchantRateDto, MerchantRate>().ReverseMap();
            configuration.CreateMap<CreateOrEditMerchantDto, Merchant>().ReverseMap();
            configuration.CreateMap<MerchantDto, Merchant>().ReverseMap();
            //Inputs
            configuration.CreateMap<CheckboxInputType, FeatureInputTypeDto>();
            configuration.CreateMap<SingleLineStringInputType, FeatureInputTypeDto>();
            configuration.CreateMap<ComboboxInputType, FeatureInputTypeDto>();
            configuration.CreateMap<IInputType, FeatureInputTypeDto>()
                .Include<CheckboxInputType, FeatureInputTypeDto>()
                .Include<SingleLineStringInputType, FeatureInputTypeDto>()
                .Include<ComboboxInputType, FeatureInputTypeDto>();
            configuration.CreateMap<StaticLocalizableComboboxItemSource, LocalizableComboboxItemSourceDto>();
            configuration.CreateMap<ILocalizableComboboxItemSource, LocalizableComboboxItemSourceDto>()
                .Include<StaticLocalizableComboboxItemSource, LocalizableComboboxItemSourceDto>();
            configuration.CreateMap<LocalizableComboboxItem, LocalizableComboboxItemDto>();
            configuration.CreateMap<ILocalizableComboboxItem, LocalizableComboboxItemDto>()
                .Include<LocalizableComboboxItem, LocalizableComboboxItemDto>();

            //Chat
            configuration.CreateMap<ChatMessage, ChatMessageDto>();
            configuration.CreateMap<ChatMessage, ChatMessageExportDto>();

            //Feature
            configuration.CreateMap<FlatFeatureSelectDto, Feature>().ReverseMap();
            configuration.CreateMap<Feature, FlatFeatureDto>();

            //Role
            configuration.CreateMap<RoleEditDto, Role>().ReverseMap();
            configuration.CreateMap<Role, RoleListDto>();
            configuration.CreateMap<UserRole, UserListRoleDto>();

            //Edition
            configuration.CreateMap<EditionEditDto, SubscribableEdition>().ReverseMap();
            configuration.CreateMap<EditionCreateDto, SubscribableEdition>();
            configuration.CreateMap<EditionSelectDto, SubscribableEdition>().ReverseMap();
            configuration.CreateMap<SubscribableEdition, EditionInfoDto>();

            configuration.CreateMap<Edition, EditionInfoDto>().Include<SubscribableEdition, EditionInfoDto>();

            configuration.CreateMap<SubscribableEdition, EditionListDto>();
            configuration.CreateMap<Edition, EditionEditDto>();
            configuration.CreateMap<Edition, SubscribableEdition>();
            configuration.CreateMap<Edition, EditionSelectDto>();

            //Payment
            configuration.CreateMap<SubscriptionPaymentDto, SubscriptionPayment>()
                .ReverseMap()
                .ForMember(dto => dto.TotalAmount, options => options.MapFrom(e => e.GetTotalAmount()));
            configuration.CreateMap<SubscriptionPaymentProductDto, SubscriptionPaymentProduct>().ReverseMap();
            configuration.CreateMap<SubscriptionPaymentListDto, SubscriptionPayment>().ReverseMap();
            configuration.CreateMap<SubscriptionPayment, SubscriptionPaymentInfoDto>();

            //Permission
            configuration.CreateMap<Permission, FlatPermissionDto>();
            configuration.CreateMap<Permission, FlatPermissionWithLevelDto>();

            //Language
            configuration.CreateMap<ApplicationLanguage, ApplicationLanguageEditDto>();
            configuration.CreateMap<ApplicationLanguage, ApplicationLanguageListDto>();
            configuration.CreateMap<NotificationDefinition, NotificationSubscriptionWithDisplayNameDto>();
            configuration.CreateMap<ApplicationLanguage, ApplicationLanguageEditDto>()
                .ForMember(ldto => ldto.IsEnabled, options => options.MapFrom(l => !l.IsDisabled));

            //Tenant
            configuration.CreateMap<Tenant, RecentTenant>();
            configuration.CreateMap<Tenant, TenantLoginInfoDto>();
            configuration.CreateMap<Tenant, TenantListDto>();
            configuration.CreateMap<TenantEditDto, Tenant>().ReverseMap();
            configuration.CreateMap<CurrentTenantInfoDto, Tenant>().ReverseMap();

            //User
            configuration.CreateMap<User, UserEditDto>()
                .ForMember(dto => dto.Password, options => options.Ignore())
                .ReverseMap()
                .ForMember(user => user.Password, options => options.Ignore());
            configuration.CreateMap<User, UserLoginInfoDto>();
            configuration.CreateMap<User, UserListDto>();
            configuration.CreateMap<User, ChatUserDto>();
            configuration.CreateMap<User, OrganizationUnitUserListDto>();
            configuration.CreateMap<Role, OrganizationUnitRoleListDto>();
            configuration.CreateMap<CurrentUserProfileEditDto, User>().ReverseMap();
            configuration.CreateMap<UserLoginAttemptDto, UserLoginAttempt>().ReverseMap();
            configuration.CreateMap<ImportUserDto, User>();
            configuration.CreateMap<User, FindUsersOutputDto>();
            configuration.CreateMap<User, FindOrganizationUnitUsersOutputDto>();

            //AuditLog
            configuration.CreateMap<AuditLog, AuditLogListDto>();
            configuration.CreateMap<EntityChange, EntityChangeListDto>();
            configuration.CreateMap<EntityPropertyChange, EntityPropertyChangeDto>();

            //Friendship
            configuration.CreateMap<Friendship, FriendDto>();
            configuration.CreateMap<FriendCacheItem, FriendDto>();

            //OrganizationUnit
            configuration.CreateMap<OrganizationUnit, OrganizationUnitDto>();

            //Webhooks
            configuration.CreateMap<WebhookSubscription, GetAllSubscriptionsOutput>();
            configuration.CreateMap<WebhookSendAttempt, GetAllSendAttemptsOutput>()
                .ForMember(webhookSendAttemptListDto => webhookSendAttemptListDto.WebhookName,
                    options => options.MapFrom(l => l.WebhookEvent.WebhookName))
                .ForMember(webhookSendAttemptListDto => webhookSendAttemptListDto.Data,
                    options => options.MapFrom(l => l.WebhookEvent.Data));

            configuration.CreateMap<WebhookSendAttempt, GetAllSendAttemptsOfWebhookEventOutput>();

            configuration.CreateMap<DynamicProperty, DynamicPropertyDto>().ReverseMap();
            configuration.CreateMap<DynamicPropertyValue, DynamicPropertyValueDto>().ReverseMap();
            configuration.CreateMap<DynamicEntityProperty, DynamicEntityPropertyDto>()
                .ForMember(dto => dto.DynamicPropertyName,
                    options => options.MapFrom(entity => entity.DynamicProperty.DisplayName.IsNullOrEmpty() ? entity.DynamicProperty.PropertyName : entity.DynamicProperty.DisplayName));
            configuration.CreateMap<DynamicEntityPropertyDto, DynamicEntityProperty>();

            configuration.CreateMap<DynamicEntityPropertyValue, DynamicEntityPropertyValueDto>().ReverseMap();

            //User Delegations
            configuration.CreateMap<CreateUserDelegationDto, UserDelegation>();

            /* ADD YOUR OWN CUSTOM AUTOMAPPER MAPPINGS HERE */

            // SignalR Args 
            configuration.CreateMap<MerchantPaymentChangedDto, MerchantPaymentEventArgs>().ReverseMap();

            configuration.CreateMap<WithdrawalDeviceChangedDto, WithdrawalDeviceEventArgs>().ReverseMap();


            configuration.CreateMap<RecipientBankAccountsDtos, RecipientBankAccountMongoEntity>().ReverseMap();

            configuration.CreateMap<RecipientBankAccountMongoEntity, CreateOrEditRecipientBankAccountsDto>().ReverseMap();

            configuration.CreateMap<RecipientBankAccountMongoEntity, LogRecipientBankAccountsMongoEntity>().ReverseMap();

        }
    }
}
