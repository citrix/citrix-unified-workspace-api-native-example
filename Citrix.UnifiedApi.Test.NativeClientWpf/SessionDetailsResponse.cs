/*
* Copyright © 2025. Cloud Software Group, Inc.
* This file is subject to the license terms contained
* in the license file that is distributed with this file.
*/

namespace Citrix.UnifiedApi.Test.NativeClientWpf
{
    public class SessionDetailsResponse
    {
        public bool IsLoggedIn { get; set; }

        public string WorkspaceDomain { get; set; }

        public string RequestVerificationToken { get; set; }
    }
}
