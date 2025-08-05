/*
* Copyright © 2025. Cloud Software Group, Inc.
* This file is subject to the license terms contained
* in the license file that is distributed with this file.
*/

using System;
using System.Text.Json.Serialization;

namespace Citrix.UnifiedApi.Test.NativeClientWpf
{
    public  class Token
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expiresIn")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        public DateTime ExpiryTime { get; set; }
    }
}
