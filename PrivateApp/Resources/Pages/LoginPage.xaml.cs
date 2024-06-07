using PrivateApp.Resources.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using PrivateApp.Resources.HelperClasses;
namespace PrivateApp
{
    public partial class LoginPage : ContentPage
    {
        private HttpClient _client;
        private JsonSerializerOptions _serializerOptions;
        private NewDeviceID _newDeviceId;
        private AuthorizationData _user;
        private Crypter _crypter;
        private Converter _converter;
        private int _sessionId;
        private byte[] _sessionKey;
        private byte[] _sessionInitVector;
        private string _deviceId;
        public LoginPage(int sessionId, byte[] sessionKey, byte[] sessionInitVector)
        {
            InitializeComponent();
            CreateRestService();
            _sessionId = sessionId;
            _sessionKey = sessionKey;
            _sessionInitVector = sessionInitVector;
            _crypter = new Crypter();
            _converter = new Converter();
            _user = new AuthorizationData();
            NavigationPage.SetHasNavigationBar(this, false);
            UserInfoStorage();
        }
        protected override bool OnBackButtonPressed()
        {
            return true;
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
        private async void UserInfoStorage()
        {
            string login = await SecureStorage.Default.GetAsync("login");
            string password = await SecureStorage.Default.GetAsync("password");
            if ((login!=null) &&(password!=null))
            {
                loginField.Text = login;
                passwordField.Text = password;
                rememberMe.IsToggled = true;
                await DeviceIdCheck();
                EntryButtonClicked(null, null);
            }
            else await DeviceIdCheck();
        }
        private async Task DeviceIdCheck()
        {
            string deviceId = await SecureStorage.Default.GetAsync("deviceId");
            if (deviceId==null)
            {
                _crypter = new();
                _newDeviceId =  await GetDeviceID();
                await SecureStorage.Default.SetAsync("deviceId", _crypter.Decrypt(_newDeviceId, _sessionKey, _sessionInitVector).ToString());
                _deviceId = await SecureStorage.Default.GetAsync("deviceId");
                _user.DeviceIdStr = _crypter.Encrypt(_deviceId, _sessionKey, _sessionInitVector);
            }
            else
            {
                _deviceId = await SecureStorage.Default.GetAsync("deviceId");
                _user.DeviceIdStr = _crypter.Encrypt(_deviceId, _sessionKey, _sessionInitVector);
            }
        }
        private async Task<NewDeviceID> GetDeviceID()
        {
            string url = DeviceInfo.Platform == DevicePlatform.Android ? $"https://10.0.2.2:5001/api/autorization/newdevice/{_sessionId}" 
                                                                              : $"https://localhost:5001/api/autorization/newdevice/{_sessionId}";
            HttpResponseMessage response = await _client.GetAsync(url);
            string receivedСontent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<NewDeviceID>(receivedСontent, _serializerOptions);
        }
        private async void EntryButtonClicked(object? sender, System.EventArgs? e)
        {
            if(loginField.Text == null) 
            {
                loginField.Placeholder = "Введите логин";
            }
            else if(passwordField.Text == null)
            {
                passwordField.Placeholder = "Введите пароль";
            }
            else
            {
                int result = await AuthorizationAttempt();
                if (result == 401) await DisplayAlert("Уведомление", "Данный пользователь не зарегистрирован", "ОK");
                else if (result == 403) await DisplayAlert("Уведомление", "Неверный пароль", "ОK");
                else if (result == 200)
                {
                    if (rememberMe.IsToggled)
                    {
                        SecureStorage.Default.Remove("login");
                        await SecureStorage.Default.SetAsync("login", loginField.Text);
                        SecureStorage.Default.Remove("password");
                        await SecureStorage.Default.SetAsync("password", passwordField.Text);
                    }
                    else
                    {
                        SecureStorage.Default.Remove("login");
                        SecureStorage.Default.Remove("password");
                    }
                    await Navigation.PushAsync(new MainPage(_sessionId, _sessionKey, _sessionInitVector, _user, loginField.Text, _deviceId));
                }
                else await DisplayAlert("Уведомление", "Проверьте подключение к интернету и повторите попытку", "ОK");
            }
        }
        private async Task<int> AuthorizationAttempt()
        {
            _user.LoginStr = _crypter.Encrypt(loginField.Text, _sessionKey, _sessionInitVector);
            _user.PasswordStr = _crypter.Encrypt(_converter.ByteArrayToIntArrayToStr(MD5.HashData(Encoding.ASCII.GetBytes(passwordField.Text))),  _sessionKey, _sessionInitVector);
            _user.EmailStr = null;
            string json = JsonSerializer.Serialize<AuthorizationData>(_user, _serializerOptions);
            StringContent sentContent = new StringContent(json, Encoding.UTF8, "application/json");
            string url = DeviceInfo.Platform == DevicePlatform.Android ? $"https://10.0.2.2:5001/api/autorization/entry/{_sessionId}"
                                                                  : $"https://localhost:5001/api/autorization/entry/{_sessionId}";
            HttpResponseMessage response = await _client.PostAsync(url, sentContent);
            return Convert.ToInt16(response.StatusCode);
        }
        private async void RegistrationButtonClicked(object sender, System.EventArgs e)
        {
            if(_user.DeviceIdStr == null) await DisplayAlert("Уведомление", "Проверьте подключение к интернету и повторите попытку", "ОK");
            else await Navigation.PushAsync(new RegistrationPage(_sessionId, _sessionKey, _sessionInitVector));
        }
    }
}
