/*
* Copyright © 2023. Cloud Software Group, Inc.
* This file is subject to the license terms contained
* in the license file that is distributed with this file.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Citrix.UnifiedApi.Test.NativeClientWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WorkspaceApiClient _workspaceApiClient;
        private AuthWindow _authWindow;
        private DiscoveryResponse _customerDiscoveryData;
        public ObservableCollection<Resource> Enumeration { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            _workspaceApiClient = new WorkspaceApiClient();
            _workspaceApiClient.WsClientHandler = new HttpClientHandler();

            ResourcesList.ItemsSource = Enumeration;
            this.DataContext = this;
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            LabelStatus.Content = "Authenticating...";
            var customerDomain = TbDomain.Text;
            _workspaceApiClient.CustomerDomain = customerDomain;
            _authWindow = Application.Current.Windows.OfType<AuthWindow>().SingleOrDefault();

            if (_authWindow == null)
            {
                _authWindow = new AuthWindow(customerDomain);
                _authWindow.Authenticated += AuthWindow_Authenticated;
            }

            _authWindow.Show();
        }

        private async void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            if (_authWindow != null)
            {
                await _authWindow.LogOut();
                _workspaceApiClient.WsClient.DefaultRequestHeaders.Authorization = null;
                LabelStatus.Content = "Unauthenticated";
            }
        }

        private async void ButtonEnumerate_Click(object sender, RoutedEventArgs e)
        {
            // Clear out list first 
            ResourcesList.ItemsSource = null;
            List<Resource> resources;

            LabelStatus.Content = "Enumerating...";
            try
            {
                resources = await _workspaceApiClient.PerformEnumeration(_customerDiscoveryData.Services.Find(s => s.Name == "store").Endpoints.Find(e => e.Id == "ListResources").Url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LabelStatus.Content = "Enumeration Failure";
                return;
            }
            Enumeration = new ObservableCollection<Resource>(resources);
            
            // Force refreshing the list in the UI
            ResourcesList.ItemsSource = null;
            ResourcesList.ItemsSource = Enumeration;
            
            this.DataContext = this;
            LabelStatus.Content = "Enumerated";
        }

        private async void ButtonLaunch_Click(object sender, RoutedEventArgs e)
        {
            LabelStatus.Content = "Launching...";
            var button = sender as Button;
            var resource = button.DataContext as Resource;
            try
            {
                var token = await _authWindow.GetAccessToken();
                await _workspaceApiClient.PerformLaunch(resource.Links.LaunchUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LabelStatus.Content = "Launch Failure";
                return;
            }
            LabelStatus.Content = "Launched";
        }

        private async void AuthWindow_Authenticated(object sender)
        {
            _workspaceApiClient.WsClient = new HttpClient(_workspaceApiClient.WsClientHandler);
            LabelStatus.Content = "Authenticated";

            try
            {
                _customerDiscoveryData = await _workspaceApiClient.Discovery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                LabelStatus.Content = "Discovery Failure";
                return;
            }
        }
    }
}
