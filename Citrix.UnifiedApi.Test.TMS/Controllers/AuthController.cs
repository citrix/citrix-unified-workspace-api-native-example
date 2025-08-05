/*
* Copyright © 2025. Cloud Software Group, Inc.
* This file is subject to the license terms contained
* in the license file that is distributed with this file.
*/

using System.ComponentModel.DataAnnotations;

using Citrix.UnifiedApi.Test.TMS.CitrixOidc;
using Citrix.UnifiedApi.Test.TMS.Discovery;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Citrix.UnifiedApi.Test.TMS.Controllers;

[ApiController]
[Route("")]
public class AuthController : Controller
{
    private readonly DiscoveryClient _client;
    private readonly IOptions<FrontEndSettings> _frontEndSettings;
    private readonly IOptions<OidcSettings> _oidcSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger, DiscoveryClient client,
        IOptions<FrontEndSettings> frontEndSettings, IOptions<OidcSettings> oidcSettings)
    {
        _logger = logger;
        _client = client;
        _frontEndSettings = frontEndSettings;
        _oidcSettings = oidcSettings;
    }

    [HttpGet("[controller]/Domains/{customerDomain}/BeginLogin", Name = "Login")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> BeginLogIn(
        [Required]
        [FromRoute]
        [RegularExpression(@"^[a-zA-Z0-9][a-zA-Z0-9-]+\.[a-zA-Z0-9][a-zA-Z0-9-]+\.[a-zA-Z]{2,63}$")]
        string customerDomain)
    {
        var discoveryModel = await _client.GetCustomerData(customerDomain);

        if (discoveryModel == null)
        {
            _logger.LogInformation("Unable to find customer information for {customerDomain}", customerDomain);
            return StatusCode(StatusCodes.Status404NotFound);
        }

        _logger.LogInformation("Starting login for {customerDomain}", customerDomain);

        var scopes = new[] { "openid", "wsp", "spa", "leases" }.ToList();
        if (_oidcSettings.Value.UseOfflineAccess)
        {
            scopes.Add("offline_access");
        }
        return Challenge(
            new OpenIdConnectChallengeProperties
            {
                Parameters = { { "acr_values", discoveryModel.ClientSettings.AcrValues } },
                Items =
                {
                    {"workspace_domain", customerDomain}
                },
                Scope = scopes,
                Prompt = discoveryModel.ClientSettings.PromptLoginEnabled ? "Login" : null,
                RedirectUri = _frontEndSettings.Value.FrontEndRedirectUrl ??
                              throw new InvalidOperationException("Missing frontend url")
            }, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpPost("[controller]/BeginLogin", Name = "BeginLogInPost")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public async Task<IActionResult> BeginLogInPost(
        [Required]
        [FromForm(Name = "CustomerDomain")]
        [RegularExpression(@"[a-zA-Z0-9][a-zA-Z0-9-]+\.[a-zA-Z0-9][a-zA-Z0-9-]+\.[a-zA-Z]{2,63}")] string customerDomain)
    {
        return await BeginLogIn(customerDomain);
    }


    [HttpPost("[controller]/[action]", Name = "LogOut")]
    public IActionResult LogOut()
    {
        _logger.LogInformation("Logging out user.");
        return SignOut(CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme);
    }
}