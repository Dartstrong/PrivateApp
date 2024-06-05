using PrivateApp.Resources.Entities;
using PrivateApp.Resources.HelperClasses;
using System;
using System.Text;
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
        private Page _helpPage;
        private Page _aboutPage;  
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
            _helpPage = new HelpPage();
            _aboutPage = new AboutPage();
            CreateRestService();
            LoadingContent();
        }
        private void CreateRestService()
        {
            _client = new HttpClientService().GetPlatformSpecificHttpClient();
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }
        private async void LoadingContent()
        {
            ListView listView = new ListView();
            listView.ItemsSource = await GetMyDialogues();
            listView.SelectionMode = ListViewSelectionMode.None;
            listView.ItemTapped += ItemTapped;
            listView.ItemTemplate = new DataTemplate(() =>
            {
                ImageCell imageCell = new ImageCell
                {
                    TextColor = Color.FromHex("#7AF4BA"),
                    DetailColor = Colors.Grey,
                    ImageSource = ImageSource.FromFile("dialogue_icon.svg"),
                };
                imageCell.SetBinding(ImageCell.TextProperty, "Receiver");
                imageCell.SetBinding(ImageCell.DetailProperty, "IdStr");
                return imageCell;
            });
            mainContent.Content = new StackLayout { Children = { listView } };
        }
        private async void ItemTapped(object? sender, ItemTappedEventArgs e)
        {
            StartedDialogue dialogue = (StartedDialogue)e.Item;
            await Navigation.PushAsync(new DialoguePage(_sessionId, _sessionKey, _sessionInitVector, _user, this.userName.Text, dialogue));
        }
        private async Task<IEnumerable<StartedDialogue>> GetMyDialogues()
        {
            IEnumerable<StartedDialogue> dialogues = new List<StartedDialogue>();
            string json = JsonSerializer.Serialize<AuthorizationData>(_user, _serializerOptions);
            StringContent sentContent = new StringContent(json, Encoding.UTF8, "application/json");
            string url = DeviceInfo.Platform == DevicePlatform.Android ? $"https://10.0.2.2:5001/api/dialogues/getstarteddialogues/{_sessionId}"
                                                                  : $"https://localhost:5001/api/dialogues/getstarteddialogues/{_sessionId}";
            HttpResponseMessage response = await _client.PostAsync(url, sentContent);
            string content = await response.Content.ReadAsStringAsync();
            dialogues = _crypter.Decrypt(JsonSerializer.Deserialize<List<StartedDialogue>>(content, _serializerOptions), _sessionKey, _sessionInitVector);
            return dialogues;
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
            IsPresented=false;
        }
        private async void SettingsButtonClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(_settingsPage);
            IsPresented = false;
        }
        private async void OutRequestsButtonClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(_outRequestsPage);
            IsPresented = false;
        }
        private async void InRequestsButtonClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(_inRequestsPage);
            IsPresented = false;
        }
        private async void HelpButtonClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(_helpPage);
            IsPresented = false;
        }
        private async void AboutButtonClicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(_aboutPage);
            IsPresented = false;
        }
    }

}
