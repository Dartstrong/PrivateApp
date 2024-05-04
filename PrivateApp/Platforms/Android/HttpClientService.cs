using Javax.Net.Ssl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Android.Net;
using Object = Java.Lang.Object;
namespace PrivateApp
{
    public partial class HttpClientService
    {
        public partial HttpMessageHandler GetPlatformSpecificHttpMessageHandler()
        {
            var androidHttpMessageHandler = new LocalhostAndroidMessageHandler
            {
                ServerCertificateCustomValidationCallback = (httpRequestMessage, certificate, chain, sslPolicyErrors) =>
                {
                    if(certificate?.Issuer=="CN=localhost"||sslPolicyErrors == SslPolicyErrors.None)
                        return true;
                    return false;
                }
            };
            return androidHttpMessageHandler;
        }
        class LocalhostAndroidMessageHandler : AndroidMessageHandler
        {
            protected override IHostnameVerifier? GetSSLHostnameVerifier(HttpsURLConnection connection) => new LocalhostHostNameVerifier();
        }

        class LocalhostHostNameVerifier : Object, IHostnameVerifier
        {
            public bool Verify(string hostname, ISSLSession session)
            {
                if (HttpsURLConnection.DefaultHostnameVerifier.Verify(hostname, session) || hostname ==  "10.0.2.2")
                {
                    return true;
                }
                return false;
            }
        }
    }
}
