using Neptune.NsPay.WithdrawalOrders;
using Neptune.NsPay.WithdrawalDevices;
using Neptune.NsPay.MerchantWithdraws;
using Neptune.NsPay.MerchantWithdrawBanks;
using Neptune.NsPay.PayOrders;
using Neptune.NsPay.PayOrderDeposits;
using Neptune.NsPay.NsPayBackgroundJobs;
using Neptune.NsPay.AbpUserMerchants;
using Neptune.NsPay.PayMents;
using Neptune.NsPay.PayGroups;
using Neptune.NsPay.PayGroupMents;
using Neptune.NsPay.NsPaySystemSettings;
using Neptune.NsPay.MerchantSettings;
using Neptune.NsPay.MerchantBills;
using Neptune.NsPay.MerchantFunds;
using Neptune.NsPay.MerchantRates;
using Neptune.NsPay.Merchants;
using System.Collections.Generic;
using System.Text.Json;
using Abp.Zero.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Neptune.NsPay.Authorization.Delegation;
using Neptune.NsPay.Authorization.Roles;
using Neptune.NsPay.Authorization.Users;
using Neptune.NsPay.Chat;
using Neptune.NsPay.Editions;
using Neptune.NsPay.ExtraProperties;
using Neptune.NsPay.Friendships;
using Neptune.NsPay.MultiTenancy;
using Neptune.NsPay.MultiTenancy.Accounting;
using Neptune.NsPay.MultiTenancy.Payments;
using Neptune.NsPay.OpenIddict.Applications;
using Neptune.NsPay.OpenIddict.Authorizations;
using Neptune.NsPay.OpenIddict.Scopes;
using Neptune.NsPay.OpenIddict.Tokens;
using Neptune.NsPay.Storage;

namespace Neptune.NsPay.EntityFrameworkCore
{
    public class NsPayDbContext : AbpZeroDbContext<Tenant, Role, User, NsPayDbContext>, IOpenIddictDbContext
    {
        public virtual DbSet<WithdrawalOrder> WithdrawalOrders { get; set; }

        public virtual DbSet<WithdrawalDevice> WithdrawalDevices { get; set; }

        public virtual DbSet<MerchantWithdraw> MerchantWithdraws { get; set; }

        public virtual DbSet<MerchantWithdrawBank> MerchantWithdrawBanks { get; set; }

        public virtual DbSet<PayOrder> PayOrders { get; set; }

        public virtual DbSet<PayOrderDeposit> PayOrderDeposits { get; set; }

        public virtual DbSet<NsPayBackgroundJob> NsPayBackgroundJobs { get; set; }

        public virtual DbSet<AbpUserMerchant> AbpUserMerchants { get; set; }

        public virtual DbSet<PayMent> PayMents { get; set; }

        public virtual DbSet<PayGroup> PayGroups { get; set; }

        public virtual DbSet<PayGroupMent> PayGroupMents { get; set; }

        public virtual DbSet<NsPaySystemSetting> NsPaySystemSettings { get; set; }

        public virtual DbSet<MerchantSetting> MerchantSettings { get; set; }

        public virtual DbSet<MerchantBill> MerchantBills { get; set; }

        public virtual DbSet<MerchantFund> MerchantFunds { get; set; }

        public virtual DbSet<MerchantRate> MerchantRates { get; set; }

        public virtual DbSet<Merchant> Merchants { get; set; }

        /* Define an IDbSet for each entity of the application */

        public virtual DbSet<OpenIddictApplication> Applications { get; }

        public virtual DbSet<OpenIddictAuthorization> Authorizations { get; }

        public virtual DbSet<OpenIddictScope> Scopes { get; }

        public virtual DbSet<OpenIddictToken> Tokens { get; }

        public virtual DbSet<BinaryObject> BinaryObjects { get; set; }

        public virtual DbSet<Friendship> Friendships { get; set; }

        public virtual DbSet<ChatMessage> ChatMessages { get; set; }

        public virtual DbSet<SubscribableEdition> SubscribableEditions { get; set; }

        public virtual DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }

        public virtual DbSet<SubscriptionPaymentProduct> SubscriptionPaymentProducts { get; set; }

        public virtual DbSet<Invoice> Invoices { get; set; }

        public virtual DbSet<UserDelegation> UserDelegations { get; set; }

        public virtual DbSet<RecentPassword> RecentPasswords { get; set; }

        public NsPayDbContext(DbContextOptions<NsPayDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BinaryObject>(b => { b.HasIndex(e => new { e.TenantId }); });

            modelBuilder.Entity<SubscriptionPayment>(x =>
            {
                x.Property(u => u.ExtraProperties)
                    .HasConversion(
                        d => JsonSerializer.Serialize(d, new JsonSerializerOptions()
                        {
                            WriteIndented = false
                        }),
                        s => JsonSerializer.Deserialize<ExtraPropertyDictionary>(s, new JsonSerializerOptions()
                        {
                            WriteIndented = false
                        })
                    );
            });

            modelBuilder.Entity<SubscriptionPaymentProduct>(x =>
            {
                x.Property(u => u.ExtraProperties)
                    .HasConversion(
                        d => JsonSerializer.Serialize(d, new JsonSerializerOptions()
                        {
                            WriteIndented = false
                        }),
                        s => JsonSerializer.Deserialize<ExtraPropertyDictionary>(s, new JsonSerializerOptions()
                        {
                            WriteIndented = false
                        })
                    );
            });

            modelBuilder.Entity<ChatMessage>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.UserId, e.ReadState });
                b.HasIndex(e => new { e.TenantId, e.TargetUserId, e.ReadState });
                b.HasIndex(e => new { e.TargetTenantId, e.TargetUserId, e.ReadState });
                b.HasIndex(e => new { e.TargetTenantId, e.UserId, e.ReadState });
            });

            modelBuilder.Entity<Friendship>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.UserId });
                b.HasIndex(e => new { e.TenantId, e.FriendUserId });
                b.HasIndex(e => new { e.FriendTenantId, e.UserId });
                b.HasIndex(e => new { e.FriendTenantId, e.FriendUserId });
            });

            modelBuilder.Entity<Tenant>(b =>
            {
                b.HasIndex(e => new { e.SubscriptionEndDateUtc });
                b.HasIndex(e => new { e.CreationTime });
            });

            modelBuilder.Entity<SubscriptionPayment>(b =>
            {
                b.HasIndex(e => new { e.Status, e.CreationTime });
                b.HasIndex(e => new { PaymentId = e.ExternalPaymentId, e.Gateway });
            });

            modelBuilder.Entity<UserDelegation>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.SourceUserId });
                b.HasIndex(e => new { e.TenantId, e.TargetUserId });
            });

            modelBuilder.ConfigureOpenIddict();
        }
    }
}