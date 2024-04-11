# Citrix Unified Workspace API - Token Management Service sample backend built with .Net Core

This is a backend service implementation for a Native application that handles tokens.

The TMS primary responsibility is to store the Client ID and Client Secret so that the user cannot access it. The service will perform the role of client in the OIDC Authorization flow and stores the Refresh tokens and Access tokens for the user in encrypted cookies.

This is purely an example and shouldn't be used for real production services.

## Prerequisites

- You have either a Private or Public Workspace OAuth Client
- You will be running the example code in Visual Studio and can run [.NET 7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

## Getting Started

### Configuration

#### Workspace OAuth Client Settings

To configure the Workspace OAuth Client settings including secrets use the dotnet user secrets feature. See https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0&tabs=windows.


These settings will be related to OAuth clients created in the Citrix Cloud Admin Console - see [the documentation](https://developer-docs.citrix.com/en-us/workspace-platform/unified-workspace-api-preview/oauth-client-management) for details on how to create one.

Example:
```json
{
  "Client": {
    "ClientId": "clientId==",
    "ClientSecret": null,
    "CallbackPath": "/callback",
    "UsePkce": true,
    "UseOfflineAccess": true
  }
}
```

- [Required] `ClientId`: The Client ID of the client. 
- [Optional] `ClientSecret`: The Client Secret of the client, shown after creating a private client. This is only needed if a private client was created.
- [Optional] `CallbackPath`: Set to `/callback` by default, this is used to formulate the 'redirect url' that is required to be set on the client, e.g. the host for this application is `https://localhost:7182` and therefore the allowed redirect URL set on the client must be `https://localhost:7182/callback`.
- [Optional] `UsePkce`: Set to true by default, this must match what you set during client creation. You can check the value by clicking the "Edit" dropdown item for the client you want to use
- [Optional] `UseOfflineAccess`: Set to true by default, this must match what you set during client creation. You can check the value by clicking the "Edit" dropdown item for the client you want to use

#### Frontend Settings

```json
"FrontEnd": {
    "FrontEndRedirectUrl": "https://localhost:3000/",
    "FrontEndCorsOrigin": "https://localhost:3000"
}
```

- [Required] `FrontEndRedirectUrl`: Url to redirect the user to after login.
- [Required] `FrontEndCorsOrigin`: Origin to be allowed under the Cors policy.

## Running the example

This example is a backend, and therefore runs in the background awaiting client calls.

### General flow for implementing client

- Client application is loaded (see the [Native](../README.md) client example)

- Client application makes a call to `Session/CheckSession` endpoint to determine if a session exists. 

- If the session is not logged in, the client application will direct a browser to `Auth/Domains/{customerDomain}/BeginLogin` to begin the login flow. Once the flow is complete and the user is authenticated it will redirect back to the client application's associated redirect url.

- Client application makes a call to `Session/RetrieveAccessToken` to retrieve the access token, with an antiforgery token in the header. This token will allow the application to call the Citrix API. The token will be short-lived and should only be stored in secure memory, and not stored in browser storage.

- Client application then uses the access token to call the Citrix Workspace API.

- Once a token has expired or is close to expiration, the application can call `Session/RetrieveAccessToken` to refresh the token.

- When the Client application performs a logout, it makes a call to `Auth/LogOut` to invalidate auth cookies.  

## License

Copyright © 2023. Cloud Software Group, Inc. All Rights Reserved.
