using PrivateApp.Resources.Entities;
using System.Text.Json;
using PrivateApp.Resources.HelperClasses;
using System.Text;
using System.Security.Cryptography;
namespace PrivateApp
{
	public partial class DialoguePage : ContentPage
	{

        private int _sessionId;
        private byte[] _sessionKey;
        private byte[] _sessionInitVector;
        private string _userName;
        private AuthorizationData _user;
        private Crypter _crypter;
        private Converter _converter;
        private HttpClient _client;
        private StartedDialogue _startedDialogue;
        private RSAParameters _rsaParameters;
        private JsonSerializerOptions _serializerOptions;
        public DialoguePage(int sessionId, byte[] sessionKey, byte[] sessionInitVector, AuthorizationData user, string userName, StartedDialogue startedDialogue)
		{
			InitializeComponent();
            CreateRestService();
            _sessionId = sessionId;
            _sessionKey = sessionKey;
            _sessionInitVector = sessionInitVector;
            _user = user;
            _userName = userName;
            _startedDialogue = startedDialogue;
            _crypter = new Crypter();
            _converter = new Converter();
            _rsaParameters = new RSAParameters();
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
            _rsaParameters.D = _converter.StrToIntArrayToByteArray(await SecureStorage.Default.GetAsync($"outRequestD/{_userName}/{_startedDialogue.IdStr}"));
            _rsaParameters.DP = _converter.StrToIntArrayToByteArray(await SecureStorage.Default.GetAsync($"outRequestDP/{_userName}/{_startedDialogue.IdStr}"));
            _rsaParameters.DQ = _converter.StrToIntArrayToByteArray(await SecureStorage.Default.GetAsync($"outRequestDQ/{_userName}/{_startedDialogue.IdStr}"));
            _rsaParameters.Exponent = _converter.StrToIntArrayToByteArray(await SecureStorage.Default.GetAsync($"outRequestExponent/{_userName}/{_startedDialogue.IdStr}"));
            _rsaParameters.InverseQ = _converter.StrToIntArrayToByteArray(await SecureStorage.Default.GetAsync($"outRequestInverseQ/{_userName}/{_startedDialogue.IdStr}"));
            _rsaParameters.Modulus= _converter.StrToIntArrayToByteArray(await SecureStorage.Default.GetAsync($"outRequestModulus/{_userName}/{_startedDialogue.IdStr}"));
            _rsaParameters.P = _converter.StrToIntArrayToByteArray(await SecureStorage.Default.GetAsync($"outRequestP/{_userName}/{_startedDialogue.IdStr}"));
            _rsaParameters.Q = _converter.StrToIntArrayToByteArray(await SecureStorage.Default.GetAsync($"outRequestQ/{_userName}/{_startedDialogue.IdStr}"));
            List<CustomMessage> messages = (List<CustomMessage>)await GetMyMessages();
            StackLayout mainStack = new StackLayout();
            foreach (var message in messages)
            {
                Label authorLabel = new()
                {
                    Text = (message.My) ? "You :" : $"{_startedDialogue.Receiver} :",
                    FontSize = 15,
                    TextColor = Colors.Gray,
                    HorizontalTextAlignment = (message.My) ? TextAlignment.End : TextAlignment.Start
                };
                Label messageLabel = new()
                {
                    Text = message.Data,
                    FontSize = 20,
                    TextColor = Color.FromHex("#7AF4BA"),
                    HorizontalTextAlignment = (message.My)? TextAlignment.End : TextAlignment.Start
                };
                mainStack.Children.Add(authorLabel);
                mainStack.Children.Add(messageLabel);
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                mainContent.Content = mainStack;
            });
        }
        private async Task<IEnumerable<CustomMessage>> GetMyMessages()
        {
            IEnumerable<CustomMessage> messages = new List<CustomMessage>();
            string json = JsonSerializer.Serialize<AuthorizationData>(_user, _serializerOptions);
            StringContent sentContent = new StringContent(json, Encoding.UTF8, "application/json");
            string url = DeviceInfo.Platform == DevicePlatform.Android ? $"https://10.0.2.2:5001/api/dialogues/getdialoguemes/{_startedDialogue.IdStr}/{_sessionId}"
                                                                  : $"https://localhost:5001/api/dialogues/ggetdialoguemes/{_startedDialogue.IdStr}/{_sessionId}";
            HttpResponseMessage response = await _client.PostAsync(url, sentContent);
            string content = await response.Content.ReadAsStringAsync();
            messages = _crypter.Decrypt(JsonSerializer.Deserialize<List<CustomMessage>>(content, _serializerOptions), _sessionKey, _sessionInitVector, _rsaParameters);
            return messages;
        }
        private void BackButtonClicked(object sender, System.EventArgs e)
        {
            Navigation.RemovePage(this);
        }

        private async void EntryCompleted(object sender, EventArgs e)
        {
            NewMessage newMessage = new()
            {
                LoginStr = _user.LoginStr,
                PasswordStr = _user.PasswordStr,
                DeviceIdStr = _user.DeviceIdStr,
                SenderData = _crypter.Encrypt( _crypter.Encrypt(myMessage.Text, _rsaParameters), _sessionKey, _sessionInitVector),
                ReceiverData = _crypter.Encrypt( _crypter.Encrypt(myMessage.Text, new RSAParameters
                {
                    Exponent = _converter.StrToIntArrayToByteArray(_startedDialogue.PublicKeyExponent),
                    Modulus = _converter.StrToIntArrayToByteArray(_startedDialogue.PublicKeyModulus)
                }), _sessionKey, _sessionInitVector)
            };
            string json = JsonSerializer.Serialize<NewMessage>(newMessage, _serializerOptions);
            StringContent sentContent = new StringContent(json, Encoding.UTF8, "application/json");
            string url = DeviceInfo.Platform == DevicePlatform.Android ? $"https://10.0.2.2:5001/api/dialogues/createdialoguemes/{_startedDialogue.IdStr}/{_sessionId}"
                                                                  : $"https://localhost:5001/api/dialogues/createdialoguemes/{_startedDialogue.IdStr}/{_sessionId}";
            await _client.PostAsync(url, sentContent);
            myMessage.Text = "";
            LoadingContent();
        }
    }
}

