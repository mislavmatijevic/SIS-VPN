﻿using Microsoft.Win32;
using SIS_VPN_Client_Application.logic;
using SIS_VPN_Client_Application.models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SIS_VPN_Client_Application.usercontrols.menu
{
    /// <summary>
    /// Interaction logic for EndpointsControl.xaml
    /// </summary>
    public partial class EndpointsControl : UserControl, INotifyPropertyChanged
    {
        private Endpoint selectedEndpoint;
        public Endpoint SelectedEndpoint
        {
            get => selectedEndpoint;
            set
            {
                selectedEndpoint = value;

                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<Endpoint> Endpoints { get; set; }
        private string settingsPath = "endpoints.json";

        public EndpointsControl()
        {
            Endpoints = new ObservableCollection<Endpoint>();

            LoadSavedEndpoints();

            InitializeComponent(); // Note to self, init collections before InitializeComponent :(
        }

        private void LoadSavedEndpoints()
        {
            if (!File.Exists(settingsPath))
                return;

            try
            {
                string endpointsJson = File.ReadAllText(settingsPath);

                var savedEndpoints = JsonSerializer.Deserialize<List<Endpoint>>(endpointsJson);

                Endpoints.Clear();
                savedEndpoints.ForEach(endpoint => Endpoints.Add(endpoint));

                SelectedEndpoint = Endpoints.FirstOrDefault(endpoint => endpoint.IsSelected);
            }
            catch (InvalidOperationException)
            {
            }
            catch(JsonException)
            {
            }
        }

        private void SaveEndpoints()
        {
            string endpointsJson = JsonSerializer.Serialize(Endpoints);

            try
            {
                File.WriteAllText(settingsPath, endpointsJson);
            }
            catch (IOException ioex)
            {
                MessageBox.Show(ioex.Message, "Error");
            }
        }

        private void ButtonSaveEndpoints_Click(object sender, RoutedEventArgs e) => SaveEndpoints();

        private void ButtonOpenConfig_Click(object sender, RoutedEventArgs e)
        {
            if (selectedEndpoint == null) return;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "OpenVPN Config (*.ovpn)|*.ovpn";

            if (openFileDialog.ShowDialog() == false) return;

            string configPath = openFileDialog.FileName;
            VPNSettingsParser parser = new VPNSettingsParser(configPath);

            parser.ReadConfigFile();
            SelectedEndpoint.IPAddress = parser.ReadIPAddress();
            SelectedEndpoint.ConfigPath = configPath;
        }

        private void ButtonNewEndpoint_Click(object sender, RoutedEventArgs e) => Endpoints.Add(new Endpoint(true, "New", ""));

        private void ButtonDeleteEndpoint_Click(object sender, RoutedEventArgs e)
        {
            if (selectedEndpoint == null)
                return;

            Endpoints.Remove(selectedEndpoint);
            SelectedEndpoint = Endpoints.Last();
        }



        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}