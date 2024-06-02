using PrivateApp.Resources.Entities;
using System.Text.Json;
using PrivateApp.Resources.HelperClasses;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
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
        private Page _settingsPage;
        private Page _outRequestsPage;
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
            listView.ItemSelected += ItemSelected;
            listView.ItemTemplate = new DataTemplate(() =>
            {
                ImageCell imageCell = new ImageCell
                {
                    TextColor = Color.FromHex("#7AF4BA"),
                    DetailColor = Colors.Grey,
                    ImageSource = ImageSource.FromFile("back_button.png"),      
                };
                imageCell.SetBinding(ImageCell.TextProperty, "Receiver");
                imageCell.SetBinding(ImageCell.DetailProperty, "IdStr");
                return imageCell;
            });
            mainContent.Content = new StackLayout { Children = { listView } };
        }
        private async void ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            DialogueRequest request = (DialogueRequest)e.SelectedItem;
            bool result = await DisplayAlert("Подтвердить действие", $"Вы хотите отменить отправку заявки пользователю {request.Receiver}, id заявки {request.IdStr}?", "Да", "Нет");
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
            var receiverLogin = await DisplayPromptAsync("Отправка заявки", "Введите логин пользователя с которым хотите начать  диалог:", "OK", "Отмена");
            if (receiverLogin != null)
            {
                int result = await CreatingNewDialogue(receiverLogin);
                if (result != 200)
                {
                    if (result == 401) { await DisplayAlert("Уведомление", "Данный пользователь не зарегестрирован", "ОK"); }
                    else { await DisplayAlert("Уведомление", "Проверьте подключение к интернету и повторите попытку", "ОK"); }
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
                string receivedСontent = await response.Content.ReadAsStringAsync();
                DialogueRequest dialogueRequest = JsonSerializer.Deserialize<DialogueRequest>(receivedСontent, _serializerOptions);
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

