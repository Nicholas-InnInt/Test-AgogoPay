using Abp.Application.Services.Dto;
using Abp.Domain.Uow;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Neptune.NsPay.Commons;
using Neptune.NsPay.DataEvent;
using Neptune.NsPay.PayOrders.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Neptune.NsPay.Web.SignalR
{
    //public class OrderTrackerService : IOrderTrackerService
    //{
    //    private readonly IHubContext<OrderTrackerHub, IOrderTrackerHubClient> _hub;
    //    private readonly IMapper _mapper;
    //    private readonly IPublicPullAppService _publicPullService;
    //    private readonly IUnitOfWorkManager _unitOfWorkManager;


    //    public OrderTrackerService(IHubContext<OrderTrackerHub, IOrderTrackerHubClient> hub, IPublicPullAppService publicPullService, IUnitOfWorkManager unitOfWorkManager)
    //    {
    //        _hub = hub;
    //        var configuration = new MapperConfiguration(cfg =>
    //        {
    //            cfg.CreateMap<PayOrdersMongoEntityGlobal, PayOrderDto>().ReverseMap();
    //        });
    //        _mapper = configuration.CreateMapper();
    //        _publicPullService = publicPullService;
    //        _unitOfWorkManager = unitOfWorkManager;
    //    }


    //    public async Task NewPayOrder(PayOrdersMongoEntityGlobal order)
    //    {
    //        var versionNumber = TimeHelper.GetUnixTimeStamp(DateTime.Now);

    //        foreach (var filterOption in OrderTrackerHub.GetOnlineUserSelectOption())
    //        {
    //            if (filterOption.Value != null && filterOption.Value.SkipCount == 0)
    //            {
    //                var finalSelectInput = filterOption.Value;
    //                finalSelectInput.OrderId = order.ID;

    //                PagedResultDto<GetPayOrderForViewDto> payOrder = null;

    //                try
    //                {
    //                    using (var uow = _unitOfWorkManager.Begin())
    //                    {
    //                        payOrder = await _publicPullService.GetAll(finalSelectInput);
    //                        await uow.CompleteAsync();
    //                    }

    //                    if (payOrder != null && payOrder.Items.Count > 0)
    //                    {
    //                        await _hub.Clients.Client(filterOption.Key).PayOrderChanged(new PayOrderChangeSignalR() { ChangeType = 1, VersionNumber = versionNumber, Order = payOrder.Items.First() });
    //                    }

    //                }
    //                catch (Exception ex)
    //                {
    //                    NlogLogger.Error("SignalR Push Add PayOrder");
    //                }

    //            }
    //        }

    //    }

    //    public async Task UpdatePayOrder(PayOrdersMongoEntityGlobal order)
    //    {
    //        var updateOrderInfo = new PayOrderChangeSignalR() { ChangeType = 2, VersionNumber = TimeHelper.GetUnixTimeStamp(DateTime.Now) };
    //        PagedResultDto<GetPayOrderForViewDto> payOrder = null;

    //        using (var uow = _unitOfWorkManager.Begin())
    //        {
    //            payOrder = await _publicPullService.GetAll(new GetAllPayOrdersInput() { OrderId = order.ID, MerchantCodeFilter = order.MerchantCode });
    //            await uow.CompleteAsync();
    //        }

    //        if (payOrder != null && payOrder.Items.Count > 0)
    //        {
    //            updateOrderInfo.Order = payOrder.Items.First();
    //        }

    //        foreach (var client in OrderTrackerHub.GetOnlineUser())
    //        {
    //            try
    //            {
    //                await _hub.Clients.Client(client.Key).PayOrderChanged(updateOrderInfo);
    //            }
    //            catch (Exception ex)
    //            {
    //                NlogLogger.Error("SignalR Push Update PayOrder");
    //            }
    //        }
    //    }

    //}

    //public interface IOrderTrackerService
    //{
    //    Task NewPayOrder(PayOrdersMongoEntityGlobal order);
    //    Task UpdatePayOrder(PayOrdersMongoEntityGlobal order);
    //}
}
