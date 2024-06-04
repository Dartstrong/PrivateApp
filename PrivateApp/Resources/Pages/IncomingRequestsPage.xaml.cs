using PrivateApp.Resources.Entities;
using System.Text.Json;
using PrivateApp.Resources.HelperClasses;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
namespace PrivateApp
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
            listView.SelectionMode = ListViewSelectionMode.None;
            listView.ItemTapped += ItemTapped;
            listView.ItemTemplate = new DataTemplate(() =>
            {
                ImageCell imageCell = new ImageCell
                {
                    TextColor = Color.FromHex("#7AF4BA"),
                    DetailColor = Colors.Grey,
                    ImageSource = ImageSource.FromFile("incoming_request_icon.svg"),
                };
                imageCell.SetBinding(ImageCell.TextProperty, "Sender");
                imageCell.SetBinding(ImageCell.DetailProperty, "IdStr");
                return imageCell;
            });
            mainContent.Content = new StackLayout { Children = { listView } };
        }
        private async void ItemTapped(object? sender, ItemTappedEventArgs e)
        {
            DialogueRequest request = (DialogueRequest)e.Item;
            bool result = await DisplayAlert("Подтвердить действие", $"Вы хотите принять заявку от пользователя {request.Sender}, id заявки {request.IdStr}?", "Да", "Нет");
            if (result)
            {
                if (await AcceptRequestDialogue(Convert.ToInt32(request.IdStr)) == 200)
                {
                    LoadingContent();
                }
            }
        }
        private async Task<int> AcceptRequestDialogue(int dialogueId)
        {
            RSA rsa = RSA.Create();
            RSAPublicKey publicKey = new RSAPublicKey(rsa.ExportParameters(false));
            RequestAcceptDialogue requestAcceptDialogue = new()
            {
                Accepted = _user.LoginStr,
                AcceptedPassword = _user.PasswordStr,
                AcceptedDeviceId = _user.DeviceIdStr,
                PublicKeyModulus = _crypter.Encrypt(publicKey.ModulusStr, _sessionKey, _sessionInitVector),
                PublicKeyExponent = _crypter.Encrypt(publicKey.ExponentStr, _sessionKey, _sessionInitVector)
            };

            string json = JsonSerializer.Serialize<RequestAcceptDialogue>(requestAcceptDialogue, _serializerOptions);
            StringContent sentContent = new StringContent(json, Encoding.UTF8, "application/json");
            string url = DeviceInfo.Platform == DevicePlatform.Android ? $"https://10.0.2.2:5001/api/dialogues/acceptindialogue/{dialogueId}/{_sessionId}"
                                                                  : $"https://localhost:5001/api/dialogues/acceptindialogue/{dialogueId}/{_sessionId}";
            HttpResponseMessage response = await _client.PostAsync(url, sentContent);
            if (Convert.ToInt16(response.StatusCode) == 200)
            {
                string receivedСontent = await response.Content.ReadAsStringAsync();
                RSAParameters rsaParameters = rsa.ExportParameters(true);
                await SecureStorage.Default.SetAsync($"outRequestD/{_userName}/{dialogueId}", _converter.ByteArrayToIntArrayToStr(rsaParameters.D));
                await SecureStorage.Default.SetAsync($"outRequestDP/{_userName}/{dialogueId}", _converter.ByteArrayToIntArrayToStr(rsaParameters.DP));
                await SecureStorage.Default.SetAsync($"outRequestDQ/{_userName}/{dialogueId}", _converter.ByteArrayToIntArrayToStr(rsaParameters.DQ));
                await SecureStorage.Default.SetAsync($"outRequestExponent/{_userName}/{dialogueId}", _converter.ByteArrayToIntArrayToStr(rsaParameters.Exponent));
                await SecureStorage.Default.SetAsync($"outRequestInverseQ/{_userName}/{dialogueId}", _converter.ByteArrayToIntArrayToStr(rsaParameters.InverseQ));
                await SecureStorage.Default.SetAsync($"outRequestModulus/{_userName}/{dialogueId}", _converter.ByteArrayToIntArrayToStr(rsaParameters.Modulus));
                await SecureStorage.Default.SetAsync($"outRequestP/{_userName}/{dialogueId}", _converter.ByteArrayToIntArrayToStr(rsaParameters.P));
                await SecureStorage.Default.SetAsync($"outRequestQ/{_userName}/{dialogueId}", _converter.ByteArrayToIntArrayToStr(rsaParameters.Q));
            }
            return Convert.ToInt16(response.StatusCode);
        }

        private async Task<IEnumerable<DialogueRequest>> GetMyRequests()
        {
            IEnumerable<DialogueRequest> dialogues = new List<DialogueRequest>();
            string json = JsonSerializer.Serialize<AuthorizationData>(_user, _serializerOptions);
            StringContent sentContent = new StringContent(json, Encoding.UTF8, "application/json");
            string url = DeviceInfo.Platform == DevicePlatform.Android ? $"https://10.0.2.2:5001/api/dialogues/getincomingdialogues/{_sessionId}"
                                                                  : $"https://localhost:5001/api/dialogues/getincomingdialogues/{_sessionId}";
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