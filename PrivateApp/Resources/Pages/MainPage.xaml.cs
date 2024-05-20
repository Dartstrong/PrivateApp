using PrivateApp.Resources.Entity;
using PrivateApp.Resources.HelperClasses;
using System;
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
        private Page _outRequests;
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
            _outRequests = new OutcomingRequestsPage();
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
            await Navigation.PushAsync(_outRequests);
        }

    }

}
