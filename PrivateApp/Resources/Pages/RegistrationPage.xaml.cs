using PrivateApp.Resources.HelperClasses;
using PrivateApp.Resources.Entity;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
namespace PrivateApp
{
	public partial class RegistrationPage : ContentPage
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
        private string _fileName = Path.Combine(FileSystem.Current.AppDataDirectory, "DeviceInfo.dat");
        private string _deviceId;
        public RegistrationPage(int sessionId, byte[] sessionKey, byte[] sessionInitVector)
		{
			InitializeComponent();
            CreateRestService();
            _sessionId = sessionId;
            _sessionKey = sessionKey;
            _sessionInitVector = sessionInitVector;
            _crypter = new Crypter();
            _converter = new Converter();
            _user = new AuthorizationData();
            DeviceIdCheck();
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
        private async void DeviceIdCheck()
        {
            if (!new FileInfo(_fileName).Exists)
            {
                _crypter = new();
                _newDeviceId = await GetDeviceID();
                using (BinaryWriter writer = new BinaryWriter(File.Open(_fileName, FileMode.OpenOrCreate)))
                {
                    writer.Write(BitConverter.GetBytes(_crypter.Decrypt(_newDeviceId, _sessionKey, _sessionInitVector)));
                    writer.Close();
                }
                using (BinaryReader reader = new BinaryReader(File.Open(_fileName, FileMode.OpenOrCreate)))
                {
                    _deviceId = reader.ReadInt16().ToString();
                    _user.DeviceIdStr = _crypter.Encrypt(_deviceId, _sessionKey, _sessionInitVector);
                    reader.Close();
                }
            }
            else
            {
                using (BinaryReader reader = new BinaryReader(File.Open(_fileName, FileMode.OpenOrCreate)))
                {
                    _deviceId = reader.ReadInt16().ToString();
                    _user.DeviceIdStr = _crypter.Encrypt(_deviceId, _sessionKey, _sessionInitVector);
                    reader.Close();
                }
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
        private async void RegistrationButtonClicked(object sender, System.EventArgs e)
        {
            if (loginField.Text == null)
            {
                loginField.Placeholder = "Введите логин";
            }
            else if (passwordField.Text == null)
            {
                passwordField.Placeholder = "Введите пароль";
            }
            else if (emailField.Text == null)
            {
                emailField.Placeholder = "Введите e-mail";
            }
            else
            {
                int result = await RegistarationAttempt();
                if (result == 409) await DisplayAlert("Уведомление", "Данный пользователь уже зарегистрирован", "ОK");
                else if (result == 200) await Navigation.PushAsync(new MainPage(_sessionId, _sessionKey, _sessionInitVector, _user, loginField.Text, _deviceId));
                else await DisplayAlert("Уведомление", "Проверьте подключение к интернету и повторите попытку", "ОK");
            }
        }
        private async Task<int> RegistarationAttempt()
        {
            _user.LoginStr = _crypter.Encrypt(loginField.Text, _sessionKey, _sessionInitVector);
            _user.PasswordStr = _crypter.Encrypt(_converter.ByteArrayToIntArrayToStr(MD5.HashData(Encoding.ASCII.GetBytes(passwordField.Text))), _sessionKey, _sessionInitVector);
            _user.EmailStr = _crypter.Encrypt(emailField.Text, _sessionKey, _sessionInitVector); ;
            string json = JsonSerializer.Serialize<AuthorizationData>(_user, _serializerOptions);
            StringContent sentContent = new StringContent(json, Encoding.UTF8, "application/json");
            string url = DeviceInfo.Platform == DevicePlatform.Android ? $"https://10.0.2.2:5001/api/autorization/newuser/{_sessionId}"
                                                                  : $"https://localhost:5001/api/autorization/newuser/{_sessionId}";
            HttpResponseMessage response = await _client.PostAsync(url, sentContent);
            return Convert.ToInt16(response.StatusCode);
        }
        private async void LoginPageButtonClicked(object sender, System.EventArgs e)
        {
            if (_user.DeviceIdStr == null) await DisplayAlert("Уведомление", "Проверьте подключение к интернету и повторите попытку", "ОK");
            else await Navigation.PushAsync(new LoginPage(_sessionId, _sessionKey, _sessionInitVector));
        }
    }
}
