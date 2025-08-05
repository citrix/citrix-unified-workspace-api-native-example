/*
* Copyright © 2025. Cloud Software Group, Inc.
* This file is subject to the license terms contained
* in the license file that is distributed with this file.
*/

using Microsoft.Web.WebView2.Core;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using System.Windows;
using System.Configuration;
using System.Net;

namespace Citrix.UnifiedApi.Test.NativeClientWpf
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private Token? _token;
        private readonly TmsApiClient _tmsApiClient;
        private bool IsAuthenticated => _token != null;
        private readonly string _customerDomain;

        private readonly string _callbackUrl;
        private string AntiForgeryToken;

        public delegate void AuthenticatedHandler(object sender);
        public event AuthenticatedHandler Authenticated;

        public AuthWindow(string customerDomain)
        {
            InitializeComponent();
            _customerDomain = customerDomain;
            _callbackUrl = ConfigurationManager.AppSettings["TmsCallBackUrl"];
            _tmsApiClient = new TmsApiClient(ConfigurationManager.AppSettings["TmsBaseUri"]);
            this.IsVisibleChanged += LoadLoginPage;
        }

        private async void LoadLoginPage(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Only update the Source if the window is now being shown 
            if (!(bool)e.NewValue) return;

            await webView.EnsureCoreWebView2Async();

            var sessionDetails = await _tmsApiClient.GetSessionDetails();
            AntiForgeryToken = sessionDetails.RequestVerificationToken;

            foreach (var cookie in _tmsApiClient.TmsClientHandler.CookieContainer.GetAllCookies())
            {
                var convertedCookie =
                    webView.CoreWebView2.CookieManager.CreateCookieWithSystemNetCookie((Cookie)cookie);
                webView.CoreWebView2.CookieManager.AddOrUpdateCookie(convertedCookie);
            }
            webView.Source = new Uri(_tmsApiClient.GetLogInEndpoint(_customerDomain));
        }

        public async Task LogOut()
        {
            await _tmsApiClient.Logout();
        }

        public async Task<string> GetAccessToken()
        {
            if (_token != null && DateTime.Now < _token.ExpiryTime)
            {
                return _token.AccessToken;
            }

            Token newToken;
            try
            {
                newToken = await _tmsApiClient.GetFreshAccessToken(AntiForgeryToken);
            }
            catch(AuthenticationException e)
            {
                // If the call to the TMS fails, it's likely due to the session having timed-out,
                // so we re-open the WebView to log in again, and return an empty access token to reset that state
                _token = null;
                Show();
                return "";
            }
                
            newToken.ExpiryTime = DateTime.Now.AddSeconds(newToken.ExpiresIn);
            _token = newToken;

            return _token.AccessToken;
        }

        private async void Navigating(object sender, CoreWebView2NavigationStartingEventArgs args)
        {
            if (IsAuthenticated)
            {
                args.Cancel = true;
                Hide();
                return;
            }

            if (args.Uri.Contains(_callbackUrl))
            {
                args.Cancel = true;

                // Extract the cookies for the Token Management Service session, and use them to get an initial OAuth access token
                var cookies = await webView.CoreWebView2.CookieManager.GetCookiesAsync(_tmsApiClient.GetBaseUri());
                cookies.ForEach(cookie => _tmsApiClient.TmsClientHandler.CookieContainer.Add(cookie.ToSystemNetCookie()));
                
                _tmsApiClient.TmsClient = new HttpClient(_tmsApiClient.TmsClientHandler);

                Authenticated(this);
                Application.Current.Windows.OfType<AuthWindow>().SingleOrDefault()?.Hide();
            }
        }
    }
}
