/*
* Copyright © 2023. Cloud Software Group, Inc.
* This file is subject to the license terms contained
* in the license file that is distributed with this file.
*/

namespace Citrix.UnifiedApi.Test.TMS.CitrixOidc;

public record OidcSettings
{
    public string? Authority { get; set; } = "https://accounts.cloud.com/core/";

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public string? CallbackPath { get; set; } = "/callback";

    public Boolean UsePkce { get; set; } = true;

    public Boolean UseOfflineAccess { get; set; } = true;
}