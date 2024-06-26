using PrivateApp.Resources.Entities;
using System.Text.Json;
using PrivateApp.Resources.HelperClasses;
using System.Text;
using System.Security.Cryptography;
namespace PrivateApp
{
	public partial class OutcomingRequestsPage : ContentPage
	{
        private int _sessionId;
        private byte[] _sessionKey;
        private byte[] _sessionInitVector;
        string _userName;
        private AuthorizationData _user;
        private Crypter _crypter;
        private Converter _converter;
        private HttpClient _client;
        private JsonSerializerOptions _serializerOptions;
        public OutcomingRequestsPage(int sessionId, byte[] sessionKey, byte[] sessionInitVector, AuthorizationData user, string userName)
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
            listView.ItemsSource = await GetMyRequests();
            listView.SelectionMode = ListViewSelectionMode.None;
            listView.ItemTapped += ItemTapped;
            listView.ItemTemplate = new DataTemplate(() =>
            {
                ImageCell imageCell = new ImageCell
                {
                    TextColor = Color.FromHex("#7AF4BA"),
                    DetailColor = Colors.Grey,
                    ImageSource = ImageSource.FromFile("outcoming_request_icon.svg"),      
                };
                imageCell.SetBinding(ImageCell.TextProperty, "Receiver");
                imageCell.SetBinding(ImageCell.DetailProperty, "IdStr");
                return imageCell;
            });
            Device.BeginInvokeOnMainThread(() =>
            {
                mainContent.Content = new StackLayout { Children = { listView } };
            });
            
        }
        private async void ItemTapped(object? sender, ItemTappedEventArgs e)
        {
            DialogueRequest request = (DialogueRequest)e.Item;
            bool result = await DisplayAlert("����������� ��������", $"�� ������ �������� �������� ������ ������������ {request.Receiver}, id ������ {request.IdStr}?", "��", "���");
            if (result)
            {
                if (await DeleteRequestDialogue(Convert.ToInt32(request.IdStr)) == 200)
                {
                    LoadingContent();
                    SecureStorage.Default.Remove($"outRequestD/{_userName}/{request.IdStr}");
                    SecureStorage.Default.Remove($"outRequestDP/{_userName}/{request.IdStr}");
                    SecureStorage.Default.Remove($"outRequestDQ/{_userName}/{request.IdStr}");
                    SecureStorage.Default.Remove($"outRequestExponent/{_userName}/{request.IdStr}");
                    SecureStorage.Default.Remove($"outRequestInverseQ/{_userName}/{request.IdStr}");
                    SecureStorage.Default.Remove($"outRequestModulus/{_userName}/{request.IdStr}");
                    SecureStorage.Default.Remove($"outRequestP/{_userName}/{request.IdStr}");
                    SecureStorage.Default.Remove($"outRequestQ/{_userName}/{request.IdStr}");
                }
                    
            }
        }
        private async Task<int> DeleteRequestDialogue(int dialogueId)
        {
            string json = JsonSerializer.Serialize<AuthorizationData>(_user, _serializerOptions);
            StringContent sentContent = new StringContent(json, Encoding.UTF8, "application/json");
            string url = DeviceInfo.Platform == DevicePlatform.Android ? $"https://10.0.2.2:5001/api/dialogues/deleteoutdialogue/{dialogueId}/{_sessionId}"
                                                                  : $"https://localhost:5001/api/dialogues/deleteoutdialogue/{dialogueId}/{_sessionId}";
            HttpResponseMessage response = await _client.PostAsync(url, sentContent);
            return Convert.ToInt16(response.StatusCode);
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
            dialogues = _crypter.Decrypt( JsonSerializer.Deserialize<List<DialogueRequest>>(content, _serializerOptions), _sessionKey, _sessionInitVector);
            return dialogues;
        }
        private async void PlusButtonClicked(object sender, System.EventArgs e)
        {
            var receiverLogin = await DisplayPromptAsync("�������� ������", "������� ����� ������������ � ������� ������ ������  ������:", "OK", "������");
            if (receiverLogin != null)
            {
                int result = await CreatingNewDialogue(receiverLogin);
                if (result != 200)
                {
                    if (result == 401) { await DisplayAlert("�����������", "������ ������������ �� ���������������", "�K"); }
                    else { await DisplayAlert("�����������", "��������� ����������� � ��������� � ��������� �������", "�K"); }
                }
                else
                {
                    LoadingContent();
                }
            }
        }
        private async Task<int>CreatingNewDialogue(string receiver)
        {
            RSA rsa = RSA.Create(); 
            RSAPublicKey publicKey = new RSAPublicKey(rsa.ExportParameters(false));
            RequestStartDialogue request = new()
            {
                Sender = _user.LoginStr,
                SenderPassword = _user.PasswordStr,
                SenderdDeviceId = _user.DeviceIdStr,
                Receiver = _crypter.Encrypt(receiver, _sessionKey, _sessionInitVector),
                PublicKeyModulus = _crypter.Encrypt(publicKey.ModulusStr, _sessionKey, _sessionInitVector),
                PublicKeyExponent = _crypter.Encrypt(publicKey.ExponentStr, _sessionKey, _sessionInitVector)
            };
            string json = JsonSerializer.Serialize<RequestStartDialogue>(request, _serializerOptions);
            StringContent sentContent = new StringContent(json, Encoding.UTF8, "application/json");
            string url = DeviceInfo.Platform == DevicePlatform.Android ? $"https://10.0.2.2:5001/api/dialogues/startdialogue/{_sessionId}"
                                                                  : $"https://localhost:5001/api/dialogues/startdialogue/{_sessionId}";
            HttpResponseMessage response = await _client.PostAsync(url, sentContent);
            if(Convert.ToInt16(response.StatusCode)==200)
            {
                string received�ontent = await response.Content.ReadAsStringAsync();
                DialogueRequest dialogueRequest = JsonSerializer.Deserialize<DialogueRequest>(received�ontent, _serializerOptions);
                RSAParameters rsaParameters = rsa.ExportParameters(true);
                await SecureStorage.Default.SetAsync($"outRequestD/{_userName}/{_crypter.Decrypt(dialogueRequest, _sessionKey, _sessionInitVector)}", _converter.ByteArrayToIntArrayToStr(rsaParameters.D));
                await SecureStorage.Default.SetAsync($"outRequestDP/{_userName}/{_crypter.Decrypt(dialogueRequest, _sessionKey, _sessionInitVector)}", _converter.ByteArrayToIntArrayToStr(rsaParameters.DP));
                await SecureStorage.Default.SetAsync($"outRequestDQ/{_userName}/{_crypter.Decrypt(dialogueRequest, _sessionKey, _sessionInitVector)}", _converter.ByteArrayToIntArrayToStr(rsaParameters.DQ));
                await SecureStorage.Default.SetAsync($"outRequestExponent/{_userName}/{_crypter.Decrypt(dialogueRequest, _sessionKey, _sessionInitVector)}", _converter.ByteArrayToIntArrayToStr(rsaParameters.Exponent));
                await SecureStorage.Default.SetAsync($"outRequestInverseQ/{_userName}/{_crypter.Decrypt(dialogueRequest, _sessionKey, _sessionInitVector)}", _converter.ByteArrayToIntArrayToStr(rsaParameters.InverseQ));
                await SecureStorage.Default.SetAsync($"outRequestModulus/{_userName}/{_crypter.Decrypt(dialogueRequest, _sessionKey, _sessionInitVector)}", _converter.ByteArrayToIntArrayToStr(rsaParameters.Modulus));
                await SecureStorage.Default.SetAsync($"outRequestP/{_userName}/{_crypter.Decrypt(dialogueRequest, _sessionKey, _sessionInitVector)}", _converter.ByteArrayToIntArrayToStr(rsaParameters.P));
                await SecureStorage.Default.SetAsync($"outRequestQ/{_userName}/{_crypter.Decrypt(dialogueRequest, _sessionKey, _sessionInitVector)}", _converter.ByteArrayToIntArrayToStr(rsaParameters.Q));
            }
            return Convert.ToInt16(response.StatusCode);
        }
        private void BackButtonClicked(object sender, System.EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}

