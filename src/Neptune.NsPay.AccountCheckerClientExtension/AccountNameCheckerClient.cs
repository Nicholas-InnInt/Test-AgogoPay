using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Configuration;
using Neptune.NsPay.Commons;
using AutoMapper;
using Neptune.NsPay.AccountCheckerClientExtension;
using Neptune.NsPay.AccountNameChecker.Dto;
using Neptune.NsPay.RedisExtensions;
using System.ComponentModel;



namespace Neptune.NsPay.AccountNameChecker
{
    public class AccountNameCheckerClient : IAccountNameCheckerClient
    {

        private Uri _checkerEndpoint;
        private readonly HttpClient client = new HttpClient();
        private readonly IMapper _objectMapper;
        private readonly IRedisService _redisService;
        private static DateTime lastRefreshCache;
        private static readonly object _lockObject = new object();

        public AccountNameCheckerClient(IConfiguration configuration , IRedisService redisService)
        {
            _redisService = redisService;
            var checkerEndpoint = configuration["AccountNameChecker:ServerUrl"];

            if (! string.IsNullOrEmpty( checkerEndpoint) && Uri.TryCreate(checkerEndpoint, UriKind.Absolute, out var _uriInstance))
            {
                _checkerEndpoint = _uriInstance;
            }

            // 如果配置文件没有就从缓存读取
            if (_checkerEndpoint == null)
            {
                refreshCache();
            }

            lastRefreshCache = DateTime.Now;
            var config = new MapperConfiguration(cfg => {

                cfg.CreateMap<CheckBankDetailResult, CheckBankDetailResultDto>();
            });

            // Step 2: Create the Mapper instance using the config
            _objectMapper = config.CreateMapper();

        }

        private void refreshCache()
        {
            try
            {
                var serverUrlstr = _redisService.GetNsPaySystemSettingKeyValue(NsPaySystemSettingKeyConst.AccountNameCheckerServerUrl);

                if (!string.IsNullOrEmpty(serverUrlstr) && Uri.TryCreate(serverUrlstr, UriKind.Absolute, out var _uriInstance))
                {
                    _checkerEndpoint = _uriInstance;
                }

                NlogLogger.Info("Server Cache Updated New Value " + serverUrlstr);
            }
            catch (Exception ex)
            {

                NlogLogger.Error("AccountNameCheckerClient  - refreshCache ", ex);
            }
        }

        private void getLatestCacheValue()
        {
            bool needUpdateCache = false;
            lock (_lockObject)
            {
                if (lastRefreshCache < (DateTime.Now - TimeSpan.FromMilliseconds(60000)))
                {
                    // need  refresh cache
                    needUpdateCache = true;
                    lastRefreshCache = DateTime.Now;
                }
            }

            if (needUpdateCache)
            {
                refreshCache();
            }
        }

        public async Task<CheckBankDetailResultDto> Get(string bankName, string accountNumber)
        {
            CheckBankDetailResult returnResult = new CheckBankDetailResult();
            getLatestCacheValue(); // use to refresh cache
            try
            {
                Uri actualUri = new Uri(_checkerEndpoint, "account/BankDetails?bankKey=" + HttpUtility.UrlEncode(bankName) + "&accountNumber=" + HttpUtility.UrlEncode(accountNumber));

                HttpResponseMessage response = await client.GetAsync(actualUri);

                if (response.IsSuccessStatusCode)
                {
                    string responseData = await response.Content.ReadAsStringAsync();
                    returnResult = JsonSerializer.Deserialize<CheckBankDetailResult>(responseData);
                    NlogLogger.Info("AccountNameCheckerClient Response " + responseData);
                }
                else
                {
                    NlogLogger.Error($"Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                NlogLogger.Error($"AccountNameCheckerClient", ex);
            }

            return _objectMapper.Map<CheckBankDetailResultDto>(returnResult);
        }
    }
}
