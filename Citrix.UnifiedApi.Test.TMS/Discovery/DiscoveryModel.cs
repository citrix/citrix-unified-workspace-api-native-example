/*
* Copyright © 2023. Cloud Software Group, Inc.
* This file is subject to the license terms contained
* in the license file that is distributed with this file.
*/

using System.Text.Json.Serialization;

namespace Citrix.UnifiedApi.Test.TMS.Discovery;

public record DiscoveryModel(
    ServiceInformation[] Services,
    string[] AuthScopes,
    ClientSettings ClientSettings
);

public record ServiceInformation(string Service, Endpoint[] Endpoints);

public record Endpoint(string Id, string Url);

public record ClientSettings(string CustomerDomain, string AuthDomain, bool PromptLoginEnabled)
{
    [JsonPropertyName("acr_values")] public string AcrValues { get; set; } = null!;
}