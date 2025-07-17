using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Neptune.NsPay.DataEvent;
using Neptune.NsPay.SignalRClient;
using Tweetinvi.Core.Models.Properties;
using Microsoft.AspNetCore.Builder;
using System.Linq;
using Neptune.NsPay.Merchants.Dtos;
using Neptune.NsPay.PayMents.Dtos;
using Neptune.NsPay.PayGroupMents.Dtos;
using Neptune.NsPay.Web.SignalR;
using Neptune.NsPay.WithdrawalDevices.Dtos;
using System.Threading.Tasks;
namespace Neptune.NsPay.Web.Startup
{
    public static class DataChangedEventHandler
    {
        public static void Configure(IApplicationBuilder app, IConfiguration configuration)
        {
            var dataChangedEvent = app.ApplicationServices.GetRequiredService<IDataChangedEvent>();
            var signalRClient = app.ApplicationServices.GetRequiredService<ISignalRClient>();
            var transferSignalRClient = app.ApplicationServices.GetRequiredService<ITransferSignalRClient>();
            var objectMapper = app.ApplicationServices.GetRequiredService<Abp.ObjectMapping.IObjectMapper>();

            // Calling a method from the DI class
            if (dataChangedEvent != null )
            {

                if(signalRClient != null)
                {
                    dataChangedEvent.AddMerchantPaymentChanged((object sender, MerchantPaymentEventArgs e) =>
                    {
                        var merchantList = e.Merchant.Select(x => new OldNewDataSet<MerchantDto>(objectMapper.Map<MerchantDto>(x.OldData), objectMapper.Map<MerchantDto>(x.NewData))).ToList();
                        var paymentList = e.Payment.Select(x => new OldNewDataSet<PayMentDto>(objectMapper.Map<PayMentDto>(x.OldData), objectMapper.Map<PayMentDto>(x.NewData))).ToList();
                        var paygroupMentList = e.PayGroupMent.Select(x => new OldNewDataSet<PayGroupMentDto>(objectMapper.Map<PayGroupMentDto>(x.OldData), objectMapper.Map<PayGroupMentDto>(x.NewData))).ToList();
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(100);
                           await signalRClient.MerchantPaymentChanged(new MerchantPaymentChangedDto() { Merchant = merchantList, Payment = paymentList, PayGroupMent = paygroupMentList });
                        });

                    }, "AdminPortal");
                }

                if(transferSignalRClient!=null)
                {
                    dataChangedEvent.AddWithdrawalDeviceChanged((object sender, WithdrawalDeviceEventArgs e) =>
                    {
                        // delay 200 second only run 
                        Task.Run(async () =>
                        {
                            // Await the delay (this does not block the current thread)
                            await Task.Delay(200);

                            // Call the function after the delay
                            await transferSignalRClient.WithdrawalDeviceChanged(new WithdrawalDeviceChangedDto() { WithdrawalDevice = e.WithdrawalDevice.Select(x => new OldNewDataSet<WithdrawalDeviceDto>(objectMapper.Map<WithdrawalDeviceDto>(x.OldData), objectMapper.Map<WithdrawalDeviceDto>(x.NewData))).ToList() });
                        });
                    }, "AdminPortal");
                }

            }
        }
    }
}
