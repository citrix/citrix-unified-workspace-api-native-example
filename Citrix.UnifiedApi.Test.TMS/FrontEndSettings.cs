/*
* Copyright © 2025. Cloud Software Group, Inc.
* This file is subject to the license terms contained
* in the license file that is distributed with this file.
*/

namespace Citrix.UnifiedApi.Test.TMS
{
    public record FrontEndSettings
    {
        /// <summary>
        ///     Url to redirect the user to after login. Should be under the <see cref="FrontEndCorsOrigin" />.
        ///     <example>https://www.frontend.example/</example>
        /// </summary>
        public string? FrontEndRedirectUrl { get; set; }

        /// <summary>
        ///     Origin to be allowed under the Cors policy.
        ///     <example>https://www.frontend.example</example>
        /// </summary>
        public string? FrontEndCorsOrigin { get; set; }
    }
}