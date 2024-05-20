using PrivateApp.Resources.Entity;
using PrivateApp.Resources.Models;
using PrivateApp.Resources.HelperClasses;
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
        RSAParameters _privateKey;
        private string _fileName=Path.Combine(FileSystem.Current.AppDataDirectory, "AppPassword.dat");
        public AppLoginPage()
		{
			InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            RSA rsa = RSA.Create();
            RSAPublicKey _publicKey = new RSAPublicKey(rsa.ExportParameters(false));
            _privateKey = rsa.ExportParameters(true);
            CreateRestService();
            StartSession(_publicKey); 
            mainLabel.Text = new FileInfo(_fileName).Exists ? "Введите пароль" : "Придумайте пароль";
            _password = [];
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
        private async void StartSession(RSAPublicKey publicKey)
        {
            string json = JsonSerializer.Serialize<RSAPublicKey>(publicKey, _serializerOptions);
            string url = DeviceInfo.Platform == DevicePlatform.Android ? "https://10.0.2.2:5001/api/session/" : "https://localhost:5001/api/session/";
            StringContent sentContent = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync(url, sentContent);
            if (response.IsSuccessStatusCode)
            {
                string receivedСontent = await response.Content.ReadAsStringAsync();
                _session =  JsonSerializer.Deserialize<Session>(receivedСontent, _serializerOptions);
            }
        }
        private void ButtonClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            _password.Add(Convert.ToInt32(button.Text));
            switch(_password.Count)
            {
                case 1: passwordField1.TextColor = Color.FromArgb("#7AF4BA"); break;
                case 2: passwordField2.TextColor = Color.FromArgb("#7AF4BA"); break;
                case 3: passwordField3.TextColor = Color.FromArgb("#7AF4BA"); break;
                case 4: passwordField4.TextColor = Color.FromArgb("#7AF4BA"); break;
                case 5: passwordField5.TextColor = Color.FromArgb("#7AF4BA"); PasswordCheck(_privateKey); break;
            }
        }
        private void ErraseButtonClicked(object sender, EventArgs e)
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
        private async void PasswordCheck(RSAParameters privateKey)
        {
            int password = 0;
            foreach (var a in _password)
            {
                password = password * 10 + a;
            }
            byte[] passwordHash = MD5.HashData(BitConverter.GetBytes(password));
            if (!new FileInfo(_fileName).Exists)
            {
                if (_session != null)
                {
                    using (BinaryWriter writer = new(File.Open(_fileName, FileMode.OpenOrCreate)))
                    {
                        writer.Write(passwordHash);
                        writer.Close();
                        Crypter crypter = new();
                        Converter converter = new();
                        await Navigation.PushAsync(new LoginPage(_session.Id, crypter.Decrypt(converter.StrToIntArrayToByteArray(_session.SymmetricKey), privateKey), crypter.Decrypt(converter.StrToIntArrayToByteArray(_session.InitVector), privateKey)), false);
                    }
                }
                else
                {
                    ClearPassword();
                    await DisplayAlert("Уведомление", "Проверьте подключение к интернету и повторите попытку", "ОK");
                    StartSession(_publicKey);
                }
            }
            else
            {
                bool error = false;
                using (BinaryReader reader = new(File.Open(_fileName, FileMode.OpenOrCreate)))
                {
                    byte[] savedHash = reader.ReadBytes(32);
                    reader.Close();
                    for(int i = 0; i < savedHash.Length; i++)
                    {
                        if (savedHash[i] != passwordHash[i])
                        {
                            error = true;
                            break;
                        }
                    }
                    if(!error) 
                    {
                        if (_session != null)
                        {
                            Crypter crypter = new();
                            Converter converter = new();
                            await Navigation.PushAsync(new LoginPage(_session.Id, crypter.Decrypt(converter.StrToIntArrayToByteArray(_session.SymmetricKey), privateKey), crypter.Decrypt(converter.StrToIntArrayToByteArray(_session.InitVector), privateKey)), false);
                        }
                        else
                        {
                            ClearPassword();
                            await DisplayAlert("Уведомление", "Проверьте подключение к интернету и повторите попытку", "ОK");
                            StartSession(_publicKey);
                        }
                    }
                    else
                    {
                        mainLabel.Text = "Неверный пароль, повторите попытку";
                        ClearPassword();
                    }      
                }
            }
        }
        private void ClearPassword()
        {
            _password.Clear();
            passwordField1.TextColor = Colors.Grey;
            passwordField2.TextColor = Colors.Grey;
            passwordField3.TextColor = Colors.Grey;
            passwordField4.TextColor = Colors.Grey;
            passwordField5.TextColor = Colors.Grey;
        }
    }
}

