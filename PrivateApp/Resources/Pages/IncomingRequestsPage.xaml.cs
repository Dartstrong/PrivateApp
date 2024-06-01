using PrivateApp.Resources.Entities;
using System.Text.Json;
using PrivateApp.Resources.HelperClasses;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
namespace PrivateApp.Resources.Pages
{
    public partial class IncomingRequestsPage : ContentPage
    {
        private int _sessionId;
        private byte[] _sessionKey;
        private byte[] _sessionInitVector;
        string _userName;
        private AuthorizationData _user;
        private Crypter _crypter;
        private Converter _converter;
        private Page _settingsPage;
        private Page _outRequestsPage;
        private HttpClient _client;
        private JsonSerializerOptions _serializerOptions;
        public IncomingRequestsPage(int sessionId, byte[] sessionKey, byte[] sessionInitVector, AuthorizationData user, string userName)
	    {
		    InitializeComponent();
            CreateRestService();
            _sessionId = sessionId;
            _sessionKey = sessionKey;
            _sessionInitVector = sessionInitVector;
            _user = user;
            _userName = userName;
            _crypter = new Crypter();
            _converter = new Converter();
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
            listView.ItemsSource = await GetMyRequests();
            listView.ItemTemplate = new DataTemplate(() =>
            {
                Label nameLabel = new Label()
                {
                    FontSize = 16,
                    TextColor = Color.FromHex("#7AF4BA")
                };
                nameLabel.SetBinding(Label.TextProperty, "Receiver");
                Label idLabel = new Label()
                {
                    FontSize = 10,
                    TextColor = Colors.Grey
                };
                idLabel.SetBinding(Label.TextProperty, "IdStr");
                return new ViewCell
                {
                    View = new VerticalStackLayout
                    {
                        Padding = new Thickness(15, 5),
                        BackgroundColor = Color.FromHex("#282828"),
                        Children = { nameLabel, idLabel }
                    }
                };
            });
        }

        private async Task<IEnumerable<DialogueRequest>> GetMyRequests()
        {
            IEnumerable<DialogueRequest> dialogues = new List<DialogueRequest>();
            string json = JsonSerializer.Serialize<AuthorizationData>(_user, _serializerOptions);
            StringContent sentContent = new StringContent(json, Encoding.UTF8, "application/json");
            string url = DeviceInfo.Platform == DevicePlatform.Android ? $"https://10.0.2.2:5001/api/dialogues/getoutcomingdialogues/{_sessionId}"
                                                                  : $"https://localhost:5001/api/dialogues/getoutcomingdialogues/{_sessionId}";
            HttpResponseMessage response = await _client.PostAsync(url, sentContent);
            string content = await response.Content.ReadAsStringAsync();
            dialogues = _crypter.Decrypt(JsonSerializer.Deserialize<List<DialogueRequest>>(content, _serializerOptions), _sessionKey, _sessionInitVector);
            return dialogues;
        }
        private void BackButtonClicked(object sender, System.EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}