using PrivateApp.Resources.HelperClasses;
using PrivateApp.Resources.Entities;
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
            string deviceId = await SecureStorage.Default.GetAsync("deviceId");
            if (deviceId == null)
            {
                _crypter = new();
                _newDeviceId = await GetDeviceID();
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
            string received�ontent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<NewDeviceID>(received�ontent, _serializerOptions);
        }
        private async void RegistrationButtonClicked(object sender, System.EventArgs e)
        {
            if (loginField.Text == null)
            {
                loginField.Placeholder = "������� �����";
            }
            else if (passwordField.Text == null)
            {
                passwordField.Placeholder = "������� ������";
            }
            else if (emailField.Text == null)
            {
                emailField.Placeholder = "������� e-mail";
            }
            else
            {
                int result = await RegistarationAttempt();
                if (result == 409) await DisplayAlert("�����������", "������ ������������ ��� ���������������", "�K");
                else if (result == 200)
                {
                    await Navigation.PushAsync(new MainPage(_sessionId, _sessionKey, _sessionInitVector, _user, loginField.Text, _deviceId));
                    Navigation.RemovePage(this);
                }
                else await DisplayAlert("�����������", "��������� ����������� � ��������� � ��������� �������", "�K");
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
            if (_user.DeviceIdStr == null) await DisplayAlert("�����������", "��������� ����������� � ��������� � ��������� �������", "�K");
            else Navigation.RemovePage(this);
            
        }
    }
}
