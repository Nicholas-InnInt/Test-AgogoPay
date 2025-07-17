using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Neptune.NsPay.ELKLogExtension
{
    public interface IHttpPostService
    {
        Task<TResponse?> PostAsync<TRequest, TResponse>( TRequest request, CancellationToken cancellationToken = default);
    }
    public class ElkHttpPostService : IHttpPostService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _isElkAvailable = false;

        public ElkHttpPostService(HttpClient httpClient , ELkLogOption option )
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            if(option!=null&& !string.IsNullOrEmpty(option.HostUrl) && !string.IsNullOrEmpty(option.Token))
            {
                _isElkAvailable =true;
                _httpClient.BaseAddress = new Uri(option.HostUrl);
                _httpClient.DefaultRequestHeaders.Add("Sign" , option.Token);
            }
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default)
        {
            if(_isElkAvailable)
            {
                var jsonContent = new StringContent(
               JsonSerializer.Serialize(request, _jsonOptions),
               Encoding.UTF8,
               "application/json");

                using var response = await _httpClient.PostAsync("", jsonContent, cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions);
            }
            else
            {
                return default(TResponse) ;
            }
        }
    }
}
