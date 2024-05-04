using PrivateApp.Resources.Entity;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace PrivateApp
{
    public partial class LoginPage : ContentPage
	{        
        HttpClient _client;
        JsonSerializerOptions _serializerOptions;
        RSAPublicKey _publicKey;
        Session _session;
		public LoginPage(Session session)
		{
			InitializeComponent();
            RSA rsa = RSA.Create();
            RSAParameters rsaParameters = rsa.ExportParameters(false);
            CreateRestService();
            _publicKey = new RSAPublicKey(rsaParameters);
            StartSession(_publicKey);

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
        private async void EntryButtonClicked(object sender, System.EventArgs e)
        {
           StartSession(_publicKey);
        }

        async void StartSession(RSAPublicKey publicKey)
        {
            string json = JsonSerializer.Serialize<RSAPublicKey>(publicKey, _serializerOptions);
            string apiAddress = DeviceInfo.Platform == DevicePlatform.Android ? "https://10.0.2.2:5001/api/session/" : "https://localhost:5001/api/session/";
            StringContent sentContent = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync(apiAddress, sentContent);
            string received—ontent = await response.Content.ReadAsStringAsync();
            _session = JsonSerializer.Deserialize<Session>(received—ontent, _serializerOptions);
        }
    }
}
