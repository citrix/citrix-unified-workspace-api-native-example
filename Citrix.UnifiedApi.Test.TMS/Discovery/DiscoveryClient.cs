/*
* Copyright © 2023. Cloud Software Group, Inc.
* This file is subject to the license terms contained
* in the license file that is distributed with this file.
*/

using System.Net;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Citrix.UnifiedApi.Test.TMS.Discovery;

public class DiscoveryClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DiscoveryClient> _logger;
    private readonly IMemoryCache _memoryCache;


    public DiscoveryClient(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, ILogger<DiscoveryClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<DiscoveryModel?> GetCustomerData(string customerDomain)
    {
        var escapedCustomerDomain = Uri.EscapeDataString(customerDomain);
        return await _memoryCache.GetOrCreateAsync("DiscoveryDocuments_" + escapedCustomerDomain, async entry =>
        {

            _logger.LogInformation("Loading data for {customerDomain}", escapedCustomerDomain);
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

            var uri = new UriBuilder(customerDomain)
            {
                Path = $"/api/discovery/configurations",
                Scheme= "https",
                Port= 443
            }.Uri;

            var httpClient = _httpClientFactory.CreateClient(nameof(DiscoveryClient));

            var httpResponseMessage = await httpClient.GetAsync(uri);

            if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            httpResponseMessage.EnsureSuccessStatusCode();

            return await httpResponseMessage.Content.ReadFromJsonAsync<DiscoveryModel>();
        });
    }
}