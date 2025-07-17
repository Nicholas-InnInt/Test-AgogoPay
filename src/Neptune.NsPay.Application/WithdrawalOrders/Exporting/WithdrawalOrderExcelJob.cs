using Abp;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Neptune.NsPay.AbpUserMerchants;
using Neptune.NsPay.Common;
using Neptune.NsPay.WithdrawalOrders.Dtos;
using Neptune.NsPay.MongoDbExtensions.Services.Interfaces;
using Neptune.NsPay.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neptune.NsPay.WithdrawalDevices.Dtos;
using Neptune.NsPay.WithdrawalDevices;


namespace Neptune.NsPay.WithdrawalOrders.Exporting
{
    public class WithdrawalOrderExcelJob : AsyncBackgroundJob<WithdrawalOrderExcelJobArgs>, ITransientDependency
    {
        private readonly IAppNotifier _appNotifier;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IWithdrawalOrdersExcelExporter _withdrawalOrdersExcelExporter;
        private readonly IRepository<WithdrawalDevice> _withdrawalDeviceRepository;
        private readonly IWithdrawalOrdersMongoService _withdrawalOrdersMongoService;
        private readonly IRepository<AbpUserMerchant> _abpUserMerchantRepository;
        public WithdrawalOrderExcelJob(
            IAppNotifier appNotifier,
            IUnitOfWorkManager unitOfWorkManager,
            IMerchantBillsMongoService merchantBillsMongoService,
            IWithdrawalOrdersExcelExporter withdrawalOrdersExcelExporter,
            IRepository<AbpUserMerchant> abpUserMerchantRepository,
            IRepository<WithdrawalDevice> withdrawalDeviceRepository,
            IWithdrawalOrdersMongoService withdrawalOrdersMongoService
            )
        {
            _appNotifier = appNotifier;
            _unitOfWorkManager = unitOfWorkManager;
            _withdrawalOrdersExcelExporter = withdrawalOrdersExcelExporter;
            _withdrawalDeviceRepository = withdrawalDeviceRepository;
            _withdrawalOrdersMongoService = withdrawalOrdersMongoService;
            _abpUserMerchantRepository = abpUserMerchantRepository;
        }

        public override async Task ExecuteAsync(WithdrawalOrderExcelJobArgs args)
        {
            var user = args.User;
            var input = args.input;

            using (var uow = _unitOfWorkManager.Begin())
            {
                var orderLists = await GetWithdrawalOrderExportAsync(user, input);

                var pageNumber = 1;
                var rowCount = 1000000;

                for (int i = 0; i < orderLists.Count; i += rowCount)
                {
                    int currentBatchSize = Math.Min(rowCount, orderLists.Count - i);

                    var file = _withdrawalOrdersExcelExporter.ExportToFile(orderLists.GetRange(i, currentBatchSize), args.UserType);

                    await _appNotifier.ExporterWithdrawalOrderAsync(args.User, file.FileToken, file.FileType, file.FileName);

                    pageNumber++;
                }

                await uow.CompleteAsync();
            }
        }

        public async Task<List<GetWithdrawalOrderForViewDto>> GetWithdrawalOrderExportAsync(UserIdentifier userIdentifier, GetAllWithdrawalOrdersForExcelInput input)
        {
            var deviceBankType = input.WithdrawalDeviceBankTypeFilter.HasValue ?
           (WithdrawalDevicesBankTypeEnum)input.WithdrawalDeviceBankTypeFilter : default;

            List<WithdrawalDevice> withDevices = new List<WithdrawalDevice>();
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                withDevices = await _withdrawalDeviceRepository.GetAllListAsync();
            }

            var devices = new List<int>();
            if (!string.IsNullOrEmpty(input.WithdrawalDevicePhoneFilter))
            {
                devices = withDevices.Select(r => r.Id).ToList();
            }
            if (input.WithdrawalDeviceBankTypeFilter.HasValue && input.WithdrawalDeviceBankTypeFilter > -1)
            {
                var temps = (await _withdrawalDeviceRepository.GetAllListAsync(r => r.BankType == deviceBankType && r.IsDeleted == false)).Select(r => r.Id).ToList();
                devices.AddRange(temps);
            }


