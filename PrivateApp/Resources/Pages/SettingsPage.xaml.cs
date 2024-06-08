using PrivateApp.Resources.Entities;
using PrivateApp.Resources.HelperClasses;
using System.Text;
using System.Text.Json;
namespace PrivateApp
{
	public partial class SettingsPage : ContentPage
	{
        private string _fileName = Path.Combine(FileSystem.Current.AppDataDirectory, "AppPassword.dat");
        private int _sessionId;
        private byte[] _sessionKey;
        private byte[] _sessionInitVector;
        private AuthorizationData _user;
        private HttpClient _client;
        private JsonSerializerOptions _serializerOptions;
        private Crypter _crypter;
        public SettingsPage(int sessionId, byte[] sessionKey, byte[] sessionInitVector, AuthorizationData user, string userName, string deviceId)
		{
			InitializeComponent();
			this.userName.Text = $"Логин аккаунта: {userName}";
			this.deviceId.Text = $"Уникальный ID устройства: {deviceId}";
            this.sessionId.Text = $"ID текущей сессии: {sessionId}";
            _sessionId = sessionId;
            _sessionKey = sessionKey;
            _sessionInitVector = sessionInitVector;
            _user = user;
            _crypter = new();
            CreateRestService();
            new Timer(new TimerCallback(delegate { LoadingContent(); }), null, 0, 5000);
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
            listView.ItemsSource = await GetLoginHistory();
            listView.SelectionMode = ListViewSelectionMode.None;
            listView.ItemTemplate = new DataTemplate(() =>
            {
                ImageCell imageCell = new ImageCell
                {
                    TextColor = Color.FromHex("#7AF4BA"),
                    DetailColor = Colors.Grey,
                    ImageSource = ImageSource.FromFile("login_history_icon.svg"),
                };
                imageCell.SetBinding(ImageCell.TextProperty, "DeviceIdStr");
                imageCell.SetBinding(ImageCell.DetailProperty, "Date");
                return imageCell;
            });
            Device.BeginInvokeOnMainThread(() =>
            {
                loginHistory.Content = new StackLayout { Children = { listView } };
            });
        }
        private async Task<IEnumerable<LoginEntry>> GetLoginHistory()
        {
            IEnumerable<LoginEntry> loginEntries = new List<LoginEntry>();
            string json = JsonSerializer.Serialize<AuthorizationData>(_user, _serializerOptions);
            StringContent sentContent = new StringContent(json, Encoding.UTF8, "application/json");
            string url = DeviceInfo.Platform == DevicePlatform.Android ? $"https://10.0.2.2:5001/api/autorization/getloginentries/{_sessionId}"
                                                                  : $"https://localhost:5001/api/autorization/ggetloginentries/{_sessionId}";
            HttpResponseMessage response = await _client.PostAsync(url, sentContent);
            string content = await response.Content.ReadAsStringAsync();
            loginEntries = _crypter.Decrypt(JsonSerializer.Deserialize<List<LoginEntry>>(content, _serializerOptions), _sessionKey, _sessionInitVector);
            return loginEntries;
        }
        private void BackButtonClicked(object sender, System.EventArgs e)
        {
            Navigation.PopAsync();
        }
        private async void ClearPasswordButtonClicked(object sender, System.EventArgs e)
        {
            File.Delete(_fileName);
            await DisplayAlert("Уведомление", "Пароль успешно сброшен", "ОK");
        }
        private async void RemoveKeysButtonClicked(object sender, System.EventArgs e)
        {
            SecureStorage.Default.RemoveAll();
            await DisplayAlert("Уведомление", "Хранилище успешно очищено", "ОK");
        }
    }
}

