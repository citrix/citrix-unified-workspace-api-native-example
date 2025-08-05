/*
* Copyright © 2025. Cloud Software Group, Inc.
* This file is subject to the license terms contained
* in the license file that is distributed with this file.
*/

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Citrix.UnifiedApi.Test.NativeClientWpf
{
    public class TmsApiClient
    {
        public HttpClientHandler TmsClientHandler;
        public HttpClient TmsClient;
        private readonly string _baseUri;
        private readonly string _tmsTokenEndpoint = "/Session/RetrieveToken";
        private readonly string _logOutEndpoint = "/auth/LogOut";
        private readonly string _logInEndpoint = "/auth/domains/{domain}/BeginLogin";
        private readonly string _sessionCheckEndpoint = "/Session/CheckSession";

        public TmsApiClient(string baseUri)
        {
            _baseUri = baseUri;
            TmsClientHandler = new HttpClientHandler();
            TmsClient = new HttpClient(TmsClientHandler);
        }

        public async Task<Token> GetFreshAccessToken(string antiForgeryToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_baseUri}{_tmsTokenEndpoint}")
            {
                Content = null
            };
            request.Headers.Add("RequestVerificationToken", antiForgeryToken);
            var response = await TmsClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new AuthenticationException();
            }
            return await response.Content.ReadFromJsonAsync<Token>() ?? throw new InvalidOperationException();
        }

        public async Task Logout()
        {
            await TmsClient.PostAsync($"{_baseUri}{_logOutEndpoint}", null);
        }

        public string GetLogInEndpoint(string domain)
        {
            return $"{_baseUri}{_logInEndpoint.Replace("{domain}", domain)}";
        }

        public string GetBaseUri()
        {
            return _baseUri;
        }

        public async Task<SessionDetailsResponse> GetSessionDetails()
        {
            var response = await TmsClient.GetAsync($"{_baseUri}{_sessionCheckEndpoint}");
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new AuthenticationException();
            }

            var sessionDetails = await response.Content.ReadFromJsonAsync<SessionDetailsResponse>() ?? throw new InvalidOperationException();
            return sessionDetails;
        }
    }
}
