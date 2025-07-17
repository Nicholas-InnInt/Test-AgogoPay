using Abp.AutoMapper;
using Neptune.NsPay.MultiTenancy.Payments.Dto;

namespace Neptune.NsPay.Web.Areas.AppArea.Models.SubscriptionManagement;

[AutoMapFrom(typeof(SubscriptionPaymentProductDto))]
public class ShowDetailModalViewModel : SubscriptionPaymentProductDto
{
}