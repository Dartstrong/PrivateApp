using Microsoft.Maui.Storage;
using PrivateApp.Resources.Entity;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace PrivateApp
{

	public partial class AppLoginPage : ContentPage
	{
        private HttpClient _client;
        private JsonSerializerOptions _serializerOptions;
        private RSAPublicKey _publicKey;
        private Session _session;
        private List<int> _password;
        private string _fileName = Path.Combine(FileSystem.Current.AppDataDirectory, "MainSettings.dat");
        public AppLoginPage()
		{
			InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            RSA rsa = RSA.Create();
            RSAParameters rsaParameters = rsa.ExportParameters(false);
            CreateRestService();
            _publicKey = new RSAPublicKey(rsaParameters);
            StartSession(_publicKey);

            mainLabel.Text = new FileInfo(_fileName).Exists ? "������� ������" : "���������� ������";
            _password = new List<int>();
        }
        void CreateRestService()
        {
            _client = new HttpClientService().GetPlatformSpecificHttpClient();
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }
        private async void StartSession(RSAPublicKey publicKey)
        {
            string json = JsonSerializer.Serialize<RSAPublicKey>(publicKey, _serializerOptions);
            string apiAddress = DeviceInfo.Platform == DevicePlatform.Android ? "https://10.0.2.2:5001/api/session/" : "https://localhost:5001/api/session/";
            StringContent sentContent = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync(apiAddress, sentContent);
            string received�ontent = await response.Content.ReadAsStringAsync();
            _session = JsonSerializer.Deserialize<Session>(received�ontent, _serializerOptions);
        }
        private void ButtonClicked(object sender, System.EventArgs e)
        {
            Button button = (Button)sender;
            _password.Add(Convert.ToInt32(button.Text));
            switch(_password.Count)
            {
                case 1: passwordField1.TextColor = Color.FromArgb("#7AF4BA"); break;
                case 2: passwordField2.TextColor = Color.FromArgb("#7AF4BA"); break;
                case 3: passwordField3.TextColor = Color.FromArgb("#7AF4BA"); break;
                case 4: passwordField4.TextColor = Color.FromArgb("#7AF4BA"); break;
                case 5: passwordField5.TextColor = Color.FromArgb("#7AF4BA"); PasswordCheck(); break;
            }
        }
        private void ErraseButtonClicked(object sender, System.EventArgs e)
        {
            if(_password.Count > 0) _password.RemoveAt(_password.Count-1);
            switch (_password.Count)
            {
                case 0: passwordField1.TextColor = Colors.Grey; break;
                case 1: passwordField2.TextColor = Colors.Grey; break;
                case 2: passwordField3.TextColor = Colors.Grey; break;
                case 3: passwordField4.TextColor = Colors.Grey; break;
                case 4: passwordField5.TextColor = Colors.Grey; break;
            }
        }
        private async void PasswordCheck()
        {
            int password = 0;
            foreach (var a in _password)
            {
                password = password * 10 + a;
            }
            byte[] passwordHash = MD5.HashData(BitConverter.GetBytes(password));
            if (!new FileInfo(_fileName).Exists)
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(_fileName, FileMode.OpenOrCreate)))
                {
                    writer.Write(MD5.HashData(passwordHash));
                }
            }
            else
            {
                bool error = false;
                using (BinaryReader reader = new BinaryReader(File.Open(_fileName, FileMode.OpenOrCreate)))
                {
                    byte[] savedHash = reader.ReadBytes(32);
                    for(int i = 0; i < savedHash.Length; i++)
                    {
                        if (savedHash[i] != passwordHash[i]) error = true;
                    }
                    if(!error) 
                    {
                        if (_session == null)
                        {
                            _password.Clear();
                            passwordField1.TextColor = Colors.Grey;
                            passwordField2.TextColor = Colors.Grey;
                            passwordField3.TextColor = Colors.Grey;
                            passwordField4.TextColor = Colors.Grey;
                            passwordField5.TextColor = Colors.Grey;
                            await DisplayAlert("�����������", "��������� ����������� � ��������� � ��������� �������", "�K");
                        }
                        else await Navigation.PushModalAsync(new LoginPage(_session));
                    }
                    else
                    {
                        _password.Clear();
                        mainLabel.Text = "�������� ������, ��������� �������";
                        passwordField1.TextColor = Colors.Grey;
                        passwordField2.TextColor = Colors.Grey;
                        passwordField3.TextColor = Colors.Grey;
                        passwordField4.TextColor = Colors.Grey;
                        passwordField5.TextColor = Colors.Grey; 
                     }      
                }
            }
        }
    }
}

