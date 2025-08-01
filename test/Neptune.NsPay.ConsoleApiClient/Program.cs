﻿using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Json;
using IdentityModel.Client;
using Abp.Web.Models;
using Neptune.NsPay.Authorization.Users.Dto;
using System.Text.Json;

namespace Neptune.NsPay.ConsoleApiClient
{
    /* 
     * This is a sample code to create an IdentityServer4 client and use ResourceOwnerPassword flow to call an API. 
     * Enable IdentityServer from appsettings.json of Web.Host/Web.Mvc project first.
     */

    class Program
    {
        private const string ServerUrlBase = "https://localhost:44301/";

        // If you have changed "Configuration.MultiTenancy.TenantIdResolveKey" in your web app, use the same value here. 
        private const string TenantIdResolveKey = "Abp.TenantId";

        static void Main(string[] args)
        {
            RunDemoAsync().Wait();
            Console.ReadLine();
        }

        public static async Task RunDemoAsync()
        {
            var accessToken = await GetAccessTokenViaOwnerPasswordAsync();
            await GetUsersListAsync(accessToken);
        }

        private static async Task<string> GetAccessTokenViaOwnerPasswordAsync()
        {
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(ServerUrlBase);
            if (disco.IsError)
            {
                throw new Exception(disco.Error);
            }

            client.DefaultRequestHeaders.Add("Abp.TenantId", "1");  //Set TenantId
            var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "client",
                ClientSecret = "def2edf7-5d42-4edc-a84a-30136c340e13",

                Scope = "default-api",

                UserName = "admin",
                Password = "123qwe"
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine("Error: ");
                Console.WriteLine(tokenResponse.Error);
            }

            Console.WriteLine(tokenResponse.Json);

            return tokenResponse.AccessToken;
        }

        private static async Task GetUsersListAsync(string accessToken)
        {
            using (var client = new HttpClient())
            {
                client.SetBearerToken(accessToken);

                var response = await client.PostAsync($"{ServerUrlBase}api/services/app/user/getUsers", new StringContent("{}", Encoding.UTF8, "application/json"));
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.StatusCode);
                    return;
                }

                var content = await response.Content.ReadAsStringAsync();
                var ajaxResponse = JsonSerializer.Deserialize<AjaxResponse<PagedResultDto<UserListDto>>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (!ajaxResponse.Success)
                {
                    throw new Exception(ajaxResponse.Error?.Message ?? "Remote service throws exception!");
                }

                Console.WriteLine();
                Console.WriteLine("Total user count: " + ajaxResponse.Result.TotalCount);
                Console.WriteLine();

                foreach (var user in ajaxResponse.Result.Items)
                {
                    Console.WriteLine($"### UserId: {user.Id}, UserName: {user.UserName}");
                    Console.WriteLine(user.ToJsonString(indented: true));
                }
            }
        }
    }
}