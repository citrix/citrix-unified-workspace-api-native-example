/*
* Copyright © 2023. Cloud Software Group, Inc.
* This file is subject to the license terms contained
* in the license file that is distributed with this file.
*/

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Citrix.UnifiedApi.Test.NativeClientWpf
{
    public class Service
    {
        [JsonPropertyName("service")]
        public string Name { get; set; }

        [JsonPropertyName("endpoints")]
        public List<Endpoint> Endpoints { get; set; }
    }

    public class Endpoint
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