            var filteredWithdrawalOrders = await _withdrawalOrdersMongoService.GetAll(input, input.userMerchantIdsList, devices);

            List<GetWithdrawalOrderForViewDto> withdrawalOrderListDtos = new List<GetWithdrawalOrderForViewDto>();
            foreach (var order in filteredWithdrawalOrders)
            {
                var deviceInfo = withDevices.FirstOrDefault(r => r.Id == order.DeviceId);
                GetWithdrawalOrderForViewDto orderForViewDto = new GetWithdrawalOrderForViewDto()
                {
                    WithdrawalOrder = new WithdrawalOrderDto
                    {
                        MerchantCode = order.MerchantCode,
                        MerchantId = order.MerchantId,
                        PlatformCode = order.PlatformCode,
                        WithdrawNo = order.WithdrawNo,
                        OrderNo = order.OrderNumber,
                        TransactionNo = order.TransactionNo,
                        OrderStatus = order.OrderStatus,
                        OrderMoney = order.OrderMoney.ToString(),
                        Rate = order.Rate,
                        FeeMoney = order.FeeMoney.ToString(),
                        DeviceId = order.DeviceId,
                        NotifyStatus = order.NotifyStatus,
                        BenAccountName = order.BenAccountName,
                        BenBankName = order.BenBankName,
                        BenAccountNo = order.BenAccountNo,
                        Id = order.ID,
                        CreationTime = CultureTimeHelper.GetCultureTimeInfo(order.CreationTime, CultureTimeHelper.TimeCodeViVn),
                        TransactionTime = CultureTimeHelper.GetCultureTimeInfo(order.TransactionTime, CultureTimeHelper.TimeCodeViVn)
                    },
                };
                if (deviceInfo != null)
                {
                    orderForViewDto.WithdrawalDevice = new WithdrawalDeviceDto
                    {
                        Id = deviceInfo.Id,
                        Name = deviceInfo.Name,
                        Phone = deviceInfo.Phone
                    };
                }

                withdrawalOrderListDtos.Add(orderForViewDto);
            }

            return withdrawalOrderListDtos;
        }

        private string GetCountryCode(string timeZone)
        {
            switch (timeZone)
            {
                case "GMT8+":
                    return CultureTimeHelper.TimeCodeZhCN;
                case "GMT7+":
                    return CultureTimeHelper.TimeCodeViVn;
                case "GMT4+":
                    return CultureTimeHelper.TimeCodeEST;
                default:
                    return CultureTimeHelper.TimeCodeZhCN;
            }

        }

        public async Task<int> GetTotalRecordExcel(UserIdentifier userIdentifier, GetAllWithdrawalOrdersForExcelInput input)
        {
            var deviceBankType = input.WithdrawalDeviceBankTypeFilter.HasValue ?
           (WithdrawalDevicesBankTypeEnum)input.WithdrawalDeviceBankTypeFilter : default;

            List<WithdrawalDevice> withDevices = new List<WithdrawalDevice>();
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.SoftDelete))
            {
                withDevices = await _withdrawalDeviceRepository.GetAllListAsync();
            }

            var devices = new List<int>();
            if (!string.IsNullOrEmpty(input.WithdrawalDevicePhoneFilter))
            {
                devices = withDevices.Select(r => r.Id).ToList();
            }
            if (input.WithdrawalDeviceBankTypeFilter.HasValue && input.WithdrawalDeviceBankTypeFilter > -1)
            {
                var temps = (await _withdrawalDeviceRepository.GetAllListAsync(r => r.BankType == deviceBankType && r.IsDeleted == false)).Select(r => r.Id).ToList();
                devices.AddRange(temps);
            }

            return await _withdrawalOrdersMongoService.GetTotalRecordExcelCount(input, input.userMerchantIdsList, devices);

        }
    }
}
