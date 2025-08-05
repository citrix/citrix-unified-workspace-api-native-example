/*
* Copyright © 2025. Cloud Software Group, Inc.
* This file is subject to the license terms contained
* in the license file that is distributed with this file.
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Citrix.UnifiedApi.Test.NativeClientWpf
{
    public class WorkspaceApiClient
    {
        private HttpClient WsClient = new();

        public WorkspaceApiClient()
        {
            SetAppIdHeader();
        }

        public void ResetWsClient()
        {
            WsClient = new HttpClient(new HttpClientHandler());
            SetAppIdHeader();
        }

        public void ClearAuthorizationHeader()
        {
            WsClient.DefaultRequestHeaders.Authorization = null;
        }

        public string CustomerDomain { get; set; } = "";

        public async Task<DiscoveryResponse> Discovery()
        {
            var response = await WsClient.GetAsync($"https://{CustomerDomain}/citrixapi/discovery/configurations");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Discovery Failure - got status {response.StatusCode} from API Gateway");
            }
            var discovery = await response.Content.ReadFromJsonAsync<DiscoveryResponse>();
            return discovery;
        }

        public async Task<List<Resource>> PerformEnumeration(string resourcesUrl)
        {
            await SetAuthHeader();
            var response = await WsClient.GetAsync(resourcesUrl);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Enumeration Failure - got status {response.StatusCode} from API Gateway");
            }
            var enumeration = await response.Content.ReadFromJsonAsync<EnumerationResponse>();
            return enumeration?.Resources;
        }

        public async Task PerformLaunch(string launchUrl)
        {
            await SetAuthHeader();
            var response = await WsClient.GetAsync(launchUrl);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Launch Failure - got status {response.StatusCode} from API Gateway");
            }
            var icaString = await response.Content.ReadAsStringAsync();

            var filepath = Path.GetTempFileName() + ".ica";
            await File.WriteAllTextAsync(filepath, icaString);
            Process.Start(new ProcessStartInfo(filepath) { UseShellExecute = true }); ;
        }

        private async Task SetAuthHeader()
        {
            var token = await Application.Current.Windows.OfType<AuthWindow>().SingleOrDefault().GetAccessToken();
            WsClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private void SetAppIdHeader()
        {
            if (!WsClient.DefaultRequestHeaders.Contains("Citrix-ApplicationId"))
            {
                var applicationId = ConfigurationManager.AppSettings["ApplicationId"]!;
                WsClient.DefaultRequestHeaders.Add("Citrix-ApplicationId", applicationId);
            }
        }
    }
}
