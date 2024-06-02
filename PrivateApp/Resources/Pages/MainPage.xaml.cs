using PrivateApp.Resources.Entities;
using PrivateApp.Resources.HelperClasses;
using System;
using System.Text.Json;
namespace PrivateApp
{
    public partial class MainPage : FlyoutPage
    {
        private int _sessionId;
        private byte[] _sessionKey;
        private byte[] _sessionInitVector;
        private AuthorizationData _user;
        private Crypter _crypter;
        private Converter _converter;
        private Page _settingsPage;
        private Page _outRequestsPage;
        private Page _inRequestsPage;
        private HttpClient _client;
        private JsonSerializerOptions _serializerOptions;
        public MainPage(int sessionId, byte[] sessionKey, byte[] sessionInitVector, AuthorizationData user, string userName, string deviceId)
        {
            InitializeComponent();
            _sessionId = sessionId;
            _sessionKey = sessionKey;
            _sessionInitVector = sessionInitVector;
            _user = user;
            _crypter = new Crypter();
            _converter = new Converter();
            this.userName.Text = userName;
            this.deviceId.Text = $"device ID: {deviceId}";
            _settingsPage = new SettingsPage();
            _outRequestsPage = new OutcomingRequestsPage(sessionId, sessionKey, sessionInitVector, user, userName);
            _inRequestsPage = new IncomingRequestsPage(sessionId, sessionKey, sessionInitVector, user, userName);
        }
        protected override bool OnBackButtonPressed()
        {
            return true;
        }
        private void FlyoutButtonClicked(object sender, System.EventArgs e)
        {
            IsPresented = true;
        }
        private void ExitButtonClicked(object sender, System.EventArgs e)
        {            
            Navigation.RemovePage(this);  
        }
        private async void SettingsButtonClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(_settingsPage);
        }
        private async void OutRequestsButtonClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(_outRequestsPage);
        }
        private async void InRequestsButtonClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(_inRequestsPage);
        }

    }

}
