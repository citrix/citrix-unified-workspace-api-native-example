/*
* Copyright © 2025. Cloud Software Group, Inc.
* This file is subject to the license terms contained
* in the license file that is distributed with this file.
*/

using Duende.AccessTokenManagement.OpenIdConnect;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Citrix.UnifiedApi.Test.TMS.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class SessionController : Controller
    {
        private readonly IAntiforgery _antiForgeryService;
        private readonly ILogger<SessionController> _logger;

        public SessionController(ILogger<SessionController> logger, IAntiforgery antiForgeryService)
        {
            _logger = logger;
            _antiForgeryService = antiForgeryService;
        }

        [HttpGet]
        public ActionResult<SessionState> CheckSession()
        {
            _logger.LogInformation("Checking session");
            var antiForgeryTokenSet = _antiForgeryService.GetAndStoreTokens(HttpContext).RequestToken!;

            var authenticateResultFeature =
                HttpContext.Features.Get<IAuthenticateResultFeature>()?.AuthenticateResult;

            if (authenticateResultFeature?.Succeeded is not true)
            {
                return new SessionState(false, null, antiForgeryTokenSet);
            }

            var domainClaim = authenticateResultFeature.Properties.GetString("workspace_domain");

            return new SessionState(true, domainClaim, antiForgeryTokenSet);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Token>> RetrieveToken([FromHeader(Name = "RequestVerificationToken")] string _)
        {
            _logger.LogInformation("Retrieving token");
            if (HttpContext.User.Identity?.IsAuthenticated is not true)
            {
                return Unauthorized();
            }

            _logger.LogInformation("Performing refresh");
            var userAccessTokenAsync =
                await HttpContext.GetUserAccessTokenAsync(new UserTokenRequestParameters { ForceRenewal = true });

            if (userAccessTokenAsync.IsError)
            {
                _logger.LogInformation("Unable to refresh token, '{errorMessage}'", userAccessTokenAsync.Error);
                return Unauthorized();
            }

            return new ActionResult<Token>(
                new Token(
                    userAccessTokenAsync.AccessToken!,
                    (int)(userAccessTokenAsync.Expiration - DateTimeOffset.Now).TotalSeconds,
                    userAccessTokenAsync.Scope!
                )
            );
        }
    }

    public record SessionState(bool IsLoggedIn, string? WorkspaceDomain, string RequestVerificationToken);

    public record Token(string AccessToken, int ExpiresIn, string Scope);
}