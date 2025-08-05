/*
* Copyright © 2025. Cloud Software Group, Inc.
* This file is subject to the license terms contained
* in the license file that is distributed with this file.
*/

using Citrix.UnifiedApi.Test.TMS;
using Citrix.UnifiedApi.Test.TMS.CitrixOidc;
using Citrix.UnifiedApi.Test.TMS.Discovery;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add logging
builder.Logging.AddConsole();

// Load configuration
var frontEndSettings = builder.Configuration.GetSection("FrontEnd").Get<FrontEndSettings>() ??
                       throw new InvalidOperationException("Missing FrontEnd settings");
var oidcSettings = builder.Configuration.GetSection("Client").Get<OidcSettings>() ??
                   throw new InvalidOperationException("Missing OIDC settings");

// Add services to the container.
var services = builder.Services;

services.Configure<FrontEndSettings>(builder.Configuration.GetSection("FrontEnd"));
services.Configure<OidcSettings>(builder.Configuration.GetSection("Client"));

services.AddControllersWithViews();
services.AddAntiforgery();

services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(
        options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
            options.SlidingExpiration = true;

            // automatically revoke refresh token at signout time
            options.Events.OnSigningOut = async e =>
            {
                await e.HttpContext.RevokeRefreshTokenAsync();
            };
        }
    )
    .AddOpenIdConnect(options =>
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Authority = oidcSettings.Authority;
        options.ResponseType = OpenIdConnectResponseType.Code;

        options.ClientId = oidcSettings.ClientId;
        options.ClientSecret = oidcSettings.ClientSecret;

        options.UsePkce = oidcSettings.UsePkce;

        options.CallbackPath = oidcSettings.CallbackPath;
        options.SaveTokens = true;
        options.Events = new CitrixOpenIdConnectEvents();
        options.UseTokenLifetime = true;
    });

builder.Services.AddOpenIdConnectAccessTokenManagement(options =>
{
});

services.AddDistributedMemoryCache();
services.AddHttpClient(nameof(DiscoveryClient), configureClient: client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Citrix TMS Example - API HttpClient");
    client.DefaultRequestHeaders.Add("Citrix-ApplicationId", oidcSettings.ApplicationId);
});
services.AddMemoryCache();
services.AddSingleton<DiscoveryClient>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowCredentials().AllowAnyHeader().AllowAnyMethod().WithOrigins(frontEndSettings.FrontEndCorsOrigin);
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors();
app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();