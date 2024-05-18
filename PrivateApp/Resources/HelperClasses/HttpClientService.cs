using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrivateApp
{
    public partial class HttpClientService
    {
        public HttpClient GetPlatformSpecificHttpClient() 
        { 
            var httpMessageHandler = GetPlatformSpecificHttpMessageHandler();
            return new HttpClient(httpMessageHandler); 
        }
        public partial HttpMessageHandler GetPlatformSpecificHttpMessageHandler();
    }
}
